using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 文档插入类 MCP 工具注册
    /// 覆盖 AppCommands.InsertPageBreak / InsertSectionBreak / InsertSymbol /
    /// InsertTextFrame / InsertHeader / InsertFooter / InsertImage / InsertTable
    /// 共 8 个 UI 命令
    /// </summary>
    public static class DocumentInsertTools
    {
        public static void Register()
        {
            // insert_page_break
            ToolRegistry.Register("insert_page_break",
                "在文档指定段落末尾插入分页符。覆盖 AppCommands.InsertPageBreak。当需要在文档中强制分页时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引（在该段落末尾插入分页符）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    return DocumentInsertService.InsertPageBreak(docId, idx);
                });

            // insert_section_break
            ToolRegistry.Register("insert_section_break",
                "在文档指定段落插入分节符。覆盖 AppCommands.InsertSectionBreak。break_type 可选 nextPage(下一页)/continuous(连续)/evenPage(偶数页)/oddPage(奇数页)，默认 nextPage。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" },
                        ["break_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "分节类型：nextPage/continuous/evenPage/oddPage，默认 nextPage",
                            ["enum"] = new JArray { "nextPage", "continuous", "evenPage", "oddPage" }
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    string bt = args["break_type"]?.ToString();
                    return DocumentInsertService.InsertSectionBreak(docId, idx, bt);
                });

            // insert_symbol
            ToolRegistry.Register("insert_symbol",
                "在文档指定段落末尾插入特殊符号（如 ©、®、∞、√ 等）。覆盖 AppCommands.InsertSymbol。可指定字体族（默认宋体）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" },
                        ["symbol"] = new JObject { ["type"] = "string", ["description"] = "要插入的符号字符（如 ©、®、∞、√ 等）" },
                        ["font_family"] = new JObject { ["type"] = "string", ["description"] = "符号字体（默认宋体，特殊符号可使用 Symbol、Wingdings 等）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index", "symbol" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    string sym = args["symbol"]?.ToString();
                    string ff = args["font_family"]?.ToString();
                    return DocumentInsertService.InsertSymbol(docId, idx, sym, ff);
                });

            // insert_text_frame
            ToolRegistry.Register("insert_text_frame",
                "在文档指定段落插入文本框（TextFrame）。覆盖 AppCommands.InsertTextFrame。可指定文本、宽度、高度、偏移量。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" },
                        ["text"] = new JObject { ["type"] = "string", ["description"] = "文本框内容" },
                        ["width"] = new JObject { ["type"] = "integer", ["description"] = "宽度（EMU/像素单位，默认 2000）" },
                        ["height"] = new JObject { ["type"] = "integer", ["description"] = "高度（EMU/像素单位，默认 1000）" },
                        ["offset_x"] = new JObject { ["type"] = "integer", ["description"] = "水平偏移（默认 0）" },
                        ["offset_y"] = new JObject { ["type"] = "integer", ["description"] = "垂直偏移（默认 0）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index", "text" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    string txt = args["text"]?.ToString();
                    int w = args["width"]?.Value<int>() ?? 2000;
                    int h = args["height"]?.Value<int>() ?? 1000;
                    int ox = args["offset_x"]?.Value<int>() ?? 0;
                    int oy = args["offset_y"]?.Value<int>() ?? 0;
                    return DocumentInsertService.InsertTextFrame(docId, idx, txt, w, h, ox, oy);
                });

            // insert_header
            ToolRegistry.Register("insert_header",
                "为文档设置页眉内容。覆盖 AppCommands.InsertHeader。简化实现：将页眉文本保存到第一段批注（[HEADER]文本 格式），客户端可解析显示。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["text"] = new JObject { ["type"] = "string", ["description"] = "页眉文本" },
                        ["header_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "页眉类型：default/first/even，默认 default",
                            ["enum"] = new JArray { "default", "first", "even" }
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "text" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    string txt = args["text"]?.ToString();
                    string ht = args["header_type"]?.ToString();
                    return DocumentInsertService.InsertHeader(docId, txt, ht);
                });

            // insert_footer
            ToolRegistry.Register("insert_footer",
                "为文档设置页脚内容。覆盖 AppCommands.InsertFooter。简化实现：将页脚文本保存到第一段批注（[FOOTER]文本 格式），客户端可解析显示。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["text"] = new JObject { ["type"] = "string", ["description"] = "页脚文本（如 '第 &P 页'）" },
                        ["footer_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "页脚类型：default/first/even，默认 default",
                            ["enum"] = new JArray { "default", "first", "even" }
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "text" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    string txt = args["text"]?.ToString();
                    string ft = args["footer_type"]?.ToString();
                    return DocumentInsertService.InsertFooter(docId, txt, ft);
                });

            // insert_image
            ToolRegistry.Register("insert_image",
                "在文档指定段落插入图片。覆盖 AppCommands.InsertImage。MCP 环境下插入图片占位标记，客户端打开后可替换为实际图片。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" },
                        ["image_path"] = new JObject { ["type"] = "string", ["description"] = "图片文件路径（如 e:\\images\\logo.png）" },
                        ["width"] = new JObject { ["type"] = "integer", ["description"] = "图片显示宽度（像素，默认 200）" },
                        ["height"] = new JObject { ["type"] = "integer", ["description"] = "图片显示高度（像素，默认 150）" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index", "image_path" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    string path = args["image_path"]?.ToString();
                    int w = args["width"]?.Value<int>() ?? 200;
                    int h = args["height"]?.Value<int>() ?? 150;
                    return DocumentInsertService.InsertImage(docId, idx, path, w, h);
                });

            // insert_table_into_document
            ToolRegistry.Register("insert_table_into_document",
                "在文档指定段落位置插入一个 Word 表格（区别于 create_table_node，后者是创建项目树中的表格节点）。覆盖 AppCommands.InsertTable。可指定行数、列数、边框样式。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引（表格将插入到该段落所在位置）" },
                        ["rows"] = new JObject { ["type"] = "integer", ["description"] = "行数（1-100）" },
                        ["cols"] = new JObject { ["type"] = "integer", ["description"] = "列数（1-50）" },
                        ["border_style"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "边框样式：single(单线)/none(无边框)/dashed(虚线)/double(双线)，默认 single",
                            ["enum"] = new JArray { "single", "none", "dashed", "double" }
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index", "rows", "cols" }
                },
                (args) =>
                {
                    long docId = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    int rows = args["rows"]?.Value<int>() ?? 1;
                    int cols = args["cols"]?.Value<int>() ?? 1;
                    string bs = args["border_style"]?.ToString();
                    return DocumentInsertService.InsertTable(docId, idx, rows, cols, bs);
                });
        }
    }
}
