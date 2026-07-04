﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using AuditAI.McpServer.State;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 管理后台 API 调用工具集（Task 3）
    /// 覆盖服务端 /api/Admin/* 共 26 个 HTTP 端点，每个端点注册为一个原子调用工具。
    /// 工具自动使用 SessionState.AdminBaseUrl（默认 8958 端口）+ AdminAuthToken。
    /// 若 AdminAuthToken 为空，先调用 TryAdminLoginAsync 自动登录。
    /// 响应自动捕获到 SessionState.LastResponse，供后续断言工具读取。
    /// </summary>
    public static class AdminApiTools
    {
        /// <summary>
        /// 注册所有管理后台 API 调用工具（6 个分组）。
        /// </summary>
        public static void Register()
        {
            RegisterStatsTools();          // 统计模块（1 个）
            RegisterUserTools();           // 用户管理（6 个）
            RegisterLicenseTools();        // 许可管理（6 个）
            RegisterActivationCodeTools(); // 激活码（5 个）
            RegisterTeamTools();           // 团队与邀请（7 个）
            RegisterPasswordTools();       // 管理员密码修改（1 个）
        }

        // =====================================================================
        // 统计模块（1 个）
        // =====================================================================

        private static void RegisterStatsTools()
        {
            // admin_get_stats: GET /api/Admin/Stats
            ToolRegistry.Register("admin_get_stats",
                "获取管理后台统计数据。返回用户数、团队数、项目数、License 数等汇总指标。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => ExecuteAdmin(() => CloudApiClient.GetAdminAsync("/api/Admin/Stats")));
        }

        // =====================================================================
        // 用户管理（6 个）
        // =====================================================================

        private static void RegisterUserTools()
        {
            // admin_list_users: GET /api/Admin/Users?page=&pageSize=
            ToolRegistry.Register("admin_list_users",
                "分页获取用户列表。支持 page（默认 1）和 pageSize（默认 20）查询参数。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["page"] = new JObject { ["type"] = "integer", ["description"] = "页码（默认 1）" },
                        ["pageSize"] = new JObject { ["type"] = "integer", ["description"] = "每页条数（默认 20）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    var query = new Dictionary<string, string>();
                    int page = args["page"] != null ? args["page"].Value<int>() : 1;
                    int pageSize = args["pageSize"] != null ? args["pageSize"].Value<int>() : 20;
                    query["page"] = page.ToString();
                    query["pageSize"] = pageSize.ToString();
                    return ExecuteAdmin(() => CloudApiClient.GetAdminAsync("/api/Admin/Users", query));
                });

            // admin_create_user: POST /api/Admin/CreateUser
            // Body: User JSON
            ToolRegistry.Register("admin_create_user",
                "创建用户。请求体为 User 对象 JSON（UserName/Password/Phone/Email/Name 等）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "User 对象 JSON" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    object body = args["body"] ?? new JObject();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/CreateUser", body));
                });

            // admin_update_user: POST /api/Admin/UpdateUser
            // Body: User JSON（含 Id）
            ToolRegistry.Register("admin_update_user",
                "更新用户信息。请求体为 User 对象 JSON（需包含 Id 字段）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "User 对象 JSON（含 Id）" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    object body = args["body"] ?? new JObject();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/UpdateUser", body));
                });

            // admin_reset_user_password: POST /api/Admin/ResetUserPassword
            // Body: { userId, newPassword }
            ToolRegistry.Register("admin_reset_user_password",
                "重置用户密码。请求体 { userId, newPassword }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id" },
                        ["newPassword"] = new JObject { ["type"] = "string", ["description"] = "新密码" }
                    },
                    ["required"] = new JArray { "userId", "newPassword" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["userId"] != null) body["userId"] = args["userId"].Value<long>();
                    if (args["newPassword"] != null) body["newPassword"] = args["newPassword"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/ResetUserPassword", body));
                });

            // admin_toggle_user_active: POST /api/Admin/ToggleUserActive
            // Body: { userId }
            ToolRegistry.Register("admin_toggle_user_active",
                "切换用户启用/禁用状态。请求体 { userId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id" }
                    },
                    ["required"] = new JArray { "userId" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["userId"] != null) body["userId"] = args["userId"].Value<long>();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/ToggleUserActive", body));
                });

            // admin_delete_user: POST /api/Admin/DeleteUser
            // Body: { userId }
            ToolRegistry.Register("admin_delete_user",
                "删除用户。请求体 { userId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id" }
                    },
                    ["required"] = new JArray { "userId" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["userId"] != null) body["userId"] = args["userId"].Value<long>();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/DeleteUser", body));
                });
        }

        // =====================================================================
        // 许可管理（6 个）
        // =====================================================================

        private static void RegisterLicenseTools()
        {
            // admin_list_licenses: GET /api/Admin/Licenses?teamId=
            ToolRegistry.Register("admin_list_licenses",
                "获取 License 列表。可通过 teamId 查询参数过滤指定团队的 License。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 Id（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    var query = new Dictionary<string, string>();
                    if (args["teamId"] != null) query["teamId"] = args["teamId"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.GetAdminAsync("/api/Admin/Licenses", query));
                });

            // admin_create_license: POST /api/Admin/CreateLicense
            // Body: License JSON
            ToolRegistry.Register("admin_create_license",
                "创建 License。请求体为 License 对象 JSON（ownerType/ownerId/planType/seats/maxProjects/durationDays 等）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "object", ["description"] = "License 对象 JSON" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    object body = args["body"] ?? new JObject();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/CreateLicense", body));
                });

            // admin_renew_license: POST /api/Admin/RenewLicense
            // Body: { licenseId, months }
            ToolRegistry.Register("admin_renew_license",
                "续费 License。请求体 { licenseId, months }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["licenseId"] = new JObject { ["type"] = "integer", ["description"] = "License Id" },
                        ["months"] = new JObject { ["type"] = "integer", ["description"] = "续费月数" }
                    },
                    ["required"] = new JArray { "licenseId", "months" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["licenseId"] != null) body["licenseId"] = args["licenseId"].Value<int>();
                    if (args["months"] != null) body["months"] = args["months"].Value<int>();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/RenewLicense", body));
                });

            // admin_update_license_quota: POST /api/Admin/UpdateLicenseQuota
            // Body: { licenseId, seats, maxProjects }
            ToolRegistry.Register("admin_update_license_quota",
                "更新 License 配额。请求体 { licenseId, seats, maxProjects }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["licenseId"] = new JObject { ["type"] = "integer", ["description"] = "License Id" },
                        ["seats"] = new JObject { ["type"] = "integer", ["description"] = "座位数" },
                        ["maxProjects"] = new JObject { ["type"] = "integer", ["description"] = "最大项目数" }
                    },
                    ["required"] = new JArray { "licenseId" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["licenseId"] != null) body["licenseId"] = args["licenseId"].Value<int>();
                    if (args["seats"] != null) body["seats"] = args["seats"].Value<int>();
                    if (args["maxProjects"] != null) body["maxProjects"] = args["maxProjects"].Value<int>();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/UpdateLicenseQuota", body));
                });

            // admin_deactivate_machine: POST /api/Admin/DeactivateMachine
            // Body: { machineCode }
            ToolRegistry.Register("admin_deactivate_machine",
                "解绑机器码。请求体 { machineCode }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["machineCode"] = new JObject { ["type"] = "string", ["description"] = "机器码" }
                    },
                    ["required"] = new JArray { "machineCode" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["machineCode"] != null) body["machineCode"] = args["machineCode"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/DeactivateMachine", body));
                });

            // admin_list_expiring_licenses: GET /api/Admin/Licenses/Expiring?days=
            ToolRegistry.Register("admin_list_expiring_licenses",
                "获取即将到期的 License 列表。支持 days 查询参数（默认 30 天内到期的 License）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["days"] = new JObject { ["type"] = "integer", ["description"] = "天数（默认 30）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    int days = args["days"] != null ? args["days"].Value<int>() : 30;
                    var query = new Dictionary<string, string> { { "days", days.ToString() } };
                    return ExecuteAdmin(() => CloudApiClient.GetAdminAsync("/api/Admin/Licenses/Expiring", query));
                });
        }

        // =====================================================================
        // 激活码（5 个）
        // =====================================================================

        private static void RegisterActivationCodeTools()
        {
            // admin_import_activation_codes: POST /api/Admin/ActivationCodes/Import
            // Body: JSON 数组
            ToolRegistry.Register("admin_import_activation_codes",
                "导入激活码。请求体为激活码对象的 JSON 数组。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["body"] = new JObject { ["type"] = "array", ["description"] = "激活码对象 JSON 数组" }
                    },
                    ["required"] = new JArray { "body" }
                },
                (args) =>
                {
                    object body = args["body"] ?? new JArray();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/ActivationCodes/Import", body));
                });

            // admin_generate_activation_codes: POST /api/Admin/ActivationCodes/Generate
            // Body: { count, licenseType, durationDays }
            ToolRegistry.Register("admin_generate_activation_codes",
                "批量生成激活码。请求体 { count, licenseType, durationDays }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["count"] = new JObject { ["type"] = "integer", ["description"] = "生成数量" },
                        ["licenseType"] = new JObject { ["type"] = "integer", ["description"] = "License 类型（0=Trial, 1=Basic, 2=Pro）" },
                        ["durationDays"] = new JObject { ["type"] = "integer", ["description"] = "有效天数" }
                    },
                    ["required"] = new JArray { "count" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["count"] != null) body["count"] = args["count"].Value<int>();
                    if (args["licenseType"] != null) body["licenseType"] = args["licenseType"].Value<int>();
                    if (args["durationDays"] != null) body["durationDays"] = args["durationDays"].Value<int>();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/ActivationCodes/Generate", body));
                });

            // admin_list_activation_codes: GET /api/Admin/ActivationCodes/List?page=&pageSize=
            ToolRegistry.Register("admin_list_activation_codes",
                "分页获取激活码列表。支持 page（默认 1）和 pageSize（默认 20）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["page"] = new JObject { ["type"] = "integer", ["description"] = "页码（默认 1）" },
                        ["pageSize"] = new JObject { ["type"] = "integer", ["description"] = "每页条数（默认 20）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    var query = new Dictionary<string, string>();
                    int page = args["page"] != null ? args["page"].Value<int>() : 1;
                    int pageSize = args["pageSize"] != null ? args["pageSize"].Value<int>() : 20;
                    query["page"] = page.ToString();
                    query["pageSize"] = pageSize.ToString();
                    return ExecuteAdmin(() => CloudApiClient.GetAdminAsync("/api/Admin/ActivationCodes/List", query));
                });

            // admin_disable_activation_code: POST /api/Admin/ActivationCodes/Disable
            // Body: { code }
            ToolRegistry.Register("admin_disable_activation_code",
                "禁用激活码。请求体 { code }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["code"] = new JObject { ["type"] = "string", ["description"] = "激活码字符串" }
                    },
                    ["required"] = new JArray { "code" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["code"] != null) body["code"] = args["code"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/ActivationCodes/Disable", body));
                });

            // admin_delete_activation_code: POST /api/Admin/ActivationCodes/Delete
            // Body: { code }
            ToolRegistry.Register("admin_delete_activation_code",
                "删除激活码。请求体 { code }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["code"] = new JObject { ["type"] = "string", ["description"] = "激活码字符串" }
                    },
                    ["required"] = new JArray { "code" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["code"] != null) body["code"] = args["code"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/ActivationCodes/Delete", body));
                });
        }

        // =====================================================================
        // 团队与邀请（7 个）
        // =====================================================================

        private static void RegisterTeamTools()
        {
            // admin_list_teams: GET /api/Admin/Teams?page=&pageSize=
            ToolRegistry.Register("admin_list_teams",
                "分页获取团队列表。支持 page（默认 1）和 pageSize（默认 20）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["page"] = new JObject { ["type"] = "integer", ["description"] = "页码（默认 1）" },
                        ["pageSize"] = new JObject { ["type"] = "integer", ["description"] = "每页条数（默认 20）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    var query = new Dictionary<string, string>();
                    int page = args["page"] != null ? args["page"].Value<int>() : 1;
                    int pageSize = args["pageSize"] != null ? args["pageSize"].Value<int>() : 20;
                    query["page"] = page.ToString();
                    query["pageSize"] = pageSize.ToString();
                    return ExecuteAdmin(() => CloudApiClient.GetAdminAsync("/api/Admin/Teams", query));
                });

            // admin_create_team: POST /api/Admin/CreateTeam
            // Body: { name, ownerUserId }
            ToolRegistry.Register("admin_create_team",
                "创建团队。请求体 { name, ownerUserId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "团队名称" },
                        ["ownerUserId"] = new JObject { ["type"] = "integer", ["description"] = "所有者用户 Id" }
                    },
                    ["required"] = new JArray { "name", "ownerUserId" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["name"] != null) body["name"] = args["name"].ToString();
                    if (args["ownerUserId"] != null) body["ownerUserId"] = args["ownerUserId"].Value<long>();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/CreateTeam", body));
                });

            // admin_update_team: POST /api/Admin/UpdateTeam
            // Body: { teamId, name }
            ToolRegistry.Register("admin_update_team",
                "更新团队名称。请求体 { teamId, name }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 Id" },
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "新团队名称" }
                    },
                    ["required"] = new JArray { "teamId", "name" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].ToString();
                    if (args["name"] != null) body["name"] = args["name"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/UpdateTeam", body));
                });

            // admin_list_team_members: GET /api/Admin/Teams/{teamId}/Members
            ToolRegistry.Register("admin_list_team_members",
                "获取指定团队的成员列表。路径参数 teamId。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 Id" }
                    },
                    ["required"] = new JArray { "teamId" }
                },
                (args) =>
                {
                    string teamId = args["teamId"] != null ? args["teamId"].ToString() : "";
                    string path = "/api/Admin/Teams/" + Uri.EscapeDataString(teamId) + "/Members";
                    return ExecuteAdmin(() => CloudApiClient.GetAdminAsync(path));
                });

            // admin_remove_team_member: POST /api/Admin/RemoveTeamMember
            // Body: { teamId, userId }
            ToolRegistry.Register("admin_remove_team_member",
                "从团队移除成员。请求体 { teamId, userId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 Id" },
                        ["userId"] = new JObject { ["type"] = "integer", ["description"] = "用户 Id" }
                    },
                    ["required"] = new JArray { "teamId", "userId" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["teamId"] != null) body["teamId"] = args["teamId"].ToString();
                    if (args["userId"] != null) body["userId"] = args["userId"].Value<long>();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/RemoveTeamMember", body));
                });

            // admin_list_invitations: GET /api/Admin/Invitations?teamId=
            ToolRegistry.Register("admin_list_invitations",
                "获取邀请列表。可通过 teamId 查询参数过滤指定团队的邀请。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["teamId"] = new JObject { ["type"] = "string", ["description"] = "团队 Id（可选）" }
                    },
                    ["required"] = new JArray()
                },
                (args) =>
                {
                    var query = new Dictionary<string, string>();
                    if (args["teamId"] != null) query["teamId"] = args["teamId"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.GetAdminAsync("/api/Admin/Invitations", query));
                });

            // admin_revoke_invitation: POST /api/Admin/RevokeInvitation
            // Body: { invitationId }
            ToolRegistry.Register("admin_revoke_invitation",
                "撤销邀请。请求体 { invitationId }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["invitationId"] = new JObject { ["type"] = "string", ["description"] = "邀请 Id" }
                    },
                    ["required"] = new JArray { "invitationId" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["invitationId"] != null) body["invitationId"] = args["invitationId"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/RevokeInvitation", body));
                });
        }

        // =====================================================================
        // 管理员密码修改（1 个）
        // =====================================================================

        private static void RegisterPasswordTools()
        {
            // admin_change_password: POST /api/Admin/ChangePassword
            // Body: { oldPassword, newPassword }
            ToolRegistry.Register("admin_change_password",
                "修改当前管理员密码。请求体 { oldPassword, newPassword }。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["oldPassword"] = new JObject { ["type"] = "string", ["description"] = "原密码" },
                        ["newPassword"] = new JObject { ["type"] = "string", ["description"] = "新密码" }
                    },
                    ["required"] = new JArray { "oldPassword", "newPassword" }
                },
                (args) =>
                {
                    var body = new JObject();
                    if (args["oldPassword"] != null) body["oldPassword"] = args["oldPassword"].ToString();
                    if (args["newPassword"] != null) body["newPassword"] = args["newPassword"].ToString();
                    return ExecuteAdmin(() => CloudApiClient.PostAdminJsonAsync("/api/Admin/ChangePassword", body));
                });
        }

        // =====================================================================
        // 辅助方法
        // =====================================================================

        /// <summary>
        /// 统一执行管理后台 API 调用：若 AdminAuthToken 为空先自动登录，
        /// 调用 action 发起请求，捕获异常并返回结构化 JSON。
        /// 登录失败不中断流程，继续调用 API（让服务端返回 401 以便断言）。
        /// </summary>
        /// <param name="action">发起管理后台请求的委托（返回 Task&lt;TestResponse&gt;）</param>
        /// <returns>结构化 JSON 字符串（TestResponse.ToJson 或错误对象）</returns>
        private static string ExecuteAdmin(Func<Task<TestResponse>> action)
        {
            try
            {
                if (string.IsNullOrEmpty(SessionState.AdminAuthToken))
                {
                    try
                    {
                        string loginErr = TryAdminLoginAsync().GetAwaiter().GetResult();
                        if (loginErr != null)
                        {
                            Console.Error.WriteLine("[AdminApi] 自动登录失败: " + loginErr);
                        }
                    }
                    catch (Exception lex)
                    {
                        Console.Error.WriteLine("[AdminApi] 自动登录异常: " + lex.Message);
                    }
                }
                TestResponse resp = action().GetAwaiter().GetResult();
                return resp != null ? resp.ToJson() : "{\"error\":\"no response\",\"statusCode\":0}";
            }
            catch (Exception ex)
            {
                string msg = EscapeJsonString(ex.Message);
                return "{\"error\":\"" + msg + "\",\"statusCode\":0}";
            }
        }

        /// <summary>
        /// 尝试使用配置的管理后台测试账号登录，提取 Token/UserId 存入 SessionState。
        /// 从 App.config 的 AdminTestUser / AdminTestPassword 读取，未配置时返回错误。
        /// 调用 /api/User/AccountLogin（8958 端口），解析 Item1.TokenValue 与 Item2.Id。
        /// </summary>
        /// <param name="userName">用户名（可选，默认从配置读取）</param>
        /// <param name="password">密码（可选，默认从配置读取）</param>
        /// <returns>null 表示成功，非 null 字符串表示错误消息</returns>
        private static async Task<string> TryAdminLoginAsync(string userName = null, string password = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    try
                    {
                        userName = ConfigurationManager.AppSettings["AdminTestUser"];
                    }
                    catch { /* 读取配置异常 */ }
                }
                if (string.IsNullOrEmpty(password))
                {
                    try
                    {
                        password = ConfigurationManager.AppSettings["AdminTestPassword"];
                    }
                    catch { /* 读取配置异常 */ }
                }

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    return "AdminTestUser/AdminTestPassword not configured in App.config";
                }

                // 读取 machineCode（首次登录会自动绑定）
                string machineCode = "TEST-MC-MCP-001";
                try
                {
                    string configMachineCode = ConfigurationManager.AppSettings["TestMachineCode"];
                    if (!string.IsNullOrEmpty(configMachineCode)) machineCode = configMachineCode;
                }
                catch { /* 读取配置异常时使用默认值 */ }

                // 明文密码 → Base64(SHA256(明文))，匹配服务端期望的密码格式
                // 注：SHA256.HashData 为 .NET 5+ API，本项目目标为 .NET Framework 4.6.2，
                // 故使用等价的 SHA256.Create().ComputeHash() 写法。
                string hashedPassword;
                using (var sha256 = SHA256.Create())
                {
                    byte[] passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
                    hashedPassword = Convert.ToBase64String(passwordHash);
                }

                var query = new Dictionary<string, string>
                {
                    { "userName", userName },
                    { "password", hashedPassword },
                    { "version", "1" },
                    { "hasProcess", "0" },
                    { "machineCode", machineCode }
                };

                TestResponse resp = await CloudApiClient.GetAdminAsync("/api/User/AccountLogin", query).ConfigureAwait(false);
                if (resp.StatusCode != 200)
                {
                    return "Admin login HTTP " + resp.StatusCode + ": " + (resp.Body ?? "");
                }

                var body = JObject.Parse(resp.Body ?? "{}");
                var token = body["Item1"] != null
                    ? (body["Item1"]["TokenValue"] ?? body["Item1"]["Token"] ?? body["Item1"]["LastToken"])
                    : null;
                string tokenStr = token != null ? token.ToString() : null;
                long userId = 0;
                var userIdToken = body["Item2"] != null ? body["Item2"]["Id"] : null;
                if (userIdToken != null) userId = userIdToken.Value<long>();

                if (string.IsNullOrEmpty(tokenStr) || userId <= 0)
                {
                    return "Admin login response missing Token/UserId";
                }

                SessionState.AdminAuthToken = tokenStr;
                SessionState.AdminUserId = userId;
                return null;
            }
            catch (Exception ex)
            {
                return "Admin login exception: " + ex.Message;
            }
        }

        /// <summary>
        /// 转义字符串以便安全嵌入 JSON 字符串字面量。
        /// 与 CloudApiClient.CaptureException 的转义逻辑保持一致。
        /// </summary>
        private static string EscapeJsonString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }
}
