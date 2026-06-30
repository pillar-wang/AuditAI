﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 自动修复闭环工具集（Task 9）
    /// 串接"读取问题清单 → 应用修复 → 构建验证 → 回归测试"闭环。
    /// 5 个工具：read_finding / list_findings / apply_fix / run_regression / mark_finding_fixed。
    /// 不抛异常：所有方法 try-catch，返回结构化 JSON。
    /// </summary>
    public static class AutoFixTools
    {
        // ===== 路径常量 =====

        /// <summary>首轮安全审查 findings 目录</summary>
        private const string FindingsDir = @"e:\lq\.trae\specs\cloud-features-audit-review\findings";

        /// <summary>findings 汇总文件（Markdown 表格）</summary>
        private static readonly string FindingsSummaryPath = Path.Combine(FindingsDir, "summary.md");

        /// <summary>E2E 自动化 spec 目录（fixes.log 存放处）</summary>
        private const string E2eSpecDir = @"e:\lq\.trae\specs\cloud-e2e-automation";

        /// <summary>修复记录日志（追加）</summary>
        private static readonly string FixesLogPath = Path.Combine(E2eSpecDir, "fixes.log");

        // ===== 内存缓存 =====

        /// <summary>findings 缓存：findingId → FindingDetail（首次访问时加载）</summary>
        private static Dictionary<string, FindingDetail> _findingCache;
        private static readonly object _cacheLock = new object();

        // =====================================================================
        // 注册入口
        // =====================================================================

        /// <summary>
        /// 注册所有自动修复工具（5 个）。
        /// </summary>
        public static void Register()
        {
            RegisterReadFinding();
            RegisterListFindings();
            RegisterApplyFix();
            RegisterRunRegression();
            RegisterMarkFindingFixed();
        }

        // =====================================================================
        // Finding 数据模型
        // =====================================================================

        /// <summary>单个安全问题详情</summary>
        private class FindingDetail
        {
            public string Id;
            public string Severity;     // Critical / High / Medium / Low
            public string Module;
            public string Title;
            public string Location;
            public string Description;
            public string FixSuggestion;
            public string Status;       // Open / Fixed
        }

        /// <summary>回归测试结果内部载体</summary>
        private class RegressionResult
        {
            public bool Passed;
            public long DurationMs;
            public string ReportJson;
            public List<object> Failures;
        }

        /// <summary>场景定义</summary>
        private struct ScenarioDef
        {
            public string Name;
            public string MethodName;
            public JObject Args;
        }

        // =====================================================================
        // SubTask 9.2: read_finding
        // =====================================================================

        private static void RegisterReadFinding()
        {
            ToolRegistry.Register("read_finding",
                "读取指定 ID 的安全问题详情。从 cloud-features-audit-review 的 findings 文档加载。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["findingId"] = new JObject { ["type"] = "string", ["description"] = "问题 ID（如 C-01、H-05、M-12）" }
                    },
                    ["required"] = new JArray { "findingId" }
                },
                (args) => ReadFindingImpl(args));
        }

        private static string ReadFindingImpl(JObject args)
        {
            try
            {
                if (args == null) args = new JObject();
                string findingId = args["findingId"] != null ? args["findingId"].ToString() : "";
                if (string.IsNullOrWhiteSpace(findingId))
                {
                    return JsonError("findingId is required");
                }
                findingId = findingId.Trim();

                var findings = EnsureFindingsLoaded();
                if (findings.ContainsKey(findingId))
                {
                    var f = findings[findingId];
                    return JsonConvert.SerializeObject(new
                    {
                        found = true,
                        id = f.Id,
                        severity = f.Severity,
                        module = f.Module,
                        title = f.Title,
                        location = f.Location,
                        description = f.Description,
                        fixSuggestion = f.FixSuggestion,
                        status = f.Status
                    }, Formatting.Indented);
                }
                return JsonConvert.SerializeObject(new
                {
                    found = false,
                    findingId = findingId,
                    message = "finding not found in findings document"
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonError("read_finding 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 9.3: list_findings
        // =====================================================================

        private static void RegisterListFindings()
        {
            ToolRegistry.Register("list_findings",
                "列出所有安全问题清单（含已修复/未修复状态）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["severity"] = new JObject { ["type"] = "string", ["description"] = "按严重级别过滤（Critical/High/Medium/Low/All，默认 All）" },
                        ["status"] = new JObject { ["type"] = "string", ["description"] = "按状态过滤（Open/Fixed/All，默认 All）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => ListFindingsImpl(args));
        }

        private static string ListFindingsImpl(JObject args)
        {
            try
            {
                if (args == null) args = new JObject();
                string severityFilter = args["severity"] != null ? args["severity"].ToString() : "All";
                string statusFilter = args["status"] != null ? args["status"].ToString() : "All";
                if (string.IsNullOrWhiteSpace(severityFilter)) severityFilter = "All";
                if (string.IsNullOrWhiteSpace(statusFilter)) statusFilter = "All";

                var findings = EnsureFindingsLoaded();
                var list = new List<object>();
                foreach (var f in findings.Values)
                {
                    if (!SeverityMatch(f.Severity, severityFilter)) continue;
                    if (!StatusMatch(f.Status, statusFilter)) continue;
                    list.Add(new
                    {
                        id = f.Id,
                        severity = f.Severity,
                        status = f.Status,
                        title = f.Title
                    });
                }
                return JsonConvert.SerializeObject(new
                {
                    total = list.Count,
                    findings = list
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonError("list_findings 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 9.4: apply_fix
        // =====================================================================

        private static void RegisterApplyFix()
        {
            ToolRegistry.Register("apply_fix",
                "应用修复并验证：记录修复到 fixes.log → build_server → run_regression。返回 {findingId, buildSuccess, regressionPassed, failures}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["findingId"] = new JObject { ["type"] = "string", ["description"] = "问题 ID" },
                        ["fixDescription"] = new JObject { ["type"] = "string", ["description"] = "修复描述（如：新增 TokenAuthMiddleware 并注册）" },
                        ["modifiedFiles"] = new JObject { ["type"] = "array", ["description"] = "本次修改的文件路径列表（用于记录）" },
                        ["suite"] = new JObject { ["type"] = "string", ["description"] = "回归测试套件（critical/all，默认 critical）" }
                    },
                    ["required"] = new JArray { "findingId", "fixDescription" }
                },
                (args) => ApplyFixImpl(args));
        }

        private static string ApplyFixImpl(JObject args)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (args == null) args = new JObject();
                string findingId = args["findingId"] != null ? args["findingId"].ToString() : "";
                string fixDescription = args["fixDescription"] != null ? args["fixDescription"].ToString() : "";
                string suite = args["suite"] != null ? args["suite"].ToString() : "critical";
                if (string.IsNullOrWhiteSpace(suite)) suite = "critical";

                if (string.IsNullOrWhiteSpace(findingId))
                    return JsonError("findingId is required");
                if (string.IsNullOrWhiteSpace(fixDescription))
                    return JsonError("fixDescription is required");

                // 收集 modifiedFiles
                var modifiedFiles = new List<string>();
                var mf = args["modifiedFiles"];
                if (mf is JArray)
                {
                    var arr = (JArray)mf;
                    foreach (var item in arr)
                    {
                        if (item != null) modifiedFiles.Add(item.ToString());
                    }
                }

                // 1. 记录到 fixes.log
                string filesJoined = modifiedFiles.Count > 0 ? string.Join(";", modifiedFiles) : "(none)";
                string safeDesc = fixDescription.Replace("|", "\\|");
                AppendFixesLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " | FIX | " + findingId + " | " + safeDesc + " | " + filesJoined);

                // 2. 调用 BuildServer
                var buildResult = ServerOpsService.BuildServer("Debug");
                bool buildSuccess = buildResult.Success;

                // 3. 构建成功则执行回归测试
                bool regressionPassed = false;
                string regressionReport = null;
                List<object> regressionFailures = new List<object>();
                long regressionMs = 0;

                if (buildSuccess)
                {
                    var regResult = RunRegressionInternal(suite);
                    regressionReport = regResult.ReportJson;
                    regressionPassed = regResult.Passed;
                    regressionMs = regResult.DurationMs;
                    regressionFailures = regResult.Failures;
                }
                else
                {
                    regressionReport = "{\"skipped\":true,\"reason\":\"build failed, regression skipped\"}";
                }

                sw.Stop();
                return JsonConvert.SerializeObject(new
                {
                    findingId = findingId,
                    fixDescription = fixDescription,
                    buildSuccess = buildSuccess,
                    buildOutput = Truncate(buildResult.Output, 2000),
                    buildError = Truncate(buildResult.Error, 500),
                    regressionPassed = regressionPassed,
                    regressionFailures = regressionFailures,
                    regressionDurationMs = regressionMs,
                    regressionReport = regressionReport,
                    durationMs = sw.ElapsedMilliseconds
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                sw.Stop();
                return JsonError("apply_fix 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 9.5: run_regression
        // =====================================================================

        private static void RegisterRunRegression()
        {
            ToolRegistry.Register("run_regression",
                "执行回归测试套件。suite=critical 执行核心场景（login/unauthorized/project_sync/concurrent_write），suite=all 执行全部 11 个场景。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["suite"] = new JObject { ["type"] = "string", ["description"] = "测试套件（critical/all，默认 critical）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => RunRegressionImpl(args));
        }

        private static string RunRegressionImpl(JObject args)
        {
            try
            {
                string suite = (args != null && args["suite"] != null) ? args["suite"].ToString() : "critical";
                if (string.IsNullOrWhiteSpace(suite)) suite = "critical";
                var result = RunRegressionInternal(suite);
                return result.ReportJson;
            }
            catch (Exception ex)
            {
                return JsonError("run_regression 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 9.6: mark_finding_fixed
        // =====================================================================

        private static void RegisterMarkFindingFixed()
        {
            ToolRegistry.Register("mark_finding_fixed",
                "标记指定问题为已修复，更新 findings 文件状态。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["findingId"] = new JObject { ["type"] = "string" },
                        ["notes"] = new JObject { ["type"] = "string", ["description"] = "修复备注（可选）" }
                    },
                    ["required"] = new JArray { "findingId" }
                },
                (args) => MarkFindingFixedImpl(args));
        }

        private static string MarkFindingFixedImpl(JObject args)
        {
            try
            {
                if (args == null) args = new JObject();
                string findingId = args["findingId"] != null ? args["findingId"].ToString() : "";
                string notes = args["notes"] != null ? args["notes"].ToString() : "";
                if (string.IsNullOrWhiteSpace(findingId))
                    return JsonError("findingId is required");
                findingId = findingId.Trim();

                // 追加到 fixes.log
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string safeNotes = notes.Replace("|", "\\|");
                AppendFixesLog(timestamp + " | FIXED | " + findingId + " | " + safeNotes);

                // 更新内存缓存状态
                lock (_cacheLock)
                {
                    if (_findingCache != null && _findingCache.ContainsKey(findingId))
                    {
                        _findingCache[findingId].Status = "Fixed";
                    }
                }

                return JsonConvert.SerializeObject(new
                {
                    findingId = findingId,
                    status = "Fixed",
                    markedAt = timestamp,
                    notes = notes
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return JsonError("mark_finding_fixed 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // Findings 加载与解析
        // =====================================================================

        /// <summary>
        /// 确保 findings 已加载到内存缓存（线程安全）。
        /// 加载流程：
        /// 1. 读取 findings/summary.md，解析 Critical/High/Medium/Low 四个表格
        /// 2. 读取 fixes.log 中的 FIXED 记录，覆盖 Status
        /// </summary>
        private static Dictionary<string, FindingDetail> EnsureFindingsLoaded()
        {
            lock (_cacheLock)
            {
                if (_findingCache != null) return _findingCache;
                _findingCache = new Dictionary<string, FindingDetail>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    if (File.Exists(FindingsSummaryPath))
                    {
                        string content = File.ReadAllText(FindingsSummaryPath, Encoding.UTF8);
                        ParseSummaryMd(content, _findingCache);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("[AutoFixTools] load findings failed: " + ex.Message);
                }
                ApplyFixedStatusFromLog(_findingCache);
                return _findingCache;
            }
        }

        /// <summary>
        /// 解析 summary.md 中的 Critical/High/Medium/Low 表格。
        /// 表格格式：| ID | 模块 | 问题 | 位置 | 修复建议 |
        /// 每个 section 前有 "### Critical 级" 等标题。
        /// </summary>
        private static void ParseSummaryMd(string content, Dictionary<string, FindingDetail> cache)
        {
            if (string.IsNullOrEmpty(content)) return;
            string currentSeverity = null;
            string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.StartsWith("### ", StringComparison.Ordinal))
                {
                    if (line.IndexOf("Critical", StringComparison.OrdinalIgnoreCase) >= 0) currentSeverity = "Critical";
                    else if (line.IndexOf("High", StringComparison.OrdinalIgnoreCase) >= 0) currentSeverity = "High";
                    else if (line.IndexOf("Medium", StringComparison.OrdinalIgnoreCase) >= 0) currentSeverity = "Medium";
                    else if (line.IndexOf("Low", StringComparison.OrdinalIgnoreCase) >= 0) currentSeverity = "Low";
                    else currentSeverity = null;
                    continue;
                }
                if (currentSeverity == null) continue;
                // 跳过表头/分隔行
                if (line.StartsWith("|---", StringComparison.Ordinal) || line.StartsWith("| ---", StringComparison.Ordinal)) continue;
                if (line.StartsWith("|ID", StringComparison.OrdinalIgnoreCase)) continue;
                if (!line.StartsWith("|", StringComparison.Ordinal)) continue;

                var cols = SplitMdTableRow(line);
                if (cols.Count < 5) continue;
                var id = cols[0].Trim();
                if (string.IsNullOrWhiteSpace(id)) continue;
                // 验证 ID 形如 C-01 / H-05 / M-12 / L-10
                if (!Regex.IsMatch(id, "^[CHML]-\\d+$", RegexOptions.IgnoreCase)) continue;

                var f = new FindingDetail
                {
                    Id = id,
                    Severity = currentSeverity,
                    Module = cols[1].Trim(),
                    Title = cols[2].Trim(),
                    Location = cols[3].Trim(),
                    Description = cols[2].Trim(),
                    FixSuggestion = cols[4].Trim(),
                    Status = "Open"
                };
                cache[id] = f;
            }
        }

        /// <summary>
        /// 切分 Markdown 表格行（按 | 分隔，去除首尾空单元格）。
        /// 处理转义的 \|。
        /// </summary>
        private static List<string> SplitMdTableRow(string line)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(line)) return result;
            string s = line.Trim();
            if (s.StartsWith("|")) s = s.Substring(1);
            if (s.EndsWith("|")) s = s.Substring(0, s.Length - 1);
            // 按未转义的 | 分割：先用占位符保护 \|
            string placeholder = "\x01";
            s = s.Replace("\\|", placeholder);
            var parts = s.Split('|');
            foreach (var p in parts) result.Add(p.Replace(placeholder, "|"));
            return result;
        }

        /// <summary>
        /// 从 fixes.log 读取 FIXED 记录，更新 finding 的 Status。
        /// 行格式：{timestamp} | FIXED | {findingId} | {notes}
        /// </summary>
        private static void ApplyFixedStatusFromLog(Dictionary<string, FindingDetail> cache)
        {
            try
            {
                if (!File.Exists(FixesLogPath)) return;
                var lines = File.ReadAllLines(FixesLogPath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { " | " }, StringSplitOptions.None);
                    if (parts.Length < 4) continue;
                    if (!string.Equals(parts[1].Trim(), "FIXED", StringComparison.OrdinalIgnoreCase)) continue;
                    var id = parts[2].Trim();
                    if (cache.ContainsKey(id))
                    {
                        cache[id].Status = "Fixed";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[AutoFixTools] apply fixed status failed: " + ex.Message);
            }
        }

        // =====================================================================
        // 回归测试执行
        // =====================================================================

        /// <summary>
        /// 执行回归测试套件。
        /// critical: login / unauthorized_access / project_sync / concurrent_write（4 个核心场景）
        /// all: 全部 11 个场景
        /// 通过反射调用 ScenarioTools 的 private static impl 方法，复用既有场景逻辑。
        /// </summary>
        private static RegressionResult RunRegressionInternal(string suite)
        {
            var sw = Stopwatch.StartNew();
            var result = new RegressionResult
            {
                Failures = new List<object>()
            };
            try
            {
                var scenarios = GetScenarioList(suite);
                var scenarioReports = new List<object>();
                int total = scenarios.Count;
                int passed = 0;
                int failed = 0;

                foreach (var s in scenarios)
                {
                    var ssw = Stopwatch.StartNew();
                    bool ok = false;
                    string errMsg = "";
                    try
                    {
                        string reportJson = InvokeScenarioImpl(s.MethodName, s.Args);
                        ok = ParseScenarioPassed(reportJson);
                        if (!ok) errMsg = ParseScenarioError(reportJson);
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        errMsg = GetRootMessage(ex);
                    }
                    ssw.Stop();
                    if (ok) passed++;
                    else
                    {
                        failed++;
                        result.Failures.Add(new
                        {
                            name = s.Name,
                            error = Truncate(errMsg, 500)
                        });
                    }
                    scenarioReports.Add(new
                    {
                        name = s.Name,
                        passed = ok,
                        durationMs = ssw.ElapsedMilliseconds,
                        error = Truncate(errMsg, 500)
                    });
                }

                sw.Stop();
                result.Passed = (failed == 0 && total > 0);
                result.DurationMs = sw.ElapsedMilliseconds;
                result.ReportJson = JsonConvert.SerializeObject(new
                {
                    suite = suite,
                    total = total,
                    passed = passed,
                    failed = failed,
                    durationMs = sw.ElapsedMilliseconds,
                    scenarios = scenarioReports
                }, Formatting.Indented);
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.Passed = false;
                result.DurationMs = sw.ElapsedMilliseconds;
                result.ReportJson = JsonError("run_regression internal 异常: " + ex.Message);
                return result;
            }
        }

        /// <summary>
        /// 根据 suite 返回要执行的场景列表。
        /// critical: login / unauthorized_access / project_sync / concurrent_write（4 个）
        /// all: 全部 11 个
        /// </summary>
        private static List<ScenarioDef> GetScenarioList(string suite)
        {
            var list = new List<ScenarioDef>();
            var emptyArgs = new JObject();
            if (string.Equals(suite, "critical", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(new ScenarioDef { Name = "login_flow", MethodName = "RunLoginFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "unauthorized_access_flow", MethodName = "RunUnauthorizedAccessFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "project_sync_flow", MethodName = "RunProjectSyncFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "concurrent_write_flow", MethodName = "RunConcurrentWriteFlowImpl", Args = emptyArgs });
            }
            else
            {
                // all：全部 11 个场景
                list.Add(new ScenarioDef { Name = "login_flow", MethodName = "RunLoginFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "user_team_flow", MethodName = "RunUserTeamFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "project_crud_flow", MethodName = "RunProjectCrudFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "project_sync_flow", MethodName = "RunProjectSyncFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "document_sync_flow", MethodName = "RunDocumentSyncFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "collaboration_flow", MethodName = "RunCollaborationFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "license_enforcement_flow", MethodName = "RunLicenseEnforcementFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "quota_enforcement_flow", MethodName = "RunQuotaEnforcementFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "concurrent_write_flow", MethodName = "RunConcurrentWriteFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "file_upload_download_flow", MethodName = "RunFileUploadDownloadFlowImpl", Args = emptyArgs });
                list.Add(new ScenarioDef { Name = "unauthorized_access_flow", MethodName = "RunUnauthorizedAccessFlowImpl", Args = emptyArgs });
            }
            return list;
        }

        /// <summary>
        /// 通过反射调用 ScenarioTools 的 private static impl 方法。
        /// 方法签名：private static string XxxImpl(JObject args)
        /// </summary>
        private static string InvokeScenarioImpl(string methodName, JObject args)
        {
            Type t = typeof(ScenarioTools);
            MethodInfo mi = t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (mi == null)
                throw new InvalidOperationException("ScenarioTools." + methodName + " not found");
            object result = mi.Invoke(null, new object[] { args });
            return result as string;
        }

        /// <summary>
        /// 从 ScenarioResult JSON 中解析 passed 字段。
        /// </summary>
        private static bool ParseScenarioPassed(string json)
        {
            if (string.IsNullOrEmpty(json)) return false;
            try
            {
                var obj = JObject.Parse(json);
                var p = obj["passed"];
                return p != null && p.Value<bool>();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从 ScenarioResult JSON 中提取错误信息（取 summary + 失败步骤的 error）。
        /// </summary>
        private static string ParseScenarioError(string json)
        {
            if (string.IsNullOrEmpty(json)) return "(empty report)";
            try
            {
                var obj = JObject.Parse(json);
                var sb = new StringBuilder();
                var summary = obj["summary"];
                if (summary != null) sb.Append(summary.ToString());
                var steps = obj["steps"] as JArray;
                if (steps != null)
                {
                    foreach (var s in steps)
                    {
                        var passed = s["passed"];
                        if (passed != null && !passed.Value<bool>())
                        {
                            var name = s["name"];
                            var err = s["error"];
                            if (err != null && !string.IsNullOrWhiteSpace(err.ToString()))
                            {
                                if (sb.Length > 0) sb.Append("; ");
                                sb.Append("[").Append(name != null ? name.ToString() : "?").Append("] ").Append(err);
                            }
                        }
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return "(unparseable report)";
            }
        }

        // =====================================================================
        // 辅助方法
        // =====================================================================

        private static bool SeverityMatch(string actual, string filter)
        {
            if (string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase)) return true;
            return string.Equals(actual, filter, StringComparison.OrdinalIgnoreCase);
        }

        private static bool StatusMatch(string actual, string filter)
        {
            if (string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase)) return true;
            return string.Equals(actual, filter, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 追加一行到 fixes.log，目录不存在则创建。
        /// </summary>
        private static void AppendFixesLog(string line)
        {
            try
            {
                string dir = Path.GetDirectoryName(FixesLogPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.AppendAllText(FixesLogPath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[AutoFixTools] append fixes.log failed: " + ex.Message);
            }
        }

        private static string Truncate(string s, int maxLen)
        {
            if (s == null) return "";
            if (maxLen <= 0) maxLen = 500;
            if (s.Length <= maxLen) return s;
            return s.Substring(0, maxLen) + "...(truncated)";
        }

        private static string GetRootMessage(Exception ex)
        {
            if (ex == null) return "(null)";
            Exception inner = ex;
            while (inner.InnerException != null) inner = inner.InnerException;
            return inner.Message ?? ex.Message ?? "(no message)";
        }

        private static string JsonError(string message)
        {
            return JsonConvert.SerializeObject(new
            {
                error = message,
                success = false
            }, Formatting.Indented);
        }
    }
}
