﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 验收报告工具集（Task 4）
    /// 汇总三类验证结果（API 工作流 + 客户端功能 + 工作流断点），
    /// 输出 Go/No-Go 验收报告。
    /// </summary>
    public static class AcceptanceReportTools
    {
        private static readonly string ReportDir = @"e:\lq\.trae\specs\cloud-workflow-acceptance-test\reports";

        /// <summary>
        /// 注册验收报告工具（1 个）。
        /// </summary>
        public static void Register()
        {
            RegisterGenerateAcceptanceReport();
        }

        // =====================================================================
        // generate_acceptance_report
        // 汇总三类验证结果，生成 Go/No-Go 验收报告。
        // 可选传入三个场景的 JSON 结果字符串；未传入则从 SessionState.AssertionResults 汇总。
        // =====================================================================
        private static void RegisterGenerateAcceptanceReport()
        {
            ToolRegistry.Register("generate_acceptance_report",
                "生成云端审计工作流验收报告（Go/No-Go）。汇总三类验证结果：" +
                "1) run_full_audit_workflow_flow（端到端用户工作流，14 步）" +
                "2) verify_client_only_features（客户端功能静态验证，4 类）" +
                "3) detect_workflow_gaps（工作流断点检测，6 个集成点）。" +
                "可选传入各场景的 JSON 结果（workflowResult/clientFeaturesResult/workflowGapsResult）；" +
                "未传入则从当前会话 SessionState.AssertionResults 汇总。报告保存到 reports 目录。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["workflowResult"] = new JObject { ["type"] = "string", ["description"] = "run_full_audit_workflow_flow 场景的 JSON 结果（可选）" },
                        ["clientFeaturesResult"] = new JObject { ["type"] = "string", ["description"] = "verify_client_only_features 场景的 JSON 结果（可选）" },
                        ["workflowGapsResult"] = new JObject { ["type"] = "string", ["description"] = "detect_workflow_gaps 场景的 JSON 结果（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => GenerateAcceptanceReportImpl(args));
        }

        private static string GenerateAcceptanceReportImpl(JObject args)
        {
            try
            {
                // 解析三个场景结果（可选）
                ScenarioSummary workflow = ParseScenarioResult(args != null ? args["workflowResult"] : null, "run_full_audit_workflow_flow");
                ScenarioSummary clientFeatures = ParseScenarioResult(args != null ? args["clientFeaturesResult"] : null, "verify_client_only_features");
                ScenarioSummary workflowGaps = ParseScenarioResult(args != null ? args["workflowGapsResult"] : null, "detect_workflow_gaps");

                // 如果未传入场景结果，从 SessionState.AssertionResults 汇总
                if (workflow == null && clientFeatures == null && workflowGaps == null)
                {
                    var summary = SummarizeFromAssertionResults();
                    if (workflow == null) workflow = summary.Workflow;
                    if (clientFeatures == null) clientFeatures = summary.Client;
                    if (workflowGaps == null) workflowGaps = summary.Gaps;
                }

                // 计算总体结论
                bool allPassed = true;
                if (workflow != null && !workflow.Passed) allPassed = false;
                if (clientFeatures != null && !clientFeatures.Passed) allPassed = false;
                if (workflowGaps != null && !workflowGaps.Passed) allPassed = false;

                // 收集阻塞问题
                var blockers = new List<string>();
                CollectBlockers(workflow, "API 工作流", blockers);
                CollectBlockers(clientFeatures, "客户端功能", blockers);
                CollectBlockers(workflowGaps, "工作流断点", blockers);

                // 生成报告
                string report = BuildReport(workflow, clientFeatures, workflowGaps, allPassed, blockers);

                // 保存报告
                string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string reportPath = Path.Combine(ReportDir, "acceptance-report-" + timestamp + ".md");
                try
                {
                    Directory.CreateDirectory(ReportDir);
                    File.WriteAllText(reportPath, report, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[AcceptanceReport] 保存报告失败: " + ex.Message);
                }

                return JsonConvert.SerializeObject(new
                {
                    conclusion = allPassed ? "GO" : "NO-GO",
                    allPassed = allPassed,
                    blockerCount = blockers.Count,
                    reportPath = reportPath,
                    workflow = workflow,
                    clientFeatures = clientFeatures,
                    workflowGaps = workflowGaps,
                    blockers = blockers
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    conclusion = "ERROR",
                    allPassed = false,
                    error = ex.Message,
                    reportPath = (string)null
                }, Formatting.Indented);
            }
        }

        // =====================================================================
        // 辅助方法
        // =====================================================================

        /// <summary>
        /// 解析场景 JSON 结果为摘要。token 为 null 时返回 null。
        /// </summary>
        private static ScenarioSummary ParseScenarioResult(JToken token, string defaultName)
        {
            if (token == null) return null;
            try
            {
                string json = token.ToString();
                if (string.IsNullOrWhiteSpace(json)) return null;
                var obj = JObject.Parse(json);
                var summary = new ScenarioSummary
                {
                    ScenarioName = obj["scenarioName"] != null ? obj["scenarioName"].ToString() : defaultName,
                    Passed = obj["passed"] != null && obj["passed"].Value<bool>(),
                    DurationMs = obj["durationMs"] != null ? obj["durationMs"].Value<long>() : 0,
                    Summary = obj["summary"] != null ? obj["summary"].ToString() : "",
                    FailedSteps = new List<string>()
                };
                if (obj["steps"] != null)
                {
                    summary.TotalSteps = obj["steps"].Count();
                    foreach (var step in obj["steps"])
                    {
                        bool passed = step["passed"] != null && step["passed"].Value<bool>();
                        if (!passed)
                        {
                            string name = step["name"] != null ? step["name"].ToString() : "(unnamed)";
                            string error = step["error"] != null ? step["error"].ToString() : "";
                            summary.FailedSteps.Add(name + (string.IsNullOrEmpty(error) ? "" : " | " + error));
                        }
                    }
                    summary.PassedSteps = summary.TotalSteps - summary.FailedSteps.Count;
                }
                return summary;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从 SessionState.AssertionResults 汇总三类场景结果。
        /// 通过 AssertionType/Message 中的关键字分类。
        /// </summary>
        private static AssertionSummaryTriple SummarizeFromAssertionResults()
        {
            var results = SessionState.AssertionResults;
            if (results == null || results.Count == 0)
                return new AssertionSummaryTriple { Workflow = null, Client = null, Gaps = null };

            // 关键字分类
            var workflowKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "login", "create_team", "invite_user", "change_password", "update_user_info", "create_template", "create_project_from_template", "push_table", "pull_table", "push_table_again", "cloud_login", "cloud_create_team", "cloud_invite_user", "cloud_update_user_info", "cloud_create_project", "cloud_push_table", "cloud_pull_table", "reset_password", "verify_undo_redo", "verify_report_gen", "verify_report_refresh", "verify_import_export", "cleanup" };
            var clientKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "undo_redo", "report_gen", "report_refresh", "import_export", "撤销", "报告生成", "报告刷新", "导入导出" };
            var gapKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "template_to_project", "table_to_sync", "sync_to_conflict", "offline_to_replay", "local_mode_guard", "project_to_table", "模板", "同步", "冲突", "离线", "守卫" };

            var wSteps = new List<AssertionResult>();
            var cSteps = new List<AssertionResult>();
            var gSteps = new List<AssertionResult>();

            foreach (var ar in results)
            {
                string key = (ar.Message ?? "") + " " + (ar.AssertionType ?? "");
                if (ContainsAny(key, workflowKeywords))
                    wSteps.Add(ar);
                else if (ContainsAny(key, clientKeywords))
                    cSteps.Add(ar);
                else if (ContainsAny(key, gapKeywords))
                    gSteps.Add(ar);
                else
                    wSteps.Add(ar); // 默认归入工作流
            }

            return new AssertionSummaryTriple
            {
                Workflow = BuildSummaryFromAssertions("run_full_audit_workflow_flow", wSteps),
                Client = BuildSummaryFromAssertions("verify_client_only_features", cSteps),
                Gaps = BuildSummaryFromAssertions("detect_workflow_gaps", gSteps)
            };
        }

        private static bool ContainsAny(string text, HashSet<string> keywords)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (var kw in keywords)
            {
                if (text.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static ScenarioSummary BuildSummaryFromAssertions(string name, List<AssertionResult> steps)
        {
            var summary = new ScenarioSummary
            {
                ScenarioName = name,
                TotalSteps = steps.Count,
                PassedSteps = steps.Count(s => s.Passed),
                FailedSteps = new List<string>(),
                Passed = steps.Count > 0 && steps.All(s => s.Passed),
                Summary = steps.Count(s => s.Passed) + "/" + steps.Count + " steps passed"
            };
            foreach (var s in steps)
            {
                if (!s.Passed)
                    summary.FailedSteps.Add((s.Message ?? s.AssertionType ?? "(unnamed)"));
            }
            return summary;
        }

        private static void CollectBlockers(ScenarioSummary summary, string category, List<string> blockers)
        {
            if (summary == null || summary.Passed) return;
            blockers.Add("[" + category + "] " + summary.ScenarioName + ": " + summary.Summary);
            if (summary.FailedSteps != null)
            {
                foreach (var fs in summary.FailedSteps)
                    blockers.Add("  - " + fs);
            }
        }

        private static string BuildReport(ScenarioSummary workflow, ScenarioSummary client, ScenarioSummary gaps, bool allPassed, List<string> blockers)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 云端审计工作流验收报告");
            sb.AppendLine();
            sb.AppendLine("**生成时间**: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("**验收结论**: " + (allPassed ? "✅ **GO** - 云端审计工作流验收通过,用户可开始正常审计工作" : "❌ **NO-GO** - 发现 " + blockers.Count + " 个阻塞问题"));
            sb.AppendLine();

            // 汇总表
            sb.AppendLine("## 验收汇总");
            sb.AppendLine();
            sb.AppendLine("| 类别 | 场景名 | 通过步骤/总步骤 | 结论 |");
            sb.AppendLine("|------|--------|-----------------|------|");
            AppendSummaryRow(sb, "API 工作流", workflow);
            AppendSummaryRow(sb, "客户端功能", client);
            AppendSummaryRow(sb, "工作流断点", gaps);
            sb.AppendLine();

            // 详情
            if (workflow != null)
            {
                sb.AppendLine("## 1. API 工作流测试结果（run_full_audit_workflow_flow）");
                sb.AppendLine();
                AppendDetail(sb, workflow);
            }

            if (client != null)
            {
                sb.AppendLine("## 2. 客户端功能验证结果（verify_client_only_features）");
                sb.AppendLine();
                AppendDetail(sb, client);
            }

            if (gaps != null)
            {
                sb.AppendLine("## 3. 工作流断点检测结果（detect_workflow_gaps）");
                sb.AppendLine();
                AppendDetail(sb, gaps);
            }

            // 阻塞问题
            if (!allPassed && blockers.Count > 0)
            {
                sb.AppendLine("## 阻塞问题清单");
                sb.AppendLine();
                foreach (var b in blockers)
                    sb.AppendLine("- " + b);
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine("*本报告由 generate_acceptance_report 工具自动生成*");
            return sb.ToString();
        }

        private static void AppendSummaryRow(StringBuilder sb, string category, ScenarioSummary s)
        {
            if (s == null)
            {
                sb.AppendLine("| " + category + " | (未执行) | - | ⚠️ 未测试 |");
            }
            else
            {
                string verdict = s.Passed ? "✅ PASS" : "❌ FAIL";
                sb.AppendLine("| " + category + " | " + s.ScenarioName + " | " + s.PassedSteps + "/" + s.TotalSteps + " | " + verdict + " |");
            }
        }

        private static void AppendDetail(StringBuilder sb, ScenarioSummary s)
        {
            sb.AppendLine("- **场景名**: " + s.ScenarioName);
            sb.AppendLine("- **通过**: " + (s.Passed ? "YES" : "NO"));
            sb.AppendLine("- **步骤统计**: " + (s.Summary ?? (s.PassedSteps + "/" + s.TotalSteps + " steps passed")));
            if (s.DurationMs > 0)
                sb.AppendLine("- **耗时**: " + s.DurationMs + " ms");
            if (s.FailedSteps != null && s.FailedSteps.Count > 0)
            {
                sb.AppendLine("- **失败步骤**:");
                foreach (var fs in s.FailedSteps)
                    sb.AppendLine("  - " + fs);
            }
            sb.AppendLine();
        }

        /// <summary>
        /// 场景摘要（内部模型）。
        /// </summary>
        private class ScenarioSummary
        {
            public string ScenarioName { get; set; }
            public bool Passed { get; set; }
            public int TotalSteps { get; set; }
            public int PassedSteps { get; set; }
            public long DurationMs { get; set; }
            public string Summary { get; set; }
            public List<string> FailedSteps { get; set; }
        }

        /// <summary>
        /// 三类场景摘要的容器（替代元组返回）。
        /// </summary>
        private class AssertionSummaryTriple
        {
            public ScenarioSummary Workflow;
            public ScenarioSummary Client;
            public ScenarioSummary Gaps;
        }
    }
}
