﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// SignalR 测试客户端封装（Task 6.1）。
    ///
    /// 协议兼容说明（P1-11 修复）：
    ///   原实现使用 Microsoft.AspNet.SignalR.Client.HubConnection（旧版 .NET Framework SignalR v2 客户端），
    ///   与服务端 ASP.NET Core SignalR 协议不兼容（握手失败导致 6 个 hub 步骤全部 skipped）。
    ///   现 net462 无法直接使用 Microsoft.AspNetCore.SignalR.Client（需 net472+），
    ///   故内部委托到 CoreSignalRClient（基于 ClientWebSocket 实现原生 ASP.NET Core SignalR JSON 协议）。
    ///
    /// 公共 API 保持不变（ConnectAsync / LoginAsync / SendPeerEventAsync / WaitForCallbackAsync / DisconnectAsync）。
    /// 通过 QueryString 传递 userId+token（与 ChatHub.OnConnectedAsync 解析逻辑一致）。
    /// 维护静态 _clients 注册表，按 sessionName 索引。
    /// </summary>
    public class SignalRTestClient : IDisposable
    {
        // ============= 静态注册表 =============
        // 按 sessionName 维护所有活跃客户端（替代 SessionState.HubConnections）
        private static readonly ConcurrentDictionary<string, SignalRTestClient> _clients =
            new ConcurrentDictionary<string, SignalRTestClient>(StringComparer.OrdinalIgnoreCase);

        /// <summary>按 sessionName 获取客户端；不存在返回 null。</summary>
        public static SignalRTestClient GetClient(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName)) return null;
            SignalRTestClient client;
            _clients.TryGetValue(sessionName, out client);
            return client;
        }

        /// <summary>按 sessionName 移除并返回客户端；不存在返回 null。</summary>
        public static SignalRTestClient RemoveClient(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName)) return null;
            SignalRTestClient client;
            _clients.TryRemove(sessionName, out client);
            return client;
        }

        /// <summary>当前所有活跃会话名（只读快照）。</summary>
        public static ICollection<string> GetSessionNames()
        {
            return _clients.Keys;
        }

        // ============= 实例字段 =============

        public string SessionName { get; private set; }

        /// <summary>
        /// 服务端分配的 ConnectionId（negotiate 返回）。
        /// 原 HubConnection.ConnectionId 的替代，保留以兼容 SignalRTestTools 的引用。
        /// </summary>
        public string ConnectionId { get; private set; }

        public bool IsConnected { get; private set; }

        private readonly CoreSignalRClient _core;
        private readonly long _userId;
        private bool _disposed;

        /// <summary>
        /// 收到的 Peer 回调事件队列：eventName -> 已收到 payload 列表（按到达顺序）。
        /// 每个 payload 形如 { event, args: JArray, timestamp }。
        /// </summary>
        public ConcurrentDictionary<string, List<JObject>> ReceivedCallbacks { get; } =
            new ConcurrentDictionary<string, List<JObject>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>等待中的 waiter 队列：eventName -> TaskCompletionSource 列表（FIFO）。</summary>
        private readonly ConcurrentDictionary<string, List<TaskCompletionSource<JObject>>> _waiters =
            new ConcurrentDictionary<string, List<TaskCompletionSource<JObject>>>(StringComparer.OrdinalIgnoreCase);

        // ============= 构造函数 =============

        /// <summary>
        /// 构造 SignalR 测试客户端。
        /// </summary>
        /// <param name="sessionName">会话名（如 userA/userB），用于多会话隔离</param>
        /// <param name="serverBaseUrl">服务端基地址（如 http://82.156.108.218:8957）</param>
        /// <param name="userId">已登录的用户 ID（必须先通过 cloud_login 获取 Token）</param>
        /// <param name="token">用户 Token（与 userId 配对，由 AuthService 生成）</param>
        public SignalRTestClient(string sessionName, string serverBaseUrl, long userId, string token)
        {
            if (string.IsNullOrEmpty(sessionName))
                throw new ArgumentException("sessionName 不能为空", "sessionName");
            if (userId <= 0)
                throw new ArgumentException("userId 必须为正整数", "userId");
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("token 不能为空", "token");

            SessionName = sessionName;
            _userId = userId;

            // 使用 CoreSignalRClient（原生 ASP.NET Core SignalR JSON 协议）
            // Hub URL：服务端 Program.cs 中 app.MapHub<ChatHub>("/ChatHub")
            _core = new CoreSignalRClient(serverBaseUrl, "/ChatHub", userId.ToString(), token);

            // 注册所有已知 Peer 回调 handler（与 ChatHub.cs 中 SendAsync("EventName", args) 一致）
            RegisterKnownCallbackHandlers();
        }

        // ============= 已知回调 handler 注册 =============

        /// <summary>
        /// 为 ChatHub 推送的所有已知 Peer 回调事件注册 handler。
        /// handler 将事件 args 封装为 JObject 存入 ReceivedCallbacks，并唤醒 WaitForCallbackAsync 的 waiter。
        /// 回调签名严格按 ChatHub.cs 中 Clients.GroupExcept(...).SendAsync("EventName", args) 调用为准。
        /// </summary>
        private void RegisterKnownCallbackHandlers()
        {
            // 1-arg string 事件
            _core.On("PeerLogin", args => StoreCallback("PeerLogin", args));
            _core.On("PeerLogout", args => StoreCallback("PeerLogout", args));

            // 2-arg string 事件
            _core.On("PeerOpensProject", args => StoreCallback("PeerOpensProject", args));
            _core.On("ReceiveFromUser", args => StoreCallback("ReceiveFromUser", args));
            _core.On("ProjectBroadcast", args => StoreCallback("ProjectBroadcast", args));
            _core.On("TeamBroadcast", args => StoreCallback("TeamBroadcast", args));
            _core.On("ProjectSynced", args => StoreCallback("ProjectSynced", args));
            _core.On("PeerPushesTreeNode", args => StoreCallback("PeerPushesTreeNode", args));

            // 3-arg string 事件
            _core.On("PeerOpensTreeNode", args => StoreCallback("PeerOpensTreeNode", args));
            _core.On("PeerTableCellChange", args => StoreCallback("PeerTableCellChange", args));
            _core.On("PeerParagraphChange", args => StoreCallback("PeerParagraphChange", args));

            // 4-arg string 事件
            _core.On("PeerOpenTicketNavTreeNode", args => StoreCallback("PeerOpenTicketNavTreeNode", args));

            // 混合事件：string + JObject（FileSection/UserState 反序列化为 JObject）
            _core.On("PeerFileSectionArrived", args => StoreCallback("PeerFileSectionArrived", args));
            _core.On("PeerStateUpload", args => StoreCallback("PeerStateUpload", args));
        }

        /// <summary>
        /// 将收到的回调事件封装为 JObject 存入 ReceivedCallbacks，并唤醒等待中的 waiter。
        /// payload 结构：{ event, args: JArray, timestamp }
        /// </summary>
        private void StoreCallback(string eventName, JArray args)
        {
            var payload = new JObject();
            payload["event"] = eventName;
            payload["args"] = args ?? new JArray();
            payload["timestamp"] = DateTime.Now.ToString("o");

            // 加入已收到队列
            var list = ReceivedCallbacks.GetOrAdd(eventName, _ => new List<JObject>());
            lock (list)
            {
                list.Add(payload);
            }

            // 唤醒一个等待中的 waiter（FIFO）
            NotifyWaiter(eventName, payload);

            Console.Error.WriteLine("[SignalRTestClient] " + SessionName + " <- " + eventName
                + " args=" + (args != null ? args.Count : 0));
        }

        /// <summary>唤醒指定事件的第一个 waiter（FIFO）。</summary>
        private void NotifyWaiter(string eventName, JObject payload)
        {
            List<TaskCompletionSource<JObject>> waiters;
            if (!_waiters.TryGetValue(eventName, out waiters)) return;
            TaskCompletionSource<JObject> toNotify = null;
            lock (waiters)
            {
                if (waiters.Count > 0)
                {
                    toNotify = waiters[0];
                    waiters.RemoveAt(0);
                }
            }
            if (toNotify != null)
            {
                var result = new JObject();
                result["received"] = true;
                result["payload"] = payload;
                toNotify.TrySetResult(result);
            }
        }

        // ============= 公共方法 =============

        /// <summary>
        /// 启动 SignalR 连接（含 negotiate 握手）。连接成功后自动注册到 _clients。
        /// </summary>
        public async Task ConnectAsync()
        {
            if (IsConnected) return;
            try
            {
                await _core.ConnectAsync();
                IsConnected = _core.IsConnected;
                ConnectionId = _core.ConnectionId;
                _clients[SessionName] = this;
                Console.Error.WriteLine("[SignalRTestClient] 已连接: " + SessionName
                    + " ConnectionId=" + (ConnectionId ?? "(null)"));
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Console.Error.WriteLine("[SignalRTestClient] 连接失败: " + SessionName + " " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 调用 Hub 的 Login 方法加入团队/项目 Group。
        /// 服务端 ChatHub.Login(string userId, UserStateDto state)：
        ///   - 不信任客户端传入的 userId，使用鉴权 userId
        ///   - state.TeamId/ProjectId 校验归属后加入对应 Group（team_{GUID}/project_{GUID}）
        ///   - 通知同组其他用户 PeerLogin
        /// </summary>
        /// <param name="projectId">项目 ID（GUID 字符串，可选；传入则加入 project_{projectId} 组）</param>
        /// <param name="teamId">团队 ID（GUID 字符串，可选；传入则加入 team_{teamId} 组）</param>
        /// <returns>调用是否成功（无异常即视为成功）</returns>
        public async Task<bool> LoginAsync(string projectId = null, string teamId = null)
        {
            if (!IsConnected) return false;
            try
            {
                // 构造 UserState（与服务端 UserStateDto 字段对齐，PascalCase）
                var state = new JObject();
                state["UserId"] = _userId.ToString();
                state["ConnectionId"] = ConnectionId ?? "";
                if (!string.IsNullOrEmpty(projectId)) state["ProjectId"] = projectId;
                if (!string.IsNullOrEmpty(teamId)) state["TeamId"] = teamId;

                // 调用 Hub 的 Login 方法（服务端忽略客户端传入的 userId，使用 QueryString 中的鉴权 userId）
                // Login 返回 IEnumerable<UserStateDto>，但现有代码不使用结果，只确认调用完成
                await _core.InvokeAsync("Login", _userId.ToString(), state);
                Console.Error.WriteLine("[SignalRTestClient] Login 成功: " + SessionName
                    + " projectId=" + (projectId ?? "(null)") + " teamId=" + (teamId ?? "(null)"));
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[SignalRTestClient] LoginAsync 失败: " + SessionName + " " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 通过 Hub 调用指定方法触发 Peer 回调。
        /// 例如调用 UpLoadTableCellId(userId, cellId) 会触发项目组其他用户收到 PeerTableCellChange。
        /// </summary>
        /// <param name="methodName">Hub 方法名（如 UpLoadTableCellId / UploadParagraphId / BroadcastToProjectUsers / OpenTreeNode）</param>
        /// <param name="args">方法参数数组（按服务端方法签名顺序）</param>
        public async Task SendPeerEventAsync(string methodName, params object[] args)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Hub 未连接，请先调用 connect_hub");
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("methodName 不能为空", "methodName");

            try
            {
                // 使用 InvokeAsync 等待 Completion，确保 Hub 方法已完成（与原 _proxy.Invoke 行为一致）
                await _core.InvokeAsync(methodName, args ?? new object[0]);
                Console.Error.WriteLine("[SignalRTestClient] -> " + SessionName
                    + " 调用 " + methodName + " args=" + (args == null ? 0 : args.Length));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[SignalRTestClient] SendPeerEventAsync 失败: " + SessionName
                    + " " + methodName + " " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 等待指定 Peer 回调事件触发。
        /// 先检查 ReceivedCallbacks 已收到队列，命中则立即返回；否则注册 waiter 等待新事件，超时返回 {received: false}。
        /// </summary>
        /// <param name="callbackName">回调事件名（如 PeerTableCellChange）</param>
        /// <param name="timeoutMs">超时毫秒（默认 5000）</param>
        /// <returns>{received: true, payload: {...}} 或 {received: false, reason: "timeout"}</returns>
        public async Task<JObject> WaitForCallbackAsync(string callbackName, int timeoutMs = 5000)
        {
            if (string.IsNullOrEmpty(callbackName))
            {
                var err = new JObject();
                err["received"] = false;
                err["reason"] = "callbackName is empty";
                return err;
            }

            // 1. 先检查已收到队列
            List<JObject> list;
            if (ReceivedCallbacks.TryGetValue(callbackName, out list))
            {
                lock (list)
                {
                    if (list.Count > 0)
                    {
                        var first = list[0];
                        list.RemoveAt(0);
                        var immediate = new JObject();
                        immediate["received"] = true;
                        immediate["payload"] = first;
                        return immediate;
                    }
                }
            }

            // 2. 注册 waiter
            var tcs = new TaskCompletionSource<JObject>();
            var waiters = _waiters.GetOrAdd(callbackName, _ => new List<TaskCompletionSource<JObject>>());
            lock (waiters)
            {
                waiters.Add(tcs);
            }

            // 3. 启动超时（CancellationTokenSource.CancelAfter 触发后 TrySetResult）
            var cts = new CancellationTokenSource();
            cts.Token.Register(() =>
            {
                var timeoutResult = new JObject();
                timeoutResult["received"] = false;
                timeoutResult["reason"] = "timeout";
                timeoutResult["callbackName"] = callbackName;
                tcs.TrySetResult(timeoutResult);
            });
            cts.CancelAfter(Math.Max(0, timeoutMs));

            // 4. 等待结果（事件触发会调用 NotifyWaiter -> tcs.TrySetResult）
            var waitResult = await tcs.Task;

            // 5. 清理：从 waiter 列表移除（如果还存在，例如超时后事件才到达）
            try
            {
                List<TaskCompletionSource<JObject>> currentWaiters;
                if (_waiters.TryGetValue(callbackName, out currentWaiters))
                {
                    lock (currentWaiters)
                    {
                        currentWaiters.Remove(tcs);
                    }
                }
            }
            catch { /* ignore cleanup errors */ }

            try { cts.Dispose(); } catch { /* ignore */ }
            return waitResult;
        }

        /// <summary>
        /// 关闭 Hub 连接，从静态注册表移除。
        /// </summary>
        public Task DisconnectAsync()
        {
            try
            {
                _core.StopAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[SignalRTestClient] DisconnectAsync 异常: " + SessionName + " " + ex.Message);
            }
            IsConnected = false;
            SignalRTestClient removed;
            _clients.TryRemove(SessionName, out removed);
            return Task.FromResult(0);
        }

        // ============= IDisposable =============

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _core.Dispose(); } catch { /* ignore */ }
            IsConnected = false;
            SignalRTestClient removed;
            _clients.TryRemove(SessionName, out removed);
        }
    }
}
