﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AuditAI.McpServer.State;
using Newtonsoft.Json;

namespace AuditAI.McpServer.Services
{
    // =====================================================================
    // 结果模型
    // =====================================================================

    /// <summary>
    /// 测试报告生成结果（Task 10.1）
    /// </summary>
    public class TestReportResult
    {
        public bool Success { get; set; }
        public string ReportPath { get; set; }
        public string ReportContent { get; set; }
        public int TotalAssertions { get; set; }
        public int PassedAssertions { get; set; }
        public int FailedAssertions { get; set; }
        public long DurationMs { get; set; }
        public string Error { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                success = Success,
                reportPath = ReportPath,
                reportContent = ReportContent,
                totalAssertions = TotalAssertions,
                passedAssertions = PassedAssertions,
                failedAssertions = FailedAssertions,
                durationMs = DurationMs,
                error = Error
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// 基线对比结果。
    /// </summary>
    public class CompareResult
    {
        public bool Success { get; set; }
        public List<string> NewFailures { get; set; }       // 当前有但基线没有的失败
        public List<string> FixedFailures { get; set; }     // 基线有但当前没有的失败
        public List<string> UnchangedFailures { get; set; } // 基线和当前都有的失败
        public string Error { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                success = Success,
                newFailures = NewFailures ?? new List<string>(),
                fixedFailures = FixedFailures ?? new List<string>(),
                unchangedFailures = UnchangedFailures ?? new List<string>(),
                error = Error
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// 保存基线结果。
    /// </summary>
    public class SaveBaselineResult
    {
        public bool Success { get; set; }
        public string BaselinePath { get; set; }
        public int TotalAssertions { get; set; }
        public string Error { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                success = Success,
                baselinePath = BaselinePath,
                totalAssertions = TotalAssertions,
                error = Error
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// 已知失败条目（从 AssertionResults 中筛选失败的断言）。
    /// </summary>
    public class KnownFailure
    {
        public string AssertionType { get; set; }
        public string Actual { get; set; }
        public string Expected { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // =====================================================================
    // 服务
    // =====================================================================

    /// <summary>
    /// 测试报告服务（Task 10）
    /// 提供测试报告生成、基线对比、已知失败列举能力。
    /// 所有方法不抛异常：异常被捕获后填充结果对象的 Error 字段。
    /// 报告输出目录：e:\lq\.trae\specs\cloud-e2e-automation\reports\
    /// </summary>
    public static class TestReportService
    {
        private static readonly string ReportsDir = Environment.GetEnvironmentVariable("AUDITAI_E2E_REPORTS_DIR")
            ?? @"e:\lq\.trae\specs\cloud-e2e-automation\reports";

        /// <summary>
        /// 生成 Markdown 测试报告，写入 reports 目录，返回文件路径与内容。
        /// 目前仅支持 markdown 格式（format 参数仅作占位，便于后续扩展）。
        /// </summary>
        public static TestReportResult GenerateReport(string format = "markdown")
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                List<AssertionResult> assertions = SessionState.AssertionResults ?? new List<AssertionResult>();
                int total = assertions.Count;
                int passed = 0;
                for (int i = 0; i < assertions.Count; i++)
                {
                    if (assertions[i].Passed) passed++;
                }
                int failed = total - passed;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# 云端验证测试报告");
                sb.AppendLine();
                sb.AppendLine("**生成时间**: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("**总断言数**: " + total);
                sb.AppendLine("**通过**: " + passed);
                sb.AppendLine("**失败**: " + failed);
                double passRate = total > 0 ? (double)passed / total * 100 : 0;
                sb.AppendLine("**通过率**: " + passRate.ToString("F1") + "%");
                sb.AppendLine();
                sb.AppendLine("## 断言详情");
                sb.AppendLine();
                sb.AppendLine("| # | 类型 | 通过 | 实际值 | 期望值 | 消息 | 时间 |");
                sb.AppendLine("|---|------|------|--------|--------|------|------|");
                for (int i = 0; i < assertions.Count; i++)
                {
                    AssertionResult a = assertions[i];
                    sb.AppendLine(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "| {0} | {1} | {2} | {3} | {4} | {5} | {6} |",
                        i + 1,
                        Escape(a.AssertionType),
                        a.Passed ? "✓" : "✗",
                        Escape(a.Actual),
                        Escape(a.Expected),
                        Escape(a.Message),
                        a.Timestamp.ToString("HH:mm:ss")));
                }

                Directory.CreateDirectory(ReportsDir);
                string fileName = "report-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".md";
                string fullPath = Path.Combine(ReportsDir, fileName);
                File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);

                sw.Stop();
                return new TestReportResult
                {
                    Success = true,
                    ReportPath = fullPath,
                    ReportContent = sb.ToString(),
                    TotalAssertions = total,
                    PassedAssertions = passed,
                    FailedAssertions = failed,
                    DurationMs = sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new TestReportResult
                {
                    Success = false,
                    DurationMs = sw.ElapsedMilliseconds,
                    Error = GetRootMessage(ex)
                };
            }
        }

        /// <summary>
        /// 与基线报告对比，识别新增失败、已修复失败、未变化失败。
        /// 失败签名格式："{AssertionType}:{Expected}"。
        /// </summary>
        public static CompareResult CompareWithBaseline(string baselinePath)
        {
            CompareResult empty = new CompareResult
            {
                NewFailures = new List<string>(),
                FixedFailures = new List<string>(),
                UnchangedFailures = new List<string>()
            };
            try
            {
                if (string.IsNullOrEmpty(baselinePath) || !File.Exists(baselinePath))
                {
                    empty.Success = false;
                    empty.Error = "基线报告不存在: " + (baselinePath ?? "(null)");
                    return empty;
                }

                List<string> baselineFailures = ParseFailuresFromReport(File.ReadAllText(baselinePath, Encoding.UTF8));

                List<AssertionResult> assertions = SessionState.AssertionResults ?? new List<AssertionResult>();
                List<string> currentFailuresList = new List<string>();
                for (int i = 0; i < assertions.Count; i++)
                {
                    AssertionResult a = assertions[i];
                    if (!a.Passed)
                    {
                        currentFailuresList.Add(a.AssertionType + ":" + (a.Expected ?? ""));
                    }
                }

                HashSet<string> currentSet = new HashSet<string>(currentFailuresList);
                HashSet<string> baselineSet = new HashSet<string>(baselineFailures);

                List<string> newFailures = new List<string>();
                List<string> fixedFailures = new List<string>();
                List<string> unchangedFailures = new List<string>();

                foreach (string f in currentFailuresList)
                {
                    if (baselineSet.Contains(f))
                    {
                        if (!unchangedFailures.Contains(f)) unchangedFailures.Add(f);
                    }
                    else
                    {
                        if (!newFailures.Contains(f)) newFailures.Add(f);
                    }
                }
                foreach (string f in baselineFailures)
                {
                    if (!currentSet.Contains(f))
                    {
                        if (!fixedFailures.Contains(f)) fixedFailures.Add(f);
                    }
                }

                return new CompareResult
                {
                    Success = true,
                    NewFailures = newFailures,
                    FixedFailures = fixedFailures,
                    UnchangedFailures = unchangedFailures
                };
            }
            catch (Exception ex)
            {
                empty.Success = false;
                empty.Error = GetRootMessage(ex);
                return empty;
            }
        }

        /// <summary>
        /// 保存当前测试结果为新基线（覆盖现有 baseline.md）。
        /// 复用 GenerateReport 逻辑生成报告后复制到 baseline.md。
        /// </summary>
        public static SaveBaselineResult SaveBaseline()
        {
            try
            {
                Directory.CreateDirectory(ReportsDir);
                string baselinePath = Path.Combine(ReportsDir, "baseline.md");

                TestReportResult report = GenerateReport("markdown");
                if (!report.Success || string.IsNullOrEmpty(report.ReportPath) || !File.Exists(report.ReportPath))
                {
                    return new SaveBaselineResult
                    {
                        Success = false,
                        Error = "生成报告失败: " + (report.Error ?? "(unknown)")
                    };
                }

                File.Copy(report.ReportPath, baselinePath, overwrite: true);

                return new SaveBaselineResult
                {
                    Success = true,
                    BaselinePath = baselinePath,
                    TotalAssertions = report.TotalAssertions
                };
            }
            catch (Exception ex)
            {
                return new SaveBaselineResult
                {
                    Success = false,
                    Error = GetRootMessage(ex)
                };
            }
        }

        /// <summary>
        /// 列出当前会话中所有失败的断言（从 AssertionResults 中筛选 Passed=false 的条目）。
        /// </summary>
        public static List<KnownFailure> ListKnownFailures()
        {
            List<KnownFailure> failures = new List<KnownFailure>();
            try
            {
                List<AssertionResult> assertions = SessionState.AssertionResults ?? new List<AssertionResult>();
                for (int i = 0; i < assertions.Count; i++)
                {
                    AssertionResult a = assertions[i];
                    if (!a.Passed)
                    {
                        failures.Add(new KnownFailure
                        {
                            AssertionType = a.AssertionType,
                            Actual = a.Actual,
                            Expected = a.Expected,
                            Message = a.Message,
                            Timestamp = a.Timestamp
                        });
                    }
                }
            }
            catch
            {
                // 防御性：异常时返回已收集的部分
            }
            return failures;
        }

        // =====================================================================
        // 内部辅助
        // =====================================================================

        /// <summary>
        /// Markdown 表格单元格转义：| 转义为 \|，换行符替换为空格。
        /// </summary>
        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("|", "\\|").Replace("\n", " ").Replace("\r", "");
        }

        /// <summary>
        /// 从 Markdown 报告中解析失败行，提取 "Type:Expected" 签名。
        /// 表格列：| # | 类型 | 通过 | 实际值 | 期望值 | 消息 | 时间 |
        /// 失败行通过 "| ✗ |" 标记识别。
        /// </summary>
        private static List<string> ParseFailuresFromReport(string reportContent)
        {
            List<string> failures = new List<string>();
            if (string.IsNullOrEmpty(reportContent)) return failures;

            string[] lines = reportContent.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("|") && line.Contains("| ✗ |"))
                {
                    string[] parts = line.Split('|');
                    if (parts.Length >= 6)
                    {
                        string type = parts[2].Trim();
                        string expected = parts[5].Trim();
                        string sig = type + ":" + expected;
                        if (!failures.Contains(sig)) failures.Add(sig);
                    }
                }
            }
            return failures;
        }

        private static string GetRootMessage(Exception ex)
        {
            if (ex == null) return "(null)";
            Exception inner = ex;
            while (inner.InnerException != null) inner = inner.InnerException;
            return inner.Message ?? ex.Message ?? "(no message)";
        }
    }
}
