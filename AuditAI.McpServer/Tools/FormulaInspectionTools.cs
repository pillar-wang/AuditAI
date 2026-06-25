﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 公式检查 MCP 工具注册
    /// 提供公式清单提取与公式错误检查等审查能力
    /// </summary>
    public static class FormulaInspectionTools
    {
        /// <summary>
        /// 注册所有公式检查工具
        /// </summary>
        public static void Register()
        {
            // get_all_formulas
            ToolRegistry.Register("get_all_formulas",
                "获取指定表格中所有单元格的公式清单，包含公式表达式、当前计算值、单元格位置等信息。当需要审查表格中所有公式的定义、了解公式分布情况时调用此工具。",
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
                    return FormulaInspectionService.GetAllFormulas(tableNodeId);
                });

            // check_formula_errors
            ToolRegistry.Register("check_formula_errors",
                "检查指定表格中所有公式的错误情况，包括公式求值错误（#REF!、#VALUE!、#DIV/0!、#N/A、#NAME?、#NULL!、#NUM!）和循环引用。返回每个错误单元格的位置、公式、错误类型及描述，以及错误统计汇总。当需要排查表格中的公式错误、定位异常单元格时调用此工具。",
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
                    return FormulaInspectionService.CheckFormulaErrors(tableNodeId);
                });
        }
    }
}