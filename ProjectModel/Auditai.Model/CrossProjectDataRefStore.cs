﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Newtonsoft.Json;

namespace Auditai.Model;

/// <summary>
/// 跨项目数据引用配置存储管理
/// 将跨项目数据引用关系持久化到项目数据库的 CrossProjectDataRef 表
/// </summary>
public class CrossProjectDataRefStore
{
    private readonly Project _project;
    private readonly string _dbPath;
    private List<CrossProjectDataRef> _refs = new List<CrossProjectDataRef>();

    public CrossProjectDataRefStore(Project project)
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
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "data", userId.ToString(), $"{project.Id}.db");
    }

    /// <summary>
    /// 确保 CrossProjectDataRef 表存在
    /// </summary>
    private void EnsureTable()
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(@"
            CREATE TABLE IF NOT EXISTS CrossProjectDataRef (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                SourceProjectId TEXT NOT NULL,
                SourceTableId INTEGER NOT NULL,
                TargetTableId INTEGER NOT NULL,
                RefMode INTEGER NOT NULL,
                RefConfig TEXT,
                FilterConfig TEXT,
                FormulaExpression TEXT,
                ColumnMapping TEXT,
                AutoRefresh INTEGER NOT NULL DEFAULT 1,
                Enabled INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                AuthId INTEGER,
                DefaultValue TEXT,
                CacheDurationSeconds INTEGER NOT NULL DEFAULT 60,
                VersionStrategy INTEGER NOT NULL DEFAULT 0,
                LockedSourceVersion INTEGER,
                LastSourceVersion INTEGER,
                LastVerifiedAt TEXT,
                LastCacheHitAt TEXT
            )", conn);
        cmd.ExecuteNonQuery();

        // 创建授权表
        using var authCmd = new SQLiteCommand(@"
            CREATE TABLE IF NOT EXISTS CrossProjectRefAuth (
                AuthId INTEGER PRIMARY KEY,
                SourceProjectId TEXT NOT NULL,
                TargetProjectId TEXT NOT NULL,
                AllowedTableIds TEXT,
                AllowedColumnIds TEXT,
                AllowedRefModes TEXT,
                GrantedAt TEXT NOT NULL,
                ExpiresAt TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1
            )", conn);
        authCmd.ExecuteNonQuery();

        // 创建变更通知表
        using var notifyCmd = new SQLiteCommand(@"
            CREATE TABLE IF NOT EXISTS CrossProjectRefNotify (
                NotifyId INTEGER PRIMARY KEY AUTOINCREMENT,
                SourceProjectId TEXT NOT NULL,
                SourceTableId INTEGER NOT NULL,
                TargetProjectId TEXT NOT NULL,
                NotifiedAt TEXT NOT NULL,
                IsAcknowledged INTEGER NOT NULL DEFAULT 0,
                AcknowledgedAt TEXT,
                RefId INTEGER
            )", conn);
        notifyCmd.ExecuteNonQuery();

        // 迁移：为旧版数据库补充缺失的列
        EnsureColumn(conn, "CrossProjectDataRef", "AuthId", "INTEGER");
        EnsureColumn(conn, "CrossProjectDataRef", "DefaultValue", "TEXT");
        EnsureColumn(conn, "CrossProjectDataRef", "CacheDurationSeconds", "INTEGER NOT NULL DEFAULT 60");
        EnsureColumn(conn, "CrossProjectDataRef", "VersionStrategy", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(conn, "CrossProjectDataRef", "LockedSourceVersion", "INTEGER");
        EnsureColumn(conn, "CrossProjectDataRef", "LastSourceVersion", "INTEGER");
        EnsureColumn(conn, "CrossProjectDataRef", "LastVerifiedAt", "TEXT");
        EnsureColumn(conn, "CrossProjectDataRef", "LastCacheHitAt", "TEXT");
    }

    /// <summary>
    /// 检查并添加缺失的列（兼容旧版数据库）
    /// </summary>
    private void EnsureColumn(SQLiteConnection conn, string tableName, string columnName, string columnDef)
    {
        try
        {
            using var checkCmd = new SQLiteCommand($"PRAGMA table_info({tableName})", conn);
            using var reader = checkCmd.ExecuteReader();
            bool found = false;
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }
            reader.Close();

            if (!found)
            {
                using var alterCmd = new SQLiteCommand($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDef}", conn);
                alterCmd.ExecuteNonQuery();
            }
        }
        catch { /* 忽略迁移异常 */ }
    }

    /// <summary>
    /// 加载指定目标表的所有引用配置
    /// </summary>
    public async Task<List<CrossProjectDataRef>> Load(Id64 tableId)
    {
        var result = new List<CrossProjectDataRef>();
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM CrossProjectDataRef WHERE TargetTableId = @TargetTableId ORDER BY Id", conn);
        cmd.Parameters.AddWithValue("@TargetTableId", tableId.Value);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var refItem = new CrossProjectDataRef
            {
                Id = new Id64(reader.GetInt64(0)),
                Name = reader.GetString(1),
                SourceProjectId = Guid.Parse(reader.GetString(2)),
                SourceTableId = new Id64(reader.GetInt64(3)),
                TargetTableId = new Id64(reader.GetInt64(4)),
                RefMode = (RefMode)reader.GetInt32(5),
                RefConfig = reader.IsDBNull(6) ? null : reader.GetString(6),
                FilterConfig = reader.IsDBNull(7) ? null : reader.GetString(7),
                FormulaExpression = reader.IsDBNull(8) ? null : reader.GetString(8),
                ColumnMapping = reader.IsDBNull(9) ? null : reader.GetString(9),
                AutoRefresh = reader.GetInt32(10) == 1,
                Enabled = reader.GetInt32(11) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(12)),
                UpdatedAt = DateTime.Parse(reader.GetString(13)),
                AuthId = reader.IsDBNull(14) ? null : new Id64(reader.GetInt64(14)),
                DefaultValue = reader.IsDBNull(15) ? null : reader.GetString(15),
                CacheDurationSeconds = reader.GetInt32(16),
                VersionStrategy = (VersionStrategy)reader.GetInt32(17),
                LockedSourceVersion = reader.IsDBNull(18) ? null : (int?)reader.GetInt32(18),
                LastSourceVersion = reader.IsDBNull(19) ? null : (int?)reader.GetInt32(19),
                LastVerifiedAt = reader.IsDBNull(20) ? null : (DateTime?)DateTime.Parse(reader.GetString(20)),
                LastCacheHitAt = reader.IsDBNull(21) ? null : (DateTime?)DateTime.Parse(reader.GetString(21))
            };
            result.Add(refItem);
        }
        _refs = result;
        return _refs;
    }

    /// <summary>
    /// 加载所有引用配置（不限目标表）
    /// </summary>
    public async Task<List<CrossProjectDataRef>> LoadAll()
    {
        _refs.Clear();
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM CrossProjectDataRef ORDER BY Id", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var refItem = new CrossProjectDataRef
            {
                Id = new Id64(reader.GetInt64(0)),
                Name = reader.GetString(1),
                SourceProjectId = Guid.Parse(reader.GetString(2)),
                SourceTableId = new Id64(reader.GetInt64(3)),
                TargetTableId = new Id64(reader.GetInt64(4)),
                RefMode = (RefMode)reader.GetInt32(5),
                RefConfig = reader.IsDBNull(6) ? null : reader.GetString(6),
                FilterConfig = reader.IsDBNull(7) ? null : reader.GetString(7),
                FormulaExpression = reader.IsDBNull(8) ? null : reader.GetString(8),
                ColumnMapping = reader.IsDBNull(9) ? null : reader.GetString(9),
                AutoRefresh = reader.GetInt32(10) == 1,
                Enabled = reader.GetInt32(11) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(12)),
                UpdatedAt = DateTime.Parse(reader.GetString(13)),
                AuthId = reader.IsDBNull(14) ? null : new Id64(reader.GetInt64(14)),
                DefaultValue = reader.IsDBNull(15) ? null : reader.GetString(15),
                CacheDurationSeconds = reader.GetInt32(16),
                VersionStrategy = (VersionStrategy)reader.GetInt32(17),
                LockedSourceVersion = reader.IsDBNull(18) ? null : (int?)reader.GetInt32(18),
                LastSourceVersion = reader.IsDBNull(19) ? null : (int?)reader.GetInt32(19),
                LastVerifiedAt = reader.IsDBNull(20) ? null : (DateTime?)DateTime.Parse(reader.GetString(20)),
                LastCacheHitAt = reader.IsDBNull(21) ? null : (DateTime?)DateTime.Parse(reader.GetString(21))
            };
            _refs.Add(refItem);
        }
        return _refs;
    }

    /// <summary>
    /// 保存引用配置（INSERT OR REPLACE）
    /// </summary>
    public async Task Save(CrossProjectDataRef refItem)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(@"
            INSERT OR REPLACE INTO CrossProjectDataRef 
            (Id, Name, SourceProjectId, SourceTableId, TargetTableId, RefMode, RefConfig, FilterConfig, FormulaExpression, ColumnMapping, AutoRefresh, Enabled, CreatedAt, UpdatedAt,
            AuthId, DefaultValue, CacheDurationSeconds, VersionStrategy, LockedSourceVersion, LastSourceVersion, LastVerifiedAt, LastCacheHitAt)
            VALUES (@Id, @Name, @SourceProjectId, @SourceTableId, @TargetTableId, @RefMode, @RefConfig, @FilterConfig, @FormulaExpression, @ColumnMapping, @AutoRefresh, @Enabled, @CreatedAt, @UpdatedAt,
            @AuthId, @DefaultValue, @CacheDurationSeconds, @VersionStrategy, @LockedSourceVersion, @LastSourceVersion, @LastVerifiedAt, @LastCacheHitAt)", conn);

        cmd.Parameters.AddWithValue("@Id", refItem.Id.Value);
        cmd.Parameters.AddWithValue("@Name", refItem.Name);
        cmd.Parameters.AddWithValue("@SourceProjectId", refItem.SourceProjectId.ToString());
        cmd.Parameters.AddWithValue("@SourceTableId", refItem.SourceTableId.Value);
        cmd.Parameters.AddWithValue("@TargetTableId", refItem.TargetTableId.Value);
        cmd.Parameters.AddWithValue("@RefMode", (int)refItem.RefMode);
        cmd.Parameters.AddWithValue("@RefConfig", refItem.RefConfig ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FilterConfig", refItem.FilterConfig ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FormulaExpression", refItem.FormulaExpression ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ColumnMapping", refItem.ColumnMapping ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@AutoRefresh", refItem.AutoRefresh ? 1 : 0);
        cmd.Parameters.AddWithValue("@Enabled", refItem.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@CreatedAt", refItem.CreatedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@UpdatedAt", refItem.UpdatedAt.ToString("o"));

        cmd.Parameters.AddWithValue("@AuthId", refItem.AuthId?.Value ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@DefaultValue", refItem.DefaultValue ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CacheDurationSeconds", refItem.CacheDurationSeconds);
        cmd.Parameters.AddWithValue("@VersionStrategy", (int)refItem.VersionStrategy);
        cmd.Parameters.AddWithValue("@LockedSourceVersion", refItem.LockedSourceVersion ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@LastSourceVersion", refItem.LastSourceVersion ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@LastVerifiedAt", refItem.LastVerifiedAt?.ToString("o") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@LastCacheHitAt", refItem.LastCacheHitAt?.ToString("o") ?? (object)DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 删除引用配置
    /// </summary>
    public async Task Delete(Id64 id)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("DELETE FROM CrossProjectDataRef WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 获取所有引用配置（内存中已加载的列表）
    /// </summary>
    public List<CrossProjectDataRef> GetAll()
    {
        return _refs;
    }

    #region — 变更通知 —

    /// <summary>记录变更通知</summary>
    public async Task RecordNotify(string sourceProjectId, long sourceTableId)
    {
        // 查询哪些项目引用了此表
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();

        // 查找引用此表的所有目标项目
        using var cmd = new SQLiteCommand(
            "SELECT DISTINCT TargetProjectId FROM CrossProjectDataRef WHERE SourceTableId = @SourceTableId", conn);
        cmd.Parameters.AddWithValue("@SourceTableId", sourceTableId);

        var targetProjects = new List<string>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            targetProjects.Add(reader.GetString(0));
        }

        // 为每个目标项目创建通知记录
        foreach (var targetId in targetProjects)
        {
            using var insertCmd = new SQLiteCommand(@"
                INSERT INTO CrossProjectRefNotify (SourceProjectId, SourceTableId, TargetProjectId, NotifiedAt)
                VALUES (@SourceProjectId, @SourceTableId, @TargetProjectId, @NotifiedAt)", conn);
            insertCmd.Parameters.AddWithValue("@SourceProjectId", sourceProjectId);
            insertCmd.Parameters.AddWithValue("@SourceTableId", sourceTableId);
            insertCmd.Parameters.AddWithValue("@TargetProjectId", targetId);
            insertCmd.Parameters.AddWithValue("@NotifiedAt", DateTime.Now.ToString("o"));
            await insertCmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>获取待处理的通知</summary>
    public async Task<List<Dictionary<string, object>>> GetPendingNotifications(string targetProjectId)
    {
        var result = new List<Dictionary<string, object>>();
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "SELECT n.*, r.Name as RefName FROM CrossProjectRefNotify n LEFT JOIN CrossProjectDataRef r ON n.RefId = r.Id WHERE n.TargetProjectId = @TargetProjectId AND n.IsAcknowledged = 0 ORDER BY n.NotifiedAt DESC", conn);
        cmd.Parameters.AddWithValue("@TargetProjectId", targetProjectId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var entry = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                entry[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            result.Add(entry);
        }
        return result;
    }

    /// <summary>确认通知已处理</summary>
    public async Task AcknowledgeNotify(long notifyId)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "UPDATE CrossProjectRefNotify SET IsAcknowledged = 1, AcknowledgedAt = @AcknowledgedAt WHERE NotifyId = @NotifyId", conn);
        cmd.Parameters.AddWithValue("@AcknowledgedAt", DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("@NotifyId", notifyId);
        await cmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region — 授权管理 —

    /// <summary>
    /// 创建授权记录
    /// </summary>
    public void CreateAuth(CrossProjectRefAuth auth)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(@"
            INSERT INTO CrossProjectRefAuth 
            (AuthId, SourceProjectId, TargetProjectId, AllowedTableIds, AllowedColumnIds, AllowedRefModes, GrantedAt, ExpiresAt, IsActive)
            VALUES (@AuthId, @SourceProjectId, @TargetProjectId, @AllowedTableIds, @AllowedColumnIds, @AllowedRefModes, @GrantedAt, @ExpiresAt, @IsActive)", conn);
        cmd.Parameters.AddWithValue("@AuthId", auth.AuthId);
        cmd.Parameters.AddWithValue("@SourceProjectId", auth.SourceProjectId.ToString());
        cmd.Parameters.AddWithValue("@TargetProjectId", auth.TargetProjectId.ToString());
        cmd.Parameters.AddWithValue("@AllowedTableIds", auth.AllowedTableIds ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@AllowedColumnIds", auth.AllowedColumnIds ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@AllowedRefModes", auth.AllowedRefModes ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@GrantedAt", auth.GrantedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@ExpiresAt", auth.ExpiresAt?.ToString("o") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", auth.IsActive ? 1 : 0);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 撤销授权（软删除）
    /// </summary>
    public void RevokeAuth(long authId)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand("UPDATE CrossProjectRefAuth SET IsActive = 0 WHERE AuthId = @AuthId", conn);
        cmd.Parameters.AddWithValue("@AuthId", authId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 查询有效的授权记录
    /// </summary>
    public CrossProjectRefAuth GetAuth(Guid sourceProjectId, Guid targetProjectId)
    {
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM CrossProjectRefAuth WHERE SourceProjectId = @SourceProjectId AND TargetProjectId = @TargetProjectId AND IsActive = 1 ORDER BY AuthId DESC LIMIT 1", conn);
        cmd.Parameters.AddWithValue("@SourceProjectId", sourceProjectId.ToString());
        cmd.Parameters.AddWithValue("@TargetProjectId", targetProjectId.ToString());
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new CrossProjectRefAuth
            {
                AuthId = reader.GetInt64(0),
                SourceProjectId = Guid.Parse(reader.GetString(1)),
                TargetProjectId = Guid.Parse(reader.GetString(2)),
                AllowedTableIds = reader.IsDBNull(3) ? null : reader.GetString(3),
                AllowedColumnIds = reader.IsDBNull(4) ? null : reader.GetString(4),
                AllowedRefModes = reader.IsDBNull(5) ? null : reader.GetString(5),
                GrantedAt = DateTime.Parse(reader.GetString(6)),
                ExpiresAt = reader.IsDBNull(7) ? null : (DateTime?)DateTime.Parse(reader.GetString(7)),
                IsActive = reader.GetInt32(8) == 1
            };
        }
        return null;
    }

    /// <summary>
    /// 获取指定项目的所有授权记录（主动授权+被授权）
    /// </summary>
    public List<CrossProjectRefAuth> GetAllAuths(Guid projectId)
    {
        var result = new List<CrossProjectRefAuth>();
        using var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
        conn.Open();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM CrossProjectRefAuth WHERE SourceProjectId = @ProjectId OR TargetProjectId = @ProjectId ORDER BY AuthId", conn);
        cmd.Parameters.AddWithValue("@ProjectId", projectId.ToString());
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new CrossProjectRefAuth
            {
                AuthId = reader.GetInt64(0),
                SourceProjectId = Guid.Parse(reader.GetString(1)),
                TargetProjectId = Guid.Parse(reader.GetString(2)),
                AllowedTableIds = reader.IsDBNull(3) ? null : reader.GetString(3),
                AllowedColumnIds = reader.IsDBNull(4) ? null : reader.GetString(4),
                AllowedRefModes = reader.IsDBNull(5) ? null : reader.GetString(5),
                GrantedAt = DateTime.Parse(reader.GetString(6)),
                ExpiresAt = reader.IsDBNull(7) ? null : (DateTime?)DateTime.Parse(reader.GetString(7)),
                IsActive = reader.GetInt32(8) == 1
            });
        }
        return result;
    }

    #endregion
}

/// <summary>
/// 跨项目数据引用授权记录模型
/// </summary>
public class CrossProjectRefAuth
{
    /// <summary>授权记录 ID</summary>
    public long AuthId { get; set; }

    /// <summary>来源项目 ID</summary>
    public Guid SourceProjectId { get; set; }

    /// <summary>目标项目 ID</summary>
    public Guid TargetProjectId { get; set; }

    /// <summary>允许访问的表 ID 列表（JSON 数组，为空表示全部允许）</summary>
    public string AllowedTableIds { get; set; }

    /// <summary>允许访问的列 ID 列表（JSON 数组，为空表示全部允许）</summary>
    public string AllowedColumnIds { get; set; }

    /// <summary>允许的引用模式列表（JSON 数组，为空表示全部允许）</summary>
    public string AllowedRefModes { get; set; }

    /// <summary>授权时间</summary>
    public DateTime GrantedAt { get; set; } = DateTime.Now;

    /// <summary>过期时间（为空表示永不过期）</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>是否有效</summary>
    public bool IsActive { get; set; } = true;
}
