﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Auditai.DTO;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 数据采集 MCP 工具注册
    /// </summary>
    public static class CollectionTools
    {
        /// <summary>
        /// 注册所有数据采集工具
        /// </summary>
        public static void Register()
        {
            // list_supported_databases
            ToolRegistry.Register("list_supported_databases",
                "列举数据采集支持的财务系统数据库类型。返回每种数据库的类型标识、显示名称、类别、描述、连接字符串格式及支持的财务系统示例。调用 collect_data 工具时需使用返回的 db_type 字段值。当前支持 SQL Server、Oracle、SQLite、Access、Paradox 五种数据库类型。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    return CollectionService.ListSupportedDatabases();
                });

            // collect_data
            ToolRegistry.Register("collect_data",
                "采集财务系统数据。异步启动采集任务并立即返回任务ID，不阻塞后续操作。采集流程：连接源数据库 → 读取账簿数据 → 构建账簿 → 导入到目标项目。启动后请使用 get_collection_status 工具传入返回的 task_id 查询采集进度。注意：MCP Server 运行在无头模式，可能需要配合AuditAI采数器.exe 完成实际采集。数据库类型请先通过 list_supported_databases 工具查询。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["db_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "数据库类型标识，可选值：sqlserver（SQL Server）、oracle（Oracle）、sqlite（SQLite）、access（Access）、paradox（Paradox）。请先调用 list_supported_databases 查询完整列表。"
                        },
                        ["connection_string"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "源数据库连接字符串。格式因数据库类型而异，可通过 list_supported_databases 工具查询各类型的连接字符串格式。例如 SQL Server: 'Server=127.0.0.1;Database=UFSystem;User Id=sa;Password=123456;'"
                        },
                        ["project_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "目标项目ID，采集的账簿数据将导入到该项目。可通过 list_projects 工具查询可用项目。"
                        }
                    },
                    ["required"] = new JArray { "db_type", "connection_string", "project_id" }
                },
                (args) =>
                {
                    string dbType = args["db_type"]?.ToString();
                    string connectionString = args["connection_string"]?.ToString();
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    return CollectionService.CollectData(dbType, connectionString, projectId);
                });

            // get_collection_status
            ToolRegistry.Register("get_collection_status",
                "查询数据采集任务的执行进度。返回进度百分比、当前步骤、是否完成、错误信息及运行时长。任务ID由 collect_data 工具启动采集时返回。任务状态包括：running（运行中）、completed（已完成）、failed（失败）。若任务完成但提示需要手动采集，请按提示使用AuditAI采数器.exe 完成采集后调用 import_ledger 工具导入账簿文件。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["task_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "采集任务ID，由 collect_data 工具返回的 task_id 字段"
                        }
                    },
                    ["required"] = new JArray { "task_id" }
                },
                (args) =>
                {
                    string taskId = args["task_id"]?.ToString();
                    return CollectionService.GetCollectionStatus(taskId);
                });
        }
    }
}
