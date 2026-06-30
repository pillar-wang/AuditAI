﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Auditai.Model;

namespace AuditAI.McpServer.State
{
    /// <summary>
    /// MCP Server 会话状态
    /// 维护当前打开的项目和活跃的文档/表格上下文
    /// </summary>
    public class SessionState
    {
        private static readonly SessionState _current = new SessionState();

        /// <summary>当前会话状态（单例）</summary>
        public static SessionState Current => _current;

        /// <summary>当前打开的项目（null 表示未打开）</summary>
        public Project CurrentProject { get; private set; }

        /// <summary>当前项目文件路径</summary>
        public string CurrentProjectPath { get; private set; }

        /// <summary>当前文档节点 ID（null 表示未设置）</summary>
        public long? CurrentDocumentNodeId { get; set; }

        /// <summary>当前表格节点 ID（null 表示未设置）</summary>
        public long? CurrentTableNodeId { get; set; }

        /// <summary>当前账簿文件路径（null 表示未导入账簿）</summary>
        public string CurrentLedgerFilePath { get; private set; }

        /// <summary>是否已导入账簿</summary>
        public bool HasLedger => !string.IsNullOrEmpty(CurrentLedgerFilePath);

        /// <summary>采集任务状态字典（任务ID → 状态）</summary>
        private readonly Dictionary<string, CollectionTaskStatus> _collectionTasks = new Dictionary<string, CollectionTaskStatus>();

        /// <summary>是否已打开项目</summary>
        public bool HasProject => CurrentProject != null;

        /// <summary>
        /// 设置当前项目
        /// </summary>
        public void SetProject(Project project, string path)
        {
            CurrentProject = project;
            CurrentProjectPath = path;
            // 切换项目时重置上下文
            CurrentDocumentNodeId = null;
            CurrentTableNodeId = null;
            CurrentLedgerFilePath = null;
        }

        /// <summary>
        /// 关闭当前项目
        /// </summary>
        public void CloseProject()
        {
            // Project 类未实现 IDisposable，无需 Dispose
            CurrentProject = null;
            CurrentProjectPath = null;
            CurrentDocumentNodeId = null;
            CurrentTableNodeId = null;
            CurrentLedgerFilePath = null;
        }

        /// <summary>
        /// 设置当前账簿文件路径
        /// </summary>
        public void SetLedgerFilePath(string path)
        {
            CurrentLedgerFilePath = path;
        }

        /// <summary>
        /// 确保已打开项目，否则抛出异常
        /// </summary>
        public void EnsureProject()
        {
            if (CurrentProject == null)
                throw new InvalidOperationException("未打开项目，请先调用 open_project 工具");
        }

        /// <summary>
        /// 注册采集任务
        /// </summary>
        public void RegisterCollectionTask(string taskId, CollectionTaskStatus status)
        {
            _collectionTasks[taskId] = status;
        }

        /// <summary>
        /// 获取采集任务状态
        /// </summary>
        public CollectionTaskStatus GetCollectionTask(string taskId)
        {
            return _collectionTasks.TryGetValue(taskId, out var status) ? status : null;
        }

        /// <summary>
        /// 更新采集任务状态
        /// </summary>
        public void UpdateCollectionTask(string taskId, int progress, string currentStep, string error = null)
        {
            if (_collectionTasks.TryGetValue(taskId, out var status))
            {
                status.Progress = progress;
                status.CurrentStep = currentStep;
                status.Error = error;
                if (progress >= 100)
                    status.IsCompleted = true;
            }
        }

        // ===== 云端测试上下文 =====
        //
        // 说明：所有云端测试状态已迁移至 TestSession 实例中（CurrentSession）。
        // 此处保留静态属性仅为向后兼容，内部均委托到 CurrentSession 对应字段，
        // 确保每个会话拥有独立的状态空间，避免多会话/多线程并发时的竞争条件。
        //
        // 多会话访问通过 _sessionLock 进行同步，避免 Dictionary 并发写入异常。

        public static TestResponse LastResponse
        {
            get => CurrentSession.LastResponse;
            set => CurrentSession.LastResponse = value;
        }

        public static string CurrentAuthToken
        {
            get => CurrentSession.AuthToken;
            set => CurrentSession.AuthToken = value;
        }

        public static long CurrentUserId
        {
            get => CurrentSession.UserId;
            set => CurrentSession.UserId = value;
        }

        public static Guid? CurrentTeamId
        {
            get => CurrentSession.TeamId;
            set => CurrentSession.TeamId = value;
        }

        public static Guid? CurrentProjectId
        {
            get => CurrentSession.ProjectId;
            set => CurrentSession.ProjectId = value;
        }

        public static HttpClient CurrentHttpClient
        {
            get => CurrentSession.HttpClient;
            set => CurrentSession.HttpClient = value;
        }

        // 多会话隔离
        private static readonly object _sessionLock = new object();
        private static readonly Dictionary<string, TestSession> _testSessions = new Dictionary<string, TestSession>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 返回会话字典的快照（线程安全）。仅在需要遍历所有会话时使用；
        /// 不要通过返回值直接增删会话，请改用 GetOrCreateSession / RemoveSession。
        /// </summary>
        public static Dictionary<string, TestSession> TestSessions
        {
            get
            {
                lock (_sessionLock)
                {
                    return new Dictionary<string, TestSession>(_testSessions, StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        public static TestSession GetOrCreateSession(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName))
                sessionName = "main";

            lock (_sessionLock)
            {
                if (!_testSessions.TryGetValue(sessionName, out var session))
                {
                    session = new TestSession
                    {
                        SessionName = sessionName,
                        HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) }
                    };
                    _testSessions[sessionName] = session;
                }
                return session;
            }
        }

        /// <summary>
        /// 移除指定会话（Dispose HttpClient 并从字典删除）。线程安全。
        /// </summary>
        public static bool RemoveSession(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName))
                return false;

            lock (_sessionLock)
            {
                if (_testSessions.TryGetValue(sessionName, out var session))
                {
                    try { session.HttpClient?.Dispose(); } catch { /* 忽略 Dispose 异常 */ }
                    _testSessions.Remove(sessionName);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 原子地替换指定会话：若已存在则先 Dispose 旧 HttpClient，再用新会话覆盖。线程安全。
        /// 用于 BeginSession 场景（重置会话状态）。
        /// </summary>
        public static void ReplaceSession(string sessionName, TestSession session)
        {
            if (string.IsNullOrEmpty(sessionName))
                sessionName = "main";
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            lock (_sessionLock)
            {
                if (_testSessions.TryGetValue(sessionName, out var existing))
                {
                    try { existing.HttpClient?.Dispose(); } catch { /* 忽略 Dispose 异常 */ }
                }
                _testSessions[sessionName] = session;
            }
        }

        public static TestSession CurrentSession => GetOrCreateSession("main");

        // 测试断言结果收集（委托到 CurrentSession，确保会话隔离）
        public static List<AssertionResult> AssertionResults => CurrentSession.AssertionResults;
        public static void ClearAssertionResults() => CurrentSession.ClearAssertionResults();
    }

    /// <summary>
    /// 采集任务状态
    /// </summary>
    public class CollectionTaskStatus
    {
        public string TaskId { get; set; }
        public int Progress { get; set; }
        public string CurrentStep { get; set; }
        public string Error { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 测试断言结果
    /// </summary>
    public class AssertionResult
    {
        public string AssertionType { get; set; }
        public bool Passed { get; set; }
        public string Actual { get; set; }
        public string Expected { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
