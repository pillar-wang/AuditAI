﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
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
    /// 表格结构服务
    /// 提供表格结构查询、单元格范围读写等能力
    /// </summary>
    public static class TableStructureService
    {
        /// <summary>
        /// 获取表格结构信息（列数、行数、列列表、合并单元格、行类型分布、是否有公式列）
        /// </summary>
        public static string GetTableStructure(long tableNodeId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;

                // 列信息
                var columns = new JArray();
                for (int c = 0; c < colCount; c++)
                {
                    var col = table.Columns[c];
                    columns.Add(new JObject
                    {
                        ["index"] = c,
                        ["caption"] = col?.Caption ?? "",
                        ["id"] = col?.Id.Value.ToString() ?? ""
                    });
                }

                // 合并单元格
                var mergedCells = new JArray();
                foreach (var m in table.MergedCells)
                {
                    try
                    {
                        mergedCells.Add(new JObject
                        {
                            ["top_row"] = m.TopLeft.Row.Index,
                            ["left_col"] = m.TopLeft.Column.Index,
                            ["bottom_row"] = m.BottomRight.Row.Index,
                            ["right_col"] = m.BottomRight.Column.Index
                        });
                    }
                    catch { }
                }

                // 行类型分布
                var rowTypeDist = new JObject();
                for (int r = 0; r < rowCount; r++)
                {
                    var role = table.Rows[r]?.Role.ToString() ?? "Normal";
                    if (rowTypeDist[role] == null)
                        rowTypeDist[role] = 0;
                    rowTypeDist[role] = rowTypeDist.Value<int>(role) + 1;
                }

                // 是否有公式列
                bool hasFormulaColumns = false;
                for (int c = 0; c < colCount && !hasFormulaColumns; c++)
                {
                    for (int r = 0; r < rowCount && !hasFormulaColumns; r++)
                    {
                        var cell = table[r, c];
                        if (cell != null && !string.IsNullOrEmpty(cell.Formula))
                            hasFormulaColumns = true;
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["col_count"] = colCount,
                    ["row_count"] = rowCount,
                    ["columns"] = columns,
                    ["merged_cells"] = mergedCells,
                    ["row_type_distribution"] = rowTypeDist,
                    ["has_formula_columns"] = hasFormulaColumns
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取表格结构失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取指定范围的单元格值，返回二维行数组结构
        /// </summary>
        public static string GetCellRange(long tableNodeId, int startRow, int endRow, int startCol, int endCol)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;

                int sr = Clamp(startRow, 0, Math.Max(0, rowCount - 1));
                int er = Clamp(endRow, sr, rowCount - 1);
                int sc = Clamp(startCol, 0, Math.Max(0, colCount - 1));
                int ec = Clamp(endCol, sc, colCount - 1);

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

                // 行数据
                var rows = new JArray();
                for (int r = sr; r <= er; r++)
                {
                    var cells = new JArray();
                    for (int c = sc; c <= ec; c++)
                    {
                        var cell = table[r, c];
                        cells.Add(new JObject
                        {
                            ["value"] = cell?.Value == null ? "" : cell.Value.ToString(),
                            ["formula"] = cell?.Formula ?? ""
                        });
                    }
                    rows.Add(new JObject
                    {
                        ["index"] = r,
                        ["cells"] = cells
                    });
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["range"] = new JObject
                    {
                        ["start_row"] = sr,
                        ["end_row"] = er,
                        ["start_col"] = sc,
                        ["end_col"] = ec
                    },
                    ["column_headers"] = colHeaders,
                    ["rows"] = rows
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取单元格范围失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 批量写入指定范围的单元格值
        /// </summary>
        public static string SetCellRange(long tableNodeId, int startRow, int startCol, JArray data)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                for (int r = 0; r < data.Count; r++)
                {
                    var rowData = data[r] as JArray;
                    if (rowData == null) continue;

                    for (int c = 0; c < rowData.Count; c++)
                    {
                        int targetRow = startRow + r;
                        int targetCol = startCol + c;

                        if (targetRow < 0 || targetRow >= table.Rows.Count ||
                            targetCol < 0 || targetCol >= table.Columns.Count)
                            continue;

                        var cell = table[targetRow, targetCol];
                        if (cell == null) continue;

                        JToken valueToken = rowData[c];
                        cell.UpdateValue(ConvertValue(valueToken));
                    }
                }

                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["start_row"] = startRow,
                    ["start_col"] = startCol,
                    ["rows_written"] = data.Count,
                    ["message"] = $"已写入 {data.Count} 行数据并保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置单元格范围失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static Table GetLoadedTable(long tableNodeId)
        {
            SessionState.Current.EnsureProject();
            var project = SessionState.Current.CurrentProject;

            var table = project.GetTableById(new Auditai.DTO.Id64(tableNodeId));
            if (table == null)
            {
                throw new InvalidOperationException($"未找到表格节点: {tableNodeId}");
            }

            table.LoadAndReturn(true);
            SessionState.Current.CurrentTableNodeId = tableNodeId;
            return table;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 将 JToken 值转换为合适的类型
        /// </summary>
        private static object ConvertValue(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return string.Empty;

            switch (token.Type)
            {
                case JTokenType.String:
                    string s = token.ToString();
                    if (double.TryParse(s, out double num)) return num;
                    if (bool.TryParse(s, out bool b)) return b;
                    if (DateTime.TryParse(s, out DateTime dt)) return dt;
                    return s;
                case JTokenType.Integer:
                    return (double)token.Value<long>();
                case JTokenType.Float:
                    return token.Value<double>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                default:
                    return token.ToString();
            }
        }

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