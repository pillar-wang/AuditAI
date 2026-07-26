using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 页面设置 MCP 工具注册
    /// 覆盖 AppCommands.PaperA4/A3/B4/B5/Custom, Portrait/Landscape, Margin*, Header*, Footer*,
    /// StartPage, ScalePageWidth/Height, WidthScale/HeightScale, FixedColumns, Monochrome, FootBorder 等 UI 操作
    /// </summary>
    public static class PageSetupTools
    {
        public static void Register()
        {
            // get_page_setup
            ToolRegistry.Register("get_page_setup",
                "获取指定表格的页面设置（纸张大小、方向、页边距、页眉页脚、缩放、打印选项等）。当需要查看页面配置时调用此工具。",
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
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    return PageSetupService.GetPageSetup(id);
                });

            // set_paper_size
            ToolRegistry.Register("set_paper_size",
                "设置纸张大小。可选 paper_kind: A4/A3/B4/B5/Custom 等。Custom 时可指定 width 和 height。当需要更改纸张类型时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["paper_kind"] = new JObject { ["type"] = "string", ["description"] = "纸张类型（A4/A3/B4/B5/Custom 等）" },
                        ["width"] = new JObject { ["type"] = "number", ["description"] = "自定义宽度（仅 Custom 时有效，可选）" },
                        ["height"] = new JObject { ["type"] = "number", ["description"] = "自定义高度（仅 Custom 时有效，可选）" }
                    },
                    ["required"] = new JArray { "table_node_id", "paper_kind" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    string pk = args["paper_kind"]?.ToString();
                    double? w = args["width"]?.Value<double>();
                    double? h = args["height"]?.Value<double>();
                    return PageSetupService.SetPaperSize(id, pk, w, h);
                });

            // set_page_orientation
            ToolRegistry.Register("set_page_orientation",
                "设置页面方向（纵向 Portrait / 横向 Landscape）。当需要切换纸张方向时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["direction"] = new JObject { ["type"] = "string", ["description"] = "方向（Portrait/Landscape 或 Vertical/Horizontal）" }
                    },
                    ["required"] = new JArray { "table_node_id", "direction" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    string dir = args["direction"]?.ToString();
                    return PageSetupService.SetPageOrientation(id, dir);
                });

            // set_margins
            ToolRegistry.Register("set_margins",
                "设置页边距（left/right/top/bottom/header/footer）。只更新传入的字段。当需要调整页边距时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["left"] = new JObject { ["type"] = "number", ["description"] = "左边距（可选）" },
                        ["right"] = new JObject { ["type"] = "number", ["description"] = "右边距（可选）" },
                        ["top"] = new JObject { ["type"] = "number", ["description"] = "上边距（可选）" },
                        ["bottom"] = new JObject { ["type"] = "number", ["description"] = "下边距（可选）" },
                        ["header"] = new JObject { ["type"] = "number", ["description"] = "页眉边距（可选）" },
                        ["footer"] = new JObject { ["type"] = "number", ["description"] = "页脚边距（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    double? l = args["left"]?.Value<double>();
                    double? r = args["right"]?.Value<double>();
                    double? t = args["top"]?.Value<double>();
                    double? b = args["bottom"]?.Value<double>();
                    double? h = args["header"]?.Value<double>();
                    double? f = args["footer"]?.Value<double>();
                    return PageSetupService.SetMargins(id, l, r, t, b, h, f);
                });

            // set_header_footer
            ToolRegistry.Register("set_header_footer",
                "设置页眉或页脚内容。position 可选 left/center/right。当需要设置页眉页脚文字时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["position"] = new JObject { ["type"] = "string", ["description"] = "位置（left/center/right）" },
                        ["content"] = new JObject { ["type"] = "string", ["description"] = "文字内容" },
                        ["is_header"] = new JObject { ["type"] = "boolean", ["description"] = "true=页眉, false=页脚" },
                        ["height"] = new JObject { ["type"] = "number", ["description"] = "高度（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id", "position", "content", "is_header" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    string pos = args["position"]?.ToString();
                    string content = args["content"]?.ToString();
                    bool isHeader = args["is_header"]?.Value<bool>() ?? true;
                    double? h = args["height"]?.Value<double>();
                    return PageSetupService.SetHeaderFooter(id, pos, content, isHeader, h);
                });

            // set_print_scale
            ToolRegistry.Register("set_print_scale",
                "设置打印缩放（horizontal_zoom/vertical_zoom）和适应页面（fit_page_width/fit_page_height）。当需要调整打印缩放比例时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["horizontal_zoom"] = new JObject { ["type"] = "number", ["description"] = "水平缩放比例（可选，1.0=100%）" },
                        ["vertical_zoom"] = new JObject { ["type"] = "number", ["description"] = "垂直缩放比例（可选，1.0=100%）" },
                        ["fit_page_width"] = new JObject { ["type"] = "boolean", ["description"] = "是否适应页宽（可选）" },
                        ["fit_page_height"] = new JObject { ["type"] = "boolean", ["description"] = "是否适应页高（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    double? hz = args["horizontal_zoom"]?.Value<double>();
                    double? vz = args["vertical_zoom"]?.Value<double>();
                    bool? fpw = args["fit_page_width"]?.Value<bool>();
                    bool? fph = args["fit_page_height"]?.Value<bool>();
                    return PageSetupService.SetPrintScale(id, hz, vz, fpw, fph);
                });

            // set_print_options
            ToolRegistry.Register("set_print_options",
                "设置打印选项（起始页码/固定打印列数/打印范围/份数/单色/边框/索引）。当需要调整打印选项时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["start_page_no"] = new JObject { ["type"] = "integer", ["description"] = "起始页码（可选）" },
                        ["fixed_print_cols_num"] = new JObject { ["type"] = "integer", ["description"] = "固定打印列数（可选）" },
                        ["print_page_range"] = new JObject { ["type"] = "string", ["description"] = "打印页范围（可选，如 '1-5' 或 '全部'）" },
                        ["print_copies"] = new JObject { ["type"] = "integer", ["description"] = "打印份数（可选）" },
                        ["one_color"] = new JObject { ["type"] = "boolean", ["description"] = "是否单色打印（可选）" },
                        ["has_note_border"] = new JObject { ["type"] = "boolean", ["description"] = "是否显示批注边框（可选）" },
                        ["is_print_index"] = new JObject { ["type"] = "boolean", ["description"] = "是否打印目录索引（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int? spn = args["start_page_no"]?.Value<int>();
                    int? fpc = args["fixed_print_cols_num"]?.Value<int>();
                    string ppr = args["print_page_range"]?.ToString();
                    short? pc = (short?)args["print_copies"]?.Value<int>();
                    bool? oc = args["one_color"]?.Value<bool>();
                    bool? hnb = args["has_note_border"]?.Value<bool>();
                    bool? ipi = args["is_print_index"]?.Value<bool>();
                    return PageSetupService.SetPrintOptions(id, spn, fpc, ppr, pc, oc, hnb, ipi);
                });
        }
    }
}
