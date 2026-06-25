﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using Auditai.DTO;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Table = Auditai.Model.Table;
using Cell = Auditai.Model.Cell;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 公式检查服务
    /// 提供公式清单提取与公式错误检查等审查能力
    /// </summary>
    public static class FormulaInspectionService
    {
        // =============================================
        // 获取所有公式
        // =============================================

        /// <summary>
        /// 获取指定表格中的所有公式
        /// 遍历所有单元格，收集非空公式的表达式及当前计算值
        /// </summary>
        public static string GetAllFormulas(long tableNodeId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;

                var formulas = new JArray();
                for (int r = 0; r < rowCount; r++)
                {
                    var row = table.Rows[r];
                    if (row == null) continue;

                    for (int c = 0; c < colCount; c++)
                    {
                        var cell = table[r, c];
                        if (cell == null) continue;

                        string formula = cell.Formula ?? "";
                        if (string.IsNullOrEmpty(formula)) continue;

                        formulas.Add(new JObject
                        {
                            ["row"] = r,
                            ["col"] = c,
                            ["formula"] = formula,
                            ["value"] = cell.Value == null ? "" : cell.Value.ToString(),
                            ["cell_id"] = cell.Id.Value.ToString(),
                            ["row_role"] = row.Role.ToString()
                        });
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["table_id"] = table.Id.Value.ToString(),
                    ["total_formula_count"] = formulas.Count,
                    ["formulas"] = formulas
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取所有公式失败: " + ex.Message);
            }
        }

        // =============================================
        // 检查公式错误
        // =============================================

        /// <summary>
        /// 检查指定表格中所有公式的错误情况
        /// 包括公式求值错误值、循环引用等
        /// </summary>
        public static string CheckFormulaErrors(long tableNodeId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                int rowCount = table.Rows.Count;
                int colCount = table.Columns.Count;

                var errorCells = new JArray();
                int totalFormulaCount = 0;
                var errorTypeCounts = new Dictionary<string, int>();

                for (int r = 0; r < rowCount; r++)
                {
                    var row = table.Rows[r];
                    if (row == null) continue;

                    for (int c = 0; c < colCount; c++)
                    {
                        var cell = table[r, c];
                        if (cell == null) continue;

                        string formula = cell.Formula ?? "";
                        if (string.IsNullOrEmpty(formula)) continue;

                        totalFormulaCount++;

                        string valStr = cell.Value == null ? "" : cell.Value.ToString();

                        // 检查公式求值错误值
                        string errorType = DetectErrorType(valStr);
                        if (errorType != null)
                        {
                            string errorDesc = GetErrorDescription(errorType, valStr);
                            errorCells.Add(new JObject
                            {
                                ["row"] = r,
                                ["col"] = c,
                                ["formula"] = formula,
                                ["value"] = valStr,
                                ["error_type"] = errorType,
                                ["error_description"] = errorDesc
                            });

                            if (!errorTypeCounts.ContainsKey(errorType))
                                errorTypeCounts[errorType] = 0;
                            errorTypeCounts[errorType]++;
                            continue; // 已有错误值，不再检查循环引用
                        }

                        // 检查循环引用
                        if (HasCircularReference(table, cell))
                        {
                            errorCells.Add(new JObject
                            {
                                ["row"] = r,
                                ["col"] = c,
                                ["formula"] = formula,
                                ["value"] = valStr,
                                ["error_type"] = "circular_reference",
                                ["error_description"] = "检测到循环引用"
                            });

                            if (!errorTypeCounts.ContainsKey("circular_reference"))
                                errorTypeCounts["circular_reference"] = 0;
                            errorTypeCounts["circular_reference"]++;
                        }
                    }
                }

                // 构建错误类型统计
                var errorTypeSummary = new JObject();
                foreach (var kv in errorTypeCounts)
                {
                    errorTypeSummary[kv.Key] = kv.Value;
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["table_id"] = table.Id.Value.ToString(),
                    ["total_formula_count"] = totalFormulaCount,
                    ["error_count"] = errorCells.Count,
                    ["errors"] = errorCells,
                    ["summary"] = new JObject
                    {
                        ["total_formulas"] = totalFormulaCount,
                        ["total_errors"] = errorCells.Count,
                        ["error_types"] = errorTypeSummary
                    }
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("检查公式错误失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        /// <summary>
        /// 检测值字符串中的错误类型
        /// </summary>
        private static string DetectErrorType(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            if (value.StartsWith("#REF!")) return "#REF!";
            if (value.StartsWith("#VALUE!")) return "#VALUE!";
            if (value.StartsWith("#DIV/0!")) return "#DIV/0!";
            if (value.StartsWith("#N/A")) return "#N/A";
            if (value.StartsWith("#NAME?")) return "#NAME?";
            if (value.StartsWith("#NULL!")) return "#NULL!";
            if (value.StartsWith("#NUM!")) return "#NUM!";

            return null;
        }

        /// <summary>
        /// 获取错误类型的描述信息
        /// </summary>
        private static string GetErrorDescription(string errorType, string value)
        {
            switch (errorType)
            {
                case "#REF!":
                    return "公式引用了无效的单元格（可能已被删除）: " + value;
                case "#VALUE!":
                    return "公式使用了错误的参数或操作数类型: " + value;
                case "#DIV/0!":
                    return "公式被零除: " + value;
                case "#N/A":
                    return "公式找不到可用的引用值: " + value;
                case "#NAME?":
                    return "公式包含无法识别的名称: " + value;
                case "#NULL!":
                    return "公式指定了无效的交叉引用: " + value;
                case "#NUM!":
                    return "公式存在数值问题（如数值过大）: " + value;
                default:
                    return "公式求值异常: " + value;
            }
        }

        /// <summary>
        /// 检测单元格是否存在循环引用
        /// 通过公式引用解析检查直接自引用，以及公式解析失败时视为可能的循环引用
        /// </summary>
        private static bool HasCircularReference(Table table, Cell cell)
        {
            try
            {
                if (string.IsNullOrEmpty(cell.Formula)) return false;

                var resolver = new FormulaReferenceModelResolver(table.Project);
                var evaluator = new FormulaEvaluator(cell.Formula);
                var refs = evaluator.GetReferences(resolver);

                // 检查直接循环引用：公式引用了自身所在单元格
                foreach (var cr in refs.CellReferences)
                {
                    if (cr?.Row?.Table?.Id.Value == table.Id.Value &&
                        cr.Row.Index == cell.Row.Index &&
                        cr.Column?.Index == cell.Column?.Index)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (FormulaException)
            {
                // 公式解析失败可能是循环引用导致的
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 根据节点 ID 获取已加载的表格
        /// </summary>
        private static Table GetLoadedTable(long tableNodeId)
        {
            SessionState.Current.EnsureProject();
            var project = SessionState.Current.CurrentProject;

            var table = project.GetTableById(new Id64(tableNodeId));
            if (table == null)
            {
                throw new InvalidOperationException($"未找到表格节点: {tableNodeId}");
            }

            table.LoadAndReturn(true);
            SessionState.Current.CurrentTableNodeId = tableNodeId;

            return table;
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