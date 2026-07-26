using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 票据（TicketTable）管理 MCP 工具注册
    /// 覆盖 AppCommands.TicketAdd / TicketDelete / TicketSave / TicketPrevious / TicketNext / TicketMode 等 UI 操作
    /// </summary>
    public static class TicketTools
    {
        public static void Register()
        {
            // get_ticket_info
            ToolRegistry.Register("get_ticket_info",
                "获取指定表格的票据配置信息（类型/级别/数据行范围/冻结/列行数/记录数等）。当需要查看票据整体结构时调用此工具。",
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
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    return TicketService.GetTicketInfo(tid);
                });

            // get_ticket_records
            ToolRegistry.Register("get_ticket_records",
                "获取票据记录列表（分页）。每条记录包含行索引、行名、行高。当需要查看票据数据记录时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["offset"] = new JObject { ["type"] = "integer", ["description"] = "起始偏移量（默认 0）" },
                        ["limit"] = new JObject { ["type"] = "integer", ["description"] = "返回最大条数（默认 50）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int offset = args["offset"]?.Value<int>() ?? 0;
                    int limit = args["limit"]?.Value<int>() ?? 50;
                    return TicketService.GetTicketRecords(tid, offset, limit);
                });

            // set_ticket_kind
            ToolRegistry.Register("set_ticket_kind",
                "设置票据类型（TicketKind）。可选值：None/FixedOneRow/FixedMultiRow/DynamicRow/FixedDataRowMixDynamicDataRow。当需要切换票据结构模式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["kind"] = new JObject { ["type"] = "string", ["description"] = "票据类型名称（不区分大小写）" }
                    },
                    ["required"] = new JArray { "table_node_id", "kind" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    string kind = args["kind"]?.ToString();
                    return TicketService.SetTicketKind(tid, kind);
                });

            // set_ticket_level
            ToolRegistry.Register("set_ticket_level",
                "设置票据级别（TicketLevel）。可选值：None/Receipt/Report。当需要区分表单/报表级别时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["level"] = new JObject { ["type"] = "string", ["description"] = "票据级别名称（不区分大小写）" }
                    },
                    ["required"] = new JArray { "table_node_id", "level" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    string level = args["level"]?.ToString();
                    return TicketService.SetTicketLevel(tid, level);
                });

            // set_ticket_data_row_height
            ToolRegistry.Register("set_ticket_data_row_height",
                "设置票据数据行的默认行高（像素）。最小值 1。当需要调整票据数据行显示高度时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["height"] = new JObject { ["type"] = "integer", ["description"] = "行高（像素）" }
                    },
                    ["required"] = new JArray { "table_node_id", "height" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int h = args["height"]?.Value<int>() ?? 36;
                    return TicketService.SetTicketDataRowHeight(tid, h);
                });

            // set_ticket_frozen
            ToolRegistry.Register("set_ticket_frozen",
                "设置票据冻结行/列数。可单独设置行或列，未提供的参数保持原值。当需要固定表头或左侧列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["rows_frozen"] = new JObject { ["type"] = "integer", ["description"] = "冻结行数（可选）" },
                        ["cols_frozen"] = new JObject { ["type"] = "integer", ["description"] = "冻结列数（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int? rows = args["rows_frozen"]?.Value<int>();
                    int? cols = args["cols_frozen"]?.Value<int>();
                    return TicketService.SetTicketFrozen(tid, rows, cols);
                });
        }
    }
}
