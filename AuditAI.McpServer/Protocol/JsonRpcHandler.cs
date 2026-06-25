﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Protocol
{
    /// <summary>
    /// JSON-RPC 2.0 请求处理器
    /// 处理 MCP 协议方法：initialize、notifications/initialized、tools/list、tools/call
    /// </summary>
    public class JsonRpcHandler
    {
        /// <summary>
        /// 处理一行 JSON-RPC 请求，返回 JSON-RPC 响应字符串
        /// 通知（无 id 字段）返回 null，表示不发送响应
        /// </summary>
        public string HandleRequest(string rawLine)
        {
            // 1. 解析 JSON，失败返回 Parse Error（id 为 null）
            JObject request;
            try
            {
                request = JObject.Parse(rawLine);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[MCP] JSON 解析失败: {ex.Message}");
                return MakeError(null, JsonRpcError.ParseError);
            }

            // 2. 判断是通知（无 id 字段）还是请求（有 id 字段）
            JProperty idProperty = request.Property("id");
            bool isNotification = idProperty == null;
            JToken idToken = idProperty != null ? idProperty.Value : null;

            // 3. 获取 method
            string method = request["method"] != null ? request["method"].ToString() : null;

            if (string.IsNullOrEmpty(method))
            {
                // 缺少 method 字段，非法请求
                if (!isNotification)
                {
                    return MakeError(idToken, JsonRpcError.InvalidRequest);
                }
                return null;
            }

            // 4. 分发方法
            try
            {
                return DispatchMethod(method, request, idToken, isNotification);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[MCP] 方法处理异常: {ex}");
                if (!isNotification)
                {
                    return MakeError(idToken, JsonRpcError.InternalError);
                }
                return null;
            }
        }

        /// <summary>
        /// 分发到具体的方法处理器
        /// </summary>
        private string DispatchMethod(string method, JObject request, JToken idToken, bool isNotification)
        {
            switch (method)
            {
                case "initialize":
                    return HandleInitialize(request, idToken, isNotification);

                case "notifications/initialized":
                    // 这是通知，正常情况下无 id，不返回响应
                    if (isNotification)
                    {
                        return null;
                    }
                    // 防御性处理：若带有 id（不符合规范），返回空结果避免客户端挂起
                    return MakeSuccess(idToken, new JObject());

                case "tools/list":
                    return HandleToolsList(request, idToken, isNotification);

                case "tools/call":
                    return HandleToolsCall(request, idToken, isNotification);

                default:
                    // 未知方法
                    if (!isNotification)
                    {
                        return MakeError(idToken, JsonRpcError.MethodNotFound);
                    }
                    Console.Error.WriteLine($"[MCP] 收到未知通知方法: {method}");
                    return null;
            }
        }

        /// <summary>
        /// 处理 initialize 方法
        /// 返回协议版本、能力和服务器信息
        /// </summary>
        private string HandleInitialize(JObject request, JToken idToken, bool isNotification)
        {
            if (isNotification)
            {
                // initialize 应当是请求而非通知，防御性处理
                return null;
            }

            var result = new JObject
            {
                ["protocolVersion"] = "2025-11-25",
                ["capabilities"] = new JObject
                {
                    ["tools"] = new JObject()
                },
                ["serverInfo"] = new JObject
                {
                    ["name"] = "auditai-mcp",
                    ["version"] = "1.0.0"
                }
            };

            return MakeSuccess(idToken, result);
        }

        /// <summary>
        /// 处理 tools/list 方法
        /// 返回所有已注册工具的列表
        /// </summary>
        private string HandleToolsList(JObject request, JToken idToken, bool isNotification)
        {
            if (isNotification)
            {
                return null;
            }

            var toolsArray = new JArray();
            foreach (McpTool tool in ToolRegistry.GetAll())
            {
                toolsArray.Add(new JObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["inputSchema"] = tool.InputSchema ?? new JObject()
                });
            }

            var result = new JObject
            {
                ["tools"] = toolsArray
            };

            return MakeSuccess(idToken, result);
        }

        /// <summary>
        /// 处理 tools/call 方法
        /// 调用指定工具并返回结果
        /// </summary>
        private string HandleToolsCall(JObject request, JToken idToken, bool isNotification)
        {
            if (isNotification)
            {
                return null;
            }

            JToken paramsToken = request["params"];
            JObject paramsObj = paramsToken as JObject;
            if (paramsObj == null)
            {
                return MakeError(idToken, JsonRpcError.InvalidParams);
            }

            string toolName = paramsObj["name"] != null ? paramsObj["name"].ToString() : null;
            if (string.IsNullOrEmpty(toolName))
            {
                return MakeError(idToken, JsonRpcError.InvalidParams);
            }

            JObject arguments = paramsObj["arguments"] as JObject;
            if (arguments == null)
            {
                arguments = new JObject();
            }

            // 工具不存在，返回 isError 结果
            if (!ToolRegistry.Exists(toolName))
            {
                return MakeToolError(idToken, $"未知工具: {toolName}");
            }

            // 调用工具
            try
            {
                string toolResult = ToolRegistry.Call(toolName, arguments);
                return MakeToolSuccess(idToken, toolResult);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[MCP] 工具 {toolName} 调用异常: {ex}");
                return MakeToolError(idToken, $"工具调用失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 构造成功响应
        /// </summary>
        private string MakeSuccess(JToken idToken, JObject result)
        {
            var response = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = idToken ?? JValue.CreateNull(),
                ["result"] = result
            };
            return response.ToString(Formatting.None);
        }

        /// <summary>
        /// 构造错误响应
        /// </summary>
        private string MakeError(JToken idToken, int code)
        {
            var response = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = idToken ?? JValue.CreateNull(),
                ["error"] = new JObject
                {
                    ["code"] = code,
                    ["message"] = JsonRpcError.GetMessage(code)
                }
            };
            return response.ToString(Formatting.None);
        }

        /// <summary>
        /// 构造工具调用成功结果
        /// result.content = [{ type: "text", text: "..." }]
        /// </summary>
        private string MakeToolSuccess(JToken idToken, string text)
        {
            var result = new JObject
            {
                ["content"] = new JArray
                {
                    new JObject
                    {
                        ["type"] = "text",
                        ["text"] = text
                    }
                }
            };
            return MakeSuccess(idToken, result);
        }

        /// <summary>
        /// 构造工具调用失败结果
        /// result.isError = true, result.content = [{ type: "text", text: "..." }]
        /// </summary>
        private string MakeToolError(JToken idToken, string errorMessage)
        {
            var result = new JObject
            {
                ["isError"] = true,
                ["content"] = new JArray
                {
                    new JObject
                    {
                        ["type"] = "text",
                        ["text"] = errorMessage
                    }
                }
            };
            return MakeSuccess(idToken, result);
        }
    }
}
