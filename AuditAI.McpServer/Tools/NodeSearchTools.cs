﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 节点搜索 MCP 工具注册
    /// 提供按关键词和节点类型搜索项目树节点的能力
    /// </summary>
    public static class NodeSearchTools
    {
        /// <summary>
        /// 注册所有节点搜索工具
        /// </summary>
        public static void Register()
        {
            // search_nodes
            ToolRegistry.Register("search_nodes",
                "按关键词和节点类型搜索当前项目的导航树节点。支持按节点名称模糊搜索（不区分大小写），可指定节点类型进行筛选（table/document/directory/image/pdf）。返回匹配节点的 ID、名称、类型、父节点 ID 和完整路径。当用户需要在项目中查找特定表格、文档或目录时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["keyword"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "搜索关键词，按节点名称模糊匹配（不区分大小写），如：\"资产\"、\"底稿\""
                        },
                        ["node_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "节点类型筛选（可选）：table（表格）、document（文档）、directory（目录）、image（图片）、pdf（PDF）。不传则返回所有匹配类型。",
                            ["enum"] = new JArray { "table", "document", "directory", "image", "pdf" }
                        }
                    },
                    ["required"] = new JArray { "keyword" }
                },
                (args) =>
                {
                    string keyword = args["keyword"]?.ToString();
                    string nodeType = args["node_type"]?.ToString();
                    return MiscService.SearchNodes(keyword, nodeType);
                });
        }
    }
}