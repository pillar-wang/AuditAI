﻿using AuditApiServer.Data;
using AuditApiServer.Models;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// 文件存储服务。阶段 7 Task 7.4 实现。
/// 文件路径规范：{ContentRootPath}/Files/{projectId}/{fileId}.bin
/// 服务器部署后自动解析为 /opt/auditapi/Files/{projectId}/{fileId}.bin
/// （由 systemd WorkingDirectory 决定绝对路径）。
/// </summary>
public class FileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileStorageService> _logger;
    private readonly FileRepository _fileRepo;
    private readonly SqliteStorage _db;

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger, FileRepository fileRepo, SqliteStorage db)
    {
        _env = env;
        _logger = logger;
        _fileRepo = fileRepo;
        _db = db;
    }

    /// <summary>
    /// 保存文件。返回相对/绝对存储路径供 ProjectFiles.StoragePath 字段使用。
    /// </summary>
    public async Task<string> SaveAsync(Guid projectId, Guid fileId, Stream stream)
    {
        var path = GetStoragePath(projectId, fileId);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fs);
        _logger.LogInformation("文件已保存: {Path}", path);
        return path;
    }

    /// <summary>
    /// 读取文件。文件不存在抛 FileNotFoundException。
    /// 调用方负责释放返回的 Stream。
    /// </summary>
    public Stream OpenRead(string storagePath)
    {
        if (!File.Exists(storagePath))
        {
            throw new FileNotFoundException($"文件不存在: {storagePath}", storagePath);
        }
        return File.OpenRead(storagePath);
    }

    /// <summary>
    /// 下载文件（安全审计修复 High）。
    /// 校验 userId 是文件所属项目的成员（ProjectMembers 表），校验通过后返回文件元数据 + Stream。
    /// 校验失败抛 UnauthorizedAccessException；文件不存在抛 FileNotFoundException。
    /// 调用方负责释放返回的 Stream。
    /// </summary>
    public async Task<(ProjectFileDto File, Stream Stream)> DownloadFileAsync(Guid fileId, long userId)
    {
        if (userId == 0) throw new UnauthorizedAccessException("未登录");

        var file = await _fileRepo.GetByIdAsync(fileId);
        if (file == null) throw new FileNotFoundException($"文件不存在: {fileId}");

        // 校验用户是文件所属项目的成员（ProjectMembers 表）
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM ProjectMembers WHERE ProjectId = @pid AND UserId = @uid";
        cmd.Parameters.AddWithValue("@pid", file.ProjectId.ToString("D"));
        cmd.Parameters.AddWithValue("@uid", userId);
        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
            throw new UnauthorizedAccessException("无权访问该文件");

        var stream = OpenRead(file.StoragePath ?? "");
        return (file, stream);
    }

    /// <summary>
    /// 删除文件。文件不存在则跳过。
    /// </summary>
    public void Delete(string storagePath)
    {
        try
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
                _logger.LogInformation("文件已删除: {Path}", storagePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "删除文件失败: {Path}", storagePath);
        }
    }

    /// <summary>
    /// 获取文件存储路径（不实际创建文件或目录）。
    /// 路径规范：{ContentRootPath}/Files/{projectId}/{fileId}.bin
    /// </summary>
    public string GetStoragePath(Guid projectId, Guid fileId)
    {
        var dir = Path.Combine(_env.ContentRootPath, "Files", projectId.ToString("D"));
        return Path.Combine(dir, $"{fileId.ToString("D")}.bin");
    }
}
