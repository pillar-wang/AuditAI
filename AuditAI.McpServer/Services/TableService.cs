﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Drawing;
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
    /// 表格管理服务
    /// 封装表格数据读取、单元格编辑、行列增删、合并、样式、导出等操作
    /// </summary>
    public static class TableService
    {
        // =============================================
        // 核心方法
        // =============================================

        /// <summary>
        /// 获取表格数据（行列标题 + 单元格值二维数组），支持范围查询
        /// </summary>
        public static string GetTableData(long tableNodeId, int? startRow, int? endRow, int? startCol, int? endCol)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;

                int sr = Clamp(startRow ?? 0, 0, Math.Max(0, rowCount - 1));
                int er = Clamp(endRow ?? rowCount - 1, sr, rowCount - 1);
                int sc = Clamp(startCol ?? 0, 0, Math.Max(0, colCount - 1));
                int ec = Clamp(endCol ?? colCount - 1, sc, colCount - 1);

                // 列标题
                var colHeaders = new JArray();
                for (int c = sc; c <= ec; c++)
                {
                    var col = table.Columns[c];
                    colHeaders.Add(new JObject
                    {
                        ["index"] = c,
                        ["caption"] = col?.Caption ?? "",
                        ["id"] = col?.Id.Value.ToString() ?? ""
                    });
                }

                // 行标题 + 单元格数据
                var rows = new JArray();
                for (int r = sr; r <= er; r++)
                {
                    var row = table.Rows[r];
                    var rowObj = new JObject
                    {
                        ["index"] = r,
                        ["id"] = row?.Id.Value.ToString() ?? "",
                        ["role"] = row?.Role.ToString() ?? "Normal",
                        ["cells"] = new JArray()
                    };

                    for (int c = sc; c <= ec; c++)
                    {
                        var cell = table[r, c];
                        ((JArray)rowObj["cells"]).Add(new JObject
                        {
                            ["value"] = cell?.Value == null ? "" : cell.Value.ToString(),
                            ["formula"] = cell?.Formula ?? "",
                            ["id"] = cell?.Id.Value.ToString() ?? ""
                        });
                    }
                    rows.Add(rowObj);
                }

                // 合并单元格信息
                var merges = new JArray();
                foreach (var m in table.MergedCells)
                {
                    try
                    {
                        merges.Add(new JObject
                        {
                            ["top_row"] = m.TopLeft.Row.Index,
                            ["left_col"] = m.TopLeft.Column.Index,
                            ["bottom_row"] = m.BottomRight.Row.Index,
                            ["right_col"] = m.BottomRight.Column.Index
                        });
                    }
                    catch
                    {
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["table_id"] = table.Id.Value.ToString(),
                    ["title"] = table.Title?.TitleCell?.Value?.ToString() ?? "",
                    ["row_count"] = rowCount,
                    ["col_count"] = colCount,
                    ["range"] = new JObject
                    {
                        ["start_row"] = sr,
                        ["end_row"] = er,
                        ["start_col"] = sc,
                        ["end_col"] = ec
                    },
                    ["column_headers"] = colHeaders,
                    ["rows"] = rows,
                    ["merged_cells"] = merges
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取表格数据失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取单元格值
        /// </summary>
        public static string GetCellValue(long tableNodeId, int row, int col)
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
                    ["value"] = cell?.Value == null ? "" : cell.Value.ToString(),
                    ["formula"] = cell?.Formula ?? "",
                    ["cell_id"] = cell?.Id.Value.ToString() ?? "",
                    ["data_type"] = cell?.Value?.GetType()?.Name ?? "String"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取单元格值失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置单元格值
        /// </summary>
        public static string SetCellValue(long tableNodeId, int row, int col, string value)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);

                var cell = table[row, col];
                if (cell == null)
                {
                    return ErrorJson($"单元格不存在: row={row}, col={col}");
                }

                // 尝试将值转换为合适的类型
                object converted = ConvertValue(value);
                cell.UpdateValue(converted);
                table.NeedSave = true;

                // 保存到数据库
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["row"] = row,
                    ["col"] = col,
                    ["value"] = value,
                    ["data_type"] = converted.GetType().Name,
                    ["message"] = "单元格值已更新并保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置单元格值失败: " + ex.Message);
            }
        }

        // =============================================
        // 行列操作
        // =============================================

        /// <summary>
        /// 添加行
        /// </summary>
        public static string AddRow(long tableNodeId, int? position)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                int insertIndex = position ?? table.Rows.Count;
                if (insertIndex < 0 || insertIndex > table.Rows.Count)
                {
                    return ErrorJson($"插入位置无效: {insertIndex}，当前行数: {table.Rows.Count}");
                }

                table.Rows.Insert(insertIndex, 1);
                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["position"] = insertIndex,
                    ["new_row_count"] = table.Rows.Count,
                    ["message"] = $"已在位置 {insertIndex} 添加 1 行"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("添加行失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 添加列
        /// </summary>
        public static string AddColumn(long tableNodeId, string name, int? position)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                int insertIndex = position ?? table.Columns.Count;
                if (insertIndex < 0 || insertIndex > table.Columns.Count)
                {
                    return ErrorJson($"插入位置无效: {insertIndex}，当前列数: {table.Columns.Count}");
                }

                table.Columns.Insert(insertIndex, 1);

                // 设置列标题
                if (!string.IsNullOrEmpty(name))
                {
                    var newCol = table.Columns[insertIndex];
                    if (newCol != null)
                    {
                        newCol.Caption = name;
                    }
                }

                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["position"] = insertIndex,
                    ["name"] = name ?? "",
                    ["new_col_count"] = table.Columns.Count,
                    ["message"] = $"已在位置 {insertIndex} 添加 1 列"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("添加列失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 删除行
        /// </summary>
        public static string DeleteRow(long tableNodeId, int row)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                if (row < 0 || row >= table.Rows.Count)
                {
                    return ErrorJson($"行索引无效: {row}，当前行数: {table.Rows.Count}");
                }

                table.Rows.Remove(row, 1);
                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["deleted_row"] = row,
                    ["new_row_count"] = table.Rows.Count,
                    ["message"] = $"已删除行 {row}"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("删除行失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 删除列
        /// </summary>
        public static string DeleteColumn(long tableNodeId, int col)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                if (col < 0 || col >= table.Columns.Count)
                {
                    return ErrorJson($"列索引无效: {col}，当前列数: {table.Columns.Count}");
                }

                table.Columns.Remove(col, 1);
                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["deleted_col"] = col,
                    ["new_col_count"] = table.Columns.Count,
                    ["message"] = $"已删除列 {col}"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("删除列失败: " + ex.Message);
            }
        }

        // =============================================
        // 合并与样式
        // =============================================

        /// <summary>
        /// 合并单元格
        /// </summary>
        public static string MergeCells(long tableNodeId, int startRow, int startCol, int endRow, int endCol)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, startRow, startCol);
                ValidateCellIndex(table, endRow, endCol);

                if (startRow > endRow || startCol > endCol)
                {
                    return ErrorJson($"合并范围无效: 起始({startRow},{startCol}) 终止({endRow},{endCol})");
                }

                table.MergeCells(startRow, startCol, endRow, endCol);
                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["start_row"] = startRow,
                    ["start_col"] = startCol,
                    ["end_row"] = endRow,
                    ["end_col"] = endCol,
                    ["message"] = $"已合并区域 ({startRow},{startCol})-({endRow},{endCol})"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("合并单元格失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置单元格样式
        /// </summary>
        public static string SetCellStyle(long tableNodeId, int row, int col, JObject style)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                ValidateCellIndex(table, row, col);

                var cell = table[row, col];
                if (cell == null)
                {
                    return ErrorJson($"单元格不存在: row={row}, col={col}");
                }

                // 通过样式池获取或创建新样式
                var newStyle = table.CellStyles.MutateAndGet(cell.Style, s =>
                {
                    if (style["font_family"] != null)
                        s.FontFamily = style["font_family"].ToString();
                    if (style["font_size"] != null)
                        s.FontSize = (float?)style["font_size"].Value<double?>();
                    if (style["bold"] != null)
                        s.Bold = style["bold"].Value<bool?>();
                    if (style["italic"] != null)
                        s.Italic = style["italic"].Value<bool?>();
                    if (style["underline"] != null)
                        s.Underline = style["underline"].Value<bool?>();
                    if (style["align"] != null)
                        s.Align = ParseAlign(style["align"].ToString());
                    if (style["fore_color"] != null)
                        s.ForeColor = ParseColor(style["fore_color"].ToString());
                    if (style["back_color"] != null)
                        s.BackColor = ParseColor(style["back_color"].ToString());
                    if (style["margin"] != null)
                        s.Margin = style["margin"].Value<int?>();
                });

                cell.UpdateStyle(newStyle);
                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["row"] = row,
                    ["col"] = col,
                    ["style"] = style,
                    ["message"] = "单元格样式已更新并保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置单元格样式失败: " + ex.Message);
            }
        }

        // =============================================
        // 导出
        // =============================================

        /// <summary>
        /// 导出表格为 Excel 文件
        /// </summary>
        public static string ExportTable(long tableNodeId, string outputPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    return ErrorJson("输出路径不能为空");
                }

                Table table = GetLoadedTable(tableNodeId);

                // 确保目录存在
                string dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // 使用 C1.C1Excel 导出
                var xlBook = new C1XLBook();
                XLSheet xlSheet = xlBook.Sheets[0];

                // 设置 sheet 名称
                string sheetName = $"表格 {tableNodeId}";
                try { xlSheet.Name = sheetName; } catch { }

                // 列标题样式（加粗）
                var headerStyle = new XLStyle(xlBook);
                headerStyle.Font = new Font("宋体", 9f, FontStyle.Bold);

                // 写入列标题（第 0 行）
                for (int c = 0; c < table.Columns.Count; c++)
                {
                    var col = table.Columns[c];
                    XLCell xlCell = xlSheet[0, c];
                    xlCell.Value = col?.Caption ?? $"列{c}";
                    xlCell.Style = headerStyle;
                }

                // 写入单元格数据
                for (int r = 0; r < table.Rows.Count; r++)
                {
                    for (int c = 0; c < table.Columns.Count; c++)
                    {
                        var cell = table[r, c];
                        XLCell xlCell = xlSheet[r + 1, c];
                        xlCell.Value = ConvertForExcel(cell?.Value);
                    }
                }

                // 保存文件
                using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    xlBook.Save(stream, FileFormat.OpenXml);
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["output_path"] = outputPath,
                    ["row_count"] = table.Rows.Count,
                    ["col_count"] = table.Columns.Count,
                    ["message"] = "表格已导出为 Excel 文件"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("导出表格失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        /// <summary>
        /// 根据节点 ID 获取已加载的表格
        /// </summary>
        private static Table GetLoadedTable(long tableNodeId)
        {
            SessionState.Current.EnsureProject();
            var project = SessionState.Current.CurrentProject;

            var table = project.GetTableById(new Auditai.DTO.Id64(tableNodeId));
            if (table == null)
            {
                throw new InvalidOperationException($"未找到表格节点: {tableNodeId}");
            }

            // 确保表格已加载
            table.LoadAndReturn(true);

            // 更新当前表格上下文
            SessionState.Current.CurrentTableNodeId = tableNodeId;

            return table;
        }

        /// <summary>
        /// 验证单元格索引
        /// </summary>
        private static void ValidateCellIndex(Table table, int row, int col)
        {
            if (row < 0 || row >= table.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(row), $"行索引无效: {row}，当前行数: {table.Rows.Count}");
            }
            if (col < 0 || col >= table.Columns.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(col), $"列索引无效: {col}，当前列数: {table.Columns.Count}");
            }
        }

        /// <summary>
        /// 将数值限制在范围内
        /// </summary>
        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 尝试将字符串值转换为合适的类型
        /// </summary>
        private static object ConvertValue(string value)
        {
            if (value == null) return string.Empty;

            // 尝试解析为 double
            if (double.TryParse(value, out double num))
            {
                return num;
            }
            // 尝试解析为 bool
            if (bool.TryParse(value, out bool b))
            {
                return b;
            }
            // 尝试解析为 DateTime
            if (DateTime.TryParse(value, out DateTime dt))
            {
                return dt;
            }
            return value;
        }

        /// <summary>
        /// 将单元格值转换为 Excel 兼容类型
        /// </summary>
        private static object ConvertForExcel(object value)
        {
            if (value == null) return string.Empty;
            if (value is string s) return s;
            if (value is double || value is int || value is long || value is float || value is decimal)
                return Convert.ToDouble(value);
            if (value is bool b) return b;
            if (value is DateTime dt) return dt;
            return value.ToString();
        }

        /// <summary>
        /// 解析对齐方式字符串
        /// </summary>
        private static CellTextAlign? ParseAlign(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            if (Enum.TryParse(s, true, out CellTextAlign align)) return align;
            // 支持简写
            switch (s.ToLowerInvariant())
            {
                case "left": return CellTextAlign.MiddleLeft;
                case "center": return CellTextAlign.MiddleCenter;
                case "right": return CellTextAlign.MiddleRight;
                default: return null;
            }
        }

        /// <summary>
        /// 解析颜色字符串（支持 #RRGGBB、颜色名称）
        /// </summary>
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
                    if (hex.Length == 8)
                    {
                        int a = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int r = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(4, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(6, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
                return Color.FromName(s);
            }
            catch
            {
                return null;
            }
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
