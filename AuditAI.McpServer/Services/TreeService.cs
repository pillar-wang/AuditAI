﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using TreeGroup = Auditai.Model.TreeGroup;
using Table = Auditai.Model.Table;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 导航树管理服务
    /// 封装项目导航树的结构查询与节点增删改查操作
    /// </summary>
    public static class TreeService
    {
        // =============================================
        // 公开方法
        // =============================================

        /// <summary>
        /// 获取当前项目的完整导航树结构
        /// 遍历项目的 TreeGroups，返回层级 JSON（节点类型、ID、名称、子节点）
        /// </summary>
        public static string GetProjectTree()
        {
            try
            {
                var project = EnsureProject();

                var groups = project.TreeGroups.Select(g => BuildGroupJson(g)).ToList();

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = project.Id,
                    ["project_name"] = project.Name,
                    ["group_count"] = project.TreeGroups.Count,
                    ["node_count"] = project.GetAllTreeNodes().Count(),
                    ["tree_groups"] = new JArray(groups)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取项目导航树失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 创建目录节点
        /// parentId 可指向 TreeGroup（创建根目录）或 TreeDirectoryNode（创建子目录）
        /// </summary>
        public static string CreateDirectoryNode(long parentId, string name)
        {
            try
            {
                var project = EnsureProject();

                if (string.IsNullOrWhiteSpace(name))
                    return ErrorJson("目录名称不能为空");

                TreeDirectoryNode newDir;
                if (TryFindParent(project, parentId, out var group, out var parentDir))
                {
                    if (group != null)
                    {
                        newDir = group.InsertRootDirectory(group.RootNodes.Count);
                    }
                    else
                    {
                        newDir = parentDir.InsertChildDirectory(parentDir.Children.Count);
                    }
                    newDir.UpdateName(name);
                }
                else
                {
                    return ErrorJson($"找不到 ID 为 {parentId} 的父节点（应为 TreeGroup 或 TreeDirectoryNode）");
                }

                return SerializeSuccessNode(newDir, parentId, "目录创建成功");
            }
            catch (Exception ex)
            {
                return ErrorJson("创建目录节点失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 创建文档节点
        /// parentId 可指向 TreeGroup（创建根文档）或 TreeDirectoryNode（创建子文档）
        /// </summary>
        public static string CreateDocumentNode(long parentId, string name)
        {
            try
            {
                var project = EnsureProject();

                if (string.IsNullOrWhiteSpace(name))
                    return ErrorJson("文档名称不能为空");

                TreeDocumentNode newDoc;
                if (TryFindParent(project, parentId, out var group, out var parentDir))
                {
                    if (group != null)
                    {
                        newDoc = group.InsertRootDocument(group.RootNodes.Count);
                    }
                    else
                    {
                        newDoc = parentDir.InsertChildDocument(parentDir.Children.Count);
                    }
                    newDoc.UpdateName(name);
                }
                else
                {
                    return ErrorJson($"找不到 ID 为 {parentId} 的父节点（应为 TreeGroup 或 TreeDirectoryNode）");
                }

                return SerializeSuccessNode(newDoc, parentId, "文档创建成功");
            }
            catch (Exception ex)
            {
                return ErrorJson("创建文档节点失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 创建表格节点（带列定义）
        /// parentId 可指向 TreeGroup（创建根表格）或 TreeDirectoryNode（创建子表格）
        /// columns 为列定义数组，元素可为字符串（列标题）或对象（含 name/width 字段）
        /// </summary>
        public static string CreateTableNode(long parentId, string name, JArray columns)
        {
            try
            {
                var project = EnsureProject();

                if (string.IsNullOrWhiteSpace(name))
                    return ErrorJson("表格名称不能为空");

                TreeTableNode newTable;
                if (TryFindParent(project, parentId, out var group, out var parentDir))
                {
                    if (group != null)
                    {
                        newTable = group.InsertRootTable(group.RootNodes.Count, InitTableMode.Default);
                    }
                    else
                    {
                        newTable = parentDir.InsertChildTable(parentDir.Children.Count, InitTableMode.Default);
                    }
                    newTable.UpdateName(name);
                }
                else
                {
                    return ErrorJson($"找不到 ID 为 {parentId} 的父节点（应为 TreeGroup 或 TreeDirectoryNode）");
                }

                // 调整列定义
                if (columns != null && columns.Count > 0)
                {
                    ApplyColumnDefinitions(newTable.Table, columns);
                }

                // 保存表格数据（Table、Columns、Rows、Cells、CellStyles）到数据库
                // 否则重新打开项目后 LoadAndReturn 找不到 Table 记录，
                // 会将 IsCorrupted 置为 true，导致 get_table_data 返回空表格
                newTable.Table.Save();

                var result = BuildSuccessNode(newTable, parentId, "表格创建成功");
                result["column_count"] = newTable.Table.Columns.Count;
                result["row_count"] = newTable.Table.Rows.Count;
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("创建表格节点失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        public static string DeleteNode(long nodeId)
        {
            try
            {
                var project = EnsureProject();

                var node = project.GetNodeById(new Id64(nodeId));
                if (node == null)
                    return ErrorJson($"找不到 ID 为 {nodeId} 的节点");

                string nodeName = node.Name;
                string nodeType = GetNodeTypeName(node);
                node.Remove();

                var result = new JObject
                {
                    ["success"] = true,
                    ["node_id"] = nodeId.ToString(),
                    ["name"] = nodeName,
                    ["type"] = nodeType,
                    ["message"] = "节点已删除"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("删除节点失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 移动节点
        /// newParentId 可指向 TreeGroup（移动为根节点）或 TreeDirectoryNode（移动为子节点）
        /// </summary>
        public static string MoveNode(long nodeId, long newParentId)
        {
            try
            {
                var project = EnsureProject();

                var node = project.GetNodeById(new Id64(nodeId));
                if (node == null)
                    return ErrorJson($"找不到 ID 为 {nodeId} 的节点");

                if (nodeId == newParentId)
                    return ErrorJson("不能将节点移动到自身");

                if (!TryFindParent(project, newParentId, out var group, out var parentDir))
                    return ErrorJson($"找不到 ID 为 {newParentId} 的目标父节点");

                // 防止将节点移动到其子节点下（造成循环）
                if (parentDir != null && IsDescendant(node, parentDir))
                    return ErrorJson("不能将节点移动到其子节点下");

                if (group != null)
                {
                    node.MoveTo(group);
                }
                else
                {
                    node.MoveTo(parentDir);
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["node_id"] = nodeId.ToString(),
                    ["name"] = node.Name,
                    ["type"] = GetNodeTypeName(node),
                    ["new_parent_id"] = newParentId.ToString(),
                    ["message"] = "节点已移动"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("移动节点失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 复制节点
        /// newParentId 可指向 TreeGroup（复制为根节点）或 TreeDirectoryNode（复制为子节点）
        /// 注意：目录节点的复制为递归深拷贝，表格/文档使用各自的 Duplicate 方法
        /// </summary>
        public static string CopyNode(long nodeId, long newParentId)
        {
            try
            {
                var project = EnsureProject();

                var source = project.GetNodeById(new Id64(nodeId));
                if (source == null)
                    return ErrorJson($"找不到 ID 为 {nodeId} 的源节点");

                if (!TryFindParent(project, newParentId, out var group, out var parentDir))
                    return ErrorJson($"找不到 ID 为 {newParentId} 的目标父节点");

                // 克隆节点（递归处理目录）
                TreeNodeBase cloned = CloneNode(source);
                if (cloned == null)
                    return ErrorJson($"不支持复制此类型节点: {GetNodeTypeName(source)}");

                // 插入到目标父节点
                if (group != null)
                {
                    group.InsertRootNode(cloned, group.RootNodes.Count);
                }
                else
                {
                    parentDir.InsertChildNode(cloned, parentDir.Children.Count);
                }

                // 复制后追加" (副本)"后缀以区分
                cloned.UpdateName(source.Name + " (副本)");

                return SerializeSuccessNode(cloned, newParentId, "节点已复制");
            }
            catch (Exception ex)
            {
                return ErrorJson("复制节点失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 重命名节点
        /// </summary>
        public static string RenameNode(long nodeId, string newName)
        {
            try
            {
                var project = EnsureProject();

                if (string.IsNullOrWhiteSpace(newName))
                    return ErrorJson("新名称不能为空");

                var node = project.GetNodeById(new Id64(nodeId));
                if (node == null)
                    return ErrorJson($"找不到 ID 为 {nodeId} 的节点");

                string oldName = node.Name;
                node.UpdateName(newName);

                var result = new JObject
                {
                    ["success"] = true,
                    ["node_id"] = nodeId.ToString(),
                    ["old_name"] = oldName,
                    ["new_name"] = node.Name,
                    ["type"] = GetNodeTypeName(node),
                    ["message"] = "节点已重命名"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("重命名节点失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        /// <summary>
        /// 确保已打开项目并同步 Project.Current（节点创建依赖此静态属性进行 ID 分配）
        /// </summary>
        private static Project EnsureProject()
        {
            var project = SessionState.Current.CurrentProject;
            if (project == null)
                throw new InvalidOperationException("未打开项目，请先调用 open_project 工具");
            // 节点创建方法内部使用 Project.Current.GetNextId()，必须同步设置
            Project.Current = project;
            return project;
        }

        /// <summary>
        /// 根据 parentId 查找父节点，优先匹配 TreeGroup，其次匹配 TreeDirectoryNode
        /// </summary>
        private static bool TryFindParent(Project project, long parentId, out TreeGroup group, out TreeDirectoryNode dir)
        {
            group = null;
            dir = null;
            var id = new Id64(parentId);

            // 先在 TreeGroups 中查找
            group = project.TreeGroups.FirstOrDefault(g => g.Id == id);
            if (group != null)
                return true;

            // 再在所有树节点中查找（仅接受 TreeDirectoryNode 作为父节点）
            var node = project.GetNodeById(id);
            if (node is TreeDirectoryNode d)
            {
                dir = d;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取节点类型的字符串名称
        /// </summary>
        private static string GetNodeTypeName(TreeNodeBase node)
        {
            if (node is TreeDirectoryNode) return "directory";
            if (node is TreeTableNode) return "table";
            if (node is TreeDocumentNode) return "document";
            if (node is TreeImageNode) return "image";
            if (node is TreePdfNode) return "pdf";
            return "unknown";
        }

        /// <summary>
        /// 构建 TreeGroup 的 JSON 表示（含其下所有根节点的递归子节点）
        /// </summary>
        private static JObject BuildGroupJson(TreeGroup group)
        {
            var obj = new JObject
            {
                ["id"] = group.Id.Value.ToString(),
                ["name"] = group.Name,
                ["type"] = "group",
                ["node_count"] = group.GetAllNodes().Count()
            };

            var children = new JArray();
            foreach (var rootNode in group.RootNodes)
            {
                children.Add(BuildNodeJson(rootNode));
            }
            obj["children"] = children;
            return obj;
        }

        /// <summary>
        /// 递归构建树节点的 JSON 表示
        /// </summary>
        private static JObject BuildNodeJson(TreeNodeBase node)
        {
            var obj = new JObject
            {
                ["id"] = node.Id.Value.ToString(),
                ["name"] = node.Name,
                ["type"] = GetNodeTypeName(node),
                ["number"] = node.Number ?? "",
                ["visible"] = node.Visible,
                ["level"] = node.Level
            };

            if (node is TreeDirectoryNode dir)
            {
                var children = new JArray();
                foreach (var child in dir.Children)
                {
                    children.Add(BuildNodeJson(child));
                }
                obj["children"] = children;
                obj["child_count"] = dir.Children.Count;
            }
            else if (node is TreeTableNode table)
            {
                obj["column_count"] = table.Table.Columns.Count;
                obj["row_count"] = table.Table.Rows.Count;
            }

            return obj;
        }

        /// <summary>
        /// 应用列定义到表格：调整列数并设置标题
        /// 列定义元素可为字符串（标题）或对象（含 name/caption、width 字段）
        /// </summary>
        private static void ApplyColumnDefinitions(Table table, JArray columns)
        {
            int currentCount = table.Columns.Count;
            int desiredCount = columns.Count;

            // 列数不足时追加
            if (desiredCount > currentCount)
            {
                table.Columns.Append(desiredCount - currentCount);
            }
            // 列数过多时从末尾移除
            else if (desiredCount < currentCount)
            {
                table.Columns.Remove(desiredCount, currentCount - desiredCount);
            }

            // 设置每列的标题和宽度
            for (int i = 0; i < columns.Count; i++)
            {
                var colSpec = columns[i];
                var column = table.Columns[i];

                if (colSpec.Type == JTokenType.String)
                {
                    column.Caption = colSpec.ToString();
                }
                else if (colSpec.Type == JTokenType.Object)
                {
                    var colObj = (JObject)colSpec;
                    string caption = colObj["name"]?.ToString()
                                     ?? colObj["caption"]?.ToString()
                                     ?? $"列{i + 1}";
                    column.Caption = caption;

                    if (colObj["width"] != null)
                    {
                        int width = colObj["width"].Value<int>();
                        if (width > 0)
                            column.Width = width;
                    }
                }
            }

            table.NeedSave = true;
        }

        /// <summary>
        /// 递归克隆树节点（用于复制操作）
        /// 目录节点递归克隆其所有子节点；表格/文档使用各自的 Duplicate 方法
        /// </summary>
        private static TreeNodeBase CloneNode(TreeNodeBase source)
        {
            if (source is TreeDirectoryNode dirSource)
            {
                var newDir = dirSource.DuplicateDirectory();
                foreach (var child in dirSource.Children)
                {
                    var clonedChild = CloneNode(child);
                    if (clonedChild != null)
                    {
                        newDir.InsertChildNode(clonedChild, newDir.Children.Count);
                    }
                }
                return newDir;
            }
            if (source is TreeTableNode tableSource)
            {
                return tableSource.DuplicateTable();
            }
            if (source is TreeDocumentNode docSource)
            {
                return docSource.DuplicateDocument();
            }
            // 图片/PDF 等节点的复制需要文件资源，暂不支持
            return null;
        }

        /// <summary>
        /// 判断 possibleDescendant 是否为 ancestor 的后代节点（防止移动时产生循环）
        /// </summary>
        private static bool IsDescendant(TreeNodeBase ancestor, TreeNodeBase possibleDescendant)
        {
            if (ancestor is TreeDirectoryNode dir)
            {
                foreach (var child in dir.Children)
                {
                    if (ReferenceEquals(child, possibleDescendant))
                        return true;
                    if (IsDescendant(child, possibleDescendant))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 构建节点创建成功的 JObject（可继续追加字段后序列化）
        /// </summary>
        private static JObject BuildSuccessNode(TreeNodeBase node, long parentId, string message)
        {
            return new JObject
            {
                ["success"] = true,
                ["node_id"] = node.Id.Value.ToString(),
                ["name"] = node.Name,
                ["type"] = GetNodeTypeName(node),
                ["parent_id"] = parentId.ToString(),
                ["message"] = message
            };
        }

        /// <summary>
        /// 构建并序列化节点创建成功的 JSON 响应
        /// </summary>
        private static string SerializeSuccessNode(TreeNodeBase node, long parentId, string message)
        {
            return JsonConvert.SerializeObject(BuildSuccessNode(node, parentId, message), Formatting.Indented);
        }

        /// <summary>
        /// 生成错误 JSON 响应
        /// </summary>
        private static string ErrorJson(string message)
        {
            var result = new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }
    }
}
