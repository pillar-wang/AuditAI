﻿using System.Security.Cryptography;
using AuditApiServer.Models;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// 短信验证码服务。
/// 阶段 2 Task 2.3 实现 GenerateCode/Validate 方法。
/// MVP 阶段：生成 6 位随机码写入 ValidateCodes 表（5 分钟有效期），日志输出 + 返回响应包含验证码（方便调试）。
/// 留出真实短信网关接入位（IISSmsProvider 接口）。
/// </summary>
public class SmsService
{
    private readonly SqliteStorage _db;
    private readonly ILogger<SmsService> _logger;

    public SmsService(SqliteStorage db, ILogger<SmsService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 生成 6 位随机验证码，5 分钟有效期，写入 ValidateCodes 表。
    /// key 通常为手机号，或 "delete_{phone}" 等业务前缀键。
    /// 频率检查 + 删除旧码 + 插入新码三步原子化，避免并发绕过 60 秒频率限制。
    /// </summary>
    public async Task<string> GenerateCodeAsync(string key)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var tx = (SqliteTransaction)await conn.BeginTransactionAsync();
        try
        {
            // V2-M-16: 60 秒下发频率限制，查询同 key 最近 CreatedAt，距上次下发不足 60 秒则拒绝
            using (var rateCmd = conn.CreateCommand())
            {
                rateCmd.Transaction = tx;
                rateCmd.CommandText = "SELECT CreatedAt FROM ValidateCodes WHERE Key = @k ORDER BY CreatedAt DESC LIMIT 1";
                rateCmd.Parameters.AddWithValue("@k", key);
                using var rateReader = await rateCmd.ExecuteReaderAsync();
                if (await rateReader.ReadAsync() && !rateReader.IsDBNull(0))
                {
                    var lastCreatedAtStr = rateReader.GetString(0);
                    if (DateTime.TryParse(lastCreatedAtStr, out var lastCreatedAt))
                    {
                        var elapsed = DateTime.Now - lastCreatedAt;
                        if (elapsed.TotalSeconds < 60)
                        {
                            var waitSeconds = 60 - (int)elapsed.TotalSeconds;
                            throw new InvalidOperationException($"验证码下发过于频繁，请 {waitSeconds} 秒后重试");
                        }
                    }
                }
            }

            // 删除该 key 的旧验证码（一次性使用）
            using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM ValidateCodes WHERE Key = @k";
                del.Parameters.AddWithValue("@k", key);
                await del.ExecuteNonQueryAsync();
            }

            // 生成 6 位数字验证码
            var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            var now = DateTime.Now;
            var expiresAt = now.AddMinutes(5);

            using var ins = conn.CreateCommand();
            ins.Transaction = tx;
            ins.CommandText = @"
                INSERT INTO ValidateCodes (Key, Code, ExpiresAt, CreatedAt)
                VALUES (@k, @c, @e, @n)";
            ins.Parameters.AddWithValue("@k", key);
            ins.Parameters.AddWithValue("@c", code);
            ins.Parameters.AddWithValue("@e", expiresAt.ToString("yyyy-MM-dd HH:mm:ss"));
            ins.Parameters.AddWithValue("@n", now.ToString("yyyy-MM-dd HH:mm:ss"));
            await ins.ExecuteNonQueryAsync();

            await tx.CommitAsync();

            _logger.LogInformation("验证码生成: key={Key} expiresAt={ExpiresAt}", MaskKey(key), expiresAt);
            return code;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// 校验验证码（key + code），校验后删除（一次性使用）。
    /// </summary>
    public async Task<bool> ValidateAsync(string? key, string? code)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(code)) return false;

        using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        long id = 0;
        using (var sel = conn.CreateCommand())
        {
            sel.CommandText = "SELECT Id FROM ValidateCodes WHERE Key = @k AND Code = @c AND ExpiresAt > @n";
            sel.Parameters.AddWithValue("@k", key);
            sel.Parameters.AddWithValue("@c", code);
            sel.Parameters.AddWithValue("@n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            using var reader = await sel.ExecuteReaderAsync();
            if (await reader.ReadAsync()) id = reader.GetInt64(0);
        }

        if (id <= 0) return false;

        // 校验通过后删除该记录（一次性使用）
        using var del = conn.CreateCommand();
        del.CommandText = "DELETE FROM ValidateCodes WHERE Id = @id";
        del.Parameters.AddWithValue("@id", id);
        await del.ExecuteNonQueryAsync();

        return true;
    }

    /// <summary>
    /// 脱敏 key（手机号/业务键）：保留前 3 后 4，中间以 * 替代。
    /// </summary>
    private static string MaskKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (key.Length <= 7) return new string('*', key.Length);
        return key.Substring(0, 3) + new string('*', key.Length - 7) + key.Substring(key.Length - 4);
    }
}
