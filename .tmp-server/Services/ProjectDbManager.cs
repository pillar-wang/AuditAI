﻿using System.IO;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

public class ProjectDbManager
{
    private const string DefaultProjectPermissions = "{\"Read\":{\"GrantAll\":true},\"Write\":{\"GrantAll\":true},\"Schema\":{\"GrantAll\":true}}";

    private readonly string _basePath;
    private readonly ILogger<ProjectDbManager> _logger;

    public ProjectDbManager(IConfiguration config, ILogger<ProjectDbManager> logger)
    {
        _basePath = config.GetValue<string>("ProjectDataPath") ?? "Data/Projects";
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    public string GetProjectDbPath(Guid teamId, Guid projectId)
    {
        // 系统模板（TeamId=NULL → Guid.Empty）：fallback 到 _System 目录
        // 系统模板存储在 _System/Templates/{templateId}.db，系统项目（罕见）存储在 _System/Projects/{projectId}.db
        if (teamId == Guid.Empty)
        {
            // 优先尝试 _System/Templates/（系统模板最常见的存储位置）
            var systemTemplatePath = Path.Combine(_basePath, "_System", "Templates", $"{projectId}.db");
            if (File.Exists(systemTemplatePath)) return systemTemplatePath;

            // 其次尝试 _System/Projects/（系统级项目，罕见）
            var systemProjectPath = Path.Combine(_basePath, "_System", "Projects", $"{projectId}.db");
            if (File.Exists(systemProjectPath)) return systemProjectPath;

            // 都不存在时返回 _System/Projects/ 路径（SQLite 会自动创建空 .db）
            var systemProjectsDir = Path.Combine(_basePath, "_System", "Projects");
            Directory.CreateDirectory(systemProjectsDir);
            return Path.Combine(systemProjectsDir, $"{projectId}.db");
        }

        var teamDir = Path.Combine(_basePath, teamId.ToString());
        var projectsDir = Path.Combine(teamDir, "Projects");
        Directory.CreateDirectory(projectsDir);
        return Path.Combine(projectsDir, $"{projectId}.db");
    }

    public string GetTemplateDbPath(Guid teamId, Guid templateId)
    {
        var teamDir = Path.Combine(_basePath, teamId.ToString());
        var templatesDir = Path.Combine(teamDir, "Templates");
        Directory.CreateDirectory(templatesDir);
        return Path.Combine(templatesDir, $"{templateId}.db");
    }

    public string GetSystemTemplateDbPath(Guid templateId)
    {
        var templatesDir = Path.Combine(_basePath, "_System", "Templates");
        Directory.CreateDirectory(templatesDir);
        return Path.Combine(templatesDir, $"{templateId}.db");
    }

    /// <summary>
    /// 构建项目/模板 .db 的连接字符串。
    /// 关键：Pooling=False —— Microsoft.Data.Sqlite 默认 Pooling=True，
    /// 即使 using 关闭连接，底层 SQLite 连接仍被连接池保持打开，
    /// 导致 wal_checkpoint(TRUNCATE) 返回 BUSY，-wal 数据无法合并到主 .db，
    /// File.Copy 复制的主 .db 缺失数据 → 客户端表格损坏。
    /// </summary>
    private static string BuildConnStr(string dbPath, bool readOnly = false)
    {
        var str = $"Data Source={dbPath};Pooling=False;";
        if (readOnly) str += "Mode=ReadOnly;";
        return str;
    }

    public bool ProjectDbExists(Guid teamId, Guid projectId)
    {
        return File.Exists(GetProjectDbPath(teamId, projectId));
    }

    public bool TemplateDbExists(Guid teamId, Guid templateId)
    {
        return File.Exists(GetTemplateDbPath(teamId, templateId)) || 
               File.Exists(GetSystemTemplateDbPath(templateId));
    }

    public SqliteConnection OpenProjectDb(Guid teamId, Guid projectId)
    {
        var dbPath = GetProjectDbPath(teamId, projectId);
        // 系统模板 fallback：系统下发模板（TeamId=NULL）无团队路径，
        // TableSyncService.GetTeamIdAsync 对其返回 Guid.Empty，回退到 _System/Templates/ 路径打开
        if (!File.Exists(dbPath))
        {
            var sysPath = GetSystemTemplateDbPath(projectId);
            if (File.Exists(sysPath)) dbPath = sysPath;
        }
        var conn = new SqliteConnection(BuildConnStr(dbPath));
        conn.Open();
        InitializePragma(conn);
        return conn;
    }

    public SqliteConnection OpenTemplateDb(Guid teamId, Guid templateId, bool readOnly = false)
    {
        var dbPath = GetTemplateDbPath(teamId, templateId);
        if (!File.Exists(dbPath))
        {
            dbPath = GetSystemTemplateDbPath(templateId);
        }
        
        var conn = new SqliteConnection(BuildConnStr(dbPath, readOnly));
        conn.Open();
        InitializePragma(conn);
        return conn;
    }

    public void CreateEmptyProjectDb(Guid teamId, Guid projectId, string projectName, Guid? templateId = null)
    {
        var dbPath = GetProjectDbPath(teamId, projectId);
        if (File.Exists(dbPath)) File.Delete(dbPath);

        using var conn = new SqliteConnection(BuildConnStr(dbPath));
        conn.Open();
        InitializePragma(conn);
        InitializeProjectSchema(conn);

        // V2-M-07: 用事务包裹多表写入，部分失败时回滚避免产生半残数据
        using var tx = conn.BeginTransaction();
        try
        {
            SaveProjectRecord(conn, new ProjectRecord
            {
                Id = projectId,
                Name = projectName,
                ParentId = projectId,
                Version = 0,
                CreateTime = DateTime.Now
            }, tx);

            if (templateId.HasValue && templateId.Value != Guid.Empty)
            {
                CopyTemplateContent(teamId, templateId.Value, conn, tx);
            }
            else
            {
                CreateDefaultTreeGroup(conn, tx);
                CreateDefaultDocumentNode(conn, tx);
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        // WAL 模式下数据先写入 -wal 文件，File.Copy 只复制 .db 主文件会丢失数据。
        // 关闭连接前执行 checkpoint 将 WAL 数据合并到主数据库文件，并切换回 DELETE 模式。
        CheckpointAndCloseWal(conn);
    }

    public void CreateEmptyTemplateDb(Guid teamId, Guid templateId, string templateName)
    {
        var dbPath = GetTemplateDbPath(teamId, templateId);
        if (File.Exists(dbPath)) File.Delete(dbPath);

        using var conn = new SqliteConnection(BuildConnStr(dbPath));
        conn.Open();
        InitializePragma(conn);
        InitializeProjectSchema(conn);

        using var tx = conn.BeginTransaction();
        try
        {
            SaveProjectRecord(conn, new ProjectRecord
            {
                Id = templateId,
                Name = templateName,
                ParentId = templateId,
                Version = 0,
                CreateTime = DateTime.Now
            }, tx);

            CreateDefaultTreeGroup(conn, tx);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        CheckpointAndCloseWal(conn);
    }

    public void CopyProjectDb(Guid teamId, Guid sourceProjectId, Guid targetProjectId, string targetProjectName)
    {
        var sourcePath = GetProjectDbPath(teamId, sourceProjectId);
        var targetPath = GetProjectDbPath(teamId, targetProjectId);
        
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("源项目数据库文件不存在", sourcePath);

        // 复制前先 checkpoint WAL 数据到主文件，避免 WAL 文件中的数据丢失
        WalCheckpointWithRetry(sourcePath, "CopyProjectDb(源)");

        File.Copy(sourcePath, targetPath, overwrite: true);

        // 复制后对目标文件执行 checkpoint
        WalCheckpointWithRetry(targetPath, "CopyProjectDb(目标)");

        using var conn = new SqliteConnection(BuildConnStr(targetPath));
        conn.Open();
        // V2-M-09: 打开目标连接后初始化 PRAGMA（busy_timeout/synchronous/WAL 等），避免并发场景下 SQLITE_BUSY
        InitializePragma(conn);

        // V2-M-07: 用事务包裹多表写入，部分失败时回滚避免产生半残数据
        using var tx = conn.BeginTransaction();
        try
        {
            SaveProjectRecord(conn, new ProjectRecord
            {
                Id = targetProjectId,
                Name = targetProjectName,
                ParentId = targetProjectId,
                Version = 0,
                CreateTime = DateTime.Now
            }, tx);
            ResetSyncStatus(conn, tx);

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        // V2-M-09: 写入完成后 checkpoint WAL 数据到主文件，避免数据滞留 -wal 文件
        CheckpointAndCloseWal(conn);
    }

    public void CopyTemplateToProject(Guid teamId, Guid templateId, Guid projectId, string projectName)
    {
        var templatePath = GetTemplateDbPath(teamId, templateId);
        if (!File.Exists(templatePath))
        {
            templatePath = GetSystemTemplateDbPath(templateId);
        }
        
        var projectPath = GetProjectDbPath(teamId, projectId);

        if (!File.Exists(templatePath))
            throw new FileNotFoundException("模板数据库文件不存在", templatePath);

        // 复制前先 checkpoint WAL 数据到主文件，避免 WAL 文件中的数据丢失
        WalCheckpointWithRetry(templatePath, "CopyTemplateToProject(源)");

        File.Copy(templatePath, projectPath, overwrite: true);

        // File.Copy 后对目标 .db 执行 checkpoint，清除可能复制的 WAL 标记
        WalCheckpointWithRetry(projectPath, "CopyTemplateToProject(目标)");

        using var conn = new SqliteConnection(BuildConnStr(projectPath));
        conn.Open();
        InitializePragma(conn);
        using var tx = conn.BeginTransaction();
        try
        {
            SaveProjectRecord(conn, new ProjectRecord
            {
                Id = projectId,
                Name = projectName,
                ParentId = projectId,
                Version = 0,
                CreateTime = DateTime.Now
            }, tx);
            ResetSyncStatus(conn, tx);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        // 写入完成后 checkpoint WAL 数据到主文件，避免数据滞留 -wal 文件
        CheckpointAndCloseWal(conn);
    }

    /// <summary>
    /// V2-C-05: 复制源项目 .db 到目标模板 .db（写入 Templates/ 目录）。
    /// 源可以是普通项目（Projects/ 目录）或模板（Templates/ 目录），目标始终写入 Templates/ 目录。
    /// 修复原 CopyProjectDb 目标路径固定为 Projects/ 导致模板复制失效的问题。
    /// </summary>
    public void CopyProjectToTemplateDb(string sourceProjectId, string sourceTeamId, string targetTemplateId, string targetTeamId)
    {
        try
        {
            if (!Guid.TryParse(sourceProjectId, out var srcProjectGuid))
                throw new ArgumentException($"无效的源项目 ID: {sourceProjectId}", nameof(sourceProjectId));
            if (!Guid.TryParse(targetTeamId, out var tgtTeamGuid))
                throw new ArgumentException($"无效的目标团队 ID: {targetTeamId}", nameof(targetTeamId));
            if (!Guid.TryParse(targetTemplateId, out var tgtTemplateGuid))
                throw new ArgumentException($"无效的目标模板 ID: {targetTemplateId}", nameof(targetTemplateId));

            // 源 .db 路径：源可能是团队模板（Templates/ 目录）、普通项目（Projects/ 目录）或系统模板（_System/Templates/）
            // sourceTeamId 为空表示系统模板
            string sourcePath;
            if (Guid.TryParse(sourceTeamId, out var srcTeamGuid))
            {
                sourcePath = GetTemplateDbPath(srcTeamGuid, srcProjectGuid);
                if (!File.Exists(sourcePath))
                {
                    sourcePath = GetProjectDbPath(srcTeamGuid, srcProjectGuid);
                }
                if (!File.Exists(sourcePath))
                {
                    sourcePath = GetSystemTemplateDbPath(srcProjectGuid);
                }
            }
            else
            {
                sourcePath = GetSystemTemplateDbPath(srcProjectGuid);
            }

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("源数据库文件不存在", sourcePath);

            // 目标 .db 路径：模板目录
            var targetPath = GetTemplateDbPath(tgtTeamGuid, tgtTemplateGuid);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            // File.Copy 前对源 .db 执行 checkpoint，确保 -wal 数据已合并到主 .db
            WalCheckpointWithRetry(sourcePath, "CopyProjectToTemplateDb(源)");

            File.Copy(sourcePath, targetPath, overwrite: true);

            // File.Copy 后对目标 .db 执行 checkpoint，清除可能复制的 WAL 标记
            WalCheckpointWithRetry(targetPath, "CopyProjectToTemplateDb(目标)");

            _logger.LogInformation("复制项目到模板库: {Source} -> {Target}", sourcePath, targetPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CopyProjectToTemplateDb 失败: sourceProjectId={SourceProjectId} sourceTeamId={SourceTeamId} targetTemplateId={TargetTemplateId} targetTeamId={TargetTeamId}",
                sourceProjectId, sourceTeamId, targetTemplateId, targetTeamId);
            throw;
        }
    }

    /// <summary>
    /// V2-C-08: 复制模板 .db 文件到目标团队的模板目录。
    /// 源模板可能位于团队模板目录或系统模板目录（sourceTeamId 为空时表示系统模板）。
    /// </summary>
    public void CopyTemplateDb(string sourceTeamId, string sourceTemplateId, string targetTeamId, string targetTemplateId)
    {
        try
        {
            if (!Guid.TryParse(sourceTemplateId, out var srcTemplateGuid))
                throw new ArgumentException($"无效的源模板 ID: {sourceTemplateId}", nameof(sourceTemplateId));
            if (!Guid.TryParse(targetTeamId, out var tgtTeamGuid))
                throw new ArgumentException($"无效的目标团队 ID: {targetTeamId}", nameof(targetTeamId));
            if (!Guid.TryParse(targetTemplateId, out var tgtTemplateGuid))
                throw new ArgumentException($"无效的目标模板 ID: {targetTemplateId}", nameof(targetTemplateId));

            // 源模板路径：优先团队模板目录，回退到系统模板目录（sourceTeamId 为空表示系统模板）
            string sourcePath;
            if (Guid.TryParse(sourceTeamId, out var srcTeamGuid))
            {
                sourcePath = GetTemplateDbPath(srcTeamGuid, srcTemplateGuid);
                if (!File.Exists(sourcePath))
                {
                    sourcePath = GetSystemTemplateDbPath(srcTemplateGuid);
                }
            }
            else
            {
                sourcePath = GetSystemTemplateDbPath(srcTemplateGuid);
            }

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("源模板数据库文件不存在", sourcePath);

            var targetPath = GetTemplateDbPath(tgtTeamGuid, tgtTemplateGuid);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            // File.Copy 前对源 .db 执行 checkpoint，确保 -wal 数据已合并到主 .db
            WalCheckpointWithRetry(sourcePath, "CopyTemplateDb(源)");

            File.Copy(sourcePath, targetPath, overwrite: true);

            // File.Copy 后对目标 .db 执行 checkpoint，清除可能复制的 WAL 标记
            WalCheckpointWithRetry(targetPath, "CopyTemplateDb(目标)");

            _logger.LogInformation("复制模板库: {Source} -> {Target}", sourcePath, targetPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CopyTemplateDb 失败: sourceTeamId={SourceTeamId} sourceTemplateId={SourceTemplateId} targetTeamId={TargetTeamId} targetTemplateId={TargetTemplateId}",
                sourceTeamId, sourceTemplateId, targetTeamId, targetTemplateId);
            throw;
        }
    }

    public void DeleteProjectDb(Guid teamId, Guid projectId)
    {
        var dbPath = GetProjectDbPath(teamId, projectId);
        if (File.Exists(dbPath))
        {
            try { File.Delete(dbPath); }
            catch (Exception ex)
            {
                // 记录日志便于排查 .db 文件残留（常见原因：SQLite 连接未释放、文件被占用）
                _logger.LogWarning(ex, "删除项目 .db 失败: {Path}", dbPath);
            }
        }
    }

    public void DeleteTemplateDb(Guid teamId, Guid templateId)
    {
        var dbPath = GetTemplateDbPath(teamId, templateId);
        if (File.Exists(dbPath))
        {
            try { File.Delete(dbPath); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除模板 .db 失败: {Path}", dbPath);
            }
        }
    }

    private void InitializePragma(SqliteConnection conn)
    {
        using var pragma = conn.CreateCommand();
        pragma.CommandText = @"
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            PRAGMA temp_store=MEMORY;
            PRAGMA cache_size=-64000;
            PRAGMA busy_timeout=5000;";
        pragma.ExecuteNonQuery();
    }

    /// <summary>
    /// 对指定 .db 文件执行 wal_checkpoint(TRUNCATE)，重试 3 次。
    /// 用于 File.Copy 前确保源 .db 的 -wal 数据已合并，File.Copy 后确保目标 .db 干净。
    /// Pooling=False 确保连接关闭后底层 SQLite 连接真正释放，checkpoint 不会因连接池持有而 BUSY。
    /// </summary>
    public void WalCheckpointWithRetry(string dbPath, string operationName)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                using var conn = new SqliteConnection(BuildConnStr(dbPath));
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                cmd.ExecuteNonQuery();
                conn.Close();
                return;
            }
            catch (Exception ex)
            {
                if (attempt < 2)
                {
                    System.Threading.Thread.Sleep(200);
                }
                else
                {
                    _logger.LogWarning("{Op}: wal_checkpoint 失败 3 次，可能丢失 WAL 数据: {Error}", operationName, ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// 将 WAL 数据 checkpoint 到主数据库文件，并切换回 DELETE 日志模式。
    /// 确保 File.Copy 复制 .db 主文件时包含全部数据（WAL 文件不随主文件复制）。
    /// </summary>
    private void CheckpointAndCloseWal(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            PRAGMA wal_checkpoint(TRUNCATE);
            PRAGMA journal_mode=DELETE;";
        cmd.ExecuteNonQuery();
    }

    private void InitializeProjectSchema(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `Project`(
`Id` GUID PRIMARY KEY,
`Name` TEXT NOT NULL,
`Parent` GUID,
`Version` INTEGER NOT NULL DEFAULT 0,
`Number` TEXT NOT NULL,
`Category` TEXT NOT NULL,
`Note` TEXT NOT NULL,
`CreateTime` TEXT NOT NULL DEFAULT '2000-01-01',
`CustomFillConfig` TEXT);

CREATE TABLE IF NOT EXISTS `TreeGroup`(
`Id` INTEGER PRIMARY KEY,
`Name` TEXT NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL);

CREATE TABLE IF NOT EXISTS `TreeNode`(
`Id` INTEGER PRIMARY KEY,
`GroupId` INTEGER NOT NULL,
`ParentId` INTEGER,
`Name` TEXT NOT NULL,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Type` INTEGER NOT NULL,
`Level` INTEGER NOT NULL,
`Version` INTEGER NOT NULL,
`Number` TEXT,
`Permissions` TEXT NOT NULL DEFAULT '" + DefaultProjectPermissions + @"',
`Visible` INTEGER NOT NULL DEFAULT 1,
`RowWrite` INTEGER NOT NULL DEFAULT 0,
`RowRead` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Table`(
`Id` INTEGER PRIMARY KEY,
`Title` TEXT NOT NULL,
`PageSetup` TEXT NOT NULL,
`Dirty` INTEGER NOT NULL,
`Note` TEXT NOT NULL,
`HeaderHeights` TEXT,
`DefaultStyleId` INTEGER NOT NULL,
`ConsolidateSettings` TEXT,
`BorderStyle` INTEGER NOT NULL DEFAULT 0,
`FrozenCols` INTEGER NOT NULL DEFAULT 0,
`HeaderMode` INTEGER NOT NULL DEFAULT 0,
`CollectSource` TEXT,
`Locker` INTEGER,
`LockerAcquiredAt` TEXT,
`FilterInfo` TEXT,
`Foot` TEXT NOT NULL DEFAULT '{}',
`RowOwnerExclusive` INTEGER NOT NULL DEFAULT 0,
`RowOwnerLoad` INTEGER NOT NULL DEFAULT 0,
`RowOwnerLoadShare` BLOB NOT NULL DEFAULT '',
`Ticket` TEXT NOT NULL DEFAULT '',
`ControlFormula` TEXT NOT NULL DEFAULT '',
`CustomBorderStyle` TEXT);

CREATE TABLE IF NOT EXISTS `Column`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Caption` TEXT NOT NULL,
`CaptionStyle` TEXT NOT NULL,
`Width` INTEGER NOT NULL,
`Visible` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`ConsolidateAttribs` TEXT,
`SubtotalAttribs` INTEGER NOT NULL DEFAULT 0,
`Formula` TEXT,
`StyleId` INTEGER,
`Permissions` TEXT NOT NULL DEFAULT '" + DefaultProjectPermissions + @"',
`CaptionFormula` TEXT DEFAULT '',
`CrossAttributes` BLOB NOT NULL DEFAULT '');

CREATE TABLE IF NOT EXISTS `Row`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Height` INTEGER NOT NULL,
`Visible` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Locked` INTEGER,
`Role` INTEGER,
`Permissions` TEXT NOT NULL DEFAULT '" + DefaultProjectPermissions + @"',
`Creator` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Cell`(
`Id` INTEGER PRIMARY KEY,
`RowId` INTEGER NOT NULL,
`ColumnId` INTEGER NOT NULL,
`Value` BLOB NOT NULL,
`StyleId` INTEGER,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Formula` TEXT,
`CollectSource` TEXT,
`HeaderFormula` TEXT DEFAULT '');

CREATE TABLE IF NOT EXISTS `CellStyle`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`FontFamily` TEXT,
`FontSize` REAL,
`ForeColor` INTEGER,
`BackColor` INTEGER,
`Align` INTEGER,
`Margin` INTEGER,
`Bold` INTEGER,
`Italic` INTEGER,
`Underline` INTEGER,
`DataType` INTEGER,
`Format` TEXT,
`Status` INTEGER NOT NULL,
`Locked` INTEGER,
`DefaultValue` TEXT,
`Comment` TEXT);

CREATE TABLE IF NOT EXISTS `Document`(
`Id` INTEGER PRIMARY KEY,
`Locker` INTEGER,
`SectPr` TEXT,
`MergeTable` INTEGER NOT NULL DEFAULT 0,
`Dirty` INTEGER NOT NULL DEFAULT 0,
`Version` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Paragraph`(
`Id` INTEGER PRIMARY KEY,
`DocumentId` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`Stream` BLOB NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Section` BLOB,
`Comment` TEXT);

CREATE TABLE IF NOT EXISTS `Merge`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`TopLeft` INTEGER NOT NULL,
`BottomRight` INTEGER NOT NULL,
`Status` INTEGER NOT NULL);

CREATE TABLE IF NOT EXISTS `DataReference`(
`Id` INTEGER PRIMARY KEY,
`Key` TEXT NOT NULL,
`Value` TEXT,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Kind` INTEGER NOT NULL DEFAULT 2);

CREATE TABLE IF NOT EXISTS `ValidationFormula`(
`Id` INTEGER PRIMARY KEY,
`LeftExpr` TEXT NOT NULL,
`Operator` INT NOT NULL,
`RightExpr` TEXT NOT NULL,
`Note` TEXT NOT NULL,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`TableId` INTEGER,
`DocumentFieldId` INTEGER);

CREATE TABLE IF NOT EXISTS `Image`(
`Id` INTEGER PRIMARY KEY,
`FileId` GUID NOT NULL,
`Dirty` INTEGER NOT NULL DEFAULT 0,
`CenterX` REAL NOT NULL DEFAULT 0.5,
`CenterY` REAL NOT NULL DEFAULT 0.5,
`ZoomFactor` REAL NOT NULL DEFAULT 1.0,
`PageSetup` TEXT,
`RotateFlip` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Pdf`(
`Id` INTEGER PRIMARY KEY,
`FileId` GUID NOT NULL);

CREATE TABLE IF NOT EXISTS `Snapshot`(
`Id` INTEGER PRIMARY KEY,
`TreeNodeId` INTEGER NOT NULL,
`DateTime` TEXT NOT NULL,
`Size` INTEGER NOT NULL,
`Kind` INTEGER NOT NULL,
`Name` TEXT NOT NULL,
`Deleted` INTEGER NOT NULL);

CREATE TABLE IF NOT EXISTS `CellProp` (
`TableId` INTEGER NOT NULL,
`CellId` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Attachments` BLOB NOT NULL DEFAULT '',
PRIMARY KEY (`TableId`,`CellId`));

CREATE INDEX `idx_Cell_RowId` ON `Cell` (`RowId`);
CREATE INDEX `idx_Row_TableId` ON `Row` (`TableId`);
CREATE INDEX `idx_Column_TableId` ON `Column` (`TableId`);

PRAGMA user_version = 43;";
        cmd.ExecuteNonQuery();

        // V2-C-02: 升级已存在的 Document 表，补充 Version 列
        EnsureDocumentVersionColumn(conn);

        // 节点级强锁：升级已存在的 Table 表，补充 LockerAcquiredAt 列
        EnsureTableLockerAcquiredAtColumn(conn);
    }

    /// <summary>
    /// V2-C-02: 检查 Document 表是否已存在 Version 列，不存在则添加。
    /// 用于升级已创建的旧版 .db 文件（CREATE TABLE IF NOT EXISTS 不会为已存在的表补充新列）。
    /// </summary>
    private void EnsureDocumentVersionColumn(SqliteConnection conn)
    {
        try
        {
            bool hasVersion = false;
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "PRAGMA table_info(`Document`)";
                using var reader = checkCmd.ExecuteReader();
                while (reader.Read())
                {
                    var colName = reader.GetString(1);
                    if (string.Equals(colName, "Version", StringComparison.OrdinalIgnoreCase))
                    {
                        hasVersion = true;
                        break;
                    }
                }
            }

            if (!hasVersion)
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE `Document` ADD COLUMN `Version` INTEGER NOT NULL DEFAULT 0";
                alterCmd.ExecuteNonQuery();
                _logger.LogInformation("已为 Document 表添加 Version 列");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "为 Document 表添加 Version 列失败（可能已存在）");
        }
    }

    /// <summary>
    /// 节点级强锁：检查 Table 表是否已存在 LockerAcquiredAt 列，不存在则添加。
    /// 用于升级已创建的旧版 .db 文件（CREATE TABLE IF NOT EXISTS 不会为已存在的表补充新列）。
    /// LockerAcquiredAt 记录锁获取时间戳，服务端 PushTable 时据此判断锁是否超时（30 分钟）。
    /// </summary>
    private void EnsureTableLockerAcquiredAtColumn(SqliteConnection conn)
    {
        try
        {
            bool hasColumn = false;
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "PRAGMA table_info(`Table`)";
                using var reader = checkCmd.ExecuteReader();
                while (reader.Read())
                {
                    var colName = reader.GetString(1);
                    if (string.Equals(colName, "LockerAcquiredAt", StringComparison.OrdinalIgnoreCase))
                    {
                        hasColumn = true;
                        break;
                    }
                }
            }

            if (!hasColumn)
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE `Table` ADD COLUMN `LockerAcquiredAt` TEXT";
                alterCmd.ExecuteNonQuery();
                _logger.LogInformation("已为 Table 表添加 LockerAcquiredAt 列");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "为 Table 表添加 LockerAcquiredAt 列失败（可能已存在）");
        }
    }

    private void SaveProjectRecord(SqliteConnection conn, ProjectRecord record, SqliteTransaction? tx = null)
    {
        using var cmd = conn.CreateCommand();
        if (tx != null) cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `Project`(`Id`,`Name`,`Parent`,`Version`,`Number`,`Category`,`Note`,`CreateTime`,`CustomFillConfig`)
            VALUES (@Id,@Name,@ParentId,@Version,@Number,@Category,@Note,@CreateTime,@CustomFillConfig)";
        cmd.Parameters.AddWithValue("@Id", record.Id.ToByteArray());
        cmd.Parameters.AddWithValue("@Name", record.Name ?? "");
        cmd.Parameters.AddWithValue("@ParentId", record.ParentId.ToByteArray());
        cmd.Parameters.AddWithValue("@Version", record.Version);
        cmd.Parameters.AddWithValue("@Number", record.Number ?? "");
        cmd.Parameters.AddWithValue("@Category", record.Category ?? "");
        cmd.Parameters.AddWithValue("@Note", record.Note ?? "");
        cmd.Parameters.AddWithValue("@CreateTime", record.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@CustomFillConfig", DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    private void CreateDefaultTreeGroup(SqliteConnection conn, SqliteTransaction? tx = null)
    {
        using var cmd = conn.CreateCommand();
        if (tx != null) cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `TreeGroup`(`Id`,`Name`,`Index`,`Status`,`Dirty`,`ServerIndex`)
            VALUES (@Id,@Name,@Index,@Status,@Dirty,@ServerIndex)";
        cmd.Parameters.AddWithValue("@Id", 1L);
        cmd.Parameters.AddWithValue("@Name", "工作底稿");
        cmd.Parameters.AddWithValue("@Index", 0);
        cmd.Parameters.AddWithValue("@Status", 0);
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.Parameters.AddWithValue("@ServerIndex", 0);
        cmd.ExecuteNonQuery();
    }

    private void CreateDefaultDocumentNode(SqliteConnection conn, SqliteTransaction? tx = null)
    {
        using var cmd = conn.CreateCommand();
        if (tx != null) cmd.Transaction = tx;
        cmd.CommandText = @"INSERT INTO `TreeNode`(`Id`,`GroupId`,`ParentId`,`Name`,`Type`,`Status`,`Dirty`,`Index`,`Level`,`Version`,`ServerIndex`)
            VALUES (@Id,@GroupId,@ParentId,@Name,@Type,@Status,@Dirty,@Index,@Level,@Version,@ServerIndex)";
        cmd.Parameters.AddWithValue("@Id", 2L);
        cmd.Parameters.AddWithValue("@GroupId", 1L);
        cmd.Parameters.AddWithValue("@ParentId", DBNull.Value);
        cmd.Parameters.AddWithValue("@Name", "审计报告");
        cmd.Parameters.AddWithValue("@Type", 2);
        cmd.Parameters.AddWithValue("@Status", 0);
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.Parameters.AddWithValue("@Index", 0);
        cmd.Parameters.AddWithValue("@Level", 0);
        cmd.Parameters.AddWithValue("@Version", 0);
        cmd.Parameters.AddWithValue("@ServerIndex", 0);
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"INSERT INTO `Document`(`Id`,`Locker`,`MergeTable`,`Dirty`)
            VALUES (@Id,@Locker,@MergeTable,@Dirty)";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@Id", 2L);
        cmd.Parameters.AddWithValue("@Locker", 0);
        cmd.Parameters.AddWithValue("@MergeTable", 0);
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.ExecuteNonQuery();
    }

    public void CopyTemplateContent(Guid teamId, Guid templateId, SqliteConnection targetConn, SqliteTransaction? externalTx = null)
    {
        var templatePath = GetTemplateDbPath(teamId, templateId);
        if (!File.Exists(templatePath))
        {
            templatePath = GetSystemTemplateDbPath(templateId);
        }

        // V2-M-07: 用事务包裹多表写入，部分失败时回滚避免产生半残数据。
        // externalTx 由调用方传入时复用，否则自建事务并在完成时提交。
        bool ownsTx = externalTx == null;
        SqliteTransaction tx = ownsTx ? targetConn.BeginTransaction() : externalTx!;

        try
        {
            if (!File.Exists(templatePath))
            {
                // 模板文件不存在，写入默认树结构
                CreateDefaultTreeGroup(targetConn, tx);
                CreateDefaultDocumentNode(targetConn, tx);
            }
            else
            {
                // 从模板文件逐表复制
                using var sourceConn = new SqliteConnection(BuildConnStr(templatePath, readOnly: true));
                sourceConn.Open();

                CopyTable(sourceConn, targetConn, "TreeGroup", tx);
                CopyTable(sourceConn, targetConn, "TreeNode", tx);
                CopyTable(sourceConn, targetConn, "DataReference", tx);
                CopyTable(sourceConn, targetConn, "ValidationFormula", tx);
                CopyTable(sourceConn, targetConn, "Document", tx);
                CopyTable(sourceConn, targetConn, "Table", tx);
                CopyTable(sourceConn, targetConn, "Column", tx);
                CopyTable(sourceConn, targetConn, "Row", tx);
                CopyTable(sourceConn, targetConn, "Cell", tx);
                CopyTable(sourceConn, targetConn, "CellStyle", tx);
                CopyTable(sourceConn, targetConn, "Paragraph", tx);
                CopyTable(sourceConn, targetConn, "Merge", tx);
                CopyTable(sourceConn, targetConn, "Image", tx);
                CopyTable(sourceConn, targetConn, "Pdf", tx);
                CopyTable(sourceConn, targetConn, "Snapshot", tx);
                CopyTable(sourceConn, targetConn, "CellProp", tx);

                // V2-H-06 修复：CopyTable 内部异常被空 catch 吞掉，静默失败后 TreeGroup/TreeNode 可能为空。
                // 复制完成后查询 TreeGroup/TreeNode 行数，为 0 时补写默认数据，保证项目至少有一个根分组和文档节点。
                EnsureDefaultTreeExists(targetConn, tx);

                ResetSyncStatus(targetConn, tx);
            }

            if (ownsTx) tx.Commit();
        }
        catch
        {
            if (ownsTx) tx.Rollback();
            throw;
        }
        finally
        {
            if (ownsTx) tx.Dispose();
        }
    }

    /// <summary>
    /// V2-H-06 修复：检查 TreeGroup/TreeNode 表是否为空，为空时补写默认数据。
    /// 用于 CopyTemplateContent 完成后的兜底校验，避免模板复制静默失败导致项目树形为空。
    /// </summary>
    private void EnsureDefaultTreeExists(SqliteConnection conn, SqliteTransaction? tx = null)
    {
        long treeGroupCount = 0;
        long treeNodeCount = 0;
        try
        {
            using (var cmd = conn.CreateCommand())
            {
                if (tx != null) cmd.Transaction = tx;
                cmd.CommandText = "SELECT COUNT(*) FROM `TreeGroup`";
                treeGroupCount = Convert.ToInt64(cmd.ExecuteScalar());
            }
            using (var cmd = conn.CreateCommand())
            {
                if (tx != null) cmd.Transaction = tx;
                cmd.CommandText = "SELECT COUNT(*) FROM `TreeNode`";
                treeNodeCount = Convert.ToInt64(cmd.ExecuteScalar());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "EnsureDefaultTreeExists 查询 TreeGroup/TreeNode 失败");
            return;
        }

        if (treeGroupCount == 0)
        {
            _logger.LogWarning("CopyTemplateContent 完成后 TreeGroup 为空，补写默认分组");
            try { CreateDefaultTreeGroup(conn, tx); }
            catch (Exception ex) { _logger.LogWarning(ex, "补写默认 TreeGroup 失败"); }
        }
        if (treeNodeCount == 0)
        {
            _logger.LogWarning("CopyTemplateContent 完成后 TreeNode 为空，补写默认文档节点");
            try { CreateDefaultDocumentNode(conn, tx); }
            catch (Exception ex) { _logger.LogWarning(ex, "补写默认 TreeNode/Document 失败"); }
        }
    }

    public void SeedSystemTemplates(SqliteStorage db)
    {
        var systemTemplatesDir = Path.Combine(_basePath, "_System", "Templates");
        Directory.CreateDirectory(systemTemplatesDir);

        // 已有模板文件则跳过
        var existingTemplates = Directory.GetFiles(systemTemplatesDir, "*.db");
        if (existingTemplates.Length > 0)
        {
            _logger.LogInformation("系统模板目录已有 {Count} 个模板文件，跳过播种", existingTemplates.Length);
            return;
        }

        // V2-M-08: 模板源目录仅从部署目录下的 Templates 子目录读取，删除硬编码开发环境路径回退
        var sourceDir = Path.Combine(AppContext.BaseDirectory, "Templates");
        if (!Directory.Exists(sourceDir))
        {
            // 播种失败时记录 Error 级别日志（生产环境首次启动无法播种会导致用户创建项目时无模板可选）
            _logger.LogError("模板源目录不存在: {SourceDir}，系统模板播种失败", sourceDir);
            return;
        }

        var sourceFiles = Directory.GetFiles(sourceDir, "*.db");
        _logger.LogInformation("开始播种系统模板，源目录: {SourceDir}，共 {Count} 个模板", sourceDir, sourceFiles.Length);

        int seeded = 0;
        foreach (var sourceFile in sourceFiles)
        {
            try
            {
                // 读取模板 .db 内部的 Project 表获取 GUID 和元数据
                Guid templateId;
                string templateName = "";
                string templateNumber = "";
                string templateCategory = "";
                string templateNote = "";
                int templateVersion = 0;

                using (var srcConn = new SqliteConnection(BuildConnStr(sourceFile, readOnly: true)))
                {
                    srcConn.Open();
                    using var cmd = srcConn.CreateCommand();
                    cmd.CommandText = "SELECT Id, Name, Number, Category, Note, Version FROM Project LIMIT 1";
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        _logger.LogWarning("模板文件 {File} 的 Project 表无数据，跳过", sourceFile);
                        continue;
                    }

                    // GUID 可能存储为 16 字节二进制 BLOB（System.Data.SQLite 默认行为）或文本字符串
                    templateId = Guid.Empty;
                    var fieldType = reader.GetFieldType(0);
                    if (fieldType == typeof(byte[]))
                    {
                        var idBytes = reader.IsDBNull(0) ? null : (byte[])reader[0];
                        if (idBytes != null && idBytes.Length == 16)
                        {
                            templateId = new Guid(idBytes);
                        }
                    }
                    else
                    {
                        var idStr = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        Guid.TryParse(idStr, out templateId);
                    }

                    if (templateId == Guid.Empty)
                    {
                        _logger.LogWarning("模板文件 {File} 的 Id 不是有效 GUID", sourceFile);
                        continue;
                    }

                    templateName = reader.IsDBNull(1) ? Path.GetFileNameWithoutExtension(sourceFile) : reader.GetString(1);
                    templateNumber = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    templateCategory = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    templateNote = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    templateVersion = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                }

                // 复制模板文件到系统模板目录
                var targetPath = GetSystemTemplateDbPath(templateId);
                File.Copy(sourceFile, targetPath, overwrite: true);

                // File.Copy 后对目标 .db 执行 checkpoint，清除可能复制的 WAL 标记
                WalCheckpointWithRetry(targetPath, "SeedSystemTemplates");

                _logger.LogInformation("已复制模板: {Name} ({Id}) -> {Target}", templateName, templateId, targetPath);

                // 在主库 Projects 表中注册模板
                using var mainConn = db.CreateConnection();
                mainConn.Open();
                using var insertCmd = mainConn.CreateCommand();
                insertCmd.CommandText = @"INSERT OR IGNORE INTO Projects
                    (Id, Number, Name, Category, Note, ParentId, CreatorId, Version, Type, ChargeType, TeamVisible, TemplateId, CreateTime, TeamId, IsTemplate, IsDemo, OperationId)
                    VALUES (@id, @number, @name, @category, @note, @parentId, 0, @version, 1, 0, 0, NULL, @createTime, NULL, 1, 0, 0)";
                insertCmd.Parameters.AddWithValue("@id", templateId.ToString());
                insertCmd.Parameters.AddWithValue("@number", templateNumber ?? "");
                insertCmd.Parameters.AddWithValue("@name", templateName ?? "");
                insertCmd.Parameters.AddWithValue("@category", templateCategory ?? "");
                insertCmd.Parameters.AddWithValue("@note", templateNote ?? "");
                insertCmd.Parameters.AddWithValue("@parentId", templateId.ToString());
                insertCmd.Parameters.AddWithValue("@version", templateVersion);
                insertCmd.Parameters.AddWithValue("@createTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insertCmd.ExecuteNonQuery();

                seeded++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "播种模板 {File} 失败", sourceFile);
            }
        }

        _logger.LogInformation("系统模板播种完成，成功 {Seeded}/{Total}", seeded, sourceFiles.Length);
    }

    private void CopyTable(SqliteConnection sourceConn, SqliteConnection targetConn, string tableName, SqliteTransaction? tx = null)
    {
        try
        {
            using var cmd = sourceConn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM `{tableName}`";
            using var reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return;
            }

            var cols = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                cols.Add(reader.GetName(i));
            }

            var colNames = string.Join(",", cols.Select(c => $"`{c}`"));
            var paramNames = string.Join(",", cols.Select((_, i) => $"@p{i}"));

            using var insertCmd = targetConn.CreateCommand();
            if (tx != null) insertCmd.Transaction = tx;
            insertCmd.CommandText = $"INSERT INTO `{tableName}`({colNames}) VALUES ({paramNames})";

            int rowNum = 0;
            while (reader.Read())
            {
                insertCmd.Parameters.Clear();
                for (int i = 0; i < cols.Count; i++)
                {
                    object value;
                    if (reader.IsDBNull(i))
                    {
                        value = DBNull.Value;
                    }
                    else
                    {
                        value = reader.GetValue(i);
                    }
                    insertCmd.Parameters.AddWithValue($"@p{i}", value);
                }

                insertCmd.ExecuteNonQuery();
                rowNum++;
            }
        }
        catch (Exception ex)
        {
            // 记录日志后向上抛出，避免模板复制部分失败被静默吞掉导致项目数据残缺
            _logger.LogError(ex, "CopyTable {Table} 失败", tableName);
            throw;
        }
    }

    private void ResetSyncStatus(SqliteConnection conn, SqliteTransaction? tx = null)
    {
        try
        {
            using var cmd = conn.CreateCommand();
            if (tx != null) cmd.Transaction = tx;
            cmd.CommandText = @"
                UPDATE TreeNode SET Version=0, Dirty=0, ServerIndex=0, Status=0;
                UPDATE TreeGroup SET Dirty=0, ServerIndex=0, Status=0;
                UPDATE [Table] SET Dirty=0;
                UPDATE Document SET Dirty=0;";
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetSyncStatus 失败");
        }
    }

    public class ProjectRecord
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid ParentId { get; set; }
        public int Version { get; set; }
        public string? Number { get; set; }
        public string? Category { get; set; }
        public string? Note { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
