using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using Table = Auditai.Model.Table;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 批量列操作与搜索服务
    /// 覆盖 AppCommands.BatchColumnDuplicate / BatchColumnRemove / BatchColumnRename / Find 等 UI 操作
    /// </summary>
    public static class BatchOperationService
    {
        // =============================================
        // 批量复制列：使用 ColumnCollection.Insert 在末尾插入新列
        // =============================================

        public static string BatchDuplicateColumn(long tableNodeId, int[] colIndices, string nameSuffix = "_copy")
        {
            try
            {
                var table = GetTable(tableNodeId);
                int duplicated = 0;
                var details = new JArray();

                // 按索引倒序处理，避免索引变化
                foreach (int idx in colIndices.OrderByDescending(x => x))
                {
                    if (idx < 0 || idx >= table.Columns.Count) continue;
                    var srcCol = table.Columns[idx];

                    // 在末尾插入 1 列
                    int newIdx = table.Columns.Count;
                    table.Columns.Insert(newIdx, 1);
                    var newCol = table.Columns[newIdx];

                    // 复制属性
                    newCol.Caption = (srcCol.Caption ?? "") + nameSuffix;
                    newCol.Width = srcCol.Width;
                    newCol.Visible = srcCol.Visible;
                    newCol.Formula = srcCol.Formula;
                    newCol.CaptionFormula = srcCol.CaptionFormula;

                    duplicated++;
                    details.Add(new JObject
                    {
                        ["source_index"] = idx,
                        ["source_caption"] = srcCol.Caption ?? "",
                        ["new_index"] = newIdx,
                        ["new_caption"] = newCol.Caption
                    });
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"已复制 {duplicated} 列", tableNodeId, new JObject { ["duplicated"] = duplicated, ["details"] = details });
            }
            catch (Exception ex) { return ErrorJson("批量复制列失败: " + ex.Message); }
        }

        // =============================================
        // 批量删除列：使用 ColumnCollection.Remove
        // =============================================

        public static string BatchRemoveColumn(long tableNodeId, int[] colIndices)
        {
            try
            {
                var table = GetTable(tableNodeId);
                int removed = 0;
                var details = new JArray();

                // 按索引倒序处理
                foreach (int idx in colIndices.OrderByDescending(x => x))
                {
                    if (idx < 0 || idx >= table.Columns.Count) continue;
                    var col = table.Columns[idx];
                    details.Add(new JObject { ["index"] = idx, ["caption"] = col.Caption ?? "" });
                    table.Columns.Remove(idx, 1);
                    removed++;
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"已删除 {removed} 列", tableNodeId, new JObject { ["removed"] = removed, ["details"] = details });
            }
            catch (Exception ex) { return ErrorJson("批量删除列失败: " + ex.Message); }
        }

        // =============================================
        // 批量重命名列：修改 Caption
        // =============================================

        public static string BatchRenameColumn(long tableNodeId, JObject[] renames)
        {
            try
            {
                var table = GetTable(tableNodeId);
                int renamed = 0;
                var details = new JArray();

                foreach (var r in renames)
                {
                    int idx = r["col_index"]?.Value<int>() ?? -1;
                    string newName = r["new_name"]?.ToString();
                    if (idx < 0 || idx >= table.Columns.Count || string.IsNullOrEmpty(newName)) continue;

                    var col = table.Columns[idx];
                    string oldName = col.Caption;
                    col.Caption = newName;
                    renamed++;
                    details.Add(new JObject { ["index"] = idx, ["old_caption"] = oldName ?? "", ["new_caption"] = newName });
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"已重命名 {renamed} 列", tableNodeId, new JObject { ["renamed"] = renamed, ["details"] = details });
            }
            catch (Exception ex) { return ErrorJson("批量重命名列失败: " + ex.Message); }
        }

        // =============================================
        // 在表格中搜索
        // =============================================

        public static string FindInTable(long tableNodeId, string keyword, bool matchCase = false, bool matchWholeCell = false)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var matches = new JArray();
                int total = 0;

                for (int r = 0; r < table.Rows.Count; r++)
                {
                    for (int c = 0; c < table.Columns.Count; c++)
                    {
                        var cell = table[r, c];
                        if (cell == null) continue;
                        string val = cell.Value?.ToString() ?? "";
                        bool matched = false;

                        if (matchWholeCell)
                        {
                            matched = matchCase ? val == keyword : string.Equals(val, keyword, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            matched = matchCase ? val.Contains(keyword) : val.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
                        }

                        if (matched)
                        {
                            total++;
                            matches.Add(new JObject
                            {
                                ["row"] = r,
                                ["col"] = c,
                                ["value"] = val,
                                ["row_index"] = table.Rows[r]?.Index ?? r,
                                ["col_caption"] = table.Columns[c]?.Caption ?? ""
                            });
                        }
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["keyword"] = keyword,
                    ["total"] = total,
                    ["matches"] = matches
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("表格搜索失败: " + ex.Message); }
        }

        // =============================================
        // 在所有表格中搜索
        // =============================================

        public static string FindInAllTables(string keyword, bool matchCase = false, bool matchWholeCell = false)
        {
            try
            {
                var project = EnsureProject();
                var allNodes = project.GetAllTableNodes().ToList();
                var tableResults = new JArray();
                int totalMatches = 0;

                foreach (var tn in allNodes)
                {
                    try
                    {
                        tn.Table.LoadAndReturn(true);
                        var result = FindInTable(tn.Id.Value, keyword, matchCase, matchWholeCell);
                        var obj = JObject.Parse(result);
                        if (obj["success"]?.Value<bool>() == true && obj["total"]?.Value<int>() > 0)
                        {
                            totalMatches += obj["total"].Value<int>();
                            tableResults.Add(new JObject
                            {
                                ["table_node_id"] = tn.Id.Value.ToString(),
                                ["table_name"] = tn.Name,
                                ["match_count"] = obj["total"],
                                ["matches"] = obj["matches"]
                            });
                        }
                    }
                    catch { /* 忽略单个表格搜索失败 */ }
                }

                var summary = new JObject
                {
                    ["success"] = true,
                    ["keyword"] = keyword,
                    ["total_tables_searched"] = allNodes.Count,
                    ["total_matches"] = totalMatches,
                    ["tables_with_matches"] = tableResults.Count,
                    ["results"] = tableResults
                };
                return JsonConvert.SerializeObject(summary, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("全局表格搜索失败: " + ex.Message); }
        }

        // =============================================
        // 在文档中搜索
        // =============================================

        public static string FindInDocument(long documentNodeId, string keyword, bool matchCase = false)
        {
            try
            {
                var project = EnsureProject();
                var id = new Id64(documentNodeId);
                var docNode = project.GetAllDocumentNodes().FirstOrDefault(d => d.Id == id);
                if (docNode == null)
                    return ErrorJson($"未找到文档节点: {documentNodeId}");
                docNode.Document.LoadAndReturn();

                var matches = new JArray();
                int total = 0;
                var paragraphs = docNode.Document.Paragraphs;
                var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var p = paragraphs[i];
                    string text = p.Stream ?? "";
                    // 简单文本匹配（OOXML 中提取纯文本较复杂，这里做粗略匹配）
                    if (text.IndexOf(keyword, comparison) >= 0)
                    {
                        total++;
                        matches.Add(new JObject
                        {
                            ["paragraph_index"] = i,
                            ["comment"] = p.Comment ?? "",
                            ["snippet"] = text.Length > 200 ? text.Substring(0, 200) + "..." : text
                        });
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["keyword"] = keyword,
                    ["total"] = total,
                    ["matches"] = matches
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("文档搜索失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static Table GetTable(long tableNodeId)
        {
            var project = EnsureProject();
            var id = new Id64(tableNodeId);
            var table = project.GetTableById(id);
            if (table == null)
                throw new InvalidOperationException($"未找到表格节点: {tableNodeId}");
            table.LoadAndReturn(true);
            return table;
        }

        private static Project EnsureProject()
        {
            var project = SessionState.Current.CurrentProject;
            if (project == null)
                throw new InvalidOperationException("未打开项目，请先调用 open_project 工具");
            Project.Current = project;
            return project;
        }

        private static string Ok(string msg, long tableNodeId, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["table_node_id"] = tableNodeId.ToString(), ["message"] = msg };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
