﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 表格管理 MCP 工具注册
    /// 提供表格数据读取、单元格编辑、行列增删、合并、样式、导出等能力
    /// </summary>
    public static class TableTools
    {
        /// <summary>
        /// 注册所有表格管理工具
        /// </summary>
        public static void Register()
        {
            // get_table_data
            ToolRegistry.Register("get_table_data",
                "获取指定表格的数据，返回列标题、行数据、单元格值及合并信息。支持按行列范围查询（可选）。当需要查看表格内容、读取数据或了解表格结构时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID（可通过 get_project_info 获取）" },
                        ["start_row"] = new JObject { ["type"] = "integer", ["description"] = "起始行索引（可选，从 0 开始）" },
                        ["end_row"] = new JObject { ["type"] = "integer", ["description"] = "结束行索引（可选，包含）" },
                        ["start_col"] = new JObject { ["type"] = "integer", ["description"] = "起始列索引（可选，从 0 开始）" },
                        ["end_col"] = new JObject { ["type"] = "integer", ["description"] = "结束列索引（可选，包含）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int? startRow = args["start_row"]?.Type == JTokenType.Null ? (int?)null : args["start_row"]?.Value<int?>();
                    int? endRow = args["end_row"]?.Type == JTokenType.Null ? (int?)null : args["end_row"]?.Value<int?>();
                    int? startCol = args["start_col"]?.Type == JTokenType.Null ? (int?)null : args["start_col"]?.Value<int?>();
                    int? endCol = args["end_col"]?.Type == JTokenType.Null ? (int?)null : args["end_col"]?.Value<int?>();
                    return TableService.GetTableData(tableNodeId, startRow, endRow, startCol, endCol);
                });

            // get_cell_value
            ToolRegistry.Register("get_cell_value",
                "获取指定表格中某个单元格的值、公式和数据类型。当需要读取单个单元格内容时调用此工具。",
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
                    return TableService.GetCellValue(tableNodeId, row, col);
                });

            // set_cell_value
            ToolRegistry.Register("set_cell_value",
                "设置指定表格中某个单元格的值，并自动保存到数据库。值会尝试自动转换为数值、布尔、日期等类型。当需要修改单元格内容时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" },
                        ["value"] = new JObject { ["type"] = "string", ["description"] = "要设置的值（字符串形式，会自动转换类型）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col", "value" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string value = args["value"]?.ToString();
                    return TableService.SetCellValue(tableNodeId, row, col, value);
                });

            // add_table_row
            ToolRegistry.Register("add_table_row",
                "在指定表格中添加一行。可指定插入位置，不指定则追加到末尾。当需要增加表格行时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["position"] = new JObject { ["type"] = "integer", ["description"] = "插入位置（可选，从 0 开始，不指定则追加到末尾）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int? position = args["position"]?.Type == JTokenType.Null ? (int?)null : args["position"]?.Value<int?>();
                    return TableService.AddRow(tableNodeId, position);
                });

            // add_table_column
            ToolRegistry.Register("add_table_column",
                "在指定表格中添加一列。可指定列名和插入位置，不指定位置则追加到末尾。当需要增加表格列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "列标题名称" },
                        ["position"] = new JObject { ["type"] = "integer", ["description"] = "插入位置（可选，从 0 开始，不指定则追加到末尾）" }
                    },
                    ["required"] = new JArray { "table_node_id", "name" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    string name = args["name"]?.ToString();
                    int? position = args["position"]?.Type == JTokenType.Null ? (int?)null : args["position"]?.Value<int?>();
                    return TableService.AddColumn(tableNodeId, name, position);
                });

            // delete_table_row
            ToolRegistry.Register("delete_table_row",
                "删除指定表格中的一行。当需要移除表格行时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "要删除的行索引（从 0 开始）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    return TableService.DeleteRow(tableNodeId, row);
                });

            // delete_table_column
            ToolRegistry.Register("delete_table_column",
                "删除指定表格中的一列。当需要移除表格列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "要删除的列索引（从 0 开始）" }
                    },
                    ["required"] = new JArray { "table_node_id", "col" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return TableService.DeleteColumn(tableNodeId, col);
                });

            // merge_cells
            ToolRegistry.Register("merge_cells",
                "合并指定表格中的矩形区域单元格。合并后仅左上角单元格保留值。当需要合并单元格时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["start_row"] = new JObject { ["type"] = "integer", ["description"] = "起始行索引（从 0 开始）" },
                        ["start_col"] = new JObject { ["type"] = "integer", ["description"] = "起始列索引（从 0 开始）" },
                        ["end_row"] = new JObject { ["type"] = "integer", ["description"] = "结束行索引（包含）" },
                        ["end_col"] = new JObject { ["type"] = "integer", ["description"] = "结束列索引（包含）" }
                    },
                    ["required"] = new JArray { "table_node_id", "start_row", "start_col", "end_row", "end_col" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int startRow = args["start_row"]?.Value<int>() ?? 0;
                    int startCol = args["start_col"]?.Value<int>() ?? 0;
                    int endRow = args["end_row"]?.Value<int>() ?? 0;
                    int endCol = args["end_col"]?.Value<int>() ?? 0;
                    return TableService.MergeCells(tableNodeId, startRow, startCol, endRow, endCol);
                });

            // set_cell_style
            ToolRegistry.Register("set_cell_style",
                "设置指定表格中某个单元格的样式。支持字体、字号、加粗、斜体、下划线、对齐、前景色、背景色等。当需要调整单元格外观时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" },
                        ["style"] = new JObject
                        {
                            ["type"] = "object",
                            ["description"] = "样式对象，支持字段：font_family(字符串)、font_size(数字)、bold(布尔)、italic(布尔)、underline(布尔)、align(TopLeft/MiddleLeft/BottomLeft/TopCenter/MiddleCenter/BottomCenter/TopRight/MiddleRight/BottomRight 或 left/center/right)、fore_color(#RRGGBB 或颜色名)、back_color(#RRGGBB 或颜色名)、margin(整数)",
                            ["properties"] = new JObject
                            {
                                ["font_family"] = new JObject { ["type"] = "string" },
                                ["font_size"] = new JObject { ["type"] = "number" },
                                ["bold"] = new JObject { ["type"] = "boolean" },
                                ["italic"] = new JObject { ["type"] = "boolean" },
                                ["underline"] = new JObject { ["type"] = "boolean" },
                                ["align"] = new JObject { ["type"] = "string" },
                                ["fore_color"] = new JObject { ["type"] = "string" },
                                ["back_color"] = new JObject { ["type"] = "string" },
                                ["margin"] = new JObject { ["type"] = "integer" }
                            }
                        }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col", "style" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    JObject style = args["style"] as JObject;
                    if (style == null && args["style"] != null)
                    {
                        style = JObject.FromObject(args["style"]);
                    }
                    return TableService.SetCellStyle(tableNodeId, row, col, style ?? new JObject());
                });

            // export_table
            ToolRegistry.Register("export_table",
                "将指定表格导出为 Excel 文件（.xlsx）。包含列标题和所有单元格数据。当需要导出表格数据到 Excel 时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["output_path"] = new JObject { ["type"] = "string", ["description"] = "输出文件路径（.xlsx 后缀）" }
                    },
                    ["required"] = new JArray { "table_node_id", "output_path" }
                },
                (args) =>
                {
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    string outputPath = args["output_path"]?.ToString();
                    return TableService.ExportTable(tableNodeId, outputPath);
                });
        }
    }
}
