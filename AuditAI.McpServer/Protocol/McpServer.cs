﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Protocol
{
    /// <summary>
    /// MCP Server 主循环
    /// 通过 stdio 实现 JSON-RPC 2.0 通信
    /// </summary>
    public static class McpServer
    {
        private static readonly JsonRpcHandler _handler = new JsonRpcHandler();

        /// <summary>
        /// 启动 MCP Server 主循环
        /// 从 stdin 读取 JSON-RPC 请求，向 stdout 写入响应
        /// 日志输出到 stderr
        /// </summary>
        public static void Run()
        {
            Console.Error.WriteLine("[MCP] AuditAI MCP Server 启动");

            string line;
            while ((line = Console.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    string response = _handler.HandleRequest(line);
                    if (response != null)
                    {
                        Console.WriteLine(response);
                        Console.Out.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[MCP] 处理请求异常: {ex}");
                }
            }

            Console.Error.WriteLine("[MCP] Server 主循环结束");
        }
    }
}
