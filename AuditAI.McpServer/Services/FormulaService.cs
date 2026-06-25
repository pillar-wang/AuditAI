﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Table = Auditai.Model.Table;
using Row = Auditai.Model.Row;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 公式服务
    /// 封装单元格公式设置/获取、公式求值、表格重算及依赖关系查询等操作
    /// </summary>
    public static class FormulaService
    {
        // =============================================
        // 公式设置与获取
        // =============================================

        /// <summary>
        /// 设置单元格公式
        /// 调用 Cell.UpdateFormula，会自动求值、更新依赖图并标记保存
        /// </summary>
        public static string SetCellFormula(long tableNodeId, int row, int col, string formula)
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

                if (formula == null)
                {
                    formula = string.Empty;
                }

                // UpdateFormula 会设置 Formula、应用求值、更新依赖、写入 FormulaManager
                cell.UpdateFormula(formula);
                table.NeedSave = true;
                table.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["row"] = row,
                    ["col"] = col,
                    ["formula"] = formula,
                    ["has_formula"] = !string.IsNullOrEmpty(formula),
                    ["value"] = cell.Value == null ? "" : cell.Value.ToString(),
                    ["cell_id"] = cell.Id.Value.ToString(),
                    ["message"] = string.IsNullOrEmpty(formula)
                        ? "已清除单元格公式"
                        : "单元格公式已设置并求值"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置单元格公式失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取单元格公式
        /// </summary>
        public static string GetCellFormula(long tableNodeId, int row, int col)
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

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["row"] = row,
                    ["col"] = col,
                    ["formula"] = cell.Formula ?? "",
                    ["has_formula"] = cell.HasFormula,
                    ["header_formula"] = cell.HeaderFormula ?? "",
                    ["has_header_formula"] = cell.HasHeaderFormula,
                    ["value"] = cell.Value == null ? "" : cell.Value.ToString(),
                    ["cell_id"] = cell.Id.Value.ToString()
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取单元格公式失败: " + ex.Message);
            }
        }

        // =============================================
        // 公式求值
        // =============================================

        /// <summary>
        /// 在指定表格上下文中求值公式表达式，返回求值结果
        /// </summary>
        public static string EvaluateFormula(long tableNodeId, string formula)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(formula))
                {
                    return ErrorJson("公式不能为空");
                }

                Table table = GetLoadedTable(tableNodeId);

                // 构造求值环境（参考 Cell.ApplyFormula 的实现）
                FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(table.Project);
                FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
                {
                    Resolver = resolver,
                    RowIndex = ResolveEvaluationRowIndex(table),
                    HostTable = table,
                    RefManager = table.Project.DataReferenceManager,
                    IsIgnoreColSheetFunBadRefrence = true,
                    RefEvalContext = new DataReferenceEvaluationContext
                    {
                        Project = table.Project,
                        CurrentTreeNode = table.TreeNode
                    }
                };

                FormulaEvaluator evaluator = new FormulaEvaluator(formula)
                {
                    Env = env
                };

                object evalResult = evaluator.Evaluate();
                string resultStr = evalResult == null ? "" : evalResult.ToString();

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["formula"] = formula,
                    ["result"] = resultStr,
                    ["result_type"] = evalResult?.GetType()?.Name ?? "Null"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (FormulaException fex)
            {
                return ErrorJson("公式求值失败（公式异常）: " + fex.Message);
            }
            catch (Exception ex)
            {
                return ErrorJson("公式求值失败: " + ex.Message);
            }
        }

        // =============================================
        // 表格重算
        // =============================================

        /// <summary>
        /// 计算整个表格，触发所有公式的依赖图重算
        /// 返回发生变更的行信息
        /// </summary>
        public static string CalculateTable(long tableNodeId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);

                // TryApplyFormula 重算所有列公式、表头公式与单元格公式，返回受影响行
                List<Row> changedRows = table.TryApplyFormula(evalLqDistinct: true);
                table.EvalControlFormula();
                FormulaEvaluator.ClearCache();
                table.NeedSave = true;
                table.Save();

                var changedRowsArr = new JArray();
                if (changedRows != null)
                {
                    foreach (var r in changedRows)
                    {
                        changedRowsArr.Add(new JObject
                        {
                            ["index"] = r?.Index ?? -1,
                            ["id"] = r?.Id.Value.ToString() ?? "",
                            ["role"] = r?.Role.ToString() ?? "Normal"
                        });
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["table_id"] = table.Id.Value.ToString(),
                    ["changed_row_count"] = changedRowsArr.Count,
                    ["changed_rows"] = changedRowsArr,
                    ["message"] = $"表格重算完成，共 {changedRowsArr.Count} 行发生变更"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("计算表格失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 计算当前项目所有表格
        /// </summary>
        public static string CalculateAllTables()
        {
            try
            {
                SessionState.Current.EnsureProject();
                var project = SessionState.Current.CurrentProject;

                var tableNodes = project.GetAllTableNodes().ToList();
                var calculated = new JArray();
                int successCount = 0;
                int failCount = 0;

                foreach (var node in tableNodes)
                {
                    try
                    {
                        var table = node.Table;
                        if (table == null) continue;

                        // 确保表格已加载
                        table.LoadAndReturn(true);

                        table.TryApplyFormula(evalLqDistinct: true);
                        table.EvalControlFormula();
                        table.NeedSave = true;
                        table.Save();

                        calculated.Add(new JObject
                        {
                            ["table_node_id"] = node.Id.Value,
                            ["title"] = table.Title?.TitleCell?.Value?.ToString() ?? "",
                            ["status"] = "success"
                        });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        calculated.Add(new JObject
                        {
                            ["table_node_id"] = node?.Id.Value ?? 0,
                            ["title"] = "",
                            ["status"] = "failed",
                            ["error"] = ex.Message
                        });
                        failCount++;
                    }
                }

                FormulaEvaluator.ClearCache();

                var result = new JObject
                {
                    ["success"] = true,
                    ["total_tables"] = tableNodes.Count,
                    ["success_count"] = successCount,
                    ["failed_count"] = failCount,
                    ["tables"] = calculated,
                    ["message"] = $"已计算 {successCount}/{tableNodes.Count} 个表格"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("计算所有表格失败: " + ex.Message);
            }
        }

        // =============================================
        // 依赖关系
        // =============================================

        /// <summary>
        /// 获取单元格公式的依赖关系
        /// 包含：该单元格公式引用了哪些单元格/列（前置依赖），以及哪些单元格引用了该单元格（反向引用）
        /// </summary>
        public static string GetFormulaDependencies(long tableNodeId, int row, int col)
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

                // 前置依赖：该单元格公式引用了哪些对象
                var precedents = new JArray();
                if (cell.HasFormula)
                {
                    try
                    {
                        FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(table.Project);
                        FormulaEvaluator evaluator = new FormulaEvaluator(cell.Formula);
                        FormulaReferences refs = evaluator.GetReferences(resolver);

                        foreach (var c in refs.CellReferences)
                        {
                            precedents.Add(new JObject
                            {
                                ["kind"] = "Cell",
                                ["table_id"] = c?.Row?.Table?.Id.Value.ToString() ?? "",
                                ["table_node_id"] = c?.Row?.Table?.TreeNode?.Id.Value ?? 0,
                                ["row"] = c?.Row?.Index ?? -1,
                                ["col"] = c?.Column?.Index ?? -1
                            });
                        }
                        foreach (var cl in refs.ColumnReferences)
                        {
                            precedents.Add(new JObject
                            {
                                ["kind"] = "Column",
                                ["table_id"] = cl?.Table?.Id.Value.ToString() ?? "",
                                ["table_node_id"] = cl?.Table?.TreeNode?.Id.Value ?? 0,
                                ["caption"] = cl?.Caption ?? ""
                            });
                        }
                        foreach (var hc in refs.HeaderCellReferences)
                        {
                            precedents.Add(new JObject
                            {
                                ["kind"] = "HeaderCell",
                                ["table_id"] = hc?.Row?.Table?.Id.Value.ToString() ?? "",
                                ["table_node_id"] = hc?.Row?.Table?.TreeNode?.Id.Value ?? 0,
                                ["row"] = hc?.Row?.Index ?? -1,
                                ["col"] = hc?.Column?.Index ?? -1
                            });
                        }
                    }
                    catch (FormulaException)
                    {
                        // 公式解析失败时忽略前置依赖
                    }
                }

                // 反向引用：哪些单元格引用了该单元格
                var dependents = new JArray();
                try
                {
                    var referrers = table.Project.FormulaManager
                        .GetCellsReferrer(table.Id, new[] { cell.Id });

                    foreach (var dep in referrers)
                    {
                        dependents.Add(new JObject
                        {
                            ["host_kind"] = dep.HostKind.ToString(),
                            ["host_table_id"] = dep.HostTable.Value.ToString(),
                            ["host_object_id"] = dep.HostObject.Value.ToString(),
                            ["referred_kind"] = dep.ReferredKind.ToString()
                        });
                    }
                }
                catch
                {
                    // 依赖图未加载时忽略
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["row"] = row,
                    ["col"] = col,
                    ["cell_id"] = cell.Id.Value.ToString(),
                    ["has_formula"] = cell.HasFormula,
                    ["formula"] = cell.Formula ?? "",
                    ["precedents"] = precedents,
                    ["precedents_count"] = precedents.Count,
                    ["dependents"] = dependents,
                    ["dependents_count"] = dependents.Count
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取公式依赖关系失败: " + ex.Message);
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

            var table = project.GetTableById(new Id64(tableNodeId));
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
        /// 为公式求值选择一个合适的行索引（优先选择普通数据行，否则用 0）
        /// </summary>
        private static int ResolveEvaluationRowIndex(Table table)
        {
            try
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (table.Rows[i]?.Role == RowRole.Normal)
                    {
                        return i;
                    }
                }
            }
            catch
            {
            }
            return 0;
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
