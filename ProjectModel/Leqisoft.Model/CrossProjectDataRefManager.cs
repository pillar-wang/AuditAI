﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Leqisoft.DTO;
using Leqisoft.LocalDataStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Leqisoft.Model;

/// <summary>
/// 跨项目数据引用执行结果
/// </summary>
public class DataRefResult
{
    /// <summary>引用配置 ID</summary>
    public Id64 RefId { get; set; }

    /// <summary>引用名称</summary>
    public string Name { get; set; }

    /// <summary>是否成功</summary>
    public bool Success { get; set; }

    /// <summary>影响行数</summary>
    public int AffectedRows { get; set; }

    /// <summary>错误信息</summary>
    public string ErrorMessage { get; set; }
}

/// <summary>
/// 跨项目数据引用状态
/// </summary>
public class DataRefStatus
{
    /// <summary>来源项目数据库文件是否存在</summary>
    public bool ProjectExists { get; set; }

    /// <summary>来源表是否存在</summary>
    public bool TableExists { get; set; }

    /// <summary>来源列是否存在</summary>
    public bool ColumnsExist { get; set; }

    /// <summary>状态描述</summary>
    public string Description { get; set; }
}

/// <summary>
/// 跨项目数据引用的执行引擎
/// 负责读取外部项目数据、执行筛选和公式运算，并将结果写入当前项目
/// </summary>
public class CrossProjectDataRefManager
{
    private readonly Project _currentProject;
    private readonly CrossProjectDataRefStore _store;

    public CrossProjectDataRefManager(Project currentProject)
    {
        _currentProject = currentProject ?? throw new ArgumentNullException(nameof(currentProject));
        _store = new CrossProjectDataRefStore(currentProject);
    }

    /// <summary>
    /// 获取管理器关联的存储
    /// </summary>
    public CrossProjectDataRefStore Store => _store;

