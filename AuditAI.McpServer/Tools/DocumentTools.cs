﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 文档操作 MCP 工具注册
    /// </summary>
    public static class DocumentTools
    {
        /// <summary>
        /// 注册所有文档操作工具
        /// </summary>
        public static void Register()
        {
            // get_document_content
            ToolRegistry.Register("get_document_content",
                "获取指定文档的内容，返回段落列表（含索引、ID、文本、备注）。需要先调用 open_project 打开项目。document_node_id 可通过 get_project_info 获取。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "文档树节点 ID（可通过 get_project_info 获取）"
                        }
                    },
                    ["required"] = new JArray { "document_node_id" }
                },
                (args) =>
                {
                    long documentNodeId = args["document_node_id"]?.Value<long>() ?? 0;
                    return DocumentService.GetDocumentContent(documentNodeId);
                });

            // set_document_content
            ToolRegistry.Register("set_document_content",
                "设置文档内容，将文本按换行拆分为段落并替换原有内容。修改后需调用 save_project 保存。注意：此操作会替换文档原有段落。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "文档树节点 ID"
                        },
                        ["content"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "文档文本内容，按换行符（\\n）拆分为段落"
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "content" }
                },
                (args) =>
                {
                    long documentNodeId = args["document_node_id"]?.Value<long>() ?? 0;
                    string content = args["content"]?.ToString();
                    return DocumentService.SetDocumentContent(documentNodeId, content);
                });

            // add_paragraph
            ToolRegistry.Register("add_paragraph",
                "向文档末尾追加一个段落。修改后需调用 save_project 保存。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "文档树节点 ID"
                        },
                        ["text"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "段落文本内容"
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "text" }
                },
                (args) =>
                {
                    long documentNodeId = args["document_node_id"]?.Value<long>() ?? 0;
                    string text = args["text"]?.ToString();
                    return DocumentService.AddParagraph(documentNodeId, text);
                });

            // export_document
            ToolRegistry.Register("export_document",
                "导出文档为指定格式（docx 或 pdf）。docx 通过文档模型直接生成，pdf 通过 TX TextControl 在 STA 线程上转换。output_path 为导出文件的完整路径。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "文档树节点 ID"
                        },
                        ["format"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "导出格式：docx 或 pdf",
                            ["enum"] = new JArray { "docx", "pdf" }
                        },
                        ["output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "导出文件的完整路径，如 C:\\\\export\\\\audit.docx"
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "format", "output_path" }
                },
                (args) =>
                {
                    long documentNodeId = args["document_node_id"]?.Value<long>() ?? 0;
                    string format = args["format"]?.ToString();
                    string outputPath = args["output_path"]?.ToString();
                    return DocumentService.ExportDocument(documentNodeId, format, outputPath);
                });
        }
    }
}
