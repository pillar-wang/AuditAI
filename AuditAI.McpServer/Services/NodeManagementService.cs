using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 节点管理扩展服务
    /// 封装节点的上下移、隐藏/显示、编号、权限、详情、按类型列举、路径查询等操作
    /// </summary>
    public static class NodeManagementService
    {
        // =============================================
        // 上下移
        // =============================================

        public static string MoveNodeUp(long nodeId)
        {
            try
            {
                var node = FindNode(nodeId);
                if (!node.CanMoveUp1)
                    return ErrorJson($"节点 {nodeId} 已是第一个兄弟节点，无法上移");
                node.MoveUp1();
                SaveProject();
                return Ok("节点已上移", nodeId);
            }
            catch (Exception ex) { return ErrorJson("上移节点失败: " + ex.Message); }
        }

        public static string MoveNodeDown(long nodeId)
        {
            try
            {
                var node = FindNode(nodeId);
                if (!node.CanMoveDown1)
                    return ErrorJson($"节点 {nodeId} 已是最后一个兄弟节点，无法下移");
                node.MoveDown1();
                SaveProject();
                return Ok("节点已下移", nodeId);
            }
            catch (Exception ex) { return ErrorJson("下移节点失败: " + ex.Message); }
        }

        // =============================================
        // 隐藏/显示
        // =============================================

        public static string SetNodeVisible(long nodeId, bool visible)
        {
            try
            {
                var node = FindNode(nodeId);
                node.UpdateVisible(visible);
                SaveProject();
                return Ok(visible ? "节点已显示" : "节点已隐藏", nodeId, new JObject { ["visible"] = visible });
            }
            catch (Exception ex) { return ErrorJson("设置节点可见性失败: " + ex.Message); }
        }

        // =============================================
        // 节点编号
        // =============================================

        public static string SetNodeNumber(long nodeId, string number)
        {
            try
            {
                var node = FindNode(nodeId);
                node.UpdateNumber(number ?? "");
                SaveProject();
                return Ok("节点编号已设置", nodeId, new JObject { ["number"] = number });
            }
            catch (Exception ex) { return ErrorJson("设置节点编号失败: " + ex.Message); }
        }

        // =============================================
        // 节点权限
        // =============================================

        public static string SetNodePermissions(long nodeId, bool rowRead, bool rowWrite)
        {
            try
            {
                var node = FindNode(nodeId);
                node.UpdateRowRead(rowRead);
                node.UpdateRowWrite(rowWrite);
                SaveProject();
                return Ok("节点权限已设置", nodeId, new JObject { ["row_read"] = rowRead, ["row_write"] = rowWrite });
            }
            catch (Exception ex) { return ErrorJson("设置节点权限失败: " + ex.Message); }
        }

        // =============================================
        // 节点详情
        // =============================================

        public static string GetNodeInfo(long nodeId)
        {
            try
            {
                var node = FindNode(nodeId);
                var result = new JObject
                {
                    ["success"] = true,
                    ["node_id"] = nodeId.ToString(),
                    ["name"] = node.Name,
                    ["type"] = GetNodeTypeName(node),
                    ["number"] = node.Number ?? "",
                    ["visible"] = node.Visible,
                    ["level"] = node.Level,
                    ["is_root"] = node.IsRoot,
                    ["index"] = node.Index,
                    ["row_read"] = node.RowRead,
                    ["row_write"] = node.RowWrite,
                    ["version"] = node.Version,
                    ["parent_id"] = node.Parent != null ? node.Parent.Id.Value.ToString() : "",
                    ["parent_name"] = node.Parent?.Name ?? "",
                    ["group_id"] = node.Group != null ? node.Group.Id.Value.ToString() : "",
                    ["group_name"] = node.Group?.Name ?? "",
                    ["path"] = GetNodePath(node)
                };

                if (node is TreeTableNode tn)
                {
                    result["column_count"] = tn.Table?.Columns?.Count ?? 0;
                    result["row_count"] = tn.Table?.Rows?.Count ?? 0;
                }
                else if (node is TreeDirectoryNode dir)
                {
                    result["child_count"] = dir.Children.Count;
                    var children = new JArray();
                    foreach (var child in dir.Children)
                    {
                        children.Add(new JObject
                        {
                            ["id"] = child.Id.Value.ToString(),
                            ["name"] = child.Name,
                            ["type"] = GetNodeTypeName(child)
                        });
                    }
                    result["children"] = children;
                }
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取节点详情失败: " + ex.Message); }
        }

        // =============================================
        // 按类型列举节点
        // =============================================

        public static string ListNodesByType(string nodeType)
        {
            try
            {
                var project = EnsureProject();
                var allNodes = project.GetAllTreeNodes();
                IEnumerable<TreeNodeBase> filtered = allNodes;

                if (!string.IsNullOrEmpty(nodeType))
                {
                    filtered = allNodes.Where(n => GetNodeTypeName(n) == nodeType.ToLowerInvariant());
                }

                var list = filtered.Select(n => new JObject
                {
                    ["id"] = n.Id.Value.ToString(),
                    ["name"] = n.Name,
                    ["type"] = GetNodeTypeName(n),
                    ["number"] = n.Number ?? "",
                    ["visible"] = n.Visible,
                    ["parent_id"] = n.Parent != null ? n.Parent.Id.Value.ToString() : "",
                    ["parent_name"] = n.Parent?.Name ?? "",
                    ["path"] = GetNodePath(n)
                }).ToList();

                var result = new JObject
                {
                    ["success"] = true,
                    ["filter_type"] = nodeType ?? "all",
                    ["total"] = list.Count,
                    ["nodes"] = new JArray(list)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("按类型列举节点失败: " + ex.Message); }
        }

        // =============================================
        // 节点路径
        // =============================================

        public static string GetNodePathById(long nodeId)
        {
            try
            {
                var node = FindNode(nodeId);
                var result = new JObject
                {
                    ["success"] = true,
                    ["node_id"] = nodeId.ToString(),
                    ["name"] = node.Name,
                    ["path"] = GetNodePath(node),
                    ["path_segments"] = new JArray(GetNodePathSegments(node).ToArray())
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取节点路径失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static Project EnsureProject()
        {
            var project = SessionState.Current.CurrentProject;
            if (project == null)
                throw new InvalidOperationException("未打开项目，请先调用 open_project 工具");
            Project.Current = project;
            return project;
        }

        private static TreeNodeBase FindNode(long nodeId)
        {
            var project = EnsureProject();
            var id = new Id64(nodeId);
            var node = project.GetNodeById(id);
            if (node == null)
                throw new InvalidOperationException($"未找到节点: {nodeId}");
            return node;
        }

        private static void SaveProject()
        {
            var project = SessionState.Current.CurrentProject;
            if (project != null) project.Save();
        }

        private static string GetNodeTypeName(TreeNodeBase node)
        {
            if (node is TreeDirectoryNode) return "directory";
            if (node is TreeTableNode) return "table";
            if (node is TreeDocumentNode) return "document";
            if (node is TreeImageNode) return "image";
            if (node is TreePdfNode) return "pdf";
            return "unknown";
        }

        private static string GetNodePath(TreeNodeBase node)
        {
            return string.Join(" / ", GetNodePathSegments(node));
        }

        private static List<string> GetNodePathSegments(TreeNodeBase node)
        {
            var segs = new List<string>();
            segs.Add(node.Group?.Name ?? "");
            var cur = node;
            while (cur != null)
            {
                segs.Add(cur.Name);
                cur = cur.Parent;
            }
            segs.Reverse();
            return segs;
        }

        private static string Ok(string msg, long nodeId, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["node_id"] = nodeId.ToString(), ["message"] = msg };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
