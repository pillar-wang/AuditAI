﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System.Security.Cryptography;
using System.Text;
using System.Web;
using AuditApiServer.Infra;
using AuditApiServer.Models;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// SQLite 存储层。
/// 维护服务端 SQLite 数据库的连接、初始化（建表 + 字段扩展）与默认数据 Seed。
/// 在 MVP 5 张表（Users/Teams/Projects/Tokens/UserTeams）基础上，扩展 10 张新表以支撑完整云端功能。
/// </summary>
public class SqliteStorage
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteStorage> _logger;

    public SqliteStorage(IConfiguration config, ILogger<SqliteStorage> logger)
    {
        // 阶段 9 Task 9.3：Microsoft.Data.Sqlite 默认 Pooling=True，不支持 Max Pool Size / Connection Lifetime 关键字
        // SQLite 连接池由内部管理（默认上限足够），无需也无法在连接字符串中显式配置
        _connectionString = config.GetConnectionString("Sqlite")
            ?? "Data Source=Data/auditai_server.db";
        _logger = logger;
    }

    public void Initialize()
    {
        var dir = Path.GetDirectoryName(_connectionString
            .Replace("Data Source=", "").Split(';')[0].Trim());
        // 兼容相对路径
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // 阶段 9 Task 9.3：SQLite 性能 PRAGMA 配置
        // WAL 模式：提升并发读写性能；persistent 设置一次即可对整个 DB 生效
        // synchronous=NORMAL：WAL 配套，性能与安全平衡（比 FULL 快，比 OFF 安全）
        // temp_store=MEMORY：临时表与索引存内存
        // cache_size=-64000：负值表示 KB，-64000 = 64MB 页缓存
        using (var pragma = conn.CreateCommand())
        {
            pragma.CommandText = @"
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=NORMAL;
                PRAGMA temp_store=MEMORY;
                PRAGMA cache_size=-64000;
                PRAGMA busy_timeout=5000;";
            pragma.ExecuteNonQuery();
        }

        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY,
                UserName TEXT NOT NULL UNIQUE,
                Password TEXT,
                Name TEXT,
                Email TEXT,
                Phone TEXT,
                Role INTEGER DEFAULT 0,
                TeamId TEXT,
                IsTeamAdmin INTEGER DEFAULT 0,
                IsSystemAdmin INTEGER DEFAULT 0,
                IsDataAdmin INTEGER DEFAULT 0,
                LicenseDate TEXT,
                IsActive INTEGER DEFAULT 1,
                CreateTime TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Teams (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Level INTEGER DEFAULT 3,
                PayStatus INTEGER DEFAULT 1,
                LicenseDate TEXT,
                OwnerUserId INTEGER
            );

            CREATE TABLE IF NOT EXISTS Projects (
                Id TEXT PRIMARY KEY,
                Number TEXT,
                Name TEXT,
                Category TEXT,
                Auditee TEXT,
                Note TEXT,
                ParentId TEXT,
                CreatorId INTEGER,
                Version INTEGER DEFAULT 0,
                Type INTEGER DEFAULT 0,
                ChargeType INTEGER DEFAULT 0,
                TeamVisible INTEGER DEFAULT 0,
                TemplateId TEXT,
                CreateTime TEXT DEFAULT CURRENT_TIMESTAMP,
                TeamId TEXT
            );

            CREATE TABLE IF NOT EXISTS Tokens (
                UserId INTEGER PRIMARY KEY,
                TokenValue TEXT,
                LastToken TEXT,
                UpdateToken TEXT,
                UpdateTime TEXT,
                ExpiresAt TEXT,
                LastRefreshAt TEXT
            );

            CREATE TABLE IF NOT EXISTS UserTeams (
                UserId INTEGER,
                TeamId TEXT,
                PRIMARY KEY (UserId, TeamId)
            );

            CREATE TABLE IF NOT EXISTS UserGroups (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                ParentId INTEGER,
                TeamId TEXT NOT NULL,
                CreatedAt TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS ProjectMembers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                UserId INTEGER NOT NULL,
                Role INTEGER DEFAULT 0,
                JoinedAt TEXT DEFAULT (datetime('now')),
                UNIQUE (ProjectId, UserId)
            );

            CREATE TABLE IF NOT EXISTS TableSchemas (
                Id TEXT NOT NULL,
                ProjectId TEXT NOT NULL,
                Name TEXT,
                Version INTEGER DEFAULT 1,
                Mask INTEGER DEFAULT 0,
                ColumnsData BLOB,
                RowsData BLOB,
                CellsData BLOB,
                CellStylesData BLOB,
                MergesData BLOB,
                CellPropsData BLOB,
                PageSetup BLOB,
                BorderStyle BLOB,
                HeaderMode INTEGER DEFAULT 0,
                FrozenCols INTEGER DEFAULT 0,
                CollectSource TEXT,
                Locker TEXT,
                FilterInfo TEXT,
                Foot TEXT,
                ControlFormula TEXT,
                UpdatedAt TEXT DEFAULT (datetime('now')),
                UpdatedBy INTEGER,
                PRIMARY KEY (Id, ProjectId)
            );

            CREATE TABLE IF NOT EXISTS Documents (
                Id TEXT NOT NULL,
                ProjectId TEXT NOT NULL,
                Name TEXT,
                Version INTEGER DEFAULT 1,
                ParagraphsData BLOB,
                UpdatedAt TEXT DEFAULT (datetime('now')),
                UpdatedBy INTEGER,
                PRIMARY KEY (Id, ProjectId)
            );

            CREATE TABLE IF NOT EXISTS VersionHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                TargetId TEXT NOT NULL,
                TargetType TEXT NOT NULL,
                Version INTEGER NOT NULL,
                ChangeType TEXT,
                Snapshot BLOB,
                CreatedAt TEXT DEFAULT (datetime('now')),
                CreatedBy INTEGER
            );

            CREATE TABLE IF NOT EXISTS ServerTasks (
                Id TEXT PRIMARY KEY,
                UserId INTEGER NOT NULL,
                TaskType TEXT NOT NULL,
                Status INTEGER DEFAULT 0,
                Progress REAL DEFAULT 0,
                InputFilePath TEXT,
                OutputFilePath TEXT,
                Description TEXT,
                Result TEXT,
                CreatedAt TEXT DEFAULT (datetime('now')),
                CompletedAt TEXT
            );

            CREATE TABLE IF NOT EXISTS ValidateCodes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Key TEXT NOT NULL,
                Code TEXT NOT NULL,
                ExpiresAt TEXT NOT NULL,
                CreatedAt TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS LoginLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Version TEXT,
                IPAddress TEXT,
                MachineCode TEXT,
                HasProcess INTEGER DEFAULT 0,
                LoginAt TEXT DEFAULT (datetime('now')),
                LogoutAt TEXT
            );

            CREATE TABLE IF NOT EXISTS ProjectFiles (
                Id TEXT PRIMARY KEY,
                ProjectId TEXT NOT NULL,
                FileName TEXT NOT NULL,
                FileSize INTEGER DEFAULT 0,
                ContentType TEXT,
                StoragePath TEXT NOT NULL,
                UploadedBy INTEGER NOT NULL,
                UploadedAt TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS DataDictionary (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DicType TEXT NOT NULL,
                Version INTEGER NOT NULL,
                Data TEXT NOT NULL,
                UpdatedAt TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS LoginAttempts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IPAddress TEXT NOT NULL,
                UserName TEXT,
                Success INTEGER DEFAULT 0,
                AttemptAt TEXT DEFAULT (datetime('now'))
            );

            -- 阶段 1 Task 2：企业层级表
            CREATE TABLE IF NOT EXISTS Enterprises (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Logo BLOB,
                ContactEmail TEXT,
                ContactPhone TEXT,
                IsActive INTEGER DEFAULT 1,
                CreateTime TEXT DEFAULT CURRENT_TIMESTAMP
            );

            -- 阶段 1 Task 3：License 主表
            CREATE TABLE IF NOT EXISTS Licenses (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                LicenseKey TEXT NOT NULL UNIQUE,
                ProductEdition INTEGER DEFAULT 2,
                PlanType INTEGER DEFAULT 0,
                Seats INTEGER DEFAULT 5,
                MaxProjects INTEGER DEFAULT 10,
                MaxTemplates INTEGER DEFAULT 5,
                StartDate TEXT,
                EndDate TEXT,
                IsActive INTEGER DEFAULT 1,
                OwnerType TEXT DEFAULT 'Team',
                OwnerId TEXT,
                CreateTime TEXT DEFAULT CURRENT_TIMESTAMP
            );

            -- 阶段 1 Task 4：机器绑定表
            CREATE TABLE IF NOT EXISTS Activations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                LicenseId INTEGER NOT NULL,
                MachineCode TEXT NOT NULL,
                ActivatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                LastSeenAt TEXT,
                IsActive INTEGER DEFAULT 1,
                UNIQUE(LicenseId, MachineCode)
            );

            -- 阶段 1 Task 5：订阅生命周期表
            CREATE TABLE IF NOT EXISTS Subscriptions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                LicenseId INTEGER NOT NULL,
                Plan TEXT,
                BillingCycle INTEGER DEFAULT 1,
                AutoRenew INTEGER DEFAULT 0,
                PaymentStatus INTEGER DEFAULT 0,
                NextBillingDate TEXT,
                RenewedFrom INTEGER,
                CreateTime TEXT DEFAULT CURRENT_TIMESTAMP
            );

            -- 阶段 1 Task 6：团队邀请表（Status: 0=Pending, 1=Accepted, 2=Expired）
            CREATE TABLE IF NOT EXISTS TeamInvitations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                InviteToken TEXT NOT NULL UNIQUE,
                TeamId TEXT NOT NULL,
                Email TEXT,
                Phone TEXT,
                Role INTEGER DEFAULT 0,
                InvitedBy INTEGER,
                ExpiresAt TEXT,
                Status INTEGER DEFAULT 0,
                CreateTime TEXT DEFAULT CURRENT_TIMESTAMP
            );

            -- 激活码注册控制 Task 1：激活码管理表
            -- Status: 0=未使用, 1=已使用, 2=已禁用
            CREATE TABLE IF NOT EXISTS ActivationCodes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE,
                Status INTEGER DEFAULT 0,
                UsedByUserId INTEGER,
                MachineCode TEXT,
                UsedAt TEXT,
                BatchId TEXT,
                CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
            );

            CREATE INDEX IF NOT EXISTS idx_tableschema_project ON TableSchemas(ProjectId);
            CREATE INDEX IF NOT EXISTS idx_documents_project ON Documents(ProjectId);
            CREATE INDEX IF NOT EXISTS idx_files_project ON ProjectFiles(ProjectId);
            CREATE INDEX IF NOT EXISTS idx_history_project ON VersionHistory(ProjectId);
            CREATE INDEX IF NOT EXISTS idx_history_target ON VersionHistory(TargetId, TargetType);
            -- 版本号批量查询专用可覆盖索引：QueryTableVersions/QueryDocumentVersions 等按
            -- (ProjectId, TargetType, TargetId IN (...)) 分组取 MAX(Version)。
            -- 旧索引无法覆盖 SELECT 的 Version 列，SQLite 必须回表读取整行（含 Snapshot BLOB），
            -- 项目节点多、历史版本累积后单次查询会扫描大量宽行。此索引让查询变成索引内扫描，
            -- 不回表，是"项目表很多时版本查询超时"的根本解法。
            CREATE INDEX IF NOT EXISTS idx_history_version_lookup ON VersionHistory(ProjectId, TargetType, TargetId, Version);
            CREATE INDEX IF NOT EXISTS idx_validatecodes_key ON ValidateCodes(Key);
            CREATE INDEX IF NOT EXISTS idx_loginlogs_user ON LoginLogs(UserId);
            CREATE INDEX IF NOT EXISTS idx_projectmembers_project ON ProjectMembers(ProjectId);
            CREATE INDEX IF NOT EXISTS idx_usergroups_team ON UserGroups(TeamId);
            CREATE INDEX IF NOT EXISTS idx_loginattempts_ip ON LoginAttempts(IPAddress, AttemptAt);
            CREATE INDEX IF NOT EXISTS idx_licenses_key ON Licenses(LicenseKey);
            CREATE INDEX IF NOT EXISTS idx_licenses_owner ON Licenses(OwnerType, OwnerId);
            CREATE INDEX IF NOT EXISTS idx_activationcodes_code ON ActivationCodes(Code);
            CREATE INDEX IF NOT EXISTS idx_activationcodes_status ON ActivationCodes(Status);

            -- 阶段 10：项目同步表（PushProjectQuick / PullProject）
            -- 客户端 Syncer.Push(Project) 上传的 Groups/Nodes/DataRefs/VFs 增量数据
            CREATE TABLE IF NOT EXISTS ProjectTreeGroups (
                Id INTEGER PRIMARY KEY,
                ProjectId TEXT NOT NULL,
                Name TEXT,
                TreeIndex INTEGER DEFAULT 0,
                ServerIndex INTEGER DEFAULT 0,
                Status INTEGER DEFAULT 0,
                Dirty INTEGER DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS idx_ptg_project ON ProjectTreeGroups(ProjectId);

            CREATE TABLE IF NOT EXISTS ProjectTreeNodes (
                Id INTEGER PRIMARY KEY,
                ProjectId TEXT NOT NULL,
                GroupId INTEGER NOT NULL,
                ParentId INTEGER,
                Name TEXT,
                TreeIndex INTEGER DEFAULT 0,
                ServerIndex INTEGER DEFAULT 0,
                Status INTEGER DEFAULT 0,
                Dirty INTEGER DEFAULT 0,
                Type INTEGER NOT NULL,
                Level INTEGER DEFAULT 0,
                Version INTEGER DEFAULT 0,
                Number TEXT,
                Permissions TEXT,
                Visible INTEGER DEFAULT 1,
                RowWrite INTEGER DEFAULT 0,
                RowRead INTEGER DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS idx_ptn_project ON ProjectTreeNodes(ProjectId);

            CREATE TABLE IF NOT EXISTS ProjectDataReferences (
                Id INTEGER PRIMARY KEY,
                ProjectId TEXT NOT NULL,
                Key TEXT,
                Value TEXT,
                Kind INTEGER DEFAULT 2,
                Status INTEGER DEFAULT 0,
                Dirty INTEGER DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS idx_pdr_project ON ProjectDataReferences(ProjectId);

            CREATE TABLE IF NOT EXISTS ProjectValidationFormulas (
                Id INTEGER PRIMARY KEY,
                ProjectId TEXT NOT NULL,
                LeftExpr TEXT,
                Operator INTEGER,
                RightExpr TEXT,
                Note TEXT,
                TableId INTEGER,
                DocumentFieldId INTEGER,
                Status INTEGER DEFAULT 0,
                Dirty INTEGER DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS idx_pvf_project ON ProjectValidationFormulas(ProjectId);

            -- 阶段 10 补充：项目变更历史表（用于 PullProject 增量返回）
            -- 每次 PushProject 时记录变更，PullProject 根据 Version 范围聚合返回增量
            CREATE TABLE IF NOT EXISTS ProjectChanges (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                Version INTEGER NOT NULL,
                EntityType INTEGER NOT NULL,
                Action INTEGER NOT NULL,
                EntityId INTEGER NOT NULL,
                Payload TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_pc_project_version ON ProjectChanges(ProjectId, Version);
            CREATE INDEX IF NOT EXISTS idx_pc_entity ON ProjectChanges(ProjectId, EntityType, EntityId);

            -- 审核/归档工作流：审核单（每轮一条）、顺序审批节点、项目归档记录
            CREATE TABLE IF NOT EXISTS ReviewSubmissions (
                Id TEXT PRIMARY KEY,
                ProjectId TEXT NOT NULL,
                TeamId TEXT,
                Round INTEGER DEFAULT 1,
                SubmitterId INTEGER,
                ReportType INTEGER DEFAULT 0,
                ReportNo TEXT,
                OpinionType INTEGER DEFAULT 0,
                Note TEXT,
                Status INTEGER DEFAULT 0,
                TotalLevel INTEGER DEFAULT 1,
                SubmitTime TEXT DEFAULT (datetime('now')),
                FinishTime TEXT
            );

            CREATE TABLE IF NOT EXISTS ReviewNodes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SubmissionId TEXT NOT NULL,
                Level INTEGER,
                ReviewerId INTEGER,
                ReviewerName TEXT,
                Status INTEGER DEFAULT 0,
                Comment TEXT,
                ReviewTime TEXT
            );

            CREATE TABLE IF NOT EXISTS ProjectArchives (
                Id TEXT PRIMARY KEY,
                ProjectId TEXT NOT NULL,
                TeamId TEXT,
                ArchiveNo TEXT,
                OperatorId INTEGER,
                ArchiveTime TEXT DEFAULT (datetime('now')),
                RetentionYears INTEGER DEFAULT 10,
                Note TEXT
            );

            CREATE INDEX IF NOT EXISTS IX_ReviewSubmissions_Project ON ReviewSubmissions(ProjectId);
            CREATE INDEX IF NOT EXISTS IX_ReviewSubmissions_Team ON ReviewSubmissions(TeamId);
            CREATE INDEX IF NOT EXISTS IX_ReviewNodes_Submission ON ReviewNodes(SubmissionId);
            CREATE INDEX IF NOT EXISTS IX_ReviewNodes_Reviewer ON ReviewNodes(ReviewerId, Status);
            CREATE INDEX IF NOT EXISTS IX_ProjectArchives_Project ON ProjectArchives(ProjectId);
            CREATE INDEX IF NOT EXISTS IX_ProjectArchives_Team ON ProjectArchives(TeamId);

            -- 审批流程模板（OA 式预置审批链）：模板主表 + 节点表（节点审批人三级指定方式见 AssigneeType 注释）
            CREATE TABLE IF NOT EXISTS ReviewFlowTemplates (
                Id TEXT PRIMARY KEY,
                TeamId TEXT NOT NULL,
                Name TEXT NOT NULL,
                TotalLevel INTEGER DEFAULT 1,
                Enabled INTEGER DEFAULT 1,
                CreatedBy INTEGER,
                CreatedAt TEXT DEFAULT (datetime('now')),
                UpdatedAt TEXT
            );

            CREATE TABLE IF NOT EXISTS ReviewFlowTemplateNodes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TemplateId TEXT NOT NULL,
                Level INTEGER,
                -- 0=指定人（ReviewerId 有效） 1=团队管理员（上报时解析） 2=发起人自选（上报时请求 reviewers 提供）
                AssigneeType INTEGER DEFAULT 0,
                ReviewerId INTEGER
            );

            CREATE INDEX IF NOT EXISTS IX_ReviewFlowTemplates_Team ON ReviewFlowTemplates(TeamId);
            CREATE INDEX IF NOT EXISTS IX_ReviewFlowTemplateNodes_Template ON ReviewFlowTemplateNodes(TemplateId);
            """;
        cmd.ExecuteNonQuery();

        // 兼容性 ALTER TABLE：仅当列不存在时追加
        EnsureColumn(conn, "Users", "GroupId INTEGER");
        EnsureColumn(conn, "Users", "JobTitle TEXT");
        EnsureColumn(conn, "Users", "Permissions TEXT");
        EnsureColumn(conn, "Users", "Salt TEXT");
        EnsureColumn(conn, "Users", "Picture BLOB");
        EnsureColumn(conn, "Users", "Sex TEXT");
        EnsureColumn(conn, "Users", "Company TEXT");
        EnsureColumn(conn, "Users", "City TEXT");
        EnsureColumn(conn, "Users", "QQId TEXT");
        EnsureColumn(conn, "Users", "WechatId TEXT");
        // 阶段 9 Task 9.4：密码 Salt 与哈希算法版本
        EnsureColumn(conn, "Users", "PasswordSalt TEXT");
        EnsureColumn(conn, "Users", "PasswordHashAlgorithm INTEGER DEFAULT 0");
        // 阶段 1 Task 8：Users 扩展企业与登录统计字段
        EnsureColumn(conn, "Users", "EnterpriseId INTEGER");
        EnsureColumn(conn, "Users", "LastLoginAt TEXT");
        EnsureColumn(conn, "Users", "LoginCount INTEGER DEFAULT 0");
        // 管理面板：用户启用/禁用标记
        EnsureColumn(conn, "Users", "IsActive INTEGER DEFAULT 1");
        // 激活码注册控制 Task 2：用户绑定设备码，登录时校验防止一号多机
        EnsureColumn(conn, "Users", "MachineCode TEXT");

        EnsureColumn(conn, "Teams", "IsDeleted INTEGER DEFAULT 0");
        EnsureColumn(conn, "Teams", "DeletedAt TEXT");
        EnsureColumn(conn, "Teams", "Type INTEGER DEFAULT 0");
        EnsureColumn(conn, "Teams", "CreatorId INTEGER");
        // 阶段 1 Task 7：Teams 扩展配额与套餐字段
        EnsureColumn(conn, "Teams", "MaxUsers INTEGER DEFAULT 5");
        EnsureColumn(conn, "Teams", "MaxProjects INTEGER DEFAULT 10");
        EnsureColumn(conn, "Teams", "MaxTemplates INTEGER DEFAULT 5");
        EnsureColumn(conn, "Teams", "PlanType INTEGER DEFAULT 0");
        EnsureColumn(conn, "Teams", "CurrentTeamIsPayByProject INTEGER DEFAULT 1");
        EnsureColumn(conn, "Teams", "EnterpriseId INTEGER");

        EnsureColumn(conn, "Projects", "IsDeleted INTEGER DEFAULT 0");
        EnsureColumn(conn, "Projects", "DeletedAt TEXT");
        EnsureColumn(conn, "Projects", "IsTemplate INTEGER DEFAULT 0");
        EnsureColumn(conn, "Projects", "IsDemo INTEGER DEFAULT 0");
        EnsureColumn(conn, "Projects", "OperationId INTEGER DEFAULT 0");
        EnsureColumn(conn, "Projects", "TeamId TEXT");
        EnsureColumn(conn, "Projects", "CreatedBy INTEGER");
        // 阶段 1 Task 8：Projects 扩展计费与 License 字段
        EnsureColumn(conn, "Projects", "ProjectChargeType INTEGER DEFAULT 0");
        EnsureColumn(conn, "Projects", "ProjectLicenseDate TEXT");
        EnsureColumn(conn, "Projects", "TemplateVersion INTEGER DEFAULT 0");
        // 审核/归档工作流：Projects 上报审核状态与归档标记
        EnsureColumn(conn, "Projects", "ReviewStatus INTEGER DEFAULT 0");
        EnsureColumn(conn, "Projects", "IsArchived INTEGER DEFAULT 0");
        EnsureColumn(conn, "Projects", "ArchivedAt TEXT");
        // 审批流程模板：审核单留痕使用的模板
        EnsureColumn(conn, "ReviewSubmissions", "TemplateId TEXT");

        // 阶段 9 Task 9.4：Tokens 表新增过期/刷新时间字段
        EnsureColumn(conn, "Tokens", "ExpiresAt TEXT");
        EnsureColumn(conn, "Tokens", "LastRefreshAt TEXT");

        SeedDefaultData(conn);
        _logger.LogInformation("SQLite 初始化完成: {Conn}", _connectionString);
    }

    /// <summary>
    /// 兼容性 ALTER TABLE：仅当目标列不存在时追加。SQLite 不支持 IF NOT EXISTS 的 ADD COLUMN，故先查询 PRAGMA table_info。
    /// </summary>
    private static void EnsureColumn(SqliteConnection conn, string table, string columnDef)
    {
        var colName = columnDef.Split(' ')[0];
        using var check = conn.CreateCommand();
        check.CommandText = $"PRAGMA table_info({table})";
        var existing = new HashSet<string>();
        using var reader = check.ExecuteReader();
        while (reader.Read())
        {
            existing.Add(reader.GetString(1));
        }
        if (!existing.Contains(colName))
        {
            using var alter = conn.CreateCommand();
            alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {columnDef};";
            alter.ExecuteNonQuery();
        }
    }

    private void SeedDefaultData(SqliteConnection conn)
    {
        // 检查是否已存在默认用户
        var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Users WHERE UserName = 'admin'";
        var count = Convert.ToInt32(check.ExecuteScalar());
        if (count == 0)
        {
            var salt = PasswordHasher.GenerateSalt();
            var adminClientInput = HttpUtility.UrlEncode(Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("admin"))));
            var hashedPassword = PasswordHasher.HashPassword(adminClientInput, salt);

            var seed = conn.CreateCommand();
            seed.CommandText = """
                INSERT INTO Users (Id, UserName, Password, PasswordSalt, PasswordHashAlgorithm,
                                   Name, Role, TeamId, IsTeamAdmin, IsSystemAdmin, IsDataAdmin, LicenseDate,
                                   Phone, Email)
                VALUES (1, 'admin', @pwd, @salt, 1,
                        '管理员', 0, '00000000-0000-0000-0000-000000000001', 1, 1, 1, '2099-12-31',
                        '13800000000', 'admin@test.local');

                INSERT INTO Teams (Id, Name, Level, PayStatus, LicenseDate, OwnerUserId)
                VALUES ('00000000-0000-0000-0000-000000000001', '默认团队', 2, 1, '2099-12-31', 1);

                INSERT INTO UserTeams (UserId, TeamId)
                VALUES (1, '00000000-0000-0000-0000-000000000001');
                """;
            seed.Parameters.AddWithValue("@pwd", hashedPassword);
            seed.Parameters.AddWithValue("@salt", salt);
            seed.ExecuteNonQuery();
            _logger.LogInformation("已预置默认管理员账号 admin/admin");
        }

        // 为默认团队预置一条 License
        var licCheck = conn.CreateCommand();
        licCheck.CommandText = "SELECT COUNT(*) FROM Licenses WHERE OwnerType = 'Team' AND OwnerId = '00000000-0000-0000-0000-000000000001'";
        if (Convert.ToInt32(licCheck.ExecuteScalar()) == 0)
        {
            var licSeed = conn.CreateCommand();
            licSeed.CommandText = @"
                INSERT INTO Licenses (LicenseKey, ProductEdition, PlanType, Seats, MaxProjects, MaxTemplates,
                                      StartDate, EndDate, IsActive, OwnerType, OwnerId)
                VALUES ('SEED-0000-0000-0001', 2, 1, 100, 100, 100,
                        '2024-01-01 00:00:00', '2099-12-31 23:59:59', 1, 'Team',
                        '00000000-0000-0000-0000-000000000001');";
            licSeed.ExecuteNonQuery();
            _logger.LogInformation("已预置默认团队 License");
        }
    }

    public SqliteConnection CreateConnection() => new(_connectionString);
}
