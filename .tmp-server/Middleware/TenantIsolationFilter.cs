using AuditApiServer.Data;
using AuditApiServer.Infra;

namespace AuditApiServer.Middleware;

/// <summary>
/// 当前请求的租户上下文。承载 TeamId（GUID 字符串）等信息。
/// 阶段 3 Task 13：基础设施先行，后续 Repository 查询将逐步追加 WHERE TeamId = @currentTeamId 过滤。
/// </summary>
public class TenantContext
{
    /// <summary>当前用户所属团队 Id（GUID 字符串）。null 表示未解析到团队。</summary>
    public string? TeamId { get; set; }

    /// <summary>当前用户 Id（long）。0 表示未登录或 Header 缺失。</summary>
    public long UserId { get; set; }
}

/// <summary>
/// 基于 AsyncLocal&lt;TenantContext&gt; 的请求级租户上下文访问器。
/// 中间件（TenantIsolationFilter）在请求开始时写入 Current，下游 Repository / Service 可读取。
/// </summary>
public static class TenantContextAccessor
{
    private static readonly AsyncLocal<TenantContext?> _current = new();

    /// <summary>当前请求的租户上下文。请求开始时由 TenantIsolationFilter 设置。</summary>
    public static TenantContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

/// <summary>
/// 租户隔离中间件。阶段 3 Task 13 实现。
/// 在请求管线最早阶段（早于 LicenseMiddleware / QuotaMiddleware）解析 UserId → TeamId，
/// 写入 TenantContextAccessor.Current 供下游使用。
/// 仅做基础设施设置，不强制拦截请求；TeamId 解析失败时直接放行，让后续中间件/端点自行处理。
/// </summary>
public class TenantIsolationFilter
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantIsolationFilter> _logger;

    public TenantIsolationFilter(RequestDelegate next, ILogger<TenantIsolationFilter> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx, LicenseRepository licenseRepo)
    {
        // 每次请求重置上下文，避免线程池复用导致残留
        TenantContextAccessor.Current = null;

        var userId = HeaderParser.ParseUserId(ctx);
        if (userId > 0)
        {
            try
            {
                var teamId = await licenseRepo.GetTeamIdByUserIdAsync(userId);
                TenantContextAccessor.Current = new TenantContext
                {
                    UserId = userId,
                    TeamId = teamId
                };
            }
            catch (Exception ex)
            {
                // 解析 TeamId 失败不应阻断请求；记录日志后放行
                _logger.LogWarning(ex, "TenantIsolationFilter 解析 TeamId 失败: UserId={UserId}", userId);
                TenantContextAccessor.Current = new TenantContext { UserId = userId, TeamId = null };
            }
        }

        await _next(ctx);
    }
}
