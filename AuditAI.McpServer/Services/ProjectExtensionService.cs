using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project = Auditai.Model.Project;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 项目扩展服务
    /// 在 ProjectService 基础上补充项目级管理能力：
    /// 删除项目（移入回收站）、读取/更新项目属性、备份项目（zip）、项目统计
    /// </summary>
    public static class ProjectExtensionService
    {
        // =============================================
        // 删除项目（移入回收站）
        // =============================================

        public static string DeleteProject(Guid projectId, bool permanent = false)
        {
            try
            {
                // 如果删除的是当前打开的项目，先关闭
                if (SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                {
                    SessionState.Current.CloseProject();
                }

                if (permanent)
                {
                    // 永久删除：从数据库和文件系统彻底移除
                    // DeleteProjectFromServer 期望 { "Ids": [guid, ...] } 格式
                    var body = new JObject { ["Ids"] = new JArray { projectId.ToString() } };
                    LocalDataStore.DeleteProjectFromServer(body).GetAwaiter().GetResult();
                    return Ok("项目已永久删除", new JObject
                    {
                        ["project_id"] = projectId.ToString(),
                        ["permanent"] = true
                    });
                }
                else
                {
                    // 移入回收站（标记 IsDeleted）
                    LocalDataStore.DeleteProject(projectId).GetAwaiter().GetResult();
                    return Ok("项目已移入回收站", new JObject
                    {
                        ["project_id"] = projectId.ToString(),
                        ["permanent"] = false,
                        ["hint"] = "可调用 restore_project 恢复"
                    });
                }
            }
            catch (Exception ex) { return ErrorJson("删除项目失败: " + ex.Message); }
        }

        // =============================================
        // 项目属性读取
        // =============================================

        public static string GetProjectProperties(Guid projectId)
        {
            try
            {
                var projects = LocalDataStore.GetProjects().GetAwaiter().GetResult();
                var p = projects.FirstOrDefault(x => x.Id == projectId);

                // 回退：若 LocalDataStore 中没有此项目，但当前会话已打开该项目，则使用会话中的 Model.Project
                if (p == null && SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                {
                    var mp = SessionState.Current.CurrentProject;
                    var sessionPath = SessionState.Current.CurrentProjectPath ?? GetProjectDbPath(projectId);
                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = mp.Id.ToString(),
                        ["name"] = mp.Name ?? "",
                        ["number"] = mp.Number ?? "",
                        ["category"] = mp.Category ?? "",
                        ["auditee"] = mp.Auditee ?? "",
                        ["note"] = mp.Note ?? "",
                        ["version"] = mp.Version,
                        ["create_time"] = mp.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ["creator_name"] = mp.Creator?.Name ?? "",
                        ["creator_id"] = mp.Creator?.Id ?? 0,
                        ["parent_id"] = mp.ParentId?.ToString() ?? "",
                        ["is_system_build"] = mp.SystemBuild,
                        ["path"] = sessionPath,
                        ["size"] = GetFileSize(sessionPath),
                        ["source"] = "session"
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }

                if (p == null)
                    return ErrorJson($"未找到项目: {projectId}");

                var dtoResult = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = p.Id.ToString(),
                    ["name"] = p.Name ?? "",
                    ["number"] = p.Number ?? "",
                    ["category"] = p.Category ?? "",
                    ["auditee"] = p.Auditee ?? "",
                    ["note"] = p.Note ?? "",
                    ["type"] = p.Type.ToString(),
                    ["version"] = p.Version,
                    ["create_time"] = p.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["creator_name"] = p.Creator?.Name ?? "",
                    ["creator_id"] = p.Creator?.Id ?? 0,
                    ["parent_id"] = p.ParentId?.ToString() ?? "",
                    ["template_id"] = p.TemplateId?.ToString() ?? "",
                    ["is_system_build"] = p.SystemBuild,
                    ["path"] = GetProjectDbPath(p.Id),
                    ["size"] = GetFileSize(GetProjectDbPath(p.Id)),
                    ["source"] = "localdatastore"
                };
                return JsonConvert.SerializeObject(dtoResult, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取项目属性失败: " + ex.Message); }
        }

        // =============================================
        // 项目属性更新
        // =============================================

        public static string SetProjectProperties(Guid projectId, JObject properties)
        {
            try
            {
                if (properties == null || properties.Count == 0)
                    return ErrorJson("未提供任何要更新的属性");

                var projects = LocalDataStore.GetProjects().GetAwaiter().GetResult();
                var p = projects.FirstOrDefault(x => x.Id == projectId);

                // 回退：若 LocalDataStore 没有，但会话中已打开该项目，则更新会话中的 Model.Project
                if (p == null && SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                {
                    var cur = SessionState.Current.CurrentProject;
                    var updated = new JObject();
                    if (properties["name"] != null) { cur.Name = properties["name"].ToString(); updated["name"] = cur.Name; }
                    if (properties["number"] != null) { cur.Number = properties["number"].ToString(); updated["number"] = cur.Number; }
                    if (properties["category"] != null) { cur.Category = properties["category"].ToString(); updated["category"] = cur.Category; }
                    if (properties["auditee"] != null) { cur.Auditee = properties["auditee"].ToString(); updated["auditee"] = cur.Auditee; }
                    if (properties["note"] != null) { cur.Note = properties["note"].ToString(); updated["note"] = cur.Note; }
                    cur.NeedSave = true;

                    var sessionResult = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId.ToString(),
                        ["message"] = "项目属性已更新（仅会话内存，未持久化到 LocalDataStore）",
                        ["updated_fields"] = updated,
                        ["source"] = "session"
                    };
                    return JsonConvert.SerializeObject(sessionResult, Formatting.Indented);
                }

                if (p == null)
                    return ErrorJson($"未找到项目: {projectId}");

                var dtoUpdated = new JObject();
                if (properties["name"] != null) { p.Name = properties["name"].ToString(); dtoUpdated["name"] = p.Name; }
                if (properties["number"] != null) { p.Number = properties["number"].ToString(); dtoUpdated["number"] = p.Number; }
                if (properties["category"] != null) { p.Category = properties["category"].ToString(); dtoUpdated["category"] = p.Category; }
                if (properties["auditee"] != null) { p.Auditee = properties["auditee"].ToString(); dtoUpdated["auditee"] = p.Auditee; }
                if (properties["note"] != null) { p.Note = properties["note"].ToString(); dtoUpdated["note"] = p.Note; }

                LocalDataStore.UpdateProject(p).GetAwaiter().GetResult();

                // 如果更新的是当前打开的项目，同步内存中的对象
                if (SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                {
                    var cur = SessionState.Current.CurrentProject;
                    if (properties["name"] != null) cur.Name = p.Name;
                    if (properties["number"] != null) cur.Number = p.Number;
                    if (properties["category"] != null) cur.Category = p.Category;
                    if (properties["auditee"] != null) cur.Auditee = p.Auditee;
                    if (properties["note"] != null) cur.Note = p.Note;
                    cur.NeedSave = true;
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId.ToString(),
                    ["message"] = "项目属性已更新",
                    ["updated_fields"] = dtoUpdated,
                    ["source"] = "localdatastore"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("更新项目属性失败: " + ex.Message); }
        }

        // =============================================
        // 项目备份（zip）
        // =============================================

        public static string BackupProject(Guid projectId, string outputPath, bool includeAttachments = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(outputPath))
                    return ErrorJson("备份输出路径不能为空");

                // 优先使用 LocalDataStore 的路径，回退到会话路径
                string dbPath = GetProjectDbPath(projectId);
                if (!File.Exists(dbPath) && SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                {
                    dbPath = SessionState.Current.CurrentProjectPath ?? dbPath;
                }
                if (!File.Exists(dbPath))
                    return ErrorJson($"项目数据库文件不存在: {dbPath}");

                EnsureOutputDirectory(outputPath);
                string absOutput = Path.GetFullPath(outputPath);

                // 同时备份项目元信息（projects.json）
                var projects = LocalDataStore.GetProjects().GetAwaiter().GetResult();
                var p = projects.FirstOrDefault(x => x.Id == projectId);
                // 回退：使用会话中的项目
                var mp = (p == null && SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                    ? SessionState.Current.CurrentProject : null;

                using (var fs = new FileStream(absOutput, FileMode.Create, FileAccess.Write))
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    // 添加 .db 文件
                    var dbEntry = zip.CreateEntry($"{projectId}.db");
                    using (var es = dbEntry.Open())
                    using (var src = File.OpenRead(dbPath))
                    {
                        src.CopyTo(es);
                    }

                    // 添加元信息
                    var metaEntry = zip.CreateEntry("project-meta.json");
                    using (var es = metaEntry.Open())
                    using (var sw = new StreamWriter(es))
                    {
                        var meta = new JObject
                        {
                            ["project_id"] = projectId.ToString(),
                            ["name"] = p?.Name ?? mp?.Name ?? "",
                            ["number"] = p?.Number ?? mp?.Number ?? "",
                            ["category"] = p?.Category ?? mp?.Category ?? "",
                            ["auditee"] = p?.Auditee ?? mp?.Auditee ?? "",
                            ["note"] = p?.Note ?? mp?.Note ?? "",
                            ["backup_time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            ["source_db_path"] = dbPath,
                            ["include_attachments"] = includeAttachments
                        };
                        sw.Write(meta.ToString(Formatting.Indented));
                    }

                    // 可选：附加附件目录（如果存在）
                    if (includeAttachments)
                    {
                        string attachDir = Path.Combine("data", "attachments", projectId.ToString());
                        if (Directory.Exists(attachDir))
                        {
                            foreach (var file in Directory.EnumerateFiles(attachDir, "*", SearchOption.AllDirectories))
                            {
                                // net462 没有 Path.GetRelativePath，手动计算相对路径
                                string rel = file.Substring(attachDir.Length).TrimStart('\\', '/');
                                var entry = zip.CreateEntry($"attachments/{rel.Replace('\\', '/')}");
                                using (var es = entry.Open())
                                using (var src = File.OpenRead(file))
                                {
                                    src.CopyTo(es);
                                }
                            }
                        }
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId.ToString(),
                    ["output_path"] = absOutput,
                    ["backup_size"] = GetFileSize(absOutput),
                    ["include_attachments"] = includeAttachments,
                    ["message"] = "项目已备份为 zip"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("备份项目失败: " + ex.Message); }
        }

        // =============================================
        // 项目统计
        // =============================================

        public static string GetProjectStats(Guid projectId)
        {
            try
            {
                var projects = LocalDataStore.GetProjects().GetAwaiter().GetResult();
                var p = projects.FirstOrDefault(x => x.Id == projectId);

                // 回退：若 LocalDataStore 中没有此项目，但当前会话已打开该项目，则使用会话中的 Model.Project
                var mp = (p == null && SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                    ? SessionState.Current.CurrentProject : null;

                if (p == null && mp == null)
                    return ErrorJson($"未找到项目: {projectId}");

                string name = p?.Name ?? mp?.Name ?? "";
                string dbPath = p != null ? GetProjectDbPath(p.Id) : (SessionState.Current.CurrentProjectPath ?? GetProjectDbPath(projectId));

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId.ToString(),
                    ["name"] = name,
                    ["db_size"] = GetFileSize(dbPath),
                    ["source"] = p != null ? "localdatastore" : "session"
                };

                // 如果项目当前已打开，提供更详细的统计
                if (SessionState.Current.HasProject && SessionState.Current.CurrentProject?.Id == projectId)
                {
                    var proj = SessionState.Current.CurrentProject;
                    var allNodes = proj.GetAllTreeNodes().ToList();
                    var tableNodes = allNodes.OfType<TreeTableNode>().ToList();
                    var docNodes = allNodes.OfType<TreeDocumentNode>().ToList();
                    var dirNodes = allNodes.OfType<TreeDirectoryNode>().ToList();

                    int totalRows = 0, totalCols = 0, totalCells = 0, totalMerges = 0;
                    long totalParagraphs = 0;

                    foreach (var tn in tableNodes)
                    {
                        try
                        {
                            tn.Table.LoadAndReturn(true);
                            totalRows += tn.Table.Rows.Count;
                            totalCols += tn.Table.Columns.Count;
                            totalCells += tn.Table.Rows.Count * tn.Table.Columns.Count;
                            totalMerges += tn.Table.MergedCells.Count;
                        }
                        catch { /* 忽略单个表格加载失败 */ }
                    }

                    foreach (var dn in docNodes)
                    {
                        try
                        {
                            dn.Document.LoadAndReturn();
                            totalParagraphs += dn.Document.Paragraphs.Count;
                        }
                        catch { /* 忽略单个文档加载失败 */ }
                    }

                    result["stats"] = new JObject
                    {
                        ["tree_group_count"] = proj.TreeGroups.Count,
                        ["total_node_count"] = allNodes.Count,
                        ["directory_node_count"] = dirNodes.Count,
                        ["table_node_count"] = tableNodes.Count,
                        ["document_node_count"] = docNodes.Count,
                        ["total_table_rows"] = totalRows,
                        ["total_table_cols"] = totalCols,
                        ["total_table_cells"] = totalCells,
                        ["total_merged_cells"] = totalMerges,
                        ["total_document_paragraphs"] = totalParagraphs,
                        ["need_save"] = proj.NeedSave
                    };
                }
                else
                {
                    result["stats"] = new JObject
                    {
                        ["loaded"] = false,
                        ["hint"] = "项目未打开，无法获取详细统计。请先调用 open_project 工具。"
                    };
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取项目统计失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static string GetProjectDbPath(Guid projectId)
        {
            long userId = Auditai.Model.User.Current?.Id ?? 1;
            return Path.Combine("data", userId.ToString(), $"{projectId}.db");
        }

        private static long GetFileSize(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return 0;
                return new FileInfo(path).Length;
            }
            catch { return 0; }
        }

        private static void EnsureOutputDirectory(string outputPath)
        {
            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
            }
            catch { /* 忽略删除失败 */ }
        }

        private static string Ok(string msg, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["message"] = msg };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
