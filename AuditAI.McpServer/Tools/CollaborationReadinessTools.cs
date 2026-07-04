﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AuditAI.McpServer.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 协同就绪度评估工具集（Task 1）
    /// 通过源代码静态扫描评估 AuditAI 项目对云端协同特性的就绪程度，共 5 个工具：
    ///   - assess_signalr_subscription：扫描 MemberManager 事件订阅情况
    ///   - assess_push_pull_integration：扫描 Save / Sync 方法是否调用 Syncer.Push/Pull
    ///   - assess_conflict_resolution：扫描 PushResult.OutOfDate 引用与冲突处理
    ///   - assess_offline_resilience：扫描离线队列、网络监听、模式切换、状态指示
    ///   - generate_collaboration_readiness_report：汇总评估，按权重计算就绪度总分并输出 Markdown 报告
    /// 所有工具均不抛异常，返回 JSON 字符串。
    /// 报告输出目录：e:\lq\.trae\specs\cloud-collaboration-readiness\reports\
    /// </summary>
    public static class CollaborationReadinessTools
    {
        // AuditAI 主项目根目录（用于源代码扫描）
        private static readonly string AuditAiRoot = Environment.GetEnvironmentVariable("AUDITAI_ROOT")
            ?? @"e:\lq\AuditAI";
        private static readonly string MemberManagerPath = Environment.GetEnvironmentVariable("AUDITAI_MEMBER_MANAGER_PATH")
            ?? @"e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\MemberManager.cs";
        private static readonly string ReportsDir = Environment.GetEnvironmentVariable("AUDITAI_COLLAB_REPORTS_DIR")
            ?? @"e:\lq\.trae\specs\cloud-collaboration-readiness\reports";

        /// <summary>
        /// 注册所有协同就绪度评估工具（5 个）。
        /// </summary>
        public static void Register()
        {
            RegisterAssessSignalRSubscription();      // SubTask 1.2
            RegisterAssessPushPullIntegration();      // SubTask 1.3
            RegisterAssessConflictResolution();       // SubTask 1.4
            RegisterAssessOfflineResilience();        // SubTask 1.5
            RegisterGenerateCollaborationReadinessReport(); // SubTask 1.6
        }

        // =====================================================================
        // 通用辅助
        // =====================================================================

        private static string SafeReadAllText(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch { return null; }
        }

        private static List<string> EnumerateCsFiles(string root)
        {
            var result = new List<string>();
            try
            {
                if (!Directory.Exists(root)) return result;
                string[] files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);
                if (files != null) result.AddRange(files);
            }
            catch { /* swallow */ }
            return result;
        }

        private static string SafeFileName(string fullPath)
        {
            try { return Path.GetFileName(fullPath); }
            catch { return fullPath; }
        }

        private static string ExtractLine(string content, int lineNumber)
        {
            if (string.IsNullOrEmpty(content) || lineNumber <= 0) return "";
            string[] lines = content.Split('\n');
            if (lineNumber > lines.Length) return "";
            string v = lines[lineNumber - 1];
            return v == null ? "" : v.Trim();
        }

        // =====================================================================
        // SubTask 1.2: assess_signalr_subscription
        // 扫描 MemberManager.cs 中所有 public event 声明，并搜索全工程中
        // `EventName +=` 订阅模式，统计每个事件的订阅文件列表。
        // =====================================================================
        private static void RegisterAssessSignalRSubscription()
        {
            ToolRegistry.Register("assess_signalr_subscription",
                "扫描 MemberManager 类的所有 .NET 事件，并通过源代码搜索统计每个事件的订阅者。" +
                "返回 {events, totalEvents, consumedEvents, unconsumedEvents}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sourceRoot"] = new JObject { ["type"] = "string", ["description"] = "源代码扫描根目录（默认 e:\\lq\\AuditAI）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => AssessSignalRSubscriptionImpl(args));
        }

        private static string AssessSignalRSubscriptionImpl(JObject args)
        {
            try
            {
                string root = args != null && args["sourceRoot"] != null ? args["sourceRoot"].ToString() : AuditAiRoot;
                if (string.IsNullOrEmpty(root)) root = AuditAiRoot;

                // 1) 提取 MemberManager.cs 中所有 public event 声明
                string memberSrc = SafeReadAllText(MemberManagerPath);
                var eventNames = new List<string>();
                if (!string.IsNullOrEmpty(memberSrc))
                {
                    // 形如: public event EventHandler<long> TableCellChanged;
                    // 形如: public event EventHandler<Tuple<string,string,string,string>> OpenTicketNavTreeNodeChanged;
                    var evRegex = new Regex(@"public\s+event\s+[\w<>,\s\.\?]+?\s+(\w+)\s*;", RegexOptions.Compiled);
                    foreach (Match m in evRegex.Matches(memberSrc))
                    {
                        if (m.Groups.Count > 1 && !eventNames.Contains(m.Groups[1].Value))
                        {
                            eventNames.Add(m.Groups[1].Value);
                        }
                    }
                }

                // 2) 对每个事件，在全工程搜索 `<EventName> +=` 模式
                var csFiles = EnumerateCsFiles(root);
                var events = new JArray();
                int consumed = 0;
                var unconsumed = new JArray();
                foreach (string ev in eventNames)
                {
                    var subscribers = new JArray();
                    var subscriberSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    // 匹配 .EventName += 或 EventName +=（事件可能是 instance.EventName += 形式）
                    string patternEv = ev;
                    foreach (string file in csFiles)
                    {
                        // 跳过 MemberManager.cs 自身（事件声明处不算订阅）
                        if (string.Equals(file, MemberManagerPath, StringComparison.OrdinalIgnoreCase)) continue;
                        string content = SafeReadAllText(file);
                        if (string.IsNullOrEmpty(content)) continue;
                        // 订阅模式：.EventName +=  或  EventName +=
                        var subRegex = new Regex(@"\b" + Regex.Escape(patternEv) + @"\s*\+=", RegexOptions.Compiled);
                        if (subRegex.IsMatch(content))
                        {
                            string fname = SafeFileName(file);
                            if (!subscriberSet.Contains(fname))
                            {
                                subscriberSet.Add(fname);
                                subscribers.Add(fname);
                            }
                        }
                    }
                    int count = subscribers.Count;
                    bool isConsumed = count > 0;
                    if (isConsumed) consumed++;
                    else unconsumed.Add(ev);
                    events.Add(new JObject
                    {
                        ["name"] = ev,
                        ["subscriberCount"] = count,
                        ["subscribers"] = subscribers,
                        ["isConsumed"] = isConsumed
                    });
                }

                var payload = new JObject
                {
                    ["events"] = events,
                    ["totalEvents"] = eventNames.Count,
                    ["consumedEvents"] = consumed,
                    ["unconsumedEvents"] = unconsumed
                };
                return payload.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("assess_signalr_subscription", ex);
            }
        }

        // =====================================================================
        // SubTask 1.3: assess_push_pull_integration
        // 扫描 Table.Save / Document.Save / MainForm.SyncProjectImpl /
        // MainForm.SaveProjectImpl，检查是否调用 Syncer.Push/Pull 以及是否检查返回值。
        // =====================================================================
        private static void RegisterAssessPushPullIntegration()
        {
            ToolRegistry.Register("assess_push_pull_integration",
                "扫描 Table.Save / Document.Save / MainForm.SyncProjectImpl / MainForm.SaveProjectImpl 等方法，" +
                "判断是否调用 Syncer.Push / Syncer.Pull 以及是否检查返回值。返回 {methods}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => AssessPushPullIntegrationImpl(args));
        }

        private static string AssessPushPullIntegrationImpl(JObject args)
        {
            try
            {
                var targets = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(@"e:\lq\AuditAI\ProjectModel\Auditai.Model\Table.cs", "Save()"),
                    new KeyValuePair<string, string>(@"e:\lq\AuditAI\ProjectModel\Auditai.Model\Document.cs", "Save()"),
                    new KeyValuePair<string, string>(@"e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\MainForm.cs", "SyncProjectImpl"),
                    new KeyValuePair<string, string>(@"e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\MainForm.cs", "SaveProjectImpl"),
                };

                var methods = new JArray();
                foreach (var kv in targets)
                {
                    string filePath = kv.Key;
                    string methodName = kv.Value;
                    string content = SafeReadAllText(filePath);
                    if (string.IsNullOrEmpty(content))
                    {
                        methods.Add(new JObject
                        {
                            ["file"] = SafeFileName(filePath),
                            ["method"] = methodName,
                            ["callsPush"] = false,
                            ["callsPull"] = false,
                            ["checksResult"] = false,
                            ["autoSync"] = false,
                            ["note"] = "源文件不可读"
                        });
                        continue;
                    }

                    // 找到方法体范围：从方法签名行到下一个 } 同层级结束
                    // 简化：通过正则匹配 "void Save(" 或 "Task SaveProjectImpl(" 等
                    int startIdx = FindMethodStartIndex(content, methodName);
                    string methodBody = startIdx >= 0 ? ExtractMethodBody(content, startIdx) : "";

                    bool callsPush = !string.IsNullOrEmpty(methodBody) && Regex.IsMatch(methodBody, @"Syncer\.Push\s*\(", RegexOptions.Compiled);
                    bool callsPull = !string.IsNullOrEmpty(methodBody) && Regex.IsMatch(methodBody, @"Syncer\.Pull\s*\(", RegexOptions.Compiled);
                    // 是否检查返回值：PushResult 变量、== PushResult.、await Syncer.Push(...) 赋值给变量
                    bool checksResult = !string.IsNullOrEmpty(methodBody) && (
                        Regex.IsMatch(methodBody, @"PushResult\s+\w+\s*=", RegexOptions.Compiled) ||
                        Regex.IsMatch(methodBody, @"==\s*PushResult\.", RegexOptions.Compiled) ||
                        Regex.IsMatch(methodBody, @"!=\s*PushResult\.", RegexOptions.Compiled));
                    // 是否自动同步：在 Save 内调用 Push（视为 autoSync）
                    bool autoSync = callsPush;

                    methods.Add(new JObject
                    {
                        ["file"] = SafeFileName(filePath),
                        ["method"] = methodName,
                        ["callsPush"] = callsPush,
                        ["callsPull"] = callsPull,
                        ["checksResult"] = checksResult,
                        ["autoSync"] = autoSync
                    });
                }

                var payload = new JObject { ["methods"] = methods };
                return payload.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("assess_push_pull_integration", ex);
            }
        }

        // 查找方法签名起始位置（粗略）：匹配包含 methodName 的方法签名行
        private static int FindMethodStartIndex(string content, string methodName)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(methodName)) return -1;
            // 匹配形如: public void Save(  /  private async Task SaveProjectImpl(  /  public void Save<
            var regex = new Regex(
                @"(?:public|private|protected|internal)\s+(?:(?:async|static|override|virtual|sealed|new|abstract)\s+)*[\w<>\.\[\],\s\?]+\s+" +
                Regex.Escape(methodName) + @"\s*\(",
                RegexOptions.Compiled);
            Match m = regex.Match(content);
            return m.Success ? m.Index : -1;
        }

        // 从方法签名起始处提取方法体（包含签名到对应闭合大括号）
        private static string ExtractMethodBody(string content, int startIdx)
        {
            if (string.IsNullOrEmpty(content) || startIdx < 0 || startIdx >= content.Length) return "";
            int parenOpen = content.IndexOf('(', startIdx);
            if (parenOpen < 0) return "";
            // 找到 ) 闭合
            int depth = 1;
            int i = parenOpen + 1;
            while (i < content.Length && depth > 0)
            {
                char c = content[i];
                if (c == '(') depth++;
                else if (c == ')') depth--;
                i++;
            }
            if (depth != 0) return "";
            // 跳过空白找到 {
            int braceStart = content.IndexOf('{', i);
            if (braceStart < 0) return "";
            depth = 1;
            int j = braceStart + 1;
            while (j < content.Length && depth > 0)
            {
                char c = content[j];
                if (c == '{') depth++;
                else if (c == '}') depth--;
                // 跳过字符串/字符字面量，避免计数错误（简化版）
                if (c == '"')
                {
                    j++;
                    while (j < content.Length)
                    {
                        if (content[j] == '\\') { j += 2; continue; }
                        if (content[j] == '"') break;
                        j++;
                    }
                }
                j++;
            }
            if (depth != 0) return "";
            return content.Substring(startIdx, j - startIdx);
        }

        // =====================================================================
        // SubTask 1.4: assess_conflict_resolution
        // 搜索 PushResult.OutOfDate 在 e:\lq\AuditAI 下的所有引用点，
        // 对每个引用读取上下文判断是否处理冲突。
        // =====================================================================
        private static void RegisterAssessConflictResolution()
        {
            ToolRegistry.Register("assess_conflict_resolution",
                "搜索 PushResult.OutOfDate 在 e:\\lq\\AuditAI 下的所有引用点，" +
                "对每个引用读取上下文判断是否处理冲突。返回 {references, totalReferences, handledConflicts, silentIgnored}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sourceRoot"] = new JObject { ["type"] = "string", ["description"] = "源代码扫描根目录（默认 e:\\lq\\AuditAI）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => AssessConflictResolutionImpl(args));
        }

        private static string AssessConflictResolutionImpl(JObject args)
        {
            try
            {
                string root = args != null && args["sourceRoot"] != null ? args["sourceRoot"].ToString() : AuditAiRoot;
                if (string.IsNullOrEmpty(root)) root = AuditAiRoot;

                var csFiles = EnumerateCsFiles(root);
                var references = new JArray();
                int handled = 0;
                int silentIgnored = 0;

                foreach (string file in csFiles)
                {
                    // 跳过 PushResult.cs 自身定义文件（enum 定义不算引用）
                    if (string.Equals(SafeFileName(file), "PushResult.cs", StringComparison.OrdinalIgnoreCase)) continue;
                    string content = SafeReadAllText(file);
                    if (string.IsNullOrEmpty(content)) continue;

                    // 匹配 OutOfDate（PushResult.OutOfDate 或单独 OutOfDate 字符串）
                    var regex = new Regex(@"OutOfDate", RegexOptions.Compiled);
                    foreach (Match m in regex.Matches(content))
                    {
                        int line = 1;
                        for (int i = 0; i < m.Index; i++) if (content[i] == '\n') line++;
                        string ctx = ExtractLine(content, line);
                        // 判断是否处理冲突：
                        //  - 包含 return PushResult.OutOfDate（仅返回不处理）
                        //  - 包含 retry/Pull/重试 等关键字则视为处理
                        bool handlesConflict = false;
                        string retryStrategy = "none";
                        // 看上下文前后 5 行
                        int startLine = Math.Max(1, line - 5);
                        int endLine = line + 5;
                        var sbCtx = new StringBuilder();
                        string[] lines = content.Split('\n');
                        for (int li = startLine; li <= endLine && li <= lines.Length; li++)
                        {
                            sbCtx.AppendLine(lines[li - 1]);
                        }
                        string surrounding = sbCtx.ToString();
                        if (surrounding.IndexOf("retry", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            surrounding.IndexOf("Retry", StringComparison.Ordinal) >= 0 ||
                            surrounding.IndexOf("重试", StringComparison.Ordinal) >= 0)
                        {
                            handlesConflict = true;
                            retryStrategy = "retry";
                        }
                        else if (surrounding.IndexOf("Syncer.Pull", StringComparison.Ordinal) >= 0 &&
                                 surrounding.IndexOf("OutOfDate", StringComparison.Ordinal) >= 0)
                        {
                            // 在 OutOfDate 上下文中调用 Pull 视为合并处理
                            handlesConflict = true;
                            retryStrategy = "pull_after_conflict";
                        }
                        else if (surrounding.IndexOf("MessageBox", StringComparison.Ordinal) >= 0 &&
                                 surrounding.IndexOf("OutOfDate", StringComparison.Ordinal) >= 0)
                        {
                            handlesConflict = true;
                            retryStrategy = "user_prompt";
                        }

                        if (handlesConflict) handled++;
                        else silentIgnored++;

                        references.Add(new JObject
                        {
                            ["file"] = SafeFileName(file),
                            ["line"] = line,
                            ["context"] = ctx,
                            ["handlesConflict"] = handlesConflict,
                            ["retryStrategy"] = retryStrategy
                        });
                    }
                }

                var payload = new JObject
                {
                    ["references"] = references,
                    ["totalReferences"] = references.Count,
                    ["handledConflicts"] = handled,
                    ["silentIgnored"] = silentIgnored
                };
                return payload.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("assess_conflict_resolution", ex);
            }
        }

        // =====================================================================
        // SubTask 1.5: assess_offline_resilience
        // 扫描离线队列、网络监听、运行时模式切换、网络状态指示 4 个维度。
        // =====================================================================
        private static void RegisterAssessOfflineResilience()
        {
            ToolRegistry.Register("assess_offline_resilience",
                "扫描离线韧性 4 个维度：离线队列、网络监听、运行时模式切换、网络状态指示。返回 {offlineQueue, networkMonitor, runtimeModeSwitch, statusIndicator}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sourceRoot"] = new JObject { ["type"] = "string", ["description"] = "源代码扫描根目录（默认 e:\\lq\\AuditAI）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => AssessOfflineResilienceImpl(args));
        }

        private static string AssessOfflineResilienceImpl(JObject args)
        {
            try
            {
                string root = args != null && args["sourceRoot"] != null ? args["sourceRoot"].ToString() : AuditAiRoot;
                if (string.IsNullOrEmpty(root)) root = AuditAiRoot;

                // 1) 离线队列：搜索 OfflinePushQueue / PendingPushes / Enqueue（排除 XML 文档）
                bool offlineQueueExists = false;
                string offlineQueueDetail = "未找到 OfflinePushQueue 类";
                var csFiles = EnumerateCsFiles(root);
                foreach (string file in csFiles)
                {
                    if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) continue;
                    string content = SafeReadAllText(file);
                    if (string.IsNullOrEmpty(content)) continue;
                    // 跳过 XML 文档文件
                    if (content.IndexOf("<members", StringComparison.Ordinal) >= 0 &&
                        content.IndexOf("<doc>", StringComparison.Ordinal) >= 0) continue;
                    if (Regex.IsMatch(content, @"\bOfflinePushQueue\b", RegexOptions.Compiled) ||
                        Regex.IsMatch(content, @"\bPendingPushes\b", RegexOptions.Compiled))
                    {
                        offlineQueueExists = true;
                        offlineQueueDetail = "找到 " + SafeFileName(file) + " 包含离线队列关键字";
                        break;
                    }
                }

                // 2) 网络监听：搜索 NetworkMonitor / NetworkChange / NetworkAddressChanged
                bool networkMonitorExists = false;
                string networkMonitorDetail = "未找到 NetworkMonitor 类";
                foreach (string file in csFiles)
                {
                    string content = SafeReadAllText(file);
                    if (string.IsNullOrEmpty(content)) continue;
                    if (Regex.IsMatch(content, @"\bNetworkMonitor\b", RegexOptions.Compiled) ||
                        Regex.IsMatch(content, @"\bNetworkChange\b", RegexOptions.Compiled) ||
                        Regex.IsMatch(content, @"\bNetworkAddressChanged\b", RegexOptions.Compiled))
                    {
                        networkMonitorExists = true;
                        networkMonitorDetail = "找到 " + SafeFileName(file) + " 包含网络监听关键字";
                        break;
                    }
                }

                // 3) 模式切换：搜索 StorageRouter.SwitchMode / _isLocalMode 的可变性
                bool runtimeModeSwitchExists = false;
                string runtimeModeSwitchDetail = "_isLocalMode 启动时固定，无 SwitchMode 方法";
                var storageRouterPath = @"e:\lq\AuditAI\AuditAI.LocalDataStore\StorageRouter.cs";
                string srContent = SafeReadAllText(storageRouterPath);
                if (!string.IsNullOrEmpty(srContent))
                {
                    if (Regex.IsMatch(srContent, @"public\s+static\s+(async\s+)?\w+\s+SwitchMode", RegexOptions.Compiled) ||
                        Regex.IsMatch(srContent, @"\bSwitchMode\b", RegexOptions.Compiled))
                    {
                        runtimeModeSwitchExists = true;
                        runtimeModeSwitchDetail = "StorageRouter 存在 SwitchMode 方法";
                    }
                    else
                    {
                        // 检查 _isLocalMode 是否被运行时改变（除 Initialize 外）
                        var assignRegex = new Regex(@"_isLocalMode\s*=", RegexOptions.Compiled);
                        int assignCount = assignRegex.Matches(srContent).Count;
                        if (assignCount > 1)
                        {
                            runtimeModeSwitchExists = true;
                            runtimeModeSwitchDetail = "_isLocalMode 在运行时被重新赋值 " + assignCount + " 次";
                        }
                    }
                }
                if (!runtimeModeSwitchExists)
                {
                    // 全工程再搜一遍 SwitchMode
                    foreach (string file in csFiles)
                    {
                        string content = SafeReadAllText(file);
                        if (string.IsNullOrEmpty(content)) continue;
                        if (Regex.IsMatch(content, @"\bSwitchMode\b", RegexOptions.Compiled))
                        {
                            runtimeModeSwitchExists = true;
                            runtimeModeSwitchDetail = "找到 " + SafeFileName(file) + " 包含 SwitchMode 关键字";
                            break;
                        }
                    }
                }

                // 4) 网络状态指示：搜索 MainForm.Text 设置点、IsOnline 在标题栏的引用
                bool statusIndicatorExists = false;
                string statusIndicatorDetail = "标题栏未显示网络状态";
                var mainFormPath = @"e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\MainForm.cs";
                string mfContent = SafeReadAllText(mainFormPath);
                if (!string.IsNullOrEmpty(mfContent))
                {
                    // 查找 View.Text = / Text = 设置点附近是否含 IsOnline/NetworkStatus/online/offline
                    var setTextRegex = new Regex(@"(View\.Text|this\.Text|Text)\s*=\s*([^\n;]+);", RegexOptions.Compiled);
                    foreach (Match m in setTextRegex.Matches(mfContent))
                    {
                        string assigned = m.Groups[2].Value;
                        if (assigned.IndexOf("IsOnline", StringComparison.Ordinal) >= 0 ||
                            assigned.IndexOf("online", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            assigned.IndexOf("offline", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            assigned.IndexOf("NetworkStatus", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            statusIndicatorExists = true;
                            statusIndicatorDetail = "MainForm 标题栏显示网络状态";
                            break;
                        }
                    }
                }

                var payload = new JObject
                {
                    ["offlineQueue"] = new JObject
                    {
                        ["exists"] = offlineQueueExists,
                        ["details"] = offlineQueueDetail
                    },
                    ["networkMonitor"] = new JObject
                    {
                        ["exists"] = networkMonitorExists,
                        ["details"] = networkMonitorDetail
                    },
                    ["runtimeModeSwitch"] = new JObject
                    {
                        ["exists"] = runtimeModeSwitchExists,
                        ["details"] = runtimeModeSwitchDetail
                    },
                    ["statusIndicator"] = new JObject
                    {
                        ["exists"] = statusIndicatorExists,
                        ["details"] = statusIndicatorDetail
                    }
                };
                return payload.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("assess_offline_resilience", ex);
            }
        }

        // =====================================================================
        // SubTask 1.6: generate_collaboration_readiness_report
        // 汇总 4 个评估工具的结果，按 8 项能力权重计算协同就绪度总分（0-100）。
        // 输出 Markdown 报告到 reports 目录。
        // =====================================================================
        private static void RegisterGenerateCollaborationReadinessReport()
        {
            ToolRegistry.Register("generate_collaboration_readiness_report",
                "汇总协同就绪度 4 项评估结果，按 8 项能力权重计算总分（0-100），" +
                "生成 Markdown 报告并写入 reports 目录。返回 {totalScore, capabilities, reportPath}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => GenerateCollaborationReadinessReportImpl(args));
        }

        private static string GenerateCollaborationReadinessReportImpl(JObject args)
        {
            try
            {
                // 1) 调用 4 项评估，解析结果
                JObject signalR = ParseJsonSafe(AssessSignalRSubscriptionImpl(null));
                JObject pushPull = ParseJsonSafe(AssessPushPullIntegrationImpl(null));
                JObject conflict = ParseJsonSafe(AssessConflictResolutionImpl(null));
                JObject offline = ParseJsonSafe(AssessOfflineResilienceImpl(null));

                // 2) 计算各项能力分数（0-100）
                // (1) SignalR 通道集成（15%）：consumedEvents/totalEvents
                int totalEvents = signalR != null && signalR["totalEvents"] != null ? signalR["totalEvents"].Value<int>() : 0;
                int consumedEvents = signalR != null && signalR["consumedEvents"] != null ? signalR["consumedEvents"].Value<int>() : 0;
                double score1 = totalEvents > 0 ? (double)consumedEvents / totalEvents * 100.0 : 0.0;

                // (2) 表格协同（15%）：Save 自动 Push + Peer 事件自动 Pull + 冲突解决
                double tableScore = 0.0;
                bool tableAutoPush = false;
                bool tablePeerPull = false;
                if (pushPull != null && pushPull["methods"] is JArray ppMethods)
                {
                    foreach (JObject m in ppMethods)
                    {
                        string file = m["file"] != null ? m["file"].ToString() : "";
                        string method = m["method"] != null ? m["method"].ToString() : "";
                        bool callsPush = m["callsPush"] != null && m["callsPush"].Value<bool>();
                        bool callsPull = m["callsPull"] != null && m["callsPull"].Value<bool>();
                        if (file.IndexOf("Table.cs", StringComparison.OrdinalIgnoreCase) >= 0 && method == "Save()")
                        {
                            tableAutoPush = callsPush;
                        }
                        if (file.IndexOf("MainForm.cs", StringComparison.OrdinalIgnoreCase) >= 0 && method == "SyncProjectImpl")
                        {
                            if (callsPush) tableAutoPush = true;
                            if (callsPull) tablePeerPull = true;
                        }
                    }
                }
                if (tableAutoPush) tableScore += 50.0;
                if (tablePeerPull) tableScore += 50.0;
                double score2 = tableScore;

                // (3) 文档协同（15%）：DocParagraphChanged 订阅 + 文档自动 Push
                double docScore = 0.0;
                bool docSubscribed = false;
                if (signalR != null && signalR["events"] is JArray evArr)
                {
                    foreach (JObject ev in evArr)
                    {
                        string nm = ev["name"] != null ? ev["name"].ToString() : "";
                        if (nm == "DocParagraphChanged")
                        {
                            int cnt = ev["subscriberCount"] != null ? ev["subscriberCount"].Value<int>() : 0;
                            docSubscribed = cnt > 0;
                        }
                    }
                }
                bool docAutoPush = false;
                if (pushPull != null && pushPull["methods"] is JArray ppMethods2)
                {
                    foreach (JObject m in ppMethods2)
                    {
                        string file = m["file"] != null ? m["file"].ToString() : "";
                        string method = m["method"] != null ? m["method"].ToString() : "";
                        bool callsPush = m["callsPush"] != null && m["callsPush"].Value<bool>();
                        if (file.IndexOf("Document.cs", StringComparison.OrdinalIgnoreCase) >= 0 && method == "Save()")
                        {
                            docAutoPush = callsPush;
                        }
                        if (file.IndexOf("MainForm.cs", StringComparison.OrdinalIgnoreCase) >= 0 && method == "SyncProjectImpl" && callsPush)
                        {
                            docAutoPush = true;
                        }
                    }
                }
                if (docSubscribed) docScore += 50.0;
                if (docAutoPush) docScore += 50.0;
                double score3 = docScore;

                // (4) 新建项目广播（10%）：CreateProject 后 BroadcastToTeamUsers 调用
                // 通过源码扫描 CreateProject 方法是否调用 BroadcastToTeamUsers
                double score4 = 0.0;
                bool createProjectBroadcasts = CheckCreateProjectBroadcasts();
                if (createProjectBroadcasts) score4 = 100.0;

                // (5) 采数字典同步（10%）：DictionarySync 的版本化同步
                double score5 = 0.0;
                bool dictSyncExists = CheckDictionarySyncVersioned();
                if (dictSyncExists) score5 = 100.0;

                // (6) 冲突解决（15%）：OutOfDate 处理完整度
                int totalRefs = conflict != null && conflict["totalReferences"] != null ? conflict["totalReferences"].Value<int>() : 0;
                int handledRefs = conflict != null && conflict["handledConflicts"] != null ? conflict["handledConflicts"].Value<int>() : 0;
                double score6 = totalRefs > 0 ? (double)handledRefs / totalRefs * 100.0 : 0.0;

                // (7) 离线韧性（10%）：离线队列 + 网络监听
                double score7 = 0.0;
                if (offline != null)
                {
                    bool offlineQueue = offline["offlineQueue"] != null && offline["offlineQueue"]["exists"] != null && offline["offlineQueue"]["exists"].Value<bool>();
                    bool networkMon = offline["networkMonitor"] != null && offline["networkMonitor"]["exists"] != null && offline["networkMonitor"]["exists"].Value<bool>();
                    if (offlineQueue) score7 += 50.0;
                    if (networkMon) score7 += 50.0;
                }

                // (8) 网络状态指示（10%）：状态指示器
                double score8 = 0.0;
                if (offline != null)
                {
                    bool statusInd = offline["statusIndicator"] != null && offline["statusIndicator"]["exists"] != null && offline["statusIndicator"]["exists"].Value<bool>();
                    if (statusInd) score8 = 100.0;
                }

                // 3) 总分（按权重加权）
                double totalScore =
                    score1 * 0.15 +
                    score2 * 0.15 +
                    score3 * 0.15 +
                    score4 * 0.10 +
                    score5 * 0.10 +
                    score6 * 0.15 +
                    score7 * 0.10 +
                    score8 * 0.10;

                // 4) 构造能力数组
                var capabilities = new JArray
                {
                    new JObject { ["name"] = "SignalR 通道集成", ["score"] = Math.Round(score1, 1), ["weight"] = 0.15, ["weighted"] = Math.Round(score1 * 0.15, 1) },
                    new JObject { ["name"] = "表格协同", ["score"] = Math.Round(score2, 1), ["weight"] = 0.15, ["weighted"] = Math.Round(score2 * 0.15, 1) },
                    new JObject { ["name"] = "文档协同", ["score"] = Math.Round(score3, 1), ["weight"] = 0.15, ["weighted"] = Math.Round(score3 * 0.15, 1) },
                    new JObject { ["name"] = "新建项目广播", ["score"] = Math.Round(score4, 1), ["weight"] = 0.10, ["weighted"] = Math.Round(score4 * 0.10, 1) },
                    new JObject { ["name"] = "采数字典同步", ["score"] = Math.Round(score5, 1), ["weight"] = 0.10, ["weighted"] = Math.Round(score5 * 0.10, 1) },
                    new JObject { ["name"] = "冲突解决", ["score"] = Math.Round(score6, 1), ["weight"] = 0.15, ["weighted"] = Math.Round(score6 * 0.15, 1) },
                    new JObject { ["name"] = "离线韧性", ["score"] = Math.Round(score7, 1), ["weight"] = 0.10, ["weighted"] = Math.Round(score7 * 0.10, 1) },
                    new JObject { ["name"] = "网络状态指示", ["score"] = Math.Round(score8, 1), ["weight"] = 0.10, ["weighted"] = Math.Round(score8 * 0.10, 1) }
                };

                // 5) 生成 Markdown 报告
                string reportPath = WriteMarkdownReport(totalScore, capabilities, signalR, pushPull, conflict, offline);

                var payload = new JObject
                {
                    ["totalScore"] = Math.Round(totalScore, 1),
                    ["capabilities"] = capabilities,
                    ["reportPath"] = reportPath
                };
                return payload.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("generate_collaboration_readiness_report", ex);
            }
        }

        // 检查 CreateProject 后是否调用 BroadcastToTeamUsers
        private static bool CheckCreateProjectBroadcasts()
        {
            var formPath = @"e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\FormProjectManage.cs";
            string content = SafeReadAllText(formPath);
            if (string.IsNullOrEmpty(content)) return false;
            int startIdx = FindMethodStartIndex(content, "CreateProject");
            if (startIdx < 0) return false;
            string body = ExtractMethodBody(content, startIdx);
            if (string.IsNullOrEmpty(body)) return false;
            return Regex.IsMatch(body, @"BroadcastToTeamUsers", RegexOptions.Compiled);
        }

        // 检查 DictionarySync 是否有版本化同步
        private static bool CheckDictionarySyncVersioned()
        {
            var dictPath = @"e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\DictionarySync.cs";
            string content = SafeReadAllText(dictPath);
            if (string.IsNullOrEmpty(content)) return false;
            // 简化判断：检查是否包含 Version 字段、CheckXxxVersionAndUpdate 方法
            bool hasVersion = Regex.IsMatch(content, @"\.Version\b", RegexOptions.Compiled);
            bool hasCheckMethod = Regex.IsMatch(content, @"Check\w+VersionAndUpdate", RegexOptions.Compiled);
            return hasVersion && hasCheckMethod;
        }

        private static string WriteMarkdownReport(double totalScore, JArray capabilities,
            JObject signalR, JObject pushPull, JObject conflict, JObject offline)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 协同就绪度评估报告");
            sb.AppendLine();
            sb.AppendLine("**生成时间**: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("**评估目标**: AuditAI 项目云端协同特性就绪度");
            sb.AppendLine("**总分**: " + totalScore.ToString("F1") + " / 100");
            sb.AppendLine();

            // 雷达图数据
            sb.AppendLine("## 一、能力雷达图数据");
            sb.AppendLine();
            sb.AppendLine("| # | 能力 | 分数 | 权重 | 加权得分 |");
            sb.AppendLine("|---|------|------|------|----------|");
            int idx = 1;
            foreach (JObject c in capabilities)
            {
                sb.AppendLine(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "| {0} | {1} | {2} | {3} | {4} |",
                    idx++,
                    c["name"],
                    c["score"],
                    c["weight"],
                    c["weighted"]));
            }
            sb.AppendLine();

            // 详细评估项
            sb.AppendLine("## 二、详细评估项");
            sb.AppendLine();

            sb.AppendLine("### 2.1 SignalR 通道集成");
            sb.AppendLine();
            sb.AppendLine("- 总事件数: " + (signalR != null && signalR["totalEvents"] != null ? signalR["totalEvents"].ToString() : "0"));
            sb.AppendLine("- 已订阅事件数: " + (signalR != null && signalR["consumedEvents"] != null ? signalR["consumedEvents"].ToString() : "0"));
            sb.AppendLine("- 未订阅事件: " + (signalR != null && signalR["unconsumedEvents"] != null ? signalR["unconsumedEvents"].ToString() : "[]"));
            sb.AppendLine();

            sb.AppendLine("### 2.2 表格协同（Save 自动 Push + Peer 事件 Pull + 冲突解决）");
            sb.AppendLine();
            sb.AppendLine("| 文件 | 方法 | callsPush | callsPull | checksResult | autoSync |");
            sb.AppendLine("|------|------|-----------|-----------|--------------|----------|");
            if (pushPull != null && pushPull["methods"] is JArray arr)
            {
                foreach (JObject m in arr)
                {
                    sb.AppendLine(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "| {0} | {1} | {2} | {3} | {4} | {5} |",
                        m["file"], m["method"],
                        (bool)m["callsPush"] ? "✓" : "✗",
                        (bool)m["callsPull"] ? "✓" : "✗",
                        (bool)m["checksResult"] ? "✓" : "✗",
                        (bool)m["autoSync"] ? "✓" : "✗"));
                }
            }
            sb.AppendLine();

            sb.AppendLine("### 2.3 文档协同（DocParagraphChanged 订阅 + 文档自动 Push）");
            sb.AppendLine();
            bool docSubscribed = false;
            if (signalR != null && signalR["events"] is JArray evArr)
            {
                foreach (JObject ev in evArr)
                {
                    if (ev["name"] != null && ev["name"].ToString() == "DocParagraphChanged")
                    {
                        docSubscribed = ev["subscriberCount"] != null && ev["subscriberCount"].Value<int>() > 0;
                    }
                }
            }
            sb.AppendLine("- DocParagraphChanged 已订阅: " + (docSubscribed ? "✓" : "✗"));
            sb.AppendLine();

            sb.AppendLine("### 2.4 新建项目广播");
            sb.AppendLine();
            sb.AppendLine("- CreateProject 后调用 BroadcastToTeamUsers: " + (CheckCreateProjectBroadcasts() ? "✓" : "✗"));
            sb.AppendLine();

            sb.AppendLine("### 2.5 采数字典同步");
            sb.AppendLine();
            sb.AppendLine("- DictionarySync 版本化同步: " + (CheckDictionarySyncVersioned() ? "✓" : "✗"));
            sb.AppendLine();

            sb.AppendLine("### 2.6 冲突解决（OutOfDate 处理完整度）");
            sb.AppendLine();
            sb.AppendLine("- 总引用数: " + (conflict != null && conflict["totalReferences"] != null ? conflict["totalReferences"].ToString() : "0"));
            sb.AppendLine("- 已处理冲突数: " + (conflict != null && conflict["handledConflicts"] != null ? conflict["handledConflicts"].ToString() : "0"));
            sb.AppendLine("- 静默忽略数: " + (conflict != null && conflict["silentIgnored"] != null ? conflict["silentIgnored"].ToString() : "0"));
            sb.AppendLine();
            if (conflict != null && conflict["references"] is JArray refs)
            {
                sb.AppendLine("| 文件 | 行号 | 上下文 | 是否处理冲突 | 重试策略 |");
                sb.AppendLine("|------|------|--------|--------------|----------|");
                foreach (JObject r in refs)
                {
                    string ctx = r["context"] != null ? r["context"].ToString().Replace("|", "\\|") : "";
                    sb.AppendLine(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "| {0} | {1} | {2} | {3} | {4} |",
                        r["file"], r["line"], ctx,
                        (bool)r["handlesConflict"] ? "✓" : "✗",
                        r["retryStrategy"]));
                }
                sb.AppendLine();
            }

            sb.AppendLine("### 2.7 离线韧性");
            sb.AppendLine();
            if (offline != null)
            {
                sb.AppendLine("- 离线队列: " + (((bool)offline["offlineQueue"]["exists"]) ? "✓" : "✗") + " - " + offline["offlineQueue"]["details"]);
                sb.AppendLine("- 网络监听: " + (((bool)offline["networkMonitor"]["exists"]) ? "✓" : "✗") + " - " + offline["networkMonitor"]["details"]);
                sb.AppendLine("- 运行时模式切换: " + (((bool)offline["runtimeModeSwitch"]["exists"]) ? "✓" : "✗") + " - " + offline["runtimeModeSwitch"]["details"]);
                sb.AppendLine();
            }

            sb.AppendLine("### 2.8 网络状态指示");
            sb.AppendLine();
            if (offline != null)
            {
                sb.AppendLine("- 状态指示器: " + (((bool)offline["statusIndicator"]["exists"]) ? "✓" : "✗") + " - " + offline["statusIndicator"]["details"]);
                sb.AppendLine();
            }

            // 改进建议
            sb.AppendLine("## 三、改进建议");
            sb.AppendLine();
            foreach (JObject c in capabilities)
            {
                double s = c["score"] != null ? c["score"].Value<double>() : 0.0;
                if (s < 100.0)
                {
                    string nm = c["name"] != null ? c["name"].ToString() : "";
                    sb.AppendLine("- **" + nm + "**（当前 " + s.ToString("F1") + " 分）: " + GetImprovement(nm, s));
                }
            }
            sb.AppendLine();

            try
            {
                Directory.CreateDirectory(ReportsDir);
                string fileName = "readiness-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".md";
                string fullPath = Path.Combine(ReportsDir, fileName);
                File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
                return fullPath;
            }
            catch
            {
                return "";
            }
        }

        private static string GetImprovement(string capabilityName, double score)
        {
            if (score <= 0)
            {
                switch (capabilityName)
                {
                    case "SignalR 通道集成": return "需要为所有 MemberManager 事件添加订阅者，确保 Peer 事件能被 UI 层消费。";
                    case "表格协同": return "需要在 Table.Save 中调用 Syncer.Push 自动推送，并在 SyncProjectImpl 中订阅 TableCellChanged 自动 Pull。";
                    case "文档协同": return "需要订阅 DocParagraphChanged 事件，并在 Document.Save 中自动 Push。";
                    case "新建项目广播": return "需要在 CreateProject 完成后调用 SignalRClient.BroadcastToTeamUsers 通知团队成员。";
                    case "采数字典同步": return "需要实现 DictionarySync 的版本化增量同步机制。";
                    case "冲突解决": return "需要捕获 PushResult.OutOfDate 并实现重试或合并策略。";
                    case "离线韧性": return "需要实现 OfflinePushQueue 与 NetworkMonitor，断网时缓存推送请求。";
                    case "网络状态指示": return "需要在 MainForm 标题栏显示在线/离线状态。";
                    default: return "需要补充实现。";
                }
            }
            switch (capabilityName)
            {
                case "SignalR 通道集成": return "仍有部分事件未订阅，请检查 unconsumedEvents 列表并补充订阅。";
                case "表格协同": return "已部分实现自动 Push/Pull，建议补充冲突解决与返回值检查。";
                case "文档协同": return "已部分实现，建议补充 DocParagraphChanged 订阅或自动 Push。";
                case "冲突解决": return "部分 OutOfDate 引用未处理冲突，建议补充重试或用户提示。";
                case "离线韧性": return "已部分实现，建议补充网络监听或离线队列。";
                default: return "建议进一步完善以达到 100 分。";
            }
        }

        // =====================================================================
        // 内部辅助：JSON 解析与错误返回
        // =====================================================================

        private static JObject ParseJsonSafe(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JObject.Parse(json); }
            catch { return null; }
        }

        private static string ErrorJson(string toolName, Exception ex)
        {
            var obj = new JObject
            {
                ["error"] = true,
                ["tool"] = toolName,
                ["message"] = ex != null ? ex.Message : "unknown error",
                ["type"] = ex != null ? ex.GetType().Name : "Exception"
            };
            return obj.ToString(Formatting.Indented);
        }
    }
}
