﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Leqisoft.DTO;
using Leqisoft.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CS1998 // 反编译代码，原始 await 在反编译中丢失

namespace Leqisoft.LocalDataStore
{
    /// <summary>
    /// 本地数据存储统一入口
    /// 替换 WebApiClient 中的所有服务端调用
    /// </summary>
    public static class LocalDataStore
    {
        private static string _dbPath;
        private static string _projectDataPath;
        private static string _templatesPath;

        private static int SafeInt(object val, int defaultVal = 0)
            => val != DBNull.Value ? int.Parse(val.ToString()) : defaultVal;

        private static DateTime SafeDateTime(object val)
            => val != DBNull.Value ? DateTime.Parse(val.ToString()) : DateTime.MinValue;

        private static Guid SafeGuid(object val)
            => val != DBNull.Value ? Guid.Parse(val.ToString()) : Guid.Empty;

        // =============================================
        // 初始化
        // =============================================

        public static void Initialize(string dbPath, string projectDataPath)
        {
            _dbPath = dbPath;
            _projectDataPath = projectDataPath;
            _templatesPath = Path.Combine(Path.GetDirectoryName(dbPath), "Templates");

            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            Directory.CreateDirectory(projectDataPath);
            Directory.CreateDirectory(_templatesPath);

            // 初始化数据库
            DatabaseInitializer.Initialize(GetConnectionString());
            // 初始化数据字典
            DictionaryInitializer.EnsureDictionaryData(GetConnectionString());
            // 初始化本地用户/团队
            InitializeLocalUserAndTeam();
        }

        private static string GetConnectionString()
            => $"Data Source={_dbPath};Version=3;";

        private static SQLiteConnection CreateConnection()
        {
            var conn = new SQLiteConnection(GetConnectionString());
            conn.Open();
            return conn;
        }

        // =============================================
        // 用户/团队
        // =============================================

        private static void InitializeLocalUserAndTeam()
        {
            using var conn = CreateConnection();

            // 检查是否有用户
            var count = (long)new SQLiteCommand(
                "SELECT COUNT(*) FROM Users", conn).ExecuteScalar();

            if (count == 0)
            {
                var teamId = Guid.NewGuid().ToString();
                // 创建本地团队
                new SQLiteCommand(@"
                    INSERT INTO Teams (Id, Name, Type, CreatorId, LicenseDate)
                    VALUES (@Id, '本地团队', @Type, 1, datetime('now', '+10 years'))", conn)
                {
                    Parameters = {
                        new("@Id", teamId),
                        new("@Type", 0)
                    }
                }.ExecuteNonQuery();

                // 创建本地管理员用户
                new SQLiteCommand(@"
                    INSERT INTO Users (Id, UserName, Name, Phone, TelPhone, TeamId, IsTeamAdmin)
                    VALUES (1, 'admin', '管理员', '13800138000', '13800138000', @TeamId, 1)", conn)
                {
                    Parameters = { new("@TeamId", teamId) }
                }.ExecuteNonQuery();

                // 同步到内存模型
                Leqisoft.Model.User.Current = new Leqisoft.Model.User
                {
                    Id = 1,
                    UserName = "admin",
                    Name = "管理员",
                    TelPhone = "13800138000",
                    TeamId = Guid.Parse(teamId),
                    IsTeamAdmin = true
                };

                UserTeam.Current = new UserTeam
                {
                    Id = Guid.Parse(teamId),
                    Name = "本地团队",
                    Type = 0,
                    LicenseDate = DateTime.Now.AddYears(10),
                    Level = TeamLevel.Ultimate,
                    PayStatus = PayStatus.Payed
                };
                UserTeam.Teams = new List<UserTeam> { UserTeam.Current };
                UserTeam.CurrentTeamIsPayByProject = true;
            }
            else
            {
                // 恢复已有用户到内存模型
                using var cmd = new SQLiteCommand(
                    "SELECT * FROM Users WHERE Id = 1", conn);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Leqisoft.Model.User.Current = new Leqisoft.Model.User
                    {
                        Id = 1,
                        Name = reader["Name"]?.ToString(),
                        UserName = reader["UserName"]?.ToString(),
                        TelPhone = reader["TelPhone"]?.ToString(),
                        TeamId = reader["TeamId"] != DBNull.Value ? Guid.Parse(reader["TeamId"].ToString()) : Guid.Empty,
                        IsTeamAdmin = (long)reader["IsTeamAdmin"] == 1,
                        IsSystemSupporter = true
                    };

                    // 填充团队
                    using var teamCmd = new SQLiteCommand(
                        "SELECT * FROM Teams WHERE Id = @Id", conn);
                    teamCmd.Parameters.Add(new("@Id", reader["TeamId"]));
                    using var teamReader = teamCmd.ExecuteReader();
                    if (teamReader.Read())
                    {
                        UserTeam.Current = new UserTeam
                        {
                            Id = Guid.Parse(teamReader["Id"].ToString()),
                            Name = teamReader["Name"].ToString(),
                            Type = teamReader["Type"] != DBNull.Value ? int.Parse(teamReader["Type"].ToString()) : 0,
                            LicenseDate = teamReader["LicenseDate"] != DBNull.Value ? DateTime.Parse(teamReader["LicenseDate"].ToString()) : DateTime.MinValue,
                            Level = TeamLevel.Ultimate,
                            PayStatus = PayStatus.Payed
                        };
                        UserTeam.Teams = new List<UserTeam> { UserTeam.Current };
                        UserTeam.CurrentTeamIsPayByProject = true;
                    }
                }
            }
        }

        // =============================================
        // 用户资料 - 替换 WebApiClient.GetUserById/UpdateUserInfo/UpdatePhoneInfo
        // =============================================

        public static async Task<Leqisoft.DTO.User> GetUserById(long userId)
        {
            using var conn = CreateConnection();
            using var cmd = new SQLiteCommand(
                "SELECT * FROM Users WHERE Id = @Id", conn);
            cmd.Parameters.Add(new("@Id", userId));
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return new Leqisoft.DTO.User { Id = userId, Name = "管理员", UserName = "admin" };
            }
            return ReadUserFromReader(reader);
        }

