﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditApiServer.Data;
using AuditApiServer.Infra;
using Newtonsoft.Json;

namespace AuditApiServer.Middleware;

/// <summary>
/// 资源配额校验中间件。阶段 3 Task 12 实现。
///
/// 仅在以下路径生效（方法必须为 POST）：
///   - /api/Project/CreateProject：校验当前 TeamId 的项目数 &lt; Teams.MaxProjects
///   - /api/Project/AddUserToTeam：校验当前 TeamId 的成员数 &lt; Licenses.Seats
///   - /api/User/BatchImport：校验当前 TeamId 的成员数 &lt; Licenses.Seats
///
/// /api/User/AcceptInvitation 是匿名端点，中间件无法解析 TeamId，座位数校验在端点内部完成。
///
/// 超出限额返回 429：
///   {"error":"quota_exceeded","message":"超过资源限额(项目数/座位数)"}
///
/// 豁免规则：
///   - /api/Admin/* 路径跳过
///   - IsSystemAdmin 用户跳过
/// </summary>
public class QuotaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<QuotaMiddleware> _logger;

    public QuotaMiddleware(RequestDelegate next, ILogger<QuotaMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx, LicenseRepository licenseRepo, UserRepository userRepo)
    {
        // 仅对 POST 请求生效
        if (!HttpMethods.IsPost(ctx.Request.Method))
        {
            await _next(ctx);
            return;
        }

        var path = ctx.Request.Path.Value ?? "";

        // 管理面板 /api/Admin/* 跳过配额校验
        if (path.StartsWith("/api/Admin/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(ctx);
            return;
        }

        // 系统管理员跳过配额校验（允许上传模板等操作）
        var userId = HeaderParser.ParseUserId(ctx);
        if (userId > 0)
        {
            var user = await userRepo.GetByIdAsync(userId);
            if (user != null && user.IsSystemAdmin)
            {
                await _next(ctx);
                return;
            }
        }

        // 路径分类：项目数配额 / 座位数配额 / 其他
        bool isCreateProject = path.Equals("/api/Project/CreateProject", StringComparison.OrdinalIgnoreCase);
        // 安全审计修复：AddUserToTeam 实际位于 /api/Project/* 下（端点定义见 Program.cs），原路径 /api/User/AddUserToTeam 永不匹配。
        // /api/User/AcceptInvitation 是匿名端点，中间件无法解析 TeamId，座位数校验在端点内部完成（端点修复由其他子代理处理）。
        bool isMemberAdd = path.Equals("/api/Project/AddUserToTeam", StringComparison.OrdinalIgnoreCase)
                        || path.Equals("/api/User/BatchImport", StringComparison.OrdinalIgnoreCase);

        if (!isCreateProject && !isMemberAdd)
        {
            await _next(ctx);
            return;
        }

        // 解析 TeamId：优先从 TenantContextAccessor 读取，缺失则回退到 LicenseRepository 查询
        var tenant = TenantContextAccessor.Current;
        var userId2 = tenant?.UserId ?? HeaderParser.ParseUserId(ctx);
        var teamId = tenant?.TeamId;
        if (string.IsNullOrEmpty(teamId) && userId2 > 0)
        {
            teamId = await licenseRepo.GetTeamIdByUserIdAsync(userId2);
        }

        // 未解析到团队，放行交由端点处理（避免重复 401/403）
        if (string.IsNullOrEmpty(teamId))
        {
            await _next(ctx);
            return;
        }

        if (isCreateProject)
        {
            var maxProjects = await licenseRepo.GetTeamMaxProjectsAsync(teamId!);
            // MaxProjects 未配置视为不限
            if (maxProjects.HasValue)
            {
                var count = await licenseRepo.CountTeamProjectsAsync(teamId!);
                if (count >= maxProjects.Value)
                {
                    _logger.LogWarning("配额超限：CreateProject TeamId={TeamId} Count={Count} Max={Max}",
                        teamId, count, maxProjects.Value);
                    await WriteQuotaExceededAsync(ctx);
                    return;
                }
            }
        }
        else // isMemberAdd
        {
            // 查询当前团队的 License（不限 EndDate，方便邀请流程在宽限期也能继续）
            var license = await licenseRepo.GetActiveLicenseByOwnerAsync("Team", teamId!);
            // License 不存在则不校验座位数，交由 LicenseMiddleware / 端点处理
            if (license != null)
            {
                var memberCount = await licenseRepo.CountTeamMembersAsync(teamId!);
                if (memberCount >= license.Seats)
                {
                    _logger.LogWarning("配额超限：AddUser TeamId={TeamId} Members={Members} Seats={Seats}",
                        teamId, memberCount, license.Seats);
                    await WriteQuotaExceededAsync(ctx);
                    return;
                }
            }
        }

        await _next(ctx);
    }

    /// <summary>
    /// 写入 429 配额超限响应。格式与 ApiResponseHelper 保持一致。
    /// </summary>
    private static async Task WriteQuotaExceededAsync(HttpContext ctx)
    {
        ctx.Response.Clear();
        ctx.Response.StatusCode = 429;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        var payload = new
        {
            error = "quota_exceeded",
            message = "超过资源限额(项目数/座位数)"
        };
        var json = JsonConvert.SerializeObject(payload, ApiResponseHelper.Settings());
        await ctx.Response.WriteAsync(json);
    }
}
