﻿using System;
using System.Collections.Generic;
using System.Net.Http;

namespace AuditAI.McpServer.State
{
    /// <summary>
    /// 测试会话（支持多会话隔离）
    /// 所有云端测试状态（Token、UserId、HttpClient、LastResponse、AssertionResults 等）
    /// 均存放于会话实例中，避免多会话/多线程并发时的静态共享竞争。
    /// </summary>
    public class TestSession
    {
        public string SessionName { get; set; }

        // 懒初始化 HttpClient，确保每个会话拥有独立实例
        private HttpClient _httpClient;
        public HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                }
                return _httpClient;
            }
            set => _httpClient = value;
        }

        public string AuthToken { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public Guid? TeamId { get; set; }
        public Guid? ProjectId { get; set; }
        public TestResponse LastResponse { get; set; }

        // 每会话独立的断言结果集合
        private readonly List<AssertionResult> _assertionResults = new List<AssertionResult>();
        public List<AssertionResult> AssertionResults => _assertionResults;

        public void ClearAssertionResults()
        {
            _assertionResults.Clear();
        }
    }
}
