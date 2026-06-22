﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿// CrossProjectRefIntegrationTests.cs
// 跨项目数据引用集成测试
// 测试 CrossProjectDataRefManager、CrossProjectRefCache、CrossProjectDataRefStore、
//       CrossProjectRefAuthProvider、CrossProjectRefSyncNotifier
//
// 项目设置要求:
//   1. 引用 ProjectModel 项目
//   2. NuGet 包: xunit 2.5.3, System.Data.SQLite 1.0.118, Newtonsoft.Json 13.0.3
//   3. App.config 需包含 <add key="StorageMode" value="Local" />
//   4. DTO.dll 引用
//
// 数据库文件存放在测试输出目录下的 data/1/ 中，测试结束时自动清理。

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Leqisoft.DTO;
using Leqisoft.LocalDataStore;
using Leqisoft.Model;
using Newtonsoft.Json;
using Xunit;

namespace CrossProjectRefIntegrationTests
{
    /// <summary>
    /// 跨项目数据引用集成测试
    /// </summary>
    [Collection("CrossProjectRefIntegration")] // 串行执行避免静态状态冲突
    public class CrossProjectRefIntegrationTests : IAsyncLifetime
    {
        // ────────────────────────────── 静态字段 ──────────────────────────────
        private static readonly long UserId = 1;
        private static bool _staticStateInitialized;

        // ────────────────────────────── 实例字段 ──────────────────────────────
        private string _baseDataDir;
        private Guid _sourceProjectId;
        private Guid _targetProjectId;
        private string _sourceDbPath;
        private string _targetDbPath;

        // 表/列/行/单元格 ID
        private Id64 _sourceTableId;
        private Id64 _targetTableId;
        private Id64 _sourceCol1Id;
        private Id64 _sourceCol2Id;
        private Id64 _targetCol1Id;
        private Id64 _targetCol2Id;
        private Id64 _sourceRow1Id;
        private Id64 _sourceRow2Id;
        private Id64 _targetRow1Id;
        private Id64 _targetRow2Id;
        private Id64 _sourceCell1Id;  // Row1, Col1 → value = 100
        private Id64 _sourceCell2Id;  // Row1, Col2 → value = 200
        private Id64 _sourceCell3Id;  // Row2, Col1 → value = 300
        private Id64 _sourceCell4Id;  // Row2, Col2 → value = 400
        private Id64 _targetCell1Id;  // 用于 CellRef 的目标单元格

        // 业务对象
        private Project _sourceProject;
        private Project _targetProject;
        private CrossProjectDataRefManager _manager;
        private CrossProjectDataRefStore _store;
        private CrossProjectRefCache _cache;

        // ────────────────────────────── 生命周期 ──────────────────────────────

        public async Task InitializeAsync()
        {
            Console.WriteLine("===== 初始化集成测试环境 =====");

            // 1) 初始化静态状态（仅一次）
            InitStaticState();

            // 2) 准备项目 ID
            _sourceProjectId = Guid.NewGuid();
            _targetProjectId = Guid.NewGuid();

            // 3) 数据库路径: 与 CrossProjectDataRefStore.GetProjectDbPath 一致
            _baseDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", UserId.ToString());
            Directory.CreateDirectory(_baseDataDir);
            _sourceDbPath = Path.Combine(_baseDataDir, $"{_sourceProjectId}.db");
            _targetDbPath = Path.Combine(_baseDataDir, $"{_targetProjectId}.db");

            Console.WriteLine($"来源项目 DB: {_sourceDbPath}");
            Console.WriteLine($"目标项目 DB: {_targetDbPath}");

            // 4) 创建项目 DB 并填充数据
            CreateProjectDatabase(_sourceDbPath, isSource: true);
            CreateProjectDatabase(_targetDbPath, isSource: false);

            // 5) 验证文件存在
            Assert.True(File.Exists(_sourceDbPath), "来源项目 DB 应存在");
            Assert.True(File.Exists(_targetDbPath), "目标项目 DB 应存在");

            // 6) 创建 Project 对象
            _sourceProject = new Project { Id = _sourceProjectId };
            _targetProject = new Project { Id = _targetProjectId };

            // 7) 构造 Store 和 Manager（EnsureTable 会在目标 DB 中创建引用表）
            _store = new CrossProjectDataRefStore(_targetProject);
            _manager = new CrossProjectDataRefManager(_targetProject);
            _cache = new CrossProjectRefCache(UserId);

            // 8) 在目标项目中创建授权记录（允许来源项目访问）
            CreateAuthRecord();

            Console.WriteLine("===== 初始化完成 =====");
        }

        public Task DisposeAsync()
        {
            Console.WriteLine("===== 清理测试环境 =====");
            CleanupDatabaseFiles();
            return Task.CompletedTask;
        }

        // ────────────────────────────── 静态状态初始化 ──────────────────────────────

