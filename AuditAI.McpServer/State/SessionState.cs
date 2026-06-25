﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
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
}
