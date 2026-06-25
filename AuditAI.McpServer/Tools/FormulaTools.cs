﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 公式管理 MCP 工具注册
    /// 提供单元格公式设置/获取、公式求值、表格重算及依赖关系查询等能力
    /// </summary>
    public static class FormulaTools
    {
        /// <summary>
        /// 注册所有公式管理工具
        /// </summary>
        public static void Register()
        {
            // set_cell_formula
            ToolRegistry.Register("set_cell_formula",
                "设置指定表格中某个单元格的公式，并自动求值、更新依赖图、保存到数据库。公式语法基于 AuditAI 公式引擎（ANTLR4 解析），例如 \"=A1+B1\"、\"SUM(A1:A10)\" 等。传入空字符串可清除公式。当需要为单元格定义计算规则时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID（可通过 get_project_info 获取）" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" },
                        ["formula"] = new JObject { ["type"] = "string", ["description"] = "公式表达式（无需前导 = 号），传空字符串清除公式" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col", "formula" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string formula = args["formula"]?.ToString();
                    return FormulaService.SetCellFormula(tableNodeId, row, col, formula);
                });

            // get_cell_formula
            ToolRegistry.Register("get_cell_formula",
                "获取指定表格中某个单元格的公式信息，包括公式表达式、是否包含公式、表头公式及当前值。当需要查看单元格的公式定义时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return FormulaService.GetCellFormula(tableNodeId, row, col);
                });

            // evaluate_formula
            ToolRegistry.Register("evaluate_formula",
                "在指定表格的上下文中求值公式表达式，返回求值结果及结果类型。不会修改任何单元格，仅用于预览公式计算结果。当需要测试公式或预览计算值时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID（提供公式引用的上下文）" },
                        ["formula"] = new JObject { ["type"] = "string", ["description"] = "要求值的公式表达式" }
                    },
                    ["required"] = new JArray { "table_node_id", "formula" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    string formula = args["formula"]?.ToString();
                    return FormulaService.EvaluateFormula(tableNodeId, formula);
                });

            // calculate_table
            ToolRegistry.Register("calculate_table",
                "重新计算指定表格中的所有公式（列公式、表头公式、单元格公式及控制公式），并返回发生变更的行列表。当修改数据后需要刷新公式结果时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    return FormulaService.CalculateTable(tableNodeId);
                });

            // calculate_all_tables
            ToolRegistry.Register("calculate_all_tables",
                "重新计算当前项目中所有表格的公式。当批量修改数据后需要全局刷新公式结果时调用此工具。返回每个表格的计算状态。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => FormulaService.CalculateAllTables());

            // get_formula_dependencies
            ToolRegistry.Register("get_formula_dependencies",
                "获取指定单元格公式的依赖关系，包括：前置依赖（该单元格公式引用了哪些单元格/列/表头）和反向引用（哪些单元格引用了该单元格）。当需要分析公式引用链、排查循环依赖或理解计算顺序时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return FormulaService.GetFormulaDependencies(tableNodeId, row, col);
                });
        }
    }
}
