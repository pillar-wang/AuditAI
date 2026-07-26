using System;
using System.Drawing;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Table = Auditai.Model.Table;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 高级表格操作服务
    /// 封装表格的拆分、行列移动、行高列宽、隐藏、行角色、列重命名、列公式、
    /// 锁定、冻结列、排序、控制公式等扩展操作
    /// </summary>
    public static class AdvancedTableService
    {
        // =============================================
        // 合并/拆分
        // =============================================

        /// <summary>
        /// 拆分合并单元格（取消指定单元格所在的合并区域）
        /// </summary>
        public static string SplitCells(long tableNodeId, int row, int col)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);

                var merge = table.MergedCells.FirstOrDefault(m =>
                    m.TopLeft.Row.Index <= row && row <= m.BottomRight.Row.Index &&
                    m.TopLeft.Column.Index <= col && col <= m.BottomRight.Column.Index);
                if (merge == null)
                    return ErrorJson($"单元格({row},{col})不在任何合并区域内");

                table.UnmergeCells(row, col);
                table.NeedSave = true;
                table.Save();

                return Ok("单元格已拆分", tableNodeId, extra: new JObject
                {
                    ["row"] = row,
                    ["col"] = col,
                    ["range"] = new JObject
                    {
                        ["top_row"] = merge.TopLeft.Row.Index,
                        ["left_col"] = merge.TopLeft.Column.Index,
                        ["bottom_row"] = merge.BottomRight.Row.Index,
                        ["right_col"] = merge.BottomRight.Column.Index
                    }
                });
            }
            catch (Exception ex) { return ErrorJson("拆分单元格失败: " + ex.Message); }
        }

        // =============================================
        // 行/列移动
        // =============================================

        /// <summary>
        /// 移动行：将位于 fromIndex 的 count 行移动到 toIndex 位置
        /// </summary>
        public static string MoveRow(long tableNodeId, int fromIndex, int toIndex, int count = 1)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                if (count < 1) count = 1;
                if (fromIndex < 0 || fromIndex >= table.Rows.Count)
                    return ErrorJson($"起始行索引无效: {fromIndex}，当前行数: {table.Rows.Count}");
                if (fromIndex + count > table.Rows.Count)
                    return ErrorJson($"要移动的行数超出范围: from={fromIndex}, count={count}, total={table.Rows.Count}");
                if (toIndex < 0 || toIndex > table.Rows.Count)
                    return ErrorJson($"目标位置无效: {toIndex}");

                table.Rows.Move(fromIndex, count, toIndex);
                table.NeedSave = true;
                table.Save();

                return Ok("行已移动", tableNodeId, extra: new JObject
                {
                    ["from_index"] = fromIndex,
                    ["to_index"] = toIndex,
                    ["count"] = count,
                    ["new_row_count"] = table.Rows.Count
                });
            }
            catch (Exception ex) { return ErrorJson("移动行失败: " + ex.Message); }
        }

        /// <summary>
        /// 移动列：将位于 fromIndex 的 count 列移动到 toIndex 位置
        /// </summary>
        public static string MoveColumn(long tableNodeId, int fromIndex, int toIndex, int count = 1)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                if (count < 1) count = 1;
                if (fromIndex < 0 || fromIndex >= table.Columns.Count)
                    return ErrorJson($"起始列索引无效: {fromIndex}，当前列数: {table.Columns.Count}");
                if (fromIndex + count > table.Columns.Count)
                    return ErrorJson($"要移动的列数超出范围: from={fromIndex}, count={count}, total={table.Columns.Count}");
                if (toIndex < 0 || toIndex > table.Columns.Count)
                    return ErrorJson($"目标位置无效: {toIndex}");

                table.Columns.Move(fromIndex, count, toIndex);
                table.NeedSave = true;
                table.Save();

                return Ok("列已移动", tableNodeId, extra: new JObject
                {
                    ["from_index"] = fromIndex,
                    ["to_index"] = toIndex,
                    ["count"] = count,
                    ["new_col_count"] = table.Columns.Count
                });
            }
            catch (Exception ex) { return ErrorJson("移动列失败: " + ex.Message); }
        }

        // =============================================
        // 行高/列宽
        // =============================================

        public static string SetRowHeight(long tableNodeId, int row, int height)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, 0);
                var r = table.Rows[row];
                r.UpdateHeight(height);
                table.Save();
                return Ok("行高已设置", tableNodeId, extra: new JObject { ["row"] = row, ["height"] = r.Height });
            }
            catch (Exception ex) { return ErrorJson("设置行高失败: " + ex.Message); }
        }

        public static string SetColumnWidth(long tableNodeId, int col, int width)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, 0, col);
                var c = table.Columns[col];
                c.UpdateWidth(width);
                table.Save();
                return Ok("列宽已设置", tableNodeId, extra: new JObject { ["col"] = col, ["width"] = c.Width });
            }
            catch (Exception ex) { return ErrorJson("设置列宽失败: " + ex.Message); }
        }

        // =============================================
        // 隐藏/显示
        // =============================================

        public static string SetRowVisible(long tableNodeId, int row, bool visible)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, 0);
                table.Rows[row].UpdateVisible(visible);
                table.Save();
                return Ok(visible ? "行已显示" : "行已隐藏", tableNodeId, extra: new JObject { ["row"] = row, ["visible"] = visible });
            }
            catch (Exception ex) { return ErrorJson("设置行可见性失败: " + ex.Message); }
        }

        public static string SetColumnVisible(long tableNodeId, int col, bool visible)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, 0, col);
                table.Columns[col].UpdateVisible(visible);
                table.Save();
                return Ok(visible ? "列已显示" : "列已隐藏", tableNodeId, extra: new JObject { ["col"] = col, ["visible"] = visible });
            }
            catch (Exception ex) { return ErrorJson("设置列可见性失败: " + ex.Message); }
        }

        // =============================================
        // 行角色 / 列重命名 / 列公式
        // =============================================

        public static string SetRowRole(long tableNodeId, int row, string role)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, 0);
                if (!Enum.TryParse(role, true, out RowRole rr))
                    return ErrorJson($"无效的行角色: {role}（可选: Normal/Header/Fixed/Subtotal/Total/Among/Minus）");
                table.Rows[row].UpdateRole(rr);
                table.Save();
                return Ok("行角色已设置", tableNodeId, extra: new JObject { ["row"] = row, ["role"] = rr.ToString() });
            }
            catch (Exception ex) { return ErrorJson("设置行角色失败: " + ex.Message); }
        }

        public static string RenameColumn(long tableNodeId, int col, string caption)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, 0, col);
                table.Columns[col].UpdateCaption(caption ?? "");
                table.Save();
                return Ok("列标题已更新", tableNodeId, extra: new JObject { ["col"] = col, ["caption"] = caption });
            }
            catch (Exception ex) { return ErrorJson("重命名列失败: " + ex.Message); }
        }

        public static string SetColumnFormula(long tableNodeId, int col, string formula)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, 0, col);
                table.Columns[col].UpdateFormula(formula ?? "");
                table.Save();
                return Ok("列公式已设置", tableNodeId, extra: new JObject { ["col"] = col, ["formula"] = formula });
            }
            catch (Exception ex) { return ErrorJson("设置列公式失败: " + ex.Message); }
        }

        public static string GetColumnFormula(long tableNodeId, int col)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, 0, col);
                var c = table.Columns[col];
                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["col"] = col,
                    ["caption"] = c.Caption ?? "",
                    ["formula"] = c.Formula ?? "",
                    ["caption_formula"] = c.CaptionFormula ?? ""
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取列公式失败: " + ex.Message); }
        }

        // =============================================
        // 锁定/冻结/排序
        // =============================================

        public static string LockTable(long tableNodeId, long lockerId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                table.UpdateLocker(lockerId);
                table.Save();
                return Ok(lockerId == 0 ? "表格已解锁" : "表格已锁定", tableNodeId, extra: new JObject { ["locker"] = lockerId });
            }
            catch (Exception ex) { return ErrorJson("锁定/解锁表格失败: " + ex.Message); }
        }

        public static string SetFrozenColumns(long tableNodeId, int frozenCount)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                if (frozenCount < 0 || frozenCount > table.Columns.Count)
                    return ErrorJson($"冻结列数无效: {frozenCount}（当前列数: {table.Columns.Count}）");
                table.UpdateFrozenCols(frozenCount);
                table.Save();
                return Ok("冻结列已设置", tableNodeId, extra: new JObject { ["frozen_cols"] = frozenCount });
            }
            catch (Exception ex) { return ErrorJson("设置冻结列失败: " + ex.Message); }
        }

        public static string SortTableByColumn(long tableNodeId, int col, bool descending = false)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, 0, col);
                // 按 Column.Sort 排序（仅对数据行排序，不影响标题行/合计行）
                var column = table.Columns[col];
                if (descending) column.SortDescending();
                else column.SortAscending();
                table.NeedSave = true;
                table.Save();
                return Ok(descending ? "已按列降序排序" : "已按列升序排序", tableNodeId, extra: new JObject { ["col"] = col, ["descending"] = descending });
            }
            catch (Exception ex) { return ErrorJson("排序失败: " + ex.Message); }
        }

        // =============================================
        // 控制公式
        // =============================================

        public static string SetControlFormula(long tableNodeId, string formula)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                table.UpdateControlFormula(formula ?? "");
                table.Save();
                return Ok("控制公式已设置", tableNodeId, extra: new JObject { ["control_formula"] = formula });
            }
            catch (Exception ex) { return ErrorJson("设置控制公式失败: " + ex.Message); }
        }

        public static string GetControlFormula(long tableNodeId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["control_formula"] = table.ControlFormula ?? ""
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取控制公式失败: " + ex.Message); }
        }

        // =============================================
        // 表格信息
        // =============================================

        public static string GetTableInfo(long tableNodeId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["table_id"] = table.Id.Value.ToString(),
                    ["title"] = table.Title?.TitleCell?.Value?.ToString() ?? "",
                    ["row_count"] = table.Rows.Count,
                    ["col_count"] = table.Columns.Count,
                    ["frozen_cols"] = table.FrozenCols,
                    ["locker"] = table.Locker,
                    ["header_mode"] = table.HeaderMode.ToString(),
                    ["control_formula"] = table.ControlFormula ?? "",
                    ["merged_cell_count"] = table.MergedCells.Count,
                    ["need_save"] = table.NeedSave
                };
                // 列摘要
                var cols = new JArray();
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    var c = table.Columns[i];
                    cols.Add(new JObject
                    {
                        ["index"] = i,
                        ["caption"] = c?.Caption ?? "",
                        ["width"] = c?.Width ?? 0,
                        ["visible"] = c?.Visible ?? true,
                        ["has_formula"] = !string.IsNullOrEmpty(c?.Formula)
                    });
                }
                result["columns"] = cols;
                // 行摘要
                var rows = new JArray();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var r = table.Rows[i];
                    rows.Add(new JObject
                    {
                        ["index"] = i,
                        ["height"] = r?.Height ?? 0,
                        ["visible"] = r?.Visible ?? true,
                        ["role"] = r?.Role.ToString() ?? "Normal",
                        ["locked"] = (r?.Locker ?? 0) != 0
                    });
                }
                result["rows"] = rows;
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取表格信息失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法（与 TableService 一致）
        // =============================================

        private static Table GetLoadedTable(long tableNodeId)
        {
            SessionState.Current.EnsureProject();
            var project = SessionState.Current.CurrentProject;
            var table = project.GetTableById(new Id64(tableNodeId));
            if (table == null)
                throw new InvalidOperationException($"未找到表格节点: {tableNodeId}");
            table.LoadAndReturn(true);
            SessionState.Current.CurrentTableNodeId = tableNodeId;
            return table;
        }

        private static void ValidateCellIndex(Table table, int row, int col)
        {
            if (row < 0 || row >= table.Rows.Count)
                throw new ArgumentOutOfRangeException(nameof(row), $"行索引无效: {row}，当前行数: {table.Rows.Count}");
            if (col < 0 || col >= table.Columns.Count)
                throw new ArgumentOutOfRangeException(nameof(col), $"列索引无效: {col}，当前列数: {table.Columns.Count}");
        }

        private static string Ok(string msg, long tableNodeId, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["table_node_id"] = tableNodeId, ["message"] = msg };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
