using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 项目扩展 MCP 工具注册
    /// 在 ProjectTools 基础上补充项目级管理能力：
    /// 删除项目（移入回收站/永久删除）、读取/更新项目属性、备份项目（zip）、项目统计
    /// </summary>
    public static class ProjectExtensionTools
    {
        public static void Register()
        {
            // delete_project
            ToolRegistry.Register("delete_project",
                "删除项目。permanent=false（默认）时移入回收站（可恢复），permanent=true 时从数据库和文件系统永久删除（不可恢复）。当需要删除项目时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "项目 ID（GUID 字符串）"
                        },
                        ["permanent"] = new JObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "是否永久删除（可选，默认 false 移入回收站）"
                        }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    string idStr = args["project_id"]?.ToString();
                    if (!Guid.TryParse(idStr, out Guid projectId))
                        return "{\"success\":false,\"error\":\"无效的项目 ID（应为 GUID 字符串）\"}";
                    bool permanent = args["permanent"]?.Value<bool>() ?? false;
                    return ProjectExtensionService.DeleteProject(projectId, permanent);
                });

            // get_project_properties
            ToolRegistry.Register("get_project_properties",
                "获取项目元数据属性：名称、编号、类别、被审计单位、备注、创建时间、创建者、模板 ID 等。当需要查看项目属性详情时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "项目 ID（GUID 字符串）"
                        }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    string idStr = args["project_id"]?.ToString();
                    if (!Guid.TryParse(idStr, out Guid projectId))
                        return "{\"success\":false,\"error\":\"无效的项目 ID（应为 GUID 字符串）\"}";
                    return ProjectExtensionService.GetProjectProperties(projectId);
                });

            // set_project_properties
            ToolRegistry.Register("set_project_properties",
                "更新项目元数据属性。可更新字段：name/number/category/auditee/note。当需要修改项目属性时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "项目 ID（GUID 字符串）"
                        },
                        ["properties"] = new JObject
                        {
                            ["type"] = "object",
                            ["description"] = "要更新的属性对象",
                            ["properties"] = new JObject
                            {
                                ["name"] = new JObject { ["type"] = "string", ["description"] = "项目名称" },
                                ["number"] = new JObject { ["type"] = "string", ["description"] = "项目编号" },
                                ["category"] = new JObject { ["type"] = "string", ["description"] = "项目类别" },
                                ["auditee"] = new JObject { ["type"] = "string", ["description"] = "被审计单位" },
                                ["note"] = new JObject { ["type"] = "string", ["description"] = "备注" }
                            }
                        }
                    },
                    ["required"] = new JArray { "project_id", "properties" }
                },
                (args) =>
                {
                    string idStr = args["project_id"]?.ToString();
                    if (!Guid.TryParse(idStr, out Guid projectId))
                        return "{\"success\":false,\"error\":\"无效的项目 ID（应为 GUID 字符串）\"}";
                    JObject props = args["properties"] as JObject;
                    return ProjectExtensionService.SetProjectProperties(projectId, props);
                });

            // backup_project
            ToolRegistry.Register("backup_project",
                "将项目备份为 zip 压缩包，包含项目 .db 文件、元信息 JSON 和可选的附件目录。当需要备份项目时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "项目 ID（GUID 字符串）"
                        },
                        ["output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "备份 zip 文件输出路径（如：D:\\backups\\project-2025.zip）"
                        },
                        ["include_attachments"] = new JObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "是否包含附件目录（可选，默认 true）"
                        }
                    },
                    ["required"] = new JArray { "project_id", "output_path" }
                },
                (args) =>
                {
                    string idStr = args["project_id"]?.ToString();
                    if (!Guid.TryParse(idStr, out Guid projectId))
                        return "{\"success\":false,\"error\":\"无效的项目 ID（应为 GUID 字符串）\"}";
                    string outputPath = args["output_path"]?.ToString();
                    bool includeAttachments = args["include_attachments"]?.Value<bool>() ?? true;
                    return ProjectExtensionService.BackupProject(projectId, outputPath, includeAttachments);
                });

            // get_project_stats
            ToolRegistry.Register("get_project_stats",
                "获取项目统计信息：数据库大小、树节点数、表格数、文档数、表格总行数/列数/单元格数、合并单元格数、文档段落数等。当需要全面了解项目规模时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "项目 ID（GUID 字符串）"
                        }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    string idStr = args["project_id"]?.ToString();
                    if (!Guid.TryParse(idStr, out Guid projectId))
                        return "{\"success\":false,\"error\":\"无效的项目 ID（应为 GUID 字符串）\"}";
                    return ProjectExtensionService.GetProjectStats(projectId);
                });
        }
    }
}
