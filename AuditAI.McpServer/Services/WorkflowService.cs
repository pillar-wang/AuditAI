﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 审计工作流服务层
    /// 组合式服务，封装常见审计流程为单次调用，简化多步骤操作的编排。
    /// 每个方法组合调用多个已有 Service 方法，处理步骤间依赖与错误隔离：
    /// 某一步失败时记录错误但继续执行后续步骤，返回每步执行状态与耗时。
    /// </summary>
    public static class WorkflowService
    {
        // =============================================
        // 公开方法
        // =============================================

        /// <summary>
        /// 创建完整审计项目
        /// 一键完成：创建项目 → 打开项目 → 采集数据 → 导入账簿 → 生成底稿模板
        /// 每一步独立执行，某步失败不影响后续步骤，返回每步执行状态。
        /// </summary>
        /// <param name="companyName">被审计单位名称</param>
        /// <param name="year">审计年度</param>
        /// <param name="dbType">数据库类型（sqlserver/oracle/sqlite/access/paradox）</param>
        /// <param name="connectionString">源数据库连接字符串</param>
        /// <returns>工作流执行结果汇总 JSON</returns>
        public static string CreateAuditProject(string companyName, int year, string dbType, string connectionString)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var steps = new JArray();
                string projectPath = null;
                long projectId = 0;
                string collectionTaskId = null;

                // 步骤 a: 创建项目
                var stepA = ExecuteStep("create_project", () =>
                {
                    string projectName = (companyName ?? "未命名") + year + "年度审计";
                    return ProjectService.CreateProject(projectName, year);
                });
                steps.Add(stepA);
                if (IsStepSuccess(stepA))
                {
                    projectPath = ExtractField(stepA, "path");
                }

                // 步骤 b: 打开项目
                var stepB = ExecuteStep("open_project", () =>
                {
                    if (string.IsNullOrEmpty(projectPath))
                        throw new InvalidOperationException("项目路径为空（创建项目步骤未返回路径）");
                    return ProjectService.OpenProject(projectPath);
                });
                steps.Add(stepB);
                if (IsStepSuccess(stepB))
                {
                    projectId = ExtractFieldLong(stepB, "project_id");
                }

                // 步骤 c: 采集数据
                var stepC = ExecuteStep("collect_data", () =>
                {
                    if (projectId == 0)
                        throw new InvalidOperationException("项目ID为空（打开项目步骤未返回ID）");
                    return CollectionService.CollectData(dbType, connectionString, projectId);
                });
                steps.Add(stepC);
                if (IsStepSuccess(stepC))
                {
                    collectionTaskId = ExtractField(stepC, "task_id");
                }

                // 步骤 d: 导入账簿
                // 采集为异步任务，无法同步获取账簿文件路径，此步骤需采集完成后手动执行
                var stepD = ExecuteStep("import_ledger", () =>
                {
                    throw new InvalidOperationException(
                        "账簿文件路径未提供：采集任务为异步执行" +
                        (collectionTaskId != null ? $"（task_id={collectionTaskId}）" : "") +
                        "，请采集完成后调用 import_ledger 工具手动导入账簿文件");
                });
                steps.Add(stepD);

                // 步骤 e: 生成底稿模板
                var stepE = ExecuteStep("generate_templates", () => GenerateWorkpaperTemplates());
                steps.Add(stepE);

                sw.Stop();

                return BuildWorkflowResult("create_audit_project", steps, sw.ElapsedMilliseconds, new JObject
                {
                    ["company_name"] = companyName,
                    ["year"] = year,
                    ["project_id"] = projectId,
                    ["project_path"] = projectPath,
                    ["collection_task_id"] = collectionTaskId
                });
            }
            catch (Exception ex)
            {
                return ErrorJson("创建审计项目工作流失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 生成审计报告
        /// 一键完成：校验所有表格 → 计算试算平衡 → 填充报告模板 → 导出 Word
        /// 每一步独立执行，某步失败不影响后续步骤，返回每步执行状态。
        /// </summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="templateName">报告模板名称（用于查找文档节点，默认"审计报告"）</param>
        /// <param name="outputPath">Word 输出文件路径（.docx）</param>
        /// <returns>工作流执行结果汇总 JSON</returns>
        public static string GenerateAuditReport(long projectId, string templateName, string outputPath)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var steps = new JArray();

                string validationSummary = null;
                string trialBalanceSummary = null;
                long documentNodeId = 0;

                // 步骤 a: 校验所有表格
                var stepA = ExecuteStep("validate_all_tables", () => ValidationService.ValidateAllTables());
                steps.Add(stepA);
                if (IsStepSuccess(stepA))
                {
                    var raw = stepA["raw_result"] as JObject;
                    int passed = raw?["total_passed"]?.Value<int>() ?? 0;
                    int failed = raw?["total_failed"]?.Value<int>() ?? 0;
                    validationSummary = $"通过 {passed} 项，失败 {failed} 项";
                }

                // 步骤 b: 计算试算平衡（关键指标）
                // 尝试从项目信息获取年度，失败则使用当前年度
                int reportYear = DateTime.Now.Year;
                try
                {
                    var infoJson = JObject.Parse(ProjectService.GetProjectInfo());
                    if (infoJson["success"]?.Value<bool>() ?? false)
                    {
                        int.TryParse(infoJson["number"]?.ToString(), out reportYear);
                    }
                }
                catch
                {
                    // 忽略，使用默认年度
                }

                var stepB = ExecuteStep("get_trial_balance", () =>
                    LedgerService.GetTrialBalance(projectId, reportYear, 12));
                steps.Add(stepB);
                if (IsStepSuccess(stepB))
                {
                    var raw = stepB["raw_result"] as JObject;
                    var summary = raw?["summary"] as JObject;
                    if (summary != null)
                    {
                        trialBalanceSummary = string.Format(
                            "期初借方: {0}, 期初贷方: {1}, 本期借方: {2}, 本期贷方: {3}, 期末借方: {4}, 期末贷方: {5}",
                            summary["begin_debit"], summary["begin_credit"],
                            summary["debit_amount"], summary["credit_amount"],
                            summary["end_debit"], summary["end_credit"]);
                    }
                }

                // 步骤 c: 查找并填充报告模板
                var stepC = ExecuteStep("fill_report_template", () =>
                {
                    string docName = string.IsNullOrWhiteSpace(templateName) ? "审计报告" : templateName;
                    documentNodeId = FindDocumentNode(docName);
                    if (documentNodeId == 0)
                    {
                        throw new InvalidOperationException(
                            $"未找到名为 '{docName}' 的文档节点，请先创建底稿模板或指定正确的模板名称");
                    }

                    string content = BuildReportContent(reportYear, validationSummary, trialBalanceSummary);
                    return DocumentService.SetDocumentContent(documentNodeId, content);
                });
                steps.Add(stepC);

                // 步骤 d: 导出 Word
                var stepD = ExecuteStep("export_to_word", () =>
                {
                    if (documentNodeId == 0)
                        throw new InvalidOperationException("文档节点ID为空（填充报告模板步骤失败）");
                    if (string.IsNullOrWhiteSpace(outputPath))
                        throw new InvalidOperationException("输出路径不能为空");
                    return ExportService.ExportToWord(documentNodeId, outputPath);
                });
                steps.Add(stepD);

                sw.Stop();

                return BuildWorkflowResult("generate_audit_report", steps, sw.ElapsedMilliseconds, new JObject
                {
                    ["project_id"] = projectId,
                    ["template_name"] = templateName,
                    ["output_path"] = outputPath,
                    ["document_node_id"] = documentNodeId
                });
            }
            catch (Exception ex)
            {
                return ErrorJson("生成审计报告工作流失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 端到端审计流程
        /// 一键完成：创建项目 → 计算所有表格 → 校验所有表格 → 生成审计报告
        /// 每一步独立执行，某步失败不影响后续步骤，返回每步执行状态。
        /// </summary>
        /// <param name="companyName">被审计单位名称</param>
        /// <param name="year">审计年度</param>
        /// <param name="dbType">数据库类型（sqlserver/oracle/sqlite/access/paradox）</param>
        /// <param name="connectionString">源数据库连接字符串</param>
        /// <param name="reportOutputPath">审计报告输出路径（.docx）</param>
        /// <returns>工作流执行结果汇总 JSON</returns>
        public static string RunFullAudit(string companyName, int year, string dbType, string connectionString, string reportOutputPath)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var steps = new JArray();
                long projectId = 0;

                // 步骤 a: 创建审计项目
                var stepA = ExecuteStep("create_audit_project", () =>
                    CreateAuditProject(companyName, year, dbType, connectionString));
                steps.Add(stepA);
                if (IsStepSuccess(stepA))
                {
                    var raw = stepA["raw_result"] as JObject;
                    projectId = raw?["project_id"]?.Value<long>() ?? 0;
                }

                // 步骤 b: 计算所有表格
                var stepB = ExecuteStep("calculate_all_tables", () =>
                {
                    if (projectId == 0)
                        throw new InvalidOperationException("项目ID为空（创建项目步骤未返回ID）");
                    return FormulaService.CalculateAllTables();
                });
                steps.Add(stepB);

                // 步骤 c: 校验所有表格
                var stepC = ExecuteStep("validate_all_tables", () =>
                {
                    if (projectId == 0)
                        throw new InvalidOperationException("项目ID为空（创建项目步骤未返回ID）");
                    return ValidationService.ValidateAllTables();
                });
                steps.Add(stepC);

                // 步骤 d: 生成审计报告
                var stepD = ExecuteStep("generate_audit_report", () =>
                {
                    if (projectId == 0)
                        throw new InvalidOperationException("项目ID为空（创建项目步骤未返回ID）");
                    return GenerateAuditReport(projectId, "审计报告", reportOutputPath);
                });
                steps.Add(stepD);

                sw.Stop();

                return BuildWorkflowResult("run_full_audit", steps, sw.ElapsedMilliseconds, new JObject
                {
                    ["company_name"] = companyName,
                    ["year"] = year,
                    ["project_id"] = projectId,
                    ["report_output_path"] = reportOutputPath
                });
            }
            catch (Exception ex)
            {
                return ErrorJson("端到端审计工作流失败: " + ex.Message);
            }
        }

        // =============================================
        // 内部实现
        // =============================================

        /// <summary>
        /// 生成标准审计底稿模板
        /// 在当前项目的第一个树分组下创建文档和表格节点
        /// </summary>
        private static string GenerateWorkpaperTemplates()
        {
            // 获取项目树以找到 TreeGroup ID
            string treeJson = TreeService.GetProjectTree();
            var tree = JObject.Parse(treeJson);
            if (!(tree["success"]?.Value<bool>() ?? false))
            {
                throw new InvalidOperationException("获取项目树失败: " + (tree["error"]?.ToString() ?? "未知错误"));
            }

            var groups = tree["tree_groups"] as JArray;
            if (groups == null || groups.Count == 0)
            {
                throw new InvalidOperationException("项目中没有树分组，无法创建底稿模板");
            }

            // 使用第一个树分组作为父节点
            long groupId = ExtractIdLong(groups[0]["id"]);
            if (groupId == 0)
            {
                throw new InvalidOperationException("无法解析树分组ID");
            }

            var created = new JArray();
            int successCount = 0;
            int failCount = 0;

            // 创建文档节点：审计报告
            var doc1 = SafeCreateNode(() => TreeService.CreateDocumentNode(groupId, "审计报告"), "审计报告");
            created.Add(doc1);
            if (doc1["success"]?.Value<bool>() ?? false) successCount++; else failCount++;

            // 创建文档节点：管理建议书
            var doc2 = SafeCreateNode(() => TreeService.CreateDocumentNode(groupId, "管理建议书"), "管理建议书");
            created.Add(doc2);
            if (doc2["success"]?.Value<bool>() ?? false) successCount++; else failCount++;

            // 创建表格节点：资产负债表
            var table1 = SafeCreateTable(groupId, "资产负债表",
                new JArray { "项目", "期末余额", "期初余额" });
            created.Add(table1);
            if (table1["success"]?.Value<bool>() ?? false) successCount++; else failCount++;

            // 创建表格节点：利润表
            var table2 = SafeCreateTable(groupId, "利润表",
                new JArray { "项目", "本期金额", "上期金额" });
            created.Add(table2);
            if (table2["success"]?.Value<bool>() ?? false) successCount++; else failCount++;

            // 创建表格节点：现金流量表
            var table3 = SafeCreateTable(groupId, "现金流量表",
                new JArray { "项目", "本期金额", "上期金额" });
            created.Add(table3);
            if (table3["success"]?.Value<bool>() ?? false) successCount++; else failCount++;

            // 创建表格节点：试算平衡表
            var table4 = SafeCreateTable(groupId, "试算平衡表",
                new JArray { "科目编码", "科目名称", "期初借方", "期初贷方", "本期借方", "本期贷方", "期末借方", "期末贷方" });
            created.Add(table4);
            if (table4["success"]?.Value<bool>() ?? false) successCount++; else failCount++;

            var result = new JObject
            {
                ["success"] = failCount == 0,
                ["group_id"] = groupId,
                ["total"] = created.Count,
                ["success_count"] = successCount,
                ["failed_count"] = failCount,
                ["templates"] = created,
                ["message"] = $"底稿模板生成完成：{successCount}/{created.Count} 个模板创建成功"
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        /// <summary>
        /// 在项目树中查找指定名称的文档节点
        /// </summary>
        private static long FindDocumentNode(string name)
        {
            string treeJson = TreeService.GetProjectTree();
            var tree = JObject.Parse(treeJson);
            if (!(tree["success"]?.Value<bool>() ?? false)) return 0;

            var groups = tree["tree_groups"] as JArray;
            if (groups == null) return 0;

            foreach (var group in groups)
            {
                var children = group["children"] as JArray;
                if (children == null) continue;
                long found = FindDocumentNodeRecursive(children, name);
                if (found != 0) return found;
            }
            return 0;
        }

        /// <summary>
        /// 递归查找文档节点
        /// </summary>
        private static long FindDocumentNodeRecursive(JArray nodes, string name)
        {
            foreach (var node in nodes)
            {
                string nodeType = node["type"]?.ToString();

                if (nodeType == "document")
                {
                    string nodeName = node["name"]?.ToString();
                    if (string.IsNullOrEmpty(name) || nodeName == name)
                    {
                        return ExtractIdLong(node["id"]);
                    }
                }

                var children = node["children"] as JArray;
                if (children != null)
                {
                    long found = FindDocumentNodeRecursive(children, name);
                    if (found != 0) return found;
                }
            }
            return 0;
        }

        /// <summary>
        /// 构建审计报告文本内容
        /// </summary>
        private static string BuildReportContent(int year, string validationSummary, string trialBalanceSummary)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{year}年度审计报告");
            sb.AppendLine();
            sb.AppendLine("一、审计意见");
            sb.AppendLine($"我们审计了{year}年度的财务报表，认为财务报表在所有重大方面按照企业会计准则的规定编制，公允反映了财务状况和经营成果。");
            sb.AppendLine();
            sb.AppendLine("二、审计概况");
            sb.AppendLine($"审计年度：{year}年");
            sb.AppendLine($"数据校验：{validationSummary ?? "未获取校验结果"}");
            sb.AppendLine($"试算平衡：{trialBalanceSummary ?? "未获取试算平衡数据"}");
            sb.AppendLine();
            sb.AppendLine("三、审计结论");
            sb.AppendLine("基于已执行的审计程序，财务报表在所有重大方面公允反映了被审计单位的财务状况和经营成果。");
            sb.AppendLine();
            sb.AppendLine("（本报告由系统自动生成，请审计人员根据实际情况补充完善。）");
            return sb.ToString();
        }

        // =============================================
        // 步骤执行与结果构建辅助方法
        // =============================================

        /// <summary>
        /// 执行单个工作流步骤，捕获异常并记录耗时与结果
        /// </summary>
        private static JObject ExecuteStep(string stepName, Func<string> action)
        {
            var stepResult = new JObject { ["step"] = stepName };
            var sw = Stopwatch.StartNew();
            try
            {
                string resultJson = action();
                sw.Stop();
                stepResult["elapsed_ms"] = sw.ElapsedMilliseconds;

                // 尝试解析为 JSON
                JToken parsed;
                try
                {
                    parsed = JToken.Parse(resultJson);
                }
                catch
                {
                    parsed = new JValue(resultJson);
                }
                stepResult["raw_result"] = parsed;

                // 提取成功状态
                bool success = false;
                if (parsed is JObject obj)
                {
                    success = obj["success"]?.Value<bool>() ?? false;
                    if (!success)
                    {
                        stepResult["error"] = obj["error"]?.ToString() ?? "步骤执行失败";
                    }
                }
                stepResult["status"] = success ? "success" : "failed";
                return stepResult;
            }
            catch (Exception ex)
            {
                sw.Stop();
                stepResult["elapsed_ms"] = sw.ElapsedMilliseconds;
                stepResult["status"] = "error";
                stepResult["error"] = ex.Message;
                return stepResult;
            }
        }

        /// <summary>
        /// 构建工作流结果汇总 JSON
        /// </summary>
        private static string BuildWorkflowResult(string workflowName, JArray steps, long elapsedMs, JObject context)
        {
            int successCount = steps.Count(s => s["status"]?.ToString() == "success");
            int failCount = steps.Count - successCount;

            var result = new JObject
            {
                ["success"] = failCount == 0,
                ["workflow"] = workflowName,
                ["total_steps"] = steps.Count,
                ["success_count"] = successCount,
                ["failed_count"] = failCount,
                ["elapsed_ms"] = elapsedMs,
                ["steps"] = steps,
                ["message"] = $"工作流执行完成：{successCount}/{steps.Count} 步成功" +
                              (failCount > 0 ? $"，{failCount} 步失败（详见 steps）" : "")
            };

            // 合并上下文字段
            foreach (var prop in context.Properties())
            {
                result[prop.Name] = prop.Value;
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        /// <summary>
        /// 判断步骤是否成功
        /// </summary>
        private static bool IsStepSuccess(JObject step)
        {
            return step["status"]?.ToString() == "success";
        }

        /// <summary>
        /// 从步骤结果中提取字符串字段
        /// </summary>
        private static string ExtractField(JObject step, string fieldName)
        {
            var raw = step["raw_result"] as JObject;
            return raw?[fieldName]?.ToString();
        }

        /// <summary>
        /// 从步骤结果中提取长整型字段
        /// </summary>
        private static long ExtractFieldLong(JObject step, string fieldName)
        {
            var raw = step["raw_result"] as JObject;
            var token = raw?[fieldName];
            if (token == null) return 0;
            return ExtractIdLong(token);
        }

        /// <summary>
        /// 从 JToken 中提取长整型 ID（兼容字符串和数值类型）
        /// </summary>
        private static long ExtractIdLong(JToken token)
        {
            if (token == null) return 0;
            if (token.Type == JTokenType.String)
            {
                long val;
                long.TryParse(token.ToString(), out val);
                return val;
            }
            try
            {
                return token.Value<long>();
            }
            catch
            {
                long val;
                long.TryParse(token.ToString(), out val);
                return val;
            }
        }

        /// <summary>
        /// 安全创建节点（捕获异常，返回包含结果的 JObject）
        /// </summary>
        private static JObject SafeCreateNode(Func<string> action, string templateName)
        {
            try
            {
                string json = action();
                var parsed = JObject.Parse(json);
                parsed["template_name"] = templateName;
                return parsed;
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["template_name"] = templateName,
                    ["error"] = ex.Message
                };
            }
        }

        /// <summary>
        /// 安全创建表格节点（捕获异常，返回包含结果的 JObject）
        /// </summary>
        private static JObject SafeCreateTable(long parentId, string name, JArray columns)
        {
            return SafeCreateNode(() => TreeService.CreateTableNode(parentId, name, columns), name);
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
