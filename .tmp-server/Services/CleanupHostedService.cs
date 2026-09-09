﻿using AuditApiServer.Data;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// 后台定时清理服务（V2-H-24 修复）。
/// 修复前：服务端未注册任何 IHostedService/BackgroundService，导致：
///   1. 过期 Token 永不清理（Tokens 表 ExpiresAt 过期记录堆积）
///   2. 回收站项目 30 天后未自动物理删除（孤儿 .db 文件持续累积）
///   3. TaskCache/{taskId}.tmp 与 Files/PullProjectDirect/ 临时文件永久残留
/// 修复后：通过 PeriodicTimer 定时执行三项清理任务，所有异常被捕获并记录日志，不导致服务崩溃。
/// 注册方式：在 Program.cs 中调用 builder.Services.AddHostedService&lt;CleanupHostedService&gt;()（由主代理协调）。
/// </summary>
public class CleanupHostedService : BackgroundService
{
    private readonly ILogger<CleanupHostedService> _logger;
    private readonly SqliteStorage _db;
    private readonly ProjectRepository _projectRepo;
    private readonly ProjectDbManager _projectDbManager;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// 回收站清理的最低间隔（每日执行一次）。
    /// </summary>
    private static readonly TimeSpan RecycleCleanupInterval = TimeSpan.FromDays(1);

    /// <summary>
    /// 临时文件保留时长（24 小时）。
    /// </summary>
    private static readonly TimeSpan TempFileRetention = TimeSpan.FromHours(24);

    /// <summary>
    /// 回收站项目保留时长（30 天）。
    /// </summary>
    private static readonly TimeSpan RecycleBinRetention = TimeSpan.FromDays(30);

    private DateTime _lastRecycleCleanupUtc = DateTime.MinValue;

    public CleanupHostedService(
        ILogger<CleanupHostedService> logger,
        SqliteStorage db,
        ProjectRepository projectRepo,
        ProjectDbManager projectDbManager,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _db = db;
        _projectRepo = projectRepo;
        _projectDbManager = projectDbManager;
        _env = env;
    }

    /// <summary>
    /// 后台清理主循环：每小时触发一次 Token 与临时文件清理，每日触发一次回收站物理清理。
    /// 使用 PeriodicTimer 而非 Task.Delay，避免漂移；stoppingToken 取消时优雅退出。
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CleanupHostedService 已启动，开始定时清理任务");

        // 启动时立即执行一次（不等待第一个周期）
        await RunAllCleanupsAsync();

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                // 每小时清理一次：过期 Token + 临时文件
                await SafeRunAsync(CleanupExpiredTokensAsync, "过期 Token 清理");
                await SafeRunAsync(CleanupTempFilesAsync, "临时文件清理");

                // 每日清理一次：回收站 30 天项目物理删除
                if (DateTime.UtcNow - _lastRecycleCleanupUtc >= RecycleCleanupInterval)
                {
                    await SafeRunAsync(CleanupRecycleBinAsync, "回收站清理");
                    _lastRecycleCleanupUtc = DateTime.UtcNow;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 服务停止时的预期退出
        }
        catch (Exception ex)
        {
            // 主循环异常兜底，不应发生（每项任务已有 SafeRunAsync 包裹），但记录以防万一
            _logger.LogError(ex, "CleanupHostedService 主循环异常");
        }

