﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AuditAI.McpServer.State;
using Newtonsoft.Json;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 云端 API 客户端
    /// 封装 HttpClient + Token 管理 + 响应捕获，支持多会话隔离。
    /// 与服务端 TokenAuthMiddleware 约定：请求头 UserId + Token（非 Bearer）。
    /// </summary>
    public static class CloudApiClient
    {
        private const string DefaultBaseUrl = "http://82.156.108.218:8957";
        private const string MainSessionName = "main";

        // 调用上下文（供 CaptureResponse 使用；线程静态以避免并发串扰）
        [ThreadStatic]
        private static string _ctxMethod;
        [ThreadStatic]
        private static string _ctxUrl;
        [ThreadStatic]
        private static string _ctxSessionName;

        /// <summary>
        /// 读取服务端基地址（从 App.config 的 ServerBaseUrl，默认 http://82.156.108.218:8957）。
        /// 末尾斜杠会被去掉。
        /// </summary>
        public static string ServerBaseUrl
        {
            get
            {
                try
                {
                    var v = ConfigurationManager.AppSettings["ServerBaseUrl"];
                    return string.IsNullOrWhiteSpace(v) ? DefaultBaseUrl : v.TrimEnd('/');
                }
                catch
                {
                    return DefaultBaseUrl;
                }
            }
        }

        // ===== 多会话管理 =====

        /// <summary>
        /// 创建新会话并加入 SessionState；若已存在则先 Dispose 旧 HttpClient 再覆盖。线程安全。
        /// </summary>
        public static void BeginSession(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName))
                sessionName = MainSessionName;
            var session = new TestSession
            {
                SessionName = sessionName,
                HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) }
            };
            SessionState.ReplaceSession(sessionName, session);
        }

        /// <summary>
        /// 获取指定会话；不存在则创建。sessionName 为 null/空时使用 "main"。
        /// </summary>
        public static TestSession UseSession(string sessionName)
        {
            return SessionState.GetOrCreateSession(sessionName);
        }

        /// <summary>
        /// 结束并移除指定会话，释放其 HttpClient。线程安全。
        /// </summary>
        public static void EndSession(string sessionName)
        {
            if (string.IsNullOrEmpty(sessionName))
                sessionName = MainSessionName;
            SessionState.RemoveSession(sessionName);
        }

        /// <summary>
        /// 登录成功后更新会话的 Token/UserId。
        /// 若 sessionName 为空或 "main"，同时同步到 SessionState.CurrentAuthToken/CurrentUserId。
        /// </summary>
        public static void SetAuthToken(string sessionName, string token, long userId)
        {
            TestSession session = UseSession(sessionName);
            session.AuthToken = token;
            session.UserId = userId;
            if (string.IsNullOrEmpty(sessionName) || sessionName == MainSessionName)
            {
                SessionState.CurrentAuthToken = token;
                SessionState.CurrentUserId = userId;
            }
        }

        // ===== HTTP 方法 =====

        /// <summary>
        /// 同步 GET 请求。
        /// </summary>
        /// <param name="path">相对路径（如 /api/User/AccountLogin）</param>
        /// <param name="query">可选查询参数字典</param>
        /// <param name="sessionName">会话名（null 表示 main）</param>
        /// <param name="withAuth">是否自动携带 UserId/Token 头</param>
        public static TestResponse GetAsync(string path, Dictionary<string, string> query = null, string sessionName = null, bool withAuth = true)
        {
            string url = BuildUrl(path, query);
            TestSession session = UseSession(sessionName);
            HttpClient client = session.HttpClient ?? SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            _ctxMethod = "GET";
            _ctxUrl = url;
            _ctxSessionName = sessionName;
            try
            {
                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    ApplyAuthHeaders(req, session, withAuth);
                    HttpResponseMessage resp = client.SendAsync(req).Result;
                    return CaptureResponse(resp, sw);
                }
            }
            catch (Exception ex)
            {
                return CaptureException(ex, sw, "GET", url, sessionName);
            }
        }

        /// <summary>
        /// 同步 POST 请求（字节体，application/octet-stream）。
        /// </summary>
        public static TestResponse PostAsync(string path, byte[] body = null, string sessionName = null, bool withAuth = true)
        {
            return PostBytesAsync(path, body, sessionName, withAuth);
        }

        /// <summary>
        /// 同步 POST JSON 请求。body 通过 JsonConvert.SerializeObject 序列化。
        /// </summary>
        public static TestResponse PostJsonAsync(string path, object body, string sessionName = null, bool withAuth = true)
        {
            string url = BuildUrl(path, null);
            TestSession session = UseSession(sessionName);
            HttpClient client = session.HttpClient ?? SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            _ctxMethod = "POST";
            _ctxUrl = url;
            _ctxSessionName = sessionName;
            try
            {
                string json = body == null ? "" : JsonConvert.SerializeObject(body);
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        req.Content = content;
                        ApplyAuthHeaders(req, session, withAuth);
                        HttpResponseMessage resp = client.SendAsync(req).Result;
                        return CaptureResponse(resp, sw);
                    }
                }
            }
            catch (Exception ex)
            {
                return CaptureException(ex, sw, "POST", url, sessionName);
            }
        }

        /// <summary>
        /// 同步 POST 字节流请求。Content-Type: application/octet-stream。
        /// </summary>
        public static TestResponse PostBytesAsync(string path, byte[] body, string sessionName = null, bool withAuth = true)
        {
            string url = BuildUrl(path, null);
            TestSession session = UseSession(sessionName);
            HttpClient client = session.HttpClient ?? SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            _ctxMethod = "POST";
            _ctxUrl = url;
            _ctxSessionName = sessionName;
            try
            {
                byte[] data = body ?? new byte[0];
                using (var content = new ByteArrayContent(data))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        req.Content = content;
                        ApplyAuthHeaders(req, session, withAuth);
                        HttpResponseMessage resp = client.SendAsync(req).Result;
                        return CaptureResponse(resp, sw);
                    }
                }
            }
            catch (Exception ex)
            {
                return CaptureException(ex, sw, "POST", url, sessionName);
            }
        }

        // ===== 管理后台 HTTP 方法（端口 8958） =====
        //
        // 说明：连接管理后台（AdminBaseUrl，默认 http://82.156.108.218:8958），
        // 自动携带 SessionState.AdminAuthToken / AdminUserId 头。
        // 复用 CaptureResponse / CaptureException 逻辑，但使用独立的 Admin 上下文字段。

        /// <summary>
        /// 异步 GET 管理后台接口。自动使用 SessionState.AdminBaseUrl 作为 BaseUrl，
        /// 自动携带 SessionState.AdminAuthToken Header。
        /// </summary>
        /// <param name="relativeUrl">相对路径（如 /api/admin/login）</param>
        /// <param name="query">可选查询参数字典</param>
        public static async Task<TestResponse> GetAdminAsync(string relativeUrl, Dictionary<string, string> query = null)
        {
            string baseUrl = ResolveAdminBaseUrl();
            string url = BuildAdminUrl(baseUrl, relativeUrl, query);
            HttpClient client = SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    ApplyAdminAuthHeaders(req);
                    HttpResponseMessage resp = await client.SendAsync(req).ConfigureAwait(false);
                    _ctxMethod = "GET";
                    _ctxUrl = url;
                    _ctxSessionName = null;
                    return CaptureResponse(resp, sw);
                }
            }
            catch (Exception ex)
            {
                _ctxMethod = "GET";
                _ctxUrl = url;
                _ctxSessionName = null;
                return CaptureException(ex, sw, "GET", url, null);
            }
        }

        /// <summary>
        /// 异步 POST 管理后台接口（原始 HttpContent）。
        /// </summary>
        public static async Task<TestResponse> PostAdminAsync(string relativeUrl, HttpContent content, Dictionary<string, string> query = null)
        {
            string baseUrl = ResolveAdminBaseUrl();
            string url = BuildAdminUrl(baseUrl, relativeUrl, query);
            HttpClient client = SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    if (content != null)
                        req.Content = content;
                    ApplyAdminAuthHeaders(req);
                    HttpResponseMessage resp = await client.SendAsync(req).ConfigureAwait(false);
                    _ctxMethod = "POST";
                    _ctxUrl = url;
                    _ctxSessionName = null;
                    return CaptureResponse(resp, sw);
                }
            }
            catch (Exception ex)
            {
                _ctxMethod = "POST";
                _ctxUrl = url;
                _ctxSessionName = null;
                return CaptureException(ex, sw, "POST", url, null);
            }
        }

        /// <summary>
        /// 异步 POST JSON 管理后台接口。body 通过 JsonConvert.SerializeObject 序列化。
        /// </summary>
        public static async Task<TestResponse> PostAdminJsonAsync(string relativeUrl, object body, Dictionary<string, string> query = null)
        {
            string baseUrl = ResolveAdminBaseUrl();
            string url = BuildAdminUrl(baseUrl, relativeUrl, query);
            HttpClient client = SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                string json = body == null ? "" : JsonConvert.SerializeObject(body);
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        req.Content = content;
                        ApplyAdminAuthHeaders(req);
                        HttpResponseMessage resp = await client.SendAsync(req).ConfigureAwait(false);
                        _ctxMethod = "POST";
                        _ctxUrl = url;
                        _ctxSessionName = null;
                        return CaptureResponse(resp, sw);
                    }
                }
            }
            catch (Exception ex)
            {
                _ctxMethod = "POST";
                _ctxUrl = url;
                _ctxSessionName = null;
                return CaptureException(ex, sw, "POST", url, null);
            }
        }

        /// <summary>
        /// 异步 POST 字节流管理后台接口。Content-Type: application/octet-stream。
        /// </summary>
        public static async Task<TestResponse> PostAdminBytesAsync(string relativeUrl, byte[] bytes, Dictionary<string, string> query = null)
        {
            string baseUrl = ResolveAdminBaseUrl();
            string url = BuildAdminUrl(baseUrl, relativeUrl, query);
            HttpClient client = SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                byte[] data = bytes ?? new byte[0];
                using (var content = new ByteArrayContent(data))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        req.Content = content;
                        ApplyAdminAuthHeaders(req);
                        HttpResponseMessage resp = await client.SendAsync(req).ConfigureAwait(false);
                        _ctxMethod = "POST";
                        _ctxUrl = url;
                        _ctxSessionName = null;
                        return CaptureResponse(resp, sw);
                    }
                }
            }
            catch (Exception ex)
            {
                _ctxMethod = "POST";
                _ctxUrl = url;
                _ctxSessionName = null;
                return CaptureException(ex, sw, "POST", url, null);
            }
        }

        // ===== 内部辅助 =====

        /// <summary>
        /// 拼接完整 URL：ServerBaseUrl + path + 可选 query。
        /// query 字典使用 Uri.EscapeDataString 编码。
        /// </summary>
        private static string BuildUrl(string path, Dictionary<string, string> query)
        {
            if (string.IsNullOrEmpty(path))
                path = "/";
            if (!path.StartsWith("/"))
                path = "/" + path;
            string url = ServerBaseUrl + path;
            if (query != null && query.Count > 0)
            {
                var sb = new StringBuilder();
                bool first = true;
                foreach (KeyValuePair<string, string> kv in query)
                {
                    if (string.IsNullOrEmpty(kv.Key)) continue;
                    sb.Append(first ? "?" : "&");
                    sb.Append(Uri.EscapeDataString(kv.Key));
                    sb.Append("=");
                    sb.Append(Uri.EscapeDataString(kv.Value ?? ""));
                    first = false;
                }
                url += sb.ToString();
            }
            return url;
        }

        /// <summary>
        /// 在 withAuth=true 时，向请求追加 UserId 与 Token 头（参照服务端 HeaderParser）。
        /// 若 session 未设置 Token，回退到 SessionState 全局值；若仍为空则不携带（用于测试 401）。
        /// </summary>
        private static void ApplyAuthHeaders(HttpRequestMessage req, TestSession session, bool withAuth)
        {
            if (!withAuth) return;
            string token = session != null ? session.AuthToken : null;
            long userId = session != null ? session.UserId : 0;
            if (string.IsNullOrEmpty(token))
            {
                token = SessionState.CurrentAuthToken;
                userId = SessionState.CurrentUserId;
            }
            if (userId > 0)
            {
                req.Headers.TryAddWithoutValidation("UserId", userId.ToString());
            }
            if (!string.IsNullOrEmpty(token))
            {
                req.Headers.TryAddWithoutValidation("Token", token);
            }
        }

        /// <summary>
        /// 解析管理后台基地址：优先 SessionState.AdminBaseUrl，其次 App.config 的 AdminBaseUrl，
        /// 最后回退到默认值 http://82.156.108.218:8958。末尾斜杠会被去掉。
        /// </summary>
        private static string ResolveAdminBaseUrl()
        {
            string baseUrl = SessionState.AdminBaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                try
                {
                    baseUrl = ConfigurationManager.AppSettings["AdminBaseUrl"];
                }
                catch
                {
                    baseUrl = null;
                }
            }
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "http://82.156.108.218:8958";
            return baseUrl.TrimEnd('/');
        }

        /// <summary>
        /// 拼接管理后台完整 URL：baseUrl + path + 可选 query。
        /// query 字典使用 Uri.EscapeDataString 编码。
        /// </summary>
        private static string BuildAdminUrl(string baseUrl, string path, Dictionary<string, string> query)
        {
            if (string.IsNullOrEmpty(path))
                path = "/";
            if (!path.StartsWith("/"))
                path = "/" + path;
            string url = baseUrl + path;
            if (query != null && query.Count > 0)
            {
                var sb = new StringBuilder();
                bool first = true;
                foreach (KeyValuePair<string, string> kv in query)
                {
                    if (string.IsNullOrEmpty(kv.Key)) continue;
                    sb.Append(first ? "?" : "&");
                    sb.Append(Uri.EscapeDataString(kv.Key));
                    sb.Append("=");
                    sb.Append(Uri.EscapeDataString(kv.Value ?? ""));
                    first = false;
                }
                url += sb.ToString();
            }
            return url;
        }

        /// <summary>
        /// 为管理后台请求追加 UserId 与 Token 头（来自 SessionState.AdminUserId/AdminAuthToken）。
        /// 若 Token 为空则不携带（用于测试 401）。
        /// </summary>
        private static void ApplyAdminAuthHeaders(HttpRequestMessage req)
        {
            long userId = SessionState.AdminUserId;
            string token = SessionState.AdminAuthToken;
            if (userId > 0)
            {
                req.Headers.TryAddWithoutValidation("UserId", userId.ToString());
            }
            if (!string.IsNullOrEmpty(token))
            {
                req.Headers.TryAddWithoutValidation("Token", token);
            }
        }

        /// <summary>
        /// 捕获 HttpResponseMessage 为 TestResponse，并同步写入 SessionState.LastResponse
        /// 与对应 session 的 LastResponse（以及 CurrentSession.LastResponse）。
        /// </summary>
        private static TestResponse CaptureResponse(HttpResponseMessage response, Stopwatch sw)
        {
            sw.Stop();
            string method = _ctxMethod ?? "UNKNOWN";
            string url = _ctxUrl ?? "";
            string sessionName = _ctxSessionName;

            var tr = new TestResponse
            {
                StatusCode = (int)response.StatusCode,
                RequestUrl = url,
                RequestMethod = method,
                ElapsedMs = sw.ElapsedMilliseconds,
                Timestamp = DateTime.Now
            };
            // 合并响应头 + 内容头
            if (response.Headers != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> h in response.Headers)
                {
                    if (string.IsNullOrEmpty(h.Key)) continue;
                    tr.Headers[h.Key] = string.Join(", ", h.Value ?? new List<string>());
                }
            }
            if (response.Content != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> h in response.Content.Headers)
                {
                    if (string.IsNullOrEmpty(h.Key)) continue;
                    tr.Headers[h.Key] = string.Join(", ", h.Value ?? new List<string>());
                }
                try
                {
                    tr.Body = response.Content.ReadAsStringAsync().Result;
                }
                catch
                {
                    tr.Body = "";
                }
            }
            // 同步到全局与对应 session
            SessionState.LastResponse = tr;
            TestSession session = UseSession(sessionName);
            session.LastResponse = tr;
            // 同时确保 main/CurrentSession 也更新（spec 要求）
            SessionState.CurrentSession.LastResponse = tr;
            Console.Error.WriteLine("[CloudApi] " + method + " " + url + " -> " + tr.StatusCode + " (" + tr.ElapsedMs + "ms)");
            return tr;
        }

        /// <summary>
        /// 网络异常时构造失败的 TestResponse（StatusCode=0），不抛异常以便上层断言处理。
        /// </summary>
        private static TestResponse CaptureException(Exception ex, Stopwatch sw, string method, string url, string sessionName)
        {
            sw.Stop();
            Exception inner = ex;
            while (inner.InnerException != null) inner = inner.InnerException;
            string msg = inner != null ? inner.Message : ex.Message;
            string escaped = (msg ?? "")
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", " ")
                .Replace("\n", " ");
            var tr = new TestResponse
            {
                StatusCode = 0,
                Body = "{\"error\":\"" + escaped + "\"}",
                ElapsedMs = sw.ElapsedMilliseconds,
                Timestamp = DateTime.Now,
                RequestUrl = url,
                RequestMethod = method
            };
            SessionState.LastResponse = tr;
            TestSession session = UseSession(sessionName);
            session.LastResponse = tr;
            SessionState.CurrentSession.LastResponse = tr;
            Console.Error.WriteLine("[CloudApi] " + method + " " + url + " -> EX " + msg);
            return tr;
        }
    }
}
