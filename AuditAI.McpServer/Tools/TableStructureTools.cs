﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 表格结构 MCP 工具注册
    /// 提供表格结构查询、单元格范围读写等能力
    /// </summary>
    public static class TableStructureTools
    {
        /// <summary>
        /// 注册所有表格结构相关工具
        /// </summary>
        public static void Register()
        {
            // get_table_structure
            ToolRegistry.Register("get_table_structure",
                "获取指定表格的结构信息，包括列数、行数、列列表（索引、标题、ID）、合并单元格列表（top_row、left_col、bottom_row、right_col）、行类型分布、是否有公式列。当需要了解表格的总体结构而非具体数据时调用此工具。",
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
                    return TableStructureService.GetTableStructure(tableNodeId);
                });

            // get_cell_range
            ToolRegistry.Register("get_cell_range",
                "获取指定表格中某矩形范围的单元格值及公式。返回二维数组结构，包含范围信息和列标题。当需要批量读取单元格数据时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["start_row"] = new JObject { ["type"] = "integer", ["description"] = "起始行索引（从 0 开始）" },
                        ["end_row"] = new JObject { ["type"] = "integer", ["description"] = "结束行索引（包含）" },
                        ["start_col"] = new JObject { ["type"] = "integer", ["description"] = "起始列索引（从 0 开始）" },
                        ["end_col"] = new JObject { ["type"] = "integer", ["description"] = "结束列索引（包含）" }
                    },
                    ["required"] = new JArray { "table_node_id", "start_row", "end_row", "start_col", "end_col" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int startRow = args["start_row"]?.Value<int>() ?? 0;
                    int endRow = args["end_row"]?.Value<int>() ?? 0;
                    int startCol = args["start_col"]?.Value<int>() ?? 0;
                    int endCol = args["end_col"]?.Value<int>() ?? 0;
                    return TableStructureService.GetCellRange(tableNodeId, startRow, endRow, startCol, endCol);
                });

            // set_cell_range
            ToolRegistry.Register("set_cell_range",
                "批量设置指定表格中某矩形范围的单元格值，并自动保存到数据库。data 参数为二维数组（外层为行，内层为列），值会尝试自动转换为数值、布尔、日期等类型。当需要批量修改单元格内容时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["start_row"] = new JObject { ["type"] = "integer", ["description"] = "起始行索引（从 0 开始）" },
                        ["start_col"] = new JObject { ["type"] = "integer", ["description"] = "起始列索引（从 0 开始）" },
                        ["data"] = new JObject
                        {
                            ["type"] = "array",
                            ["description"] = "要写入的二维数据，外层为行数组，内层为列数组。如：[[\"val1\", \"val2\"], [\"val3\", \"val4\"]]",
                            ["items"] = new JObject
                            {
                                ["type"] = "array"
                            }
                        }
                    },
                    ["required"] = new JArray { "table_node_id", "start_row", "start_col", "data" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int startRow = args["start_row"]?.Value<int>() ?? 0;
                    int startCol = args["start_col"]?.Value<int>() ?? 0;
                    JArray data = args["data"] as JArray;
                    if (data == null && args["data"] != null)
                    {
                        data = JArray.FromObject(args["data"]);
                    }
                    return TableStructureService.SetCellRange(tableNodeId, startRow, startCol, data ?? new JArray());
                });
        }
    }
}