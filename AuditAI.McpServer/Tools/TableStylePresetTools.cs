using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 表格样式预设 MCP 工具注册
    /// 覆盖 AppCommands.TableStyle0/1/2/3/4/NoLine/Custom/Style/BatchApplyTableStyle 等 UI 操作
    /// </summary>
    public static class TableStylePresetTools
    {
        public static void Register()
        {
            // get_table_style
            ToolRegistry.Register("get_table_style",
                "获取表格的边框样式预设信息（internal_number/各方向线型/是否自定义/自定义JSON）。当需要查看表格边框样式时调用此工具。",
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
                    return TableStylePresetService.GetTableStyle(tid);
                });

            // apply_table_style_preset
            ToolRegistry.Register("apply_table_style_preset",
                "应用预设表格边框样式。覆盖 TableStyle0/Grid, TableStyle1, TableStyle2, TableStyle3, TableStyle4/NoLine, TableStyleCustom。preset 可选: grid(0)/thick_up_down_dash_body(1)/thick_up_down_thin_body(2)/thick_border_thin_body(3)/no_line(4)/custom。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["preset"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "预设样式名",
                            ["enum"] = new JArray { "grid", "thick_up_down_dash_body", "thick_up_down_thin_body", "thick_border_thin_body", "no_line", "custom" }
                        }
                    },
                    ["required"] = new JArray { "table_node_id", "preset" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    string preset = args["preset"]?.ToString() ?? "grid";
                    return TableStylePresetService.ApplyPreset(tid, preset);
                });

            // set_custom_border_style
            ToolRegistry.Register("set_custom_border_style",
                "设置自定义表格边框样式（细粒度配置各方向线型）。覆盖 TableStyle/Custom。line_style 可选: none/thin/thick/dash/dotted/dotdash/doubledotdash。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["up_down_line"] = new JObject { ["type"] = "string", ["description"] = "上下边线型" },
                        ["left_right_line"] = new JObject { ["type"] = "string", ["description"] = "左右边线型" },
                        ["body_line"] = new JObject { ["type"] = "string", ["description"] = "正文线型" },
                        ["second_line"] = new JObject { ["type"] = "string", ["description"] = "分隔线型" },
                        ["keyword_row_bold_underline"] = new JObject { ["type"] = "boolean", ["description"] = "关键词行是否加粗加下划线" },
                        ["keyword_list"] = new JObject { ["type"] = "string", ["description"] = "自定义关键词列表（逗号分隔），默认: 合计,小计,总计,关键词" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    string ud = args["up_down_line"]?.ToString();
                    string lr = args["left_right_line"]?.ToString();
                    string bl = args["body_line"]?.ToString();
                    string sl = args["second_line"]?.ToString();
                    bool kwbu = args["keyword_row_bold_underline"]?.Value<bool>() ?? false;
                    string kwl = args["keyword_list"]?.ToString();
                    return TableStylePresetService.SetCustomBorderStyle(tid, ud, lr, bl, sl, kwbu, kwl);
                });
        }
    }
}
