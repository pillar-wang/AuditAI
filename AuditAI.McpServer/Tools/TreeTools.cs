﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 导航树 MCP 工具注册
    /// 向 AI 客户端暴露项目导航树的查询与节点增删改查能力
    /// </summary>
    public static class TreeTools
    {
        /// <summary>
        /// 注册所有导航树工具
        /// </summary>
        public static void Register()
        {
            // get_project_tree
            ToolRegistry.Register("get_project_tree",
                "获取当前项目的完整导航树结构。返回所有分组（TreeGroup）及其下的层级节点（目录、表格、文档等），包含节点 ID、名称、类型、子节点等信息。当用户想查看项目结构时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => TreeService.GetProjectTree());

            // create_directory_node
            ToolRegistry.Register("create_directory_node",
                "在导航树中创建目录节点。parent_id 可指向分组（TreeGroup）ID（创建根目录）或目录节点（TreeDirectoryNode）ID（创建子目录）。需要提供目录名称。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["parent_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "父节点 ID。传入分组 ID 时创建根目录，传入目录节点 ID 时创建子目录"
                        },
                        ["name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "目录名称，如：审计底稿、凭证抽查"
                        }
                    },
                    ["required"] = new JArray { "parent_id", "name" }
                },
                (args) =>
                {
                    long parentId = args["parent_id"]?.Value<long>() ?? 0;
                    string name = args["name"]?.ToString();
                    return TreeService.CreateDirectoryNode(parentId, name);
                });

            // create_document_node
            ToolRegistry.Register("create_document_node",
                "在导航树中创建文档节点。文档节点用于承载文字内容（类似 Word 文档）。parent_id 可指向分组 ID（创建根文档）或目录节点 ID（创建子文档）。需要提供文档名称。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["parent_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "父节点 ID。传入分组 ID 时创建根文档，传入目录节点 ID 时创建子文档"
                        },
                        ["name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "文档名称，如：审计报告、管理建议书"
                        }
                    },
                    ["required"] = new JArray { "parent_id", "name" }
                },
                (args) =>
                {
                    long parentId = args["parent_id"]?.Value<long>() ?? 0;
                    string name = args["name"]?.ToString();
                    return TreeService.CreateDocumentNode(parentId, name);
                });

            // create_table_node
            ToolRegistry.Register("create_table_node",
                "在导航树中创建表格节点。表格节点用于承载结构化数据（类似 Excel 工作表）。可通过 columns 参数定义列结构。parent_id 可指向分组 ID（创建根表格）或目录节点 ID（创建子表格）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["parent_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "父节点 ID。传入分组 ID 时创建根表格，传入目录节点 ID 时创建子表格"
                        },
                        ["name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "表格名称，如：资产负债表、利润表"
                        },
                        ["columns"] = new JObject
                        {
                            ["type"] = "array",
                            ["description"] = "列定义数组。元素可为字符串（列标题）或对象（含 name/caption 和可选的 width 字段）。如不提供则使用默认列。",
                            ["items"] = new JObject
                            {
                                ["oneOf"] = new JArray
                                {
                                    new JObject { ["type"] = "string" },
                                    new JObject
                                    {
                                        ["type"] = "object",
                                        ["properties"] = new JObject
                                        {
                                            ["name"] = new JObject { ["type"] = "string", ["description"] = "列标题" },
                                            ["caption"] = new JObject { ["type"] = "string", ["description"] = "列标题（与 name 等效）" },
                                            ["width"] = new JObject { ["type"] = "integer", ["description"] = "列宽（像素）" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    ["required"] = new JArray { "parent_id", "name" }
                },
                (args) =>
                {
                    long parentId = args["parent_id"]?.Value<long>() ?? 0;
                    string name = args["name"]?.ToString();
                    JArray columns = args["columns"] as JArray;
                    return TreeService.CreateTableNode(parentId, name, columns);
                });

            // delete_node
            ToolRegistry.Register("delete_node",
                "删除导航树中的指定节点。支持删除目录、表格、文档等所有类型的节点。删除目录时会递归删除其下所有子节点。删除操作不可撤销，请谨慎使用。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "要删除的节点 ID"
                        }
                    },
                    ["required"] = new JArray { "node_id" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    return TreeService.DeleteNode(nodeId);
                });

            // move_node
            ToolRegistry.Register("move_node",
                "将节点移动到新的父节点下。new_parent_id 可指向分组 ID（移动为根节点）或目录节点 ID（移动为子节点）。不能将节点移动到自身或其子节点下。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "要移动的节点 ID"
                        },
                        ["new_parent_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "目标父节点 ID。传入分组 ID 时移动为根节点，传入目录节点 ID 时移动为子节点"
                        }
                    },
                    ["required"] = new JArray { "node_id", "new_parent_id" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    long newParentId = args["new_parent_id"]?.Value<long>() ?? 0;
                    return TreeService.MoveNode(nodeId, newParentId);
                });

            // copy_node
            ToolRegistry.Register("copy_node",
                "复制节点到新的父节点下。支持目录（递归深拷贝）、表格、文档。复制后的节点名称会追加\" (副本)\"后缀。new_parent_id 可指向分组 ID 或目录节点 ID。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "要复制的源节点 ID"
                        },
                        ["new_parent_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "目标父节点 ID。传入分组 ID 时复制为根节点，传入目录节点 ID 时复制为子节点"
                        }
                    },
                    ["required"] = new JArray { "node_id", "new_parent_id" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    long newParentId = args["new_parent_id"]?.Value<long>() ?? 0;
                    return TreeService.CopyNode(nodeId, newParentId);
                });

            // rename_node
            ToolRegistry.Register("rename_node",
                "重命名导航树中的指定节点。支持所有节点类型（目录、表格、文档等）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "要重命名的节点 ID"
                        },
                        ["new_name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "新的节点名称"
                        }
                    },
                    ["required"] = new JArray { "node_id", "new_name" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    string newName = args["new_name"]?.ToString();
                    return TreeService.RenameNode(nodeId, newName);
                });
        }
    }
}
