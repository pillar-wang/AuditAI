using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 单元格边框 MCP 工具注册
    /// 覆盖 AppCommands.TicketBorderTop/Bottom/Left/Right/None/All/Border1/Border2 等 UI 操作
    /// </summary>
    public static class CellBorderTools
    {
        public static void Register()
        {
            // get_cell_borders
            ToolRegistry.Register("get_cell_borders",
                "获取票据单元格的四条边框配置（top/right/bottom/left 的 width）。当需要查看单元格边框状态时调用此工具。",
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
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return CellBorderService.GetCellBorders(tid, row, col);
                });

            // set_cell_border
            ToolRegistry.Register("set_cell_border",
                "设置票据单元格的单条边框。覆盖 TicketBorderTop/Bottom/Left/Right。width: 0=无边框, 1=细线, 2=中线, 3=粗线。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" },
                        ["edge"] = new JObject { ["type"] = "string", ["description"] = "边框方向：top/right/bottom/left", ["enum"] = new JArray { "top", "right", "bottom", "left" } },
                        ["width"] = new JObject { ["type"] = "integer", ["description"] = "边框宽度：0=无边框, 1=细线, 2=中线, 3=粗线", ["default"] = 1 }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col", "edge" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string edge = args["edge"]?.ToString() ?? "top";
                    int width = args["width"]?.Value<int>() ?? 1;
                    return CellBorderService.SetCellBorder(tid, row, col, edge, width);
                });

            // set_cell_all_borders
            ToolRegistry.Register("set_cell_all_borders",
                "设置票据单元格的所有四条边框（或清除全部）。覆盖 TicketBorderAll（width>0）和 TicketBorderNone（width=0）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" },
                        ["width"] = new JObject { ["type"] = "integer", ["description"] = "边框宽度：0=清除所有, 1=细线, 2=中线, 3=粗线", ["default"] = 1 }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    int width = args["width"]?.Value<int>() ?? 1;
                    return CellBorderService.SetCellAllBorders(tid, row, col, width);
                });

            // set_range_borders
            ToolRegistry.Register("set_range_borders",
                "批量设置矩形区域内所有单元格的边框。覆盖 TicketBorder1（外框线 outline）和 TicketBorder2（内框线 inner）。mode: all=全部框线, none=清除全部, outline=仅外框, inner=仅内框。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["start_row"] = new JObject { ["type"] = "integer", ["description"] = "起始行索引（含）" },
                        ["start_col"] = new JObject { ["type"] = "integer", ["description"] = "起始列索引（含）" },
                        ["end_row"] = new JObject { ["type"] = "integer", ["description"] = "结束行索引（含）" },
                        ["end_col"] = new JObject { ["type"] = "integer", ["description"] = "结束列索引（含）" },
                        ["mode"] = new JObject { ["type"] = "string", ["description"] = "边框模式", ["enum"] = new JArray { "all", "none", "outline", "inner" } },
                        ["width"] = new JObject { ["type"] = "integer", ["description"] = "边框宽度：0=无, 1=细线, 2=中线, 3=粗线", ["default"] = 1 }
                    },
                    ["required"] = new JArray { "table_node_id", "start_row", "start_col", "end_row", "end_col", "mode" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int sr = args["start_row"]?.Value<int>() ?? 0;
                    int sc = args["start_col"]?.Value<int>() ?? 0;
                    int er = args["end_row"]?.Value<int>() ?? 0;
                    int ec = args["end_col"]?.Value<int>() ?? 0;
                    string mode = args["mode"]?.ToString() ?? "all";
                    int width = args["width"]?.Value<int>() ?? 1;
                    return CellBorderService.SetRangeBorders(tid, sr, sc, er, ec, mode, width);
                });
        }
    }
}
