﻿﻿﻿﻿﻿using AuditApiServer.Data;
using AuditApiServer.Models;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// 版本历史快照服务。阶段 6 Task 6.7 实现。
/// 负责 VersionHistory 表的快照写入、按版本号读取、时间线/版本列表查询。
/// TargetType: 'Table' | 'Document' | 'Image' | 'Pdf'。
/// ChangeType: 'Push' | 'Revert'。
/// Snapshot 字段保存目标表的 BLOB 数据（如 TableSchemas 的关键 BLOB 拼接），
/// 用于 RevertTable / RevertDocument 时还原历史版本。
/// </summary>
public class VersionHistoryService
{
    private readonly SqliteStorage _db;
    private readonly ILogger<VersionHistoryService> _logger;

    public VersionHistoryService(SqliteStorage db, ILogger<VersionHistoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 创建版本快照。
    /// </summary>
    public async Task<long> CreateSnapshotAsync(
        Guid projectId, Guid targetId, string targetType,
        int version, string changeType, byte[]? snapshot, long? createdBy = null)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO VersionHistory (ProjectId, TargetId, TargetType, Version, ChangeType, Snapshot, CreatedAt, CreatedBy)
            VALUES (@pid, @tid, @ttype, @ver, @ctype, @snap, @cts, @cby);
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
        cmd.Parameters.AddWithValue("@tid", targetId.ToString("D"));
        cmd.Parameters.AddWithValue("@ttype", targetType);
        cmd.Parameters.AddWithValue("@ver", version);
        cmd.Parameters.AddWithValue("@ctype", (object?)changeType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@snap", (object?)snapshot ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@cts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@cby", (object?)createdBy ?? DBNull.Value);
        var id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        _logger.LogInformation("创建快照: Id={Id} Target={TargetType}:{TargetId} Version={Version}", id, targetType, targetId, version);
        return id;
    }

    /// <summary>
    /// 按版本号获取快照。返回 null 表示不存在。
    /// V2-M-01 修复：增加 projectId 参数，SQL 加 AND ProjectId=@pid 条件，
    /// 防止攻击者利用已知 tableId Guid 构造跨项目快照读取。
    /// </summary>
    public async Task<VersionHistoryDto?> GetSnapshotAsync(Guid projectId, Guid targetId, string targetType, int version)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, ProjectId, TargetId, TargetType, Version, ChangeType, Snapshot, CreatedAt, CreatedBy
            FROM VersionHistory
            WHERE ProjectId = @pid AND TargetId = @tid AND TargetType = @ttype AND Version = @ver
            ORDER BY Id DESC LIMIT 1";
        cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
        cmd.Parameters.AddWithValue("@tid", targetId.ToString("D"));
        cmd.Parameters.AddWithValue("@ttype", targetType);
        cmd.Parameters.AddWithValue("@ver", version);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return ReadHistory(reader);
    }

    /// <summary>
    /// 列出指定目标对象的时间线（按时间倒序）。
    /// </summary>
    public async Task<List<VersionHistoryDto>> ListTimelineAsync(Guid projectId, Guid targetId, string? targetType = null)
    {
        var list = new List<VersionHistoryDto>();
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        if (string.IsNullOrEmpty(targetType))
        {
            cmd.CommandText = @"
                SELECT Id, ProjectId, TargetId, TargetType, Version, ChangeType, Snapshot, CreatedAt, CreatedBy
                FROM VersionHistory
                WHERE ProjectId = @pid AND TargetId = @tid
                ORDER BY Id DESC";
            cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
            cmd.Parameters.AddWithValue("@tid", targetId.ToString("D"));
        }
        else
        {
            cmd.CommandText = @"
                SELECT Id, ProjectId, TargetId, TargetType, Version, ChangeType, Snapshot, CreatedAt, CreatedBy
                FROM VersionHistory
                WHERE ProjectId = @pid AND TargetId = @tid AND TargetType = @ttype
                ORDER BY Id DESC";
            cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
            cmd.Parameters.AddWithValue("@tid", targetId.ToString("D"));
            cmd.Parameters.AddWithValue("@ttype", targetType);
        }
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadHistory(reader));
        }
        return list;
    }

    /// <summary>
    /// 列出指定目标对象的版本列表（轻量字段，不含 Snapshot BLOB）。
    /// 返回 JArray 形式数据，包含 { version, changeType, createdAt, createdBy } 字段。
    /// </summary>
    public async Task<List<VersionHistoryDto>> ListVersionsAsync(Guid projectId, Guid targetId, string? targetType = null)
    {
        var list = new List<VersionHistoryDto>();
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        if (string.IsNullOrEmpty(targetType))
        {
            cmd.CommandText = @"
                SELECT Id, ProjectId, TargetId, TargetType, Version, ChangeType, NULL, CreatedAt, CreatedBy
                FROM VersionHistory
                WHERE ProjectId = @pid AND TargetId = @tid
                ORDER BY Version DESC";
            cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
            cmd.Parameters.AddWithValue("@tid", targetId.ToString("D"));
        }
        else
        {
            cmd.CommandText = @"
                SELECT Id, ProjectId, TargetId, TargetType, Version, ChangeType, NULL, CreatedAt, CreatedBy
                FROM VersionHistory
                WHERE ProjectId = @pid AND TargetId = @tid AND TargetType = @ttype
                ORDER BY Version DESC";
            cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
            cmd.Parameters.AddWithValue("@tid", targetId.ToString("D"));
            cmd.Parameters.AddWithValue("@ttype", targetType);
        }
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadHistory(reader));
        }
        return list;
    }

    /// <summary>
    /// 删除指定目标对象的所有版本历史。
    /// </summary>
    public async Task<int> DeleteAllAsync(Guid targetId, string? targetType = null, Guid? projectId = null)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();

        var where = "WHERE TargetId = @tid";
        cmd.Parameters.AddWithValue("@tid", targetId.ToString("D"));
        if (!string.IsNullOrEmpty(targetType))
        {
            where += " AND TargetType = @ttype";
            cmd.Parameters.AddWithValue("@ttype", targetType);
        }
        if (projectId.HasValue)
        {
            where += " AND ProjectId = @pid";
            cmd.Parameters.AddWithValue("@pid", projectId.Value.ToString("D"));
        }
        cmd.CommandText = "DELETE FROM VersionHistory " + where;

        var affected = await cmd.ExecuteNonQueryAsync();
        if (affected > 0)
        {
            _logger.LogInformation("清理版本历史 {Count} 条: TargetId={TargetId} Type={TargetType} ProjectId={ProjectId}", affected, targetId, targetType ?? "ALL", projectId);
        }
        return affected;
    }

    private static VersionHistoryDto ReadHistory(SqliteDataReader reader)
    {
        return new VersionHistoryDto
        {
            Id = reader.GetInt64(0),
            ProjectId = Guid.Parse(reader.GetString(1)),
            TargetId = Guid.Parse(reader.GetString(2)),
            TargetType = reader.IsDBNull(3) ? null : reader.GetString(3),
            Version = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
            ChangeType = reader.IsDBNull(5) ? null : reader.GetString(5),
            Snapshot = reader.IsDBNull(6) ? null : (byte[])reader.GetValue(6),
            CreatedAt = reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7)),
            CreatedBy = reader.IsDBNull(8) ? null : reader.GetInt64(8)
        };
    }
}
