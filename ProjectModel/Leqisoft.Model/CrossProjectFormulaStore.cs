﻿using System;
using System.Collections.Generic;
using System.Data;
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
/// 跨项目公式引用存储管理
/// 将引用关系持久化到项目数据库的 CrossProjectFormula 表
/// </summary>
public class CrossProjectFormulaStore
{
    private readonly Project _project;
    private readonly string _dbPath;
    private List<CrossProjectFormula> _formulas = new List<CrossProjectFormula>();

    public CrossProjectFormulaStore(Project project)
    {
        _project = project;
        _dbPath = GetProjectDbPath(project);
        EnsureTable();
    }

    /// <summary>
    /// 获取项目数据库路径
    /// </summary>
    private static string GetProjectDbPath(Project project)
    {
        long userId = User.Current?.Id ?? 1;
        return Path.Combine("data", userId.ToString(), $"{project.Id}.db");
    }

    /// <summary>
    /// 确保 CrossProjectFormula 表存在
    /// </summary>
    private void EnsureTable()
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(@"
            CREATE TABLE IF NOT EXISTS CrossProjectFormula (
                Id INTEGER PRIMARY KEY,
                SourceProjectId TEXT NOT NULL,
                SourceTableId INTEGER NOT NULL,
                TargetTableId INTEGER NOT NULL,
                TargetCellId INTEGER,
                FormulaType INTEGER NOT NULL,
                FormulaExpression TEXT,
                SourceColumnIds TEXT,
                TargetColumnIds TEXT,
                Enabled INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL
            )", conn);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 加载所有公式引用
    /// </summary>
    public async Task<List<CrossProjectFormula>> Load()
    {
        _formulas.Clear();
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM CrossProjectFormula WHERE Enabled = 1 ORDER BY Id", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var formula = new CrossProjectFormula
            {
                Id = new Id64(reader.GetInt64(0)),
                SourceProjectId = Guid.Parse(reader.GetString(1)),
                SourceTableId = new Id64(reader.GetInt64(2)),
                TargetTableId = new Id64(reader.GetInt64(3)),
                TargetCellId = reader.IsDBNull(4) ? Id64.Zero : new Id64(reader.GetInt64(4)),
                FormulaType = (CrossProjectFormulaType)reader.GetInt32(5),
                FormulaExpression = reader.IsDBNull(6) ? null : reader.GetString(6),
                Enabled = reader.GetInt32(9) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(10))
            };

            if (!reader.IsDBNull(7))
                formula.SourceColumnIds = JsonConvert.DeserializeObject<List<long>>(reader.GetString(7))
                    ?.Select(id => new Id64(id)).ToList() ?? new List<Id64>();
            if (!reader.IsDBNull(8))
                formula.TargetColumnIds = JsonConvert.DeserializeObject<List<long>>(reader.GetString(8))
                    ?.Select(id => new Id64(id)).ToList() ?? new List<Id64>();

            _formulas.Add(formula);
        }
        return _formulas;
    }

    /// <summary>
    /// 保存公式引用
    /// </summary>
    public async Task Save(CrossProjectFormula formula)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(@"
            INSERT OR REPLACE INTO CrossProjectFormula 
            (Id, SourceProjectId, SourceTableId, TargetTableId, TargetCellId, FormulaType, FormulaExpression, SourceColumnIds, TargetColumnIds, Enabled, CreatedAt)
            VALUES (@Id, @SourceProjectId, @SourceTableId, @TargetTableId, @TargetCellId, @FormulaType, @FormulaExpression, @SourceColumnIds, @TargetColumnIds, @Enabled, @CreatedAt)", conn);
        
        cmd.Parameters.AddWithValue("@Id", formula.Id.Value);
        cmd.Parameters.AddWithValue("@SourceProjectId", formula.SourceProjectId.ToString());
        cmd.Parameters.AddWithValue("@SourceTableId", formula.SourceTableId.Value);
        cmd.Parameters.AddWithValue("@TargetTableId", formula.TargetTableId.Value);
        cmd.Parameters.AddWithValue("@TargetCellId", formula.TargetCellId.IsZero() ? (object)DBNull.Value : formula.TargetCellId.Value);
        cmd.Parameters.AddWithValue("@FormulaType", (int)formula.FormulaType);
        cmd.Parameters.AddWithValue("@FormulaExpression", formula.FormulaExpression ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@SourceColumnIds", formula.SourceColumnIds?.Count > 0 
            ? JsonConvert.SerializeObject(formula.SourceColumnIds.Select(id => id.Value)) 
            : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@TargetColumnIds", formula.TargetColumnIds?.Count > 0 
            ? JsonConvert.SerializeObject(formula.TargetColumnIds.Select(id => id.Value)) 
            : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Enabled", formula.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@CreatedAt", formula.CreatedAt.ToString("o"));
        
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 删除公式引用
    /// </summary>
    public async Task Delete(Id64 id)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("DELETE FROM CrossProjectFormula WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 评估所有公式并返回结果
    /// </summary>
    public async Task<List<CrossProjectFormulaResult>> Evaluate()
    {
        var results = new List<CrossProjectFormulaResult>();
        foreach (var formula in _formulas.Where(f => f.Enabled))
        {
            try
            {
                var result = await EvaluateFormula(formula);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new CrossProjectFormulaResult
                {
                    FormulaId = formula.Id,
                    Success = false,
                    Error = ex.Message
                });
            }
        }
        return results;
    }

    private Task<CrossProjectFormulaResult> EvaluateFormula(CrossProjectFormula formula)
    {
        // 非本地模式暂不支持
        if (!StorageRouter.IsLocalMode)
        {
            return Task.FromResult(new CrossProjectFormulaResult { FormulaId = formula.Id, Success = false, Error = "非本地模式暂不支持" });
        }

        // 通过 ProjectDAL 打开外部项目数据库
        string externalDbPath = Path.Combine("data", User.Current?.Id.ToString() ?? "1", $"{formula.SourceProjectId}.db");
        if (!File.Exists(externalDbPath))
        {
            return Task.FromResult(new CrossProjectFormulaResult { FormulaId = formula.Id, Success = false, Error = "来源项目数据库不存在" });
        }

        var dal = new ProjectDAL(externalDbPath);
        var tableDto = dal.GetTable(formula.SourceTableId);
        if (tableDto == null)
        {
            return Task.FromResult(new CrossProjectFormulaResult { FormulaId = formula.Id, Success = false, Error = "来源表不存在" });
        }

        var result = new CrossProjectFormulaResult
        {
            FormulaId = formula.Id,
            Success = true,
            Data = new Dictionary<string, object>()
        };

        switch (formula.FormulaType)
        {
            case CrossProjectFormulaType.Batch:
            {
                // 批量引用多列数据：获取每列所有 Normal 行的值
                var batchColumns = dal.GetColumns(formula.SourceTableId).ToList();
                var batchRows = dal.GetRows(formula.SourceTableId).Where(r => r.Role == 0).ToList();
                var batchCells = dal.GetCells(formula.SourceTableId).ToList();
                foreach (var colId in formula.SourceColumnIds)
                {
                    var col = batchColumns.FirstOrDefault(c => c.Id == colId);
                    if (col == null) continue;
                    var colValues = new List<object>();
                    foreach (var row in batchRows)
                    {
                        var cell = batchCells.FirstOrDefault(c => c.RowId == row.Id && c.ColumnId == colId);
                        colValues.Add(cell?.Value.Value);
                    }
                    result.Data[col.Caption] = colValues;
                }
                result.Data["__Rows__"] = batchRows.Count;
                break;
            }
            case CrossProjectFormulaType.Range:
            {
                // 区域引用：按行列索引范围获取单元格值
                var rangeObj = JObject.Parse(formula.FormulaExpression);
                int startRow = rangeObj.Value<int>("StartRow");
                int endRow = rangeObj.Value<int>("EndRow");
                int startCol = rangeObj.Value<int>("StartCol");
                int endCol = rangeObj.Value<int>("EndCol");
                var rangeColumns = dal.GetColumns(formula.SourceTableId).OrderBy(c => c.Index).ToList();
                var rangeRows = dal.GetRows(formula.SourceTableId).OrderBy(r => r.Index).ToList();
                var rangeCells = dal.GetCells(formula.SourceTableId).ToList();
                var selectedCols = rangeColumns.Where(c => c.Index >= startCol && c.Index <= endCol).ToList();
                var selectedRows = rangeRows.Where(r => r.Index >= startRow && r.Index <= endRow).ToList();
                foreach (var row in selectedRows)
                {
                    foreach (var col in selectedCols)
                    {
                        var cell = rangeCells.FirstOrDefault(c => c.RowId == row.Id && c.ColumnId == col.Id);
                        result.Data[$"R{row.Index}C{col.Index}"] = cell?.Value.Value;
                    }
                }
                result.Data["__RowCount__"] = selectedRows.Count;
                result.Data["__ColCount__"] = selectedCols.Count;
                break;
            }
            case CrossProjectFormulaType.Compute:
            {
                // 公式运算：对每列 Normal 行求和，再用 DataTable.Compute 计算表达式
                var computeRows = dal.GetRows(formula.SourceTableId).Where(r => r.Role == 0).ToList();
                var computeCells = dal.GetCells(formula.SourceTableId).ToList();
                var sums = new List<double>();
                foreach (var colId in formula.SourceColumnIds)
                {
                    double sum = 0;
                    foreach (var row in computeRows)
                    {
                        var cell = computeCells.FirstOrDefault(c => c.RowId == row.Id && c.ColumnId == colId);
                        if (cell == null) continue;
                        var val = cell.Value.Value;
                        if (val is double d) sum += d;
                        else if (val is string s && double.TryParse(s, out var parsed)) sum += parsed;
                    }
                    sums.Add(sum);
                }
                var computeArgs = sums.Select(s => (object)s.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                var expr = string.Format(formula.FormulaExpression, computeArgs);
                result.Data["__Result__"] = new DataTable().Compute(expr, null);
                break;
            }
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// 获取所有公式引用
    /// </summary>
    public List<CrossProjectFormula> GetAll()
    {
        return _formulas;
    }
}

/// <summary>
/// 公式评估结果
/// </summary>
public class CrossProjectFormulaResult
{
    public Id64 FormulaId { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
    public Dictionary<string, object> Data { get; set; }
}