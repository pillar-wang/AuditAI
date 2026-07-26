using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 单元格格式 MCP 工具注册
    /// 提供数据格式、零值格式、批注、样式读取、格式刷、列默认样式、批量样式等能力
    /// </summary>
    public static class CellFormatTools
    {
        public static void Register()
        {
            // set_cell_data_format
            ToolRegistry.Register("set_cell_data_format",
                "设置单元格的数据格式（数字/日期/百分比/货币/文本/布尔等）及小数位数和零值显示。可选 format_type: Text/Number/Percent/Currency/Date/Time/Boolean/Enum 等。可选 zero_format: Zero/Empty/Dash。当需要规范单元格数据格式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["format_type"] = new JObject { ["type"] = "string", ["description"] = "数据格式类型（Text/Number/Percent/Currency/Date/Time/Boolean/Enum）" },
                        ["decimal_length"] = new JObject { ["type"] = "integer", ["description"] = "小数位数（可选，默认 2）" },
                        ["zero_format"] = new JObject { ["type"] = "string", ["description"] = "零值显示格式（可选，Zero/Empty/Dash，默认 Zero）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col", "format_type" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string formatType = args["format_type"]?.ToString();
                    int decimalLength = args["decimal_length"]?.Value<int>() ?? 2;
                    string zeroFormat = args["zero_format"]?.ToString() ?? "zero";
                    return CellFormatService.SetCellDataFormat(id, row, col, formatType, decimalLength, zeroFormat);
                });

            // set_cell_zero_format
            ToolRegistry.Register("set_cell_zero_format",
                "设置单元格的零值显示格式。可选: Zero(显示0)/Empty(显示空)/Dash(显示-)。当需要调整零值显示样式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["zero_format"] = new JObject { ["type"] = "string", ["description"] = "零值格式（Zero/Empty/Dash）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col", "zero_format" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string zeroFormat = args["zero_format"]?.ToString();
                    return CellFormatService.SetCellZeroFormat(id, row, col, zeroFormat);
                });

            // set_cell_comment
            ToolRegistry.Register("set_cell_comment",
                "为单元格设置批注内容（用于审计说明、备注等）。当需要为单元格添加批注时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["comment"] = new JObject { ["type"] = "string", ["description"] = "批注内容" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col", "comment" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string comment = args["comment"]?.ToString();
                    return CellFormatService.SetCellComment(id, row, col, comment);
                });

            // get_cell_comment
            ToolRegistry.Register("get_cell_comment",
                "获取单元格的批注内容。当需要查看单元格批注时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return CellFormatService.GetCellComment(id, row, col);
                });

            // get_cell_style
            ToolRegistry.Register("get_cell_style",
                "获取单元格的完整样式信息：字体、字号、粗体/斜体/下划线、对齐、前景色、背景色、边距、批注、默认值、数据格式等。当需要查看单元格样式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return CellFormatService.GetCellStyle(id, row, col);
                });

            // copy_cell_style
            ToolRegistry.Register("copy_cell_style",
                "复制源单元格样式到目标单元格区域（格式刷功能）。支持将一个单元格的样式批量应用到一片区域。当需要批量复制样式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["src_row"] = new JObject { ["type"] = "integer", ["description"] = "源单元格行索引" },
                        ["src_col"] = new JObject { ["type"] = "integer", ["description"] = "源单元格列索引" },
                        ["dst_start_row"] = new JObject { ["type"] = "integer", ["description"] = "目标区域起始行" },
                        ["dst_end_row"] = new JObject { ["type"] = "integer", ["description"] = "目标区域结束行（包含）" },
                        ["dst_start_col"] = new JObject { ["type"] = "integer", ["description"] = "目标区域起始列" },
                        ["dst_end_col"] = new JObject { ["type"] = "integer", ["description"] = "目标区域结束列（包含）" }
                    },
                    ["required"] = new JArray { "table_node_id", "src_row", "src_col", "dst_start_row", "dst_end_row", "dst_start_col", "dst_end_col" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int sr = args["src_row"]?.Value<int>() ?? 0;
                    int sc = args["src_col"]?.Value<int>() ?? 0;
                    int dr1 = args["dst_start_row"]?.Value<int>() ?? 0;
                    int dr2 = args["dst_end_row"]?.Value<int>() ?? 0;
                    int dc1 = args["dst_start_col"]?.Value<int>() ?? 0;
                    int dc2 = args["dst_end_col"]?.Value<int>() ?? 0;
                    return CellFormatService.CopyCellStyle(id, sr, sc, dr1, dr2, dc1, dc2);
                });

            // set_column_style
            ToolRegistry.Register("set_column_style",
                "设置列的默认样式（用于新增行时自动应用的样式）。样式对象支持字段：font_family/font_size/bold/italic/underline/align/fore_color/back_color/margin/comment/default_value。当需要为整列设置默认样式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["style"] = new JObject
                        {
                            ["type"] = "object",
                            ["description"] = "样式对象",
                            ["properties"] = new JObject
                            {
                                ["font_family"] = new JObject { ["type"] = "string" },
                                ["font_size"] = new JObject { ["type"] = "number" },
                                ["bold"] = new JObject { ["type"] = "boolean" },
                                ["italic"] = new JObject { ["type"] = "boolean" },
                                ["underline"] = new JObject { ["type"] = "boolean" },
                                ["align"] = new JObject { ["type"] = "string", ["description"] = "Left/Center/Right" },
                                ["fore_color"] = new JObject { ["type"] = "string", ["description"] = "颜色值，如 #FF0000 或 Red" },
                                ["back_color"] = new JObject { ["type"] = "string", ["description"] = "颜色值，如 #FFFF00 或 Yellow" },
                                ["margin"] = new JObject { ["type"] = "integer" },
                                ["comment"] = new JObject { ["type"] = "string" },
                                ["default_value"] = new JObject { ["type"] = "string" }
                            }
                        }
                    },
                    ["required"] = new JArray { "table_node_id", "col", "style" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    JObject style = args["style"] as JObject;
                    return CellFormatService.SetColumnStyle(id, col, style);
                });

            // batch_set_cell_style
            ToolRegistry.Register("batch_set_cell_style",
                "批量设置单元格区域的样式。可一次性为一片区域的单元格应用统一样式。样式对象字段与 set_column_style 一致。当需要批量设置样式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["start_row"] = new JObject { ["type"] = "integer", ["description"] = "起始行" },
                        ["end_row"] = new JObject { ["type"] = "integer", ["description"] = "结束行（包含）" },
                        ["start_col"] = new JObject { ["type"] = "integer", ["description"] = "起始列" },
                        ["end_col"] = new JObject { ["type"] = "integer", ["description"] = "结束列（包含）" },
                        ["style"] = new JObject
                        {
                            ["type"] = "object",
                            ["description"] = "样式对象（字段同 set_column_style）",
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
                                ["margin"] = new JObject { ["type"] = "integer" },
                                ["comment"] = new JObject { ["type"] = "string" },
                                ["default_value"] = new JObject { ["type"] = "string" }
                            }
                        }
                    },
                    ["required"] = new JArray { "table_node_id", "start_row", "end_row", "start_col", "end_col", "style" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int sr = args["start_row"]?.Value<int>() ?? 0;
                    int er = args["end_row"]?.Value<int>() ?? 0;
                    int sc = args["start_col"]?.Value<int>() ?? 0;
                    int ec = args["end_col"]?.Value<int>() ?? 0;
                    JObject style = args["style"] as JObject;
                    return CellFormatService.BatchSetCellStyle(id, sr, er, sc, ec, style);
                });
        }
    }
}
