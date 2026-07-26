using System;
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
    /// 采集与汇总服务
    /// 覆盖 AppCommands.CollectByColumn / CollectByCell / ExecuteCollect / ConsolidateSetting /
    /// ExecuteConsolidateFull / ExecuteConsolidateBrief / RefreshConsolidate / OneClickCollect 等 UI 操作
    /// </summary>
    public static class CollectConsolidateService
    {
        // =============================================
        // 获取采集配置
        // =============================================

        public static string GetCollectConfig(long tableNodeId)
        {
            try
            {
                var table = GetTable(tableNodeId);

                // 获取采集字典
                var collectDic = table.CellPropManager?.DicCellAttachments;
                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["has_collect_data"] = collectDic != null && collectDic.Count > 0,
                    ["collected_cell_count"] = collectDic?.Count ?? 0
                };

                if (collectDic != null && collectDic.Count > 0)
                {
                    var sample = new JArray();
                    foreach (var kv in collectDic.Take(20))
                    {
                        sample.Add(new JObject
                        {
                            ["cell_id"] = kv.Key.Value.ToString(),
                            ["dirty"] = kv.Value.Dirty,
                            ["status"] = kv.Value.Status.ToString(),
                            ["attachment_count"] = kv.Value.Attachments?.Count ?? 0
                        });
                    }
                    result["sample_cells"] = sample;
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取采集配置失败: " + ex.Message); }
        }

        // =============================================
        // 采集单元格数据
        // =============================================

        public static string CollectCells(long tableNodeId, int[] cellIndices, string collectKey)
        {
            try
            {
                var table = GetTable(tableNodeId);
                int collected = 0;
                var details = new JArray();

                foreach (var idx in cellIndices)
                {
                    int row = idx / 10000;
                    int col = idx % 10000;
                    if (row < 0 || row >= table.Rows.Count) continue;
                    if (col < 0 || col >= table.Columns.Count) continue;

                    var cell = table[row, col];
                    if (cell == null) continue;

                    // 标记为采集状态：通过 CellAttachments.Dirty = true
                    if (table.CellPropManager != null)
                    {
                        if (table.CellPropManager.TryGetAttachments(cell, out var att))
                        {
                            att.Dirty = true;
                        }
                        collected++;
                        details.Add(new JObject { ["row"] = row, ["col"] = col, ["cell_id"] = cell.Id.Value.ToString() });
                    }
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"已采集 {collected} 个单元格", tableNodeId,
                    new JObject { ["collected"] = collected, ["collect_key"] = collectKey ?? "", ["details"] = details });
            }
            catch (Exception ex) { return ErrorJson("采集单元格失败: " + ex.Message); }
        }

        // =============================================
        // 采集整列数据
        // =============================================

        public static string CollectByColumn(long tableNodeId, int colIndex, string collectKey)
        {
            try
            {
                var table = GetTable(tableNodeId);
                if (colIndex < 0 || colIndex >= table.Columns.Count)
                    return ErrorJson($"列索引 {colIndex} 超出范围（共 {table.Columns.Count} 列）");

                int collected = 0;
                for (int r = 0; r < table.Rows.Count; r++)
                {
                    var cell = table[r, colIndex];
                    if (cell == null) continue;
                    if (table.CellPropManager != null)
                    {
                        if (table.CellPropManager.TryGetAttachments(cell, out var att))
                        {
                            att.Dirty = true;
                        }
                        collected++;
                    }
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"已采集列 {colIndex} 的 {collected} 个单元格", tableNodeId,
                    new JObject { ["col_index"] = colIndex, ["collected"] = collected, ["collect_key"] = collectKey ?? "" });
            }
            catch (Exception ex) { return ErrorJson("按列采集失败: " + ex.Message); }
        }

        // =============================================
        // 获取汇总配置
        // =============================================

        public static string GetConsolidateConfig(long tableNodeId)
        {
            try
            {
                var table = GetTable(tableNodeId);

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["has_data_reference_manager"] = table.Project.DataReferenceManager != null,
                    ["row_count"] = table.Rows.Count,
                    ["col_count"] = table.Columns.Count,
                    ["has_consolidate_settings"] = table.ConsolidateSettings != null
                };

                // 检查跨项目引用
                var refs = table.Project.DataReferenceManager;
                if (refs != null)
                {
                    int refCount = refs.Enumerate().Count();
                    result["total_data_references"] = refCount;
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取汇总配置失败: " + ex.Message); }
        }

        // =============================================
        // 执行汇总：标记含 REF 公式的单元格为脏，触发后续重算
        // =============================================

        public static string ExecuteConsolidate(long tableNodeId, bool fullRefresh = false)
        {
            try
            {
                var table = GetTable(tableNodeId);
                int refreshed = 0;
                int failed = 0;
                var errors = new JArray();

                // 遍历所有单元格，标记含 REF 公式的单元格为脏
                foreach (var row in table.Rows)
                {
                    foreach (var col in table.Columns)
                    {
                        var cell = table[row.Index, col.Index];
                        if (cell == null) continue;
                        try
                        {
                            if (cell.HasFormula && cell.Formula.Contains("REF"))
                            {
                                cell.NeedSave = true;
                                if (table.CellPropManager != null && table.CellPropManager.TryGetAttachments(cell, out var att))
                                {
                                    att.Dirty = true;
                                }
                                refreshed++;
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            if (errors.Count < 20)
                            {
                                errors.Add(new JObject { ["row"] = row.Index, ["col"] = col.Index, ["error"] = ex.Message });
                            }
                        }
                    }
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"汇总完成：刷新 {refreshed} 个单元格，失败 {failed} 个", tableNodeId,
                    new JObject { ["refreshed"] = refreshed, ["failed"] = failed, ["full_refresh"] = fullRefresh, ["errors"] = errors });
            }
            catch (Exception ex) { return ErrorJson("执行汇总失败: " + ex.Message); }
        }

        // =============================================
        // 一键采集：采集所有数据行（非表头/固定行）
        // =============================================

        public static string OneClickCollect(long tableNodeId)
        {
            try
            {
                var table = GetTable(tableNodeId);
                int collected = 0;

                for (int r = 0; r < table.Rows.Count; r++)
                {
                    var row = table.Rows[r];
                    // 跳过表头行和固定行
                    if (row.Role == RowRole.Header || row.Role == RowRole.Fixed) continue;

                    for (int c = 0; c < table.Columns.Count; c++)
                    {
                        var cell = table[r, c];
                        if (cell == null) continue;
                        if (table.CellPropManager != null)
                        {
                            if (table.CellPropManager.TryGetAttachments(cell, out var att))
                            {
                                att.Dirty = true;
                            }
                            collected++;
                        }
                    }
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"一键采集完成：共采集 {collected} 个数据单元格", tableNodeId,
                    new JObject { ["collected"] = collected });
            }
            catch (Exception ex) { return ErrorJson("一键采集失败: " + ex.Message); }
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