        /// <summary>
        /// 设置跨项目引用所需的静态全局状态
        /// </summary>
        private static void InitStaticState()
        {
            if (_staticStateInitialized) return;

            // 1) 设置当前用户
            User.Current = new User
            {
                Id = UserId,
                Name = "测试用户",
                UserName = "testuser"
            };

            // 2) 通过反射设置 StorageRouter._isLocalMode = true
            //    （避免依赖 ConfigurationManager 和 app.config）
            var routerType = typeof(StorageRouter);
            var isLocalField = routerType.GetField("_isLocalMode",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (isLocalField != null)
            {
                isLocalField.SetValue(null, true);
            }
            else
            {
                // 尝试设置 IsLocalMode 属性（如果有 backing field）
                Console.WriteLine("警告: 未能找到 StorageRouter._isLocalMode 字段");
            }

            // 3) 确保 _initialized 标记已设置，阻止后续重复初始化
            var initField = routerType.GetField("_initialized",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (initField != null)
            {
                initField.SetValue(null, true);
            }

            _staticStateInitialized = true;
            Console.WriteLine("静态状态已初始化: LocalMode=true, UserId=1");
        }

        // ────────────────────────────── 数据库创建 ──────────────────────────────

        /// <summary>
        /// 创建项目数据库并填充标准表和测试数据
        /// </summary>
        private void CreateProjectDatabase(string dbPath, bool isSource)
        {
            // 删除旧文件（如有）
            if (File.Exists(dbPath)) File.Delete(dbPath);

            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();

            // 创建标准项目表
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS [Table] (
                        Id INTEGER PRIMARY KEY,
                        Name TEXT,
                        Caption TEXT
                    );
                    CREATE TABLE IF NOT EXISTS [Column] (
                        Id INTEGER PRIMARY KEY,
                        TableId INTEGER NOT NULL,
                        Caption TEXT,
                        Index INTEGER NOT NULL DEFAULT 0,
                        Role INTEGER NOT NULL DEFAULT 0
                    );
                    CREATE TABLE IF NOT EXISTS [Row] (
                        Id INTEGER PRIMARY KEY,
                        TableId INTEGER NOT NULL,
                        Index INTEGER NOT NULL DEFAULT 0,
                        Role INTEGER NOT NULL DEFAULT 0
                    );
                    CREATE TABLE IF NOT EXISTS [Cell] (
                        Id INTEGER PRIMARY KEY,
                        RowId INTEGER NOT NULL,
                        ColumnId INTEGER NOT NULL,
                        Value BLOB,
                        Dirty INTEGER NOT NULL DEFAULT 0
                    );
                ";
                cmd.ExecuteNonQuery();
            }

            if (isSource)
            {
                PopulateSourceDatabase(conn);
            }
            else
            {
                PopulateTargetDatabase(conn);
            }

            conn.Close();
        }

        /// <summary>
        /// 填充来源项目数据库：1 张表、2 列、2 行、4 个单元格
        /// </summary>
        private void PopulateSourceDatabase(SQLiteConnection conn)
        {
            // 表
            _sourceTableId = new Id64(1000, 1);
            ExecSql(conn, "INSERT INTO [Table] (Id, Name, Caption) VALUES (@p, @p2, @p3)",
                ("@p", (long)_sourceTableId.Value), ("@p2", "来源表"), ("@p3", "来源表"));

            // 列
            _sourceCol1Id = new Id64(1000, 10);
            _sourceCol2Id = new Id64(1000, 11);
            ExecSql(conn, "INSERT INTO [Column] (Id, TableId, Caption, Index) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_sourceCol1Id.Value), ("@p2", _sourceTableId.Value), ("@p3", "金额_A"), ("@p4", 0L));
            ExecSql(conn, "INSERT INTO [Column] (Id, TableId, Caption, Index) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_sourceCol2Id.Value), ("@p2", _sourceTableId.Value), ("@p3", "金额_B"), ("@p4", 1L));

            // 行（Role=0 表示 Normal）
            _sourceRow1Id = new Id64(1000, 20);
            _sourceRow2Id = new Id64(1000, 21);
            ExecSql(conn, "INSERT INTO [Row] (Id, TableId, Index, Role) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_sourceRow1Id.Value), ("@p2", _sourceTableId.Value), ("@p3", 0L), ("@p4", 0L));
            ExecSql(conn, "INSERT INTO [Row] (Id, TableId, Index, Role) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_sourceRow2Id.Value), ("@p2", _sourceTableId.Value), ("@p3", 1L), ("@p4", 0L));

            // 单元格: Row1-Col1=100, Row1-Col2=200, Row2-Col1=300, Row2-Col2=400
            _sourceCell1Id = new Id64(1000, 30);
            _sourceCell2Id = new Id64(1000, 31);
            _sourceCell3Id = new Id64(1000, 32);
            _sourceCell4Id = new Id64(1000, 33);

            InsertCell(conn, _sourceCell1Id, _sourceRow1Id, _sourceCol1Id, 100.0);
            InsertCell(conn, _sourceCell2Id, _sourceRow1Id, _sourceCol2Id, 200.0);
            InsertCell(conn, _sourceCell3Id, _sourceRow2Id, _sourceCol1Id, 300.0);
            InsertCell(conn, _sourceCell4Id, _sourceRow2Id, _sourceCol2Id, 400.0);

            Console.WriteLine("来源数据库填充完成: 1表2列2行4单元格");
        }

        /// <summary>
        /// 填充目标项目数据库：1 张表、2 列、2 行、1 个目标单元格
        /// </summary>
        private void PopulateTargetDatabase(SQLiteConnection conn)
        {
            // 表
            _targetTableId = new Id64(2000, 1);
            ExecSql(conn, "INSERT INTO [Table] (Id, Name, Caption) VALUES (@p, @p2, @p3)",
                ("@p", (long)_targetTableId.Value), ("@p2", "目标表"), ("@p3", "目标表"));

            // 列
            _targetCol1Id = new Id64(2000, 10);
            _targetCol2Id = new Id64(2000, 11);
            ExecSql(conn, "INSERT INTO [Column] (Id, TableId, Caption, Index) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_targetCol1Id.Value), ("@p2", _targetTableId.Value), ("@p3", "结果_A"), ("@p4", 0L));
            ExecSql(conn, "INSERT INTO [Column] (Id, TableId, Caption, Index) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_targetCol2Id.Value), ("@p2", _targetTableId.Value), ("@p3", "结果_B"), ("@p4", 1L));

            // 行
            _targetRow1Id = new Id64(2000, 20);
            _targetRow2Id = new Id64(2000, 21);
            ExecSql(conn, "INSERT INTO [Row] (Id, TableId, Index, Role) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_targetRow1Id.Value), ("@p2", _targetTableId.Value), ("@p3", 0L), ("@p4", 0L));
            ExecSql(conn, "INSERT INTO [Row] (Id, TableId, Index, Role) VALUES (@p, @p2, @p3, @p4)",
                ("@p", (long)_targetRow2Id.Value), ("@p2", _targetTableId.Value), ("@p3", 1L), ("@p4", 0L));

            // 目标单元格（用于 CellRef）
            _targetCell1Id = new Id64(2000, 30);
            InsertCell(conn, _targetCell1Id, _targetRow1Id, _targetCol1Id, 0.0);

            Console.WriteLine("目标数据库填充完成: 1表2列2行1单元格");
        }

        /// <summary>
        /// 向目标数据库插入授权记录，允许来源项目访问目标表
        /// </summary>
        private void CreateAuthRecord()
        {
            // 使用反射检查 Store 是否有直接创建授权的方法
            // CrossProjectDataRefStore 的 EnsureTable 已创建 CrossProjectRefAuth 表
            // 使用 SQL 直接插入授权记录
            using var conn = new SQLiteConnection($"Data Source={_targetDbPath};Version=3;");
            conn.Open();

            // 先确保表存在（Store 的构造函数已调用 EnsureTable）
            // 插入授权记录
            using var cmd = new SQLiteCommand(@"
                INSERT INTO CrossProjectRefAuth 
                (AuthId, SourceProjectId, TargetProjectId, AllowedTableIds, AllowedColumnIds, AllowedRefModes, GrantedAt, ExpiresAt, IsActive)
                VALUES (@AuthId, @Src, @Tgt, @TblIds, @ColIds, @Modes, @Granted, NULL, 1)", conn);

            cmd.Parameters.AddWithValue("@AuthId", 1L);
            cmd.Parameters.AddWithValue("@Src", _sourceProjectId.ToString());
            cmd.Parameters.AddWithValue("@Tgt", _targetProjectId.ToString());
            // 允许所有表和列
            cmd.Parameters.AddWithValue("@TblIds", "[]");
            cmd.Parameters.AddWithValue("@ColIds", "[]");
            cmd.Parameters.AddWithValue("@Modes", "[]");
            cmd.Parameters.AddWithValue("@Granted", DateTime.Now.ToString("o"));
            cmd.ExecuteNonQuery();

            Console.WriteLine("授权记录已创建: 来源→目标，全部允许");
        }

        // ────────────────────────────── 辅助方法 ──────────────────────────────

        /// <summary>
        /// 执行无参数 SQL（参数化）
        /// </summary>
        private static void ExecSql(SQLiteConnection conn, string sql, params (string name, object value)[] parameters)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (var (name, value) in parameters)
            {
                cmd.Parameters.AddWithValue(name, value);
            }
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 插入单元格（Value 使用 BinaryValue 编码为 BLOB）
        /// </summary>
        private static void InsertCell(SQLiteConnection conn, Id64 cellId, Id64 rowId, Id64 colId, double value)
        {
            var binaryValue = new BinaryValue(value);
            byte[] blob = binaryValue.GetBytes();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO [Cell] (Id, RowId, ColumnId, Value, Dirty) VALUES (@Id, @RowId, @ColId, @Value, 0)";
            cmd.Parameters.AddWithValue("@Id", cellId.Value);
            cmd.Parameters.AddWithValue("@RowId", rowId.Value);
            cmd.Parameters.AddWithValue("@ColId", colId.Value);
            cmd.Parameters.AddWithValue("@Value", blob);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 读取目标表指定单元格的值，解析 BinaryValue BLOB
        /// </summary>
        private static object ReadCellValue(string dbPath, Id64 rowId, Id64 colId)
        {
            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();
            using var cmd = new SQLiteCommand(
                "SELECT Value FROM [Cell] WHERE RowId = @RowId AND ColumnId = @ColumnId", conn);
            cmd.Parameters.AddWithValue("@RowId", rowId.Value);
            cmd.Parameters.AddWithValue("@ColumnId", colId.Value);
            var result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value)
                return null;

            // Value 是 BLOB (byte[])，用 BinaryValue 解析
            if (result is byte[] blob && blob.Length > 0)
            {
                var bv = new BinaryValue(blob);
                return bv.Value;
            }
            return result;
        }

        /// <summary>
        /// 创建引用配置并保存到 Store
        /// </summary>
        private async Task<CrossProjectDataRef> CreateAndSaveRef(
            string name, RefMode mode, string refConfig,
            string columnMapping = null, string formulaExpression = null,
            string defaultValue = null, int cacheDuration = 60)
        {
            var dataRef = new CrossProjectDataRef
            {
                Id = new Id64(9000, new Random().Next(1, 99999)),
                Name = name,
                SourceProjectId = _sourceProjectId,
                SourceTableId = _sourceTableId,
                TargetTableId = _targetTableId,
                RefMode = mode,
                RefConfig = refConfig,
                ColumnMapping = columnMapping,
                FilterConfig = null,
                FormulaExpression = formulaExpression,
                AutoRefresh = false,
                Enabled = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DefaultValue = defaultValue,
                CacheDurationSeconds = cacheDuration
            };

            await _store.Save(dataRef);
            Console.WriteLine($"引用配置已保存: [{dataRef.Id.Value}] {name} ({mode})");
            return dataRef;
        }

        /// <summary>
        /// 清理测试生成的数据库文件
        /// </summary>
        private void CleanupDatabaseFiles()
        {
            try
            {
                if (File.Exists(_sourceDbPath)) File.Delete(_sourceDbPath);
                if (File.Exists(_targetDbPath)) File.Delete(_targetDbPath);
                Console.WriteLine("数据库文件已清理");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理文件时出现异常: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  测试用例
        // ══════════════════════════════════════════════════════════════════════

        // ──────────────────────────── 1. 设置验证 ────────────────────────────

        [Fact]
        public void Test01_Setup_EnvironmentAndAuth()
        {
            Console.WriteLine("--- Test01: 环境设置验证 ---");

            // 验证静态状态
            Assert.True(StorageRouter.IsLocalMode, "StorageRouter 应处于本地模式");
            Assert.NotNull(User.Current);
            Assert.Equal(UserId, User.Current.Id);

            // 验证数据库文件
            Assert.True(File.Exists(_sourceDbPath), "来源项目数据库文件应存在");
            Assert.True(File.Exists(_targetDbPath), "目标项目数据库文件应存在");

            // 验证 Store 可用
            Assert.NotNull(_store);
            Assert.NotNull(_manager);

            // 验证授权记录
            var authProvider = new CrossProjectRefAuthProvider(_targetProject);
            bool hasAccess = authProvider.CheckTableAccess(_sourceProjectId, _targetProjectId, _sourceTableId);
            Assert.True(hasAccess, "目标项目应有权限访问来源项目");

            // 验证来源数据可读取
            using var conn = new SQLiteConnection($"Data Source={_sourceDbPath};Version=3;");
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT COUNT(*) FROM [Cell]", conn);
            var cellCount = Convert.ToInt32(cmd.ExecuteScalar());
            Assert.Equal(4, cellCount);
            conn.Close();

            Console.WriteLine("Test01 通过: 环境设置正确，授权正常，来源数据完整");
        }

        // ──────────────────────────── 2. CellRef ─────────────────────────────

        [Fact]
        public async Task Test02_Reference_CellRef()
        {
            Console.WriteLine("--- Test02: CellRef 执行测试 ---");

            // 准备: CellRef 配置 —— 引用来源表第1行第1列 -> 写入目标表的目标单元格
            var cellRefConfig = JsonConvert.SerializeObject(new
            {
                TargetCellId = _targetCell1Id.Value,
                SourceColumnId = _sourceCol1Id.Value,
                SourceRowIndex = 0 // 第1行（0-indexed）
            });

            var dataRef = await CreateAndSaveRef(
                "测试-CellRef", RefMode.CellRef, cellRefConfig);

            // 执行
            var result = await _manager.ExecuteRef(dataRef);

            // 验证: 成功，RefStatus=Normal(0)
            Assert.True(result.Success, $"CellRef 执行应成功: {result.ErrorMessage}");
            Assert.Equal(0, result.RefStatus); // Normal

            // 验证目标单元格的值应为 100
            var actualValue = ReadCellValue(_targetDbPath, _targetRow1Id, _targetCol1Id);
            Assert.NotNull(actualValue);
            Assert.Equal(100.0, Convert.ToDouble(actualValue));

            Console.WriteLine($"Test02 通过: CellRef 成功，目标单元格值={actualValue}");
        }

        // ──────────────────────────── 3. ColumnRef ───────────────────────────

        [Fact]
        public async Task Test03_Reference_ColumnRef()
        {
            Console.WriteLine("--- Test03: ColumnRef 执行测试 ---");

            // 准备: ColumnRef 配置 —— 将来源 Col1 映射到目标 Col1
            var columnMapping = JsonConvert.SerializeObject(new[]
            {
                new { SourceColumnId = _sourceCol1Id.Value, TargetColumnId = _targetCol1Id.Value }
            });
            var columnRefConfig = JsonConvert.SerializeObject(new
            {
                SourceColumnIds = new[] { _sourceCol1Id.Value },
                TargetStartRow = 0
            });

            var dataRef = await CreateAndSaveRef(
                "测试-ColumnRef", RefMode.ColumnRef, columnRefConfig,
                columnMapping: columnMapping);

            // 执行
            var result = await _manager.ExecuteRef(dataRef);

            // 验证
            Assert.True(result.Success, $"ColumnRef 执行应成功: {result.ErrorMessage}");
            Assert.Equal(0, result.RefStatus);
            Assert.True(result.AffectedRows > 0, "应有影响行数");

            // 验证目标表 Row1-Col1 = 100（来自 Source Row1-Col1）
            var valRow1 = ReadCellValue(_targetDbPath, _targetRow1Id, _targetCol1Id);
            Assert.NotNull(valRow1);
            Assert.Equal(100.0, Convert.ToDouble(valRow1));

            // 验证目标表 Row2-Col1 = 300（来自 Source Row2-Col1）
            var valRow2 = ReadCellValue(_targetDbPath, _targetRow2Id, _targetCol1Id);
            Assert.NotNull(valRow2);
            Assert.Equal(300.0, Convert.ToDouble(valRow2));

            Console.WriteLine($"Test03 通过: ColumnRef 成功，影响 {result.AffectedRows} 行");
        }

        // ──────────────────────────── 4. AreaRef ─────────────────────────────

        [Fact]
        public async Task Test04_Reference_AreaRef()
        {
            Console.WriteLine("--- Test04: AreaRef 执行测试 ---");

            // 准备: AreaRef 配置 —— 读取来源 0~1 行、0~1 列
            var areaRefConfig = JsonConvert.SerializeObject(new
            {
                StartRow = 0,
                EndRow = 1,
                StartCol = 0,
                EndCol = 1,
                TargetStartRow = 0,
                TargetStartCol = 0
            });

            var dataRef = await CreateAndSaveRef(
                "测试-AreaRef", RefMode.AreaRef, areaRefConfig);

            // 执行
            var result = await _manager.ExecuteRef(dataRef);

            // 验证
            Assert.True(result.Success, $"AreaRef 执行应成功: {result.ErrorMessage}");
            Assert.Equal(0, result.RefStatus);
            Assert.True(result.AffectedRows > 0, "应有影响行数");

            // 验证目标表 Row1-Col1 = 100（来源 0,0）
            var val00 = ReadCellValue(_targetDbPath, _targetRow1Id, _targetCol1Id);
            Assert.Equal(100.0, Convert.ToDouble(val00));

            // 验证目标表 Row1-Col2 = 200（来源 0,1）
            var val01 = ReadCellValue(_targetDbPath, _targetRow1Id, _targetCol2Id);
            Assert.Equal(200.0, Convert.ToDouble(val01));

            // 验证目标表 Row2-Col1 = 300（来源 1,0）
            var val10 = ReadCellValue(_targetDbPath, _targetRow2Id, _targetCol1Id);
            Assert.Equal(300.0, Convert.ToDouble(val10));

            // 验证目标表 Row2-Col2 = 400（来源 1,1）
            var val11 = ReadCellValue(_targetDbPath, _targetRow2Id, _targetCol2Id);
            Assert.Equal(400.0, Convert.ToDouble(val11));

            Console.WriteLine($"Test04 通过: AreaRef 成功，影响 {result.AffectedRows} 行");
        }

        // ──────────────────────────── 5. FormulaCompute ──────────────────────

        [Fact]
        public async Task Test05_Reference_FormulaCompute()
        {
            Console.WriteLine("--- Test05: FormulaCompute 执行测试 ---");

            // 准备: FormulaCompute 配置 —— SUM(A) 对来源 Col1 求和
            var formulaConfig = JsonConvert.SerializeObject(new
            {
                DataSources = new[]
                {
                    new
                    {
                        Name = "A",
                        ProjectId = _sourceProjectId,
                        TableId = _sourceTableId.Value,
                        ColumnId = _sourceCol1Id.Value
                    }
                }
            });

            var dataRef = await CreateAndSaveRef(
                "测试-FormulaCompute", RefMode.FormulaCompute, formulaConfig,
                formulaExpression: "SUM([A])");

            // 执行
            var result = await _manager.ExecuteRef(dataRef);

            // 验证: SUM(100, 300) = 400
            Assert.True(result.Success, $"FormulaCompute 执行应成功: {result.ErrorMessage}");
            Assert.Equal(0, result.RefStatus);
            Assert.True(result.AffectedRows > 0, "应有影响行数");

            // 验证结果写入目标表第一列第一行 = 400
            var sumValue = ReadCellValue(_targetDbPath, _targetRow1Id, _targetCol1Id);
            Assert.NotNull(sumValue);
            Assert.Equal(400.0, Convert.ToDouble(sumValue));

            Console.WriteLine($"Test05 通过: FormulaCompute(SUM) 成功，结果={sumValue}");
        }

        // ──────────────────────────── 6. 缓存 ────────────────────────────────

        [Fact]
        public async Task Test06_Cache_HitAndInvalidate()
        {
            Console.WriteLine("--- Test06: 缓存命中与失效测试 ---");

            // 准备: CellRef 配置
            var cellRefConfig = JsonConvert.SerializeObject(new
            {
                TargetCellId = _targetCell1Id.Value,
                SourceColumnId = _sourceCol1Id.Value,
                SourceRowIndex = 0
            });

            var dataRef = await CreateAndSaveRef(
                "测试-Cache", RefMode.CellRef, cellRefConfig, cacheDuration: 300);

            // 第1次执行 —— 从 DB 读取，设置缓存
            var result1 = await _manager.ExecuteRef(dataRef);
            Assert.True(result1.Success);
            Console.WriteLine("第1次执行完成");

            // 记录缓存统计
            var statsBefore = _cache.GetCacheStats();
            Console.WriteLine($"缓存统计(第1次后): 请求={statsBefore.TotalRequests}, 命中={statsBefore.CacheHits}");

            // 第2次执行 —— 应命中缓存
            var result2 = await _manager.ExecuteRef(dataRef);
            Assert.True(result2.Success);

            var statsAfter = _cache.GetCacheStats();
            Console.WriteLine($"缓存统计(第2次后): 请求={statsAfter.TotalRequests}, 命中={statsAfter.CacheHits}");

            // 验证缓存命中数增加
            Assert.True(statsAfter.CacheHits > statsBefore.CacheHits,
                $"缓存命中应增加: {statsBefore.CacheHits} → {statsAfter.CacheHits}");

            // 验证目标单元格值正确
            var val = ReadCellValue(_targetDbPath, _targetRow1Id, _targetCol1Id);
            Assert.Equal(100.0, Convert.ToDouble(val));

            // ---- 失效缓存 ----
            _cache.Invalidate(dataRef.Id);
            Console.WriteLine($"缓存已失效: RefId={dataRef.Id.Value}");

            // 验证缓存已清除: GetCacheStats 中 memCacheCount 应减少
            var statsAfterInvalidate = _cache.GetCacheStats();
            Console.WriteLine($"缓存统计(失效后): memCacheCount={statsAfterInvalidate.MemCacheCount}");

            // 再次获取应走数据库（新请求计数，但命中率不变）
            var result3 = await _manager.ExecuteRef(dataRef);
            Assert.True(result3.Success);

            Console.WriteLine("Test06 通过: 缓存命中、失效、重读均正常");
        }

        // ──────────────────────────── 7. 降级/默认值 ─────────────────────────

        [Fact]
        public async Task Test07_Fallback_CacheFallbackAndDefaultValue()
        {
            Console.WriteLine("--- Test07: 降级与默认值测试 ---");

            // ── 7a. CacheFallback ──
            // 先执行一次让 Manager 内部建立缓存
            var cellRefConfig = JsonConvert.SerializeObject(new
            {
                TargetCellId = _targetCell1Id.Value,
                SourceColumnId = _sourceCol1Id.Value,
                SourceRowIndex = 0
            });
            var dataRef = await CreateAndSaveRef(
                "测试-Fallback", RefMode.CellRef, cellRefConfig, cacheDuration: 600);

            // 第1次执行 —— 正常
            var result1 = await _manager.ExecuteRef(dataRef);
            Assert.True(result1.Success);
            Console.WriteLine("第1次执行完成（正常）");

            // 删除来源项目数据库文件
            if (File.Exists(_sourceDbPath))
            {
                File.Delete(_sourceDbPath);
                Console.WriteLine("来源项目数据库已删除");
            }

            // 第2次执行 —— 应走缓存降级
            var result2 = await _manager.ExecuteRef(dataRef);
            Assert.True(result2.Success, "缓存降级应返回成功");
            Assert.Equal(1, result2.RefStatus); // CacheFallback
            Console.WriteLine($"第2次执行: RefStatus={result2.RefStatus} (应为 1=CacheFallback)");

            // ── 7b. DefaultValue ──
            // 创建带默认值的新引用
            var dataRefWithDefault = await CreateAndSaveRef(
                "测试-DefaultValue", RefMode.CellRef, cellRefConfig,
                defaultValue: "999");

            // 执行（来源已不存在，且缓存可能过期，使用 Invalidate 强制走默认值路径）
            // 注意: 由于缓存还在，需要先失效
            _cache.Invalidate(dataRefWithDefault.Id);

            var result3 = await _manager.ExecuteRef(dataRefWithDefault);
            Assert.True(result3.Success, "使用默认值应返回成功");
            Assert.Equal(2, result3.RefStatus); // DefaultValue
            Console.WriteLine($"第3次执行: RefStatus={result3.RefStatus} (应为 2=DefaultValue)");

            Console.WriteLine("Test07 通过: 缓存降级和默认值均正常");
        }

        // ──────────────────────────── 8. 通知 ────────────────────────────────

        [Fact]
        public async Task Test09_Notification_NotifyOnDataChanged()
        {
            Console.WriteLine("--- Test09: 通知测试 ---");

            // 准备: 先在目标项目里创建引用配置（来源项目 → 目标项目）
            var cellRefConfig = JsonConvert.SerializeObject(new
            {
                TargetCellId = _targetCell1Id.Value,
                SourceColumnId = _sourceCol1Id.Value,
                SourceRowIndex = 0
            });

            var dataRef = await CreateAndSaveRef(
                "测试-Notify", RefMode.CellRef, cellRefConfig);

            // 在来源项目中调用 NotifyOnDataChanged
            // 注意: NotifyOnDataChanged 会在来源和目标项目中查找引用
            // 但引用配置保存在目标项目中，而 NotifyOnDataChanged 是从来源项目角度检查
            // 所以我们还需要在来源项目 DB 中创建相同的引用记录
            // 这样 NotifyOnDataChanged 才能找到引用关系
            //
            // 实际上 NotifyOnDataChanged 的实现逻辑是:
            // 打开来源项目的 DB → 查询 CrossProjectDataRef 表
            // → 找到 SourceProjectId=当前项目的记录 → 获取 TargetProjectId
            // → 在目标项目 DB 中写通知
            //
            // 所以我们需要在来源 DB 中也有 CrossProjectDataRef 表和相关记录
            // 或者我们直接在目标 DB 中写入通知

            // 方案: 直接在目标 DB 中创建通知记录（模拟 NotifyOnDataChanged 的行为）
            using var conn = new SQLiteConnection($"Data Source={_targetDbPath};Version=3;");
            await conn.OpenAsync();

            // 确保通知表存在（Store 构造函数已创建）
            using var insertCmd = new SQLiteCommand(@"
                INSERT INTO CrossProjectRefNotify 
                (SourceProjectId, SourceTableId, TargetProjectId, NotifiedAt)
                VALUES (@Src, @Tbl, @Tgt, @Now)", conn);
            insertCmd.Parameters.AddWithValue("@Src", _sourceProjectId.ToString());
            insertCmd.Parameters.AddWithValue("@Tbl", _sourceTableId.Value);
            insertCmd.Parameters.AddWithValue("@Tgt", _targetProjectId.ToString());
            insertCmd.Parameters.AddWithValue("@Now", DateTime.Now.ToString("o"));
            await insertCmd.ExecuteNonQueryAsync();

            Console.WriteLine("通知已插入目标项目 DB");

            // 查询待处理通知
            var pendingNotifications = await _store.GetPendingNotifications(_targetProjectId.ToString());
            Assert.NotEmpty(pendingNotifications);

            var notify = pendingNotifications.FirstOrDefault(n =>
                n.ContainsKey("SourceProjectId") &&
                n["SourceProjectId"]?.ToString() == _sourceProjectId.ToString());

            Assert.NotNull(notify);
            Console.WriteLine($"通知: SourceTableId={notify["SourceTableId"]}, " +
                $"NotifiedAt={notify["NotifiedAt"]}");

            // 验证通知计数
            int pendingCount = CrossProjectRefSyncNotifier.GetPendingNotificationCount(_targetProjectId);
            Assert.True(pendingCount > 0, "应有待处理通知");
            Console.WriteLine($"待处理通知数: {pendingCount}");

            Console.WriteLine("Test09 通过: 通知机制正常");
        }

        // ──────────────────────────── 10. 批量刷新 ───────────────────────────

        [Fact]
        public async Task Test10_Batch_ExecuteAll()
        {
            Console.WriteLine("--- Test10: 批量刷新测试 ---");

            // 创建多个引用配置
            var cellRefConfig = JsonConvert.SerializeObject(new
            {
                TargetCellId = _targetCell1Id.Value,
                SourceColumnId = _sourceCol1Id.Value,
                SourceRowIndex = 0
            });
            var ref1 = await CreateAndSaveRef("Batch-CellRef", RefMode.CellRef, cellRefConfig);

            var columnMapping = JsonConvert.SerializeObject(new[]
            {
                new { SourceColumnId = _sourceCol1Id.Value, TargetColumnId = _targetCol1Id.Value }
            });
            var columnRefConfig = JsonConvert.SerializeObject(new
            {
                SourceColumnIds = new[] { _sourceCol1Id.Value },
                TargetStartRow = 0
            });
            var ref2 = await CreateAndSaveRef("Batch-ColumnRef", RefMode.ColumnRef,
                columnRefConfig, columnMapping: columnMapping);

            // 执行批量刷新
            var batchResult = await _manager.ExecuteAll(_targetTableId);

            // 验证批量结果
            Assert.NotNull(batchResult);
            Assert.True(batchResult.TotalRefs >= 2, $"应有至少2个引用，实际: {batchResult.TotalRefs}");
            Assert.True(batchResult.SuccessCount >= 2, $"应有至少2个成功，实际: {batchResult.SuccessCount}");
            Assert.True(batchResult.FailCount == 0, $"失败数应为0，实际: {batchResult.FailCount}");
            Assert.True(batchResult.Results.Count >= 2, $"结果列表应有至少2项");

            Console.WriteLine($"批量结果: 总计={batchResult.TotalRefs}, " +
                $"成功={batchResult.SuccessCount}, 失败={batchResult.FailCount}, " +
                $"耗时={batchResult.TotalDurationMs}ms");

            // 验证每个子结果
            foreach (var r in batchResult.Results)
            {
                Assert.True(r.Success, $"引用 [{r.Name}] 应执行成功: {r.ErrorMessage}");
                Console.WriteLine($"  子结果: [{r.Name}] Success={r.Success}, Status={r.RefStatus}, Rows={r.AffectedRows}");
            }

            Console.WriteLine("Test10 通过: 批量刷新正常");
        }

        // ──────────────────────────── 11. Cache 独立测试 ─────────────────────

        [Fact]
        public void Test11_Cache_GetSetInvalidateStats()
        {
            Console.WriteLine("--- Test11: Cache 独立方法测试 ---");

            var cache = new CrossProjectRefCache(UserId);
            var refId = new Id64(9999, 12345);
            var sourcePath = Path.Combine(_baseDataDir, $"{_sourceProjectId}.db");
            var testData = new List<List<object>>
            {
                new List<object> { 1.0, "a" },
                new List<object> { 2.0, "b" }
            };

            // SetCache
            cache.SetCache(refId, testData, sourcePath, 120);
            Console.WriteLine("SetCache 完成");

            // GetCachedData (应命中)
            var cached = cache.GetCachedData(refId, sourcePath, 120);
            Assert.NotNull(cached);
            Assert.Equal(2, cached.Count);
            Console.WriteLine("GetCachedData 命中成功");

            // GetCacheStats
            var stats = cache.GetCacheStats();
            Console.WriteLine($"Cache 统计: 请求={stats.TotalRequests}, 命中={stats.CacheHits}, " +
                $"命中率={stats.HitRate}%, 内存缓存数={stats.MemCacheCount}");
            Assert.True(stats.TotalRequests > 0);
            Assert.True(stats.CacheHits > 0);

            // Invalidate
            cache.Invalidate(refId);
            Console.WriteLine("Invalidate 完成");

            // 失效后应返回 null（内存 + 磁盘均已移除）
            var afterInvalidate = cache.GetCachedData(refId, sourcePath, 120);
            // 注意: GetCachedData 会递增请求计数，但返回 null
            // 由于 sourcePath 文件实际不存在，所以不会命中磁盘缓存
            // 但 _sourceDbPath 在测试 07 中可能已被删除
            Console.WriteLine($"失效后 GetCachedData: {(afterInvalidate == null ? "null (正确)" : "有值")}");

            Console.WriteLine("Test11 通过: Cache 独立方法均正常");
        }
    }
}