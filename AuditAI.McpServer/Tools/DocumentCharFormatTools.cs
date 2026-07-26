using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 文档字符格式 MCP 工具注册
    /// 覆盖 AppCommands.DocumentFont/DocumentFontSize/DocForeColor/DocBackColor/
    /// Bold/Italic/Underline/DoubleUnderline/Subscript/Superscript 等 UI 操作
    /// </summary>
    public static class DocumentCharFormatTools
    {
        public static void Register()
        {
            // get_char_format
            ToolRegistry.Register("get_char_format",
                "获取文档段落默认字符格式（字体/字号/粗体/斜体/下划线/颜色/上下标）。当需要查看段落字符格式时调用此工具。",
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
                    long dnid = args["document_node_id"]?.Value<long>() ?? 0;
                    int pi = args["paragraph_index"]?.Value<int>() ?? 0;
                    return DocumentCharFormatService.GetCharFormat(dnid, pi);
                });

            // set_char_format
            ToolRegistry.Register("set_char_format",
                "设置文档段落默认字符格式（一次可设置多个属性）。覆盖 DocumentFont/DocumentFontSize/DocForeColor/DocBackColor/Bold/Italic/Underline/DoubleUnderline/Subscript/Superscript。颜色用十六进制 RGB（如 FF0000 表示红色），underline 可选 none/single/double，vertical_align 可选 baseline/subscript/superscript。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引（从 0 开始）" },
                        ["font_family"] = new JObject { ["type"] = "string", ["description"] = "字体族名（如 宋体/微软雅黑/Arial）" },
                        ["font_size"] = new JObject { ["type"] = "number", ["description"] = "字号（磅，如 12 表示 12pt）" },
                        ["bold"] = new JObject { ["type"] = "boolean", ["description"] = "是否粗体" },
                        ["italic"] = new JObject { ["type"] = "boolean", ["description"] = "是否斜体" },
                        ["underline"] = new JObject { ["type"] = "string", ["description"] = "下划线样式", ["enum"] = new JArray { "none", "single", "double" } },
                        ["fore_color"] = new JObject { ["type"] = "string", ["description"] = "前景色 RGB（如 FF0000 表示红色，不带 #）" },
                        ["back_color"] = new JObject { ["type"] = "string", ["description"] = "背景色 RGB（如 FFFF00 表示黄色，不带 #）" },
                        ["vertical_align"] = new JObject { ["type"] = "string", ["description"] = "垂直对齐", ["enum"] = new JArray { "baseline", "subscript", "superscript" } }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long dnid = args["document_node_id"]?.Value<long>() ?? 0;
                    int pi = args["paragraph_index"]?.Value<int>() ?? 0;
                    string ff = args["font_family"]?.ToString();
                    double? fs = args["font_size"]?.Type == JTokenType.Float ? args["font_size"]?.Value<double>() : (args["font_size"]?.Type == JTokenType.Integer ? (double?)args["font_size"]?.Value<int>() : null);
                    bool? b = args["bold"]?.Value<bool>();
                    bool? it = args["italic"]?.Value<bool>();
                    string u = args["underline"]?.ToString();
                    string fc = args["fore_color"]?.ToString();
                    string bc = args["back_color"]?.ToString();
                    string va = args["vertical_align"]?.ToString();
                    return DocumentCharFormatService.SetCharFormat(dnid, pi, ff, fs, b, it, u, fc, bc, va);
                });
        }
    }
}
