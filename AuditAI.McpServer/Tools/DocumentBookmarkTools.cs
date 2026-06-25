﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 文档书签 MCP 工具注册
    /// </summary>
    public static class DocumentBookmarkTools
    {
        /// <summary>
        /// 注册所有文档书签工具
        /// </summary>
        public static void Register()
        {
            // get_document_bookmarks
            ToolRegistry.Register("get_document_bookmarks",
                "获取指定文档中所有书签的信息。返回书签列表，包含书签名称、所在段落索引和书签ID。需要先调用 open_project 打开项目。document_node_id 可通过 get_project_info 获取。",
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
                    return DocumentBookmarkService.GetDocumentBookmarks(documentNodeId);
                });

            // set_bookmark_text
            ToolRegistry.Register("set_bookmark_text",
                "设置指定文档中书签位置的文本内容。根据书签名称找到书签位置，替换其中的文本。修改后需调用 save_project 保存。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "文档树节点 ID（可通过 get_project_info 获取）"
                        },
                        ["bookmark_name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "书签名称"
                        },
                        ["text"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "要设置的文本内容"
                        }
                    },
                    ["required"] = new JArray { "document_node_id", "bookmark_name", "text" }
                },
                (args) =>
                {
                    long documentNodeId = args["document_node_id"]?.Value<long>() ?? 0;
                    string bookmarkName = args["bookmark_name"]?.Value<string>() ?? "";
                    string text = args["text"]?.Value<string>() ?? "";
                    return DocumentBookmarkService.SetBookmarkText(documentNodeId, bookmarkName, text);
                });
        }
    }
}