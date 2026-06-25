﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Auditai.DTO;
using Auditai.LocalDataStore;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 项目管理服务
    /// 封装项目生命周期管理操作
    /// </summary>
    public static class ProjectService
    {
        /// <summary>
        /// 列举所有本地项目
        /// </summary>
        public static string ListProjects()
        {
            try
            {
                var projects = LocalDataStore.GetProjects().GetAwaiter().GetResult();
                var list = projects.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    number = p.Number,
                    category = p.Category,
                    auditee = p.Auditee,
                    note = p.Note,
                    type = p.Type.ToString(),
                    version = p.Version,
                    create_time = p.CreateTime,
                    creator = p.Creator?.Name,
                    path = GetProjectDbPath(p.Id),
                    size = GetFileSize(GetProjectDbPath(p.Id))
                }).ToList();

                var result = new JObject
                {
                    ["total"] = list.Count,
                    ["projects"] = JArray.FromObject(list)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("列举项目失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 创建新项目
        /// </summary>
        public static string CreateProject(string name, int year, string templateType = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return ErrorJson("项目名称不能为空");
                }

                var project = new Auditai.DTO.Project
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Number = year.ToString(),
                    Category = string.IsNullOrEmpty(templateType) ? "" : templateType,
                    Auditee = "",
                    Note = "",
                    Type = ProjectType.Project,
                    Version = 0,
                    CreateTime = DateTime.Now,
                    Creator = new Auditai.DTO.User
                    {
                        Id = Auditai.Model.User.Current?.Id ?? 1,
                        Name = Auditai.Model.User.Current?.Name ?? "管理员",
                        UserName = Auditai.Model.User.Current?.UserName ?? "admin",
                        Role = UserRole.Manager
                    },
                    Users = new List<Auditai.DTO.User>
                    {
                        new Auditai.DTO.User
                        {
                            Id = Auditai.Model.User.Current?.Id ?? 1,
                            Name = Auditai.Model.User.Current?.Name ?? "管理员",
                            UserName = Auditai.Model.User.Current?.UserName ?? "admin",
                            Role = UserRole.Manager
                        }
                    }
                };

                // 如果指定了模板类型，尝试查找对应模板
                if (!string.IsNullOrEmpty(templateType))
                {
                    var template = FindTemplateByCategory(templateType);
                    if (template != null)
                    {
                        project.TemplateId = template.Id;
                    }
                }

                LocalDataStore.CreateProject(project).GetAwaiter().GetResult();

                string projectPath = GetProjectDbPath(project.Id);

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = project.Id,
                    ["name"] = project.Name,
                    ["year"] = year,
                    ["path"] = projectPath,
                    ["message"] = "项目创建成功"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("创建项目失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 打开项目
        /// </summary>
        public static string OpenProject(string projectPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectPath))
                {
                    return ErrorJson("项目路径不能为空");
                }

                if (!File.Exists(projectPath))
                {
                    return ErrorJson($"项目文件不存在: {projectPath}");
                }

                // 如果当前已有打开的项目，先关闭
                if (SessionState.Current.HasProject)
                {
                    SessionState.Current.CloseProject();
                }

                // 确保 CrossProjectFormulaStore 所需的 data/{userId} 目录存在
                // CrossProjectFormulaStore 构造函数会用 project.Id（此时为 Guid.Empty）拼接相对路径
                // data/{userId}/00000000-0000-0000-0000-000000000000.db 并调用 EnsureTable()
                long userId = Auditai.Model.User.Current?.Id ?? 1;
                string dataDir = Path.Combine("data", userId.ToString());
                Directory.CreateDirectory(dataDir);

                // 加载项目
                var project = new Auditai.Model.Project();
                project.Dal = new ProjectDAL(projectPath);

                // 确保项目数据库中存在 Project 行
                // new ProjectDAL(path) 仅创建 schema，不插入数据
                // 若项目 DB 是新建的（无 Project 行），Dal.GetProject() 返回 null，
                // 后续 project.Load() 中 Dal.GetProject().Version 会抛 NRE
                var existingDto = project.Dal.GetProject();
                if (existingDto == null)
                {
                    // 从路径提取 projectId（文件名 = {projectId}.db）
                    Guid projectId = ExtractProjectIdFromPath(projectPath);

                    // 从中央 LocalDataStore 获取项目元信息
                    var centralProject = LocalDataStore.GetProjects().GetAwaiter().GetResult()
                        .FirstOrDefault(p => p.Id == projectId);

                    if (centralProject != null)
                    {
                        project.Dal.SaveProject(new Auditai.DTO.Project
                        {
                            Id = centralProject.Id,
                            Name = centralProject.Name ?? "",
                            ParentId = centralProject.Id,
                            Version = 0,
                            Number = centralProject.Number ?? "",
                            Category = centralProject.Category ?? "",
                            Note = centralProject.Note ?? "",
                            Auditee = centralProject.Auditee ?? "",
                            CreateTime = centralProject.CreateTime
                        });
                    }
                    else
                    {
                        // 中央存储中也没有记录，使用最小化默认值
                        project.Dal.SaveProject(new Auditai.DTO.Project
                        {
                            Id = projectId,
                            Name = Path.GetFileNameWithoutExtension(projectPath),
                            ParentId = projectId,
                            Version = 0,
                            Number = "",
                            Category = "",
                            Note = "",
                            Auditee = "",
                            CreateTime = DateTime.Now
                        });
                    }
                }

                // 在 Load 之前设置 Project.Current，因为部分加载逻辑可能依赖它
                Auditai.Model.Project.Current = project;

                // 在 Load 之前填充项目元信息（特别是 project.Id）
                // 因为 project.Load() 内部会调用 FormulaStore.Load()，
                // 而 FormulaStore 的 DB 路径依赖 project.Id
                var preLoadDto = project.Dal.GetProject();
                if (preLoadDto != null)
                {
                    project.PopulateFieldsFromDto(preLoadDto);
                }

                project.Load();

                // 设置 ID 基址，避免重新打开项目后生成重复的节点 ID
                // 与 MainForm 中的 toOpen.SetIdBase(tup.Item1) 行为一致：
                // 通过 StorageRouter.OpenProject 获取 OperationId（每次打开递增），
                // 确保新生成 ID 的高 32 位与历史已存在 ID 不冲突
                try
                {
                    int operationId = 0;
                    if (project.Id != Guid.Empty)
                    {
                        var openResult = Auditai.LocalDataStore.StorageRouter.OpenProject(project.Id).GetAwaiter().GetResult();
                        operationId = openResult.Item1;
                    }

                    // 计算现有节点 ID 的最大高 32 位作为安全下限
                    // 防止项目数据库中存在历史 ID 与新基址冲突
                    int maxBase = operationId;
                    foreach (var node in project.GetAllTreeNodes())
                    {
                        int nodeBase = (int)((ulong)node.Id.Value >> 32);
                        if (nodeBase >= maxBase) maxBase = nodeBase + 1;
                    }

                    project.SetIdBase(maxBase);
                }
                catch (Exception idEx)
                {
                    Console.Error.WriteLine($"[SetIdBase Warning] {idEx.Message}");
                    // 即使设置失败也继续，使用默认的 0 基址
                }

                // 设置到会话状态
                SessionState.Current.SetProject(project, projectPath);

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = project.Id,
                    ["name"] = project.Name,
                    ["number"] = project.Number,
                    ["category"] = project.Category,
                    ["auditee"] = project.Auditee,
                    ["note"] = project.Note,
                    ["version"] = project.Version,
                    ["create_time"] = project.CreateTime,
                    ["path"] = projectPath,
                    ["tree_group_count"] = project.TreeGroups.Count,
                    ["node_count"] = project.GetAllTreeNodes().Count(),
                    ["message"] = "项目已打开"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[OpenProject Error] {ex}");
                if (ex.InnerException != null)
                    Console.Error.WriteLine($"[Inner] {ex.InnerException}");
                Console.Error.WriteLine($"[StackTrace] {ex.StackTrace}");
                return ErrorJson("打开项目失败: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 保存当前项目
        /// </summary>
        public static string SaveProject()
        {
            try
            {
                if (!SessionState.Current.HasProject)
                {
                    return ErrorJson("未打开项目，请先调用 open_project 工具");
                }

                var project = SessionState.Current.CurrentProject;
                project.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = project.Id,
                    ["name"] = project.Name,
                    ["path"] = SessionState.Current.CurrentProjectPath,
                    ["message"] = "项目已保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("保存项目失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 关闭当前项目
        /// </summary>
        public static string CloseProject()
        {
            try
            {
                if (!SessionState.Current.HasProject)
                {
                    var empty = new JObject
                    {
                        ["success"] = true,
                        ["message"] = "当前没有打开的项目"
                    };
                    return JsonConvert.SerializeObject(empty, Formatting.Indented);
                }

                var projectId = SessionState.Current.CurrentProject.Id;
                var projectName = SessionState.Current.CurrentProject.Name;
                SessionState.Current.CloseProject();

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId,
                    ["name"] = projectName,
                    ["message"] = "项目已关闭"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("关闭项目失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取当前项目信息
        /// </summary>
        public static string GetProjectInfo()
        {
            try
            {
                if (!SessionState.Current.HasProject)
                {
                    return ErrorJson("未打开项目，请先调用 open_project 工具");
                }

                var project = SessionState.Current.CurrentProject;
                var tableNodes = project.GetAllTableNodes().ToList();
                var docNodes = project.GetAllDocumentNodes().ToList();

                var result = new JObject
                {
                    ["project_id"] = project.Id,
                    ["name"] = project.Name,
                    ["number"] = project.Number,
                    ["category"] = project.Category,
                    ["auditee"] = project.Auditee,
                    ["note"] = project.Note,
                    ["version"] = project.Version,
                    ["create_time"] = project.CreateTime,
                    ["creator"] = project.Creator?.Name,
                    ["path"] = SessionState.Current.CurrentProjectPath,
                    ["tree_group_count"] = project.TreeGroups.Count,
                    ["table_count"] = tableNodes.Count,
                    ["document_count"] = docNodes.Count,
                    ["need_save"] = project.NeedSave
                };

                // 列出树结构概要
                var groups = project.TreeGroups.Select(g => new JObject
                {
                    ["id"] = g.Id.ToString(),
                    ["name"] = g.Name,
                    ["node_count"] = g.GetAllNodes().Count()
                });
                result["tree_groups"] = new JArray(groups);

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取项目信息失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        /// <summary>
        /// 获取项目 .db 文件路径
        /// </summary>
        private static string GetProjectDbPath(Guid projectId)
        {
            long userId = Auditai.Model.User.Current?.Id ?? 1;
            return Path.Combine("data", userId.ToString(), $"{projectId}.db");
        }

        /// <summary>
        /// 从项目 .db 文件路径提取 projectId
        /// 文件名格式为 {projectId}.db
        /// </summary>
        private static Guid ExtractProjectIdFromPath(string projectPath)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(projectPath);
                if (Guid.TryParse(fileName, out Guid id))
                    return id;
            }
            catch { }
            return Guid.Empty;
        }

        /// <summary>
        /// 获取文件大小（字节），文件不存在返回 0
        /// </summary>
        private static long GetFileSize(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return 0;
                return new FileInfo(path).Length;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 根据类别查找模板
        /// </summary>
        private static Auditai.DTO.Project FindTemplateByCategory(string category)
        {
            try
            {
                var templates = LocalDataStore.GetTemplates().GetAwaiter().GetResult();
                return templates.FirstOrDefault(t =>
                    !string.IsNullOrEmpty(t.Category) &&
                    t.Category.IndexOf(category, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 生成错误 JSON 响应
        /// </summary>
        private static string ErrorJson(string message)
        {
            var result = new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }
    }
}
