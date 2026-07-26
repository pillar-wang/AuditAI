using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 快照与回收站管理 MCP 工具注册
    /// 覆盖 AppCommands.ManageSnapshots / AppCommands.RecycleNode 等 UI 操作
    /// </summary>
    public static class SnapshotTools
    {
        public static void Register()
        {
            // list_snapshots
            ToolRegistry.Register("list_snapshots",
                "列举指定节点的所有历史快照。当需要查看节点历史版本或审计快照时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["tree_node_id"] = new JObject { ["type"] = "integer", ["description"] = "树节点 ID（表格/文档/图片/PDF 节点）" }
                    },
                    ["required"] = new JArray { "tree_node_id" }
                },
                (args) =>
                {
                    long id = args["tree_node_id"]?.Value<long>() ?? 0;
                    return SnapshotService.ListSnapshots(id);
                });

            // create_snapshot
            ToolRegistry.Register("create_snapshot",
                "为指定节点创建当前内容的快照（版本备份）。支持表格/文档/图片/PDF 节点。当需要保存当前版本以便后续恢复时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["tree_node_id"] = new JObject { ["type"] = "integer", ["description"] = "树节点 ID" }
                    },
                    ["required"] = new JArray { "tree_node_id" }
                },
                (args) =>
                {
                    long id = args["tree_node_id"]?.Value<long>() ?? 0;
                    return SnapshotService.CreateSnapshot(id);
                });

            // restore_snapshot
            ToolRegistry.Register("restore_snapshot",
                "将指定节点恢复到历史快照版本。恢复前会自动为当前内容创建快照防止数据丢失。当需要回退节点内容到历史版本时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["snapshot_id"] = new JObject { ["type"] = "integer", ["description"] = "快照 ID" },
                        ["tree_node_id"] = new JObject { ["type"] = "integer", ["description"] = "目标树节点 ID" }
                    },
                    ["required"] = new JArray { "snapshot_id", "tree_node_id" }
                },
                (args) =>
                {
                    int sid = args["snapshot_id"]?.Value<int>() ?? 0;
                    long nid = args["tree_node_id"]?.Value<long>() ?? 0;
                    return SnapshotService.RestoreSnapshot(sid, nid);
                });

            // delete_snapshot
            ToolRegistry.Register("delete_snapshot",
                "删除指定快照。当需要清理历史版本时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["snapshot_id"] = new JObject { ["type"] = "integer", ["description"] = "快照 ID" }
                    },
                    ["required"] = new JArray { "snapshot_id" }
                },
                (args) =>
                {
                    int sid = args["snapshot_id"]?.Value<int>() ?? 0;
                    return SnapshotService.DeleteSnapshot(sid);
                });

            // list_recycled_nodes
            ToolRegistry.Register("list_recycled_nodes",
                "列举项目回收站中所有被删除的节点。当需要查看回收站内容或恢复已删除节点时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject {}
                },
                (args) => SnapshotService.ListRecycledNodes());

            // restore_recycled_node
            ToolRegistry.Register("restore_recycled_node",
                "从回收站恢复已删除的节点到指定父目录下。当需要恢复误删的节点时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["snapshot_id"] = new JObject { ["type"] = "integer", ["description"] = "回收站快照 ID" },
                        ["parent_node_id"] = new JObject { ["type"] = "integer", ["description"] = "恢复到的父目录节点 ID" }
                    },
                    ["required"] = new JArray { "snapshot_id", "parent_node_id" }
                },
                (args) =>
                {
                    int sid = args["snapshot_id"]?.Value<int>() ?? 0;
                    long pid = args["parent_node_id"]?.Value<long>() ?? 0;
                    return SnapshotService.RestoreRecycledNode(sid, pid);
                });

            // purge_recycled_node
            ToolRegistry.Register("purge_recycled_node",
                "彻底删除回收站中的指定节点（不可恢复）。当确定不再需要某节点时调用此工具释放空间。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["snapshot_id"] = new JObject { ["type"] = "integer", ["description"] = "回收站快照 ID" }
                    },
                    ["required"] = new JArray { "snapshot_id" }
                },
                (args) =>
                {
                    int sid = args["snapshot_id"]?.Value<int>() ?? 0;
                    return SnapshotService.PurgeRecycledNode(sid);
                });
        }
    }
}
