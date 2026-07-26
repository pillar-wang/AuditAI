using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 节点管理扩展 MCP 工具注册
    /// 提供节点的上下移、隐藏/显示、编号、权限、详情、按类型列举、路径查询等能力
    /// </summary>
    public static class NodeManagementTools
    {
        public static void Register()
        {
            // move_node_up
            ToolRegistry.Register("move_node_up",
                "将节点在同级兄弟节点中上移一位。如果已是第一个兄弟节点则返回错误。当需要调整节点顺序时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "要上移的节点 ID" }
                    },
                    ["required"] = new JArray { "node_id" }
                },
                (args) =>
                {
                    long id = args["node_id"]?.Value<long>() ?? 0;
                    return NodeManagementService.MoveNodeUp(id);
                });

            // move_node_down
            ToolRegistry.Register("move_node_down",
                "将节点在同级兄弟节点中下移一位。如果已是最后一个兄弟节点则返回错误。当需要调整节点顺序时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "要下移的节点 ID" }
                    },
                    ["required"] = new JArray { "node_id" }
                },
                (args) =>
                {
                    long id = args["node_id"]?.Value<long>() ?? 0;
                    return NodeManagementService.MoveNodeDown(id);
                });

            // set_node_visible
            ToolRegistry.Register("set_node_visible",
                "设置节点的可见性（隐藏/显示）。隐藏后节点在导航树中不可见。当需要隐藏或显示节点时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "节点 ID" },
                        ["visible"] = new JObject { ["type"] = "boolean", ["description"] = "true=显示，false=隐藏" }
                    },
                    ["required"] = new JArray { "node_id", "visible" }
                },
                (args) =>
                {
                    long id = args["node_id"]?.Value<long>() ?? 0;
                    bool visible = args["visible"]?.Value<bool>() ?? true;
                    return NodeManagementService.SetNodeVisible(id, visible);
                });

            // set_node_number
            ToolRegistry.Register("set_node_number",
                "设置节点的编号（用于显示在导航树节点前的序号/编号）。当需要为节点设置编号时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "节点 ID" },
                        ["number"] = new JObject { ["type"] = "string", ["description"] = "节点编号（如 1.1、2.3.1）" }
                    },
                    ["required"] = new JArray { "node_id", "number" }
                },
                (args) =>
                {
                    long id = args["node_id"]?.Value<long>() ?? 0;
                    string number = args["number"]?.ToString();
                    return NodeManagementService.SetNodeNumber(id, number);
                });

            // set_node_permissions
            ToolRegistry.Register("set_node_permissions",
                "设置节点的行级权限（row_read=只读、row_write=可写）。用于控制不同用户对节点的访问权限。当需要配置节点权限时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "节点 ID" },
                        ["row_read"] = new JObject { ["type"] = "boolean", ["description"] = "是否可读" },
                        ["row_write"] = new JObject { ["type"] = "boolean", ["description"] = "是否可写" }
                    },
                    ["required"] = new JArray { "node_id", "row_read", "row_write" }
                },
                (args) =>
                {
                    long id = args["node_id"]?.Value<long>() ?? 0;
                    bool rowRead = args["row_read"]?.Value<bool>() ?? false;
                    bool rowWrite = args["row_write"]?.Value<bool>() ?? false;
                    return NodeManagementService.SetNodePermissions(id, rowRead, rowWrite);
                });

            // get_node_info
            ToolRegistry.Register("get_node_info",
                "获取节点的完整信息：名称、类型、编号、可见性、层级、是否根节点、索引、权限、版本、父节点、所在分组、完整路径。表格节点还会返回行列数，目录节点还会返回子节点列表。当需要全面了解节点信息时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "节点 ID" }
                    },
                    ["required"] = new JArray { "node_id" }
                },
                (args) =>
                {
                    long id = args["node_id"]?.Value<long>() ?? 0;
                    return NodeManagementService.GetNodeInfo(id);
                });

            // list_nodes_by_type
            ToolRegistry.Register("list_nodes_by_type",
                "按类型列举项目中的所有节点。可选类型: directory(目录)/table(表格)/document(文档)/image(图片)/pdf(PDF)。不传则返回所有节点。当需要按类型筛选节点时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "节点类型筛选（可选）：directory/table/document/image/pdf。不传则返回所有节点"
                        }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string nodeType = args["node_type"]?.ToString();
                    return NodeManagementService.ListNodesByType(nodeType);
                });

            // get_node_path_by_id
            ToolRegistry.Register("get_node_path_by_id",
                "根据节点 ID 获取节点的完整路径（分组名 / 父节点 / ... / 当前节点）和路径分段数组。当需要定位节点位置时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject { ["type"] = "integer", ["description"] = "节点 ID" }
                    },
                    ["required"] = new JArray { "node_id" }
                },
                (args) =>
                {
                    long id = args["node_id"]?.Value<long>() ?? 0;
                    return NodeManagementService.GetNodePathById(id);
                });
        }
    }
}
