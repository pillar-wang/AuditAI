﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
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
                    // 修复：原实现只写 stderr、不回任何响应，导致该 id 的请求永久无响应
                    // （客户端只能等自身超时）。改为回一个 JSON-RPC 内部错误响应
                    // （id 已在解析前失败时不带 id，符合 JSON-RPC 2.0 对解析错误的约定）。
                    Console.Error.WriteLine($"[MCP] 处理请求异常: {ex}");
                    try
                    {
                        string errorResponse = JsonRpcHandler.MakeInternalError(
                            AuditAI.McpServer.Protocol.JsonRpcHandler.TryExtractId(line),
                            "服务器内部错误: " + ex.Message);
                        if (errorResponse != null)
                        {
                            Console.WriteLine(errorResponse);
                            Console.Out.Flush();
                        }
                    }
                    catch (Exception nested)
                    {
                        Console.Error.WriteLine($"[MCP] 回写错误响应失败: {nested.Message}");
                    }
                }
            }

            Console.Error.WriteLine("[MCP] Server MCP_EXITING_NOW");
            // 强制退出进程：HttpClient 连接池、Timer 等后台线程会阻止进程正常退出
            // 确保所有 stdout 缓冲已刷新
            Console.Out.Flush();
            Environment.Exit(0);
        }
    }
}
