using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 文档段落格式 MCP 工具注册
    /// 覆盖 AppCommands.ParagraphAlignLeft/Right/Center/Justify, LineSpacing*, AboveSpacing*, BelowSpacing*,
    /// IndentFirstLine, UnindentFirstLine, IndentParagraph, UnindentParagraph 等 UI 操作
    /// </summary>
    public static class DocumentFormatTools
    {
        public static void Register()
        {
            // get_paragraph_format
            ToolRegistry.Register("get_paragraph_format",
                "获取文档段落的格式信息（对齐、行间距、段前段后、缩进等）。当需要查看段落格式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引（从 0 开始）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    return DocumentFormatService.GetParagraphFormat(id, idx);
                });

            // set_paragraph_alignment
            ToolRegistry.Register("set_paragraph_alignment",
                "设置段落对齐方式。可选: left/center/right/justify/distribute。对应 UI 的左对齐/居中/右对齐/两端对齐/分散对齐。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" },
                        ["alignment"] = new JObject { ["type"] = "string", ["description"] = "对齐方式（left/center/right/justify/distribute）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index", "alignment" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    string align = args["alignment"]?.ToString();
                    return DocumentFormatService.SetParagraphAlignment(id, idx, align);
                });

            // set_paragraph_spacing
            ToolRegistry.Register("set_paragraph_spacing",
                "设置段落间距。可设置行距(line_spacing)、行距规则(line_spacing_rule: auto/exact/atLeast)、段前(space_before)、段后(space_after)。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" },
                        ["line_spacing"] = new JObject { ["type"] = "number", ["description"] = "行距（auto 规则下 1.0=单倍, 1.5=1.5倍, 2.0=双倍）" },
                        ["line_spacing_rule"] = new JObject { ["type"] = "string", ["description"] = "行距规则（auto/exact/atLeast，默认 auto）" },
                        ["space_before"] = new JObject { ["type"] = "number", ["description"] = "段前间距（磅）" },
                        ["space_after"] = new JObject { ["type"] = "number", ["description"] = "段后间距（磅）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    double? ls = args["line_spacing"]?.Value<double>();
                    string rule = args["line_spacing_rule"]?.ToString();
                    double? sb = args["space_before"]?.Value<double>();
                    double? sa = args["space_after"]?.Value<double>();
                    return DocumentFormatService.SetParagraphSpacing(id, idx, ls, rule, sb, sa);
                });

            // set_paragraph_indent
            ToolRegistry.Register("set_paragraph_indent",
                "设置段落缩进。indent_left=左缩进, indent_right=右缩进, indent_first_line=首行缩进（正数=首行缩进, 负数=悬挂缩进）。单位为字符。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" },
                        ["indent_left"] = new JObject { ["type"] = "number", ["description"] = "左缩进（字符数，可选）" },
                        ["indent_right"] = new JObject { ["type"] = "number", ["description"] = "右缩进（字符数，可选）" },
                        ["indent_first_line"] = new JObject { ["type"] = "number", ["description"] = "首行缩进（正数=首行缩进, 负数=悬挂缩进，可选）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    double? il = args["indent_left"]?.Value<double>();
                    double? ir = args["indent_right"]?.Value<double>();
                    double? ifl = args["indent_first_line"]?.Value<double>();
                    return DocumentFormatService.SetParagraphIndent(id, idx, il, ir, ifl);
                });
        }
    }
}
