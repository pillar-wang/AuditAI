﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 测试报告工具集（Task 10）
    /// 提供测试报告生成、基线对比、基线保存、已知失败列举能力。
    /// 所有工具直接调用 TestReportService 静态方法，返回 JSON 字符串。
    /// </summary>
    public static class TestReportTools
    {
        private static readonly string DefaultBaselinePath = @"e:\lq\.trae\specs\cloud-e2e-automation\reports\baseline.md";

        /// <summary>
        /// 注册所有测试报告工具（4 个）。
        /// </summary>
        public static void Register()
        {
            RegisterGenerateTestReport();
            RegisterCompareWithBaseline();
            RegisterSaveBaseline();
            RegisterListKnownFailures();
        }

        // =====================================================================
        // generate_test_report
        // 生成当前会话的测试报告（Markdown），写入 reports 目录。
        // 汇总 SessionState.AssertionResults 中所有断言结果。
        // =====================================================================
        private static void RegisterGenerateTestReport()
        {
            ToolRegistry.Register("generate_test_report",
                "生成当前会话的测试报告（Markdown 格式），写入 reports 目录。汇总 SessionState.AssertionResults 中所有断言结果，" +
                "返回报告文件路径、内容、总数/通过/失败/通过率与耗时。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["format"] = new JObject { ["type"] = "string", ["description"] = "报告格式（默认 markdown，目前仅支持 markdown）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string format = args != null && args["format"] != null ? args["format"].ToString() : "markdown";
                    if (string.IsNullOrEmpty(format)) format = "markdown";
                    return TestReportService.GenerateReport(format).ToJson();
                });
        }

        // =====================================================================
        // compare_with_baseline
        // 与基线报告对比，识别新增失败、已修复失败、未变化失败。
        // =====================================================================
        private static void RegisterCompareWithBaseline()
        {
            ToolRegistry.Register("compare_with_baseline",
                "与基线报告对比，识别新增失败、已修复失败、未变化失败。基线默认为 reports/baseline.md。" +
                "失败签名格式为 \"{AssertionType}:{Expected}\"。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["baselinePath"] = new JObject { ["type"] = "string", ["description"] = "基线报告路径（默认 reports/baseline.md）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string baselinePath = args != null && args["baselinePath"] != null ? args["baselinePath"].ToString() : DefaultBaselinePath;
                    if (string.IsNullOrEmpty(baselinePath)) baselinePath = DefaultBaselinePath;
                    return TestReportService.CompareWithBaseline(baselinePath).ToJson();
                });
        }

        // =====================================================================
        // save_baseline
        // 保存当前测试结果为新基线（覆盖现有 baseline.md）。
        // =====================================================================
        private static void RegisterSaveBaseline()
        {
            ToolRegistry.Register("save_baseline",
                "保存当前测试结果为新基线（覆盖现有 reports/baseline.md）。后续 compare_with_baseline 将以此基线为参照。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => TestReportService.SaveBaseline().ToJson());
        }

        // =====================================================================
        // list_known_failures
        // 列出当前会话中所有失败的断言。
        // =====================================================================
        private static void RegisterListKnownFailures()
        {
            ToolRegistry.Register("list_known_failures",
                "列出当前会话中所有失败的断言（SessionState.AssertionResults 中 Passed=false 的条目），" +
                "返回总数与失败明细（AssertionType/Actual/Expected/Message/Timestamp）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    var failures = TestReportService.ListKnownFailures();
                    return JsonConvert.SerializeObject(new { total = failures.Count, failures = failures }, Formatting.Indented);
                });
        }
    }
}