        public static async Task UpdateUserInfo(Leqisoft.DTO.User userInfo)
        {
            using var conn = CreateConnection();
            new SQLiteCommand(@"
                UPDATE Users SET
                    UserName = @UserName,
                    Name = @Name,
                    Phone = @Phone,
                    Email = @Email,
                    City = @City,
                    Sex = @Sex,
                    Picture = @Picture,
                    UpdatedAt = datetime('now')
                WHERE Id = @Id", conn)
            {
                Parameters = {
                    new("@Id", userInfo.Id),
                    new("@UserName", userInfo.UserName ?? (object)DBNull.Value),
                    new("@Name", userInfo.Name ?? (object)DBNull.Value),
                    new("@Phone", userInfo.Phone ?? (object)DBNull.Value),
                    new("@Email", userInfo.Email ?? (object)DBNull.Value),
                    new("@City", userInfo.City ?? (object)DBNull.Value),
                    new("@Sex", userInfo.Sex ?? (object)DBNull.Value),
                    new("@Picture", userInfo.Picture ?? (object)DBNull.Value)
                }
            }.ExecuteNonQuery();

            // 同步到内存模型
            if (Leqisoft.Model.User.Current != null && Leqisoft.Model.User.Current.Id == userInfo.Id)
            {
                Leqisoft.Model.User.Current.Name = userInfo.Name;
                Leqisoft.Model.User.Current.UserName = userInfo.UserName;
            }
        }

        public static async Task UpdatePhoneInfo(Leqisoft.DTO.User userInfo, string validateCode)
        {
            // 本地模式不校验验证码，直接更新手机号
            await UpdateUserInfo(userInfo);
        }

        private static Leqisoft.DTO.User ReadUserFromReader(System.Data.SQLite.SQLiteDataReader reader)
        {
            return new Leqisoft.DTO.User
            {
                Id = SafeInt(reader["Id"]),
                UserName = reader["UserName"]?.ToString(),
                Name = reader["Name"]?.ToString(),
                Phone = reader["Phone"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                City = reader["City"]?.ToString(),
                Sex = reader["Sex"]?.ToString(),
                Picture = reader["Picture"] as byte[],
                TeamId = reader["TeamId"] != DBNull.Value ? Guid.Parse(reader["TeamId"].ToString()) : Guid.Empty,
                IsTeamAdmin = reader["IsTeamAdmin"] != DBNull.Value && (long)reader["IsTeamAdmin"] == 1
            };
        }

        // =============================================
        // 项目管理
        // =============================================

        public static async Task UpdateProject(Leqisoft.DTO.Project project)
        {
            using var conn = CreateConnection();

            // 先确保列存在
            EnsureProjectColumns(conn);

            new SQLiteCommand(@"
                UPDATE Projects SET
                    Name = @Name,
                    Number = @Number,
                    Category = @Category,
                    Auditee = @Auditee,
                    Note = @Note,
                    ParentId = @ParentId,
                    UpdatedAt = datetime('now')
                WHERE Id = @Id", conn)
            {
                Parameters = {
                    new("@Id", project.Id.ToString()),
                    new("@Name", project.Name ?? ""),
                    new("@Number", project.Number ?? ""),
                    new("@Category", project.Category ?? ""),
                    new("@Auditee", project.Auditee ?? ""),
                    new("@Note", project.Note ?? ""),
                    new("@ParentId", project.ParentId.HasValue ? project.ParentId.Value.ToString() : (object)DBNull.Value)
                }
            }.ExecuteNonQuery();

            // 更新项目成员
            if (project.Users != null)
            {
                // 先删除旧成员
                new SQLiteCommand("DELETE FROM ProjectMembers WHERE ProjectId = @Id", conn)
                {
                    Parameters = { new("@Id", project.Id.ToString()) }
                }.ExecuteNonQuery();

                // 添加新成员
                foreach (var user in project.Users)
                {
                    new SQLiteCommand(@"
                        INSERT OR IGNORE INTO ProjectMembers (ProjectId, UserId, Role)
                        VALUES (@ProjectId, @UserId, @Role)", conn)
                    {
                        Parameters = {
                            new("@ProjectId", project.Id.ToString()),
                            new("@UserId", user.Id.ToString()),
                            new("@Role", (int)user.Role)
                        }
                    }.ExecuteNonQuery();
                }
            }
        }

        public static async Task<IEnumerable<Leqisoft.DTO.Project>> GetProjects()
        {
            var projects = new List<Leqisoft.DTO.Project>();

            // 第一步：读取所有项目基本信息（使用独立连接，确保 Reader 关闭后再读成员）
            using (var conn = CreateConnection())
            {
                // 先确保列存在
                EnsureProjectColumns(conn);

                using var cmd = new SQLiteCommand(@"
                    SELECT p.Id, p.Name, p.Type, p.Version, p.TeamId, p.CreatedBy,
                        p.CreatedAt, p.IsDeleted, p.OperationId,
                        IFNULL(p.Number, '') as Number,
                        IFNULL(p.Category, '') as Category,
                        IFNULL(p.Auditee, '') as Auditee,
                        IFNULL(p.Note, '') as Note,
                        p.ParentId
                    FROM Projects p
                    WHERE p.IsDeleted = 0 AND p.Type = 0
                    ORDER BY p.CreatedAt DESC", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var projectId = SafeGuid(reader["Id"]);
                    var project = new Leqisoft.DTO.Project
                    {
                        Id = projectId,
                        Name = reader["Name"].ToString(),
                        Number = reader["Number"].ToString(),
                        Category = reader["Category"].ToString(),
                        Auditee = reader["Auditee"].ToString(),
                        Note = reader["Note"].ToString(),
                        Type = (ProjectType)SafeInt(reader["Type"]),
                        Version = SafeInt(reader["Version"]),
                        CreateTime = SafeDateTime(reader["CreatedAt"]),
                        ParentId = reader["ParentId"] != DBNull.Value ? (Guid?)SafeGuid(reader["ParentId"]) : null,
                        Creator = new Leqisoft.DTO.User
                        {
                            Id = 1,
                            Name = "管理员",
                            UserName = "admin",
                            Role = UserRole.Manager
                        }
                    };

                    projects.Add(project);
                }
            } // conn 和 reader 在此关闭

            // 第二步：逐个读取项目成员（使用独立连接，避免嵌套 DataReader）
            foreach (var project in projects)
            {
                project.Users = GetProjectMembers(project.Id);
            }

            return projects;
        }

        private static List<Leqisoft.DTO.User> GetProjectMembers(Guid projectId)
        {
            var users = new List<Leqisoft.DTO.User>();
            try
            {
                using var conn = CreateConnection();
                using var cmd = new SQLiteCommand(@"
                    SELECT pm.UserId, pm.Role, u.Name, u.UserName
                    FROM ProjectMembers pm
                    LEFT JOIN Users u ON u.Id = pm.UserId
                    WHERE pm.ProjectId = @ProjectId", conn);
                cmd.Parameters.Add(new("@ProjectId", projectId.ToString()));

                using var memberReader = cmd.ExecuteReader();
                while (memberReader.Read())
                {
                    users.Add(new Leqisoft.DTO.User
                    {
                        Id = long.Parse(memberReader["UserId"].ToString()),
                        Name = memberReader["Name"]?.ToString() ?? "管理员",
                        UserName = memberReader["UserName"]?.ToString() ?? "admin",
                        Role = (UserRole)SafeInt(memberReader["Role"])
                    });
                }
            }
            catch
            {
                // 如果 ProjectMembers 表不存在，返回默认管理员
                users.Add(new Leqisoft.DTO.User
                {
                    Id = 1,
                    Name = "管理员",
                    UserName = "admin",
                    Role = UserRole.Manager
                });
            }

            // 如果没有成员记录，确保至少有一个管理员（本地模式兜底）
            if (users.Count == 0)
            {
                users.Add(new Leqisoft.DTO.User
                {
                    Id = 1,
                    Name = "管理员",
                    UserName = "admin",
                    Role = UserRole.Manager
                });
            }

            return users;
        }

        private static void EnsureProjectColumns(SQLiteConnection conn)
        {
            // 查询 Projects 表现有的列
            var existingColumns = new HashSet<string>();
            using (var cmd = new SQLiteCommand("PRAGMA table_info(Projects)", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    existingColumns.Add(reader["name"].ToString());
                }
            }

            var alterStatements = new Dictionary<string, string> {
                { "Number", "TEXT DEFAULT ''" },
                { "Category", "TEXT DEFAULT ''" },
                { "Auditee", "TEXT DEFAULT ''" },
                { "Note", "TEXT DEFAULT ''" },
                { "ParentId", "TEXT DEFAULT NULL" },
                { "TeamId", "TEXT DEFAULT ''" }
            };

            foreach (var kv in alterStatements)
            {
                if (!existingColumns.Contains(kv.Key))
                {
                    new SQLiteCommand($"ALTER TABLE Projects ADD COLUMN {kv.Key} {kv.Value}", conn).ExecuteNonQuery();
                }
            }
        }

        public static async Task<IEnumerable<Leqisoft.DTO.Project>> GetTemplates()
        {
            var templates = new List<Leqisoft.DTO.Project>();
            if (!Directory.Exists(_templatesPath))
                return templates;

            foreach (var dbFile in Directory.GetFiles(_templatesPath, "*.db"))
            {
                var connStr = $"Data Source={dbFile};Version=3;Read Only=True;";
                try
                {
                    using var conn = new SQLiteConnection(connStr);
                    conn.Open();
                    using var cmd = new SQLiteCommand("SELECT * FROM Project", conn);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var template = new Leqisoft.DTO.Project
                        {
                            Id = SafeGuid(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Number = reader["Number"]?.ToString() ?? "",
                            Category = reader["Category"]?.ToString() ?? "",
                            Note = reader["Note"]?.ToString() ?? "",
                            Type = ProjectType.Template,
                            SystemBuild = true,
                            Version = SafeInt(reader["Version"]),
                            CreateTime = SafeDateTime(reader["CreateTime"]),
                            Creator = new Leqisoft.DTO.User
                            {
                                Id = 1,
                                Name = "管理员",
                                UserName = "admin",
                                Role = UserRole.Manager
                            }
                        };
                        templates.Add(template);
                    }
                }
                catch
                {
                    // 跳过无法读取的模板文件
                }
            }
            return templates;
        }

        /// <summary>
        /// 删除模板（本地模式：直接删除 Data\Templates 目录下对应的 .db 文件）
        /// </summary>
        public static async Task DeleteTemplate(Guid templateId)
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(_templatesPath))
                {
                    throw new DirectoryNotFoundException($"模板目录不存在: {_templatesPath}");
                }

                // 方式一：按文件名匹配（模板文件以 {Id}.db 命名）
                string expectedPath = Path.Combine(_templatesPath, $"{templateId}.db");
                if (File.Exists(expectedPath))
                {
                    File.Delete(expectedPath);
                    System.Diagnostics.Debug.WriteLine($"[DeleteTemplate] 已按文件名删除: {expectedPath}");
                    return;
                }

                // 方式二：遍历所有 .db 文件，匹配 Project.Id
                bool found = false;
                foreach (var dbFile in Directory.GetFiles(_templatesPath, "*.db"))
                {
                    try
                    {
                        Guid idFromDb = Guid.Empty;
                        var connStr = $"Data Source={dbFile};Version=3;Read Only=True;";
                        using (var conn = new SQLiteConnection(connStr))
                        {
                            conn.Open();
                            using var cmd = new SQLiteCommand("SELECT Id FROM Project LIMIT 1", conn);
                            using var reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                idFromDb = SafeGuid(reader["Id"]);
                            }
                        }

                        if (idFromDb == templateId)
                        {
                            found = true;
                            System.Diagnostics.Debug.WriteLine($"[DeleteTemplate] 找到模板文件: {dbFile}, Id={idFromDb}");
                            File.Delete(dbFile);
                            System.Diagnostics.Debug.WriteLine($"[DeleteTemplate] 模板文件已删除: {dbFile}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DeleteTemplate] 处理文件 {dbFile} 时出错: {ex.Message}");
                    }
                }

                if (!found)
                {
                    throw new FileNotFoundException($"未找到 Id={templateId} 对应的模板文件，已检查 {_templatesPath} 目录下所有 .db 文件");
                }
            });
        }

        /// <summary>
        /// 根据模板 Id 获取模板的 DTO 信息（本地模式：从 Data\Templates\*.db 读取）
        /// </summary>
        public static async Task<Leqisoft.DTO.Project> GetTemplateById(Guid templateId)
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(_templatesPath))
                    return null;

