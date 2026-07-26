using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using ValidationFormula = Auditai.Model.ValidationFormula;
using ValidationResult = Auditai.Model.ValidationResult;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 验证点（ValidationFormula）管理服务
    /// 封装验证公式的增删改查，对应 AppCommands.AddValidationPoint / RemoveValidationPoint / DocValidationMgmt
    /// </summary>
    public static class ValidationPointService
    {
        // =============================================
        // 列举验证点
        // =============================================

        public static string ListValidationPoints(long? tableNodeId = null)
        {
            try
            {
                var project = EnsureProject();
                var formulas = project.ValidationManager.Formulas.AsEnumerable();
                if (tableNodeId.HasValue)
                {
                    var tid = new Id64(tableNodeId.Value);
                    formulas = formulas.Where(f => f.TableId == tid);
                }

                var arr = new JArray(formulas.Select(f => new JObject
                {
                    ["id"] = f.Id.Value.ToString(),
                    ["table_id"] = f.TableId.Value.ToString(),
                    ["left_expr"] = f.LeftExpr ?? "",
                    ["operator"] = f.Operator?.Display ?? "",
                    ["operator_code"] = f.Operator?.Code ?? 0,
                    ["right_expr"] = f.RightExpr ?? "",
                    ["note"] = f.Note ?? "",
                    ["is_dirty"] = f.IsDirty
                }));

                var result = new JObject
                {
                    ["success"] = true,
                    ["total"] = formulas.Count(),
                    ["filter_table_id"] = tableNodeId?.ToString() ?? "all",
                    ["validation_points"] = arr
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("列举验证点失败: " + ex.Message); }
        }

        // =============================================
        // 添加验证点
        // =============================================

        public static string AddValidationPoint(long tableNodeId, string leftExpr, string op, string rightExpr, string note)
        {
            try
            {
                var project = EnsureProject();
                var tid = new Id64(tableNodeId);
                var table = project.GetTableById(tid);
                if (table == null)
                    return ErrorJson($"未找到表格节点: {tableNodeId}");

                table.LoadAndReturn(true);

                var vop = ParseOperator(op);
                if (vop == null)
                    return ErrorJson($"不支持的操作符: {op}（可用: Equal/NotEqual/GreaterThan/GreaterThanOrEqual/LessThan/LessThanOrEqual 或 ==/!=/>/>=/</<=）");

                var vf = new ValidationFormula
                {
                    Id = project.GetNextId(),
                    TableId = tid,
                    LeftExpr = leftExpr ?? "",
                    Operator = vop,
                    RightExpr = rightExpr ?? "",
                    Note = note ?? ""
                };
                project.ValidationManager.Formulas.Add(vf);
                project.Save();

                var result = new JObject
                {
                    ["success"] = true,
                    ["validation_id"] = vf.Id.Value.ToString(),
                    ["table_id"] = tableNodeId.ToString(),
                    ["message"] = "验证点已添加"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("添加验证点失败: " + ex.Message); }
        }

        // =============================================
        // 更新验证点
        // =============================================

        public static string UpdateValidationPoint(long validationId, string leftExpr, string op, string rightExpr, string note)
        {
            try
            {
                var project = EnsureProject();
                var vf = project.ValidationManager.Formulas.FirstOrDefault(f => f.Id.Value == validationId);
                if (vf == null)
                    return ErrorJson($"未找到验证点: {validationId}");

                if (leftExpr != null) vf.LeftExpr = leftExpr;
                if (rightExpr != null) vf.RightExpr = rightExpr;
                if (note != null) vf.Note = note;
                if (!string.IsNullOrEmpty(op))
                {
                    var vop = ParseOperator(op);
                    if (vop != null) vf.Operator = vop;
                }
                vf.IsDirty = true;
                project.Save();

                return Ok($"验证点 {validationId} 已更新", new JObject { ["validation_id"] = validationId.ToString() });
            }
            catch (Exception ex) { return ErrorJson("更新验证点失败: " + ex.Message); }
        }

        // =============================================
        // 删除验证点
        // =============================================

        public static string RemoveValidationPoint(long validationId)
        {
            try
            {
                var project = EnsureProject();
                var vf = project.ValidationManager.Formulas.FirstOrDefault(f => f.Id.Value == validationId);
                if (vf == null)
                    return ErrorJson($"未找到验证点: {validationId}");

                project.ValidationManager.RemoveOne(vf);
                project.Save();

                return Ok($"验证点 {validationId} 已删除", new JObject { ["validation_id"] = validationId.ToString() });
            }
            catch (Exception ex) { return ErrorJson("删除验证点失败: " + ex.Message); }
        }

        // =============================================
        // 执行验证
        // =============================================

        public static string RunValidation(long validationId)
        {
            try
            {
                var project = EnsureProject();
                var vf = project.ValidationManager.Formulas.FirstOrDefault(f => f.Id.Value == validationId);
                if (vf == null)
                    return ErrorJson($"未找到验证点: {validationId}");

                var results = project.ValidationManager.Validate(vf, false);
                var arr = new JArray(results.Select(r => new JObject
                {
                    ["passed"] = r.Passed,
                    ["left_value"] = r.LeftValue == null ? null : ValidationResult.ValueToString(r.LeftValue),
                    ["right_value"] = r.RightValue == null ? null : ValidationResult.ValueToString(r.RightValue),
                    ["row_index"] = r.RowIndex,
                    ["has_wildcard"] = r.HasWildcard,
                    ["is_valid"] = r.IsValid
                }));

                return JsonConvert.SerializeObject(new JObject
                {
                    ["success"] = true,
                    ["validation_id"] = validationId.ToString(),
                    ["total"] = results.Count,
                    ["passed"] = results.Count(r => r.Passed),
                    ["failed"] = results.Count(r => !r.Passed),
                    ["results"] = arr
                }, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("执行验证失败: " + ex.Message); }
        }

        // =============================================
        // 获取验证错误列表（覆盖 PreviousError/NextError/DocPreviousError/DocNextError）
        // =============================================

        public static string GetValidationErrors(long? tableNodeId = null)
        {
            try
            {
                var project = EnsureProject();
                var formulas = project.ValidationManager.Formulas.AsEnumerable();
                if (tableNodeId.HasValue)
                    formulas = formulas.Where(f => f.TableId.Value == tableNodeId.Value);

                var errorList = new JArray();
                int total = 0;
                int passed = 0;
                int failed = 0;

                foreach (var vf in formulas)
                {
                    try
                    {
                        var results = project.ValidationManager.Validate(vf, false);
                        foreach (var r in results)
                        {
                            if (r.Passed) { passed++; continue; }
                            failed++;
                            errorList.Add(new JObject
                            {
                                ["error_index"] = total,
                                ["validation_id"] = vf.Id.Value.ToString(),
                                ["table_id"] = vf.TableId.Value.ToString(),
                                ["note"] = vf.Note ?? "",
                                ["left_expr"] = vf.LeftExpr ?? "",
                                ["operator"] = vf.Operator?.Code.ToString() ?? "0",
                                ["right_expr"] = vf.RightExpr ?? "",
                                ["row_index"] = r.RowIndex,
                                ["left_value"] = r.LeftValue == null ? null : ValidationResult.ValueToString(r.LeftValue),
                                ["right_value"] = r.RightValue == null ? null : ValidationResult.ValueToString(r.RightValue)
                            });
                            total++;
                        }
                    }
                    catch { /* 跳过单个验证点失败 */ }
                }

                return JsonConvert.SerializeObject(new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId?.ToString() ?? "",
                    ["total_errors"] = total,
                    ["total_passed"] = passed,
                    ["total_failed"] = failed,
                    ["errors"] = errorList
                }, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取验证错误列表失败: " + ex.Message); }
        }

        // =============================================
        // 获取下一个验证错误（覆盖 NextError / DocNextError）
        // =============================================

        public static string GetNextValidationError(long? tableNodeId, int currentIndex)
        {
            try
            {
                var project = EnsureProject();
                var formulas = project.ValidationManager.Formulas.AsEnumerable();
                if (tableNodeId.HasValue)
                    formulas = formulas.Where(f => f.TableId.Value == tableNodeId.Value);

                int idx = -1;
                foreach (var vf in formulas)
                {
                    try
                    {
                        var results = project.ValidationManager.Validate(vf, false);
                        foreach (var r in results)
                        {
                            if (r.Passed) continue;
                            idx++;
                            if (idx > currentIndex)
                            {
                                return JsonConvert.SerializeObject(new JObject
                                {
                                    ["success"] = true,
                                    ["found"] = true,
                                    ["error_index"] = idx,
                                    ["validation_id"] = vf.Id.Value.ToString(),
                                    ["table_id"] = vf.TableId.Value.ToString(),
                                    ["note"] = vf.Note ?? "",
                                    ["row_index"] = r.RowIndex,
                                    ["left_value"] = r.LeftValue == null ? null : ValidationResult.ValueToString(r.LeftValue),
                                    ["right_value"] = r.RightValue == null ? null : ValidationResult.ValueToString(r.RightValue)
                                }, Formatting.Indented);
                            }
                        }
                    }
                    catch { /* 跳过单个验证点失败 */ }
                }

                return JsonConvert.SerializeObject(new JObject
                {
                    ["success"] = true,
                    ["found"] = false,
                    ["message"] = "已到达最后一个错误"
                }, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取下一个验证错误失败: " + ex.Message); }
        }

        // =============================================
        // 获取上一个验证错误（覆盖 PreviousError / DocPreviousError）
        // =============================================

        public static string GetPreviousValidationError(long? tableNodeId, int currentIndex)
        {
            try
            {
                var project = EnsureProject();
                var formulas = project.ValidationManager.Formulas.AsEnumerable();
                if (tableNodeId.HasValue)
                    formulas = formulas.Where(f => f.TableId.Value == tableNodeId.Value);

                // 收集所有错误（需要倒序查找）
                var errors = new System.Collections.Generic.List<(int idx, ValidationFormula vf, ValidationResult r)>();
                int idx = -1;
                foreach (var vf in formulas)
                {
                    try
                    {
                        var results = project.ValidationManager.Validate(vf, false);
                        foreach (var r in results)
                        {
                            if (r.Passed) continue;
                            idx++;
                            errors.Add((idx, vf, r));
                        }
                    }
                    catch { /* 跳过单个验证点失败 */ }
                }

                // 查找索引小于 currentIndex 的最后一个错误
                for (int i = errors.Count - 1; i >= 0; i--)
                {
                    if (errors[i].idx < currentIndex)
                    {
                        var e = errors[i];
                        return JsonConvert.SerializeObject(new JObject
                        {
                            ["success"] = true,
                            ["found"] = true,
                            ["error_index"] = e.idx,
                            ["validation_id"] = e.vf.Id.Value.ToString(),
                            ["table_id"] = e.vf.TableId.Value.ToString(),
                            ["note"] = e.vf.Note ?? "",
                            ["row_index"] = e.r.RowIndex,
                            ["left_value"] = e.r.LeftValue == null ? null : ValidationResult.ValueToString(e.r.LeftValue),
                            ["right_value"] = e.r.RightValue == null ? null : ValidationResult.ValueToString(e.r.RightValue)
                        }, Formatting.Indented);
                    }
                }

                return JsonConvert.SerializeObject(new JObject
                {
                    ["success"] = true,
                    ["found"] = false,
                    ["message"] = "已到达第一个错误"
                }, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取上一个验证错误失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static ValidationOperator ParseOperator(string op)
        {
            if (string.IsNullOrEmpty(op)) return null;
            string trimmed = op.Trim();

            // 支持符号化操作符
            switch (trimmed)
            {
                case "==": case "=": return ValidationOperator.FromCode(0);
                case "!=": case "<>": return ValidationOperator.FromCode(5);
                case ">": return ValidationOperator.FromCode(1);
                case ">=": return ValidationOperator.FromCode(2);
                case "<": return ValidationOperator.FromCode(3);
                case "<=": return ValidationOperator.FromCode(4);
            }

            // 支持名称（不区分大小写）
            switch (trimmed.ToLowerInvariant())
            {
                case "equal": case "equals": return ValidationOperator.FromCode(0);
                case "notequal": case "not_equals": return ValidationOperator.FromCode(5);
                case "greaterthan": return ValidationOperator.FromCode(1);
                case "greaterthanorequal": return ValidationOperator.FromCode(2);
                case "lessthan": return ValidationOperator.FromCode(3);
                case "lessthanorequal": return ValidationOperator.FromCode(4);
            }

            return null;
        }

        private static Project EnsureProject()
        {
            var project = SessionState.Current.CurrentProject;
            if (project == null)
                throw new InvalidOperationException("未打开项目，请先调用 open_project 工具");
            Project.Current = project;
            return project;
        }

        private static string Ok(string msg, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["message"] = msg };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
