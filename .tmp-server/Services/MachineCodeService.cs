using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// 机器码记录服务。
/// 阶段 2 Task 2.5 实现登录日志记录。
/// MVP 阶段仅记录不强制校验，写入 LoginLogs 表（UserId、Version、IPAddress、MachineCode、HasProcess）。
/// </summary>
public class MachineCodeService
{
    private readonly SqliteStorage _db;
    private readonly ILogger<MachineCodeService> _logger;

    public MachineCodeService(SqliteStorage db, ILogger<MachineCodeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 记录登录日志。
    /// </summary>
    public async Task LogLoginAsync(long userId, string? version, string? ipAddress, string? machineCode, int hasProcess)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO LoginLogs (UserId, Version, IPAddress, MachineCode, HasProcess, LoginAt)
            VALUES (@u, @v, @ip, @mc, @hp, @n)";
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@v", (object?)version ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ip", (object?)ipAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mc", (object?)machineCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@hp", hasProcess);
        cmd.Parameters.AddWithValue("@n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        await cmd.ExecuteNonQueryAsync();
        _logger.LogInformation("记录登录日志: userId={UserId} version={Version} ip={IP} machineCode={MachineCode} hasProcess={HasProcess}",
            userId, version, ipAddress, machineCode, hasProcess);
    }

    /// <summary>
    /// 更新退出时间（最近一条未关闭的登录记录）。
    /// </summary>
    public async Task LogLogoutAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE LoginLogs SET LogoutAt = @n WHERE UserId = @u AND LogoutAt IS NULL";
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        await cmd.ExecuteNonQueryAsync();
        _logger.LogInformation("记录退出日志: userId={UserId}", userId);
    }
}
