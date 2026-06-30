using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using AuditAI.McpServer.State;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 云端 API 调用工具集（Task 4）
    /// 覆盖服务端 8 大模块的 HTTP 端点，每个端点注册为一个原子调用工具。
    /// 工具发起 HTTP 请求、捕获响应（状态码/头/体/耗时），存入 SessionState 供后续断言。
    /// 登录类工具会自动解析 Token 并保存到会话上下文。
    /// </summary>
    public static class CloudApiTools
    {
        /// <summary>
        /// 注册所有云端 API 调用工具（8 大模块）。
        /// </summary>
        public static void Register()
        {
            RegisterAuthTools();        // SubTask 4.2
            RegisterUserTeamTools();    // SubTask 4.3
            RegisterProjectTools();     // SubTask 4.4
            RegisterSyncTools();        // SubTask 4.5
            RegisterFileStorageTools(); // SubTask 4.6
            RegisterTaskTools();        // SubTask 4.7
            RegisterDictionaryTools();  // SubTask 4.8
            RegisterLicenseTools();     // SubTask 4.9
        }

        // =====================================================================
        // SubTask 4.2: 认证模块工具（10 个）
        // 服务端端点定义在 Program.cs（MapGet/MapPost），非 Controller 文件。
        // withAuth=false 的端点：AccountLogin / AccountLoginBySMS / SMSReLogin /
        //   GetValidateCode / Register / FindPassword / WechatLogin / QQLogin。
        // 需要额外 Header（ValidateCode）的端点使用 SendWithExtraHeaders 辅助方法。
        // =====================================================================

        private static void RegisterAuthTools()
        {
            // cloud_login: GET /api/User/AccountLogin?userName=&password=&version=
            ToolRegistry.Register("cloud_login",
                "登录服务端并获取 Token。登录成功后自动保存 Token 和 UserId 到当前会话，后续 cloud_* 工具会自动携带。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["password"] = new JObject { ["type"] = "string", ["description"] = "密码" },
                        ["version"] = new JObject { ["type"] = "string", ["description"] = "客户端版本号（可选，默认 1.0.0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选，用于多会话场景，默认 main）" }
                    },
                    ["required"] = new JArray { "userName", "password" }
                },
                (args) =>
                {
                    string userName = args["userName"] != null ? args["userName"].ToString() : null;
                    string password = args["password"] != null ? args["password"].ToString() : null;
                    string version = args["version"] != null ? args["version"].ToString() : "1.0.0";
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;

                    var query = new Dictionary<string, string>
                    {
                        { "userName", userName },
                        { "password", password },
                        { "version", version }
                    };

                    var resp = CloudApiClient.GetAsync("/api/User/AccountLogin", query, sessionName, withAuth: false);

                    if (resp.StatusCode == 200)
                    {
                        try
                        {
                            var body = JObject.Parse(resp.Body ?? "{}");
                            var token = body["Item1"] != null ? (body["Item1"]["TokenValue"] ?? body["Item1"]["Token"] ?? body["Item1"]["LastToken"]) : null;
                            string tokenStr = token != null ? token.ToString() : null;
                            long userId = 0;
                            var userIdToken = body["Item2"] != null ? body["Item2"]["Id"] : null;
                            if (userIdToken != null) userId = userIdToken.Value<long>();
                            if (!string.IsNullOrEmpty(tokenStr) && userId > 0)
                            {
                                CloudApiClient.SetAuthToken(sessionName, tokenStr, userId);
                            }
                        }
                        catch { /* 忽略解析错误，响应仍正常返回 */ }
                    }

                    return resp.ToJson();
                });

            // cloud_login_by_sms: GET /api/User/AccountLoginBySMS?phone=&version=
            // Header: ValidateCode（手机号验证码）
            ToolRegistry.Register("cloud_login_by_sms",
                "短信验证码登录。需先调用 cloud_get_validate_code 获取验证码，再将验证码通过 validateCode 参数传入。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号" },
                        ["validateCode"] = new JObject { ["type"] = "string", ["description"] = "短信验证码（通过 Header ValidateCode 传递）" },
                        ["version"] = new JObject { ["type"] = "string", ["description"] = "客户端版本号（可选）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "phone", "validateCode" }
                },
                (args) =>
                {
                    string phone = args["phone"] != null ? args["phone"].ToString() : null;
                    string validateCode = args["validateCode"] != null ? args["validateCode"].ToString() : null;
                    string version = args["version"] != null ? args["version"].ToString() : null;
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;

                    var query = new Dictionary<string, string>();
                    query["phone"] = phone;
                    if (version != null) query["version"] = version;

                    var extraHeaders = new Dictionary<string, string>();
                    if (validateCode != null) extraHeaders["ValidateCode"] = validateCode;

                    var resp = SendWithExtraHeaders("GET", "/api/User/AccountLoginBySMS", query, null, null, sessionName, false, extraHeaders);

                    if (resp.StatusCode == 200)
                    {
                        try
                        {
                            var body = JObject.Parse(resp.Body ?? "{}");
                            var token = body["Item1"] != null ? (body["Item1"]["TokenValue"] ?? body["Item1"]["Token"] ?? body["Item1"]["LastToken"]) : null;
                            string tokenStr = token != null ? token.ToString() : null;
                            long userId = 0;
                            var userIdToken = body["Item2"] != null ? body["Item2"]["Id"] : null;
                            if (userIdToken != null) userId = userIdToken.Value<long>();
                            if (!string.IsNullOrEmpty(tokenStr) && userId > 0)
                            {
                                CloudApiClient.SetAuthToken(sessionName, tokenStr, userId);
                            }
                        }
                        catch { /* 忽略解析错误 */ }
                    }

                    return resp.ToJson();
                });

            // cloud_sms_relogin: GET /api/User/SMSReLogin?userName=&version=
            // Header: ValidateCode（服务端已修复为需要 Token/验证码校验）
            ToolRegistry.Register("cloud_sms_relogin",
                "短信重新登录。通过用户名 + 手机号验证码重新登录，服务端要求 Header 携带 ValidateCode。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["validateCode"] = new JObject { ["type"] = "string", ["description"] = "短信验证码（通过 Header ValidateCode 传递）" },
                        ["version"] = new JObject { ["type"] = "string", ["description"] = "客户端版本号（可选）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName", "validateCode" }
                },
                (args) =>
                {
                    string userName = args["userName"] != null ? args["userName"].ToString() : null;
                    string validateCode = args["validateCode"] != null ? args["validateCode"].ToString() : null;
                    string version = args["version"] != null ? args["version"].ToString() : null;
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;

                    var query = new Dictionary<string, string>();
                    query["userName"] = userName;
                    if (version != null) query["version"] = version;

                    var extraHeaders = new Dictionary<string, string>();
                    if (validateCode != null) extraHeaders["ValidateCode"] = validateCode;

                    var resp = SendWithExtraHeaders("GET", "/api/User/SMSReLogin", query, null, null, sessionName, false, extraHeaders);

                    if (resp.StatusCode == 200)
                    {
                        try
                        {
                            var body = JObject.Parse(resp.Body ?? "{}");
                            var token = body["Item1"] != null ? (body["Item1"]["TokenValue"] ?? body["Item1"]["Token"] ?? body["Item1"]["LastToken"]) : null;
                            string tokenStr = token != null ? token.ToString() : null;
                            long userId = 0;
                            var userIdToken = body["Item2"] != null ? body["Item2"]["Id"] : null;
                            if (userIdToken != null) userId = userIdToken.Value<long>();
                            if (!string.IsNullOrEmpty(tokenStr) && userId > 0)
                            {
                                CloudApiClient.SetAuthToken(sessionName, tokenStr, userId);
                            }
                        }
                        catch { /* 忽略解析错误 */ }
                    }

                    return resp.ToJson();
                });

            // cloud_update_token: GET /api/User/UpdateToken
            // 使用当前会话的 UserId + Token 刷新 Token
            ToolRegistry.Register("cloud_update_token",
                "刷新当前会话的 Token。使用已保存的 UserId + Token 调用服务端 UpdateToken，成功后更新会话 Token。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.GetAsync("/api/User/UpdateToken", null, sessionName, withAuth: true);

                    if (resp.StatusCode == 200)
                    {
                        try
                        {
                            var body = JObject.Parse(resp.Body ?? "{}");
                            var token = body["Token"];
                            if (token != null)
                            {
                                string tokenStr = token.ToString();
                                if (!string.IsNullOrEmpty(tokenStr))
                                {
                                    long userId = SessionState.CurrentUserId;
                                    CloudApiClient.SetAuthToken(sessionName, tokenStr, userId);
                                }
                            }
                        }
                        catch { /* 忽略解析错误 */ }
                    }

                    return resp.ToJson();
                });

            // cloud_client_quit: GET /api/User/ClientQuit
            ToolRegistry.Register("cloud_client_quit",
                "客户端退出登录。清除服务端 Token 记录，但不清除本地会话状态。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.GetAsync("/api/User/ClientQuit", null, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_register: POST /api/User/Register
            // Body: User JSON；Header: ValidateCode + MachineCode
            ToolRegistry.Register("cloud_register",
                "注册新用户。请求体为 User 对象 JSON，需通过 validateCode 参数传入手机号验证码（Header ValidateCode）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["password"] = new JObject { ["type"] = "string", ["description"] = "密码（明文，服务端加盐存储）" },
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号" },
                        ["email"] = new JObject { ["type"] = "string", ["description"] = "邮箱（可选）" },
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "显示名称（可选）" },
                        ["validateCode"] = new JObject { ["type"] = "string", ["description"] = "手机号验证码（通过 Header ValidateCode 传递）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName", "password", "phone", "validateCode" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    string validateCode = args["validateCode"] != null ? args["validateCode"].ToString() : null;

                    var body = new JObject();
                    if (args["userName"] != null) body["UserName"] = args["userName"].ToString();
                    if (args["password"] != null) body["password"] = args["password"].ToString();
                    if (args["phone"] != null) body["Phone"] = args["phone"].ToString();
                    if (args["email"] != null) body["Email"] = args["email"].ToString();
                    if (args["name"] != null) body["Name"] = args["name"].ToString();

                    var extraHeaders = new Dictionary<string, string>();
                    if (validateCode != null) extraHeaders["ValidateCode"] = validateCode;

                    var resp = SendWithExtraHeaders("POST", "/api/User/Register", null,
                        Encoding.UTF8.GetBytes(body.ToString(Newtonsoft.Json.Formatting.None)),
                        "application/json", sessionName, false, extraHeaders);
                    return resp.ToJson();
                });

            // cloud_find_password: GET /api/User/FindPassword?userName=&password=
            // Header: ValidateCode + MachineCode
            ToolRegistry.Register("cloud_find_password",
                "找回密码。通过用户名 + 手机号验证码重置密码。需先调用 cloud_get_validate_code 获取验证码。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["password"] = new JObject { ["type"] = "string", ["description"] = "新密码" },
                        ["validateCode"] = new JObject { ["type"] = "string", ["description"] = "手机号验证码（通过 Header ValidateCode 传递）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName", "password", "validateCode" }
                },
                (args) =>
                {
                    string userName = args["userName"] != null ? args["userName"].ToString() : null;
                    string password = args["password"] != null ? args["password"].ToString() : null;
                    string validateCode = args["validateCode"] != null ? args["validateCode"].ToString() : null;
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;

                    var query = new Dictionary<string, string>
                    {
                        { "userName", userName },
                        { "password", password }
                    };

                    var extraHeaders = new Dictionary<string, string>();
                    if (validateCode != null) extraHeaders["ValidateCode"] = validateCode;

                    var resp = SendWithExtraHeaders("GET", "/api/User/FindPassword", query, null, null, sessionName, false, extraHeaders);
                    return resp.ToJson();
                });

            // cloud_get_validate_code: GET /api/User/GetValidateCode?phone=&smsTemplate=
            ToolRegistry.Register("cloud_get_validate_code",
                "获取手机号验证码。服务端生成验证码并通过短信通道发送（开发环境记录到 DB）。响应体不含验证码本身。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号" },
                        ["smsTemplate"] = new JObject { ["type"] = "string", ["description"] = "短信模板（可选）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "phone" }
                },
                (args) =>
                {
                    string phone = args["phone"] != null ? args["phone"].ToString() : null;
                    string smsTemplate = args["smsTemplate"] != null ? args["smsTemplate"].ToString() : null;
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;

                    var query = new Dictionary<string, string> { { "phone", phone } };
                    if (smsTemplate != null) query["smsTemplate"] = smsTemplate;

                    var resp = CloudApiClient.GetAsync("/api/User/GetValidateCode", query, sessionName, withAuth: false);
                    return resp.ToJson();
                });

            // cloud_wechat_login: GET /api/User/WechatLogin?code=&state=&version=
            ToolRegistry.Register("cloud_wechat_login",
                "微信登录。MVP 阶段服务端未配置第三方登录，返回 {error: '未配置第三方登录'}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["code"] = new JObject { ["type"] = "string", ["description"] = "微信授权 code" },
                        ["state"] = new JObject { ["type"] = "string", ["description"] = "state 参数" },
                        ["version"] = new JObject { ["type"] = "string", ["description"] = "客户端版本号（可选）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "code" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["code"] != null) query["code"] = args["code"].ToString();
                    if (args["state"] != null) query["state"] = args["state"].ToString();
                    if (args["version"] != null) query["version"] = args["version"].ToString();

                    var resp = CloudApiClient.GetAsync("/api/User/WechatLogin", query, sessionName, withAuth: false);
                    return resp.ToJson();
                });

            // cloud_qq_login: GET /api/User/QQLogin?code=&state=&version=
            ToolRegistry.Register("cloud_qq_login",
                "QQ 登录。MVP 阶段服务端未配置第三方登录，返回 {error: '未配置第三方登录'}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["code"] = new JObject { ["type"] = "string", ["description"] = "QQ 授权 code" },
                        ["state"] = new JObject { ["type"] = "string", ["description"] = "state 参数" },
                        ["version"] = new JObject { ["type"] = "string", ["description"] = "客户端版本号（可选）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "code" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["code"] != null) query["code"] = args["code"].ToString();
                    if (args["state"] != null) query["state"] = args["state"].ToString();
                    if (args["version"] != null) query["version"] = args["version"].ToString();

                    var resp = CloudApiClient.GetAsync("/api/User/QQLogin", query, sessionName, withAuth: false);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // SubTask 4.3: 用户/团队模块工具（10 个）
        // 端点分布在 /api/User/* 和 /api/Project/* 下。
        // =====================================================================

        private static void RegisterUserTeamTools()
        {
            // cloud_get_user_info: GET /api/User/GetUserById?userId=
            ToolRegistry.Register("cloud_get_user_info",
                "获取用户信息。按 userId 查询用户详情；未传 userId 时使用当前会话用户。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id（可选，默认当前会话用户）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    long userId = 0;
                    if (args["userId"] != null) userId = args["userId"].Value<long>();
                    if (userId <= 0) userId = SessionState.CurrentUserId;

                    var query = new Dictionary<string, string> { { "userId", userId.ToString() } };
                    var resp = CloudApiClient.GetAsync("/api/User/GetUserById", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_user_info: POST /api/User/UpdateUserInfo
            // Body: User JSON；Header: UserId + Token
            ToolRegistry.Register("cloud_update_user_info",
                "更新当前用户资料。请求体为 User 对象 JSON（敏感字段如 Role/IsTeamAdmin 由服务端强制保留原值）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "User 对象 JSON（如 {Name, Email, Phone}）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/User/UpdateUserInfo", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_create_team: POST /api/Project/CreateTeam
            // Body: { teamName, type }
            ToolRegistry.Register("cloud_create_team",
                "创建团队。返回新 Team 对象，服务端会自动为新团队创建 Trial License（7 天有效期）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamName"] = new JObject { ["type"] = "string", ["description"] = "团队名称" },
                        ["type"] = new JObject { ["type"] = "integer", ["description"] = "团队类型（可选，默认 0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamName"] != null) body["teamName"] = args["teamName"].ToString();
                    int type = 0;
                    if (args["type"] != null) type = args["type"].Value<int>();
                    body["type"] = type;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/CreateTeam", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_dismiss_team: POST /api/Project/DismissTeam
            ToolRegistry.Register("cloud_dismiss_team",
                "解散当前用户的团队。仅团队所有者可调用。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/DismissTeam", new JObject(), sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_add_user_to_team: POST /api/Project/AddUserToTeam
            // Body: { UserName }
            ToolRegistry.Register("cloud_add_user_to_team",
                "添加用户到当前团队。仅 TeamAdmin 可调用。请求体 { UserName }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "待添加用户的用户名" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["userName"] != null) body["UserName"] = args["userName"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/AddUserToTeam", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_remove_user_from_team: POST /api/Project/RemoveUserFromTeam
            // Body: { UserName, TeamId? }
            ToolRegistry.Register("cloud_remove_user_from_team",
                "从团队移除用户。仅 TeamAdmin 可调用。请求体 { UserName, TeamId? }，TeamId 省略时使用当前用户团队。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "待移除用户的用户名" },
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 Id（可选，默认当前用户团队）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["userName"] != null) body["UserName"] = args["userName"].ToString();
                    if (args["teamId"] != null) body["TeamId"] = args["teamId"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/RemoveUserFromTeam", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_team_users: GET /api/Project/GetTeamUsers
            ToolRegistry.Register("cloud_get_team_users",
                "获取当前用户团队的所有成员列表。返回 User 数组。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.GetAsync("/api/Project/GetTeamUsers", null, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_invite_user: POST /api/Project/InviteUser
            // Body: { teamId, phone, email, role }
            ToolRegistry.Register("cloud_invite_user",
                "发送团队邀请。仅 TeamAdmin 可调用。返回邀请链接和 Token。请求体 { teamId, phone?, email?, role? }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 Id" },
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "被邀请人手机号（可选）" },
                        ["email"] = new JObject { ["type"] = "string", ["description"] = "被邀请人邮箱（可选）" },
                        ["role"] = new JObject { ["type"] = "integer", ["description"] = "角色（可选，默认 0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].ToString();
                    if (args["phone"] != null) body["phone"] = args["phone"].ToString();
                    if (args["email"] != null) body["email"] = args["email"].ToString();
                    int role = 0;
                    if (args["role"] != null) role = args["role"].Value<int>();
                    body["role"] = role;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/InviteUser", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_accept_invitation: POST /api/User/AcceptInvitation
            // Body: { inviteToken, userName, password, name?, phone? }
            ToolRegistry.Register("cloud_accept_invitation",
                "接受团队邀请。校验邀请 Token 后创建用户并加入团队。请求体 { inviteToken, userName, password, name?, phone? }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["inviteToken"] = new JObject { ["type"] = "string", ["description"] = "邀请 Token" },
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "新用户用户名" },
                        ["password"] = new JObject { ["type"] = "string", ["description"] = "密码" },
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "显示名称（可选）" },
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号（可选）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "inviteToken", "userName", "password" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["inviteToken"] != null) body["inviteToken"] = args["inviteToken"].ToString();
                    if (args["userName"] != null) body["userName"] = args["userName"].ToString();
                    if (args["password"] != null) body["password"] = args["password"].ToString();
                    if (args["name"] != null) body["name"] = args["name"].ToString();
                    if (args["phone"] != null) body["phone"] = args["phone"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/User/AcceptInvitation", body, sessionName, withAuth: false);
                    return resp.ToJson();
                });

            // cloud_set_user_team_permissions: POST /api/User/SetUserTeamPermissions
            // Body: User JSON（含 Id 和 Permissions）
            ToolRegistry.Register("cloud_set_user_team_permissions",
                "设置团队成员权限。请求体为 User 对象 JSON，需包含 Id 和 Permissions 字段。仅 TeamAdmin 可调用。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "目标用户 Id" },
                        ["permissions"] = new JObject { ["type"] = "string", ["description"] = "权限 JSON 字符串" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userId", "permissions" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["userId"] != null) body["Id"] = args["userId"].Value<long>();
                    if (args["permissions"] != null) body["Permissions"] = args["permissions"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/User/SetUserTeamPermissions", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // SubTask 4.4: 项目模块工具（11 个）
        // 端点均在 /api/Project/* 下。
        // =====================================================================

        private static void RegisterProjectTools()
        {
            // cloud_get_projects: GET /api/Project/GetProjects
            ToolRegistry.Register("cloud_get_projects",
                "获取当前用户团队的所有项目列表。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.GetAsync("/api/Project/GetProjects", null, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_open_project: GET /api/Project/OpenProject?projectId=
            ToolRegistry.Register("cloud_open_project",
                "打开项目。返回 OperationId 和 ServerVersion，用于后续 Push/Pull 操作的并发控制。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    string projectId = args["projectId"] != null ? args["projectId"].ToString() : null;
                    var query = new Dictionary<string, string> { { "projectId", projectId } };

                    var resp = CloudApiClient.GetAsync("/api/Project/OpenProject", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_create_project: POST /api/Project/CreateProject
            // Body: Project JSON
            ToolRegistry.Register("cloud_create_project",
                "创建项目。请求体为 Project 对象 JSON，返回新创建的 Project 对象。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "Project 对象 JSON（如 {Name, Number, Category, Auditee}）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/CreateProject", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_delete_project: POST /api/Project/DeleteProject
            // Body: Guid JSON 字符串
            ToolRegistry.Register("cloud_delete_project",
                "软删除项目（移入回收站）。请求体为 projectId 的 JSON 字符串。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    string projectId = args["projectId"] != null ? args["projectId"].ToString() : "";
                    // 服务端期望 body 为 JSON 字符串 "\"guid\""
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/DeleteProject", projectId, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_restore_projects: POST /api/Project/RestoreProjects
            // Body: JArray of GUID 字符串
            ToolRegistry.Register("cloud_restore_projects",
                "从回收站恢复项目。请求体为 GUID 字符串数组。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectIds"] = new JObject
                        {
                            ["type"] = "array",
                            ["items"] = new JObject { ["type"] = "string" },
                            ["description"] = "项目 Id（GUID）数组"
                        },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectIds" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var arr = new JArray();
                    if (args["projectIds"] is JArray ids)
                    {
                        for (int i = 0; i < ids.Count; i++) arr.Add(ids[i].ToString());
                    }
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/RestoreProjects", arr, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_copy_project: POST /api/Project/DuplicateProject
            // Body: { projectId, newName }
            ToolRegistry.Register("cloud_copy_project",
                "复制项目。返回新项目 Id。请求体 { projectId, newName }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "源项目 Id（GUID）" },
                        ["newName"] = new JObject { ["type"] = "string", ["description"] = "新项目名称" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "newName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["newName"] != null) body["newName"] = args["newName"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/DuplicateProject", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_share_project: POST /api/Project/ShareProject
            // Body: { projectId, userIds: [long] }
            ToolRegistry.Register("cloud_share_project",
                "分享项目给指定用户列表。请求体 { projectId, userIds: [long] }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["userIds"] = new JObject
                        {
                            ["type"] = "array",
                            ["items"] = new JObject { ["type"] = "integer" },
                            ["description"] = "用户 Id 数组"
                        },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "userIds" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["userIds"] is JArray uids) body["userIds"] = uids;
                    else body["userIds"] = new JArray();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/ShareProject", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_project: POST /api/Project/UpdateProject
            ToolRegistry.Register("cloud_update_project",
                "更新项目元数据。请求体为 Project 对象 JSON（需包含 Id）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "Project 对象 JSON（需包含 Id 字段）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/UpdateProject", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_project_members: GET /api/Project/GetProjectUsersWithPic?projectId=
            ToolRegistry.Register("cloud_get_project_members",
                "获取项目成员列表（含头像信息）。返回 { users: [User] }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    string projectId = args["projectId"] != null ? args["projectId"].ToString() : null;
                    var query = new Dictionary<string, string> { { "projectId", projectId } };

                    var resp = CloudApiClient.GetAsync("/api/Project/GetProjectUsersWithPic", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_project_members: POST /api/Project/UpdateProjectMembers
            // Body: Project JSON（Users 数组含成员 Id）
            ToolRegistry.Register("cloud_update_project_members",
                "全量替换项目成员。请求体为 Project 对象 JSON（Users 数组含成员 Id）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "Project 对象 JSON（需包含 Id 和 Users 数组）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/UpdateProjectMembers", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_recycle_projects: GET /api/Project/GetRecycleProjects
            ToolRegistry.Register("cloud_get_recycle_projects",
                "获取回收站项目列表（已软删除的项目）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.GetAsync("/api/Project/GetRecycleProjects", null, sessionName, withAuth: true);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // SubTask 4.5: 表格/文档同步工具（9 个）
        // PushTableQuick/PushDocumentQuick 接收 Protobuf 字节流（application/octet-stream）。
        // 工具支持 base64 字符串传入，或通过 useFixture 参数从 TestFixtures 加载样本。
        // =====================================================================

        private static void RegisterSyncTools()
        {
            // cloud_push_table_quick: POST /api/Project/PushTableQuick
            // Body: PushTable Protobuf 字节流
            ToolRegistry.Register("cloud_push_table_quick",
                "快速推送表格（≤1000 单元格）。请求体为 PushTable Protobuf 字节流。可通过 base64 参数或 useFixture 加载样本。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["base64"] = new JObject { ["type"] = "string", ["description"] = "PushTable Protobuf 字节的 base64 编码（可选）" },
                        ["useFixture"] = new JObject { ["type"] = "string", ["description"] = "使用夹具样本（可选，传 'sample_push_table' 加载内置样本）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    byte[] bytes = ResolveBytes(args);
                    var resp = CloudApiClient.PostBytesAsync("/api/Project/PushTableQuick", bytes, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_push_table: GET /api/Project/PushTable?taskId=&projectId=&tableId=&version=
            // 用于大表格：先 UploadTaskInputFile 上传字节，再 GET 调用
            ToolRegistry.Register("cloud_push_table",
                "推送大表格（>1000 单元格）。需先调用 cloud_upload_task_input_file 上传 Protobuf 字节，再通过 taskId 触发推送。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["taskId"] = new JObject { ["type"] = "integer", ["description"] = "任务 Id（由 cloud_generate_task_id 获取）" },
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["tableId"] = new JObject { ["type"] = "string", ["description"] = "表格 Id（GUID）" },
                        ["version"] = new JObject { ["type"] = "integer", ["description"] = "客户端当前版本号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "taskId", "projectId", "tableId", "version" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["taskId"] != null) query["taskId"] = args["taskId"].Value<long>().ToString();
                    if (args["projectId"] != null) query["projectId"] = args["projectId"].ToString();
                    if (args["tableId"] != null) query["tableId"] = args["tableId"].ToString();
                    if (args["version"] != null) query["version"] = args["version"].Value<int>().ToString();

                    var resp = CloudApiClient.GetAsync("/api/Project/PushTable", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_pull_table: POST /api/Project/PullTable
            // Body: { projectId, tableId, version }
            ToolRegistry.Register("cloud_pull_table",
                "拉取表格。返回 PullTable Protobuf 字节流（Content-Type: application/x-protobuf）。请求体 { projectId, tableId, version }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["tableId"] = new JObject { ["type"] = "string", ["description"] = "表格 Id（GUID）" },
                        ["version"] = new JObject { ["type"] = "integer", ["description"] = "客户端当前版本号（可选，默认 0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "tableId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["tableId"] != null) body["tableId"] = args["tableId"].ToString();
                    int version = 0;
                    if (args["version"] != null) version = args["version"].Value<int>();
                    body["version"] = version;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/PullTable", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_revert_table: POST /api/Project/RevertTable
            // Body: { projectId, tableId, targetVersion }
            ToolRegistry.Register("cloud_revert_table",
                "回滚表格到指定版本。请求体 { projectId, tableId, targetVersion }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["tableId"] = new JObject { ["type"] = "string", ["description"] = "表格 Id（GUID）" },
                        ["targetVersion"] = new JObject { ["type"] = "integer", ["description"] = "目标版本号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "tableId", "targetVersion" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["tableId"] != null) body["tableId"] = args["tableId"].ToString();
                    int targetVersion = 0;
                    if (args["targetVersion"] != null) targetVersion = args["targetVersion"].Value<int>();
                    body["targetVersion"] = targetVersion;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/RevertTable", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_table_timeline: POST /api/Project/GetTableTimeline
            // Body: { projectId, tableId }
            ToolRegistry.Register("cloud_get_table_timeline",
                "获取表格版本时间线。返回 [{ id, version, changeType, createdAt, createdBy }]。请求体 { projectId, tableId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["tableId"] = new JObject { ["type"] = "string", ["description"] = "表格 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "tableId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["tableId"] != null) body["tableId"] = args["tableId"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/GetTableTimeline", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_query_table_versions: POST /api/Project/QueryTableVersions
            // Body: { projectId, tableId }
            ToolRegistry.Register("cloud_query_table_versions",
                "查询表格版本列表。返回 [{ version, changeType, createdAt }]。请求体 { projectId, tableId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["tableId"] = new JObject { ["type"] = "string", ["description"] = "表格 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "tableId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["tableId"] != null) body["tableId"] = args["tableId"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/QueryTableVersions", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_table_revert_diff: POST /api/Project/GetTableRevertDiff
            // Body: { projectId, tableId, targetVersion }
            ToolRegistry.Register("cloud_get_table_revert_diff",
                "获取表格回退差异。返回 { taskId, url }。请求体 { projectId, tableId, targetVersion }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["tableId"] = new JObject { ["type"] = "string", ["description"] = "表格 Id（GUID）" },
                        ["targetVersion"] = new JObject { ["type"] = "integer", ["description"] = "目标版本号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "tableId", "targetVersion" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["tableId"] != null) body["tableId"] = args["tableId"].ToString();
                    int targetVersion = 0;
                    if (args["targetVersion"] != null) targetVersion = args["targetVersion"].Value<int>();
                    body["targetVersion"] = targetVersion;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/GetTableRevertDiff", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_table_columns: POST /api/Project/GetTableColumns
            // Body: { projectId, tableId }
            ToolRegistry.Register("cloud_get_table_columns",
                "获取表格列定义。返回 JArray 或 { data: base64 }。请求体 { projectId, tableId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["tableId"] = new JObject { ["type"] = "string", ["description"] = "表格 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "tableId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["tableId"] != null) body["tableId"] = args["tableId"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/GetTableColumns", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_push_document: POST /api/Project/PushDocumentQuick
            // Body: PushDocument Protobuf 字节流
            ToolRegistry.Register("cloud_push_document",
                "快速推送文档。请求体为 PushDocument Protobuf 字节流。可通过 base64 参数或 useFixture 加载样本。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["base64"] = new JObject { ["type"] = "string", ["description"] = "PushDocument Protobuf 字节的 base64 编码（可选）" },
                        ["useFixture"] = new JObject { ["type"] = "string", ["description"] = "使用夹具样本（可选，传 'sample_push_document' 加载内置样本）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    byte[] bytes = ResolveBytes(args);
                    var resp = CloudApiClient.PostBytesAsync("/api/Project/PushDocumentQuick", bytes, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_pull_document: POST /api/Project/PullDocument
            // Body: { projectId, documentId, version }
            ToolRegistry.Register("cloud_pull_document",
                "拉取文档。返回 PullDocument Protobuf 字节流。请求体 { projectId, documentId, version }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["documentId"] = new JObject { ["type"] = "string", ["description"] = "文档 Id（GUID）" },
                        ["version"] = new JObject { ["type"] = "integer", ["description"] = "客户端当前版本号（可选，默认 0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "documentId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["documentId"] != null) body["documentId"] = args["documentId"].ToString();
                    int version = 0;
                    if (args["version"] != null) version = args["version"].Value<int>();
                    body["version"] = version;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/PullDocument", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_revert_document: POST /api/Project/RevertDocument
            // Body: { projectId, documentId, targetVersion }
            ToolRegistry.Register("cloud_revert_document",
                "回滚文档到指定版本。请求体 { projectId, documentId, targetVersion }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["documentId"] = new JObject { ["type"] = "string", ["description"] = "文档 Id（GUID）" },
                        ["targetVersion"] = new JObject { ["type"] = "integer", ["description"] = "目标版本号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "documentId", "targetVersion" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["documentId"] != null) body["documentId"] = args["documentId"].ToString();
                    int targetVersion = 0;
                    if (args["targetVersion"] != null) targetVersion = args["targetVersion"].Value<int>();
                    body["targetVersion"] = targetVersion;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/RevertDocument", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_document_timeline: POST /api/Project/GetDocumentTimeline
            // Body: { projectId, documentId }
            ToolRegistry.Register("cloud_get_document_timeline",
                "获取文档版本时间线。返回 [{ id, version, changeType, createdAt, createdBy }]。请求体 { projectId, documentId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["documentId"] = new JObject { ["type"] = "string", ["description"] = "文档 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "documentId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["documentId"] != null) body["documentId"] = args["documentId"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/GetDocumentTimeline", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_query_document_versions: POST /api/Project/QueryDocumentVersions
            // Body: { projectId, documentId }
            ToolRegistry.Register("cloud_query_document_versions",
                "查询文档版本列表。返回 [{ version, changeType, createdAt }]。请求体 { projectId, documentId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["documentId"] = new JObject { ["type"] = "string", ["description"] = "文档 Id（GUID）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "documentId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["documentId"] != null) body["documentId"] = args["documentId"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/QueryDocumentVersions", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_document_revert_diff: POST /api/Project/GetDocumentRevertDiff
            // Body: { projectId, documentId, targetVersion }
            ToolRegistry.Register("cloud_get_document_revert_diff",
                "获取文档回退差异。返回 { taskId, url }。请求体 { projectId, documentId, targetVersion }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（GUID）" },
                        ["documentId"] = new JObject { ["type"] = "string", ["description"] = "文档 Id（GUID）" },
                        ["targetVersion"] = new JObject { ["type"] = "integer", ["description"] = "目标版本号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "documentId", "targetVersion" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].ToString();
                    if (args["documentId"] != null) body["documentId"] = args["documentId"].ToString();
                    int targetVersion = 0;
                    if (args["targetVersion"] != null) targetVersion = args["targetVersion"].Value<int>();
                    body["targetVersion"] = targetVersion;

                    var resp = CloudApiClient.PostJsonAsync("/api/Project/GetDocumentRevertDiff", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // SubTask 4.6: 文件存储工具（6 个）
        // UploadFile/DownloadFile 需 FileId Header，使用 SendWithExtraHeaders。
        // =====================================================================

        private static void RegisterFileStorageTools()
        {
            // cloud_upload_file: POST /api/Project/UploadFile
            // Header: FileId；Body: 二进制流；query: projectId?
            ToolRegistry.Register("cloud_upload_file",
                "上传文件附件。通过 FileId 标识文件，请求体为二进制流。可通过 base64 参数传入文件内容。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["fileId"] = new JObject { ["type"] = "string", ["description"] = "文件 Id（GUID，通过 Header FileId 传递）" },
                        ["base64"] = new JObject { ["type"] = "string", ["description"] = "文件内容的 base64 编码" },
                        ["projectId"] = new JObject { ["type"] = "string", ["description"] = "项目 Id（可选，用于存储路径分组）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "fileId", "base64" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    string fileId = args["fileId"] != null ? args["fileId"].ToString() : null;
                    byte[] bytes = ResolveBytes(args);

                    var query = new Dictionary<string, string>();
                    if (args["projectId"] != null) query["projectId"] = args["projectId"].ToString();

                    var extraHeaders = new Dictionary<string, string>();
                    if (fileId != null) extraHeaders["FileId"] = fileId;

                    var resp = SendWithExtraHeaders("POST", "/api/Project/UploadFile", query,
                        bytes ?? new byte[0], "application/octet-stream", sessionName, true, extraHeaders);
                    return resp.ToJson();
                });

            // cloud_download_file: GET /api/Project/DownloadFile
            // Header: FileId
            ToolRegistry.Register("cloud_download_file",
                "下载文件附件。通过 FileId 标识文件，返回二进制流 + FileLength 响应头。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["fileId"] = new JObject { ["type"] = "string", ["description"] = "文件 Id（GUID，通过 Header FileId 传递）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "fileId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    string fileId = args["fileId"] != null ? args["fileId"].ToString() : null;

                    var extraHeaders = new Dictionary<string, string>();
                    if (fileId != null) extraHeaders["FileId"] = fileId;

                    var resp = SendWithExtraHeaders("GET", "/api/Project/DownloadFile", null, null, null, sessionName, true, extraHeaders);
                    return resp.ToJson();
                });

            // cloud_push_image: POST /api/Project/PushImage
            // Body: JObject { Id, ProjectId, Version, FileId?, ZoomFactor?, ... }
            ToolRegistry.Register("cloud_push_image",
                "推送图片元数据版本。请求体 { Id, ProjectId, Version, FileId?, ZoomFactor?, CenterX?, CenterY?, PageSetup?, RotateFlip? }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "图片元数据 JSON（需包含 Id 和 ProjectId）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/PushImage", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_pull_image: POST /api/Project/PullImage
            // Body: JObject { Id, ProjectId, Version }
            ToolRegistry.Register("cloud_pull_image",
                "拉取图片元数据版本。返回 { Result: 'NeedUpdate'|'Latest'|'NotExist', Version, ... }。请求体 { Id, ProjectId, Version }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "图片查询 JSON（需包含 Id 和 ProjectId）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/PullImage", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_push_pdf: POST /api/Project/PushPdf
            ToolRegistry.Register("cloud_push_pdf",
                "推送 PDF 元数据版本。请求体 { Id, ProjectId, Version, FileId? }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "PDF 元数据 JSON（需包含 Id 和 ProjectId）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/PushPdf", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_pull_pdf: POST /api/Project/PullPdf
            ToolRegistry.Register("cloud_pull_pdf",
                "拉取 PDF 元数据版本。返回 { Result: 'NeedUpdate'|'Latest'|'NotExist', Version, ... }。请求体 { Id, ProjectId, Version }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "PDF 查询 JSON（需包含 Id 和 ProjectId）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    object body = args["body"] ?? new JObject();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/PullPdf", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // SubTask 4.7: 异步任务工具（4 个）
        // 端点在 /api/ServerTask/* 下。
        // UploadTaskInputFile 接收分片二进制流，可用 base64 传入。
        // =====================================================================

        private static void RegisterTaskTools()
        {
            // cloud_generate_task_id: GET /api/ServerTask/GenerateTaskId
            ToolRegistry.Register("cloud_generate_task_id",
                "生成异步任务 Id。返回 { taskId: <long> }，用于后续上传任务输入文件和查询任务状态。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.GetAsync("/api/ServerTask/GenerateTaskId", null, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_upload_task_input_file: POST /api/ServerTask/UploadTaskInputFile?taskId=&offset=
            // Body: 二进制分片（最大 2MB/片）
            ToolRegistry.Register("cloud_upload_task_input_file",
                "上传任务输入文件分片。请求体为二进制流，按 offset 随机写入临时文件。可通过 base64 参数传入分片内容。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["taskId"] = new JObject { ["type"] = "integer", ["description"] = "任务 Id" },
                        ["offset"] = new JObject { ["type"] = "integer", ["description"] = "分片偏移量（字节）" },
                        ["base64"] = new JObject { ["type"] = "string", ["description"] = "分片内容的 base64 编码" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "taskId", "offset", "base64" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    long taskId = 0;
                    if (args["taskId"] != null) taskId = args["taskId"].Value<long>();
                    long offset = 0;
                    if (args["offset"] != null) offset = args["offset"].Value<long>();
                    byte[] bytes = ResolveBytes(args);

                    var query = new Dictionary<string, string>
                    {
                        { "taskId", taskId.ToString() },
                        { "offset", offset.ToString() }
                    };

                    // CloudApiClient.PostBytesAsync 不支持 query 参数，故使用 SendWithExtraHeaders
                    // （query 通过 URL 拼接）以传递 taskId / offset。
                    var resp = SendWithExtraHeaders("POST", "/api/ServerTask/UploadTaskInputFile", query,
                        bytes ?? new byte[0], "application/octet-stream", sessionName, true, null);
                    return resp.ToJson();
                });

            // cloud_get_task_running_status: GET /api/ServerTask/GetTaskRunningStatus?taskId=
            ToolRegistry.Register("cloud_get_task_running_status",
                "查询异步任务运行状态。返回 { progressValue, isTaskEnd, isTaskSuccess, isTimeOut, taskDesc, taskResult }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["taskId"] = new JObject { ["type"] = "integer", ["description"] = "任务 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "taskId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    long taskId = 0;
                    if (args["taskId"] != null) taskId = args["taskId"].Value<long>();
                    var query = new Dictionary<string, string> { { "taskId", taskId.ToString() } };

                    var resp = CloudApiClient.GetAsync("/api/ServerTask/GetTaskRunningStatus", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_clear_task_cache_data: GET /api/ServerTask/ClearTaskCacheData?taskId=
            ToolRegistry.Register("cloud_clear_task_cache_data",
                "清理异步任务缓存数据。删除临时文件 + ServerTasks 记录 + 内存状态。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["taskId"] = new JObject { ["type"] = "integer", ["description"] = "任务 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "taskId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    long taskId = 0;
                    if (args["taskId"] != null) taskId = args["taskId"].Value<long>();
                    var query = new Dictionary<string, string> { { "taskId", taskId.ToString() } };

                    var resp = CloudApiClient.GetAsync("/api/ServerTask/ClearTaskCacheData", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // SubTask 4.8: 数据字典工具（3 个）
        // 端点在 /api/DataSource/* 下，白名单路径（无需 Token）。
        // =====================================================================

        private static void RegisterDictionaryTools()
        {
            // cloud_get_table_collect_dic: GET /api/DataSource/TableCollectDic?version=
            ToolRegistry.Register("cloud_get_table_collect_dic",
                "获取表格采集字典。客户端版本 >= 服务端版本时返回 { update: '0' }，否则返回完整字典数据。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["version"] = new JObject { ["type"] = "integer", ["description"] = "客户端当前字典版本号（可选，默认 0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    int version = 0;
                    if (args["version"] != null) version = args["version"].Value<int>();
                    var query = new Dictionary<string, string> { { "version", version.ToString() } };

                    var resp = CloudApiClient.GetAsync("/api/DataSource/TableCollectDic", query, sessionName, withAuth: false);
                    return resp.ToJson();
                });

            // cloud_get_cell_collect_dic: GET /api/DataSource/CellCollectDic?version=
            ToolRegistry.Register("cloud_get_cell_collect_dic",
                "获取单元格采集字典。客户端版本 >= 服务端版本时返回 { update: '0' }，否则返回完整字典数据。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["version"] = new JObject { ["type"] = "integer", ["description"] = "客户端当前字典版本号（可选，默认 0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    int version = 0;
                    if (args["version"] != null) version = args["version"].Value<int>();
                    var query = new Dictionary<string, string> { { "version", version.ToString() } };

                    var resp = CloudApiClient.GetAsync("/api/DataSource/CellCollectDic", query, sessionName, withAuth: false);
                    return resp.ToJson();
                });

            // cloud_get_ledger_validate_dic: GET /api/DataSource/LedgerValidateDic?version=
            ToolRegistry.Register("cloud_get_ledger_validate_dic",
                "获取账簿校验字典。客户端版本 >= 服务端版本时返回 { update: '0' }，否则返回完整字典数据。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["version"] = new JObject { ["type"] = "integer", ["description"] = "客户端当前字典版本号（可选，默认 0）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    int version = 0;
                    if (args["version"] != null) version = args["version"].Value<int>();
                    var query = new Dictionary<string, string> { { "version", version.ToString() } };

                    var resp = CloudApiClient.GetAsync("/api/DataSource/LedgerValidateDic", query, sessionName, withAuth: false);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // SubTask 4.9: 许可/配额工具（5 个）
        // 服务端实际有 5 个 License 端点：Create / Activate / Status / Renew / Deactivate。
        // spec 预期 4 个（cloud_get_subscription_history / cloud_check_quota 无对应服务端端点），
        // 按实际端点注册 5 个工具。
        // =====================================================================

        private static void RegisterLicenseTools()
        {
            // cloud_get_license: GET /api/License/Status
            ToolRegistry.Register("cloud_get_license",
                "获取当前用户 License 状态。返回 PlanType/Seats/EndDate/Activations/daysRemaining/isExpired/inGracePeriod。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var resp = CloudApiClient.GetAsync("/api/License/Status", null, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_create_license: POST /api/License/Create
            // Body: { ownerType, ownerId, planType, seats, maxProjects, maxTemplates, durationYears }
            ToolRegistry.Register("cloud_create_license",
                "创建 License。仅 TeamAdmin/IsSystemAdmin 可调用。返回 { licenseKey, licenseId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["ownerType"] = new JObject { ["type"] = "string", ["description"] = "所有者类型（默认 'Team'）" },
                        ["ownerId"] = new JObject { ["type"] = "string", ["description"] = "所有者 Id（团队 Id）" },
                        ["planType"] = new JObject { ["type"] = "integer", ["description"] = "套餐类型（0=Trial, 1=Basic, 2=Pro）" },
                        ["seats"] = new JObject { ["type"] = "integer", ["description"] = "座位数（默认 5）" },
                        ["maxProjects"] = new JObject { ["type"] = "integer", ["description"] = "最大项目数（默认 10）" },
                        ["maxTemplates"] = new JObject { ["type"] = "integer", ["description"] = "最大模板数（默认 5）" },
                        ["durationYears"] = new JObject { ["type"] = "integer", ["description"] = "有效期年数（默认 1）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "ownerId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    body["ownerType"] = args["ownerType"] != null ? args["ownerType"].ToString() : "Team";
                    if (args["ownerId"] != null) body["ownerId"] = args["ownerId"].ToString();
                    body["planType"] = args["planType"] != null ? args["planType"].Value<int>() : 0;
                    body["seats"] = args["seats"] != null ? args["seats"].Value<int>() : 5;
                    body["maxProjects"] = args["maxProjects"] != null ? args["maxProjects"].Value<int>() : 10;
                    body["maxTemplates"] = args["maxTemplates"] != null ? args["maxTemplates"].Value<int>() : 5;
                    body["durationYears"] = args["durationYears"] != null ? args["durationYears"].Value<int>() : 1;

                    var resp = CloudApiClient.PostJsonAsync("/api/License/Create", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_activate_license: POST /api/License/Activate
            // Body: { licenseKey, machineCode }
            ToolRegistry.Register("cloud_activate_license",
                "激活 License。绑定机器码到 License，更新团队 LicenseDate。请求体 { licenseKey, machineCode }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["licenseKey"] = new JObject { ["type"] = "string", ["description"] = "License Key" },
                        ["machineCode"] = new JObject { ["type"] = "string", ["description"] = "机器码" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "licenseKey", "machineCode" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["licenseKey"] != null) body["licenseKey"] = args["licenseKey"].ToString();
                    if (args["machineCode"] != null) body["machineCode"] = args["machineCode"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/License/Activate", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_renew_license: POST /api/License/Renew
            // Body: { licenseId, years }
            ToolRegistry.Register("cloud_renew_license",
                "续费 License。仅 TeamAdmin/IsSystemAdmin 可调用。请求体 { licenseId, years }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["licenseId"] = new JObject { ["type"] = "integer", ["description"] = "License Id" },
                        ["years"] = new JObject { ["type"] = "integer", ["description"] = "续费年数" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "licenseId", "years" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["licenseId"] != null) body["licenseId"] = args["licenseId"].Value<int>();
                    if (args["years"] != null) body["years"] = args["years"].Value<int>();

                    var resp = CloudApiClient.PostJsonAsync("/api/License/Renew", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_deactivate_license: POST /api/License/Deactivate
            // Body: { licenseId, machineCode }
            ToolRegistry.Register("cloud_deactivate_license",
                "解绑 License 机器码。请求体 { licenseId, machineCode }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["licenseId"] = new JObject { ["type"] = "integer", ["description"] = "License Id" },
                        ["machineCode"] = new JObject { ["type"] = "string", ["description"] = "机器码" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "licenseId", "machineCode" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["licenseId"] != null) body["licenseId"] = args["licenseId"].Value<int>();
                    if (args["machineCode"] != null) body["machineCode"] = args["machineCode"].ToString();

                    var resp = CloudApiClient.PostJsonAsync("/api/License/Deactivate", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // =====================================================================
            // 用户模块 GET 端点
            // =====================================================================

            // cloud_user_name_exists: GET /api/User/UserNameExists?userName={userName}
            ToolRegistry.Register("cloud_user_name_exists",
                "检查用户名是否存在。GET /api/User/UserNameExists?userName={userName}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["userName"] != null) query["userName"] = args["userName"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/UserNameExists", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_check_user_name: GET /api/User/CheckUserName?userName={userName}
            ToolRegistry.Register("cloud_check_user_name",
                "兼容端点检查用户名。GET /api/User/CheckUserName?userName={userName}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["userName"] != null) query["userName"] = args["userName"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/CheckUserName", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_phone_exists: GET /api/User/PhoneExists?phone={phone}
            ToolRegistry.Register("cloud_phone_exists",
                "检查手机号是否已注册。GET /api/User/PhoneExists?phone={phone}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "phone" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["phone"] != null) query["phone"] = args["phone"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/PhoneExists", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_user_by_name: GET /api/User/GetUserByName?userName={userName}
            ToolRegistry.Register("cloud_get_user_by_name",
                "按用户名查询用户。GET /api/User/GetUserByName?userName={userName}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["userName"] != null) query["userName"] = args["userName"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/GetUserByName", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_fuzzy_phone: GET /api/User/GetFuzzyPhone?phone={phone}
            ToolRegistry.Register("cloud_get_fuzzy_phone",
                "获取脱敏手机号。GET /api/User/GetFuzzyPhone?phone={phone}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "phone" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["phone"] != null) query["phone"] = args["phone"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/GetFuzzyPhone", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_username_by_phone: GET /api/User/GetUsernameByPhone?phone={phone}
            ToolRegistry.Register("cloud_get_username_by_phone",
                "按手机号反查用户名。GET /api/User/GetUsernameByPhone?phone={phone}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "phone" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["phone"] != null) query["phone"] = args["phone"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/GetUsernameByPhone", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_code_by_name: GET /api/User/GetCodeByName?userName={userName}
            ToolRegistry.Register("cloud_get_code_by_name",
                "按用户名发送短信验证码。GET /api/User/GetCodeByName?userName={userName}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["userName"] != null) query["userName"] = args["userName"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/GetCodeByName", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_delete_project_code: GET /api/User/GetDeleteProjectValidateCode?phone={phone}
            ToolRegistry.Register("cloud_get_delete_project_code",
                "发送删除项目专用验证码。GET /api/User/GetDeleteProjectValidateCode?phone={phone}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "phone" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["phone"] != null) query["phone"] = args["phone"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/GetDeleteProjectValidateCode", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_reset_password: GET /api/User/ResetPassword?userName=&oldPassword=&newPassword=&code=
            ToolRegistry.Register("cloud_reset_password",
                "重置密码。GET /api/User/ResetPassword?userName={userName}&oldPassword={oldPassword}&newPassword={newPassword}&code={code}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["oldPassword"] = new JObject { ["type"] = "string", ["description"] = "原密码" },
                        ["newPassword"] = new JObject { ["type"] = "string", ["description"] = "新密码" },
                        ["code"] = new JObject { ["type"] = "string", ["description"] = "短信验证码" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName", "oldPassword", "newPassword", "code" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["userName"] != null) query["userName"] = args["userName"].ToString();
                    if (args["oldPassword"] != null) query["oldPassword"] = args["oldPassword"].ToString();
                    if (args["newPassword"] != null) query["newPassword"] = args["newPassword"].ToString();
                    if (args["code"] != null) query["code"] = args["code"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/ResetPassword", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_reset_password_no_sms: GET /api/User/ResetPasswordWithoutSMS?userName=&newPassword=
            ToolRegistry.Register("cloud_reset_password_no_sms",
                "免短信重置密码。GET /api/User/ResetPasswordWithoutSMS?userName={userName}&newPassword={newPassword}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["newPassword"] = new JObject { ["type"] = "string", ["description"] = "新密码" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName", "newPassword" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["userName"] != null) query["userName"] = args["userName"].ToString();
                    if (args["newPassword"] != null) query["newPassword"] = args["newPassword"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/User/ResetPasswordWithoutSMS", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_wechat_relogin: GET /api/User/WechatRelogin
            ToolRegistry.Register("cloud_wechat_relogin",
                "微信重新登录。GET /api/User/WechatRelogin。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    var resp = CloudApiClient.GetAsync("/api/User/WechatRelogin", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_qq_relogin: GET /api/User/QQRelogin
            ToolRegistry.Register("cloud_qq_relogin",
                "QQ 重新登录。GET /api/User/QQRelogin。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    var resp = CloudApiClient.GetAsync("/api/User/QQRelogin", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // =====================================================================
            // 用户模块 POST 端点
            // =====================================================================

            // cloud_single_register: POST /api/User/SingleRegister
            // Body: { userName, password, name, phone, email, role }
            ToolRegistry.Register("cloud_single_register",
                "管理员创建用户。请求体 { userName, password, name, phone, email, role }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userName"] = new JObject { ["type"] = "string", ["description"] = "用户名" },
                        ["password"] = new JObject { ["type"] = "string", ["description"] = "密码" },
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "姓名（可选）" },
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "手机号（可选）" },
                        ["email"] = new JObject { ["type"] = "string", ["description"] = "邮箱（可选）" },
                        ["role"] = new JObject { ["type"] = "string", ["description"] = "角色（可选）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userName", "password" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["userName"] != null) body["userName"] = args["userName"].ToString();
                    if (args["password"] != null) body["password"] = args["password"].ToString();
                    if (args["name"] != null) body["name"] = args["name"].ToString();
                    if (args["phone"] != null) body["phone"] = args["phone"].ToString();
                    if (args["email"] != null) body["email"] = args["email"].ToString();
                    if (args["role"] != null) body["role"] = args["role"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/User/SingleRegister", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_batch_import: POST /api/User/BatchImport
            // Body: { teamId, users:[{userName,password,name,phone}] }
            ToolRegistry.Register("cloud_batch_import",
                "批量导入用户。请求体 { teamId, users:[{userName,password,name,phone}] }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["users"] = new JObject { ["type"] = "array", ["description"] = "用户列表 [{userName,password,name,phone}]" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "users" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["users"] != null) body["users"] = args["users"];
                    var resp = CloudApiClient.PostJsonAsync("/api/User/BatchImport", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_picture: POST /api/User/UpdatePicture
            // Body: { pictureBase64 }
            ToolRegistry.Register("cloud_update_picture",
                "更新头像。请求体 { pictureBase64 }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["pictureBase64"] = new JObject { ["type"] = "string", ["description"] = "头像 Base64 字符串" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "pictureBase64" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["pictureBase64"] != null) body["pictureBase64"] = args["pictureBase64"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/User/UpdatePicture", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_phone_info: POST /api/User/UpdatePhoneInfo
            // Body: { phone, code }
            ToolRegistry.Register("cloud_update_phone_info",
                "更新手机号。请求体 { phone, code }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["phone"] = new JObject { ["type"] = "string", ["description"] = "新手机号" },
                        ["code"] = new JObject { ["type"] = "string", ["description"] = "短信验证码" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "phone", "code" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["phone"] != null) body["phone"] = args["phone"].ToString();
                    if (args["code"] != null) body["code"] = args["code"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/User/UpdatePhoneInfo", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_team_user_permissions: POST /api/User/GetTeamUserPermissions
            // Body: { userId, teamId }
            ToolRegistry.Register("cloud_get_team_user_permissions",
                "查询团队成员权限。请求体 { userId, teamId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id" },
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "userId", "teamId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["userId"] != null) body["userId"] = args["userId"].Value<int>();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/User/GetTeamUserPermissions", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_team_merge_request: POST /api/User/TeamMergeRequest
            // Body: { targetTeamId }
            ToolRegistry.Register("cloud_team_merge_request",
                "团队合并请求。请求体 { targetTeamId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["targetTeamId"] = new JObject { ["type"] = "integer", ["description"] = "目标团队 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "targetTeamId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["targetTeamId"] != null) body["targetTeamId"] = args["targetTeamId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/User/TeamMergeRequest", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // =====================================================================
            // 团队模块 GET 端点
            // =====================================================================

            // cloud_get_user_teams: GET /api/Project/GetUserTeams
            ToolRegistry.Register("cloud_get_user_teams",
                "获取用户所属团队。GET /api/Project/GetUserTeams。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    var resp = CloudApiClient.GetAsync("/api/Project/GetUserTeams", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_team_users_with_pic: GET /api/Project/GetTeamUsersWithPic?teamId={teamId}
            ToolRegistry.Register("cloud_get_team_users_with_pic",
                "获取团队成员含头像。GET /api/Project/GetTeamUsersWithPic?teamId={teamId}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["teamId"] != null) query["teamId"] = args["teamId"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/Project/GetTeamUsersWithPic", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_team_user_groups: GET /api/Project/GetTeamUserGroups?teamId={teamId}
            ToolRegistry.Register("cloud_get_team_user_groups",
                "获取用户分组树。GET /api/Project/GetTeamUserGroups?teamId={teamId}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    if (args["teamId"] != null) query["teamId"] = args["teamId"].ToString();
                    var resp = CloudApiClient.GetAsync("/api/Project/GetTeamUserGroups", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_pending_invitations: GET /api/Project/GetPendingInvitations
            ToolRegistry.Register("cloud_get_pending_invitations",
                "获取待处理邀请列表。GET /api/Project/GetPendingInvitations。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    var resp = CloudApiClient.GetAsync("/api/Project/GetPendingInvitations", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_templates: GET /api/Project/GetTemplates
            ToolRegistry.Register("cloud_get_templates",
                "获取模板项目。GET /api/Project/GetTemplates。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    var resp = CloudApiClient.GetAsync("/api/Project/GetTemplates", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_team_payed_projects: GET /api/Project/GetTeamPayedProjects
            ToolRegistry.Register("cloud_get_team_payed_projects",
                "获取团队付费项目。GET /api/Project/GetTeamPayedProjects。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var query = new Dictionary<string, string>();
                    var resp = CloudApiClient.GetAsync("/api/Project/GetTeamPayedProjects", query, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // =====================================================================
            // 团队模块 POST 端点
            // =====================================================================

            // cloud_update_team_name: POST /api/Project/UpdateTeamName
            // Body: { teamId, name }
            ToolRegistry.Register("cloud_update_team_name",
                "修改团队名称。请求体 { teamId, name }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "新团队名称" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "name" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["name"] != null) body["name"] = args["name"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/UpdateTeamName", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_current_team: POST /api/Project/UpdateCurrentTeam
            // Body: { teamId }
            ToolRegistry.Register("cloud_update_current_team",
                "切换当前团队。请求体 { teamId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/UpdateCurrentTeam", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_add_user_group: POST /api/Project/AddUserGroup
            // Body: { teamId, groupName }
            ToolRegistry.Register("cloud_add_user_group",
                "新增用户分组。请求体 { teamId, groupName }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["groupName"] = new JObject { ["type"] = "string", ["description"] = "分组名称" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "groupName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["groupName"] != null) body["groupName"] = args["groupName"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/AddUserGroup", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_move_user_to_group: POST /api/Project/MoveUserToGroup
            // Body: { teamId, userId, groupId }
            ToolRegistry.Register("cloud_move_user_to_group",
                "移动用户至分组。请求体 { teamId, userId, groupId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id" },
                        ["groupId"] = new JObject { ["type"] = "integer", ["description"] = "分组 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "userId", "groupId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["userId"] != null) body["userId"] = args["userId"].Value<int>();
                    if (args["groupId"] != null) body["groupId"] = args["groupId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/MoveUserToGroup", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_delete_user_group: POST /api/Project/DeleteUserGroup
            // Body: { teamId, groupId }
            ToolRegistry.Register("cloud_delete_user_group",
                "删除用户分组。请求体 { teamId, groupId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["groupId"] = new JObject { ["type"] = "integer", ["description"] = "分组 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "groupId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["groupId"] != null) body["groupId"] = args["groupId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/DeleteUserGroup", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_rename_user_group: POST /api/Project/RenameUserGroup
            // Body: { teamId, groupId, newName }
            ToolRegistry.Register("cloud_rename_user_group",
                "重命名分组。请求体 { teamId, groupId, newName }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["groupId"] = new JObject { ["type"] = "integer", ["description"] = "分组 Id" },
                        ["newName"] = new JObject { ["type"] = "string", ["description"] = "新分组名称" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "groupId", "newName" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["groupId"] != null) body["groupId"] = args["groupId"].Value<int>();
                    if (args["newName"] != null) body["newName"] = args["newName"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/RenameUserGroup", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_job_title: POST /api/Project/UpdateJobTitle
            // Body: { teamId, userId, jobTitle }
            ToolRegistry.Register("cloud_update_job_title",
                "更新职位。请求体 { teamId, userId, jobTitle }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id" },
                        ["jobTitle"] = new JObject { ["type"] = "string", ["description"] = "职位名称" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "userId", "jobTitle" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["userId"] != null) body["userId"] = args["userId"].Value<int>();
                    if (args["jobTitle"] != null) body["jobTitle"] = args["jobTitle"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/UpdateJobTitle", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_allow_team_merge: POST /api/Project/AllowTeamMerge
            // Body: { teamId, allow }
            ToolRegistry.Register("cloud_allow_team_merge",
                "允许团队合并。请求体 { teamId, allow }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "integer", ["description"] = "团队 Id" },
                        ["allow"] = new JObject { ["type"] = "boolean", ["description"] = "是否允许合并" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "teamId", "allow" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].Value<int>();
                    if (args["allow"] != null) body["allow"] = args["allow"].Value<bool>();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/AllowTeamMerge", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // =====================================================================
            // 项目模块 POST 端点
            // =====================================================================

            // cloud_get_project_dto: POST /api/Project/GetProjectDto
            // Body: { projectId }
            ToolRegistry.Register("cloud_get_project_dto",
                "获取项目详情。请求体 { projectId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "integer", ["description"] = "项目 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/GetProjectDto", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_get_project_descendants: POST /api/Project/GetProjectDescendants
            // Body: { projectId }
            ToolRegistry.Register("cloud_get_project_descendants",
                "获取项目后代树。请求体 { projectId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "integer", ["description"] = "项目 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/GetProjectDescendants", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_update_project_version: POST /api/Project/UpdateProjectVersion
            // Body: { projectId }
            ToolRegistry.Register("cloud_update_project_version",
                "递增项目版本。请求体 { projectId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "integer", ["description"] = "项目 Id" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].Value<int>();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/UpdateProjectVersion", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_create_demo: POST /api/Project/CreateDemo
            // Body: { name }
            ToolRegistry.Register("cloud_create_demo",
                "创建演示项目。请求体 { name }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "演示项目名称" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "name" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["name"] != null) body["name"] = args["name"].ToString();
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/CreateDemo", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // =====================================================================
            // 图片/PDF 版本查询
            // =====================================================================

            // cloud_query_image_versions: POST /api/Project/QueryImageVersions
            // Body: { projectId, imageVersions:[{id}] }
            ToolRegistry.Register("cloud_query_image_versions",
                "查询图片版本。请求体 { projectId, imageVersions:[{id}] }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "integer", ["description"] = "项目 Id" },
                        ["imageVersions"] = new JObject { ["type"] = "array", ["description"] = "图片版本 Id 列表（对象数组 [{id}]）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "imageVersions" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].Value<int>();
                    if (args["imageVersions"] != null) body["imageVersions"] = args["imageVersions"];
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/QueryImageVersions", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });

            // cloud_query_pdf_versions: POST /api/Project/QueryPdfVersions
            // Body: { projectId, pdfVersions:[{id}] }
            ToolRegistry.Register("cloud_query_pdf_versions",
                "查询 PDF 版本。请求体 { projectId, pdfVersions:[{id}] }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["projectId"] = new JObject { ["type"] = "integer", ["description"] = "项目 Id" },
                        ["pdfVersions"] = new JObject { ["type"] = "array", ["description"] = "PDF 版本 Id 列表（对象数组 [{id}]）" },
                        ["sessionName"] = new JObject { ["type"] = "string", ["description"] = "会话名称（可选）" }
                    },
                    ["required"] = new JArray { "projectId", "pdfVersions" }
                },
                (args) =>
                {
                    string sessionName = args["sessionName"] != null ? args["sessionName"].ToString() : null;
                    var body = new JObject();
                    if (args["projectId"] != null) body["projectId"] = args["projectId"].Value<int>();
                    if (args["pdfVersions"] != null) body["pdfVersions"] = args["pdfVersions"];
                    var resp = CloudApiClient.PostJsonAsync("/api/Project/QueryPdfVersions", body, sessionName, withAuth: true);
                    return resp.ToJson();
                });
        }

        // =====================================================================
        // 内部辅助方法
        // =====================================================================

        /// <summary>
        /// 从工具参数解析字节数组：优先 base64 参数，其次 useFixture 从 TestFixtures 加载样本。
        /// 两者均未提供时返回空字节数组。
        /// </summary>
        private static byte[] ResolveBytes(JObject args)
        {
            string base64 = args["base64"] != null ? args["base64"].ToString() : null;
            if (!string.IsNullOrEmpty(base64))
            {
                try { return Convert.FromBase64String(base64); }
                catch { /* 解析失败回退到 fixture */ }
            }

            string useFixture = args["useFixture"] != null ? args["useFixture"].ToString() : null;
            if (!string.IsNullOrEmpty(useFixture))
            {
                byte[] fixtureBytes = TestFixtures.GetFixtureBytes(useFixture);
                if (fixtureBytes != null && fixtureBytes.Length > 0) return fixtureBytes;
            }

            return new byte[0];
        }

        /// <summary>
        /// 发送 HTTP 请求并支持额外 Header（如 ValidateCode、FileId）。
        /// CloudApiClient 的公开方法不支持自定义 Header，故在此文件内实现辅助方法，
        /// 复用 CloudApiClient.ServerBaseUrl 和 SessionState 会话管理，并同步捕获响应到 SessionState。
        /// </summary>
        /// <param name="method">HTTP 方法（GET/POST）</param>
        /// <param name="path">相对路径（如 /api/User/Register）</param>
        /// <param name="query">查询参数（可选）</param>
        /// <param name="body">请求体字节（可选，null 表示无 body）</param>
        /// <param name="contentType">Content-Type（可选，默认 application/octet-stream）</param>
        /// <param name="sessionName">会话名（null 表示 main）</param>
        /// <param name="withAuth">是否携带 UserId/Token 头</param>
        /// <param name="extraHeaders">额外 Header（如 ValidateCode、FileId）</param>
        private static TestResponse SendWithExtraHeaders(
            string method, string path,
            Dictionary<string, string> query,
            byte[] body, string contentType,
            string sessionName, bool withAuth,
            Dictionary<string, string> extraHeaders)
        {
            string url = BuildUrl(path, query);
            TestSession session = SessionState.GetOrCreateSession(sessionName);
            HttpClient client = session.HttpClient ?? SessionState.CurrentHttpClient;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                using (HttpRequestMessage req = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    // 认证头
                    if (withAuth)
                    {
                        string token = session.AuthToken;
                        long userId = session.UserId;
                        if (string.IsNullOrEmpty(token))
                        {
                            token = SessionState.CurrentAuthToken;
                            userId = SessionState.CurrentUserId;
                        }
                        if (userId > 0) req.Headers.TryAddWithoutValidation("UserId", userId.ToString());
                        if (!string.IsNullOrEmpty(token)) req.Headers.TryAddWithoutValidation("Token", token);
                    }

                    // 额外头
                    if (extraHeaders != null)
                    {
                        foreach (KeyValuePair<string, string> kv in extraHeaders)
                        {
                            if (!string.IsNullOrEmpty(kv.Key))
                                req.Headers.TryAddWithoutValidation(kv.Key, kv.Value ?? "");
                        }
                    }

                    // 请求体
                    if (body != null && body.Length > 0)
                    {
                        ByteArrayContent content = new ByteArrayContent(body);
                        content.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/octet-stream");
                        req.Content = content;
                    }

                    HttpResponseMessage resp = client.SendAsync(req).Result;
                    return CaptureResponse(resp, sw, method, url, sessionName, session);
                }
            }
            catch (Exception ex)
            {
                return CaptureException(ex, sw, method, url, sessionName, session);
            }
        }

        /// <summary>
        /// 拼接完整 URL：CloudApiClient.ServerBaseUrl + path + 可选 query。
        /// </summary>
        private static string BuildUrl(string path, Dictionary<string, string> query)
        {
            if (string.IsNullOrEmpty(path)) path = "/";
            if (!path.StartsWith("/")) path = "/" + path;
            string url = CloudApiClient.ServerBaseUrl + path;
            if (query != null && query.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
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
        /// 捕获 HttpResponseMessage 为 TestResponse，并同步写入 SessionState.LastResponse
        /// 与对应 session 的 LastResponse（以及 CurrentSession.LastResponse）。
        /// 逻辑与 CloudApiClient.CaptureResponse 一致，确保两种调用路径的响应均可被断言工具读取。
        /// </summary>
        private static TestResponse CaptureResponse(HttpResponseMessage response, Stopwatch sw,
            string method, string url, string sessionName, TestSession session)
        {
            sw.Stop();
            TestResponse tr = new TestResponse
            {
                StatusCode = (int)response.StatusCode,
                RequestUrl = url,
                RequestMethod = method,
                ElapsedMs = sw.ElapsedMilliseconds,
                Timestamp = DateTime.Now
            };

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
                try { tr.Body = response.Content.ReadAsStringAsync().Result; }
                catch { tr.Body = ""; }
            }

            SessionState.LastResponse = tr;
            session.LastResponse = tr;
            SessionState.CurrentSession.LastResponse = tr;
            Console.Error.WriteLine("[CloudApi] " + method + " " + url + " -> " + tr.StatusCode + " (" + tr.ElapsedMs + "ms)");
            return tr;
        }

        /// <summary>
        /// 网络异常时构造失败的 TestResponse（StatusCode=0），与 CloudApiClient.CaptureException 一致。
        /// </summary>
        private static TestResponse CaptureException(Exception ex, Stopwatch sw,
            string method, string url, string sessionName, TestSession session)
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
            TestResponse tr = new TestResponse
            {
                StatusCode = 0,
                Body = "{\"error\":\"" + escaped + "\"}",
                ElapsedMs = sw.ElapsedMilliseconds,
                Timestamp = DateTime.Now,
                RequestUrl = url,
                RequestMethod = method
            };
            SessionState.LastResponse = tr;
            session.LastResponse = tr;
            SessionState.CurrentSession.LastResponse = tr;
            Console.Error.WriteLine("[CloudApi] " + method + " " + url + " -> EX " + msg);
            return tr;
        }
    }
}
