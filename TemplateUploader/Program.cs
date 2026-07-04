﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auditai.DTO;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace TemplateUploader
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var userName = GetArg(args, "userName");
            var password = GetArg(args, "password");
            var templatesDir = GetArg(args, "templatesDir")
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Templates");
            var apiBase = GetArg(args, "apiBase") ?? "http://82.156.108.218:8957/api/";

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("用法: TemplateUploader -userName <用户名> -password <密码> [-templatesDir <目录>] [-apiBase <地址>]");
                return 1;
            }

            // 初始化：必须在创建 Project 实例前设置 StringConstBase.Current
            Auditai.Model.StringConstBase.Current = Auditai.Model.StringConstEditions.Audit;
            WebApiClient.IsLocalMode = false;
            Auditai.Model.Syncer.Disabled = false;
            WebApiClient.SetBaseAddress(apiBase);
            Console.WriteLine($"API 地址: {apiBase}");

            // 登录（密码需 SHA256 哈希）
            Console.Write("正在登录... ");
            try
            {
                var hashPassword = Encrypts.SHA256Encrypt(password, isUrl: false);
                Console.WriteLine($"密码哈希: {hashPassword}");
                var loginResult = await WebApiClient.AccountLogin(userName, hashPassword);
                if (loginResult?.Item2 == null)
                {
                    Console.WriteLine("失败: 服务器返回无效数据");
                    return 1;
                }
                var userDto = loginResult.Item2;
                Auditai.Model.User.Current = new Auditai.Model.User
                {
                    Id = userDto.Id,
                    Name = userDto.Name,
                    UserName = userDto.UserName,
                    TelPhone = userDto.Phone,
                    IsSystemAdmin = userDto.IsSystemAdmin
                };
                Console.WriteLine($"成功 ({userDto.Name})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"失败: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"堆栈: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                    Console.WriteLine($"内部堆栈: {ex.InnerException.StackTrace}");
                }
                return 1;
            }

            // 获取团队列表
            Console.Write("正在获取团队... ");
            var teams = await WebApiClient.GetUserTeams();
            if (teams == null || teams.Count == 0)
            {
                Console.WriteLine("失败: 没有可用的团队");
                return 1;
            }
            var firstTeam = teams[0];
            var teamId = Guid.Parse(firstTeam.Value<string>("teamId"));
            await WebApiClient.UpdateCurrentTeam(teamId);
            Console.WriteLine($"{firstTeam.Value<string>("teamName")} ({teamId})");

            // 获取已有模板列表（用于去重）
            Console.Write("正在获取云端已有模板... ");
            var existingTemplates = await WebApiClient.GetTemplates();
            var existingNames = new HashSet<string>(
                (existingTemplates ?? Enumerable.Empty<Auditai.DTO.Project>())
                    .Select(t => t.Name ?? string.Empty));
            Console.WriteLine($"{existingNames.Count} 个");

            // 查找本地 .db 模板文件
            if (!Directory.Exists(templatesDir))
            {
                Console.WriteLine($"模板目录不存在: {templatesDir}");
                return 1;
            }
            var dbFiles = Directory.GetFiles(templatesDir, "*.db");
            Console.WriteLine($"找到 {dbFiles.Length} 个本地模板文件");
            Console.WriteLine();

            int success = 0, skipped = 0, failed = 0;
            foreach (var dbFile in dbFiles)
            {
                var templateName = Path.GetFileNameWithoutExtension(dbFile);
                try
                {
                    var result = await UploadTemplateAsync(dbFile, templateName, existingNames);
                    switch (result)
                    {
                        case UploadResult.Uploaded: success++; break;
                        case UploadResult.Skipped: skipped++; break;
                        default: failed++; break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  [失败] {ex.Message}");
                    failed++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("===== 汇总 =====");
            Console.WriteLine($"成功: {success}, 跳过: {skipped}, 失败: {failed}");
            return failed > 0 ? 1 : 0;
        }

        private enum UploadResult { Uploaded, Skipped, Failed }

        private static async Task<UploadResult> UploadTemplateAsync(
            string dbFile, string templateName, HashSet<string> existingNames)
        {
            // 检查同名模板
            if (existingNames.Contains(templateName))
            {
                Console.WriteLine($"[跳过] {templateName} (云端已存在同名模板)");
                return UploadResult.Skipped;
            }

            Console.WriteLine($"[上传] {templateName}");

            // 用 ProjectDAL 打开本地 .db 并加载模型
            var dal = new ProjectDAL(dbFile);
            var project = new Auditai.Model.Project();
            project.Dal = dal;
            project.Load();
            var tableNodes = project.GetAllTableNodes().ToList();
            var docNodes = project.GetAllDocumentNodes().ToList();
            Console.WriteLine($"  表格: {tableNodes.Count}, 文档: {docNodes.Count}");

            // 在云端创建模板项目（Type=ProjectType.Template）
            var cloudProjectId = Guid.NewGuid();
            var currentUser = Auditai.Model.User.Current;
            var dtoProject = new Auditai.DTO.Project
            {
                Id = cloudProjectId,
                ParentId = null,
                Number = string.Empty,
                Name = templateName,
                Category = string.Empty,
                Note = string.Empty,
                Auditee = string.Empty,
                Type = ProjectType.Template,
                ChargeType = ChargeType.None,
                TeamVisible = true,
                Creator = new Auditai.DTO.User
                {
                    Id = currentUser.Id,
                    Name = currentUser.Name,
                    UserName = currentUser.UserName
                },
                Users = new Auditai.DTO.User[]
                {
                    new Auditai.DTO.User
                    {
                        Id = currentUser.Id,
                        Name = currentUser.Name,
                        UserName = currentUser.UserName,
                        Role = UserRole.Editor
                    }
                },
                CreateTime = DateTime.Now
            };

            await WebApiClient.CreateProject(dtoProject);
            Console.WriteLine($"  云端项目已创建: {cloudProjectId}");

            // 设置 model Project 的 Id（Syncer.Push 内部使用 table.Project.Id）
            project.Id = cloudProjectId;
            Auditai.Model.Project.Current = project;

            // 重置所有 TreeGroup 和 TreeNode 状态为 New + Version=0，触发全量推送
            foreach (var group in project.TreeGroups)
            {
                group.Status = Auditai.Model.SyncStatus.New;
            }
            foreach (var node in project.GetAllTreeNodes())
            {
                node.Status = Auditai.Model.SyncStatus.New;
                node.Version = 0;
            }

            // 推送项目结构（树组 + 树节点 + 数据引用 + 验证公式）
            var pushProjectResult = await Auditai.Model.Syncer.Push(project);
            if (pushProjectResult != Auditai.Model.PushResult.Success)
            {
                Console.WriteLine($"  项目结构推送异常: {pushProjectResult}");
                return UploadResult.Failed;
            }
            Console.WriteLine("  项目结构已推送");

            // 推送所有表格
            int tableCount = 0;
            foreach (var tableNode in tableNodes)
            {
                var table = tableNode.Table;
                tableNode.Version = 0; // 强制全量推送
                table.LoadAndReturn();

                // 重置所有子实体状态为 New
                foreach (var col in table.Columns) col.Status = Auditai.Model.SyncStatus.New;
                foreach (var row in table.Rows) row.Status = Auditai.Model.SyncStatus.New;
                foreach (var cell in table.Cells) cell.Status = Auditai.Model.SyncStatus.New;
                foreach (var style in table.CellStyles) style.Status = Auditai.Model.SyncStatus.New;
                foreach (var merge in table.MergedCells) merge.Status = Auditai.Model.SyncStatus.New;

                await Auditai.Model.Syncer.Push(table);
                tableCount++;
            }
            Console.WriteLine($"  表格已推送: {tableCount}");

            // 推送所有文档
            int docCount = 0;
            foreach (var docNode in docNodes)
            {
                var doc = docNode.Document;
                docNode.Version = 0; // 强制全量推送
                doc.LoadAndReturn();

                foreach (var para in doc.Paragraphs) para.Status = Auditai.Model.SyncStatus.New;

                await Auditai.Model.Syncer.Push(doc);
                docCount++;
            }
            Console.WriteLine($"  文档已推送: {docCount}");

            existingNames.Add(templateName);
            Console.WriteLine($"[完成] {templateName}");
            return UploadResult.Uploaded;
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-" + name || args[i] == "/" + name)
                    return args[i + 1];
            }
            return null;
        }
    }
}
