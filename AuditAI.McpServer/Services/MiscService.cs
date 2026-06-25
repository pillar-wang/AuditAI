﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C1.C1Excel;
using Auditai.DTO;
using Auditai.Model;
using Table = Auditai.Model.Table;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 杂项服务
    /// 提供节点搜索和 Excel 数据导入等辅助功能
    /// </summary>
    public static class MiscService
    {
        /// <summary>
        /// 按关键词和节点类型搜索项目树节点
        /// </summary>
        /// <param name="keyword">搜索关键词（节点名称包含，不区分大小写）</param>
        /// <param name="nodeType">节点类型筛选：table/document/directory/image/pdf（可选）</param>
        /// <returns>匹配节点列表 JSON</returns>
        public static string SearchNodes(string keyword, string nodeType)
        {
            try
            {
                SessionState.Current.EnsureProject();
                var project = SessionState.Current.CurrentProject;

                if (string.IsNullOrWhiteSpace(keyword))
                    return ErrorJson("搜索关键词不能为空");

                var allNodes = project.GetAllTreeNodes();
                var matchingNodes = allNodes.Where(n =>
                    n.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);

                if (!string.IsNullOrWhiteSpace(nodeType))
                {
                    string filterType = nodeType.ToLowerInvariant();
                    matchingNodes = matchingNodes.Where(n => MatchNodeType(n, filterType));
                }

                var nodes = new JArray();
                foreach (var node in matchingNodes)
                {
                    var item = new JObject
                    {
                        ["node_id"] = node.Id.Value.ToString(),
                        ["name"] = node.Name,
                        ["type"] = GetNodeTypeName(node),
                        ["parent_id"] = node.Parent?.Id.Value.ToString() ?? "",
                        ["path"] = BuildNodePath(node)
                    };
                    nodes.Add(item);
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["keyword"] = keyword,
                    ["node_type"] = nodeType ?? "",
                    ["match_count"] = nodes.Count,
                    ["nodes"] = nodes
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("搜索节点失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 从 Excel 文件导入数据到指定表格
        /// 使用 C1.C1Excel 读取，支持指定起始行列和是否跳过表头行
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <param name="tableNodeId">目标表格节点 ID</param>
        /// <param name="startRow">表格起始行索引（从 0 开始）</param>
        /// <param name="startCol">表格起始列索引（从 0 开始）</param>
        /// <param name="hasHeader">Excel 第一行是否为表头（为 true 时跳过不导入）</param>
        /// <returns>导入结果 JSON</returns>
        public static string ImportExcelToTable(string filePath, long tableNodeId, int startRow, int startCol, bool hasHeader)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (string.IsNullOrWhiteSpace(filePath))
                    return ErrorJson("文件路径不能为空");

                if (!File.Exists(filePath))
                    return ErrorJson($"文件不存在: {filePath}");

                if (startRow < 0) startRow = 0;
                if (startCol < 0) startCol = 0;

                var project = SessionState.Current.CurrentProject;
                var table = project.GetTableById(new Id64(tableNodeId));
                if (table == null)
                    return ErrorJson($"未找到表格节点: {tableNodeId}");

                table.LoadAndReturn(true);

                // 读取 Excel 文件
                var xlBook = new C1XLBook();
                xlBook.Load(filePath);
                XLSheet xlSheet = xlBook.Sheets[0];

                // 确定有效数据范围
                int maxUsedRow = -1;
                int maxUsedCol = -1;
                int emptyRowCount = 0;
                const int maxCheckRows = 100000;
                const int maxCheckCols = 1000;
                const int emptyRowThreshold = 3;

                for (int r = 0; r < maxCheckRows; r++)
                {
                    bool rowHasData = false;
                    for (int c = 0; c < maxCheckCols; c++)
                    {
                        var xlCell = xlSheet[r, c];
                        if (xlCell != null && xlCell.Value != null)
                        {
                            string val = xlCell.Value.ToString();
                            if (!string.IsNullOrWhiteSpace(val))
                            {
                                rowHasData = true;
                                if (c > maxUsedCol) maxUsedCol = c;
                            }
                        }
                    }

                    if (rowHasData)
                    {
                        maxUsedRow = r;
                        emptyRowCount = 0;
                    }
                    else
                    {
                        emptyRowCount++;
                        if (emptyRowCount >= emptyRowThreshold)
                            break;
                    }
                }

                if (maxUsedRow < 0 || maxUsedCol < 0)
                    return ErrorJson("Excel 文件中未找到有效数据");

                // 如果 Excel 有表头行，跳过第一行
                int excelStartRow = hasHeader ? 1 : 0;

                int importedRowCount = 0;
                int importedColCount = maxUsedCol + 1;

                for (int r = excelStartRow; r <= maxUsedRow; r++)
                {
                    int tableRow = startRow + (r - excelStartRow);

                    // 确保表格有足够的行
                    while (table.Rows.Count <= tableRow)
                    {
                        table.Rows.Append(1);
                    }

                    bool rowHasData = false;
                    for (int c = 0; c <= maxUsedCol; c++)
                    {
                        int tableCol = startCol + c;

                        // 确保表格有足够的列
                        while (table.Columns.Count <= tableCol)
                        {
                            table.Columns.Append(1);
                        }

                        var xlCell = xlSheet[r, c];
                        if (xlCell != null && xlCell.Value != null)
                        {
                            var cell = table[tableRow, tableCol];
                            if (cell != null)
                            {
                                cell.UpdateValue(xlCell.Value);
                                rowHasData = true;
                            }
                        }
                    }

                    if (rowHasData)
                    {
                        importedRowCount++;
                    }
                }

                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["imported_row_count"] = importedRowCount,
                    ["imported_col_count"] = importedColCount,
                    ["has_header"] = hasHeader,
                    ["message"] = $"成功导入 {importedRowCount} 行、{importedColCount} 列数据"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("导入 Excel 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取节点类型名称
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
        /// 判断节点是否匹配指定的类型筛选
        /// </summary>
        private static bool MatchNodeType(TreeNodeBase node, string nodeType)
        {
            switch (nodeType)
            {
                case "table": return node is TreeTableNode;
                case "document": return node is TreeDocumentNode;
                case "directory": return node is TreeDirectoryNode;
                case "image": return node is TreeImageNode;
                case "pdf": return node is TreePdfNode;
                default: return false;
            }
        }

        /// <summary>
        /// 构建节点路径（分组名/目录1/目录2/.../节点名）
        /// </summary>
        private static string BuildNodePath(TreeNodeBase node)
        {
            var parts = new List<string>();
            var current = node;
            while (current != null)
            {
                parts.Insert(0, current.Name);
                current = current.Parent;
            }
            parts.Insert(0, node.Group?.Name ?? "");
            return string.Join("/", parts);
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