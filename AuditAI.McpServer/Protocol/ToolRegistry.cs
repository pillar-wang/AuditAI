﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Protocol
{
    /// <summary>
    /// MCP 工具定义
    /// </summary>
    public class McpTool
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public JObject InputSchema { get; set; }
        public Func<JObject, string> Handler { get; set; }
    }

    /// <summary>
    /// 工具注册中心
    /// </summary>
    public static class ToolRegistry
    {
        private static readonly Dictionary<string, McpTool> _tools = new Dictionary<string, McpTool>();

        /// <summary>注册工具</summary>
        public static void Register(McpTool tool)
        {
            _tools[tool.Name] = tool;
        }

        /// <summary>注册工具（便捷方法）</summary>
        public static void Register(string name, string description, JObject inputSchema, Func<JObject, string> handler)
        {
            Register(new McpTool
            {
                Name = name,
                Description = description,
                InputSchema = inputSchema,
                Handler = handler
            });
        }

        /// <summary>获取所有已注册工具</summary>
        public static IEnumerable<McpTool> GetAll()
        {
            return _tools.Values;
        }

        /// <summary>调用工具</summary>
        public static string Call(string name, JObject arguments)
        {
            if (!_tools.ContainsKey(name))
                throw new ArgumentException($"未知工具: {name}");
            return _tools[name].Handler(arguments);
        }

        /// <summary>检查工具是否存在</summary>
        public static bool Exists(string name)
        {
            return _tools.ContainsKey(name);
        }
    }
}
