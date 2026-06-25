﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 校验管理 MCP 工具注册
    /// 提供文档校验、表格校验、批量校验及校验结果查询等能力
    /// </summary>
    public static class ValidationTools
    {
        /// <summary>
        /// 注册所有校验管理工具
        /// </summary>
        public static void Register()
        {
            // validate_document
            ToolRegistry.Register("validate_document",
                "校验指定文档，运行项目级校验规则（包括绑定到文档域的校验公式以及所有表格校验公式），返回通过/失败项汇总。校验公式由用户在 AuditAI UI 中配置，存储在项目数据库中。当需要检查文档整体合规性或运行所有校验规则时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID（可通过 get_project_info 获取）" }
                    },
                    ["required"] = new JArray { "document_node_id" }
                },
                (args) =>
                {
                    long documentNodeId = args["document_node_id"]?.Value<long>() ?? 0;
                    return ValidationService.ValidateDocument(documentNodeId);
                });

            // validate_table
            ToolRegistry.Register("validate_table",
                "校验指定表格，运行绑定到该表格的校验公式，并执行基本的数据完整性检查（公式错误、空值、数据类型不匹配等），返回通过/失败项列表。校验公式由用户在 AuditAI UI 中配置。当需要检查表格数据正确性、排查公式错误或验证勾稽关系时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID（可通过 get_project_info 获取）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    return ValidationService.ValidateTable(tableNodeId);
                });

            // validate_all_tables
            ToolRegistry.Register("validate_all_tables",
                "校验当前项目中所有表格，遍历每个表格节点运行校验规则，返回汇总结果（每个表格的通过/失败统计）。当批量修改数据后需要全局检查数据完整性时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => ValidationService.ValidateAllTables());

            // get_validation_results
            ToolRegistry.Register("get_validation_results",
                "获取指定节点的最近一次校验结果（需先调用 validate_table 或 validate_document）。返回校验结果项列表，包括每条校验公式的通过状态、左右表达式值、行索引等。当需要复查校验结果或获取详细失败信息时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档或表格节点 ID" }
                    },
                    ["required"] = new JArray { "node_id" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    return ValidationService.GetValidationResults(nodeId);
                });
        }
    }
}
