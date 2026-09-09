﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditApiServer.Data;
using AuditApiServer.Infra;
using Newtonsoft.Json;

namespace AuditApiServer.Middleware;

/// <summary>
/// License 校验中间件。阶段 3 Task 11 实现。
///
/// 管线路径作用于 /api/Project/*、/api/ServerTask/* 和 /api/User/BatchImport；其余路径放行。
/// /api/User/AcceptInvitation 由匿名用户调用，中间件无法解析 TeamId，License 校验在端点内部完成。
/// 校验逻辑：
///   1. 从 HeaderParser 获取 UserId
///   2. 经 TenantContextAccessor 取 TeamId（由前置的 TenantIsolationFilter 写入），缺失时回退到 LicenseRepository 查询
///   3. 通过 OwnerId=TeamId 查询 Licenses 表
///   4. License 不存在或 EndDate &lt; Now - 7 天（超过宽限期）→ 402 license_expired
///   5. EndDate &lt; Now 但在 7 天宽限期内：GET 放行，POST/PUT/DELETE → 402 license_grace_period
///   6. 否则放行
///
/// 白名单例外路径（在 /api/Project/* 范围内但仍放行）：
///   CreateTeam / GetUserTeams / UpdateCurrentTeam / InviteUser
/// </summary>
public class LicenseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LicenseMiddleware> _logger;

    // License 过期后的 7 天宽限期
    private static readonly TimeSpan GracePeriod = TimeSpan.FromDays(7);

    // /api/Project/* 下需要放行的白名单（路径片段）
    private static readonly HashSet<string> WhitelistedProjectSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreateTeam",
        "GetUserTeams",
        "UpdateCurrentTeam",
        "InviteUser"
    };

    public LicenseMiddleware(RequestDelegate next, ILogger<LicenseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx, LicenseRepository licenseRepo)
    {
        var path = ctx.Request.Path;

        // 管理面板 /api/Admin/* 跳过 License 校验（admin 端点不应受 License 限制）
        if (path.StartsWithSegments("/api/Admin"))
        {
            await _next(ctx);
            return;
        }

        // /api/User/AcceptInvitation 由匿名用户调用，中间件无法解析 TeamId，
        // License 有效性校验在端点内部完成（端点修复由其他子代理处理）。
        if (path.StartsWithSegments("/api/User/AcceptInvitation"))
        {
            await _next(ctx);
            return;
        }

        // 覆盖范围：/api/Project/*、/api/ServerTask/*、/api/User/BatchImport
        var isBatchImport = path.StartsWithSegments("/api/User/BatchImport");
        if (!path.StartsWithSegments("/api/Project", out var remainingProject)
            && !path.StartsWithSegments("/api/ServerTask")
            && !isBatchImport)
        {
            await _next(ctx);
            return;
        }

        // /api/Project/* 下的白名单例外（如 CreateTeam、GetUserTeams、UpdateCurrentTeam、InviteUser）
        // remainingProject 形如 "/CreateTeam"
        if (path.StartsWithSegments("/api/Project"))
        {
            var segment = (remainingProject.Value ?? string.Empty).TrimStart('/');
            // 取首个路径片段（兼容尾部斜杠）
            var firstSlash = segment.IndexOf('/');
            if (firstSlash >= 0) segment = segment.Substring(0, firstSlash);
            if (WhitelistedProjectSegments.Contains(segment))
            {
                await _next(ctx);
                return;
            }
        }

        var userId = HeaderParser.ParseUserId(ctx);
        // V2-M-28 修复：非白名单路径下未登录请求（userId=0）拒绝访问，返回 401 Unauthorized。
        // 白名单路径（CreateTeam/GetUserTeams/UpdateCurrentTeam/InviteUser）与 AcceptInvitation 已在上方放行；
        // 此分支仅处理 /api/Project/*、/api/ServerTask/*、/api/User/BatchImport 中的非白名单端点，
        // 匿名请求不应绕过 License 校验，失去纵深防御能力。
        if (userId == 0)
        {
            await WriteJsonAsync(ctx, 401, new
            {
                error = "unauthorized",
                message = "未登录或Token无效"
            });
            return;
        }

        // 优先从 TenantContextAccessor 取 TeamId（前置中间件已写入），缺失则回退查询
        var teamId = TenantContextAccessor.Current?.TeamId;
        if (string.IsNullOrEmpty(teamId))
        {
            teamId = await licenseRepo.GetTeamIdByUserIdAsync(userId);
        }

        // 用户未绑定团队，放行交由端点处理
        if (string.IsNullOrEmpty(teamId))
        {
            await _next(ctx);
            return;
        }

        // 查询该团队的 License（不限 EndDate，方便判断过期/宽限期）
        var license = await licenseRepo.GetActiveLicenseByOwnerAsync("Team", teamId!);

        var now = DateTime.Now;
        // 无 License 或超过 7 天宽限期 → 402 license_expired
        if (license == null || license.EndDate < now - GracePeriod)
        {
            await WriteJsonAsync(ctx, 402, new
            {
                error = "license_expired",
                message = "许可证已过期，请联系管理员续费"
            });
            return;
        }

        // 已过期但在 7 天宽限期内
        if (license.EndDate < now)
        {
            var method = ctx.Request.Method;
            // 读操作（GET）放行；写操作（POST/PUT/DELETE/PATCH）拒绝
            var isWrite = !HttpMethods.IsGet(method);
            if (isWrite)
            {
                await WriteJsonAsync(ctx, 402, new
                {
                    error = "license_grace_period",
                    message = "许可证已过期(宽限期内)，仅可查看不可编辑，请联系管理员续费"
                });
                return;
            }
        }

        // License 有效（或在宽限期内的 GET 请求）→ 放行
        await _next(ctx);
    }

    /// <summary>
    /// 写入 JSON 错误响应并设置状态码。格式与 ApiResponseHelper 保持一致。
    /// </summary>
    private static async Task WriteJsonAsync(HttpContext ctx, int statusCode, object payload)
    {
        ctx.Response.Clear();
        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        var json = JsonConvert.SerializeObject(payload, ApiResponseHelper.Settings());
        await ctx.Response.WriteAsync(json);
    }
}
