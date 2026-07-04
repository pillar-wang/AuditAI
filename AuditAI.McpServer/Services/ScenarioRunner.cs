﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuditAI.McpServer.State;
using Newtonsoft.Json;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 场景执行结果（Task 7.1）
    /// 包含场景名称、总体是否通过、耗时、步骤明细、汇总描述。
    /// </summary>
    public class ScenarioResult
    {
        public string ScenarioName { get; set; }
        public bool Passed { get; set; }
        public long DurationMs { get; set; }
        public List<ScenarioStep> Steps { get; set; } = new List<ScenarioStep>();
        public string Summary { get; set; }

        /// <summary>
        /// 序列化为结构化 JSON 字符串（缩进格式），供 MCP 工具直接返回。
        /// </summary>
        public string ToJson()
        {
            List<object> stepsData = new List<object>();
            if (Steps != null)
            {
                foreach (ScenarioStep s in Steps)
                {
                    stepsData.Add(new
                    {
                        name = s.Name,
                        action = s.Action,
                        passed = s.Passed,
                        detail = s.Detail,
                        durationMs = s.DurationMs,
                        error = s.Error
                    });
                }
            }
            return JsonConvert.SerializeObject(new
            {
                scenarioName = ScenarioName,
                passed = Passed,
                durationMs = DurationMs,
                steps = stepsData,
                summary = Summary
            }, Formatting.Indented);
        }
    }

    /// <summary>
    /// 场景单步执行记录。
    /// </summary>
    public class ScenarioStep
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public bool Passed { get; set; }
        public string Detail { get; set; }
        public long DurationMs { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// 场景编排引擎（Task 7.1）
    /// 提供 RunStep / RunSequence / RunParallel / ToMarkdown 四个静态方法，
    /// 供 ScenarioTools 中的复合场景工具调用，统一编排多步 CloudApi + 断言流程。
    /// 不抛异常：所有异常被捕获并记录到 ScenarioStep.Error。
    /// </summary>
    public static class ScenarioRunner
    {
        /// <summary>
        /// 执行单步并记录。stepFn 抛异常视为失败，捕获后填充 Error。
        /// stepFn 返回的 ScenarioStep 可只设置 Passed/Detail，Name/Action/DurationMs 由本方法补齐。
        /// </summary>
        public static ScenarioStep RunStep(string name, string action, Func<ScenarioStep> stepFn)
        {
            ScenarioStep step = new ScenarioStep { Name = name, Action = action ?? "" };
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                ScenarioStep result = stepFn != null ? stepFn() : null;
                sw.Stop();
                if (result != null)
                {
                    result.Name = name;
                    result.Action = action ?? "";
                    if (result.DurationMs == 0) result.DurationMs = sw.ElapsedMilliseconds;
                    RecordAssertion(result);
                    return result;
                }
                step.Passed = true;
                step.DurationMs = sw.ElapsedMilliseconds;
                RecordAssertion(step);
                return step;
            }
            catch (Exception ex)
            {
                sw.Stop();
                step.Passed = false;
                step.DurationMs = sw.ElapsedMilliseconds;
                step.Error = GetRootMessage(ex);
                RecordAssertion(step);
                return step;
            }
        }

        /// <summary>
        /// 执行多步序列。任一失败且 continueOnFailure=false 时停止后续步骤；
        /// continueOnFailure=true 时继续执行剩余步骤。
        /// 返回 ScenarioResult，Passed = 所有已完成步骤均通过且至少有一个步骤。
        /// </summary>
        public static ScenarioResult RunSequence(string scenarioName, List<Func<ScenarioStep>> steps, bool continueOnFailure = false)
        {
            ScenarioResult result = new ScenarioResult { ScenarioName = scenarioName };
            Stopwatch sw = Stopwatch.StartNew();
            if (steps != null)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    Func<ScenarioStep> stepFn = steps[i];
                    ScenarioStep step;
                    try
                    {
                        step = stepFn != null ? stepFn() : null;
                        if (step == null)
                        {
                            step = new ScenarioStep { Name = "step" + i, Action = "", Passed = true };
                        }
                    }
                    catch (Exception ex)
                    {
                        step = new ScenarioStep
                        {
                            Name = "step" + i,
                            Action = "",
                            Passed = false,
                            Error = GetRootMessage(ex)
                        };
                        RecordAssertion(step);
                    }
                    result.Steps.Add(step);
                    if (!step.Passed && !continueOnFailure) break;
                }
            }
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            result.Passed = ComputePassed(result.Steps);
            result.Summary = BuildSummary(result);
            return result;
        }

        /// <summary>
        /// 并行执行多步，等待全部完成或超时。
        /// 超时未完成的步骤标记为失败（Error="timeout after {timeoutMs}ms"）。
        /// 适用场景：并发写测试、双会话协作事件等待。
        /// </summary>
        public static ScenarioResult RunParallel(string scenarioName, List<Func<ScenarioStep>> steps, int timeoutMs = 30000)
        {
            ScenarioResult result = new ScenarioResult { ScenarioName = scenarioName };
            Stopwatch sw = Stopwatch.StartNew();
            if (steps != null && steps.Count > 0)
            {
                Task<ScenarioStep>[] tasks = new Task<ScenarioStep>[steps.Count];
                for (int i = 0; i < steps.Count; i++)
                {
                    int idx = i;
                    Func<ScenarioStep> fn = steps[i];
                    // 修复: 使用 LongRunning 创建专用线程,避免 Task.Run 使用线程池导致 sync-over-async 死锁。
                    // CloudApiClient 内部使用 SendAsync().Result 阻塞线程,如果占用线程池线程,
                    // Task.Delay 的定时器回调无法调度,导致 Task.WhenAny(...).Result 永久阻塞。
                    tasks[idx] = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            ScenarioStep s = fn != null ? fn() : null;
                            if (s == null) s = new ScenarioStep { Name = "step" + idx, Passed = true };
                            if (s.Name == null) s.Name = "step" + idx;
                            RecordAssertion(s);
                            return s;
                        }
                        catch (Exception ex)
                        {
                            var s = new ScenarioStep
                            {
                                Name = "step" + idx,
                                Passed = false,
                                Error = GetRootMessage(ex)
                            };
                            RecordAssertion(s);
                            return s;
                        }
                    }, TaskCreationOptions.LongRunning);
                }
                Task allTask = Task.WhenAll(tasks);
                Task timeoutTask = Task.Delay(Math.Max(0, timeoutMs));
                Task finished = Task.WhenAny(allTask, timeoutTask).Result;
                if (finished == timeoutTask)
                {
                    // 超时：已完成的结果收集，未完成的标记超时失败
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        if (tasks[i].IsCompleted && tasks[i].Status == TaskStatus.RanToCompletion)
                        {
                            result.Steps.Add(tasks[i].Result);
                        }
                        else
                        {
                            var timeoutStep = new ScenarioStep
                            {
                                Name = "step" + i,
                                Passed = false,
                                Error = "timeout after " + timeoutMs + "ms"
                            };
                            RecordAssertion(timeoutStep);
                            result.Steps.Add(timeoutStep);
                        }
                    }
                }
                else
                {
                    foreach (Task<ScenarioStep> t in tasks)
                    {
                        result.Steps.Add(t.Result);
                    }
                }
            }
            sw.Stop();
            result.DurationMs = sw.ElapsedMilliseconds;
            result.Passed = ComputePassed(result.Steps);
            result.Summary = BuildSummary(result);
            return result;
        }

        /// <summary>
        /// 生成 Markdown 报告：含场景汇总和步骤表格。
        /// </summary>
        public static string ToMarkdown(ScenarioResult result)
        {
            if (result == null) return "# Scenario Result\n\n(null)";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# " + (result.ScenarioName ?? "(unnamed)"));
            sb.AppendLine();
            sb.AppendLine("- **Passed**: " + (result.Passed ? "YES" : "NO"));
            sb.AppendLine("- **Duration**: " + result.DurationMs + " ms");
            sb.AppendLine("- **Summary**: " + (result.Summary ?? ""));
            sb.AppendLine();
            sb.AppendLine("| # | Step | Action | Passed | Duration | Detail | Error |");
            sb.AppendLine("|---|------|--------|--------|----------|--------|-------|");
            if (result.Steps != null)
            {
                for (int i = 0; i < result.Steps.Count; i++)
                {
                    ScenarioStep s = result.Steps[i];
                    sb.AppendLine(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "| {0} | {1} | {2} | {3} | {4}ms | {5} | {6} |",
                        i + 1,
                        EscapeMd(s.Name),
                        EscapeMd(s.Action),
                        s.Passed ? "PASS" : "FAIL",
                        s.DurationMs,
                        EscapeMd(s.Detail),
                        EscapeMd(s.Error)));
                }
            }
            return sb.ToString();
        }

        // ===== 内部辅助 =====

        /// <summary>
        /// 将场景步骤记录到 SessionState.AssertionResults，供 TestReportService.GenerateReport 汇总。
        /// 从 step.Detail 中尝试解析 actual/expected，无法解析时以整段 Detail 作为 Actual。
        /// </summary>
        private static void RecordAssertion(ScenarioStep step)
        {
            try
            {
                if (SessionState.AssertionResults == null) return;
                string actual = step.Detail ?? "";
                string expected = "";
                // 尝试从 detail 中解析 "actual=X" 和 "expected=Y"
                if (!string.IsNullOrEmpty(step.Detail))
                {
                    int aIdx = step.Detail.IndexOf("actual=");
                    int eIdx = step.Detail.IndexOf("expected=");
                    if (aIdx >= 0 && eIdx >= 0)
                    {
                        actual = ExtractToken(step.Detail, aIdx + 7);
                        expected = ExtractToken(step.Detail, eIdx + 9);
                    }
                }
                SessionState.AssertionResults.Add(new AssertionResult
                {
                    AssertionType = string.IsNullOrEmpty(step.Action) ? step.Name : step.Action,
                    Passed = step.Passed,
                    Actual = actual,
                    Expected = expected,
                    Message = step.Name + (string.IsNullOrEmpty(step.Error) ? "" : " | " + step.Error),
                    Timestamp = DateTime.Now
                });
            }
            catch
            {
                // 防御性：记录失败不影响场景执行
            }
        }

        /// <summary>
        /// 从 detail 字符串的指定位置提取 token 值（到下一个空格或字符串末尾）。
        /// </summary>
        private static string ExtractToken(string s, int startIndex)
        {
            if (string.IsNullOrEmpty(s) || startIndex < 0 || startIndex >= s.Length) return "";
            int end = s.IndexOf(' ', startIndex);
            if (end < 0) end = s.Length;
            return s.Substring(startIndex, end - startIndex);
        }

        private static bool ComputePassed(List<ScenarioStep> steps)
        {
            if (steps == null || steps.Count == 0) return false;
            foreach (ScenarioStep s in steps)
            {
                if (!s.Passed) return false;
            }
            return true;
        }

        private static string BuildSummary(ScenarioResult result)
        {
            int total = result.Steps != null ? result.Steps.Count : 0;
            int passed = 0;
            if (result.Steps != null)
            {
                foreach (ScenarioStep s in result.Steps) if (s.Passed) passed++;
            }
            return passed + "/" + total + " steps passed";
        }

        private static string EscapeMd(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
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
