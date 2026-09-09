﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditApiServer.Infra;
using AuditApiServer.Services;
using Newtonsoft.Json;

namespace AuditApiServer.Middleware;

/// <summary>
/// 全局 Token 鉴权中间件。
///
/// 修复"全局缺失 Token 校验"Critical 问题：原服务端所有端点仅信任 HTTP UserId Header，
/// 不校验 Token，攻击者设置 UserId Header 即可冒充任意用户。
///
/// 管线位置：应在 TenantIsolationFilter 之前注册，确保下游中间件/端点拿到的 UserId 已通过 Token 校验。
/// 白名单路径（登录/注册/验证码/三方登录/数据字典/Swagger/SignalR）不校验 Token。
/// OPTIONS 预检请求直接放行（CORS）。
/// </summary>
public class TokenAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthMiddleware> _logger;

    /// <summary>/api/User/* 下不校验 Token 的端点片段</summary>
    private static readonly HashSet<string> WhitelistedUserSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "AccountLogin",
        "AccountLoginBySMS",
        "SMSReLogin",
        "GetValidateCode",
        "GetValidateCodeByEmail",
        "GetCodeByName",
        "GetDeleteProjectValidateCode",
        "Register",
        // 安全审计修复（High）：SingleRegister 不再免 Token。该端点仅管理员可用（端点内已校验
        // IsTeamAdmin/IsSystemAdmin），放行白名单会导致仅凭 UserId Header 即可冒充管理员建号。
        // 客户端管理端调用（MCP cloud_single_register 等）均携带 UserId+Token 头，不受影响。
        "FindPassword",
        "ResetPassword",
        "ResetPasswordWithoutSMS",
        "UserNameExists",
        "CheckUserName",
        "AcceptInvitation",
        // UpdateToken 必须放行：Token 刷新端点本身需要用旧 Token 换新 Token，
        // 若由中间件先行校验，过期/失效后永远无法刷新，客户端陷入 401 死锁。
        // 端点内部由 AuthService.UpdateTokenAsync 自行校验旧 Token 合法性。
        "UpdateToken"
    };

    public TokenAuthMiddleware(RequestDelegate next, ILogger<TokenAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx, AuthService authService)
    {
        var path = ctx.Request.Path;

        // 1. OPTIONS 预检请求直接放行（CORS）
        if (HttpMethods.IsOptions(ctx.Request.Method))
        {
            await _next(ctx);
            return;
        }

        // 2. 白名单路径放行
        if (IsWhitelisted(path))
        {
            await _next(ctx);
            return;
        }

        // 3. 解析 UserId / Token
        var userId = HeaderParser.ParseUserId(ctx);
        var token = HeaderParser.ParseToken(ctx);

        if (userId == 0)
        {
            await WriteUnauthorizedAsync(ctx, "无效的 Token");
            return;
        }

        // 4. 调用 AuthService 校验 Token
        bool valid;
        try
        {
            valid = await authService.ValidateTokenAsync(userId, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TokenAuthMiddleware 调用 ValidateTokenAsync 异常: UserId={UserId}", userId);
            await WriteUnauthorizedAsync(ctx, "无效的 Token");
            return;
        }

        if (!valid)
        {
            _logger.LogWarning("Token 校验失败: UserId={UserId} Path={Path}", userId, path);
            await WriteUnauthorizedAsync(ctx, "无效的 Token");
            return;
        }

        // 5. 校验通过，继续管线
        await _next(ctx);
    }

    /// <summary>
    /// 判断请求路径是否在白名单内（不校验 Token）。
    /// </summary>
    private static bool IsWhitelisted(PathString path)
    {
        // /api/User/* 下的白名单端点
        if (path.StartsWithSegments("/api/User", out var remainingUser))
        {
            var segment = (remainingUser.Value ?? string.Empty).TrimStart('/');
            var firstSlash = segment.IndexOf('/');
            if (firstSlash >= 0) segment = segment.Substring(0, firstSlash);

            if (WhitelistedUserSegments.Contains(segment))
            {
                return true;
            }
        }

        // /api/DataSource/ 前缀所有端点（数据字典公开）
        if (path.StartsWithSegments("/api/DataSource"))
        {
            return true;
        }

        // /swagger 前缀（Swagger UI）
        if (path.StartsWithSegments("/swagger"))
        {
            return true;
        }

        // /ChatHub（SignalR 端点，由 Hub 内部鉴权）
        if (path.StartsWithSegments("/ChatHub"))
        {
            return true;
        }

        // 管理后台静态文件和登录页（匿名访问）
        if (path == "/" || path.StartsWithSegments("/index.html") ||
            path.StartsWithSegments("/dashboard.html") ||
            path.StartsWithSegments("/pages/") ||
            path.StartsWithSegments("/css/") ||
            path.StartsWithSegments("/js/"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 写入 401 JSON 错误响应。格式与 ApiResponseHelper.Unauthorized 保持一致。
    /// </summary>
    private static async Task WriteUnauthorizedAsync(HttpContext ctx, string message)
    {
        ctx.Response.Clear();
        ctx.Response.StatusCode = 401;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        var json = JsonConvert.SerializeObject(new { error = message }, ApiResponseHelper.Settings());
        await ctx.Response.WriteAsync(json);
    }
}
