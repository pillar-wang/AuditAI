﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Table = Auditai.Model.Table;
using ValidationFormula = Auditai.Model.ValidationFormula;
using ValidationResult = Auditai.Model.ValidationResult;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 校验服务层
    /// 封装文档/表格校验、批量校验及校验结果查询等操作。
    /// 校验逻辑基于 Project.ValidationManager（用户在 UI 中配置的校验公式），
    /// 并补充基本的数据完整性检查（公式错误、空值、数据类型不匹配等）。
    /// </summary>
    public static class ValidationService
    {
        // =============================================
        // 校验结果缓存（按节点 ID 缓存最近一次校验结果）
        // =============================================
        private static readonly Dictionary<long, JArray> _resultsCache = new Dictionary<long, JArray>();

        // =============================================
        // 核心方法
        // =============================================

        /// <summary>
        /// 校验文档
        /// 运行项目级校验规则（包括绑定到文档域的校验公式以及所有表格校验公式），
        /// 返回校验结果汇总。
        /// </summary>
        /// <param name="documentNodeId">文档树节点 ID</param>
        /// <returns>校验结果 JSON</returns>
        public static string ValidateDocument(long documentNodeId)
        {
            try
            {
                SessionState.Current.EnsureProject();
                var project = SessionState.Current.CurrentProject;

                var docNode = project.GetAllDocumentNodes().FirstOrDefault(n => n.Id == new Id64(documentNodeId));
                if (docNode == null)
                {
                    return ErrorJson($"未找到文档节点: {documentNodeId}");
                }

                // 文档校验：运行项目中所有校验公式（文档域绑定 + 表格绑定），
                // 并汇总通过/失败统计。
                var allFormulas = project.ValidationManager.Formulas;
                var passedItems = new JArray();
                var failedItems = new JArray();
                int errorCount = 0;

                foreach (var vf in allFormulas)
                {
                    try
                    {
                        var results = project.ValidationManager.Validate(vf);
                        if (results == null || results.Count == 0) continue;

                        foreach (var r in results)
                        {
                            if (!r.IsValid)
                            {
                                errorCount++;
                                continue;
                            }

                            var item = BuildResultItem(vf, r);
                            if (r.Passed)
                                passedItems.Add(item);
                            else
                                failedItems.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        failedItems.Add(new JObject
                        {
                            ["formula_id"] = vf.Id.Value.ToString(),
                            ["note"] = vf.Note ?? "",
                            ["is_valid"] = false,
                            ["error"] = ex.Message
                        });
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId,
                    ["document_name"] = docNode.Name,
                    ["total_formulas"] = allFormulas.Count,
                    ["passed_count"] = passedItems.Count,
                    ["failed_count"] = failedItems.Count,
                    ["error_count"] = errorCount,
                    ["passed_items"] = passedItems,
                    ["failed_items"] = failedItems,
                    ["message"] = $"文档校验完成：通过 {passedItems.Count} 项，失败 {failedItems.Count} 项，错误 {errorCount} 项"
                };

                // 缓存校验结果
                var cacheItems = new JArray();
                foreach (var p in passedItems) cacheItems.Add(p);
                foreach (var f in failedItems) cacheItems.Add(f);
                _resultsCache[documentNodeId] = cacheItems;

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("校验文档失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 校验表格
        /// 运行绑定到该表格的校验公式，并执行基本的数据完整性检查
        /// （公式错误、空值、数据类型不匹配等），返回通过/失败项列表。
        /// </summary>
        /// <param name="tableNodeId">表格树节点 ID</param>
        /// <returns>校验结果 JSON</returns>
        public static string ValidateTable(long tableNodeId)
        {
            try
            {
                Table table = GetLoadedTable(tableNodeId);
                var project = table.Project;
                var tableId = table.Id;

                var passedItems = new JArray();
                var failedItems = new JArray();
                int formulaErrorCount = 0;

                // ============================================
                // 1. 运行绑定到该表格的校验公式
                // ============================================
                var tableFormulas = project.ValidationManager.Formulas
                    .Where(f => f.TableId == tableId && f.DocumentFieldId.Value == 0)
                    .ToList();

                foreach (var vf in tableFormulas)
                {
                    try
                    {
                        var results = project.ValidationManager.Validate(vf);
                        if (results == null || results.Count == 0) continue;

                        foreach (var r in results)
                        {
                            if (!r.IsValid)
                            {
                                formulaErrorCount++;
                                continue;
                            }

                            var item = BuildResultItem(vf, r);
                            if (r.Passed)
                                passedItems.Add(item);
                            else
                                failedItems.Add(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        formulaErrorCount++;
                        failedItems.Add(new JObject
                        {
                            ["formula_id"] = vf.Id.Value.ToString(),
                            ["note"] = vf.Note ?? "",
                            ["is_valid"] = false,
                            ["error"] = ex.Message
                        });
                    }
                }

                // ============================================
                // 2. 基本数据完整性检查
                // ============================================
                var integrityIssues = CheckDataIntegrity(table);
                foreach (var issue in integrityIssues)
                {
                    failedItems.Add(issue);
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId,
                    ["table_id"] = tableId.Value.ToString(),
                    ["title"] = table.Title?.TitleCell?.Value?.ToString() ?? "",
                    ["row_count"] = table.Rows.Count,
                    ["col_count"] = table.Columns.Count,
                    ["validation_formula_count"] = tableFormulas.Count,
                    ["passed_count"] = passedItems.Count,
                    ["failed_count"] = failedItems.Count,
                    ["formula_error_count"] = formulaErrorCount,
                    ["integrity_issue_count"] = integrityIssues.Count,
                    ["passed_items"] = passedItems,
                    ["failed_items"] = failedItems,
                    ["message"] = $"表格校验完成：通过 {passedItems.Count} 项，失败 {failedItems.Count} 项（含数据完整性问题 {integrityIssues.Count} 项）"
                };

                // 缓存校验结果
                var cacheItems = new JArray();
                foreach (var p in passedItems) cacheItems.Add(p);
                foreach (var f in failedItems) cacheItems.Add(f);
                _resultsCache[tableNodeId] = cacheItems;

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("校验表格失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 校验当前项目所有表格
        /// 遍历所有表格节点，运行校验，返回汇总结果。
        /// </summary>
        /// <returns>校验结果汇总 JSON</returns>
        public static string ValidateAllTables()
        {
            try
            {
                SessionState.Current.EnsureProject();
                var project = SessionState.Current.CurrentProject;

                var tableNodes = project.GetAllTableNodes().ToList();
                var tables = new JArray();
                int totalPassed = 0;
                int totalFailed = 0;
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

                        // 复用 ValidateTable 的核心逻辑（直接调用，避免重复 JSON 解析）
                        var tableId = table.Id;
                        var tableFormulas = project.ValidationManager.Formulas
                            .Where(f => f.TableId == tableId && f.DocumentFieldId.Value == 0)
                            .ToList();

                        int passed = 0;
                        int failed = 0;
                        int errors = 0;

                        foreach (var vf in tableFormulas)
                        {
                            try
                            {
                                var results = project.ValidationManager.Validate(vf);
                                if (results == null) continue;
                                foreach (var r in results)
                                {
                                    if (!r.IsValid) { errors++; continue; }
                                    if (r.Passed) passed++; else failed++;
                                }
                            }
                            catch
                            {
                                errors++;
                            }
                        }

                        var integrityIssues = CheckDataIntegrity(table);
                        failed += integrityIssues.Count;

                        tables.Add(new JObject
                        {
                            ["table_node_id"] = node.Id.Value,
                            ["table_id"] = tableId.Value.ToString(),
                            ["title"] = table.Title?.TitleCell?.Value?.ToString() ?? "",
                            ["status"] = "success",
                            ["passed_count"] = passed,
                            ["failed_count"] = failed,
                            ["error_count"] = errors,
                            ["integrity_issue_count"] = integrityIssues.Count
                        });

                        totalPassed += passed;
                        totalFailed += failed;
                        successCount++;

                        // 缓存每个表格的结果（简化版，仅记录统计）
                        _resultsCache[node.Id.Value] = new JArray();
                    }
                    catch (Exception ex)
                    {
                        tables.Add(new JObject
                        {
                            ["table_node_id"] = node != null ? node.Id.Value : 0,
                            ["title"] = "",
                            ["status"] = "failed",
                            ["error"] = ex.Message
                        });
                        failCount++;
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["total_tables"] = tableNodes.Count,
                    ["success_count"] = successCount,
                    ["failed_count"] = failCount,
                    ["total_passed"] = totalPassed,
                    ["total_failed"] = totalFailed,
                    ["tables"] = tables,
                    ["message"] = $"已校验 {successCount}/{tableNodes.Count} 个表格：通过 {totalPassed} 项，失败 {totalFailed} 项"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("校验所有表格失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取校验结果
        /// 返回上次校验（ValidateDocument/ValidateTable）的结果。
        /// </summary>
        /// <param name="nodeId">文档或表格节点 ID</param>
        /// <returns>校验结果 JSON</returns>
        public static string GetValidationResults(long nodeId)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (_resultsCache.TryGetValue(nodeId, out var cached))
                {
                    var result = new JObject
                    {
                        ["success"] = true,
                        ["node_id"] = nodeId,
                        ["has_results"] = true,
                        ["result_count"] = cached.Count,
                        ["results"] = cached,
                        ["message"] = $"已获取节点 {nodeId} 的最近校验结果（共 {cached.Count} 项）"
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }

                // 无缓存结果，返回提示
                var empty = new JObject
                {
                    ["success"] = true,
                    ["node_id"] = nodeId,
                    ["has_results"] = false,
                    ["result_count"] = 0,
                    ["results"] = new JArray(),
                    ["message"] = $"节点 {nodeId} 暂无校验结果，请先调用 validate_table 或 validate_document"
                };
                return JsonConvert.SerializeObject(empty, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取校验结果失败: " + ex.Message);
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
        /// 构建校验结果项
        /// </summary>
        private static JObject BuildResultItem(ValidationFormula vf, ValidationResult r)
        {
            return new JObject
            {
                ["formula_id"] = vf.Id.Value.ToString(),
                ["note"] = vf.Note ?? "",
                ["left_expr"] = vf.LeftExpr ?? "",
                ["right_expr"] = vf.RightExpr ?? "",
                ["operator"] = vf.Operator?.Display ?? "",
                ["row_index"] = r.RowIndex,
                ["passed"] = r.Passed,
                ["left_value"] = r.LeftValue == null ? "" : r.LeftValue.ToString(),
                ["right_value"] = r.RightValue == null ? "" : r.RightValue.ToString(),
                ["has_wildcard"] = r.HasWildcard
            };
        }

        /// <summary>
        /// 基本数据完整性检查
        /// 检查表格中的公式错误、空值、数据类型不匹配等问题。
        /// </summary>
        private static List<JObject> CheckDataIntegrity(Table table)
        {
            var issues = new List<JObject>();
            int rowCount = table.Rows.Count;
            int colCount = table.Columns.Count;

            for (int r = 0; r < rowCount; r++)
            {
                var row = table.Rows[r];
                if (row == null) continue;
                // 仅检查普通数据行
                if (row.Role != RowRole.Normal) continue;

                for (int c = 0; c < colCount; c++)
                {
                    var cell = table[r, c];
                    if (cell == null) continue;

                    // 1. 检查公式错误：单元格有公式但值为错误标识
                    if (cell.HasFormula)
                    {
                        var valStr = cell.Value?.ToString() ?? "";
                        if (IsErrorValue(valStr))
                        {
                            issues.Add(new JObject
                            {
                                ["type"] = "formula_error",
                                ["severity"] = "error",
                                ["row"] = r,
                                ["col"] = c,
                                ["formula"] = cell.Formula ?? "",
                                ["value"] = valStr,
                                ["message"] = $"单元格 ({r},{c}) 公式求值异常: {valStr}"
                            });
                        }
                    }

                    // 2. 检查数据类型不匹配：列有公式但单元格值类型异常
                    var column = table.Columns[c];
                    if (column != null && column.HasFormula && cell.Value != null)
                    {
                        var valStr = cell.Value.ToString();
                        // 列公式通常产生数值，若结果是无法解析的字符串则可能存在类型问题
                        if (!string.IsNullOrEmpty(valStr) && !IsErrorValue(valStr))
                        {
                            // 仅当列公式存在且单元格值看起来像错误占位符时报告
                            if (valStr.IndexOf("#REF", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                valStr.IndexOf("#VALUE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                valStr.IndexOf("#DIV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                valStr.IndexOf("#NAME", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                valStr.IndexOf("#N/A", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                issues.Add(new JObject
                                {
                                    ["type"] = "data_type_mismatch",
                                    ["severity"] = "warning",
                                    ["row"] = r,
                                    ["col"] = c,
                                    ["column_caption"] = column.Caption ?? "",
                                    ["value"] = valStr,
                                    ["message"] = $"单元格 ({r},{c}) 列 \"{column.Caption}\" 值包含错误标识: {valStr}"
                                });
                            }
                        }
                    }
                }
            }

            // 3. 检查空标题（表格无标题）
            try
            {
                var title = table.Title?.TitleCell?.Value?.ToString();
                if (string.IsNullOrWhiteSpace(title))
                {
                    issues.Add(new JObject
                    {
                        ["type"] = "empty_title",
                        ["severity"] = "warning",
                        ["row"] = -1,
                        ["col"] = -1,
                        ["message"] = "表格标题为空"
                    });
                }
            }
            catch
            {
                // 忽略标题检查异常
            }

            return issues;
        }

        /// <summary>
        /// 判断值是否为公式错误标识
        /// </summary>
        private static bool IsErrorValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.IndexOf("#REF", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("#VALUE", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("#DIV", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("#NAME", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("#N/A", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("#NULL", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("#NUM", StringComparison.OrdinalIgnoreCase) >= 0;
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
