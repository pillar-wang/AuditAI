﻿using AuditApiServer.Models;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// 认证服务。负责 Token 生成、校验、刷新与清除。
/// 阶段 2 Task 2.4 实现完整业务逻辑。
/// 阶段 9 Task 9.4：增加 ExpiresAt 过期校验与 LastRefreshAt 跟踪。
/// Token 存储 UPSERT 逻辑参考 Program.cs 中现有 UpsertTokenAsync。
/// </summary>
public class AuthService
{
    /// <summary>Token 默认有效期：7 天（客户端每 60 秒刷新一次）</summary>
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(7);

    private readonly SqliteStorage _db;
    private readonly ILogger<AuthService> _logger;

    public AuthService(SqliteStorage db, ILogger<AuthService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 生成新 Token 并 UPSERT 到 Tokens 表。ExpiresAt = UtcNow + 7 天。
    /// </summary>
    public async Task<UserTokenDto> GenerateTokenAsync(long userId)
    {
        var now = DateTime.UtcNow;
        var token = new UserTokenDto
        {
            UserId = userId,
            TokenValue = Guid.NewGuid().ToString("N"),
            LastToken = Guid.NewGuid().ToString("N"),
            UpdateToken = Guid.NewGuid().ToString("N"),
            UpdateTime = DateTime.Now,
            ExpiresAt = now.Add(TokenLifetime),
            LastRefreshAt = now
        };
        await UpsertTokenAsync(token);
        _logger.LogInformation("生成 Token: userId={UserId} tokenPrefix={TokenPrefix} expiresAt={Expires}", userId, GetTokenPrefix(token.TokenValue), token.ExpiresAt);
        return token;
    }

    /// <summary>
    /// 校验 Token（UserId + TokenValue）。允许使用 LastToken 短暂重叠（客户端 Token 轮换期间）。
    /// 同时校验 ExpiresAt > UtcNow；NULL 视为 Legacy 不过期（向后兼容）。
    /// </summary>
    public async Task<bool> ValidateTokenAsync(long userId, string? tokenValue)
    {
        if (string.IsNullOrEmpty(tokenValue)) return false;
        var token = await QueryTokenAsync(userId);
        if (token == null) return false;
        // 过期校验：NULL ExpiresAt 视为不过期（Legacy 数据兼容）
        if (token.ExpiresAt.HasValue && token.ExpiresAt.Value <= DateTime.UtcNow)
        {
            _logger.LogWarning("Token 已过期: userId={UserId} expiresAt={Expires}", userId, token.ExpiresAt);
            return false;
        }
        return string.Equals(token.TokenValue, tokenValue, StringComparison.OrdinalIgnoreCase)
            || string.Equals(token.LastToken, tokenValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 刷新 Token：旧 TokenValue → LastToken，新 TokenValue/UpdateToken，UPSERT。
    /// 注意：调用方需自行校验旧 TokenValue 与 ExpiresAt（参见 UpdateTokenAsync）。
    /// </summary>
    public async Task<UserTokenDto> RefreshTokenAsync(long userId)
    {
        var existing = await QueryTokenAsync(userId);
        if (existing == null)
        {
            // 无现有 Token，直接生成新 Token
            return await GenerateTokenAsync(userId);
        }

        var now = DateTime.UtcNow;
        existing.LastToken = existing.TokenValue;
        existing.TokenValue = Guid.NewGuid().ToString("N");
        existing.UpdateToken = Guid.NewGuid().ToString("N");
        existing.UpdateTime = DateTime.Now;
        existing.ExpiresAt = now.Add(TokenLifetime);
        existing.LastRefreshAt = now;
        await UpsertTokenAsync(existing);
        _logger.LogInformation("刷新 Token: userId={UserId} tokenPrefix={TokenPrefix}", userId, GetTokenPrefix(existing.TokenValue));
        return existing;
    }

    /// <summary>
    /// 阶段 9 Task 9.4：基于旧 TokenValue 刷新 Token。
    /// 校验顺序：Token 是否存在 → Token 是否匹配 → Token 是否过期。
    /// 成功返回新 Token；失败返回 (null, errorCode)。
    /// errorCode: "InvalidToken" | "TokenExpired"
    /// </summary>
    public async Task<(UserTokenDto? Token, string? ErrorCode)> UpdateTokenAsync(long userId, string? oldTokenValue)
    {
        var existing = await QueryTokenAsync(userId);
        if (existing == null)
            return (null, "InvalidToken");

        // 旧 TokenValue 或 LastToken 均可（兼容客户端 60 秒刷新重叠期）
        var matched = string.Equals(existing.TokenValue, oldTokenValue, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(existing.LastToken, oldTokenValue, StringComparison.OrdinalIgnoreCase);
        if (!matched)
            return (null, "InvalidToken");

        // 过期校验：NULL ExpiresAt 视为不过期（Legacy 数据兼容）
        if (existing.ExpiresAt.HasValue && existing.ExpiresAt.Value <= DateTime.UtcNow)
            return (null, "TokenExpired");

        var now = DateTime.UtcNow;
        existing.LastToken = existing.TokenValue;
        existing.TokenValue = Guid.NewGuid().ToString("N");
        existing.UpdateToken = Guid.NewGuid().ToString("N");
        existing.UpdateTime = DateTime.Now;
        existing.ExpiresAt = now.Add(TokenLifetime);
        existing.LastRefreshAt = now;
        await UpsertTokenAsync(existing);
        _logger.LogInformation("UpdateToken 成功: userId={UserId} tokenPrefix={TokenPrefix}", userId, GetTokenPrefix(existing.TokenValue));
        return (existing, null);
    }

    /// <summary>
    /// 清除 Token（用户退出）。
    /// </summary>
    public async Task ClearTokenAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Tokens WHERE UserId = @u";
        cmd.Parameters.AddWithValue("@u", userId);
        await cmd.ExecuteNonQueryAsync();
        _logger.LogInformation("清除 Token: userId={UserId}", userId);
    }

    /// <summary>
    /// 返回 Token 的前 8 位作为指纹，用于日志追踪且不泄露完整 Token。
    /// </summary>
    private static string GetTokenPrefix(string? token)
    {
        if (string.IsNullOrEmpty(token)) return "";
        return token.Length >= 8 ? token.Substring(0, 8) : token;
    }

    private async Task<UserTokenDto?> QueryTokenAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT UserId, TokenValue, LastToken, UpdateToken, UpdateTime, ExpiresAt, LastRefreshAt FROM Tokens WHERE UserId = @u";
        cmd.Parameters.AddWithValue("@u", userId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return new UserTokenDto
        {
            UserId = reader.GetInt64(0),
            TokenValue = reader.IsDBNull(1) ? null : reader.GetString(1),
            LastToken = reader.IsDBNull(2) ? null : reader.GetString(2),
            UpdateToken = reader.IsDBNull(3) ? null : reader.GetString(3),
            UpdateTime = reader.IsDBNull(4) ? DateTime.Now : DateTime.Parse(reader.GetString(4)),
            ExpiresAt = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            LastRefreshAt = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6))
        };
    }

    private async Task UpsertTokenAsync(UserTokenDto token)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Tokens (UserId, TokenValue, LastToken, UpdateToken, UpdateTime, ExpiresAt, LastRefreshAt)
            VALUES (@uid, @tv, @lt, @ut, @time, @exp, @lr)
            ON CONFLICT(UserId) DO UPDATE SET
                TokenValue = excluded.TokenValue,
                LastToken = excluded.LastToken,
                UpdateToken = excluded.UpdateToken,
                UpdateTime = excluded.UpdateTime,
                ExpiresAt = excluded.ExpiresAt,
                LastRefreshAt = excluded.LastRefreshAt;";
        cmd.Parameters.AddWithValue("@uid", token.UserId);
        cmd.Parameters.AddWithValue("@tv", (object?)token.TokenValue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lt", (object?)token.LastToken ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ut", (object?)token.UpdateToken ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@time", token.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@exp", (object?)token.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lr", (object?)token.LastRefreshAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }
}
