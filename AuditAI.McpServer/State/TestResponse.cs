﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AuditAI.McpServer.State
{
    /// <summary>
    /// 测试响应捕获模型
    /// </summary>
    public class TestResponse
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string Body { get; set; }
        public long ElapsedMs { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }

        public string GetHeader(string name)
        {
            return Headers.TryGetValue(name, out var v) ? v : null;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                statusCode = StatusCode,
                headers = Headers,
                body = Body,
                elapsedMs = ElapsedMs,
                timestamp = Timestamp.ToString("o"),
                requestUrl = RequestUrl,
                requestMethod = RequestMethod
            }, Formatting.Indented);
        }
    }
}
