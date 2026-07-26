using System;
using System.Collections.Generic;
using System.IO;
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
    /// 快照与回收站管理服务
    /// 封装快照列举、创建、恢复、删除以及回收站列举、恢复、彻底删除等操作
    /// </summary>
    public static class SnapshotService
    {
        // =============================================
        // 快照列举
        // =============================================

        public static string ListSnapshots(long treeNodeId)
        {
            try
            {
                var project = EnsureProject();
                var node = FindNode(project, treeNodeId);
                var snapshots = project.SnapshotManager.GetSnapshots(node);

                var arr = new JArray(snapshots.Select(s => new JObject
                {
                    ["id"] = s.Id,
                    ["name"] = s.Name ?? "",
                    ["kind"] = GetSnapshotKindName(s.Kind),
                    ["kind_code"] = s.Kind,
                    ["date_time"] = s.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["size"] = s.Size,
                    ["tree_node_id"] = s.TreeNodeId.Value.ToString(),
                    ["deleted"] = s.Deleted
                }));

                var result = new JObject
                {
                    ["success"] = true,
                    ["tree_node_id"] = treeNodeId.ToString(),
                    ["node_name"] = node.Name,
                    ["total"] = snapshots.Count,
                    ["snapshots"] = arr
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("列举快照失败: " + ex.Message); }
        }

        // =============================================
        // 创建快照
        // =============================================

        public static string CreateSnapshot(long treeNodeId)
        {
            try
            {
                var project = EnsureProject();
                var node = FindNode(project, treeNodeId);

                if (node is TreeTableNode tn)
                {
                    tn.Table.LoadAndReturn(true);
                    project.SnapshotManager.SaveSnapshot(tn.Table, false);
                    project.Save();
                    return Ok($"表格 '{tn.Name}' 快照已创建", treeNodeId);
                }
                if (node is TreeDocumentNode dn)
                {
                    dn.Document.LoadAndReturn();
                    project.SnapshotManager.SaveSnapshot(dn.Document, false);
                    project.Save();
                    return Ok($"文档 '{dn.Name}' 快照已创建", treeNodeId);
                }
                if (node is TreeImageNode img)
                {
                    project.SnapshotManager.SaveSnapshot(img.Image, false);
                    project.Save();
                    return Ok($"图片 '{img.Name}' 快照已创建", treeNodeId);
                }
                if (node is TreePdfNode pdf)
                {
                    project.SnapshotManager.SaveSnapshot(pdf.Pdf, false);
                    project.Save();
                    return Ok($"PDF '{pdf.Name}' 快照已创建", treeNodeId);
                }
                return ErrorJson($"节点 {treeNodeId} 不是表格/文档/图片/PDF 节点，无法创建快照");
            }
            catch (Exception ex) { return ErrorJson("创建快照失败: " + ex.Message); }
        }

        // =============================================
        // 恢复快照
        // =============================================

        public static string RestoreSnapshot(int snapshotId, long treeNodeId)
        {
            try
            {
                var project = EnsureProject();
                var node = FindNode(project, treeNodeId);
                var snapshots = project.SnapshotManager.GetSnapshots(node);
                var target = snapshots.FirstOrDefault(s => s.Id == snapshotId);
                if (target == null)
                    return ErrorJson($"未找到快照 ID={snapshotId}（节点 {treeNodeId}）");

                // 恢复前先为当前内容创建一份快照（防止数据丢失）
                if (node is TreeTableNode tn)
                {
                    tn.Table.LoadAndReturn(true);
                    project.SnapshotManager.SaveSnapshot(tn.Table, false);
                    var restored = project.SnapshotManager.GetSnapshotTable(target);
                    tn.SetTable(restored.Table);
                    tn.Dirty = 1;
                    project.Save();
                    return Ok($"已恢复表格快照 #{snapshotId}", treeNodeId, new JObject { ["snapshot_id"] = snapshotId });
                }
                if (node is TreeDocumentNode dn)
                {
                    dn.Document.LoadAndReturn();
                    project.SnapshotManager.SaveSnapshot(dn.Document, false);
                    var restored = project.SnapshotManager.GetSnapshotDocument(target);
                    // 文档没有 setter，需手动复制段落与节属性
                    dn.Document.Paragraphs.Clear();
                    foreach (var p in restored.Document.Paragraphs)
                    {
                        p.Document = dn.Document;
                        dn.Document.Paragraphs.Add(p);
                    }
                    dn.Document.SectPr = restored.Document.SectPr;
                    dn.Document.MergeTable = restored.Document.MergeTable;
                    dn.Dirty = 1;
                    project.Save();
                    return Ok($"已恢复文档快照 #{snapshotId}", treeNodeId, new JObject { ["snapshot_id"] = snapshotId });
                }
                return ErrorJson($"节点 {treeNodeId} 类型不支持快照恢复（仅支持表格/文档）");
            }
            catch (Exception ex) { return ErrorJson("恢复快照失败: " + ex.Message); }
        }

        // =============================================
        // 删除快照
        // =============================================

        public static string DeleteSnapshot(int snapshotId)
        {
            try
            {
                var project = EnsureProject();
                var si = FindSnapshotInfo(project, snapshotId);
                if (si == null)
                    return ErrorJson($"未找到快照 ID={snapshotId}");
                project.SnapshotManager.DeleteSnapshot(si);
                project.Save();
                return Ok($"快照 #{snapshotId} 已删除", 0, new JObject { ["snapshot_id"] = snapshotId });
            }
            catch (Exception ex) { return ErrorJson("删除快照失败: " + ex.Message); }
        }

        // =============================================
        // 回收站列举
        // =============================================

        public static string ListRecycledNodes()
        {
            try
            {
                var project = EnsureProject();
                var recycled = project.SnapshotManager.GetRecycleList();

                var arr = new JArray(recycled.Select(s => new JObject
                {
                    ["snapshot_id"] = s.Id,
                    ["name"] = s.Name ?? "",
                    ["kind"] = GetSnapshotKindName(s.Kind),
                    ["kind_code"] = s.Kind,
                    ["date_time"] = s.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["size"] = s.Size,
                    ["tree_node_id"] = s.TreeNodeId.Value.ToString(),
                    ["deleted"] = s.Deleted
                }));

                var result = new JObject
                {
                    ["success"] = true,
                    ["total"] = recycled.Count,
                    ["recycled_nodes"] = arr
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("列举回收站失败: " + ex.Message); }
        }

        // =============================================
        // 恢复回收站节点
        // =============================================

        public static string RestoreRecycledNode(int snapshotId, long parentNodeId)
        {
            try
            {
                var project = EnsureProject();
                var recycled = project.SnapshotManager.GetRecycleList();
                var target = recycled.FirstOrDefault(s => s.Id == snapshotId);
                if (target == null)
                    return ErrorJson($"回收站中未找到快照 ID={snapshotId}");

                var parent = FindNode(project, parentNodeId) as TreeDirectoryNode;
                if (parent == null)
                    return ErrorJson($"父节点 {parentNodeId} 不是目录节点，无法恢复");

                TreeNodeBase restoredNode = null;
                if (target.Kind == 0)
                {
                    var tn = project.SnapshotManager.GetSnapshotTable(target);
                    parent.InsertChildNode(tn, parent.Children.Count);
                    restoredNode = tn;
                }
                else if (target.Kind == 1)
                {
                    var dn = project.SnapshotManager.GetSnapshotDocument(target);
                    parent.InsertChildNode(dn, parent.Children.Count);
                    restoredNode = dn;
                }
                else if (target.Kind == 2)
                {
                    var img = project.SnapshotManager.GetSnapshotImage(target);
                    parent.InsertChildNode(img, parent.Children.Count);
                    restoredNode = img;
                }
                else if (target.Kind == 3)
                {
                    var pdf = project.SnapshotManager.GetSnapshotPdf(target);
                    parent.InsertChildNode(pdf, parent.Children.Count);
                    restoredNode = pdf;
                }

                if (restoredNode == null)
                    return ErrorJson($"快照 #{snapshotId} 类型 {target.Kind} 不支持恢复");

                project.Save();
                return Ok($"已从回收站恢复 '{target.Name}'", restoredNode.Id.Value,
                    new JObject { ["snapshot_id"] = snapshotId, ["new_node_id"] = restoredNode.Id.Value.ToString() });
            }
            catch (Exception ex) { return ErrorJson("恢复回收站节点失败: " + ex.Message); }
        }

        // =============================================
        // 彻底删除回收站节点
        // =============================================

        public static string PurgeRecycledNode(int snapshotId)
        {
            try
            {
                var project = EnsureProject();
                var si = FindSnapshotInfoInRecycle(project, snapshotId);
                if (si == null)
                    return ErrorJson($"回收站中未找到快照 ID={snapshotId}");
                project.SnapshotManager.DeleteSnapshot(si);
                project.Save();
                return Ok($"回收站快照 #{snapshotId} 已彻底删除", 0, new JObject { ["snapshot_id"] = snapshotId });
            }
            catch (Exception ex) { return ErrorJson("彻底删除回收站节点失败: " + ex.Message); }
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

        private static TreeNodeBase FindNode(Project project, long nodeId)
        {
            var id = new Id64(nodeId);
            var node = project.GetNodeById(id);
            if (node == null)
                throw new InvalidOperationException($"未找到节点: {nodeId}");
            return node;
        }

        private static SnapshotInfo FindSnapshotInfo(Project project, int snapshotId)
        {
            // 在所有节点的快照中查找
            foreach (var node in project.GetAllTreeNodes())
            {
                foreach (var s in project.SnapshotManager.GetSnapshots(node))
                {
                    if (s.Id == snapshotId) return s;
                }
            }
            // 也在回收站中查找
            return FindSnapshotInfoInRecycle(project, snapshotId);
        }

        private static SnapshotInfo FindSnapshotInfoInRecycle(Project project, int snapshotId)
        {
            return project.SnapshotManager.GetRecycleList().FirstOrDefault(s => s.Id == snapshotId);
        }

        private static string GetSnapshotKindName(int kind)
        {
            switch (kind)
            {
                case 0: return "table";
                case 1: return "document";
                case 2: return "image";
                case 3: return "pdf";
                default: return "unknown";
            }
        }

        private static string Ok(string msg, long nodeId, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["message"] = msg };
            if (nodeId > 0) r["node_id"] = nodeId.ToString();
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
