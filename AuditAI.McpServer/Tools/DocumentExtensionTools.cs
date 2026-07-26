using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 文档扩展 MCP 工具注册
    /// 在 DocumentTools 基础上补充段落级编辑能力：
    /// 设置段落批注、按位置插入段落、删除段落、获取段落信息、获取文档概要
    /// </summary>
    public static class DocumentExtensionTools
    {
        public static void Register()
        {
            // set_paragraph_comment
            ToolRegistry.Register("set_paragraph_comment",
                "为文档中指定段落设置批注内容。当需要为段落添加审计说明/备注时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引（从 0 开始）" },
                        ["comment"] = new JObject { ["type"] = "string", ["description"] = "批注内容" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index", "comment" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    string comment = args["comment"]?.ToString();
                    return DocumentExtensionService.SetParagraphComment(id, idx, comment);
                });

            // insert_paragraph_at
            ToolRegistry.Register("insert_paragraph_at",
                "在文档指定位置插入新段落，原位置及之后的段落索引自动后移。当需要在文档中间插入段落时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["position"] = new JObject { ["type"] = "integer", ["description"] = "插入位置（0=开头，N=末尾追加，N+1 越界）" },
                        ["text"] = new JObject { ["type"] = "string", ["description"] = "段落文本内容" }
                    },
                    ["required"] = new JArray { "document_node_id", "position", "text" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int pos = args["position"]?.Value<int>() ?? 0;
                    string text = args["text"]?.ToString();
                    return DocumentExtensionService.InsertParagraphAt(id, pos, text);
                });

            // delete_paragraph
            ToolRegistry.Register("delete_paragraph",
                "删除文档中指定索引的段落（软删除，保存时生效）。删除后剩余段落会自动重新编号保持连续。当需要删除段落时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "要删除的段落索引" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    return DocumentExtensionService.DeleteParagraph(id, idx);
                });

            // get_paragraph_info
            ToolRegistry.Register("get_paragraph_info",
                "获取文档中指定段落的详细信息：文本内容、批注、段落 ID、状态、文本长度等。当需要查看单个段落详情时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["paragraph_index"] = new JObject { ["type"] = "integer", ["description"] = "段落索引" }
                    },
                    ["required"] = new JArray { "document_node_id", "paragraph_index" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    int idx = args["paragraph_index"]?.Value<int>() ?? 0;
                    return DocumentExtensionService.GetParagraphInfo(id, idx);
                });

            // get_document_info
            ToolRegistry.Register("get_document_info",
                "获取文档概要信息：段落数、总文本长度、批注数、已删除段落数、节点路径，以及前 20 个段落的预览。当需要快速了解文档结构时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" }
                    },
                    ["required"] = new JArray { "document_node_id" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    return DocumentExtensionService.GetDocumentInfo(id);
                });
        }
    }
}
