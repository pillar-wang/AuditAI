﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Auditai.DTO;
using Google.Protobuf;
using Newtonsoft.Json;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 测试夹具加载器
    /// 从 TestFixtures\ 目录加载种子数据（用户/团队/项目）和 Protobuf 样本字节
    /// </summary>
    public static class TestFixtures
    {
        private static readonly string FixturesDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFixtures");

        private static IReadOnlyList<TestUser> _users = new List<TestUser>();
        private static IReadOnlyList<TestTeam> _teams = new List<TestTeam>();
        private static IReadOnlyList<TestProject> _projects = new List<TestProject>();

        /// <summary>种子用户列表（admin/testuser1/testuser2）</summary>
        public static IReadOnlyList<TestUser> Users
        {
            get { return _users; }
            private set { _users = value; }
        }

        /// <summary>种子团队列表（测试团队A/B）</summary>
        public static IReadOnlyList<TestTeam> Teams
        {
            get { return _teams; }
            private set { _teams = value; }
        }

        /// <summary>种子项目列表（2024/2025 审计项目）</summary>
        public static IReadOnlyList<TestProject> Projects
        {
            get { return _projects; }
            private set { _projects = value; }
        }

        /// <summary>
        /// 加载所有夹具：3 个 JSON + 2 个 .bin（.bin 缺失时通过 Protobuf 生成）。
        /// 加载失败不抛异常，仅输出错误日志并将集合设为空列表。
        /// </summary>
        public static void Load()
        {
            var emptyUsers = new List<TestUser>();
            var emptyTeams = new List<TestTeam>();
            var emptyProjects = new List<TestProject>();

            try
            {
                if (!Directory.Exists(FixturesDir))
                {
                    Console.Error.WriteLine("[TestFixtures] 目录不存在: " + FixturesDir);
                    _users = emptyUsers;
                    _teams = emptyTeams;
                    _projects = emptyProjects;
                    return;
                }

                _users = LoadUsers();
                _teams = LoadTeams();
                _projects = LoadProjects();

                EnsureSamplePushTable();
                EnsureSamplePushDocument();

                Console.Error.WriteLine(
                    "[TestFixtures] 加载完成: Users=" + _users.Count
                    + " Teams=" + _teams.Count
                    + " Projects=" + _projects.Count);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[TestFixtures] Load 失败: " + ex);
                _users = emptyUsers;
                _teams = emptyTeams;
                _projects = emptyProjects;
            }
        }

        /// <summary>
        /// 按 name 返回 .bin 字节；自动追加 .bin 后缀。
        /// 示例 name: "sample_push_table"、"sample_push_document"。
        /// 文件不存在返回 null。
        /// </summary>
        public static byte[] GetFixtureBytes(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            try
            {
                string path = Path.Combine(FixturesDir, name + ".bin");
                if (!File.Exists(path))
                    return null;
                return File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[TestFixtures] GetFixtureBytes 失败: " + name + " -> " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 按 name 返回 .json/.txt 文本；自动追加 .json 后缀。
        /// 示例 name: "seed_users"、"seed_teams"、"seed_projects"。
        /// 文件不存在返回 null。
        /// </summary>
        public static string GetFixtureText(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            try
            {
                string path = Path.Combine(FixturesDir, name + ".json");
                if (!File.Exists(path))
                    return null;
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[TestFixtures] GetFixtureText 失败: " + name + " -> " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 按 userName 查找种子用户；不存在返回 null。
        /// </summary>
        public static TestUser GetUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return null;
            for (int i = 0; i < _users.Count; i++)
            {
                if (string.Equals(_users[i].UserName, userName, StringComparison.OrdinalIgnoreCase))
                    return _users[i];
            }
            return null;
        }

        /// <summary>
        /// 按 name 查找种子项目；不存在返回 null。
        /// </summary>
        public static TestProject GetProject(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            for (int i = 0; i < _projects.Count; i++)
            {
                if (string.Equals(_projects[i].Name, name, StringComparison.Ordinal))
                    return _projects[i];
            }
            return null;
        }

        /// <summary>
        /// 按 name 查找种子团队；不存在返回 null。
        /// </summary>
        public static TestTeam GetTeam(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            for (int i = 0; i < _teams.Count; i++)
            {
                if (string.Equals(_teams[i].Name, name, StringComparison.Ordinal))
                    return _teams[i];
            }
            return null;
        }

        // ===== 内部加载逻辑 =====

        private static IReadOnlyList<TestUser> LoadUsers()
        {
            string json = GetFixtureText("seed_users");
            if (string.IsNullOrEmpty(json))
                return new List<TestUser>();
            var wrapper = JsonConvert.DeserializeObject<UsersWrapper>(json);
            if (wrapper == null || wrapper.Users == null)
                return new List<TestUser>();
            return wrapper.Users;
        }

        private static IReadOnlyList<TestTeam> LoadTeams()
        {
            string json = GetFixtureText("seed_teams");
            if (string.IsNullOrEmpty(json))
                return new List<TestTeam>();
            var wrapper = JsonConvert.DeserializeObject<TeamsWrapper>(json);
            if (wrapper == null || wrapper.Teams == null)
                return new List<TestTeam>();
            return wrapper.Teams;
        }

        private static IReadOnlyList<TestProject> LoadProjects()
        {
            string json = GetFixtureText("seed_projects");
            if (string.IsNullOrEmpty(json))
                return new List<TestProject>();
            var wrapper = JsonConvert.DeserializeObject<ProjectsWrapper>(json);
            if (wrapper == null || wrapper.Projects == null)
                return new List<TestProject>();
            return wrapper.Projects;
        }

        // ===== Protobuf 样本生成 =====

        /// <summary>
        /// 若 sample_push_table.bin 不存在，则使用 Google.Protobuf 序列化一个 3 列 5 行的样本表格。
        /// 列：科目 / 金额 / 备注；行：现金/银行/应收/存货/固定资产。
        /// </summary>
        private static void EnsureSamplePushTable()
        {
            string path = Path.Combine(FixturesDir, "sample_push_table.bin");
            try
            {
                if (File.Exists(path))
                    return;
                byte[] bytes = GenerateSamplePushTable();
                if (bytes == null || bytes.Length == 0)
                {
                    Console.Error.WriteLine("[TestFixtures] GenerateSamplePushTable 返回空字节");
                    return;
                }
                File.WriteAllBytes(path, bytes);
                Console.Error.WriteLine("[TestFixtures] 已生成 sample_push_table.bin (" + bytes.Length + " bytes)");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[TestFixtures] 生成 sample_push_table.bin 失败: " + ex);
            }
        }

        /// <summary>
        /// 若 sample_push_document.bin 不存在，则使用 Google.Protobuf 序列化一个 3 段落样本文档。
        /// 段落：审计报告 / 审计意见描述 / 无保留意见结论。
        /// </summary>
        private static void EnsureSamplePushDocument()
        {
            string path = Path.Combine(FixturesDir, "sample_push_document.bin");
            try
            {
                if (File.Exists(path))
                    return;
                byte[] bytes = GenerateSamplePushDocument();
                if (bytes == null || bytes.Length == 0)
                {
                    Console.Error.WriteLine("[TestFixtures] GenerateSamplePushDocument 返回空字节");
                    return;
                }
                File.WriteAllBytes(path, bytes);
                Console.Error.WriteLine("[TestFixtures] 已生成 sample_push_document.bin (" + bytes.Length + " bytes)");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[TestFixtures] 生成 sample_push_document.bin 失败: " + ex);
            }
        }

        /// <summary>
        /// 生成 PushTable Protobuf 样本字节（3 列 5 行）。
        /// 字段对应服务端 Auditai.DTO.PushTable：
        ///   Columns (field 5) → PushColumn
        ///   Rows    (field 6) → PushRow
        ///   Cells   (field 7) → PushCell（CId=列 Id, RId=行 Id, Value=UTF-8 字节）
        /// </summary>
        internal static byte[] GenerateSamplePushTable()
        {
            // 列定义：科目 / 金额 / 备注
            long colId1 = 1001L;
            long colId2 = 1002L;
            long colId3 = 1003L;

            var col1 = new PushColumn
            {
                Id = colId1,
                Action = 1, // New
                Width = 120,
                Index = 0,
                Caption = "科目",
                Visible = true
            };
            var col2 = new PushColumn
            {
                Id = colId2,
                Action = 1,
                Width = 100,
                Index = 1,
                Caption = "金额",
                Visible = true
            };
            var col3 = new PushColumn
            {
                Id = colId3,
                Action = 1,
                Width = 160,
                Index = 2,
                Caption = "备注",
                Visible = true
            };

            // 行定义：5 行数据
            long[] rowIds = new long[] { 2001L, 2002L, 2003L, 2004L, 2005L };
            var rows = new PushRow[5];
            for (int i = 0; i < rowIds.Length; i++)
            {
                rows[i] = new PushRow
                {
                    Id = rowIds[i],
                    Action = 1, // New
                    Height = 24,
                    Index = i,
                    Visible = true
                };
            }

            // 单元格：5 行 × 3 列 = 15 个
            string[][] data = new string[][]
            {
                new string[] { "现金", "1000", "期初" },
                new string[] { "银行", "2000", "" },
                new string[] { "应收", "3000", "坏账" },
                new string[] { "存货", "4000", "" },
                new string[] { "固定资产", "5000", "折旧" }
            };

            long cellId = 3001L;
            var cells = new PushCell[15];
            int cellIdx = 0;
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    cells[cellIdx] = new PushCell
                    {
                        Id = cellId++,
                        Action = 1, // New
                        CId = (c == 0) ? colId1 : (c == 1) ? colId2 : colId3,
                        RId = rowIds[r],
                        Value = ByteString.CopyFromUtf8(data[r][c] ?? "")
                    };
                    cellIdx++;
                }
            }

            // 组装 PushTable
            var pushTable = new PushTable
            {
                Id = 9001L,
                Version = 1,
                Title = "测试表格-样本"
            };
            pushTable.Columns.Add(col1);
            pushTable.Columns.Add(col2);
            pushTable.Columns.Add(col3);
            for (int i = 0; i < rows.Length; i++)
                pushTable.Rows.Add(rows[i]);
            for (int i = 0; i < cells.Length; i++)
                pushTable.Cells.Add(cells[i]);

            return pushTable.ToByteArray();
        }

        /// <summary>
        /// 生成 PushDocument Protobuf 样本字节（3 段落）。
        /// 字段对应服务端 Auditai.DTO.PushDocument：
        ///   Paragraphs (field 5) → PushParagraph（Stream=UTF-8 段落文本字节）
        /// </summary>
        internal static byte[] GenerateSamplePushDocument()
        {
            string[] paragraphTexts = new string[]
            {
                "审计报告",
                "根据审计程序，我们认为财务报表公允反映了财务状况。",
                "审计意见：无保留意见。"
            };

            long paraId = 7001L;
            var paragraphs = new PushParagraph[paragraphTexts.Length];
            for (int i = 0; i < paragraphTexts.Length; i++)
            {
                paragraphs[i] = new PushParagraph
                {
                    Id = paraId++,
                    Action = 1, // New
                    Index = i,
                    Stream = ByteString.CopyFromUtf8(paragraphTexts[i])
                };
            }

            var pushDocument = new PushDocument
            {
                Id = 8001L,
                Version = 1
            };
            for (int i = 0; i < paragraphs.Length; i++)
                pushDocument.Paragraphs.Add(paragraphs[i]);

            return pushDocument.ToByteArray();
        }

        // ===== 反序列化包装类 =====

        private class UsersWrapper
        {
            [JsonProperty("users")]
            public List<TestUser> Users { get; set; }
        }

        private class TeamsWrapper
        {
            [JsonProperty("teams")]
            public List<TestTeam> Teams { get; set; }
        }

        private class ProjectsWrapper
        {
            [JsonProperty("projects")]
            public List<TestProject> Projects { get; set; }
        }
    }

    /// <summary>种子用户</summary>
    public class TestUser
    {
        [JsonProperty("userName")] public string UserName;
        [JsonProperty("password")] public string Password;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("role")] public string Role;
        [JsonProperty("email")] public string Email;
        [JsonProperty("phone")] public string Phone;
    }

    /// <summary>种子团队</summary>
    public class TestTeam
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("description")] public string Description;
        [JsonProperty("ownerUserName")] public string OwnerUserName;
        [JsonProperty("maxMembers")] public int MaxMembers;
        [JsonProperty("licenseSeats")] public int LicenseSeats;
    }

    /// <summary>种子项目</summary>
    public class TestProject
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("year")] public int Year;
        [JsonProperty("templateType")] public string TemplateType;
        [JsonProperty("ownerUserName")] public string OwnerUserName;
        [JsonProperty("teamName")] public string TeamName;
        [JsonProperty("tableNodeName")] public string TableNodeName;
        [JsonProperty("documentNodeName")] public string DocumentNodeName;
    }
}
