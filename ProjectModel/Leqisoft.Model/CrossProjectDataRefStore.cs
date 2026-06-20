﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Leqisoft.DTO;
using Leqisoft.LocalDataStore;
using Newtonsoft.Json;

namespace Leqisoft.Model;

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
                UpdatedAt TEXT NOT NULL
            )", conn);
        cmd.ExecuteNonQuery();
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
                UpdatedAt = DateTime.Parse(reader.GetString(13))
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
                UpdatedAt = DateTime.Parse(reader.GetString(13))
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
            (Id, Name, SourceProjectId, SourceTableId, TargetTableId, RefMode, RefConfig, FilterConfig, FormulaExpression, ColumnMapping, AutoRefresh, Enabled, CreatedAt, UpdatedAt)
            VALUES (@Id, @Name, @SourceProjectId, @SourceTableId, @TargetTableId, @RefMode, @RefConfig, @FilterConfig, @FormulaExpression, @ColumnMapping, @AutoRefresh, @Enabled, @CreatedAt, @UpdatedAt)", conn);

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
}
