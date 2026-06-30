﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 原生 ASP.NET Core SignalR JSON 协议客户端（基于 ClientWebSocket）。
    ///
    /// 用途：net462 项目无法直接使用 Microsoft.AspNetCore.SignalR.Client（需 net472+/.NET Standard 2.0），
    /// 故手工实现 SignalR JSON 协议，与服务端 AuditApiServer/Hubs/ChatHub.cs 通信。
    ///
    /// 协议要点（参考 https://learn.microsoft.com/aspnet/core/signalr/protocol-spec）：
    ///   1. POST /HubName/negotiate?negotiateVersion=1 获取 {connectionId, connectionToken, availableTransports}
    ///   2. WebSocket 连接到 /HubName?id={connectionToken}&userId={uid}&token={tok}
    ///   3. 发送握手消息 `{"protocol":"json","version":1}\x1E`
    ///   4. 接收握手响应：成功为 `{}` + 0x1E；失败为 `{"error":"..."}` + 0x1E
    ///   5. 消息以 0x1E（RecordSeparator）分隔
    ///   6. 消息类型：
    ///      - type=1 Invocation: { type:1, target:"EventName", arguments:[...] }
    ///      - type=2 StreamItem: 不使用
    ///      - type=3 Completion: { type:3, invocationId:"<id>", result:..., error:... }
    ///      - type=6 Ping: { type:6 }，服务端约 15 秒一次，客户端必须忽略（不要求回复）
    ///      - type=7 Close: { type:7, error:"..." }
    ///
    /// 客户端 -> 服务端的消息类型：
    ///   - type=1 Invocation（带 invocationId 等待 Completion；不带则单向）
    ///   - type=6 Ping（每 15 秒发送一次保持连接）
    /// </summary>
    public sealed class CoreSignalRClient : IDisposable
    {
        private const string RecordSeparator = "\x1E";
        private const string HandshakeMessage = "{\"protocol\":\"json\",\"version\":1}\x1E";
        private static readonly TimeSpan PingInterval = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(15);

        private readonly string _serverBaseUrl;
        private readonly string _hubPath;       // 例如 "/ChatHub"
        private readonly string _userId;
        private readonly string _token;

        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private Task _receiveLoopTask;
        private Task _pingLoopTask;

        /// <summary>服务端 negotiate 返回的 connectionId（即 Hub 的 Context.ConnectionId）。</summary>
        public string ConnectionId { get; private set; }

        public bool IsConnected { get; private set; }

        // 客户端 -> 服务端调用：invocationId -> TCS（等待 Completion 消息）
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JObject>> _pendingInvocations =
            new ConcurrentDictionary<string, TaskCompletionSource<JObject>>(StringComparer.Ordinal);

        private int _nextInvocationId = 0;

        // 服务端 -> 客户端回调：target -> handlers
        private readonly ConcurrentDictionary<string, List<Action<JArray>>> _handlers =
            new ConcurrentDictionary<string, List<Action<JArray>>>(StringComparer.OrdinalIgnoreCase);

        // 接收缓冲（用于处理 0x1E 分隔符跨 WebSocket 帧的情况）
        private readonly StringBuilder _recvBuffer = new StringBuilder();
        private readonly object _recvBufferLock = new object();

        public CoreSignalRClient(string serverBaseUrl, string hubPath, string userId, string token)
        {
            if (string.IsNullOrEmpty(serverBaseUrl))
                throw new ArgumentException("serverBaseUrl 不能为空", "serverBaseUrl");
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId 不能为空", "userId");
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("token 不能为空", "token");

            _serverBaseUrl = serverBaseUrl.TrimEnd('/');
            _hubPath = hubPath.StartsWith("/") ? hubPath : "/" + hubPath;
            _userId = userId;
            _token = token;
        }

        /// <summary>
        /// 注册服务端推送事件 handler。可在 ConnectAsync 之前调用。
        /// handler 接收一个 JArray（事件参数数组），元素顺序与服务端 SendAsync(target, args...) 一致。
        /// </summary>
        public void On(string target, Action<JArray> handler)
        {
            if (string.IsNullOrEmpty(target) || handler == null) return;
            var list = _handlers.GetOrAdd(target, _ => new List<Action<JArray>>());
            lock (list)
            {
                list.Add(handler);
            }
        }

        /// <summary>
        /// 启动连接：negotiate → WebSocket 握手 → 启动接收/ping 循环。
        /// </summary>
        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            // 1. POST negotiate 获取 connectionToken
            string connectionToken = await NegotiateAsync(ct);
            if (string.IsNullOrEmpty(connectionToken))
                throw new InvalidOperationException("negotiate 未返回 connectionToken");

            // 2. WebSocket 连接
            var wsUrl = BuildWebSocketUrl(connectionToken);
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(wsUrl), ct);

            // 3. 发送握手
            var handshakeBytes = Encoding.UTF8.GetBytes(HandshakeMessage);
            await _ws.SendAsync(new ArraySegment<byte>(handshakeBytes),
                WebSocketMessageType.Text, endOfMessage: true, ct);

            // 4. 接收握手响应（必须是一条 0x1E 结尾的消息）
            string handshakeResp = await ReceiveOneMessageAsync(ct);
            if (string.IsNullOrEmpty(handshakeResp))
                throw new InvalidOperationException("SignalR 握手响应为空");
            var handshakeJson = JObject.Parse(handshakeResp);
            if (handshakeJson["error"] != null)
                throw new InvalidOperationException("SignalR 握手失败: " + handshakeJson["error"]);

            // 5. 启动接收循环 + ping 循环
            IsConnected = true;
            _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(ct));
            _pingLoopTask = Task.Run(() => PingLoopAsync(ct));

            Console.Error.WriteLine("[CoreSignalRClient] 已连接: ConnectionId=" + (ConnectionId ?? "(null)"));
        }

        /// <summary>
        /// 调用 Hub 方法（client -> server），等待 Completion 后返回完整响应 JObject。
        /// 若服务端返回 error，抛 InvalidOperationException。
        /// </summary>
        public async Task<JObject> InvokeAsync(string methodName, params object[] args)
        {
            if (!IsConnected || _ws == null)
                throw new InvalidOperationException("Hub 未连接");
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("methodName 不能为空", "methodName");

            var invocationId = Interlocked.Increment(ref _nextInvocationId).ToString();
            var msg = new JObject
            {
                ["type"] = 1,
                ["invocationId"] = invocationId,
                ["target"] = methodName,
                ["arguments"] = SerializeArgs(args)
            };
            var tcs = new TaskCompletionSource<JObject>();
            _pendingInvocations[invocationId] = tcs;

            await SendMessageAsync(msg);

            // 等待 Completion（type=3）
            var completion = await tcs.Task;
            var error = completion["error"]?.ToString();
            if (!string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Hub 调用失败: " + error);
            return completion;
        }

        /// <summary>不等待返回的单向调用（type=1 但不带 invocationId，服务端不会回 Completion）。</summary>
        public async Task SendAsync(string methodName, params object[] args)
        {
            if (!IsConnected || _ws == null)
                throw new InvalidOperationException("Hub 未连接");
            var msg = new JObject
            {
                ["type"] = 1,
                ["target"] = methodName,
                ["arguments"] = SerializeArgs(args)
            };
            await SendMessageAsync(msg);
        }

        public Task StopAsync()
        {
            IsConnected = false;
            try { _cts?.Cancel(); } catch { /* ignore */ }
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                try
                {
                    _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "client close", CancellationToken.None)
                        .Wait(TimeSpan.FromSeconds(2));
                }
                catch { /* ignore */ }
            }
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            try { IsConnected = false; } catch { /* ignore */ }
            try { _cts?.Cancel(); } catch { /* ignore */ }
            try { _ws?.Dispose(); } catch { /* ignore */ }
            try { _cts?.Dispose(); } catch { /* ignore */ }
        }

        // ============= 私有方法 =============

        private async Task<string> NegotiateAsync(CancellationToken ct)
        {
            var negotiateUrl = _serverBaseUrl + _hubPath + "/negotiate?negotiateVersion=1"
                + "&userId=" + Uri.EscapeDataString(_userId)
                + "&token=" + Uri.EscapeDataString(_token);

            using (var http = new HttpClient { Timeout = ConnectTimeout })
            {
                var resp = await http.PostAsync(negotiateUrl, new StringContent(""), ct);
                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException("negotiate 失败: " + (int)resp.StatusCode + " " + resp.ReasonPhrase);

                var body = await resp.Content.ReadAsStringAsync();
                var obj = JObject.Parse(body);
                // negotiateVersion=1 返回 connectionToken；negotiateVersion=0 仅返回 connectionId
                var token = obj["connectionToken"] ?? obj["connectionId"];
                var connId = obj["connectionId"];
                if (connId != null) ConnectionId = connId.ToString();
                return token?.ToString();
            }
        }

        private string BuildWebSocketUrl(string connectionToken)
        {
            // 将 http:// 转为 ws://，https:// 转为 wss://
            string baseUrl = _serverBaseUrl;
            if (baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                baseUrl = "wss://" + baseUrl.Substring("https://".Length);
            else if (baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                baseUrl = "ws://" + baseUrl.Substring("http://".Length);

            return baseUrl + _hubPath
                + "?id=" + Uri.EscapeDataString(connectionToken)
                + "&userId=" + Uri.EscapeDataString(_userId)
                + "&token=" + Uri.EscapeDataString(_token);
        }

        private async Task SendMessageAsync(JObject msg)
        {
            var json = msg.ToString(Formatting.None) + RecordSeparator;
            var bytes = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text, endOfMessage: true, _cts.Token);
        }

        /// <summary>将参数数组序列化为 JArray，保留 JToken 原始类型。</summary>
        private JArray SerializeArgs(object[] args)
        {
            var arr = new JArray();
            if (args == null) return arr;
            foreach (var arg in args)
            {
                if (arg == null) arr.Add(JValue.CreateNull());
                else if (arg is JToken jt) arr.Add(jt);
                else if (arg is string s) arr.Add(s);
                else if (arg is bool b) arr.Add(b);
                else if (arg is long l) arr.Add(l);
                else if (arg is int i) arr.Add(i);
                else if (arg is double d) arr.Add(d);
                else if (arg is float f) arr.Add(f);
                else if (arg is decimal dec) arr.Add(dec);
                else arr.Add(JToken.FromObject(arg));
            }
            return arr;
        }

        /// <summary>接收一条 0x1E 结尾的消息（用于握手阶段）。</summary>
        private async Task<string> ReceiveOneMessageAsync(CancellationToken ct)
        {
            var buffer = new byte[4096];
            var sb = new StringBuilder();
            while (!ct.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    IsConnected = false;
                    return null;
                }
                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                string s = sb.ToString();
                if (s.EndsWith(RecordSeparator))
                {
                    return s.Substring(0, s.Length - 1);  // 去掉末尾 0x1E
                }
                if (result.EndOfMessage)
                {
                    return s;
                }
            }
            return null;
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[8192];
            try
            {
                while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    try
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (WebSocketException wsex)
                    {
                        Console.Error.WriteLine("[CoreSignalRClient] ReceiveLoop WebSocket 异常: " + wsex.Message);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        IsConnected = false;
                        Console.Error.WriteLine("[CoreSignalRClient] 服务端关闭连接");
                        break;
                    }

                    string chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    List<string> messages = null;
                    lock (_recvBufferLock)
                    {
                        _recvBuffer.Append(chunk);
                        // 切分所有 0x1E 结尾的完整消息
                        string s = _recvBuffer.ToString();
                        if (s.IndexOf(RecordSeparator) >= 0)
                        {
                            messages = new List<string>();
                            while (true)
                            {
                                int idx = s.IndexOf(RecordSeparator);
                                if (idx < 0) break;
                                messages.Add(s.Substring(0, idx));
                                s = s.Substring(idx + 1);
                            }
                            _recvBuffer.Clear();
                            if (s.Length > 0) _recvBuffer.Append(s);  // 保留尾部不完整部分
                        }
                    }

                    if (messages != null)
                    {
                        foreach (var msgJson in messages)
                        {
                            if (string.IsNullOrEmpty(msgJson)) continue;
                            try { ProcessMessage(msgJson); }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("[CoreSignalRClient] ProcessMessage 异常: " + ex.Message + " json=" + msgJson);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException) { /* 正常关闭 */ }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[CoreSignalRClient] ReceiveLoop 异常: " + ex.Message);
            }
            IsConnected = false;

            // 取消所有等待中的调用
            foreach (var kv in _pendingInvocations)
            {
                var errResult = new JObject();
                errResult["error"] = "connection closed";
                kv.Value.TrySetResult(errResult);
            }
            _pendingInvocations.Clear();
        }

        private void ProcessMessage(string msgJson)
        {
            var msg = JObject.Parse(msgJson);
            var type = msg["type"]?.Value<int>() ?? 0;

            switch (type)
            {
                case 1: // Invocation
                    {
                        var target = msg["target"]?.ToString();
                        var args = msg["arguments"] as JArray ?? new JArray();
                        if (!string.IsNullOrEmpty(target))
                        {
                            List<Action<JArray>> handlers;
                            if (_handlers.TryGetValue(target, out handlers))
                            {
                                Action<JArray>[] snapshot;
                                lock (handlers) snapshot = handlers.ToArray();
                                foreach (var h in snapshot)
                                {
                                    try { h(args); }
                                    catch (Exception ex)
                                    {
                                        Console.Error.WriteLine("[CoreSignalRClient] handler 异常: " + target + " " + ex.Message);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case 3: // Completion
                    {
                        var invocationId = msg["invocationId"]?.ToString();
                        if (!string.IsNullOrEmpty(invocationId))
                        {
                            TaskCompletionSource<JObject> tcs;
                            if (_pendingInvocations.TryRemove(invocationId, out tcs))
                            {
                                tcs.TrySetResult(msg);
                            }
                        }
                        break;
                    }
                case 6: // Ping，忽略（不要求回复）
                    break;
                case 7: // Close
                    {
                        var error = msg["error"]?.ToString();
                        Console.Error.WriteLine("[CoreSignalRClient] 服务端 Close: " + (error ?? "(no error)"));
                        IsConnected = false;
                        try { _cts?.Cancel(); } catch { /* ignore */ }
                        break;
                    }
                default:
                    // 其他类型忽略（StreamItem=2 / StreamInvocation=4 / CancelInvocation=5 / Ack=8 / Sequence=9）
                    break;
            }
        }

        private async Task PingLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsConnected && _ws.State == WebSocketState.Open)
                {
                    try { await Task.Delay(PingInterval, ct); }
                    catch (OperationCanceledException) { break; }
                    if (!IsConnected || _ws.State != WebSocketState.Open) break;
                    var pingJson = "{\"type\":6}\x1E";
                    var bytes = Encoding.UTF8.GetBytes(pingJson);
                    try
                    {
                        await _ws.SendAsync(new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text, endOfMessage: true, ct);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (WebSocketException wsex)
                    {
                        Console.Error.WriteLine("[CoreSignalRClient] Ping 发送失败: " + wsex.Message);
                        break;
                    }
                }
            }
            catch (OperationCanceledException) { /* 正常 */ }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[CoreSignalRClient] PingLoop 异常: " + ex.Message);
            }
        }
    }
}
