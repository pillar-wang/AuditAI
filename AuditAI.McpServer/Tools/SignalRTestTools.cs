﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// SignalR Hub 实时协作测试工具集（Task 6）
    /// 提供多会话连接、Group 加入、Peer 事件收发、回调等待、断开连接共 5 个原子工具。
    /// 与服务端 AuditApiServer/Hubs/ChatHub.cs 对接：
    ///   - Hub URL: /ChatHub（app.MapHub&lt;ChatHub&gt;("/ChatHub")）
    ///   - 鉴权方式：QueryString 传递 userId + token（ChatHub.OnConnectedAsync）
    ///   - Hub 方法：Login(string userId, UserStateDto state)、UpLoadTableCellId(userId, cellId)、OpenProject(projectId) 等
    ///   - 回调事件：PeerTableCellChange / PeerParagraphChange / PeerLogin / PeerLogout / ProjectSynced 等
    /// 所有工具调用 try-catch，返回结构化 JSON，不抛异常给上层。
    /// </summary>
    public static class SignalRTestTools
    {
        /// <summary>
        /// 注册所有 SignalR 测试工具（5 个）。
        /// </summary>
        public static void Register()
        {
            RegisterConnectHub();         // SubTask 6.3
            RegisterHubLogin();           // SubTask 6.4
            RegisterSendPeerEvent();      // SubTask 6.5
            RegisterWaitPeerCallback();   // SubTask 6.6
            RegisterDisconnectHub();      // SubTask 6.7
        }

        // =====================================================================
        // SubTask 6.3: connect_hub
        // =====================================================================

        private static void RegisterConnectHub()
        {
            ToolRegistry.Register("connect_hub",
                "连接服务端 ChatHub。传入会话名称、用户 ID、Token，建立 SignalR WebSocket 连接。" +
                "Token 通过 QueryString 传递（与服务端 ChatHub.OnConnectedAsync 一致）。" +
                "返回 {connected, connectionId, sessionName}。如已存在同名会话会先断开旧连接。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（如 userA、userB，用于多会话隔离）" },
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "已登录的用户 ID（先调 cloud_login 获取）" },
                        ["token"] = new JObject { ["type"] = "string", ["description"] = "用户 Token（与 userId 配对，由 AuthService 生成）" }
                    },
                    ["required"] = new JArray { "sessionName", "userId", "token" }
                },
                (args) => ConnectHubImpl(args));
        }

        private static string ConnectHubImpl(JObject args)
        {
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
            long userId = args["userId"] != null ? args["userId"].Value<long>() : 0L;
            string token = args["token"] != null ? args["token"].ToString() : null;

            if (string.IsNullOrEmpty(sessionName))
                return ErrorJson("sessionName 不能为空");
            if (userId <= 0)
                return ErrorJson("userId 必须为正整数");
            if (string.IsNullOrEmpty(token))
                return ErrorJson("token 不能为空");

            try
            {
                // 如已存在同名会话，先 Dispose 旧客户端
                var existing = SignalRTestClient.RemoveClient(sessionName);
                if (existing != null)
                {
                    try { existing.Dispose(); } catch { /* ignore */ }
                }

                // 创建新客户端并连接（使用 CloudApiClient.ServerBaseUrl 作为服务端地址）
                var client = new SignalRTestClient(sessionName, CloudApiClient.ServerBaseUrl, userId, token);
                client.ConnectAsync().Wait();

                return JsonConvert.SerializeObject(new
                {
                    connected = client.IsConnected,
                    connectionId = client.ConnectionId,
                    sessionName = sessionName,
                    serverBaseUrl = CloudApiClient.ServerBaseUrl
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("连接 Hub 失败: " + GetRootMessage(ex));
            }
        }

        // =====================================================================
        // SubTask 6.4: hub_login
        // =====================================================================

        private static void RegisterHubLogin()
        {
            ToolRegistry.Register("hub_login",
                "调用 Hub 的 Login 方法加入项目/团队的 Group。服务端 ChatHub.Login(string userId, UserStateDto state)：" +
                "校验用户归属后加入 project_{projectId}/team_{teamId} 组，并通知同组其他用户 PeerLogin。" +
                "projectId 与 teamId 至少传一个；都不传则只注册在线状态，不加入任何组。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（connect_hub 时传入的名称）" },
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 ID（GUID 字符串，用于加入 project_{projectId} 组）" },
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 ID（GUID 字符串，用于加入 team_{teamId} 组）" }
                    },
                    ["required"] = new JArray { "sessionName" }
                },
                (args) => HubLoginImpl(args));
        }

        private static string HubLoginImpl(JObject args)
        {
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
            string projectId = args["projectId"] != null ? args["projectId"].ToString() : null;
            string teamId = args["teamId"] != null ? args["teamId"].ToString() : null;

            if (string.IsNullOrEmpty(sessionName))
                return ErrorJson("sessionName 不能为空");

            var client = SignalRTestClient.GetClient(sessionName);
            if (client == null)
                return ErrorJson("会话不存在: " + sessionName + "（请先调用 connect_hub）");
            if (!client.IsConnected)
                return ErrorJson("会话未连接: " + sessionName);

            try
            {
                bool ok = client.LoginAsync(projectId, teamId).Result;
                string groupName = null;
                if (!string.IsNullOrEmpty(projectId)) groupName = "project_" + projectId;
                else if (!string.IsNullOrEmpty(teamId)) groupName = "team_" + teamId;

                return JsonConvert.SerializeObject(new
                {
                    success = ok,
                    sessionName = sessionName,
                    projectId = projectId,
                    teamId = teamId,
                    groupName = groupName
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("Login 失败: " + GetRootMessage(ex));
            }
        }

        // =====================================================================
        // SubTask 6.5: hub_send_peer_event
        // =====================================================================

        private static void RegisterSendPeerEvent()
        {
            ToolRegistry.Register("hub_send_peer_event",
                "通过 Hub 调用指定方法触发 Peer 回调（如 UpLoadTableCellId 通知项目组其他客户端单元格变更）。" +
                "常用方法名：UpLoadTableCellId(userId, cellId) -> 触发 PeerTableCellChange；" +
                "UploadParagraphId(userId, paragraphId) -> 触发 PeerParagraphChange；" +
                "BroadcastToProjectUsers(message) -> 触发 ProjectBroadcast；" +
                "OpenTreeNode(projectId, nodeId) -> 触发 PeerOpensTreeNode；" +
                "SyncProject(projectId) -> 触发 ProjectSynced。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称" },
                        ["methodName"] = new JObject { ["type"] = "string", ["description"] = "Hub 方法名（如 UpLoadTableCellId）" },
                        ["args"] = new JObject { ["type"] = "array", ["description"] = "方法参数数组（按服务端方法签名顺序，JSON 值）" }
                    },
                    ["required"] = new JArray { "sessionName", "methodName" }
                },
                (args) => SendPeerEventImpl(args));
        }

        private static string SendPeerEventImpl(JObject args)
        {
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
            string methodName = args["methodName"] != null ? args["methodName"].ToString() : null;

            if (string.IsNullOrEmpty(sessionName))
                return ErrorJson("sessionName 不能为空");
            if (string.IsNullOrEmpty(methodName))
                return ErrorJson("methodName 不能为空");

            var client = SignalRTestClient.GetClient(sessionName);
            if (client == null)
                return ErrorJson("会话不存在: " + sessionName + "（请先调用 connect_hub）");
            if (!client.IsConnected)
                return ErrorJson("会话未连接: " + sessionName);

            // 将 args JArray 转换为 object[]，保留原始 JToken 类型（SignalR Newtonsoft 序列化能正确处理）
            object[] invokeArgs = ExtractArgsArray(args["args"]);

            try
            {
                client.SendPeerEventAsync(methodName, invokeArgs).Wait();
                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    sessionName = sessionName,
                    methodName = methodName,
                    argCount = invokeArgs.Length
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("调用 Hub 方法 " + methodName + " 失败: " + GetRootMessage(ex));
            }
        }

        // =====================================================================
        // SubTask 6.6: wait_peer_callback
        // =====================================================================

        private static void RegisterWaitPeerCallback()
        {
            ToolRegistry.Register("wait_peer_callback",
                "等待指定 Peer 回调事件触发。带超时（默认 5000ms）。" +
                "回调事件名（与 ChatHub.cs 中 Clients.GroupExcept(...).SendAsync 一致）：" +
                "PeerTableCellChange(fromId, projectId, cellId)、PeerParagraphChange(fromId, projectId, paragraphId)、" +
                "PeerLogin(peerId)、PeerLogout(peerId)、PeerOpensProject(peerId, projectId)、" +
                "PeerOpensTreeNode(fromId, projectId, nodeId)、PeerPushesTreeNode(fromId, nodeId)、" +
                "PeerFileSectionArrived(fromId, section)、PeerStateUpload(userId, state)、" +
                "ProjectBroadcast(fromId, message)、TeamBroadcast(fromId, message)、ProjectSynced(fromId, projectId)、" +
                "ReceiveFromUser(fromId, message)、PeerOpenTicketNavTreeNode(peerId, projectId, tableId, nodePath)。" +
                "返回 {received: true, payload: {event, args, timestamp}} 或 {received: false, reason: 'timeout'}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称" },
                        ["callbackName"] = new JObject { ["type"] = "string", ["description"] = "回调事件名（如 PeerTableCellChange）" },
                        ["timeoutMs"] = new JObject { ["type"] = "integer", ["description"] = "超时毫秒（默认 5000）" }
                    },
                    ["required"] = new JArray { "sessionName", "callbackName" }
                },
                (args) => WaitPeerCallbackImpl(args));
        }

        private static string WaitPeerCallbackImpl(JObject args)
        {
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
            string callbackName = args["callbackName"] != null ? args["callbackName"].ToString() : null;
            int timeoutMs = args["timeoutMs"] != null ? args["timeoutMs"].Value<int>() : 5000;

            if (string.IsNullOrEmpty(sessionName))
                return ErrorJson("sessionName 不能为空");
            if (string.IsNullOrEmpty(callbackName))
                return ErrorJson("callbackName 不能为空");

            var client = SignalRTestClient.GetClient(sessionName);
            if (client == null)
                return ErrorJson("会话不存在: " + sessionName + "（请先调用 connect_hub）");

            try
            {
                JObject result = client.WaitForCallbackAsync(callbackName, timeoutMs).Result;
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("WaitForCallback 失败: " + GetRootMessage(ex));
            }
        }

        // =====================================================================
        // SubTask 6.7: disconnect_hub
        // =====================================================================

        private static void RegisterDisconnectHub()
        {
            ToolRegistry.Register("disconnect_hub",
                "关闭指定会话的 Hub 连接，从静态注册表移除。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称" }
                    },
                    ["required"] = new JArray { "sessionName" }
                },
                (args) => DisconnectHubImpl(args));
        }

        private static string DisconnectHubImpl(JObject args)
        {
            string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
            if (string.IsNullOrEmpty(sessionName))
                return ErrorJson("sessionName 不能为空");

            var client = SignalRTestClient.GetClient(sessionName);
            if (client == null)
            {
                // 已不存在视为已断开
                return JsonConvert.SerializeObject(new
                {
                    disconnected = true,
                    sessionName = sessionName,
                    note = "session was not found (already disconnected)"
                }, Formatting.Indented);
            }

            try
            {
                client.DisconnectAsync().Wait();
                // 同步从静态表移除（DisconnectAsync 内部已移除，此处幂等）
                SignalRTestClient removed = SignalRTestClient.RemoveClient(sessionName);
                try { if (removed != null) removed.Dispose(); } catch { /* ignore */ }
                return JsonConvert.SerializeObject(new
                {
                    disconnected = true,
                    sessionName = sessionName
                }, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("Disconnect 失败: " + GetRootMessage(ex));
            }
        }

        // =====================================================================
        // 辅助方法
        // =====================================================================

        /// <summary>
        /// 构造标准错误 JSON 字符串。
        /// </summary>
        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new
            {
                success = false,
                error = message
            }, Formatting.Indented);
        }

        /// <summary>
        /// 从 args["args"] JArray 中提取参数数组。
        /// 保留原始 JToken/JValue 类型，SignalR Newtonsoft 序列化能正确处理。
        /// </summary>
        private static object[] ExtractArgsArray(JToken argsToken)
        {
            if (argsToken == null || argsToken.Type != JTokenType.Array)
                return new object[0];
            var arr = (JArray)argsToken;
            var result = new object[arr.Count];
            for (int i = 0; i < arr.Count; i++)
            {
                JToken t = arr[i];
                if (t == null || t.Type == JTokenType.Null)
                {
                    result[i] = null;
                }
                else if (t.Type == JTokenType.String)
                {
                    result[i] = t.ToString();
                }
                else if (t.Type == JTokenType.Integer)
                {
                    result[i] = t.Value<long>();
                }
                else if (t.Type == JTokenType.Float)
                {
                    result[i] = t.Value<double>();
                }
                else if (t.Type == JTokenType.Boolean)
                {
                    result[i] = t.Value<bool>();
                }
                else
                {
                    // JObject / JArray 等复杂类型保留原 JToken
                    result[i] = t;
                }
            }
            return result;
        }

        /// <summary>
        /// 提取异常的最内层 Message（解包 AggregateException / TargetInvocationException）。
        /// </summary>
        private static string GetRootMessage(Exception ex)
        {
            if (ex == null) return "(null)";
            Exception inner = ex;
            while (inner.InnerException != null) inner = inner.InnerException;
            return inner.Message ?? ex.Message ?? "(no message)";
        }
    }
}