        _logger.LogInformation("CleanupHostedService 已停止");
    }

    /// <summary>
    /// 启动时立即执行所有清理任务（不抛异常）。
    /// </summary>
    private async Task RunAllCleanupsAsync()
    {
        await SafeRunAsync(CleanupExpiredTokensAsync, "过期 Token 清理");
        await SafeRunAsync(CleanupRecycleBinAsync, "回收站清理");
        await SafeRunAsync(CleanupTempFilesAsync, "临时文件清理");
        _lastRecycleCleanupUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// 安全执行异步任务：捕获所有异常并记录日志，不让单个清理任务失败影响其他任务或导致服务崩溃。
    /// </summary>
    private async Task SafeRunAsync(Func<Task> action, string name)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} 任务执行失败", name);
        }
    }

    /// <summary>
    /// 清理 1：过期 Token 清理。
    /// DELETE FROM Tokens WHERE ExpiresAt IS NOT NULL AND ExpiresAt &lt; datetime('now')
    /// </summary>
    private async Task CleanupExpiredTokensAsync()
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Tokens WHERE ExpiresAt IS NOT NULL AND ExpiresAt < datetime('now')";
        var deleted = await cmd.ExecuteNonQueryAsync();
        if (deleted > 0)
            _logger.LogInformation("清理过期 Token 完成: 删除 {Count} 条记录", deleted);
    }

    /// <summary>
    /// 清理 2：回收站 30 天自动物理删除。
    /// 查询 Projects WHERE IsDeleted=1 AND DeletedAt &lt; datetime('now','-30 days')，
    /// 循环删除每个项目的 .db 文件（含 -wal/-shm）+ 主库记录。
    /// </summary>
    private async Task CleanupRecycleBinAsync()
    {
        List<(string Id, string TeamId, int Type)> projectsToDelete;

        using (var conn = _db.CreateConnection())
        {
            await conn.OpenAsync();
            using var queryCmd = conn.CreateCommand();
            queryCmd.CommandText = @"
                SELECT Id, TeamId, Type FROM Projects
                WHERE IsDeleted = 1
                  AND DeletedAt IS NOT NULL
                  AND DeletedAt < datetime('now', @retention)";
            queryCmd.Parameters.AddWithValue("@retention", $"-{(int)RecycleBinRetention.TotalDays} days");
            projectsToDelete = new List<(string, string, int)>();
            using var reader = await queryCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                projectsToDelete.Add((
                    reader.IsDBNull(0) ? "" : reader.GetString(0),
                    reader.IsDBNull(1) ? "" : reader.GetString(1),
                    reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                ));
            }
        }

        if (projectsToDelete.Count == 0)
            return;

        _logger.LogInformation("开始清理回收站过期项目: 共 {Count} 个待清理", projectsToDelete.Count);
        int cleaned = 0;
        foreach (var (idStr, teamIdStr, type) in projectsToDelete)
        {
            try
            {
                if (!Guid.TryParse(idStr, out var projectId))
                {
                    _logger.LogWarning("跳过无效项目 ID: {Id}", idStr);
                    continue;
                }

                Guid.TryParse(teamIdStr, out var teamId);

                // 删除项目 .db 文件及 -wal/-shm 副文件
                DeleteProjectDbFiles(teamId, projectId, type);

                // 删除主库 Projects + ProjectMembers + TableSchemas + Documents + VersionHistory + ProjectFiles
                if (teamId != Guid.Empty)
                {
                    await _projectRepo.DeleteFromServerAsync(projectId, teamId);
                }
                else
                {
                    // 无 TeamId 的记录（如系统模板）直接按 Id 删除主库行
                    using var conn = _db.CreateConnection();
                    await conn.OpenAsync();
                    using var delCmd = conn.CreateCommand();
                    delCmd.CommandText = "DELETE FROM Projects WHERE Id = @id";
                    delCmd.Parameters.AddWithValue("@id", idStr);
                    await delCmd.ExecuteNonQueryAsync();
                }

                cleaned++;
                _logger.LogInformation("已清理回收站过期项目: Id={ProjectId} TeamId={TeamId} Type={Type}",
                    projectId, teamIdStr, type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理回收站项目失败: Id={ProjectId}", idStr);
            }
        }

        _logger.LogInformation("回收站清理完成: 成功 {Cleaned}/{Total}", cleaned, projectsToDelete.Count);
    }

    /// <summary>
    /// 删除项目 .db 文件及其 -wal / -shm 副文件。
    /// 根据 Type 选择项目目录或模板目录。
    /// </summary>
    private void DeleteProjectDbFiles(Guid teamId, Guid projectId, int type)
    {
        if (teamId == Guid.Empty)
            return;

        // 模板类型（Type=1）使用模板目录；其他类型使用项目目录
        // 同时尝试删除两个位置，File.Exists 检查避免误删
        DeleteDbFileWithSidecars(_projectDbManager.GetProjectDbPath(teamId, projectId));
        if (type == 1)
        {
            DeleteDbFileWithSidecars(_projectDbManager.GetTemplateDbPath(teamId, projectId));
        }
    }

    /// <summary>
    /// 删除 .db 文件及其 -wal / -shm 副文件（容忍文件不存在）。
    /// </summary>
    private void DeleteDbFileWithSidecars(string dbPath)
    {
        if (string.IsNullOrEmpty(dbPath)) return;

        TryDeleteFile(dbPath);
        TryDeleteFile(dbPath + "-wal");
        TryDeleteFile(dbPath + "-shm");
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "删除文件失败: {Path}", path);
        }
    }

    /// <summary>
    /// 清理 3：临时文件 24 小时清理。
    /// 清理 TaskCache/{taskId}.tmp 和 Files/PullProjectDirect/ 下的过期文件。
    /// 使用 LastWriteTimeUtc 作为清理依据。
    /// </summary>
    private Task CleanupTempFilesAsync()
    {
        var cutoff = DateTime.UtcNow.Subtract(TempFileRetention);

        var taskCacheDir = Path.Combine(_env.ContentRootPath, "TaskCache");
        var pullProjectDirectDir = Path.Combine(_env.ContentRootPath, "Files", "PullProjectDirect");

        CleanupDirectory(taskCacheDir, cutoff);
        CleanupDirectory(pullProjectDirectDir, cutoff);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 清理指定目录下最后修改时间早于 cutoff 的文件。
    /// 不删除子目录，仅清理文件；不抛异常。
    /// </summary>
    private void CleanupDirectory(string dirPath, DateTime cutoff)
    {
        if (string.IsNullOrEmpty(dirPath) || !Directory.Exists(dirPath))
            return;

        int deleted = 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(dirPath))
            {
                try
                {
                    var info = new FileInfo(file);
                    if (info.LastWriteTimeUtc < cutoff)
                    {
                        File.Delete(file);
                        deleted++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除临时文件失败: {File}", file);
                }
            }

            if (deleted > 0)
                _logger.LogInformation("清理临时目录 {Dir} 完成: 删除 {Count} 个文件", dirPath, deleted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "遍历临时目录失败: {Dir}", dirPath);
        }
    }
}
