﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 综合报告工具集（Task 2 + Task 13）
    /// 提供端点覆盖率分析（list_uncovered_endpoints）、按模块分组的 Go/No-Go 综合报告
    /// （generate_comprehensive_report）、与基线对比（compare_with_comprehensive_baseline）能力。
    /// 所有工具不抛异常：异常被捕获后以 success=false + error 返回。
    /// 报告输出目录：e:\lq\.trae\specs\cloud-comprehensive-automation-coverage\reports\
    /// </summary>
    public static class ComprehensiveReportTools
    {
        private static readonly string ReportsDir = Environment.GetEnvironmentVariable("AUDITAI_REPORTS_DIR")
            ?? @"e:\lq\.trae\specs\cloud-comprehensive-automation-coverage\reports";
        private static readonly string ServerProgramPath = Environment.GetEnvironmentVariable("AUDITAI_SERVER_PROGRAM_PATH")
            ?? @"e:\lq\Server\AuditApiServer\Program.cs";

        // 模块顺序（用于报告输出与 JSON 字段排序）
        private static readonly string[] ModuleOrder =
        {
            "User", "Team", "Project", "TableSync", "DocumentSync",
            "File", "Task", "DataSource", "License", "Admin", "Other"
        };

        // 服务端端点提取正则：兼容 app.MapGet("/path") 与 app.MapHub<ChatHub>("/path")
        private static readonly Regex EndpointRegex = new Regex(
            @"app\.(MapGet|MapPost|MapPut|MapDelete|MapHub)(?:<[^>]*>)?\s*\(\s*""([^""]+)""",
            RegexOptions.Compiled);

        // 从断言文本中提取工具名（cloud_* / admin_*）
        private static readonly Regex ToolNamePattern = new Regex(
            @"(cloud_[a-z_]+|admin_[a-z_]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // 团队相关 action（跨 /api/Project/ 与 /api/User/）
        private static readonly HashSet<string> TeamActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CreateTeam", "DismissTeam", "UpdateTeamName", "UpdateCurrentTeam",
            "GetTeamUsers", "GetTeamUserGroups", "GetTeamUsersWithPic",
            "AddUserToTeam", "RemoveUserFromTeam",
            "AddUserGroup", "MoveUserToGroup", "DeleteUserGroup", "RenameUserGroup",
            "UpdateJobTitle", "AllowTeamMerge", "GetPendingInvitations",
            "GetTeamUserPermissions", "SetUserTeamPermissions", "TeamMergeRequest", "AcceptInvitation"
        };

        // 表格同步 action
        private static readonly HashSet<string> TableSyncActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PushTableQuick", "PushTable", "PullTable", "RevertTable",
            "GetTableTimeline", "QueryTableVersions", "GetTableRevertDiff", "GetTableColumns"
        };

        // 文档同步 action
        private static readonly HashSet<string> DocumentSyncActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PushDocumentQuick", "PushDocument", "PullDocument", "RevertDocument",
            "GetDocumentTimeline", "QueryDocumentVersions", "GetDocumentRevertDiff"
        };

        // 文件/图片/PDF action
        private static readonly HashSet<string> FileActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "UploadFile", "DownloadFile",
            "PushImage", "PullImage", "QueryImageVersions",
            "PushPdf", "PullPdf", "QueryPdfVersions"
        };

        // 工具名 → 端点路径映射（104 个 cloud_* + 26 个 admin_*）
        private static readonly Dictionary<string, string> ToolEndpointMap = BuildToolEndpointMap();

        /// <summary>
        /// 注册所有综合报告工具（3 个）。
        /// </summary>
        public static void Register()
        {
            RegisterListUncoveredEndpoints();
            RegisterGenerateComprehensiveReport();
            RegisterCompareWithComprehensiveBaseline();
        }

        // =====================================================================
        // list_uncovered_endpoints
        // 扫描服务端 Program.cs 所有 MapGet/MapPost/MapPut/MapDelete/MapHub 端点，
        // 与已注册的 cloud_* + admin_* 工具映射对比，输出未覆盖端点 Markdown 报告。
        // =====================================================================
        private static void RegisterListUncoveredEndpoints()
        {
            ToolRegistry.Register("list_uncovered_endpoints",
                "扫描服务端 Program.cs 所有 HTTP 端点（MapGet/MapPost/MapPut/MapDelete/MapHub），" +
                "与已注册的 cloud_* + admin_* 工具映射对比，计算未覆盖端点列表与覆盖率百分比，" +
                "输出 Markdown 报告到 reports 目录。返回 {totalEndpoints, coveredEndpoints, uncoveredEndpoints, coveragePercent, uncoveredList, reportPath}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    try
                    {
                        if (!File.Exists(ServerProgramPath))
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                success = false,
                                error = "服务端 Program.cs 不存在: " + ServerProgramPath
                            }, Formatting.Indented);
                        }

                        string programContent = File.ReadAllText(ServerProgramPath, Encoding.UTF8);
                        List<string> serverEndpoints = ExtractServerEndpoints(programContent);

                        // 已覆盖端点集合（来自工具映射，已规范化去重）
                        HashSet<string> coveredSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in ToolEndpointMap)
                        {
                            coveredSet.Add(NormalizePath(pair.Value));
                        }

                        List<string> uncoveredList = new List<string>();
                        HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < serverEndpoints.Count; i++)
                        {
                            string ep = serverEndpoints[i];
                            if (seen.Contains(ep)) continue;
                            seen.Add(ep);
                            if (!coveredSet.Contains(ep))
                            {
                                uncoveredList.Add(ep);
                            }
                        }

                        int total = seen.Count;
                        int covered = total - uncoveredList.Count;
                        double coveragePercent = total > 0 ? (double)covered / total * 100 : 0;

                        EnsureReportsDirectory();
                        string reportPath = Path.Combine(ReportsDir, "coverage-" + FormatTimestamp() + ".md");
                        string reportContent = GenerateCoverageMarkdown(serverEndpoints, seen, coveredSet, uncoveredList, total, covered, coveragePercent);
                        File.WriteAllText(reportPath, reportContent, Encoding.UTF8);

                        return JsonConvert.SerializeObject(new
                        {
                            success = true,
                            totalEndpoints = total,
                            coveredEndpoints = covered,
                            uncoveredEndpoints = uncoveredList.Count,
                            coveragePercent = Math.Round(coveragePercent, 2),
                            uncoveredList = uncoveredList,
                            reportPath = reportPath
                        }, Formatting.Indented);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            error = GetRootMessage(ex)
                        }, Formatting.Indented);
                    }
                });
        }

        // =====================================================================
        // generate_comprehensive_report
        // 汇总 SessionState.AssertionResults，按 10 个模块分组，
        // 生成含模块覆盖矩阵、失败详情、Go/No-Go 结论的 Markdown 综合报告。
        // =====================================================================
        private static void RegisterGenerateComprehensiveReport()
        {
            ToolRegistry.Register("generate_comprehensive_report",
                "汇总当前会话所有断言结果（SessionState.AssertionResults），按 10 个模块分组" +
                "（User/Team/Project/TableSync/DocumentSync/File/Task/DataSource/License/Admin），" +
                "生成含模块覆盖矩阵、失败用例详情、Go/No-Go 结论的 Markdown 综合报告，" +
                "保存到 reports 目录。返回 {reportPath, goNoGo, totalAssertions, passed, failed, moduleStats}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    try
                    {
                        List<AssertionResult> assertions = SessionState.AssertionResults ?? new List<AssertionResult>();
                        Dictionary<string, ModuleStat> stats = ComputeModuleStats(assertions);

                        int total = 0, passed = 0, failed = 0;
                        foreach (var pair in stats)
                        {
                            total += pair.Value.Total;
                            passed += pair.Value.Passed;
                            failed += pair.Value.Failed;
                        }

                        string goNoGo = (failed == 0 && total > 0) ? "GO"
                            : (failed == 0 && total == 0 ? "NO-GO" : "NO-GO");
                        // 全部通过 = GO；任一失败或无断言 = NO-GO

                        EnsureReportsDirectory();
                        string reportPath = Path.Combine(ReportsDir, "comprehensive-" + FormatTimestamp() + ".md");
                        string reportContent = GenerateComprehensiveMarkdown(stats, assertions, total, passed, failed, goNoGo);
                        File.WriteAllText(reportPath, reportContent, Encoding.UTF8);

                        JObject moduleStats = new JObject();
                        foreach (string module in ModuleOrder)
                        {
                            if (!stats.ContainsKey(module)) continue;
                            ModuleStat s = stats[module];
                            moduleStats[module] = new JObject
                            {
                                ["total"] = s.Total,
                                ["passed"] = s.Passed,
                                ["failed"] = s.Failed,
                                ["passRate"] = Math.Round(s.PassRate, 2)
                            };
                        }

                        return JsonConvert.SerializeObject(new
                        {
                            success = true,
                            reportPath = reportPath,
                            goNoGo = goNoGo,
                            totalAssertions = total,
                            passed = passed,
                            failed = failed,
                            moduleStats = moduleStats
                        }, Formatting.Indented);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            error = GetRootMessage(ex)
                        }, Formatting.Indented);
                    }
                });
        }

        // =====================================================================
        // compare_with_comprehensive_baseline
        // 读取基线综合报告（Markdown），解析模块统计与失败签名，
        // 与当前 SessionState.AssertionResults 对比。
        // =====================================================================
        private static void RegisterCompareWithComprehensiveBaseline()
        {
            ToolRegistry.Register("compare_with_comprehensive_baseline",
                "与基线综合报告对比，识别新增失败、已修复失败、未变化失败，以及各模块失败数变化。" +
                "基线报告为 generate_comprehensive_report 生成的 Markdown。失败签名格式为 \"module:assertionType:expected\"。" +
                "返回 {newFailures, fixedFailures, unchangedFailures, moduleDelta}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["baselinePath"] = new JObject { ["type"] = "string", ["description"] = "基线综合报告路径（Markdown）" }
                    },
                    ["required"] = new JArray { "baselinePath" }
                },
                (args) =>
                {
                    try
                    {
                        string baselinePath = args != null && args["baselinePath"] != null ? args["baselinePath"].ToString() : null;
                        if (string.IsNullOrEmpty(baselinePath) || !File.Exists(baselinePath))
                        {
                            return JsonConvert.SerializeObject(new
                            {
                                success = false,
                                error = "基线报告不存在: " + (baselinePath ?? "(null)"),
                                newFailures = new List<string>(),
                                fixedFailures = new List<string>(),
                                unchangedFailures = new List<string>(),
                                moduleDelta = new JObject()
                            }, Formatting.Indented);
                        }

                        string baselineContent = File.ReadAllText(baselinePath, Encoding.UTF8);
                        Dictionary<string, ModuleStat> baselineStats = ParseBaselineModuleStats(baselineContent);
                        List<string> baselineFailures = ParseBaselineFailures(baselineContent);

                        List<AssertionResult> assertions = SessionState.AssertionResults ?? new List<AssertionResult>();
                        Dictionary<string, ModuleStat> currentStats = ComputeModuleStats(assertions);

                        List<string> currentFailures = new List<string>();
                        foreach (var pair in currentStats)
                        {
                            foreach (string sig in pair.Value.FailureSignatures)
                            {
                                if (!currentFailures.Contains(sig)) currentFailures.Add(sig);
                            }
                        }

                        HashSet<string> baselineSet = new HashSet<string>(baselineFailures, StringComparer.OrdinalIgnoreCase);
                        HashSet<string> currentSet = new HashSet<string>(currentFailures, StringComparer.OrdinalIgnoreCase);

                        List<string> newFailures = new List<string>();
                        List<string> fixedFailures = new List<string>();
                        List<string> unchangedFailures = new List<string>();

                        for (int i = 0; i < currentFailures.Count; i++)
                        {
                            string f = currentFailures[i];
                            if (baselineSet.Contains(f))
                            {
                                if (!unchangedFailures.Contains(f)) unchangedFailures.Add(f);
                            }
                            else
                            {
                                if (!newFailures.Contains(f)) newFailures.Add(f);
                            }
                        }
                        for (int i = 0; i < baselineFailures.Count; i++)
                        {
                            string f = baselineFailures[i];
                            if (!currentSet.Contains(f))
                            {
                                if (!fixedFailures.Contains(f)) fixedFailures.Add(f);
                            }
                        }

                        // 各模块失败数变化
                        JObject moduleDelta = new JObject();
                        HashSet<string> allModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var m in baselineStats.Keys) allModules.Add(m);
                        foreach (var m in currentStats.Keys) allModules.Add(m);
                        List<string> orderedModules = new List<string>();
                        foreach (string m in ModuleOrder)
                        {
                            if (allModules.Contains(m)) orderedModules.Add(m);
                        }
                        foreach (string m in allModules)
                        {
                            if (!orderedModules.Contains(m)) orderedModules.Add(m);
                        }
                        foreach (string module in orderedModules)
                        {
                            int baselineFailed = baselineStats.ContainsKey(module) ? baselineStats[module].Failed : 0;
                            int currentFailed = currentStats.ContainsKey(module) ? currentStats[module].Failed : 0;
                            moduleDelta[module] = new JObject
                            {
                                ["baselineFailed"] = baselineFailed,
                                ["currentFailed"] = currentFailed,
                                ["delta"] = currentFailed - baselineFailed
                            };
                        }

                        return JsonConvert.SerializeObject(new
                        {
                            success = true,
                            newFailures = newFailures,
                            fixedFailures = fixedFailures,
                            unchangedFailures = unchangedFailures,
                            moduleDelta = moduleDelta
                        }, Formatting.Indented);
                    }
                    catch (Exception ex)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            error = GetRootMessage(ex),
                            newFailures = new List<string>(),
                            fixedFailures = new List<string>(),
                            unchangedFailures = new List<string>(),
                            moduleDelta = new JObject()
                        }, Formatting.Indented);
                    }
                });
        }

        // =====================================================================
        // 辅助方法
        // =====================================================================

        /// <summary>
        /// 根据工具名返回所属模块名。
        /// admin_* → Admin；cloud_* 通过工具→端点映射再由端点路径推导模块。
        /// 未知工具返回 "Other"。
        /// </summary>
        private static string GetModuleForTool(string toolName)
        {
            if (string.IsNullOrEmpty(toolName)) return "Other";
            if (toolName.StartsWith("admin_", StringComparison.OrdinalIgnoreCase)) return "Admin";
            string endpoint;
            if (!ToolEndpointMap.TryGetValue(toolName, out endpoint) || string.IsNullOrEmpty(endpoint))
                return "Other";
            return GetModuleForEndpoint(endpoint);
        }

        /// <summary>
        /// 根据服务端端点路径返回所属模块名。
        /// </summary>
        private static string GetModuleForEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return "Other";
            string path = endpoint.TrimEnd('/');
            if (path.Length == 0) return "Other";

            if (path.StartsWith("/api/Admin/", StringComparison.OrdinalIgnoreCase)) return "Admin";
            if (path.StartsWith("/api/User/", StringComparison.OrdinalIgnoreCase))
            {
                string action = path.Substring("/api/User/".Length);
                if (TeamActions.Contains(action)) return "Team";
                return "User";
            }
            if (path.StartsWith("/api/ServerTask/", StringComparison.OrdinalIgnoreCase)) return "Task";
            if (path.StartsWith("/api/DataSource/", StringComparison.OrdinalIgnoreCase)) return "DataSource";
            if (path.StartsWith("/api/License/", StringComparison.OrdinalIgnoreCase)) return "License";
            if (path.StartsWith("/api/Project/", StringComparison.OrdinalIgnoreCase))
            {
                string action = path.Substring("/api/Project/".Length);
                if (TeamActions.Contains(action)) return "Team";
                if (TableSyncActions.Contains(action)) return "TableSync";
                if (DocumentSyncActions.Contains(action)) return "DocumentSync";
                if (FileActions.Contains(action)) return "File";
                return "Project";
            }
            return "Other";
        }

        /// <summary>
        /// 从断言结果中推断所属模块：扫描 AssertionType + Message 中的工具名，
        /// 命中已知工具则用其模块，否则返回 "Other"。
        /// </summary>
        private static string GetModuleForAssertion(AssertionResult a)
        {
            string text = (a.AssertionType ?? "") + " " + (a.Message ?? "");
            Match m = ToolNamePattern.Match(text);
            while (m.Success)
            {
                string tool = m.Value;
                if (ToolEndpointMap.ContainsKey(tool))
                {
                    return GetModuleForTool(tool);
                }
                m = m.NextMatch();
            }
            return "Other";
        }

        /// <summary>
        /// 从服务端 Program.cs 内容中提取所有端点路径（已规范化、去重保序）。
        /// </summary>
        private static List<string> ExtractServerEndpoints(string programContent)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(programContent)) return result;
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            MatchCollection matches = EndpointRegex.Matches(programContent);
            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (m.Groups.Count >= 3)
                {
                    string path = NormalizePath(m.Groups[2].Value);
                    if (seen.Contains(path)) continue;
                    seen.Add(path);
                    result.Add(path);
                }
            }
            return result;
        }

        /// <summary>
        /// 规范化路径：去除尾部斜杠（根路径除外），保留原始大小写。
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (path.Length > 1 && path.EndsWith("/")) return path.TrimEnd('/');
            return path;
        }

        /// <summary>
        /// 计算按模块分组的断言统计。
        /// </summary>
        private static Dictionary<string, ModuleStat> ComputeModuleStats(List<AssertionResult> assertions)
        {
            Dictionary<string, ModuleStat> stats = new Dictionary<string, ModuleStat>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < assertions.Count; i++)
            {
                AssertionResult a = assertions[i];
                string module = GetModuleForAssertion(a);
                if (!stats.ContainsKey(module))
                {
                    stats[module] = new ModuleStat { Module = module };
                }
                ModuleStat s = stats[module];
                s.Total++;
                if (a.Passed) s.Passed++;
                else
                {
                    s.Failed++;
                    string sig = module + ":" + (a.AssertionType ?? "") + ":" + (a.Expected ?? "");
                    if (!s.FailureSignatures.Contains(sig)) s.FailureSignatures.Add(sig);
                }
            }
            return stats;
        }

        /// <summary>
        /// 生成端点覆盖率 Markdown 报告。
        /// </summary>
        private static string GenerateCoverageMarkdown(List<string> serverEndpoints, HashSet<string> seenSet, HashSet<string> coveredSet, List<string> uncoveredList, int total, int covered, double coveragePercent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 云端功能端点覆盖率报告");
            sb.AppendLine();
            sb.AppendLine("**生成时间**: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            sb.AppendLine("**服务端源文件**: " + ServerProgramPath);
            sb.AppendLine("**总端点数**: " + total);
            sb.AppendLine("**已覆盖**: " + covered);
            sb.AppendLine("**未覆盖**: " + uncoveredList.Count);
            sb.AppendLine("**覆盖率**: " + coveragePercent.ToString("F2", CultureInfo.InvariantCulture) + "%");
            sb.AppendLine();
            sb.AppendLine("## 未覆盖端点列表");
            sb.AppendLine();
            if (uncoveredList.Count == 0)
            {
                sb.AppendLine("> 所有端点均已覆盖，无未覆盖项。");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("| # | 端点路径 | 所属模块 |");
                sb.AppendLine("|---|----------|----------|");
                for (int i = 0; i < uncoveredList.Count; i++)
                {
                    string ep = uncoveredList[i];
                    string module = GetModuleForEndpoint(ep);
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "| {0} | {1} | {2} |", i + 1, Escape(ep), module));
                }
                sb.AppendLine();
            }

            sb.AppendLine("## 全部端点覆盖明细");
            sb.AppendLine();
            sb.AppendLine("| # | 端点路径 | 所属模块 | 覆盖状态 |");
            sb.AppendLine("|---|----------|----------|----------|");
            int idx = 1;
            foreach (string ep in seenSet)
            {
                string module = GetModuleForEndpoint(ep);
                string status = coveredSet.Contains(ep) ? "已覆盖" : "未覆盖";
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "| {0} | {1} | {2} | {3} |", idx++, Escape(ep), module, status));
            }
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("*报告由 list_uncovered_endpoints 工具自动生成*");
            return sb.ToString();
        }

        /// <summary>
        /// 生成综合测试报告 Markdown（含模块矩阵、失败详情、Go/No-Go 结论）。
        /// </summary>
        private static string GenerateComprehensiveMarkdown(Dictionary<string, ModuleStat> stats, List<AssertionResult> assertions, int total, int passed, int failed, string goNoGo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 云端功能综合测试报告");
            sb.AppendLine();
            sb.AppendLine("**生成时间**: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            sb.AppendLine("**总断言数**: " + total);
            sb.AppendLine("**通过**: " + passed);
            sb.AppendLine("**失败**: " + failed);
            double passRate = total > 0 ? (double)passed / total * 100 : 0;
            sb.AppendLine("**通过率**: " + passRate.ToString("F1", CultureInfo.InvariantCulture) + "%");
            sb.AppendLine();
            sb.AppendLine("## Go/No-Go 决策");
            sb.AppendLine();
            if (goNoGo == "GO")
            {
                sb.AppendLine("**结论: GO** - 全部 " + total + " 个断言通过，可放行。");
            }
            else if (total == 0)
            {
                sb.AppendLine("**结论: NO-GO** - 当前会话无断言结果，无法判定。请先执行测试场景。");
            }
            else
            {
                sb.AppendLine("**结论: NO-GO** - 存在 " + failed + " 个失败用例，不可放行，请修复后回归。");
            }
            sb.AppendLine();
            sb.AppendLine("## 模块覆盖矩阵");
            sb.AppendLine();
            sb.AppendLine("| 模块 | 总断言数 | 通过数 | 失败数 | 通过率 |");
            sb.AppendLine("|------|----------|--------|--------|--------|");
            foreach (string module in ModuleOrder)
            {
                if (!stats.ContainsKey(module)) continue;
                ModuleStat s = stats[module];
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "| {0} | {1} | {2} | {3} | {4}% |",
                    Escape(module), s.Total, s.Passed, s.Failed,
                    s.PassRate.ToString("F1", CultureInfo.InvariantCulture)));
            }
            sb.AppendLine();

            sb.AppendLine("## 各模块通过/失败统计");
            sb.AppendLine();
            foreach (string module in ModuleOrder)
            {
                if (!stats.ContainsKey(module)) continue;
                ModuleStat s = stats[module];
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "- **{0}**: {1} 通过 / {2} 失败（通过率 {3}%）",
                    module, s.Passed, s.Failed, s.PassRate.ToString("F1", CultureInfo.InvariantCulture)));
            }
            sb.AppendLine();

            sb.AppendLine("## 失败用例详情");
            sb.AppendLine();
            if (failed == 0)
            {
                sb.AppendLine("> 无失败用例。");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("| # | 模块 | 断言类型 | 实际值 | 期望值 | 失败原因 |");
                sb.AppendLine("|---|------|----------|--------|--------|----------|");
                int failIdx = 1;
                for (int i = 0; i < assertions.Count; i++)
                {
                    AssertionResult a = assertions[i];
                    if (a.Passed) continue;
                    string module = GetModuleForAssertion(a);
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        "| {0} | {1} | {2} | {3} | {4} | {5} |",
                        failIdx++,
                        Escape(module),
                        Escape(a.AssertionType),
                        Escape(a.Actual),
                        Escape(a.Expected),
                        Escape(a.Message)));
                }
                sb.AppendLine();
            }

            sb.AppendLine("## 全部断言明细");
            sb.AppendLine();
            sb.AppendLine("| # | 模块 | 断言类型 | 通过 | 实际值 | 期望值 | 消息 | 时间 |");
            sb.AppendLine("|---|------|----------|------|--------|--------|------|------|");
            for (int i = 0; i < assertions.Count; i++)
            {
                AssertionResult a = assertions[i];
                string module = GetModuleForAssertion(a);
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} |",
                    i + 1,
                    Escape(module),
                    Escape(a.AssertionType),
                    a.Passed ? "PASS" : "FAIL",
                    Escape(a.Actual),
                    Escape(a.Expected),
                    Escape(a.Message),
                    a.Timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture)));
            }
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("*报告由 generate_comprehensive_report 工具自动生成*");
            return sb.ToString();
        }

        /// <summary>
        /// 从基线综合报告中解析模块统计（模块矩阵表）。
        /// 表头: | 模块 | 总断言数 | 通过数 | 失败数 | 通过率 |
        /// </summary>
        private static Dictionary<string, ModuleStat> ParseBaselineModuleStats(string reportContent)
        {
            Dictionary<string, ModuleStat> stats = new Dictionary<string, ModuleStat>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(reportContent)) return stats;

            string[] lines = reportContent.Split('\n');
            bool inMatrix = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');
                if (line.Contains("模块覆盖矩阵"))
                {
                    inMatrix = true;
                    continue;
                }
                if (inMatrix)
                {
                    if (!line.StartsWith("|")) continue;
                    // 跳过表头与分隔行
                    if (line.Contains("---") || line.Contains("模块") || line.Contains("总断言数")) continue;
                    string[] parts = line.Split('|');
                    if (parts.Length >= 6)
                    {
                        string module = parts[1].Trim();
                        int total = ParseIntSafe(parts[2].Trim());
                        int passed = ParseIntSafe(parts[3].Trim());
                        int failed = ParseIntSafe(parts[4].Trim());
                        if (!string.IsNullOrEmpty(module))
                        {
                            stats[module] = new ModuleStat
                            {
                                Module = module,
                                Total = total,
                                Passed = passed,
                                Failed = failed
                            };
                        }
                    }
                    // 空行表示矩阵结束
                    if (string.IsNullOrEmpty(line.Trim()) && stats.Count > 0) break;
                }
            }
            return stats;
        }

        /// <summary>
        /// 从基线综合报告中解析失败签名列表（失败用例详情表）。
        /// 表头: | # | 模块 | 断言类型 | 实际值 | 期望值 | 失败原因 |
        /// 签名格式: "module:assertionType:expected"
        /// </summary>
        private static List<string> ParseBaselineFailures(string reportContent)
        {
            List<string> failures = new List<string>();
            if (string.IsNullOrEmpty(reportContent)) return failures;

            string[] lines = reportContent.Split('\n');
            bool inFailures = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');
                if (line.Contains("失败用例详情"))
                {
                    inFailures = true;
                    continue;
                }
                if (inFailures)
                {
                    if (!line.StartsWith("|")) continue;
                    if (line.Contains("---") || line.Contains("断言类型") || line.Contains("失败原因")) continue;
                    string[] parts = line.Split('|');
                    if (parts.Length >= 6)
                    {
                        string module = parts[2].Trim();
                        string type = parts[3].Trim();
                        string expected = parts[5].Trim();
                        if (string.IsNullOrEmpty(module) && string.IsNullOrEmpty(type)) continue;
                        string sig = module + ":" + type + ":" + expected;
                        if (!failures.Contains(sig)) failures.Add(sig);
                    }
                }
            }
            return failures;
        }

        private static int ParseIntSafe(string s)
        {
            int v;
            if (int.TryParse(s, out v)) return v;
            // 尝试去除 % 等非数字字符
            if (!string.IsNullOrEmpty(s))
            {
                StringBuilder digits = new StringBuilder();
                for (int i = 0; i < s.Length; i++)
                {
                    if (char.IsDigit(s[i]) || s[i] == '-' || s[i] == '+') digits.Append(s[i]);
                }
                if (digits.Length > 0 && int.TryParse(digits.ToString(), out v)) return v;
            }
            return 0;
        }

        /// <summary>
        /// 确保 reports 目录存在。
        /// </summary>
        private static void EnsureReportsDirectory()
        {
            if (!Directory.Exists(ReportsDir))
            {
                Directory.CreateDirectory(ReportsDir);
            }
        }

        /// <summary>
        /// 格式化时间戳为 yyyyMMdd-HHmmss。
        /// </summary>
        private static string FormatTimestamp()
        {
            return DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Markdown 表格单元格转义：| 转义为 \|，换行符替换为空格。
        /// </summary>
        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("|", "\\|").Replace("\n", " ").Replace("\r", "");
        }

        private static string GetRootMessage(Exception ex)
        {
            if (ex == null) return "(null)";
            Exception inner = ex;
            while (inner.InnerException != null) inner = inner.InnerException;
            return inner.Message ?? ex.Message ?? "(no message)";
        }

        // =====================================================================
        // 数据模型
        // =====================================================================

        private class ModuleStat
        {
            public string Module { get; set; }
            public int Total { get; set; }
            public int Passed { get; set; }
            public int Failed { get; set; }
            public List<string> FailureSignatures { get; set; } = new List<string>();
            public double PassRate
            {
                get { return Total > 0 ? (double)Passed / Total * 100 : 0; }
            }
        }

        // =====================================================================
        // 工具名 → 端点路径映射（硬编码，104 个 cloud_* + 26 个 admin_*）
        // 数据来源：CloudApiTools.cs / AdminApiTools.cs 中每个工具的注释行。
        // =====================================================================
        private static Dictionary<string, string> BuildToolEndpointMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // ===== cloud_* 认证与用户模块 =====
            map["cloud_login"] = "/api/User/AccountLogin";
            map["cloud_login_by_sms"] = "/api/User/AccountLoginBySMS";
            map["cloud_sms_relogin"] = "/api/User/SMSReLogin";
            map["cloud_update_token"] = "/api/User/UpdateToken";
            map["cloud_client_quit"] = "/api/User/ClientQuit";
            map["cloud_register"] = "/api/User/Register";
            map["cloud_find_password"] = "/api/User/FindPassword";
            map["cloud_get_validate_code"] = "/api/User/GetValidateCode";
            map["cloud_get_user_info"] = "/api/User/GetUserById";
            map["cloud_update_user_info"] = "/api/User/UpdateUserInfo";
            map["cloud_user_name_exists"] = "/api/User/UserNameExists";
            map["cloud_check_user_name"] = "/api/User/CheckUserName";
            map["cloud_phone_exists"] = "/api/User/PhoneExists";
            map["cloud_get_user_by_name"] = "/api/User/GetUserByName";
            map["cloud_get_fuzzy_phone"] = "/api/User/GetFuzzyPhone";
            map["cloud_get_username_by_phone"] = "/api/User/GetUsernameByPhone";
            map["cloud_get_code_by_name"] = "/api/User/GetCodeByName";
            map["cloud_get_username_by_email"] = "/api/User/GetUsernameByEmail";
            map["cloud_get_validate_code_by_email"] = "/api/User/GetValidateCodeByEmail";
            map["cloud_get_delete_project_code"] = "/api/User/GetDeleteProjectValidateCode";
            map["cloud_reset_password"] = "/api/User/ResetPassword";
            map["cloud_reset_password_no_sms"] = "/api/User/ResetPasswordWithoutSMS";
            map["cloud_single_register"] = "/api/User/SingleRegister";
            map["cloud_batch_import"] = "/api/User/BatchImport";
            map["cloud_update_picture"] = "/api/User/UpdatePicture";
            map["cloud_update_phone_info"] = "/api/User/UpdatePhoneInfo";
            map["cloud_accept_invitation"] = "/api/User/AcceptInvitation";
            map["cloud_set_user_team_permissions"] = "/api/User/SetUserTeamPermissions";
            map["cloud_get_team_user_permissions"] = "/api/User/GetTeamUserPermissions";
            map["cloud_team_merge_request"] = "/api/User/TeamMergeRequest";

            // ===== cloud_* 团队与项目模块 =====
            map["cloud_create_team"] = "/api/Project/CreateTeam";
            map["cloud_dismiss_team"] = "/api/Project/DismissTeam";
            map["cloud_add_user_to_team"] = "/api/Project/AddUserToTeam";
            map["cloud_remove_user_from_team"] = "/api/Project/RemoveUserFromTeam";
            map["cloud_get_team_users"] = "/api/Project/GetTeamUsers";
            map["cloud_invite_user"] = "/api/Project/InviteUser";
            map["cloud_get_user_teams"] = "/api/Project/GetUserTeams";
            map["cloud_get_team_users_with_pic"] = "/api/Project/GetTeamUsersWithPic";
            map["cloud_get_team_user_groups"] = "/api/Project/GetTeamUserGroups";
            map["cloud_get_pending_invitations"] = "/api/Project/GetPendingInvitations";
            map["cloud_update_team_name"] = "/api/Project/UpdateTeamName";
            map["cloud_update_current_team"] = "/api/Project/UpdateCurrentTeam";
            map["cloud_add_user_group"] = "/api/Project/AddUserGroup";
            map["cloud_move_user_to_group"] = "/api/Project/MoveUserToGroup";
            map["cloud_delete_user_group"] = "/api/Project/DeleteUserGroup";
            map["cloud_rename_user_group"] = "/api/Project/RenameUserGroup";
            map["cloud_update_job_title"] = "/api/Project/UpdateJobTitle";
            map["cloud_allow_team_merge"] = "/api/Project/AllowTeamMerge";

            // ===== cloud_* 项目管理 =====
            map["cloud_get_projects"] = "/api/Project/GetProjects";
            map["cloud_open_project"] = "/api/Project/OpenProject";
            map["cloud_create_project"] = "/api/Project/CreateProject";
            map["cloud_delete_project"] = "/api/Project/DeleteProject";
            map["cloud_restore_projects"] = "/api/Project/RestoreProjects";
            map["cloud_copy_project"] = "/api/Project/DuplicateProject";
            map["cloud_share_project"] = "/api/Project/ShareProject";
            map["cloud_update_project"] = "/api/Project/UpdateProject";
            map["cloud_get_project_members"] = "/api/Project/GetProjectUsersWithPic";
            map["cloud_update_project_members"] = "/api/Project/UpdateProjectMembers";
            map["cloud_get_recycle_projects"] = "/api/Project/GetRecycleProjects";
            map["cloud_get_project_dto"] = "/api/Project/GetProjectDto";
            map["cloud_get_project_descendants"] = "/api/Project/GetProjectDescendants";
            map["cloud_update_project_version"] = "/api/Project/UpdateProjectVersion";
            map["cloud_create_demo"] = "/api/Project/CreateDemo";
            map["cloud_get_templates"] = "/api/Project/GetTemplates";
            map["cloud_get_team_payed_projects"] = "/api/Project/GetTeamPayedProjects";
            map["cloud_delete_project_from_server"] = "/api/Project/DeleteProjectFromServer";

            // ===== cloud_* 表格同步 =====
            map["cloud_push_table_quick"] = "/api/Project/PushTableQuick";
            map["cloud_push_table"] = "/api/Project/PushTable";
            map["cloud_pull_table"] = "/api/Project/PullTable";
            map["cloud_revert_table"] = "/api/Project/RevertTable";
            map["cloud_get_table_timeline"] = "/api/Project/GetTableTimeline";
            map["cloud_query_table_versions"] = "/api/Project/QueryTableVersions";
            map["cloud_get_table_revert_diff"] = "/api/Project/GetTableRevertDiff";
            map["cloud_get_table_columns"] = "/api/Project/GetTableColumns";

            // ===== cloud_* 文档同步 =====
            map["cloud_push_document"] = "/api/Project/PushDocumentQuick";
            map["cloud_pull_document"] = "/api/Project/PullDocument";
            map["cloud_revert_document"] = "/api/Project/RevertDocument";
            map["cloud_get_document_timeline"] = "/api/Project/GetDocumentTimeline";
            map["cloud_query_document_versions"] = "/api/Project/QueryDocumentVersions";
            map["cloud_get_document_revert_diff"] = "/api/Project/GetDocumentRevertDiff";

            // ===== cloud_* 文件/图片/PDF =====
            map["cloud_upload_file"] = "/api/Project/UploadFile";
            map["cloud_download_file"] = "/api/Project/DownloadFile";
            map["cloud_push_image"] = "/api/Project/PushImage";
            map["cloud_pull_image"] = "/api/Project/PullImage";
            map["cloud_push_pdf"] = "/api/Project/PushPdf";
            map["cloud_pull_pdf"] = "/api/Project/PullPdf";
            map["cloud_query_image_versions"] = "/api/Project/QueryImageVersions";
            map["cloud_query_pdf_versions"] = "/api/Project/QueryPdfVersions";

            // ===== cloud_* 任务模块 =====
            map["cloud_generate_task_id"] = "/api/ServerTask/GenerateTaskId";
            map["cloud_upload_task_input_file"] = "/api/ServerTask/UploadTaskInputFile";
            map["cloud_get_task_running_status"] = "/api/ServerTask/GetTaskRunningStatus";
            map["cloud_clear_task_cache_data"] = "/api/ServerTask/ClearTaskCacheData";

            // ===== cloud_* 数据字典模块 =====
            map["cloud_get_table_collect_dic"] = "/api/DataSource/TableCollectDic";
            map["cloud_get_cell_collect_dic"] = "/api/DataSource/CellCollectDic";
            map["cloud_get_ledger_validate_dic"] = "/api/DataSource/LedgerValidateDic";

            // ===== cloud_* License 模块 =====
            map["cloud_get_license"] = "/api/License/Status";
            map["cloud_create_license"] = "/api/License/Create";
            map["cloud_activate_license"] = "/api/License/Activate";
            map["cloud_renew_license"] = "/api/License/Renew";
            map["cloud_deactivate_license"] = "/api/License/Deactivate";

            // ===== admin_* 管理后台模块（26 个） =====
            map["admin_get_stats"] = "/api/Admin/Stats";
            map["admin_list_users"] = "/api/Admin/Users";
            map["admin_create_user"] = "/api/Admin/CreateUser";
            map["admin_update_user"] = "/api/Admin/UpdateUser";
            map["admin_reset_user_password"] = "/api/Admin/ResetUserPassword";
            map["admin_toggle_user_active"] = "/api/Admin/ToggleUserActive";
            map["admin_delete_user"] = "/api/Admin/DeleteUser";
            map["admin_list_licenses"] = "/api/Admin/Licenses";
            map["admin_create_license"] = "/api/Admin/CreateLicense";
            map["admin_renew_license"] = "/api/Admin/RenewLicense";
            map["admin_update_license_quota"] = "/api/Admin/UpdateLicenseQuota";
            map["admin_deactivate_machine"] = "/api/Admin/DeactivateMachine";
            map["admin_list_expiring_licenses"] = "/api/Admin/Licenses/Expiring";
            map["admin_import_activation_codes"] = "/api/Admin/ActivationCodes/Import";
            map["admin_generate_activation_codes"] = "/api/Admin/ActivationCodes/Generate";
            map["admin_list_activation_codes"] = "/api/Admin/ActivationCodes/List";
            map["admin_disable_activation_code"] = "/api/Admin/ActivationCodes/Disable";
            map["admin_delete_activation_code"] = "/api/Admin/ActivationCodes/Delete";
            map["admin_list_teams"] = "/api/Admin/Teams";
            map["admin_create_team"] = "/api/Admin/CreateTeam";
            map["admin_update_team"] = "/api/Admin/UpdateTeam";
            map["admin_list_team_members"] = "/api/Admin/Teams/{teamId}/Members";
            map["admin_remove_team_member"] = "/api/Admin/RemoveTeamMember";
            map["admin_list_invitations"] = "/api/Admin/Invitations";
            map["admin_revoke_invitation"] = "/api/Admin/RevokeInvitation";
            map["admin_change_password"] = "/api/Admin/ChangePassword";

            return map;
        }
    }
}
