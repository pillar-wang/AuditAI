﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 跨项目管理 MCP 工具注册
    /// 提供项目合并、跨项目数据引用管理及跨项目公式求值等能力
    /// </summary>
    public static class CrossProjectTools
    {
        /// <summary>
        /// 注册所有跨项目管理工具
        /// </summary>
        public static void Register()
        {
            // consolidate_projects
            ToolRegistry.Register("consolidate_projects",
                "合并多个项目生成合并工作底稿。支持两种合并模式：group_summary（分组汇总，按表格名称聚合相同结构的数值列并求和）和 row_append（行追加，将各项目的数据行依次追加）。传入要合并的项目 ID 列表（Guid 字符串）和合并模式。当需要编制集团合并报表、汇总多个子公司的审计数据时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_ids"] = new JObject
                        {
                            ["type"] = "array",
                            ["description"] = "要合并的项目 ID 列表（Guid 字符串数组，可通过 list_projects 获取）",
                            ["items"] = new JObject { ["type"] = "string" }
                        },
                        ["merge_mode"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "合并模式：group_summary（分组汇总，按表名聚合数值列求和）或 row_append（行追加，依次追加所有数据行）",
                            ["enum"] = new JArray { "group_summary", "row_append" }
                        }
                    },
                    ["required"] = new JArray { "project_ids", "merge_mode" }
                },
                (args) =>
                {
                    JArray projectIds = args["project_ids"] as JArray;
                    if (projectIds == null && args["project_ids"] != null)
                    {
                        projectIds = new JArray(args["project_ids"]);
                    }
                    string mergeMode = args["merge_mode"]?.ToString();
                    return CrossProjectService.ConsolidateProjects(projectIds, mergeMode);
                });

            // set_cross_project_reference
            ToolRegistry.Register("set_cross_project_reference",
                "设置跨项目数据引用关系，建立从来源项目到当前（目标）项目的数据引用。引用关系会持久化到当前项目的跨项目引用配置表中，可用于后续的跨项目取数和公式运算。调用前需先打开目标项目（open_project）。当需要引用其他项目的数据（如母公司引用子公司数据）时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["source_project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "来源项目 ID（Guid 字符串，数据来源项目）"
                        },
                        ["target_project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "目标项目 ID（Guid 字符串，应为当前已打开的项目）"
                        },
                        ["formula"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "引用公式表达式，定义如何从来源项目取数（如 [来源项目Guid.表ID.列ID] 形式的引用）"
                        }
                    },
                    ["required"] = new JArray { "source_project_id", "target_project_id", "formula" }
                },
                (args) =>
                {
                    string sourceProjectId = args["source_project_id"]?.ToString();
                    string targetProjectId = args["target_project_id"]?.ToString();
                    string formula = args["formula"]?.ToString();
                    return CrossProjectService.SetCrossProjectReference(sourceProjectId, targetProjectId, formula);
                });

            // get_cross_project_reference
            ToolRegistry.Register("get_cross_project_reference",
                "获取指定项目的所有跨项目引用关系，包括数据引用（CrossProjectDataRef）和公式引用（CrossProjectFormula）。返回每条引用的来源项目、目标表、引用模式、公式表达式等详细信息。调用前需先打开目标项目。当需要查看项目依赖了哪些外部项目数据、或排查跨项目引用问题时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "项目 ID（Guid 字符串，应为当前已打开的项目）"
                        }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    string projectId = args["project_id"]?.ToString();
                    return CrossProjectService.GetCrossProjectReference(projectId);
                });

            // evaluate_cross_project_formula
            ToolRegistry.Register("evaluate_cross_project_formula",
                "在跨项目上下文中求值公式表达式。会先尝试匹配已注册的跨项目公式（CrossProjectFormula），若匹配则返回其求值结果；否则执行基本求值：解析公式中的项目 Guid 引用，打开对应项目汇总数值后代入公式计算。调用前需先打开当前项目。当需要测试跨项目取数公式或预览跨项目计算结果时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "项目 ID（Guid 字符串，提供求值上下文的当前项目）"
                        },
                        ["formula"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "要求值的跨项目公式表达式（如 [来源项目Guid] + [另一项目Guid] 形式的引用）"
                        }
                    },
                    ["required"] = new JArray { "project_id", "formula" }
                },
                (args) =>
                {
                    string projectId = args["project_id"]?.ToString();
                    string formula = args["formula"]?.ToString();
                    return CrossProjectService.EvaluateCrossProjectFormula(projectId, formula);
                });
        }
    }
}