                foreach (var dbFile in Directory.GetFiles(_templatesPath, "*.db"))
                {
                    try
                    {
                        var connStr = $"Data Source={dbFile};Version=3;Read Only=True;";
                        using var conn = new SQLiteConnection(connStr);
                        conn.Open();
                        using var cmd = new SQLiteCommand("SELECT * FROM Project LIMIT 1", conn);
                        using var reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            var id = SafeGuid(reader["Id"]);
                            if (id == templateId)
                            {
                                return new Leqisoft.DTO.Project
                                {
                                    Id = id,
                                    Name = reader["Name"].ToString(),
                                    Number = reader["Number"]?.ToString() ?? "",
                                    Category = reader["Category"]?.ToString() ?? "",
                                    Note = reader["Note"]?.ToString() ?? "",
                                    Type = ProjectType.Template,
                                    SystemBuild = true,
                                    Version = SafeInt(reader["Version"]),
                                    CreateTime = SafeDateTime(reader["CreateTime"]),
                                    Creator = new Leqisoft.DTO.User
                                    {
                                        Id = 1,
                                        Name = "管理员",
                                        UserName = "admin",
                                        Role = UserRole.Manager
                                    }
                                };
                            }
                        }
                    }
                    catch
                    {
                        // 跳过无法读取的模板文件
                    }
                }
                return null;
            });
        }

        /// <summary>
        /// 获取模板 .db 文件路径（根据模板 Id）
        /// </summary>
        public static string GetTemplateDbPath(Guid templateId)
        {
            if (!Directory.Exists(_templatesPath))
                return null;

            foreach (var dbFile in Directory.GetFiles(_templatesPath, "*.db"))
            {
                try
                {
                    var connStr = $"Data Source={dbFile};Version=3;Read Only=True;";
                    using var conn = new SQLiteConnection(connStr);
                    conn.Open();
                    using var cmd = new SQLiteCommand("SELECT Id FROM Project LIMIT 1", conn);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var id = SafeGuid(reader["Id"]);
                        if (id == templateId)
                        {
                            return dbFile;
                        }
                    }
                }
                catch
                {
                    // 跳过无法读取的模板文件
                }
            }
            return null;
        }

        /// <summary>
        /// 更新模板信息（本地模式：更新 Data\Templates 下对应 .db 文件的 Project 表）
        /// </summary>
        public static async Task UpdateTemplate(Leqisoft.DTO.Project template)
        {
            await Task.Run(() =>
            {
                var dbFile = GetTemplateDbPath(template.Id);
                if (dbFile == null || !File.Exists(dbFile))
                    return;

                // 更新模板 .db 文件中的 Project 表
                var connStr = $"Data Source={dbFile};Version=3;";
                using var conn = new SQLiteConnection(connStr);
                conn.Open();
                new SQLiteCommand(@"
                    UPDATE Project SET
                        Name = @Name,
                        Number = @Number,
                        Category = @Category,
                        Note = @Note",
                    conn)
                {
                    Parameters = {
                        new("@Name", template.Name ?? ""),
                        new("@Number", template.Number ?? ""),
                        new("@Category", template.Category ?? ""),
                        new("@Note", template.Note ?? "")
                    }
                }.ExecuteNonQuery();
            });
        }

        /// <summary>
        /// 复制模板（本地模式：复制源模板 .db 文件到 Data\Templates，并更新 Project 表）
        /// </summary>
        public static async Task DuplicateTemplate(Guid sourceTemplateId, Leqisoft.DTO.Project newTemplate)
        {
            await Task.Run(() =>
            {
                var sourceDbPath = GetTemplateDbPath(sourceTemplateId);
                if (sourceDbPath == null || !File.Exists(sourceDbPath))
                    return;

                // 确保模板目录存在
                Directory.CreateDirectory(_templatesPath);

                // 目标文件路径（用新模板 Id 命名，避免与源文件重名）
                string destDbPath = Path.Combine(_templatesPath, $"{newTemplate.Id}.db");

                // 复制源模板文件
                File.Copy(sourceDbPath, destDbPath, overwrite: true);

                // 更新复制后的数据库中的 Project 表
                try
                {
                    var connStr = $"Data Source={destDbPath};Version=3;";
                    using var conn = new SQLiteConnection(connStr);
                    conn.Open();
                    new SQLiteCommand(@"
                        UPDATE Project SET
                            Id = @Id,
                            Name = @Name,
                            Number = @Number,
                            Category = @Category,
                            Note = @Note,
                            Version = 0,
                            CreateTime = @CreateTime",
                        conn)
                    {
                        Parameters = {
                            new("@Id", newTemplate.Id.ToString()),
                            new("@Name", newTemplate.Name ?? ""),
                            new("@Number", newTemplate.Number ?? ""),
                            new("@Category", newTemplate.Category ?? ""),
                            new("@Note", newTemplate.Note ?? ""),
                            new("@CreateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        }
                    }.ExecuteNonQuery();

                    // 重置同步状态
                    new SQLiteCommand(@"
                        UPDATE TreeNode SET Version=0, Dirty=0, ServerIndex=0;
                        UPDATE TreeGroup SET Dirty=0, ServerIndex=0;
                        UPDATE [Table] SET Dirty=0;
                        UPDATE Document SET Dirty=0;", conn).ExecuteNonQuery();
                }
                catch
                {
                    // 表可能不存在，忽略
                }
            });
        }

        /// <summary>
        /// 将项目另存为模板（本地模式：复制项目 .db 文件到 Data\Templates，并更新 Project 表）
        /// </summary>
        public static async Task SaveProjectAsTemplate(Guid sourceProjectId, Leqisoft.DTO.Project newTemplate)
        {
            await Task.Run(() =>
            {
                // 获取源项目 .db 文件路径
                long userId = Leqisoft.Model.User.Current?.Id ?? 1;
                string sourceDbPath = Path.Combine("data", userId.ToString(), $"{sourceProjectId}.db");
                if (!File.Exists(sourceDbPath))
                    return;

                // 确保模板目录存在
                Directory.CreateDirectory(_templatesPath);

                // 目标文件路径
                string destDbPath = Path.Combine(_templatesPath, $"{newTemplate.Id}.db");

                // 复制项目文件作为模板
                File.Copy(sourceDbPath, destDbPath, overwrite: true);

                // 更新复制后的数据库中的 Project 表
                try
                {
                    var connStr = $"Data Source={destDbPath};Version=3;";
                    using var conn = new SQLiteConnection(connStr);
                    conn.Open();
                    new SQLiteCommand(@"
                        UPDATE Project SET
                            Id = @Id,
                            Name = @Name,
                            Number = @Number,
                            Category = @Category,
                            Note = @Note,
                            Version = 0,
                            CreateTime = @CreateTime",
                        conn)
                    {
                        Parameters = {
                            new("@Id", newTemplate.Id.ToString()),
                            new("@Name", newTemplate.Name ?? ""),
                            new("@Number", newTemplate.Number ?? ""),
                            new("@Category", newTemplate.Category ?? ""),
                            new("@Note", newTemplate.Note ?? ""),
                            new("@CreateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        }
                    }.ExecuteNonQuery();

                    // 重置同步状态
                    new SQLiteCommand(@"
                        UPDATE TreeNode SET Version=0, Dirty=0, ServerIndex=0;
                        UPDATE TreeGroup SET Dirty=0, ServerIndex=0;
                        UPDATE [Table] SET Dirty=0;
                        UPDATE Document SET Dirty=0;", conn).ExecuteNonQuery();
                }
                catch
                {
                    // 表可能不存在，忽略
                }
            });
        }

        public static async Task CreateProject(Leqisoft.DTO.Project project)
        {
            using var conn = CreateConnection();

            // 先确保列存在
            EnsureProjectColumns(conn);

            using var tx = conn.BeginTransaction();

            try
            {
                new SQLiteCommand(@"
                    INSERT INTO Projects (Id, Name, Number, Category, Auditee, Note, Type, Version,
                        TeamId, ParentId, CreatedBy, CreatedAt)
                    VALUES (@Id, @Name, @Number, @Category, @Auditee, @Note, @Type, 1,
                        @TeamId, @ParentId, 1, @CreatedAt)", conn, tx)
                {
                    Parameters = {
                        new("@Id", project.Id.ToString()),
                        new("@Name", project.Name ?? ""),
                        new("@Number", project.Number ?? ""),
                        new("@Category", project.Category ?? ""),
                        new("@Auditee", project.Auditee ?? ""),
                        new("@Note", project.Note ?? ""),
                        new("@Type", (int)project.Type),
                        new("@TeamId", Leqisoft.Model.User.Current?.TeamId.ToString() ?? ""),
                        new("@ParentId", project.ParentId.HasValue ? project.ParentId.Value.ToString() : (object)DBNull.Value),
                        new("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    }
                }.ExecuteNonQuery();

                // 添加项目成员
                if (project.Users != null)
                {
                    foreach (var user in project.Users)
                    {
                        new SQLiteCommand(@"
                            INSERT OR IGNORE INTO ProjectMembers (ProjectId, UserId, Role)
                            VALUES (@ProjectId, @UserId, @Role)", conn, tx)
                        {
                            Parameters = {
                                new("@ProjectId", project.Id.ToString()),
                                new("@UserId", user.Id.ToString()),
                                new("@Role", (int)user.Role)
                            }
                        }.ExecuteNonQuery();
                    }
                }
                else
                {
                    // 没有成员则添加创建者
                    new SQLiteCommand(@"
                        INSERT INTO ProjectMembers (ProjectId, UserId, Role)
                        VALUES (@ProjectId, 1, 1)", conn, tx)
                    {
                        Parameters = { new("@ProjectId", project.Id.ToString()) }
                    }.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            // 创建项目/模板专属 .db 文件
            if (project.Type == ProjectType.Template)
            {
                // 模板：创建到 Data\Templates 目录
                CreateTemplateDbFile(project);
            }
            else
            {
                // 项目：创建到 data/{userId} 目录
                CreateProjectDbFile(project);
            }
        }

        /// <summary>
        /// 创建模板专属数据库文件（新建空模板，保存到 Data\Templates 目录）
        /// </summary>
        private static void CreateTemplateDbFile(Leqisoft.DTO.Project project)
        {
            Directory.CreateDirectory(_templatesPath);
            string templateDbPath = Path.Combine(_templatesPath, $"{project.Id}.db");

            // 创建空的模板数据库
            var dal = new ProjectDAL(templateDbPath);
            dal.SaveProject(new Leqisoft.DTO.Project
            {
                Id = project.Id,
                Name = project.Name,
                ParentId = project.Id,
                Version = 0,
                Number = project.Number ?? "",
                Category = project.Category ?? "",
                Note = project.Note ?? "",
                Auditee = project.Auditee ?? "",
                CreateTime = DateTime.Now
            });

            // 创建默认的侧边栏分组
            dal.SaveTreeGroups(new[]
            {
                new Leqisoft.DTO.TreeGroup
                {
                    Id = new Leqisoft.DTO.Id64(1),
                    Name = "工作底稿",
                    Index = 0,
                    Status = 0,
                    Dirty = 0,
                    ServerIndex = 0
                }
            });
        }

        /// <summary>
        /// 创建项目专属数据库文件，如果指定了模板则从模板复制
        /// </summary>
        private static void CreateProjectDbFile(Leqisoft.DTO.Project project)
        {
            // 确定项目数据库文件路径：与 GetDbPathByGuid 保持一致，使用 data/{userId}/{projectId}.db
            // GetDbPathByGuid 使用 Leqisoft.Model.User.Current.Id 作为子目录
            long userId = Leqisoft.Model.User.Current?.Id ?? 1;
            string userDir = Path.Combine("data", userId.ToString());
            Directory.CreateDirectory(userDir);
            string projectDbPath = Path.Combine(userDir, $"{project.Id}.db");

            // 如果指定了模板，从模板复制
            if (project.TemplateId.HasValue && project.TemplateId.Value != Guid.Empty)
            {
                string templateDbPath = FindTemplateDbFile(project.TemplateId.Value);
                if (templateDbPath != null && File.Exists(templateDbPath))
                {
                    File.Copy(templateDbPath, projectDbPath, overwrite: true);

                    // 更新复制后的数据库中的项目信息
                    var dal = new ProjectDAL(projectDbPath);
                    var projectDto = dal.GetProject();
                    if (projectDto != null)
                    {
                        projectDto.Id = project.Id;
                        projectDto.Name = project.Name;
                        projectDto.ParentId = project.Id;
                        projectDto.Version = 0;
                        projectDto.Number = project.Number ?? "";
                        projectDto.Category = project.Category ?? "";
                        projectDto.Note = project.Note ?? "";
                        projectDto.Auditee = project.Auditee ?? "";
                        projectDto.CreateTime = DateTime.Now;
                        dal.SaveProject(projectDto);
                    }

                    // 重置模板中的同步状态字段，避免新项目误认为有未同步数据
                    try
                    {
                        using var conn = new SQLiteConnection($"Data Source={projectDbPath};Version=3;");
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = @"
                            UPDATE TreeNode SET Version=0, Dirty=0, ServerIndex=0;
                            UPDATE TreeGroup SET Dirty=0, ServerIndex=0;
                            UPDATE [Table] SET Dirty=0;
                            UPDATE Document SET Dirty=0;";
                        cmd.ExecuteNonQuery();
                    }
                    catch { /* 表可能不存在，忽略 */ }

                    return;
                }
                // 模板文件未找到，降级为空项目（不静默，但继续创建空项目）
            }

            // 没有模板或模板文件不存在，创建空的项目数据库
            var emptyDal = new ProjectDAL(projectDbPath);
            emptyDal.SaveProject(new Leqisoft.DTO.Project
            {
                Id = project.Id,
                Name = project.Name,
                ParentId = project.Id,
                Version = 0,
                Number = project.Number ?? "",
                Category = project.Category ?? "",
                Note = project.Note ?? "",
                Auditee = project.Auditee ?? "",
                CreateTime = DateTime.Now
            });

            // 创建默认的侧边栏分组
            emptyDal.SaveTreeGroups(new[]
            {
                new Leqisoft.DTO.TreeGroup
                {
                    Id = new Leqisoft.DTO.Id64(1),
                    Name = "工作底稿",
                    Index = 0,
                    Status = 0,
                    Dirty = 0,
                    ServerIndex = 0
                }
            });

            // 创建默认的文档节点
            long nextId = 2; // TreeGroup 用了 1，从 2 开始
            var defaultDocNodeId = new Leqisoft.DTO.Id64(nextId++);
            emptyDal.SaveTreeNodes(new[]
            {
                new Leqisoft.DTO.TreeNode
                {
                    Id = defaultDocNodeId,
                    GroupId = new Leqisoft.DTO.Id64(1),
                    ParentId = Leqisoft.DTO.Id64.Zero,
                    Name = "审计报告",
                    Type = 2, // 文档类型
                    Status = 0,
                    Dirty = 0,
                    Index = 0,
                    Version = 0,
                    ServerIndex = 0
                }
            });

            // 为默认文档节点创建 Document 记录
            emptyDal.SaveDocument(new Leqisoft.DTO.Document
            {
                Id = defaultDocNodeId,
                Version = 0,
                Locker = 0,
                SectPr = Leqisoft.Model.Properties.Resource.DefaultSectPr,
                MergeTable = Leqisoft.DTO.Id64.Zero,
                Dirty = 0
            });
        }

        /// <summary>
        /// 根据模板Id查找对应的 .db 文件
        /// </summary>
        private static string FindTemplateDbFile(Guid templateId)
        {
            if (!Directory.Exists(_templatesPath))
                return null;

            foreach (var dbFile in Directory.GetFiles(_templatesPath, "*.db"))
            {
                try
                {
                    var connStr = $"Data Source={dbFile};Version=3;Read Only=True;";
                    using var conn = new SQLiteConnection(connStr);
                    conn.Open();
                    using var cmd = new SQLiteCommand("SELECT Id FROM Project LIMIT 1", conn);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var id = SafeGuid(reader["Id"]);
                        if (id == templateId)
                            return dbFile;
                    }
                }
                catch
                {
                    // 跳过无法读取的模板文件
                }
            }
            return null;
        }

        public static async Task<Tuple<int, int>> OpenProject(Guid projectId)
        {
            using var conn = CreateConnection();

            // 获取当前 OperationId
            using var cmd = new SQLiteCommand(
                "SELECT OperationId, Version FROM Projects WHERE Id = @Id", conn);
            cmd.Parameters.Add(new("@Id", projectId.ToString()));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int operationId = SafeInt(reader["OperationId"]);
                int version = SafeInt(reader["Version"]);

                // 递增 OperationId
                new SQLiteCommand(
                    "UPDATE Projects SET OperationId = OperationId + 1 WHERE Id = @Id", conn)
                {
                    Parameters = { new("@Id", projectId.ToString()) }
                }.ExecuteNonQuery();

                return Tuple.Create(operationId, version);
            }
            return Tuple.Create(0, 0);
        }

        /// <summary>
        /// 删除项目（移入回收站：标记 IsDeleted）
        /// </summary>
        public static async Task DeleteProject(Guid projectId)
        {
            using var conn = CreateConnection();

            // 标记项目为已删除
            new SQLiteCommand(
                "UPDATE Projects SET IsDeleted = 1, DeletedAt = @DeletedAt WHERE Id = @Id", conn)
            {
                Parameters = {
                    new("@Id", projectId.ToString()),
                    new("@DeletedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                }
            }.ExecuteNonQuery();
        }

        /// <summary>
        /// 从服务器彻底删除项目（本地模式：从数据库删除记录并删除项目 .db 文件）
        /// </summary>
        public static async Task DeleteProjectFromServer(JObject jobj)
        {
            var ids = jobj["Ids"].ToObject<List<Guid>>();

            using var conn = CreateConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                foreach (var projectId in ids)
                {
                    // 删除项目成员
                    new SQLiteCommand("DELETE FROM ProjectMembers WHERE ProjectId = @Id", conn, tx)
                    {
                        Parameters = { new("@Id", projectId.ToString()) }
                    }.ExecuteNonQuery();

                    // 删除项目文件记录
                    new SQLiteCommand("DELETE FROM ProjectFiles WHERE ProjectId = @Id", conn, tx)
                    {
                        Parameters = { new("@Id", projectId.ToString()) }
                    }.ExecuteNonQuery();

                    // 删除项目文档记录
                    new SQLiteCommand("DELETE FROM Documents WHERE ProjectId = @Id", conn, tx)
                    {
                        Parameters = { new("@Id", projectId.ToString()) }
                    }.ExecuteNonQuery();

                    // 删除项目记录
                    new SQLiteCommand("DELETE FROM Projects WHERE Id = @Id", conn, tx)
                    {
                        Parameters = { new("@Id", projectId.ToString()) }
                    }.ExecuteNonQuery();

                    // 删除项目专属 .db 文件
                    long userId = Leqisoft.Model.User.Current?.Id ?? 1;
                    string projectDbPath = Path.Combine("data", userId.ToString(), $"{projectId}.db");
                    if (File.Exists(projectDbPath))
                    {
                        try { File.Delete(projectDbPath); } catch { }
                    }
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 获取回收站中的项目列表
        /// </summary>
        public static async Task<IEnumerable<Leqisoft.DTO.Project>> GetRecycleProjects()
        {
            using var conn = CreateConnection();
            var projects = new List<Leqisoft.DTO.Project>();

            using var cmd = new SQLiteCommand(@"
                SELECT p.*, GROUP_CONCAT(pm.UserId) as MemberIds
                FROM Projects p
                LEFT JOIN ProjectMembers pm ON pm.ProjectId = p.Id
                WHERE p.IsDeleted = 1
                GROUP BY p.Id
                ORDER BY p.DeletedAt DESC", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var project = new Leqisoft.DTO.Project
                {
                    Id = SafeGuid(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Number = reader["Number"] != DBNull.Value ? reader["Number"].ToString() : "",
                    Category = reader["Category"] != DBNull.Value ? reader["Category"].ToString() : "",
                    Auditee = reader["Auditee"] != DBNull.Value ? reader["Auditee"].ToString() : "",
                    Type = (ProjectType)SafeInt(reader["Type"]),
                    Version = SafeInt(reader["Version"]),
                    CreateTime = SafeDateTime(reader["CreatedAt"]),
                    Creator = new Leqisoft.DTO.User
                    {
                        Id = 1,
                        Name = "管理员",
                        UserName = "admin",
                        Role = UserRole.Manager
                    },
                    Users = new List<Leqisoft.DTO.User>
                    {
                        new Leqisoft.DTO.User
                        {
                            Id = 1,
                            Name = "管理员",
                            UserName = "admin",
                            Role = UserRole.Manager
                        }
                    }
                };

                projects.Add(project);
            }
            return projects;
        }

        /// <summary>
        /// 恢复回收站中的项目
        /// </summary>
        public static async Task RestoreProjects(JObject jobj)
        {
            var ids = jobj["Ids"].ToObject<List<Guid>>();

            using var conn = CreateConnection();
            foreach (var projectId in ids)
            {
                new SQLiteCommand(
                    "UPDATE Projects SET IsDeleted = 0, DeletedAt = NULL WHERE Id = @Id", conn)
                {
                    Parameters = { new("@Id", projectId.ToString()) }
                }.ExecuteNonQuery();
            }
        }

        // =============================================
        // 推送表格数据 - 替换 WebApiClient.PushTable()
        // =============================================

        public static async Task<JObject> PushTable(PushTable request)
        {
            using var conn = CreateConnection();
            string tableId = request.Id.ToString();
            string projectId = new Guid(request.ProjectId.ToByteArray()).ToString();

            // 序列化整个 PushTable 到 BLOB
            byte[] tableData = request.ToByteArray();

            using var tx = conn.BeginTransaction();

            // 检查是否存在
            var exists = new SQLiteCommand(
                "SELECT COUNT(*) FROM TableSchemas WHERE Id = @Id AND ProjectId = @PId", conn, tx);
            exists.Parameters.Add(new("@Id", tableId));
            exists.Parameters.Add(new("@PId", projectId));
            bool isNew = (long)exists.ExecuteScalar() == 0;

            if (isNew)
            {
                new SQLiteCommand(@"
                    INSERT INTO TableSchemas (Id, ProjectId, OperationId, Name, Version,
                        ColumnsData, RowsData, CellsData, CellStylesData, MergesData, CellPropsData)
                    VALUES (@Id, @PId, @OpId, @Name, @Version,
                        @Data, @Data, @Data, @Data, @Data, @Data)", conn, tx)
                {
                    Parameters = {
                        new("@Id", tableId),
                        new("@PId", projectId),
                        new("@OpId", request.Mask),
                        new("@Name", ""),
                        new("@Version", request.Version),
                        new("@Data", tableData)
                    }
                }.ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand(@"
                    UPDATE TableSchemas SET
                        Version = @Version,
                        ColumnsData = @Data,
                        RowsData = @Data,
                        CellsData = @Data,
                        CellStylesData = @Data,
                        MergesData = @Data,
                        CellPropsData = @Data,
                        UpdatedAt = datetime('now')
                    WHERE Id = @Id AND ProjectId = @PId", conn, tx)
                {
                    Parameters = {
                        new("@Id", tableId),
                        new("@PId", projectId),
                        new("@Version", request.Version),
                        new("@Data", tableData)
                    }
                }.ExecuteNonQuery();
            }

            // 更新项目版本号
            new SQLiteCommand(
                "UPDATE Projects SET Version = @Version, UpdatedAt = datetime('now') WHERE Id = @PId", conn, tx)
            {
                Parameters = {
                    new("@Version", request.Version),
                    new("@PId", projectId)
                }
            }.ExecuteNonQuery();

            // 记录版本历史
            new SQLiteCommand(@"
                INSERT INTO VersionHistory (ProjectId, TargetId, TargetType, Version, ChangeType, Snapshot)
                VALUES (@PId, @TId, 'Table', @Version, 'Push', @Snapshot)", conn, tx)
            {
                Parameters = {
                    new("@PId", projectId),
                    new("@TId", tableId),
                    new("@Version", request.Version),
                    new("@Snapshot", tableData)
                }
            }.ExecuteNonQuery();

            tx.Commit();
            return new JObject { ["Result"] = "ok" };
        }

        // =============================================
        // 拉取表格数据 - 替换 WebApiClient.PullTable()
        // =============================================

        public static async Task<PullTable> PullTable(JObject request)
        {
            string projectId = request["ProjectId"]?.ToString();
            string tableId = request["TableId"]?.ToString();
            int clientVersion = (int)(request["Version"] ?? 0);

            using var conn = CreateConnection();
            using var cmd = new SQLiteCommand(@"
                SELECT * FROM TableSchemas
                WHERE Id = @Id AND ProjectId = @PId", conn);
            cmd.Parameters.Add(new("@Id", tableId));
            cmd.Parameters.Add(new("@PId", projectId));

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return new PullTable { Result = "NoUpdate" };
            }

            int serverVersion = SafeInt(reader["Version"]);
            if (serverVersion <= clientVersion)
            {
                return new PullTable { Result = "NoUpdate" };
            }

            var pullTable = new PullTable
            {
                Result = "NeedUpdate",
                Version = serverVersion
            };

            // 从 BLOB 反序列化整个 PushTable，再转换成 PullTable 的增量格式
            byte[] tableData = reader["ColumnsData"] as byte[];
            if (tableData != null && tableData.Length > 0)
            {
                var pushTable = Leqisoft.DTO.PushTable.Parser.ParseFrom(tableData);

                // 将 Push* 类型序列化后按 Pull* 类型反序列化（Protobuf 线格式兼容）
                foreach (var col in pushTable.Columns)
                {
                    pullTable.NewColumns.Add(PullColumn.Parser.ParseFrom(col.ToByteArray()));
                }
                foreach (var row in pushTable.Rows)
                {
                    pullTable.NewRows.Add(PullRow.Parser.ParseFrom(row.ToByteArray()));
                }
                foreach (var cell in pushTable.Cells)
                {
                    pullTable.NewCells.Add(PullCell.Parser.ParseFrom(cell.ToByteArray()));
                }
                foreach (var style in pushTable.CellStyles)
                {
                    pullTable.CellStyles.Add(PullCellStyle.Parser.ParseFrom(style.ToByteArray()));
                }
                foreach (var merge in pushTable.Merges)
                {
                    pullTable.NewMerges.Add(PullMerge.Parser.ParseFrom(merge.ToByteArray()));
                }
            }

            return pullTable;
        }

        // =============================================
        // 推送文档数据 - 替换 WebApiClient.PushDocument()
        // =============================================

        public static async Task<JObject> PushDocument(PushDocument request)
        {
            using var conn = CreateConnection();
            string docId = request.Id.ToString();
            string projectId = new Guid(request.ProjectId.ToByteArray()).ToString();

            // 序列化整个 PushDocument 到 BLOB
            byte[] docData = request.ToByteArray();

            using var tx = conn.BeginTransaction();

            var exists = new SQLiteCommand(
                "SELECT COUNT(*) FROM Documents WHERE Id = @Id AND ProjectId = @PId", conn, tx);
            exists.Parameters.Add(new("@Id", docId));
            exists.Parameters.Add(new("@PId", projectId));
            bool isNew = (long)exists.ExecuteScalar() == 0;

            if (isNew)
            {
                new SQLiteCommand(@"
                    INSERT INTO Documents (Id, ProjectId, Name, Version, ParagraphsData)
                    VALUES (@Id, @PId, @Name, @Version, @Data)", conn, tx)
                {
                    Parameters = {
                        new("@Id", docId),
                        new("@PId", projectId),
                        new("@Name", ""),
                        new("@Version", request.Version),
                        new("@Data", docData)
                    }
                }.ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand(@"
                    UPDATE Documents SET Version = @Version, ParagraphsData = @Data,
                        UpdatedAt = datetime('now')
                    WHERE Id = @Id AND ProjectId = @PId", conn, tx)
                {
                    Parameters = {
                        new("@Id", docId),
                        new("@PId", projectId),
                        new("@Version", request.Version),
                        new("@Data", docData)
                    }
                }.ExecuteNonQuery();
            }

            tx.Commit();
            return new JObject { ["Result"] = "ok" };
        }

        // =============================================
        // 拉取文档数据 - 替换 WebApiClient.PullDocument()
        // =============================================

        public static async Task<PullDocument> PullDocument(JObject request)
        {
            string projectId = request["ProjectId"]?.ToString();
            string docId = request["DocId"]?.ToString();
            int clientVersion = (int)(request["Version"] ?? 0);

            using var conn = CreateConnection();
            using var cmd = new SQLiteCommand(
                "SELECT * FROM Documents WHERE Id = @Id AND ProjectId = @PId", conn);
            cmd.Parameters.Add(new("@Id", docId));
            cmd.Parameters.Add(new("@PId", projectId));

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return new PullDocument { Result = "NoUpdate" };
            }

            int serverVersion = SafeInt(reader["Version"]);
            if (serverVersion <= clientVersion)
            {
                return new PullDocument { Result = "NoUpdate" };
            }

            var pullDoc = new PullDocument
            {
                Result = "NeedUpdate",
                Version = serverVersion
            };

            // 从 BLOB 反序列化整个 PushDocument
            byte[] data = reader["ParagraphsData"] as byte[];
            if (data != null && data.Length > 0)
            {
                var pushDoc = Leqisoft.DTO.PushDocument.Parser.ParseFrom(data);

                // 将 PushParagraph 转换为 PullParagraph
                foreach (var para in pushDoc.Paragraphs)
                {
                    pullDoc.NewParagraphs.Add(
                        PullParagraph.Parser.ParseFrom(para.ToByteArray()));
                }
            }

            return pullDoc;
        }

        // =============================================
        // 数据字典 - 替换 WebApiClient.TableCollectDic() 等
        // =============================================

        public static async Task<JObject> GetTableCollectDic(int version = 0)
        {
            using var conn = CreateConnection();
            using var cmd = new SQLiteCommand(
                "SELECT Data FROM DataDictionary WHERE DicType = 'TableCollect'", conn);
            var result = cmd.ExecuteScalar()?.ToString();
            return string.IsNullOrEmpty(result)
                ? new JObject()
                : JObject.Parse(result);
        }

        public static async Task<JObject> GetCellCollectDic(int version = 0)
        {
            using var conn = CreateConnection();
            using var cmd = new SQLiteCommand(
                "SELECT Data FROM DataDictionary WHERE DicType = 'CellCollect'", conn);
            var result = cmd.ExecuteScalar()?.ToString();
            return string.IsNullOrEmpty(result)
                ? new JObject()
                : JObject.Parse(result);
        }

        public static async Task<JObject> GetLedgerValidateDic(int version = 0)
        {
            using var conn = CreateConnection();
            using var cmd = new SQLiteCommand(
                "SELECT Data FROM DataDictionary WHERE DicType = 'LedgerValidate'", conn);
            var result = cmd.ExecuteScalar()?.ToString();
            return string.IsNullOrEmpty(result)
                ? new JObject()
                : JObject.Parse(result);
        }

        // =============================================
        // 团队成员 - 替换 WebApiClient.GetTeamUsersWithPic()
        // =============================================

        public static async Task<IEnumerable<Leqisoft.DTO.User>> GetTeamUsersWithPic()
        {
            using var conn = CreateConnection();
            var users = new List<Leqisoft.DTO.User>();
            using var cmd = new SQLiteCommand(
                "SELECT * FROM Users WHERE IsDeleted = 0", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new Leqisoft.DTO.User
                {
                    Id = long.Parse(reader["Id"].ToString()), // Id 是 INTEGER PK，不会 null
                    UserName = reader["UserName"].ToString(),
                    Name = reader["Name"]?.ToString(),
                    Phone = reader["Phone"]?.ToString(),
                    TeamId = SafeGuid(reader["TeamId"]),
                    IsTeamAdmin = (long)reader["IsTeamAdmin"] == 1
                });
            }
            return users;
        }

        // =============================================
        // 团队列表 - 替换 WebApiClient.GetUserTeams()
        // =============================================

        public static async Task<JObject> GetUserTeams()
        {
            using var conn = CreateConnection();
            var teams = new List<object>();
            using var cmd = new SQLiteCommand(
                "SELECT * FROM Teams WHERE IsDeleted = 0", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                teams.Add(new
                {
                    Id = reader["Id"].ToString(),
                    Name = reader["Name"].ToString(),
                    Type = SafeInt(reader["Type"]),
                    LicenseDate = reader["LicenseDate"]?.ToString()
                });
            }
            return new JObject { ["teams"] = JArray.FromObject(teams) };
        }

        // =============================================
        // 文件上传/下载 - 替换 WebApiClient.UploadFile/DownloadFile()
        // =============================================

        public static async Task UploadFile(Guid fileId, Stream fileStream)
        {
            string fileDir = Path.Combine(_projectDataPath, "files");
            Directory.CreateDirectory(fileDir);
            string localPath = Path.Combine(fileDir, fileId.ToString());

            using var fs = new FileStream(localPath, FileMode.Create);
            await fileStream.CopyToAsync(fs);
        }

        public static async Task<Stream> DownloadFile(Guid fileId)
        {
            string localPath = Path.Combine(_projectDataPath, "files", fileId.ToString());
            if (!File.Exists(localPath))
                return null;
            return new FileStream(localPath, FileMode.Open, FileAccess.Read);
        }
    }
}