    /// <summary>
    /// 执行单个引用
    /// </summary>
    public async Task<DataRefResult> ExecuteRef(CrossProjectDataRef dataRef)
    {
        var result = new DataRefResult
        {
            RefId = dataRef.Id,
            Name = dataRef.Name,
            Success = false,
            AffectedRows = 0
        };

        try
        {
            // 非本地模式暂不支持
            if (!StorageRouter.IsLocalMode)
            {
                result.ErrorMessage = "非本地模式暂不支持跨项目数据引用";
                return result;
            }

            // 打开外部项目数据库
            string externalDbPath = GetExternalDbPath(dataRef.SourceProjectId);
            if (!File.Exists(externalDbPath))
            {
                result.ErrorMessage = "来源项目数据库不存在";
                return result;
            }

            var sourceDal = new ProjectDAL(externalDbPath);
            var sourceTable = sourceDal.GetTable(dataRef.SourceTableId);
            if (sourceTable == null)
            {
                result.ErrorMessage = "来源表不存在";
                return result;
            }

            // 读取来源表数据为 List<List<object>> 格式
            List<List<object>> sourceData = ReadSourceData(sourceDal, dataRef.SourceTableId);

            // 如果有筛选配置，应用筛选
            var filteredIndices = CrossProjectDataRefFilter.ApplyFilter(dataRef.FilterConfig, sourceData);
            var filteredData = filteredIndices.Select(i => sourceData[i]).ToList();

            if (filteredData.Count == 0)
            {
                result.Success = true;
                result.AffectedRows = 0;
                return result;
            }

            // 根据 RefMode 处理数据并写入目标表
            int affectedRows = 0;

            switch (dataRef.RefMode)
            {
                case RefMode.CellRef:
                    affectedRows = await ExecuteCellRef(dataRef, filteredData);
                    break;

                case RefMode.ColumnRef:
                    affectedRows = await ExecuteColumnRef(dataRef, filteredData);
                    break;

                case RefMode.AreaRef:
                    // AreaRef：先在全量数据上应用筛选（筛选的 ColumnIndex 是全表列索引），
                    // 再从筛选后的行中提取指定列范围
                    {
                        var areaConfig = SafeDeserialize<AreaRefConfig>(dataRef.RefConfig);
                        if (areaConfig == null)
                        {
                            result.ErrorMessage = "AreaRef 配置无效";
                            return result;
                        }
                        // 使用已筛选的全量数据（filteredData），按区域列范围裁剪
                        var areaFilteredData = new List<List<object>>();
                        foreach (var row in filteredData)
                        {
                            var croppedRow = new List<object>();
                            for (int c = areaConfig.StartCol; c <= areaConfig.EndCol && c < row.Count; c++)
                            {
                                if (c >= 0)
                                    croppedRow.Add(row[c]);
                            }
                            areaFilteredData.Add(croppedRow);
                        }
                        // 按区域行范围裁剪
                        int startRow = Math.Max(0, areaConfig.StartRow);
                        int endRow = Math.Min(areaConfig.EndRow, areaFilteredData.Count - 1);
                        if (startRow <= endRow)
                            areaFilteredData = areaFilteredData.GetRange(startRow, endRow - startRow + 1);
                        else
                            areaFilteredData = new List<List<object>>();
                        affectedRows = await ExecuteAreaRef(dataRef, areaFilteredData);
                    }
                    break;

                case RefMode.FormulaCompute:
                    affectedRows = await ExecuteFormulaCompute(dataRef, sourceDal);
                    break;

                default:
                    result.ErrorMessage = $"不支持的引用模式: {dataRef.RefMode}";
                    return result;
            }

            result.Success = true;
            result.AffectedRows = affectedRows;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// 执行指定目标表的所有已启用引用
    /// </summary>
    public async Task<List<DataRefResult>> ExecuteAll(Id64 targetTableId)
    {
        var refs = await _store.Load(targetTableId);
        var results = new List<DataRefResult>();

        foreach (var dataRef in refs.Where(r => r.Enabled))
        {
            var result = await ExecuteRef(dataRef);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// 异步批量执行所有已启用引用，失败不影响其他
    /// </summary>
    public async Task<List<DataRefResult>> ExecuteAllAsync()
    {
        var refs = await _store.LoadAll();
        var results = new List<DataRefResult>();

        foreach (var dataRef in refs.Where(r => r.Enabled))
        {
            try
            {
                var result = await ExecuteRef(dataRef);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new DataRefResult
                {
                    RefId = dataRef.Id,
                    Name = dataRef.Name,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        return results;
    }

    /// <summary>
    /// 获取引用状态（来源是否存在、数据是否可用等）
    /// </summary>
    public DataRefStatus GetRefStatus(CrossProjectDataRef dataRef)
    {
        var status = new DataRefStatus
        {
            ProjectExists = false,
            TableExists = false,
            ColumnsExist = false
        };

        try
        {
            // 检查来源项目数据库文件是否存在
            string externalDbPath = GetExternalDbPath(dataRef.SourceProjectId);
            if (!File.Exists(externalDbPath))
            {
                status.Description = "来源项目数据库文件不存在";
                return status;
            }
            status.ProjectExists = true;

            // 检查来源表是否存在
            var sourceDal = new ProjectDAL(externalDbPath);
            var sourceTable = sourceDal.GetTable(dataRef.SourceTableId);
            if (sourceTable == null)
            {
                status.Description = "来源表不存在";
                return status;
            }
            status.TableExists = true;

            // 检查来源列是否存在
            if (!string.IsNullOrWhiteSpace(dataRef.ColumnMapping))
            {
                try
                {
                    var mapping = SafeDeserialize<List<ColumnMappingItem>>(dataRef.ColumnMapping);
                    if (mapping != null && mapping.Count > 0)
                    {
                        var sourceColumns = sourceDal.GetColumns(dataRef.SourceTableId).ToList();
                        bool allExist = mapping.All(m => sourceColumns.Any(c => c.Id.Value == m.SourceColumnId));
                        status.ColumnsExist = allExist;
                    }
                    else
                    {
                        status.ColumnsExist = true;
                    }
                }
                catch (Exception ex)
                {
                    status.ColumnsExist = false;
                    status.Description = $"列映射解析失败: {ex.Message}";
                }
            }
            else
            {
                status.ColumnsExist = true;
            }

            status.Description = "引用状态正常";
        }
        catch (Exception ex)
        {
            status.Description = $"检查状态时出错: {ex.Message}";
        }

        return status;
    }

    /// <summary>
    /// 刷新过期的引用（通过文件时间戳检测）
    /// </summary>
    public async Task<List<DataRefResult>> RefreshStaleRefs()
    {
        var refs = await _store.LoadAll();
        var results = new List<DataRefResult>();

        // 按来源项目分组，记录每个来源项目数据库文件的最新写入时间
        var sourceFileTimestamps = new Dictionary<Guid, DateTime>();

        foreach (var dataRef in refs.Where(r => r.Enabled && r.AutoRefresh))
        {
            DateTime lastWriteTime;
            if (!sourceFileTimestamps.TryGetValue(dataRef.SourceProjectId, out lastWriteTime))
            {
                string dbPath = GetExternalDbPath(dataRef.SourceProjectId);
                if (File.Exists(dbPath))
                {
                    lastWriteTime = File.GetLastWriteTimeUtc(dbPath);
                    sourceFileTimestamps[dataRef.SourceProjectId] = lastWriteTime;
                }
                else
                {
                    continue;
                }
            }

            // 如果来源文件在上次更新之后有修改，则刷新
            if (lastWriteTime > dataRef.UpdatedAt.ToUniversalTime())
            {
                var result = await ExecuteRef(dataRef);
                results.Add(result);
            }
        }

        return results;
    }

    #region — Private Helpers —

    /// <summary>
    /// 安全反序列化 JSON，对 null/空白输入返回 default
    /// </summary>
    private static T SafeDeserialize<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// 将对象值转换为 BinaryValue 字节序列，用于写入 Cell.Value 列（BLOB）
    /// </summary>
    private static byte[] ToBinaryValueBytes(object value)
    {
        if (value == null)
            return new BinaryValue(string.Empty).GetBytes();

        // 如果已经是 byte[]（来自 ExecuteScalar 读取 BLOB），直接返回
        if (value is byte[] bytes)
            return bytes;

        // 根据运行时类型构造 BinaryValue
        if (value is double d)
            return new BinaryValue(d).GetBytes();
        if (value is float f)
            return new BinaryValue((double)f).GetBytes();
        if (value is int i)
            return new BinaryValue((double)i).GetBytes();
        if (value is long l)
            return new BinaryValue((double)l).GetBytes();
        if (value is decimal dec)
            return new BinaryValue((double)dec).GetBytes();
        if (value is bool b)
            return new BinaryValue(b).GetBytes();
        if (value is DateTime dt)
            return new BinaryValue(dt).GetBytes();

        // 默认作为字符串
        return new BinaryValue(value.ToString()).GetBytes();
    }

    /// <summary>
    /// 获取外部项目数据库路径
    /// </summary>
    private static string GetExternalDbPath(Guid projectId)
    {
        long userId = User.Current?.Id ?? 1;
        return Path.Combine("data", userId.ToString(), $"{projectId}.db");
    }

    /// <summary>
    /// 获取当前项目数据库路径
    /// </summary>
    private string GetCurrentDbPath()
    {
        long userId = User.Current?.Id ?? 1;
        return Path.Combine("data", userId.ToString(), $"{_currentProject.Id}.db");
    }

    /// <summary>
    /// 从来源表读取全部数据（含所有列、Normal 行）为行列表格式
    /// </summary>
    private static List<List<object>> ReadSourceData(ProjectDAL dal, Id64 tableId)
    {
        var columns = dal.GetColumns(tableId).OrderBy(c => c.Index).ToList();
        var rows = dal.GetRows(tableId).Where(r => r.Role == 0).OrderBy(r => r.Index).ToList();
        var cells = dal.GetCells(tableId).ToList();

        var result = new List<List<object>>();

        foreach (var row in rows)
        {
            var rowData = new List<object>();
            foreach (var col in columns)
            {
                var cell = cells.FirstOrDefault(c => c.RowId.Value == row.Id.Value && c.ColumnId.Value == col.Id.Value);
                rowData.Add(cell != null ? cell.Value.Value : null);
            }
            result.Add(rowData);
        }

        return result;
    }

    /// <summary>
    /// 仅读取指定列的数据
    /// </summary>
    private static List<List<object>> ReadSourceDataByColumnIds(ProjectDAL dal, Id64 tableId, List<long> columnIds)
    {
        var allColumns = dal.GetColumns(tableId).ToList();
        var columns = allColumns.Where(c => columnIds.Contains(c.Id.Value)).OrderBy(c => columnIds.IndexOf(c.Id.Value)).ToList();
        var rows = dal.GetRows(tableId).Where(r => r.Role == 0).OrderBy(r => r.Index).ToList();
        var cells = dal.GetCells(tableId).ToList();

        var result = new List<List<object>>();

        foreach (var row in rows)
        {
            var rowData = new List<object>();
            foreach (var col in columns)
            {
                var cell = cells.FirstOrDefault(c => c.RowId.Value == row.Id.Value && c.ColumnId.Value == col.Id.Value);
                rowData.Add(cell != null ? cell.Value.Value : null);
            }
            result.Add(rowData);
        }

        return result;
    }

    /// <summary>
    /// 读取指定行列范围的数据
    /// </summary>
    private static List<List<object>> ReadSourceDataRange(ProjectDAL dal, Id64 tableId, int startRow, int endRow, int startCol, int endCol)
    {
        var columns = dal.GetColumns(tableId).Where(c => c.Index >= startCol && c.Index <= endCol).OrderBy(c => c.Index).ToList();
        var rows = dal.GetRows(tableId).Where(r => r.Role == 0 && r.Index >= startRow && r.Index <= endRow).OrderBy(r => r.Index).ToList();
        var cells = dal.GetCells(tableId).ToList();

        var result = new List<List<object>>();

        foreach (var row in rows)
        {
            var rowData = new List<object>();
            foreach (var col in columns)
            {
                var cell = cells.FirstOrDefault(c => c.RowId.Value == row.Id.Value && c.ColumnId.Value == col.Id.Value);
                rowData.Add(cell != null ? cell.Value.Value : null);
            }
            result.Add(rowData);
        }

        return result;
    }

    /// <summary>
    /// 执行 CellRef 模式：取单个单元格值，填入目标表
    /// </summary>
    private async Task<int> ExecuteCellRef(CrossProjectDataRef dataRef, List<List<object>> filteredData)
    {
        if (filteredData == null || filteredData.Count == 0)
            return 0;

        // 解析 RefConfig 获取目标单元格 ID 和来源单元格信息
        var config = SafeDeserialize<CellRefConfig>(dataRef.RefConfig);
        if (config == null || config.TargetCellId <= 0)
            throw new InvalidOperationException("CellRef 配置无效：缺少 TargetCellId");

        // 确定来源单元格的值
        object cellValue;

        if (config.SourceCellId > 0)
        {
            // 通过 SourceCellId 直接从来源数据库读取
            string externalDbPath = GetExternalDbPath(dataRef.SourceProjectId);
            using var srcConn = new SQLiteConnection($"Data Source={externalDbPath};Version=3;");
            await srcConn.OpenAsync();
            using var srcCmd = new SQLiteCommand("SELECT `Value` FROM `Cell` WHERE `Id` = @Id", srcConn);
            srcCmd.Parameters.AddWithValue("@Id", config.SourceCellId);
            var val = await srcCmd.ExecuteScalarAsync();
            cellValue = val;
        }
        else if (config.SourceColumnId > 0)
        {
            // 通过 SourceColumnId + SourceRowIndex 定位
            int rowIndex = config.SourceRowIndex;
            if (rowIndex < 0 || rowIndex >= filteredData.Count)
                throw new InvalidOperationException($"CellRef: 来源行索引 {rowIndex} 超出范围（共 {filteredData.Count} 行）");

            // 需要找到 SourceColumnId 在 filteredData 中对应的列位置
            var sourceDal = new ProjectDAL(GetExternalDbPath(dataRef.SourceProjectId));
            var allCols = sourceDal.GetColumns(dataRef.SourceTableId).OrderBy(c => c.Index).ToList();
            int colPos = -1;
            for (int i = 0; i < allCols.Count; i++)
            {
                if (allCols[i].Id.Value == config.SourceColumnId)
                {
                    colPos = i;
                    break;
                }
            }
            if (colPos < 0 || colPos >= filteredData[rowIndex].Count)
                throw new InvalidOperationException($"CellRef: 来源列 ID {config.SourceColumnId} 不存在或超出范围");

            cellValue = filteredData[rowIndex][colPos];
        }
        else
        {
            // 兼容旧配置：取第一行第一列
            cellValue = filteredData[0].Count > 0 ? filteredData[0][0] : null;
        }

        // 写入目标表
        string currentDbPath = GetCurrentDbPath();
        using var conn = new SQLiteConnection($"Data Source={currentDbPath};Version=3;");
        await conn.OpenAsync();

        // 更新目标单元格值
        using var updateCmd = new SQLiteCommand(
            "UPDATE `Cell` SET `Value` = @Value, `Dirty` = 1 WHERE `Id` = @Id", conn);
        updateCmd.Parameters.AddWithValue("@Id", config.TargetCellId);
        updateCmd.Parameters.AddWithValue("@Value", ToBinaryValueBytes(cellValue));
        await updateCmd.ExecuteNonQueryAsync();

        return 1;
    }

    /// <summary>
    /// 执行 ColumnRef 模式：按 ColumnMapping 映射，逐行填入目标列
    /// </summary>
    private async Task<int> ExecuteColumnRef(CrossProjectDataRef dataRef, List<List<object>> filteredData)
    {
        // 解析 ColumnMapping
        var mapping = SafeDeserialize<List<ColumnMappingItem>>(dataRef.ColumnMapping);
        if (mapping == null || mapping.Count == 0)
            throw new InvalidOperationException("ColumnRef 需要有效的 ColumnMapping");

        // 解析 RefConfig
        var config = SafeDeserialize<ColumnRefConfig>(dataRef.RefConfig);
        int targetStartRow = config?.TargetStartRow ?? 0;

        // 获取来源表的列顺序，用于查找 SourceColumnId 在 filteredData 中的列位置
        var sourceDal = new ProjectDAL(GetExternalDbPath(dataRef.SourceProjectId));
        var sourceColumns = sourceDal.GetColumns(dataRef.SourceTableId).OrderBy(c => c.Index).ToList();
        // 构建 SourceColumnId → 列位置索引 的映射
        var sourceColIndexMap = new Dictionary<long, int>();
        for (int i = 0; i < sourceColumns.Count; i++)
        {
            sourceColIndexMap[sourceColumns[i].Id.Value] = i;
        }

        string currentDbPath = GetCurrentDbPath();
        using var conn = new SQLiteConnection($"Data Source={currentDbPath};Version=3;");
        await conn.OpenAsync();

        // 获取目标表的行（按 Index 排序）
        var targetDal = new ProjectDAL(currentDbPath);
        var targetRows = targetDal.GetRows(dataRef.TargetTableId)
            .Where(r => r.Role == 0)
            .OrderBy(r => r.Index)
            .ToList();

        // 获取目标表的列
        var targetColumns = targetDal.GetColumns(dataRef.TargetTableId).ToList();

        int affectedRows = 0;

        for (int i = 0; i < filteredData.Count; i++)
        {
            int targetRowIndex = targetStartRow + i;
            if (targetRowIndex >= targetRows.Count)
                break; // 目标行不够时停止

            var targetRow = targetRows[targetRowIndex];

            for (int j = 0; j < mapping.Count; j++)
            {
                var mapItem = mapping[j];

                // 找出映射的目标列
                var targetCol = targetColumns.FirstOrDefault(c => c.Id.Value == mapItem.TargetColumnId);
                if (targetCol == null)
                    continue;

                // 通过 SourceColumnId 查找在 filteredData 中的实际列位置
                object value = null;
                if (sourceColIndexMap.TryGetValue(mapItem.SourceColumnId, out int sourceColPos))
                {
                    if (sourceColPos < filteredData[i].Count)
                        value = filteredData[i][sourceColPos];
                }

                // 更新目标单元格
                using var cmd = new SQLiteCommand(
                    "UPDATE `Cell` SET `Value` = @Value, `Dirty` = 1 WHERE `RowId` = @RowId AND `ColumnId` = @ColumnId", conn);
                cmd.Parameters.AddWithValue("@RowId", targetRow.Id.Value);
                cmd.Parameters.AddWithValue("@ColumnId", targetCol.Id.Value);
                cmd.Parameters.AddWithValue("@Value", ToBinaryValueBytes(value));
                int updated = await cmd.ExecuteNonQueryAsync();
                if (updated > 0)
                    affectedRows++;
            }
        }

        return affectedRows;
    }

    /// <summary>
    /// 执行 AreaRef 模式：按行列范围，填入目标区域
    /// </summary>
    private async Task<int> ExecuteAreaRef(CrossProjectDataRef dataRef, List<List<object>> filteredData)
    {
        // 解析 RefConfig
        var config = SafeDeserialize<AreaRefConfig>(dataRef.RefConfig);
        if (config == null)
            throw new InvalidOperationException("AreaRef 配置无效");

        string currentDbPath = GetCurrentDbPath();
        using var conn = new SQLiteConnection($"Data Source={currentDbPath};Version=3;");
        await conn.OpenAsync();

        // 获取目标表的行、列
        var targetDal = new ProjectDAL(currentDbPath);
        var targetRows = targetDal.GetRows(dataRef.TargetTableId)
            .Where(r => r.Role == 0)
            .OrderBy(r => r.Index)
            .ToList();
        var targetColumns = targetDal.GetColumns(dataRef.TargetTableId)
            .OrderBy(c => c.Index)
            .ToList();

        int affectedRows = 0;

        for (int i = 0; i < filteredData.Count; i++)
        {
            int targetRowIdx = config.TargetStartRow + i;
            if (targetRowIdx >= targetRows.Count)
                break;

            var targetRow = targetRows[targetRowIdx];

            for (int j = 0; j < filteredData[i].Count; j++)
            {
                int targetColIdx = config.TargetStartCol + j;
                if (targetColIdx >= targetColumns.Count)
                    break;

                var targetCol = targetColumns[targetColIdx];

                // 更新目标单元格
                using var cmd = new SQLiteCommand(
                    "UPDATE `Cell` SET `Value` = @Value, `Dirty` = 1 WHERE `RowId` = @RowId AND `ColumnId` = @ColumnId", conn);
                cmd.Parameters.AddWithValue("@RowId", targetRow.Id.Value);
                cmd.Parameters.AddWithValue("@ColumnId", targetCol.Id.Value);
                cmd.Parameters.AddWithValue("@Value", ToBinaryValueBytes(filteredData[i][j]));
                int updated = await cmd.ExecuteNonQueryAsync();
                if (updated > 0)
                    affectedRows++;
            }
        }

        return affectedRows;
    }

    /// <summary>
    /// 执行 FormulaCompute 模式：调用公式引擎计算结果并写入目标
    /// </summary>
    private async Task<int> ExecuteFormulaCompute(CrossProjectDataRef dataRef, ProjectDAL sourceDal)
    {
        // 解析 RefConfig 获取数据源定义
        var config = SafeDeserialize<FormulaComputeRefConfig>(dataRef.RefConfig);
        if (config == null || config.DataSources == null || config.DataSources.Count == 0)
            throw new InvalidOperationException("FormulaCompute 配置无效：缺少 DataSources");

        // 构建公式引擎需要的数据源列表
        var computeDataSources = new List<CrossProjectDataRefCompute.DataSource>();

        foreach (var dsConfig in config.DataSources)
        {
            var dsDal = new ProjectDAL(GetExternalDbPath(dsConfig.ProjectId));
            var dsTable = dsDal.GetTable(dsConfig.TableId);
            if (dsTable == null)
                continue;

            var dsRows = dsDal.GetRows(dsConfig.TableId)
                .Where(r => r.Role == 0)
                .OrderBy(r => r.Index)
                .ToList();
            var dsCells = dsDal.GetCells(dsConfig.TableId).ToList();

            var rowValues = new List<double>();
            foreach (var row in dsRows)
            {
                var cell = dsCells.FirstOrDefault(c =>
                    c.RowId.Value == row.Id.Value && c.ColumnId.Value == dsConfig.ColumnId.Value);
                if (cell != null && cell.Value.Value != null)
                {
                    try
                    {
                        double val = Convert.ToDouble(cell.Value.Value);
                        rowValues.Add(val);
                    }
                    catch
                    {
                        rowValues.Add(0);
                    }
                }
                else
                {
                    rowValues.Add(0);
                }
            }

            computeDataSources.Add(new CrossProjectDataRefCompute.DataSource
            {
                Name = dsConfig.Name,
                ProjectId = dsConfig.ProjectId,
                TableId = dsConfig.TableId,
                ColumnId = dsConfig.ColumnId,
                RowValues = rowValues
            });
        }

        if (computeDataSources.Count == 0)
            throw new InvalidOperationException("FormulaCompute: 没有有效的数据源");

        // 执行公式运算
        var computeResult = CrossProjectDataRefCompute.Compute(dataRef.FormulaExpression, computeDataSources);
        if (!computeResult.Success)
            throw new InvalidOperationException($"公式运算失败: {computeResult.Error}");

        if (computeResult.Results.Count == 0)
            return 0;

        // 将计算结果写入目标表
        string currentDbPath = GetCurrentDbPath();
        var targetDal = new ProjectDAL(currentDbPath);
        var targetRows = targetDal.GetRows(dataRef.TargetTableId)
            .Where(r => r.Role == 0)
            .OrderBy(r => r.Index)
            .ToList();

        // FormulaCompute 的结果写入目标表的第一列（或通过 ColumnMapping 指定）
        var targetColMapping = new List<long>();
        if (!string.IsNullOrWhiteSpace(dataRef.ColumnMapping))
        {
            var mapping = SafeDeserialize<List<ColumnMappingItem>>(dataRef.ColumnMapping);
            if (mapping != null && mapping.Count > 0)
            {
                targetColMapping = mapping.Select(m => m.TargetColumnId).ToList();
            }
        }

        var targetColumns = targetDal.GetColumns(dataRef.TargetTableId)
            .OrderBy(c => c.Index)
            .ToList();

        int affectedRows = 0;
        using var conn = new SQLiteConnection($"Data Source={currentDbPath};Version=3;");
        await conn.OpenAsync();

        for (int i = 0; i < computeResult.Results.Count && i < targetRows.Count; i++)
        {
            var targetRow = targetRows[i];

            if (targetColMapping.Count > 0)
            {
                // 写入到映射的列
                foreach (var colId in targetColMapping)
                {
                    using var cmd = new SQLiteCommand(
                        "UPDATE `Cell` SET `Value` = @Value, `Dirty` = 1 WHERE `RowId` = @RowId AND `ColumnId` = @ColumnId", conn);
                    cmd.Parameters.AddWithValue("@RowId", targetRow.Id.Value);
                    cmd.Parameters.AddWithValue("@ColumnId", colId);
                    cmd.Parameters.AddWithValue("@Value", ToBinaryValueBytes(computeResult.Results[i]));
                    affectedRows += await cmd.ExecuteNonQueryAsync();
                }
            }
            else if (targetColumns.Count > 0)
            {
                // 写入到第一列
                using var cmd = new SQLiteCommand(
                    "UPDATE `Cell` SET `Value` = @Value, `Dirty` = 1 WHERE `RowId` = @RowId AND `ColumnId` = @ColumnId", conn);
                cmd.Parameters.AddWithValue("@RowId", targetRow.Id.Value);
                cmd.Parameters.AddWithValue("@ColumnId", targetColumns[0].Id.Value);
                cmd.Parameters.AddWithValue("@Value", ToBinaryValueBytes(computeResult.Results[i]));
                affectedRows += await cmd.ExecuteNonQueryAsync();
            }
        }

        return affectedRows;
    }

    #endregion

    #region — Config DTOs —

    /// <summary>
    /// CellRef 模式的配置
    /// </summary>
    private class CellRefConfig
    {
        /// <summary>目标单元格 ID</summary>
        public long TargetCellId { get; set; }

        /// <summary>来源单元格 ID（可选，如果不指定则取第一行第一列）</summary>
        public long SourceCellId { get; set; }

        /// <summary>来源列 ID（与 SourceCellId 二选一）</summary>
        public long SourceColumnId { get; set; }

        /// <summary>来源行索引（从0开始，与 SourceColumnId 配合使用）</summary>
        public int SourceRowIndex { get; set; }
    }

    /// <summary>
    /// ColumnRef 模式的配置
    /// </summary>
    private class ColumnRefConfig
    {
        /// <summary>来源列 ID 列表</summary>
        public List<long> SourceColumnIds { get; set; }

        /// <summary>目标起始行索引</summary>
        public int TargetStartRow { get; set; }
    }

    /// <summary>
    /// AreaRef 模式的配置
    /// </summary>
    private class AreaRefConfig
    {
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int StartCol { get; set; }
        public int EndCol { get; set; }
        public int TargetStartRow { get; set; }
        public int TargetStartCol { get; set; }
    }

    /// <summary>
    /// FormulaCompute 模式的配置
    /// </summary>
    private class FormulaComputeRefConfig
    {
        /// <summary>数据源列表</summary>
        public List<DataSourceConfig> DataSources { get; set; }
    }

    /// <summary>
    /// 公式计算数据源配置
    /// </summary>
    private class DataSourceConfig
    {
        /// <summary>A, B, C 标识名</summary>
        public string Name { get; set; }

        /// <summary>来源项目 ID</summary>
        public Guid ProjectId { get; set; }

        /// <summary>来源表 ID</summary>
        public Id64 TableId { get; set; }

        /// <summary>来源列 ID</summary>
        public Id64 ColumnId { get; set; }
    }

    /// <summary>
    /// 列映射项
    /// </summary>
    private class ColumnMappingItem
    {
        /// <summary>来源列 ID</summary>
        public long SourceColumnId { get; set; }

        /// <summary>目标列 ID</summary>
        public long TargetColumnId { get; set; }
    }

    #endregion
}