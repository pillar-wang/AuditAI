using System;
using System.Drawing;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Table = Auditai.Model.Table;
using CellStyle = Auditai.Model.CellStyle;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 单元格格式服务
    /// 封装单元格数据格式、零值格式、批注、样式复制、列默认样式、批量样式等操作
    /// </summary>
    public static class CellFormatService
    {
        // =============================================
        // 数据格式
        // =============================================

        /// <summary>
        /// 设置单元格数据格式（数字/日期/百分比/布尔/文本等）
        /// </summary>
        public static string SetCellDataFormat(long tableNodeId, int row, int col, string formatType, int decimalLength = 2, string zeroFormat = "zero")
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);

                if (!Enum.TryParse(formatType, true, out DataFormatType ft))
                    return ErrorJson($"无效的数据格式类型: {formatType}（可选: Text/Number/Percent/Currency/Date/Time/Boolean/Enum 等）");

                if (!Enum.TryParse(zeroFormat, true, out ZeroFormat zf))
                    zf = ZeroFormat.Zero;

                var cell = table[row, col];
                var style = cell.Style ?? new CellStyle();
                style.Format = new DataFormat(ft)
                {
                    DecimalLength = decimalLength,
                    ZeroFormat = zf
                };
                cell.UpdateStyle(style);
                table.NeedSave = true;
                table.Save();

                return Ok("单元格数据格式已设置", tableNodeId, new JObject
                {
                    ["row"] = row, ["col"] = col,
                    ["format_type"] = ft.ToString(),
                    ["decimal_length"] = decimalLength,
                    ["zero_format"] = zf.ToString()
                });
            }
            catch (Exception ex) { return ErrorJson("设置数据格式失败: " + ex.Message); }
        }

        /// <summary>
        /// 设置零值显示格式（zero/empty/dash）
        /// </summary>
        public static string SetCellZeroFormat(long tableNodeId, int row, int col, string zeroFormat)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);

                if (!Enum.TryParse(zeroFormat, true, out ZeroFormat zf))
                    return ErrorJson($"无效的零值格式: {zeroFormat}（可选: Zero/Empty/Dash）");

                var cell = table[row, col];
                var style = cell.Style ?? new CellStyle();
                var fmt = style.Format ?? new DataFormat(DataFormatType.Number);
                fmt.ZeroFormat = zf;
                style.Format = fmt;
                cell.UpdateStyle(style);
                table.NeedSave = true;
                table.Save();

                return Ok("零值显示格式已设置", tableNodeId, new JObject
                {
                    ["row"] = row, ["col"] = col, ["zero_format"] = zf.ToString()
                });
            }
            catch (Exception ex) { return ErrorJson("设置零值格式失败: " + ex.Message); }
        }

        // =============================================
        // 单元格批注
        // =============================================

        public static string SetCellComment(long tableNodeId, int row, int col, string comment)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);
                var cell = table[row, col];
                var style = cell.Style ?? new CellStyle();
                style.Comment = comment ?? "";
                cell.UpdateStyle(style);
                table.NeedSave = true;
                table.Save();
                return Ok("批注已设置", tableNodeId, new JObject
                {
                    ["row"] = row, ["col"] = col, ["comment"] = comment
                });
            }
            catch (Exception ex) { return ErrorJson("设置批注失败: " + ex.Message); }
        }

        public static string GetCellComment(long tableNodeId, int row, int col)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);
                var cell = table[row, col];
                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["row"] = row,
                    ["col"] = col,
                    ["comment"] = cell?.Style?.Comment ?? ""
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取批注失败: " + ex.Message); }
        }

        // =============================================
        // 单元格样式读取
        // =============================================

        public static string GetCellStyle(long tableNodeId, int row, int col)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);
                var cell = table[row, col];
                var style = cell?.Style;
                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["row"] = row,
                    ["col"] = col,
                    ["cell_id"] = cell?.Id.Value.ToString() ?? "",
                    ["font_family"] = style?.FontFamily ?? "",
                    ["font_size"] = style?.FontSize ?? 0,
                    ["bold"] = style?.Bold ?? false,
                    ["italic"] = style?.Italic ?? false,
                    ["underline"] = style?.Underline ?? false,
                    ["align"] = style?.Align?.ToString() ?? "",
                    ["fore_color"] = style?.ForeColor != null ? ColorToHex(style.ForeColor.Value) : "",
                    ["back_color"] = style?.BackColor != null ? ColorToHex(style.BackColor.Value) : "",
                    ["margin"] = style?.Margin ?? 0,
                    ["comment"] = style?.Comment ?? "",
                    ["default_value"] = style?.DefaultValue ?? "",
                    ["format_type"] = style?.Format?.FormatType.ToString() ?? "",
                    ["decimal_length"] = style?.Format?.DecimalLength ?? 0,
                    ["zero_format"] = style?.Format?.ZeroFormat.ToString() ?? "Zero"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取单元格样式失败: " + ex.Message); }
        }

        // =============================================
        // 复制样式（格式刷）
        // =============================================

        /// <summary>
        /// 复制源单元格样式到目标单元格区域
        /// </summary>
        public static string CopyCellStyle(long tableNodeId, int srcRow, int srcCol, int dstStartRow, int dstEndRow, int dstStartCol, int dstEndCol)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, srcRow, srcCol);
                var srcStyle = table[srcRow, srcCol]?.Style;
                if (srcStyle == null)
                    return ErrorJson($"源单元格({srcRow},{srcCol})无样式");

                int copied = 0;
                for (int r = dstStartRow; r <= dstEndRow; r++)
                {
                    for (int c = dstStartCol; c <= dstEndCol; c++)
                    {
                        if (r < 0 || r >= table.Rows.Count || c < 0 || c >= table.Columns.Count) continue;
                        var cell = table[r, c];
                        cell.UpdateStyle(srcStyle);
                        copied++;
                    }
                }
                table.NeedSave = true;
                table.Save();

                return Ok($"样式已复制到 {copied} 个单元格", tableNodeId, new JObject
                {
                    ["source_row"] = srcRow, ["source_col"] = srcCol,
                    ["target_range"] = new JObject
                    {
                        ["start_row"] = dstStartRow, ["end_row"] = dstEndRow,
                        ["start_col"] = dstStartCol, ["end_col"] = dstEndCol
                    },
                    ["copied_count"] = copied
                });
            }
            catch (Exception ex) { return ErrorJson("复制样式失败: " + ex.Message); }
        }

        // =============================================
        // 列默认样式
        // =============================================

        public static string SetColumnStyle(long tableNodeId, int col, JObject styleObj)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, 0, col);
                var column = table.Columns[col];
                var style = column.Style ?? new CellStyle();
                ApplyStyleObj(style, styleObj);
                column.UpdateStyle(style);
                table.NeedSave = true;
                table.Save();
                return Ok("列默认样式已设置", tableNodeId, new JObject { ["col"] = col });
            }
            catch (Exception ex) { return ErrorJson("设置列样式失败: " + ex.Message); }
        }

        // =============================================
        // 批量样式
        // =============================================

        /// <summary>
        /// 批量设置单元格区域样式
        /// </summary>
        public static string BatchSetCellStyle(long tableNodeId, int startRow, int endRow, int startCol, int endCol, JObject styleObj)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                int updated = 0;
                for (int r = startRow; r <= endRow; r++)
                {
                    for (int c = startCol; c <= endCol; c++)
                    {
                        if (r < 0 || r >= table.Rows.Count || c < 0 || c >= table.Columns.Count) continue;
                        var cell = table[r, c];
                        var style = cell.Style ?? new CellStyle();
                        ApplyStyleObj(style, styleObj);
                        cell.UpdateStyle(style);
                        updated++;
                    }
                }
                table.NeedSave = true;
                table.Save();
                return Ok($"已批量更新 {updated} 个单元格的样式", tableNodeId, new JObject
                {
                    ["updated"] = updated,
                    ["range"] = new JObject
                    {
                        ["start_row"] = startRow, ["end_row"] = endRow,
                        ["start_col"] = startCol, ["end_col"] = endCol
                    }
                });
            }
            catch (Exception ex) { return ErrorJson("批量设置样式失败: " + ex.Message); }
        }

        // =============================================
        // 辅助
        // =============================================

        private static void ApplyStyleObj(CellStyle style, JObject obj)
        {
            if (obj == null) return;
            if (obj["font_family"] != null) style.FontFamily = obj["font_family"].ToString();
            if (obj["font_size"] != null) style.FontSize = (float)obj["font_size"].Value<double>();
            if (obj["bold"] != null) style.Bold = obj["bold"].Value<bool>();
            if (obj["italic"] != null) style.Italic = obj["italic"].Value<bool>();
            if (obj["underline"] != null) style.Underline = obj["underline"].Value<bool>();
            if (obj["align"] != null)
            {
                if (Enum.TryParse(obj["align"].ToString(), true, out CellTextAlign a)) style.Align = a;
            }
            if (obj["fore_color"] != null)
            {
                var c = ParseColor(obj["fore_color"].ToString());
                if (c.HasValue) style.ForeColor = c;
            }
            if (obj["back_color"] != null)
            {
                var c = ParseColor(obj["back_color"].ToString());
                if (c.HasValue) style.BackColor = c;
            }
            if (obj["margin"] != null) style.Margin = obj["margin"].Value<int>();
            if (obj["comment"] != null) style.Comment = obj["comment"].ToString();
            if (obj["default_value"] != null) style.DefaultValue = obj["default_value"].ToString();
        }

        private static Color? ParseColor(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            try
            {
                if (s.StartsWith("#"))
                {
                    string hex = s.Substring(1);
                    if (hex.Length == 6)
                    {
                        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                        return Color.FromArgb(r, g, b);
                    }
                }
                return Color.FromName(s);
            }
            catch { return null; }
        }

        private static string ColorToHex(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

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
