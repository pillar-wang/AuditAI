﻿﻿using System.IO.Compression;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using Auditai.DTO;
using AuditApiServer.Data;
using AuditApiServer.Hubs;
using AuditApiServer.Infra;
using AuditApiServer.Middleware;
using AuditApiServer.Models;
using AuditApiServer.Services;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// V2-H-17 修复：显式配置 Kestrel 请求体大小上限，防止 DoS / OOM。
// 默认 100MB，可通过 appsettings.json 的 Kestrel:Limits:MaxRequestBodySize 覆盖。
builder.WebHost.UseKestrel((ctx, opts) =>
{
    opts.Limits.MaxRequestBodySize =
        ctx.Configuration.GetValue<long?>("Kestrel:Limits:MaxRequestBodySize") ?? 104_857_600L; // 100MB
});

// 配置 Newtonsoft.Json（与客户端 WebApiClient 的反序列化行为保持一致）
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new DefaultNamingStrategy() // 保持 PascalCase（与客户端默认一致）
        };
        options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 数据访问层
builder.Services.AddSingleton<SqliteStorage>();
builder.Services.AddSingleton<ProjectDbManager>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<TeamRepository>();
builder.Services.AddSingleton<ProjectRepository>();
builder.Services.AddSingleton<TableRepository>();
builder.Services.AddSingleton<DocumentRepository>();
builder.Services.AddSingleton<TaskRepository>();
builder.Services.AddSingleton<FileRepository>();
builder.Services.AddSingleton<DictionaryRepository>();
// 阶段 9 Task 9.4：登录防暴力尝试仓储
builder.Services.AddSingleton<LoginAttemptRepository>();
// 阶段 3 Task 10：License 仓储（供中间件与 License 管理 API 使用）
builder.Services.AddSingleton<LicenseRepository>();
// 阶段 5 Task 16：团队邀请仓储（供 InviteUser / AcceptInvitation 等端点使用）
builder.Services.AddSingleton<InvitationRepository>();
// 阶段 2 Task 4-5：激活码仓储（供注册端点消费激活码使用）
builder.Services.AddSingleton<ActivationCodeRepository>();

// 业务服务层
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<SmsService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<MachineCodeService>();
builder.Services.AddSingleton<ProjectService>();
builder.Services.AddSingleton<TeamService>();
builder.Services.AddSingleton<TaskService>();
builder.Services.AddSingleton<FileStorageService>();
// 阶段 6 新增同步服务
builder.Services.AddSingleton<VersionHistoryService>();
builder.Services.AddSingleton<TableSyncService>();
builder.Services.AddSingleton<DocumentSyncService>();
// 上报审核与归档工作流（Review/Archive）
builder.Services.AddSingleton<ReviewRepository>();
builder.Services.AddSingleton<ArchiveRepository>();
builder.Services.AddSingleton<ReviewService>();
builder.Services.AddSingleton<ArchiveService>();
// 审批流程模板（review-flow-template）
builder.Services.AddSingleton<ReviewFlowRepository>();
builder.Services.AddSingleton<ReviewFlowService>();

// V2-H-24: 后台清理服务（过期 Token / 回收站 30 天 / 临时文件 24 小时）
builder.Services.AddHostedService<CleanupHostedService>();

builder.Services.AddLogging();

// 阶段 9 Task 9.3：响应压缩（GZip）
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = new[] { "application/json", "application/x-protobuf", "text/plain" };
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// 请求解压（支持客户端发送 gzip 压缩的请求体）
builder.Services.AddRequestDecompression();

// 阶段 8 Task 8.5：配置 CORS（允许 SignalR WebSocket 升级，必需 AllowCredentials）
// V2-H-18 修复：从配置读取允许的 Origin 白名单，替代 SetIsOriginAllowed(_ => true) + AllowCredentials 的危险组合。
// 开发环境仍保留宽松策略；生产环境必须显式列出可信源，避免 CSRF 式数据窃取。
// 安全审计修复（Med）：生产环境未配置 CORS 白名单的降级标记（用于启动后输出警告日志）
// 注意：在 AddCors 之前预先读取配置，避免依赖 services.Configure 延迟执行的副作用。
var allowedCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
bool corsDegradedNoCredentials = !builder.Environment.IsDevelopment() && allowedCorsOrigins.Length == 0;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // 安全审计修复（Med）：生产环境未配置白名单时，原实现与开发环境一样
        // "任意 Origin + AllowCredentials"，构成 CSRF 式数据窃取面。
        // 降级为"无凭据的宽松策略"（不 AllowCredentials），并打警告日志提示运维配置白名单；
        // 不改为拒绝启动（避免部署事故）。
        if (builder.Environment.IsDevelopment())
        {
            // 开发环境：允许任意 Origin 并携带凭据（仅用于本地调试）
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else if (allowedCorsOrigins.Length == 0)
        {
            // 生产环境未配置白名单：降级为无凭据的宽松策略
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // 生产环境：仅允许白名单源携带凭据
            policy.WithOrigins(allowedCorsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// 阶段 8 Task 8.1 + 8.4：注册 SignalR 服务
// 使用 Newtonsoft.Json 协议确保与客户端 Microsoft.AspNet.SignalR.Client v2 兼容
// 注意：ASP.NET Core 8 中扩展方法名为 AddNewtonsoftJsonProtocol（旧版本的 UseNewtonsoftJsonProtocol 已弃用）
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
})
.AddNewtonsoftJsonProtocol();

var app = builder.Build();

// 安全审计修复（Med）：生产环境未配置 CORS 白名单时的降级警告
if (corsDegradedNoCredentials)
{
    app.Logger.LogWarning("生产环境未配置 Cors:AllowedOrigins 白名单，CORS 已降级为无凭据的宽松策略（不携带凭据）。请尽快在 appsettings.json 配置可信源列表。");
}

// 初始化 SQLite
var storage = app.Services.GetRequiredService<SqliteStorage>();
storage.Initialize();

// 播种系统模板（从客户端模板目录复制到服务端 _System/Templates/）
var dbManager = app.Services.GetRequiredService<ProjectDbManager>();
dbManager.SeedSystemTemplates(storage);

// 一次性数据迁移：修复历史模板记录的 IsTemplate 和 Type 值
// 1. TemplateUploader 上传的模板因 AddProjectParameters bug 导致 IsTemplate=0 → 修复为 1
// 2. 旧版 SeedSystemTemplates 播种的模板 Type=3 → 修复为 1（ProjectType.Template）
//
// V2-C-07 修复：已删除 "UPDATE Projects SET TeamId=NULL WHERE IsTemplate=1 AND TeamId IS NOT NULL"
// 该语句每次启动无条件执行（无迁移版本标记），会将所有团队模板的 TeamId 置为 NULL，
// 导致团队模板隔离失效（重启后所有团队看到所有模板）+ 模板 .db 文件路径错位。
// 历史系统模板 Type=1 且 TeamId=NULL 不受影响；团队模板 Type=1 且 TeamId=团队Guid 保留 TeamId。
// 若未来需修复被错误设置 TeamId 的系统模板，应增加精确条件（如 WHERE Type=3 AND IsTemplate=1）
// 且配合迁移版本标记（PRAGMA user_version 或 SchemaMigrations 表）确保只执行一次。
try
{
    using var migrateConn = storage.CreateConnection();
    migrateConn.Open();
    using var migrateCmd = migrateConn.CreateCommand();
    migrateCmd.CommandText = @"
        UPDATE Projects SET IsTemplate=1 WHERE Type=1 AND IsTemplate=0;
        UPDATE Projects SET Type=1 WHERE Type=3 AND IsTemplate=1;";
    var fixedRows = migrateCmd.ExecuteNonQuery();
    var migrateLogger = app.Services.GetRequiredService<ILogger<Program>>();
    migrateLogger.LogInformation("模板数据迁移完成，影响 {Rows} 行", fixedRows);
}
catch (Exception migrateEx)
{
    var migrateLogger = app.Services.GetRequiredService<ILogger<Program>>();
    migrateLogger.LogWarning(migrateEx, "模板数据迁移失败（不影响启动）");
}

// 一次性数据迁移：修复历史项目 .db 中 TreeNode.ParentId=0 → NULL
// 原因：CopyTable 旧版将 NULL ParentId 转为整数默认值 0，导致客户端无法识别根节点
try
{
    var projectDataPath = dbManager.GetType().GetField("_basePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(dbManager) as string ?? "Data/Projects";
    var projectsRoot = Path.Combine(AppContext.BaseDirectory, projectDataPath);
    var fixedTotal = 0;
    if (Directory.Exists(projectsRoot))
    {
        foreach (var dbFile in Directory.EnumerateFiles(projectsRoot, "*.db", SearchOption.AllDirectories))
        {
            // 跳过 _System/Templates 下的模板文件
            if (dbFile.Contains("_System")) continue;
            try
            {
                using var fixConn = new SqliteConnection($"Data Source={dbFile};Pooling=False;");
                fixConn.Open();
                using var fixCmd = fixConn.CreateCommand();
                fixCmd.CommandText = "UPDATE TreeNode SET ParentId=NULL WHERE ParentId=0";
                var fixedRows = fixCmd.ExecuteNonQuery();
                if (fixedRows > 0)
                {
                    fixedTotal += fixedRows;
                    app.Logger.LogInformation("项目 .db 迁移: {File} 修复 {Rows} 行 ParentId=0→NULL", Path.GetFileName(dbFile), fixedRows);
                }
                fixConn.Close();
            }
            catch (Exception fixEx)
            {
                app.Logger.LogWarning(fixEx, "项目 .db 迁移失败: {File}", dbFile);
            }
        }
    }
    app.Logger.LogInformation("项目 .db ParentId 迁移完成，共修复 {Total} 行", fixedTotal);
}
catch (Exception projMigrateEx)
{
    app.Logger.LogWarning(projMigrateEx, "项目 .db ParentId 迁移失败（不影响启动）");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 阶段 8 Task 8.5：启用 CORS 中间件（必须在 MapGet/MapHub 之前）
app.UseCors();

// 请求解压中间件（必须在读取请求体之前）
app.UseRequestDecompression();

// 阶段 9 Task 9.3：响应压缩中间件（在路由之前）
app.UseResponseCompression();

// 阶段 9 Task 9.2：全局异常处理中间件（捕获所有未处理异常，返回统一 JSON）
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = ctx.RequestServices.GetService<ILogger<Program>>();
        logger?.LogError(ex, "未处理异常: Path={Path} Method={Method}",
            ctx.Request.Path, ctx.Request.Method);

        ctx.Response.Clear();
        ctx.Response.ContentType = "application/json; charset=utf-8";

        // 区分异常类型
        var (statusCode, payload) = ex switch
        {
            BadHttpRequestException => (400, new { error = "BadRequest", code = 400, message = ex.Message }),
            UnauthorizedAccessException => (401, new { error = "Unauthorized", code = 401, message = ex.Message }),
            InvalidOperationException => (400, new { error = "BusinessError", code = 400, message = ex.Message }),
            // V2-H-02 修复：PushTableQuick/PushDocumentQuick 在 Protobuf 解析失败时 re-throw 为 ArgumentException，
            // 之前落入 default 返回 500，使客户端无法区分"请求格式错误"与"服务端内部故障"。
            ArgumentException => (400, new { error = "BadRequest", code = 400, message = ex.Message }),
            // 安全审计修复（Med）：约 40+ 处 JObject.Parse/DeserializeObject 无保护，非法 JSON 请求体
            // 此前落入 default 返回 500。统一在此转 400 "请求格式错误"（单点修复，无需逐端点包 try/catch）。
            JsonReaderException => (400, new { error = "BadRequest", code = 400, message = "请求格式错误" }),
            System.Text.Json.JsonException => (400, new { error = "BadRequest", code = 400, message = "请求格式错误" }),
            _ => (500, new { error = "InternalServerError", code = 500, message = "服务端内部错误" })
        };

        ctx.Response.StatusCode = statusCode;
        var json = JsonConvert.SerializeObject(payload, ApiResponseHelper.Settings());
        await ctx.Response.WriteAsync(json);
    }
});

// 端口路由安全隔离：确保管理后台和用户API完全分离
app.Use(async (ctx, next) =>
{
    var port = ctx.Connection.LocalPort;
    var path = ctx.Request.Path.ToString().ToLowerInvariant();

    if (port == 8957)
    {
        // 用户API端口：仅允许 /api/* 路径，拒绝管理后台访问
        if (path.StartsWith("/api/admin/") || path == "/" || path.StartsWith("/dashboard") || 
            path.StartsWith("/pages/") || path.StartsWith("/css/") || path.StartsWith("/js/"))
        {
            ctx.Response.StatusCode = 403;
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Forbidden", code = 403, message = "管理后台不可从此端口访问" }));
            return;
        }
    }
    else if (port == 8958)
        {
            var lowerPath = path.ToLowerInvariant();
            var isAdminPath = lowerPath.StartsWith("/api/admin/") || path == "/" || path == "/index.html" ||
                              lowerPath.StartsWith("/dashboard") || lowerPath.StartsWith("/pages/") || 
                              lowerPath.StartsWith("/css/") || lowerPath.StartsWith("/js/");
            var isUserApi = lowerPath.StartsWith("/api/user/accountlogin") || lowerPath.StartsWith("/api/user/updatetoken");
            if (!isAdminPath && !isUserApi)
            {
                ctx.Response.StatusCode = 403;
                await ctx.Response.WriteAsync(JsonConvert.SerializeObject(new { error = "Forbidden", code = 403, message = "用户API不可从此端口访问" }));
                return;
            }
        }

    await next();
});

// 管理面板静态文件托管（wwwroot/）：默认文件 + 静态文件，需在 Token 鉴权之前以放行匿名访问
// 通过 OnStarting 为非 API 响应注入 no-cache 头，避免浏览器缓存旧版前端导致逻辑不生效
app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        if (!ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        }
        return Task.CompletedTask;
    });
    await next();
});
app.UseDefaultFiles();
app.UseStaticFiles();

// Newtonsoft.Json 输出辅助：见 AuditApiServer.Infra.ApiResponseHelper.JsonNet(...)
// （PascalCase，与客户端默认期望一致；Tuple<T1,T2> 序列化为 {Item1, Item2}）

// 阶段 3 Task 13 + 11 + 12：授权中间件链
// 顺序：先 TokenAuth（统一 Token 校验）→ Tenant（解析并写入 TeamId 到 AsyncLocal）→ License（用 TeamId 查 License）→ 最后 Quota（校验配额）
// 安全审计修复：TokenAuthMiddleware 必须最先执行，确保后续中间件/端点拿到的 UserId 已通过 Token 校验。
// 三者均为约定式中间件（构造函数注入 RequestDelegate，InvokeAsync 注入 LicenseRepository）。
app.UseMiddleware<TokenAuthMiddleware>();
app.UseMiddleware<TenantIsolationFilter>();
app.UseMiddleware<LicenseMiddleware>();
app.UseMiddleware<QuotaMiddleware>();

// 客户端调用基础路径为 http://host:8957/api/{Controller}/{Action}
// 故所有路由加 /api 前缀
app.MapGet("/api/User/AccountLogin", async (
    string userName,
    string password,
    string? version,
    string? hasProcess,
    string? machineCode,
    UserRepository userRepo,
    AuthService auth,
    LoginAttemptRepository loginAttempts,
    HttpContext ctx) =>
{
    // V2-C-12 修复：已删除临时调试日志 "AccountLogin RECEIVED: userName=..., password=..., machineCode=..., queryString=..."
    // 该日志记录用户明文密码、machineCode、queryString，日志泄露即等于密码泄露。
    // 登录诊断仅记录 userName/IP/时间/成败，绝不记录 password/queryString/machineCode。

    // 安全审计修复（High）：密码通过 query string 传输（GET 方法），存在被代理日志/浏览器历史泄露的风险。
    // 保守修复：保留 GET 端点（不破坏客户端契约），仅记录警告日志，长期建议客户端改用 POST。
    if (ctx.Request.Query.ContainsKey("password"))
    {
        app.Logger.LogWarning("AccountLogin: 密码通过 query string 传输（建议改为 POST），IP={IP}", ctx.Connection.RemoteIpAddress);
    }

    // 阶段 9 Task 9.4：防暴力登录 - 5 分钟内同 IP 失败 ≥ 5 次则拒绝
    // 本地测试环境（localhost）跳过此限制
    var clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    bool isLocalhost = clientIp == "::1" || clientIp == "127.0.0.1" || clientIp == "localhost";
    if (!isLocalhost)
    {
        var recentFailed = await loginAttempts.CountRecentFailedAttemptsAsync(clientIp, 5);
        if (recentFailed >= 5)
        {
            return ApiResponseHelper.JsonNet(
                new { error = "TooManyAttempts", code = 429, message = "登录失败次数过多，请 15 分钟后再试" },
                429);
        }
    }

    var user = await userRepo.GetByUserNameAsync(userName);
    // V2-C-12 修复：原临时调试日志记录 MachineCode，违反"绝不记录 machineCode"原则，已移除该字段。
    app.Logger.LogInformation("AccountLogin: user found={found}, PasswordHashAlgorithm={alg}",
        user != null, user?.PasswordHashAlgorithm);

    // 客户端修复后直接发送明文，此处不再二次哈希，直接交给 PasswordHasher.VerifyPassword 处理
    var verified = user != null && VerifyPassword(user, password);
    // 临时调试日志：打印验证结果（仅记录成败布尔值，不含敏感字段）
    app.Logger.LogInformation("AccountLogin: password verified={verified}", verified);
    
    if (!verified)
    {
        // 记录失败尝试
        await loginAttempts.RecordAttemptAsync(clientIp, userName, false);
        return ApiResponseHelper.JsonNet(new { error = "用户名或密码错误" }, 401);
    }

    // 记录成功尝试，并清理该 IP 历史失败记录
    await loginAttempts.RecordAttemptAsync(clientIp, userName, true);

    // 阶段 9 Task 9.4：使用 AuthService 生成 Token（自动设置 ExpiresAt）
    var token = await auth.GenerateTokenAsync(user!.Id);

    app.Logger.LogInformation("用户登录成功: {UserName} (Id={Id}) IP={IP}", userName, user!.Id, clientIp);

    // 客户端用 Tuple<UserToken, User> 反序列化，Newtonsoft 默认序列化为 {Item1, Item2}
    return ApiResponseHelper.JsonNet(new { Item1 = token, Item2 = user });
});

app.MapGet("/api/User/UserNameExists", async (string userName, SqliteStorage db) =>
{
    var user = await QueryUserAsync(db, userName);
    return ApiResponseHelper.JsonNet(user != null);
});

// 兼容设计文档里提到的 CheckUserName（实际客户端未调用，但留作扩展）
app.MapGet("/api/User/CheckUserName", async (string userName, SqliteStorage db) =>
{
    var user = await QueryUserAsync(db, userName);
    return ApiResponseHelper.JsonNet(user);
});

app.MapGet("/api/User/UpdateToken", async (HttpContext ctx, AuthService auth) =>
{
    // 客户端会在 Header 中携带 UserId、Token
    var userId = ParseUserId(ctx);
    var oldToken = HeaderParser.ParseToken(ctx);

    // 阶段 9 Task 9.4：基于旧 TokenValue 刷新，校验是否过期/匹配
    var (newToken, errorCode) = await auth.UpdateTokenAsync(userId, oldToken);
    if (newToken == null)
    {
        var message = errorCode == "TokenExpired"
            ? "Token 已过期，请重新登录"
            : "Token 无效";
        return ApiResponseHelper.JsonNet(new { error = errorCode, code = 401, message }, 401);
    }
    return ApiResponseHelper.JsonNet(newToken);
});

app.MapGet("/api/Project/GetUserTeams", async (HttpContext ctx, SqliteStorage db) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    app.Logger.LogInformation("GetUserTeams: userId={UserId}", userId);
    
    var authHeader = ctx.Request.Headers.Authorization.FirstOrDefault();
    app.Logger.LogInformation("GetUserTeams: Authorization header exists={HasAuth}", authHeader != null);
    if (authHeader != null)
    {
        app.Logger.LogInformation("GetUserTeams: Authorization header starts with Bearer={IsBearer}", authHeader.StartsWith("Bearer "));
    }

    if (userId == 0)
    {
        app.Logger.LogWarning("GetUserTeams: userId is 0, returning empty");
        return ApiResponseHelper.JsonNet(new { teams = new List<object>() });
    }

    var teams = await QueryUserTeamsAsync(db, userId);
    app.Logger.LogInformation("GetUserTeams: QueryUserTeamsAsync returned {TeamCount} teams", teams?.Count ?? 0);
    
    if (teams == null)
    {
        return ApiResponseHelper.JsonNet(new { teams = new List<object>() });
    }

    if (teams.Count > 0)
    {
        foreach (var t in teams)
        {
            app.Logger.LogInformation("GetUserTeams: teamId={TeamId}, teamName={TeamName}, type={Type}, Level={Level}, payStatus={PayStatus}", 
                t.Id, t.Name, t.Level, t.Level, t.PayStatus);
        }
    }

    // 客户端 Program.GetUserTeams() 期望 camelCase 字段名：teamId/teamName/type/payStatus/licenseDate/Level/managerId
    // TeamDto 的 PascalCase 字段（Id/Name/Level/PayStatus/LicenseDate/OwnerUserId）不匹配，需手动映射
    var mapped = teams.Select(t => new
    {
        teamId = t.Id.ToString(),
        teamName = t.Name ?? "",
        type = t.Level,           // 客户端用 "type" 字段过滤团队
        Level = t.Level,          // 客户端也读 "Level" 字段
        payStatus = t.PayStatus,
        licenseDate = t.LicenseDate,
        managerId = t.OwnerUserId
    }).ToList();
    
    app.Logger.LogInformation("GetUserTeams: returning {MappedCount} mapped teams", mapped.Count);
    return ApiResponseHelper.JsonNet(new { teams = mapped });
});

app.MapGet("/api/Project/GetProjects", async (HttpContext ctx, ProjectService svc) =>
{
    var projects = await svc.GetAllAsync();
    app.Logger.LogInformation("GetProjects: returning {Count} projects with Users populated", projects.Count);
    foreach (var p in projects)
    {
        var userCount = p.Users?.Count() ?? 0;
        if (userCount > 0)
        {
            app.Logger.LogInformation("  Project {Name}: {UserCount} users", p.Name, userCount);
        }
    }
    return ApiResponseHelper.JsonNet(projects);
});

app.MapGet("/api/Project/OpenProject", async (HttpContext ctx, Guid projectId, ProjectService svc, ProjectRepository projectRepo, ReviewRepository reviewRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 统一访问校验（与 GetProjectDto / PullProjectDirect / Push* 一致）：
    // - 团队项目/模板：TeamId 必须匹配当前租户
    // - 系统下发模板（TeamId=NULL）：仅系统管理员可打开编辑
    // 原实现用 GetByIdAsync（租户过滤 WHERE TeamId=@teamId），系统模板永远查不到 → 误报 403"无权访问该项目"
    // Task 5.5：审批人以只读方式打开待审项目——常规校验失败时追加当前节点审批人放行
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, svc, app.Logger, reviewRepo: reviewRepo, allowPendingReviewerAccess: true);
    if (accessDenied != null) return accessDenied;

    // 阶段 4 Task 4.2：返回真实 OperationId + Project 当前 Version
    // Version 用无租户查询，否则系统模板恒返回 0
    var operationId = await svc.IncrementOperationIdAsync(projectId);
    var project = await projectRepo.GetByIdNoTenantAsync(projectId);
    var serverVersion = project?.Version ?? 0;
    app.Logger.LogInformation("OpenProject: projectId={ProjectId} operationId={OperationId} version={Version}", projectId, operationId, serverVersion);
    return ApiResponseHelper.JsonNet(new { OperationId = operationId, ServerVersion = serverVersion });
});

app.MapGet("/api/Project/GetTeamUsersWithPic", async (HttpContext ctx, SqliteStorage db) =>
{
    var users = await QueryTeamUsersAsync(db);
    // 客户端期望 JObject["users"] 为数组
    return ApiResponseHelper.JsonNet(new { users });
});

// ============= 阶段 2 Task 2.1：认证扩展接口（7 个端点） =============

// GET /api/User/AccountLoginBySMS?phone={phone}&version={version}
// Header: ValidateCode（手机号验证码）
app.MapGet("/api/User/AccountLoginBySMS", async (
    string phone,
    string? version,
    HttpContext ctx,
    UserRepository userRepo,
    AuthService auth,
    SmsService sms,
    MachineCodeService machineCode) =>
{
    var code = HeaderParser.ParseValidateCode(ctx);
    if (string.IsNullOrEmpty(code) || !await sms.ValidateAsync(phone, code))
        return ApiResponseHelper.Unauthorized("验证码错误");

    var user = await userRepo.GetByPhoneAsync(phone);
    if (user == null) return ApiResponseHelper.Unauthorized("用户不存在");

    var token = await auth.GenerateTokenAsync(user.Id);
    var hasProcess = ParseHasProcess(ctx);
    await machineCode.LogLoginAsync(user.Id, version,
        ctx.Connection.RemoteIpAddress?.ToString(),
        HeaderParser.ParseMachineCode(ctx), hasProcess);

    app.Logger.LogInformation("短信登录成功: phone={Phone} (Id={Id})", phone, user.Id);
    // 客户端用 Tuple<UserToken, User> 反序列化 → {Item1, Item2}
    return ApiResponseHelper.JsonNet(new { Item1 = token, Item2 = user });
});

// GET /api/User/SMSReLogin?userName={userName}&version={version}
// 安全审计修复（Critical）：原实现仅凭 userName 即生成 Token，无任何凭证校验。
// 修复：要求 Header 携带 ValidateCode，通过用户绑定手机号校验，校验失败返回 401。
app.MapGet("/api/User/SMSReLogin", async (
    string userName,
    string? version,
    HttpContext ctx,
    UserRepository userRepo,
    AuthService auth,
    SmsService sms,
    MachineCodeService machineCode) =>
{
    var user = await userRepo.GetByUserNameAsync(userName);
    if (user == null) return ApiResponseHelper.Unauthorized("用户不存在");

    // 强制校验手机号验证码，防止任意用户冒充登录
    if (string.IsNullOrEmpty(user.Phone))
        return ApiResponseHelper.Unauthorized("用户未绑定手机号，无法完成短信重新登录");

    var code = HeaderParser.ParseValidateCode(ctx);
    if (string.IsNullOrEmpty(code) || !await sms.ValidateAsync(user.Phone, code))
        return ApiResponseHelper.Unauthorized("无效的验证码");

    var token = await auth.GenerateTokenAsync(user.Id);
    var hasProcess = ParseHasProcess(ctx);
    await machineCode.LogLoginAsync(user.Id, version,
        ctx.Connection.RemoteIpAddress?.ToString(),
        HeaderParser.ParseMachineCode(ctx), hasProcess);

    app.Logger.LogInformation("短信重新登录成功: userName={UserName} (Id={Id})", userName, user.Id);
    return ApiResponseHelper.JsonNet(new { Item1 = token, Item2 = user });
});

// GET /api/User/ClientQuit
// Header: UserId + Token
app.MapGet("/api/User/ClientQuit", async (
    HttpContext ctx,
    AuthService auth,
    MachineCodeService machineCode) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    await auth.ClearTokenAsync(userId);
    await machineCode.LogLogoutAsync(userId);
    return ApiResponseHelper.Ok();
});

// ============= 阶段 2 Task 2.2：用户注册与管理接口（10 个端点） =============

// GET /api/User/PhoneExists?phone={phone}
app.MapGet("/api/User/PhoneExists", async (string phone, UserRepository repo) =>
    ApiResponseHelper.JsonNet(await repo.PhoneExistsAsync(phone)));

// GET /api/User/GetUserById?userId={userId}
app.MapGet("/api/User/GetUserById", async (long userId, UserRepository repo) =>
    ApiResponseHelper.JsonNet(await repo.GetByIdAsync(userId)));

// GET /api/User/GetUserByName?userName={userName}
app.MapGet("/api/User/GetUserByName", async (string userName, UserRepository repo) =>
    ApiResponseHelper.JsonNet(await repo.GetByUserNameAsync(userName)));

// GET /api/User/GetFuzzyPhone?userName={userName}
// 返回脱敏手机号（如 138****0000）
app.MapGet("/api/User/GetFuzzyPhone", async (string userName, UserRepository repo) =>
    ApiResponseHelper.JsonNet(await repo.GetFuzzyPhoneAsync(userName)));

// GET /api/User/GetUsernameByPhone?phone={phone}
app.MapGet("/api/User/GetUsernameByPhone", async (string phone, UserRepository repo) =>
    ApiResponseHelper.JsonNet(await repo.GetUsernameByPhoneAsync(phone)));

// GET /api/User/GetValidateCode?phone={phone}&smsTemplate={smsTemplate}
// 安全审计修复（Critical）：原响应体包含 validateCode 字段，验证码可通过 HTTP 响应泄露。
// 修复：验证码只通过 SmsService 内部记录到 ValidateCodes 表（生产环境对接短信网关），响应体不再包含验证码。
app.MapGet("/api/User/GetValidateCode", async (string phone, string? smsTemplate, SmsService sms) =>
{
    await sms.GenerateCodeAsync(phone);
    return ApiResponseHelper.JsonNet(new { success = true, message = "验证码已发送" });
});

// GET /api/User/GetCodeByName?userName={userName}&smsTemplate={smsTemplate}
// 安全审计修复（Critical）：同 GetValidateCode，移除响应体中的 validateCode 字段。
app.MapGet("/api/User/GetCodeByName", async (string userName, string? smsTemplate, UserRepository repo, SmsService sms) =>
{
    var user = await repo.GetByUserNameAsync(userName);
    if (user?.Phone == null) return ApiResponseHelper.Error("用户手机号未设置");
    await sms.GenerateCodeAsync(user.Phone);
    return ApiResponseHelper.JsonNet(new { success = true, message = "验证码已发送" });
});

// GET /api/User/GetUsernameByEmail?email={email}
app.MapGet("/api/User/GetUsernameByEmail", async (string email, UserRepository repo) =>
    ApiResponseHelper.JsonNet(await repo.GetUsernameByEmailAsync(email)));

// GET /api/User/GetValidateCodeByEmail?email={email}
// 生成验证码并通过邮箱发送。使用邮箱作为 ValidateCodes 的 key。
app.MapGet("/api/User/GetValidateCodeByEmail", async (string email, UserRepository repo, SmsService sms, EmailService emailService) =>
{
    var user = await repo.GetByEmailAsync(email);
    if (user == null) return ApiResponseHelper.Error("该邮箱未注册");
    
    var code = await sms.GenerateCodeAsync(email);
    await emailService.SendVerificationCodeAsync(email, code);
    return ApiResponseHelper.JsonNet(new { success = true, message = "验证码已发送到邮箱" });
});

// GET /api/User/GetDeleteProjectValidateCode?phone={phone}
// 使用 "delete_{phone}" 作为 ValidateCodes 的 key，与删除项目校验流程区分。
// 安全审计修复（Critical）：原直接返回验证码字符串，客户端可读取后任意提交。
// 修复：验证码只通过 SmsService 内部记录，响应体不再包含验证码。
app.MapGet("/api/User/GetDeleteProjectValidateCode", async (string phone, SmsService sms) =>
{
    await sms.GenerateCodeAsync($"delete_{phone}");
    return ApiResponseHelper.JsonNet(new { success = true, message = "验证码已发送" });
});

// POST /api/User/Register
// Body: User 对象 JSON；Header: ActivationCode（激活码）+ MachineCode（设备码）
// 阶段 2 Task 4：激活码注册控制——移除短信验证码，改为激活码校验 + 设备码绑定
app.MapPost("/api/User/Register", async (
    HttpContext ctx,
    UserRepository repo,
    ActivationCodeRepository activationCodeRepo) =>
{
    var bodyText = await ReadBodyAsync(ctx);
    var user = JsonConvert.DeserializeObject<UserDto>(bodyText, ApiResponseHelper.Settings());
    if (user == null) return ApiResponseHelper.Error("请求体无效");
    if (string.IsNullOrEmpty(user.UserName)) return ApiResponseHelper.Error("用户名不能为空");
    if (string.IsNullOrEmpty(user.Phone)) return ApiResponseHelper.Error("手机号不能为空");

    // 阶段 2 Task 4：从 Header 读取激活码与设备码（大小写不敏感）
    var activationCode = ctx.Request.Headers["ActivationCode"].FirstOrDefault() ?? "";
    var machineCode = HeaderParser.ParseMachineCode(ctx) ?? "";

    // 4.4 激活码校验：空值 → 400
    if (string.IsNullOrWhiteSpace(activationCode))
    {
        return ApiResponseHelper.JsonNet(
            new { error = "missing_activation_code", message = "请输入激活码" }, 400);
    }

    // 查询激活码记录：null 或非未使用状态（Status != 0）→ 400
    var codeRecord = await activationCodeRepo.GetByCodeAsync(activationCode.Trim());
    if (codeRecord == null || codeRecord.Status != 0)
    {
        return ApiResponseHelper.JsonNet(
            new { error = "invalid_activation_code", message = "激活码无效或已被使用" }, 400);
    }

    // UserDto.Password 被 [JsonIgnore] 标记（防止响应泄露哈希），反序列化时不会赋值，
    // 需用 JObject 手动提取 password 字段（兼容大小写）。
    var data = JObject.Parse(bodyText);
    var rawPassword = (data["password"] ?? data["Password"])?.ToString();
    if (!string.IsNullOrEmpty(rawPassword))
    {
        // 客户端发送密码时使用 Encrypts.SHA256Encrypt(password, isUrl: false)，
        // 需要进行 URL 编码后再存储，与验证逻辑保持一致。
        var encodedPassword = HttpUtility.UrlEncode(rawPassword);
        var salt = PasswordHasher.GenerateSalt();
        user.Password = PasswordHasher.HashPassword(encodedPassword, salt);
        user.PasswordSalt = salt;
        user.PasswordHashAlgorithm = 1;
    }

    // 4.5 先创建用户拿到 newUser.Id，再消费激活码（事务 + WHERE Status=0 防并发）
    var newId = await repo.CreateAsync(user);
    var consumed = await activationCodeRepo.ConsumeAsync(activationCode.Trim(), newId, machineCode);
    if (!consumed)
    {
        // 并发竞争已消费：回滚刚创建的用户，提示重试
        await repo.DeleteUserAsync(newId);
        // V2-M-35 修复：移除日志中的激活码明文，仅记录操作结果
        app.Logger.LogWarning("激活码并发消费失败，已回滚用户: userName={UserName} id={Id}",
            user.UserName, newId);
        return ApiResponseHelper.JsonNet(
            new { error = "activation_code_consumed", message = "激活码已被使用，请重试" }, 409);
    }

    // 4.6 写入设备码到 Users.MachineCode 列
    await repo.UpdateMachineCodeAsync(newId, machineCode);

    // V2-M-35 修复：移除日志中的激活码明文，仅记录操作结果
    app.Logger.LogInformation("用户注册成功: userName={UserName} id={Id}",
        user.UserName, newId);
    // 客户端 SendAsObject<long> 接收，返回新用户 Id
    return ApiResponseHelper.JsonNet(newId);
});

// POST /api/User/SingleRegister
// Body: User 对象 JSON；Header: MachineCode（管理员创建，免验证码）
// 阶段 5 Task 17：追加权限校验（仅 TeamAdmin/IsSystemAdmin）+ 密码强制加盐
app.MapPost("/api/User/SingleRegister", async (HttpContext ctx, UserRepository repo) =>
{
    // 17.1 权限校验：HeaderParser 获取 UserId → 查 Users.IsTeamAdmin/IsSystemAdmin
    var adminUserId = HeaderParser.ParseUserId(ctx);
    if (adminUserId == 0) return ApiResponseHelper.Unauthorized();
    var adminUser = await repo.GetByIdAsync(adminUserId);
    if (adminUser == null || (!adminUser.IsTeamAdmin && !adminUser.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    var bodyText = await ReadBodyAsync(ctx);
    var user = JsonConvert.DeserializeObject<UserDto>(bodyText, ApiResponseHelper.Settings());
    if (user == null) return ApiResponseHelper.Error("请求体无效");
    if (string.IsNullOrEmpty(user.UserName)) return ApiResponseHelper.Error("用户名不能为空");

    // UserDto.Password 被 [JsonIgnore] 标记，手动从 JObject 提取（与 Register 端点一致）。
    var data = JObject.Parse(bodyText);
    var rawPassword = (data["password"] ?? data["Password"])?.ToString();
    if (!string.IsNullOrEmpty(rawPassword))
    {
        // 客户端发送密码时使用 Encrypts.SHA256Encrypt(password, isUrl: false)，
        // 需要进行 URL 编码后再存储，与验证逻辑保持一致。
        var encodedPassword = HttpUtility.UrlEncode(rawPassword);
        var salt = PasswordHasher.GenerateSalt();
        user.Password = PasswordHasher.HashPassword(encodedPassword, salt);
        user.PasswordSalt = salt;
        user.PasswordHashAlgorithm = 1;
    }

    var id = await repo.CreateAsync(user);
    app.Logger.LogInformation("管理员创建用户成功: userName={UserName} id={Id} 创建者={AdminId}", user.UserName, id, adminUserId);
    return ApiResponseHelper.JsonNet(id);
});

// ============= 阶段 5 Task 15-16：组织成员管理接口（4 个端点） =============

// POST /api/User/BatchImport
// Body: { teamId, users: [{ userName, name, phone, email, password }] }
// 仅 TeamAdmin/IsSystemAdmin 可调用；校验 License.Seats 未超；返回成功/失败明细
app.MapPost("/api/User/BatchImport", async (
    HttpContext ctx,
    UserRepository userRepo,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    // 15.2 权限校验
    var adminUserId = HeaderParser.ParseUserId(ctx);
    if (adminUserId == 0) return ApiResponseHelper.Unauthorized();
    var adminUser = await userRepo.GetByIdAsync(adminUserId);
    if (adminUser == null || (!adminUser.IsTeamAdmin && !adminUser.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var teamIdStr = (data["teamId"] ?? data["TeamId"])?.ToString() ?? "";
    if (string.IsNullOrEmpty(teamIdStr))
        return ApiResponseHelper.Error("teamId 不能为空");

    var usersArr = data["users"] as JArray;
    if (usersArr == null || usersArr.Count == 0)
        return ApiResponseHelper.Error("users 数组不能为空");

    // 安全审计修复（High）：原实现只验"是管理员"，不验"是该团队的管理员"，任意团队管理员
    // 可向其他团队批量导入成员。非系统管理员时校验 adminUser.TeamId == 目标 teamId。
    if (!adminUser.IsSystemAdmin && !string.Equals(
            adminUser.TeamId == Guid.Empty ? "" : adminUser.TeamId.ToString(), teamIdStr, StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅可管理本团队的成员" }, 403);
    }

    // 15.1.2 校验该 Team 的 License.Seats 未超（现有成员数 + 新增数 <= Seats）
    var license = await licenseRepo.GetActiveLicenseByOwnerAsync("Team", teamIdStr);
    if (license == null)
        return ApiResponseHelper.Error("团队未配置有效 License");

    var currentMemberCount = await licenseRepo.CountTeamMembersAsync(teamIdStr);
    var remaining = license.Seats - currentMemberCount;
    if (usersArr.Count > remaining)
    {
        ctx.Response.StatusCode = 429;
        return ApiResponseHelper.JsonNet(new
        {
            error = "seats_exceeded",
            message = $"座位数不足，剩余 {remaining} 个"
        }, 429);
    }

    // 15.1.3 遍历 users 数组：校验 UserName 不重复 + 密码加盐 + 插入 Users + UserTeams
    var imported = 0;
    var failed = new List<object>();
    foreach (var item in usersArr)
    {
        var userName = (item["userName"] ?? item["UserName"])?.ToString() ?? "";
        var name = (item["name"] ?? item["Name"])?.ToString();
        var phone = (item["phone"] ?? item["Phone"])?.ToString();
        var email = (item["email"] ?? item["Email"])?.ToString();
        var password = (item["password"] ?? item["Password"])?.ToString() ?? "";

        if (string.IsNullOrEmpty(userName))
        {
            failed.Add(new { userName = "", reason = "用户名不能为空" });
            continue;
        }

        // 校验 UserName 不重复
        var exists = await userRepo.GetByUserNameAsync(userName);
        if (exists != null)
        {
            failed.Add(new { userName, reason = "用户名已存在" });
            continue;
        }

        // 密码加盐
        var salt = PasswordHasher.GenerateSalt();
        var hashedPassword = PasswordHasher.HashPassword(password, salt);

        var newUser = new UserDto
        {
            UserName = userName,
            Name = name,
            Phone = phone,
            Email = email,
            Password = hashedPassword,
            PasswordSalt = salt,
            PasswordHashAlgorithm = 1
        };
        var newId = await userRepo.CreateAsync(newUser);

        // 插入 UserTeams 关联
        using (var conn2 = db.CreateConnection())
        {
            await conn2.OpenAsync();
            using var cmd2 = conn2.CreateCommand();
            cmd2.CommandText = "INSERT OR IGNORE INTO UserTeams (UserId, TeamId) VALUES (@uid, @tid)";
            cmd2.Parameters.AddWithValue("@uid", newId);
            cmd2.Parameters.AddWithValue("@tid", teamIdStr);
            await cmd2.ExecuteNonQueryAsync();
        }

        imported++;
    }

    app.Logger.LogInformation("批量导入用户: TeamId={TeamId} 导入={Imported} 失败={Failed} 创建者={AdminId}",
        teamIdStr, imported, failed.Count, adminUserId);
    return ApiResponseHelper.JsonNet(new { imported, failed });
});

// POST /api/Project/InviteUser
// Body: { teamId, phone, email, role }
// 仅 TeamAdmin 可调用；生成邀请 Token，返回邀请链接
app.MapPost("/api/Project/InviteUser", async (
    HttpContext ctx,
    UserRepository userRepo,
    InvitationRepository invitationRepo) =>
{
    // 权限校验
    var adminUserId = HeaderParser.ParseUserId(ctx);
    if (adminUserId == 0) return ApiResponseHelper.Unauthorized();
    var adminUser = await userRepo.GetByIdAsync(adminUserId);
    if (adminUser == null || (!adminUser.IsTeamAdmin && !adminUser.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var teamIdStr = (data["teamId"] ?? data["TeamId"])?.ToString() ?? "";
    var phone = (data["phone"] ?? data["Phone"])?.ToString();
    var email = (data["email"] ?? data["Email"])?.ToString();
    var role = data.Value<int?>("role") ?? data.Value<int?>("Role") ?? 0;

    if (string.IsNullOrEmpty(teamIdStr))
        return ApiResponseHelper.Error("teamId 不能为空");

    // 安全审计修复（High）：原实现只验"是管理员"，不验"是该团队的管理员"，任意团队管理员
    // 可向其他团队发邀请。非系统管理员时校验 adminUser.TeamId == 目标 teamId。
    if (!adminUser.IsSystemAdmin && !string.Equals(
            adminUser.TeamId == Guid.Empty ? "" : adminUser.TeamId.ToString(), teamIdStr, StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅可管理本团队的成员" }, 403);
    }

    var token = await invitationRepo.CreateInvitationAsync(teamIdStr, email, phone, role, adminUserId);
    var inviteUrl = $"https://auditai.com/invite?token={token}";

    // V2-H-22 修复：日志不再记录 Token 明文（7 天有效期内可被冒用），仅记录 TeamId、操作者与脱敏的被邀请人联系方式。
    var maskedEmail = string.IsNullOrEmpty(email) ? "" : MaskEmail(email);
    var maskedPhone = string.IsNullOrEmpty(phone) ? "" : MaskPhone(phone);
    app.Logger.LogInformation("发送团队邀请: TeamId={TeamId} 创建者={AdminId} Email={Email} Phone={Phone}", teamIdStr, adminUserId, maskedEmail, maskedPhone);
    return ApiResponseHelper.JsonNet(new { inviteUrl, token });
});

// POST /api/User/AcceptInvitation
// Body: { inviteToken, userName, password, name, phone }
// 校验 Token 有效 → 创建用户（密码加盐）→ 加入团队 → 更新 Status=Accepted
// V2-H-15 修复：QuotaMiddleware 因匿名调用无法解析 TeamId，导致座位数校验失效。
//              在端点内部根据 invitation.TeamId 显式校验座位数，超限返回 429。
app.MapPost("/api/User/AcceptInvitation", async (
    HttpContext ctx,
    UserRepository userRepo,
    InvitationRepository invitationRepo,
    LicenseRepository licenseRepo) =>
{
    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var inviteToken = (data["inviteToken"] ?? data["InviteToken"])?.ToString() ?? "";
    var userName = (data["userName"] ?? data["UserName"])?.ToString() ?? "";
    var password = (data["password"] ?? data["Password"])?.ToString() ?? "";
    var name = (data["name"] ?? data["Name"])?.ToString();
    var phone = (data["phone"] ?? data["Phone"])?.ToString();

    if (string.IsNullOrEmpty(inviteToken))
        return ApiResponseHelper.Error("inviteToken 不能为空");
    if (string.IsNullOrEmpty(userName))
        return ApiResponseHelper.Error("userName 不能为空");
    if (string.IsNullOrEmpty(password))
        return ApiResponseHelper.Error("password 不能为空");

    // 校验 InviteToken 有效（未过期、未接受）
    var invitation = await invitationRepo.GetInvitationByTokenAsync(inviteToken);
    if (invitation == null)
    {
        ctx.Response.StatusCode = 400;
        return ApiResponseHelper.JsonNet(new { error = "invalid_token", message = "邀请链接无效或已过期" }, 400);
    }

    // V2-H-15 修复：在创建用户前校验目标团队 License 座位数。
    // 参考 BatchImport 端点 (line 766-780) 的实现。
    var teamIdStr = invitation.TeamId?.ToString() ?? "";
    if (!string.IsNullOrEmpty(teamIdStr))
    {
        var license = await licenseRepo.GetActiveLicenseByOwnerAsync("Team", teamIdStr);
        if (license != null)
        {
            var currentMemberCount = await licenseRepo.CountTeamMembersAsync(teamIdStr);
            var remaining = license.Seats - currentMemberCount;
            if (remaining <= 0)
            {
                ctx.Response.StatusCode = 429;
                return ApiResponseHelper.JsonNet(new
                {
                    error = "seats_exceeded",
                    message = $"团队座位数已满，剩余 {remaining} 个"
                }, 429);
            }
        }
    }

    // 校验 UserName 不重复
    var exists = await userRepo.GetByUserNameAsync(userName);
    if (exists != null)
    {
        ctx.Response.StatusCode = 400;
        return ApiResponseHelper.JsonNet(new { error = "username_exists", message = "用户名已存在" }, 400);
    }

    // 创建用户（密码加盐）
    var salt = PasswordHasher.GenerateSalt();
    var hashedPassword = PasswordHasher.HashPassword(password, salt);
    var newUser = new UserDto
    {
        UserName = userName,
        Name = name,
        Phone = phone,
        Email = invitation.Email,
        Password = hashedPassword,
        PasswordSalt = salt,
        PasswordHashAlgorithm = 1
    };
    var newId = await userRepo.CreateAsync(newUser);

    // 接受邀请：更新 Status=Accepted，插入 UserTeams
    await invitationRepo.AcceptInvitationAsync(inviteToken, newId);

    // V2-H-22 修复：日志不再记录邀请 Token 明文，仅记录 TeamId、用户名与 UserId。
    app.Logger.LogInformation("接受团队邀请: TeamId={TeamId} UserName={UserName} UserId={UserId}",
        invitation.TeamId, userName, newId);
    return ApiResponseHelper.JsonNet(new { accepted = true, teamId = invitation.TeamId });
});

// GET /api/Project/GetPendingInvitations?teamId={teamId}
// 仅 TeamAdmin 可调用；返回 Status=0 的待接受邀请列表
app.MapGet("/api/Project/GetPendingInvitations", async (
    HttpContext ctx,
    string teamId,
    UserRepository userRepo,
    InvitationRepository invitationRepo) =>
{
    // 权限校验
    var adminUserId = HeaderParser.ParseUserId(ctx);
    if (adminUserId == 0) return ApiResponseHelper.Unauthorized();
    var adminUser = await userRepo.GetByIdAsync(adminUserId);
    if (adminUser == null || (!adminUser.IsTeamAdmin && !adminUser.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    if (string.IsNullOrEmpty(teamId))
        return ApiResponseHelper.Error("teamId 不能为空");

    // 安全审计修复（High）：原实现只验"是管理员"，不验"是该团队的管理员"，任意团队管理员
    // 可查看其他团队的待接受邀请。非系统管理员时校验 adminUser.TeamId == 目标 teamId。
    if (!adminUser.IsSystemAdmin && !string.Equals(
            adminUser.TeamId == Guid.Empty ? "" : adminUser.TeamId.ToString(), teamId, StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅可管理本团队的成员" }, 403);
    }

    var invitations = await invitationRepo.GetPendingInvitationsAsync(teamId);
    return ApiResponseHelper.JsonNet(new { invitations });
});

// POST /api/User/UpdateUserInfo
// Body: User 对象 JSON；Header: UserId + Token
// 安全审计修复（Critical）：原实现直接调用 UpdateAsync 写入所有字段（含 Role/Permissions/IsTeamAdmin/IsSystemAdmin/IsDataAdmin），
// 普通用户可通过修改 Body 自提权。修复：先从 DB 加载现有用户，强制将敏感字段回填为原值，仅允许更新非敏感资料字段。
app.MapPost("/api/User/UpdateUserInfo", async (HttpContext ctx, UserRepository repo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var user = JsonConvert.DeserializeObject<UserDto>(bodyText, ApiResponseHelper.Settings());
    if (user == null) return ApiResponseHelper.Error("请求体无效");

    // 加载现有用户作为权威来源，强制剥离客户端提交的敏感字段
    var existing = await repo.GetByIdAsync(userId);
    if (existing == null) return ApiResponseHelper.Unauthorized("用户不存在");

    user.Id = userId;
    // 敏感字段一律以服务端现有值为准（防自提权）
    user.Role = existing.Role;
    user.Permissions = existing.Permissions;
    user.IsTeamAdmin = existing.IsTeamAdmin;
    user.IsSystemAdmin = existing.IsSystemAdmin;
    user.IsDataAdmin = existing.IsDataAdmin;
    user.TeamId = existing.TeamId;
    user.GroupId = existing.GroupId;
    user.LicenseDate = existing.LicenseDate;
    // 密码相关字段不在本端点更新（专用端点 ResetPassword/UpdatePhoneInfo 处理）
    user.Password = existing.Password;
    user.PasswordSalt = existing.PasswordSalt;
    user.PasswordHashAlgorithm = existing.PasswordHashAlgorithm;
    user.Salt = existing.Salt;
    // 修复: 客户端未发送 Phone 时保留原值，避免 UpdateAsync 将 Phone 清空为 NULL
    // （UpdateAsync 执行全字段 UPDATE，null Phone 会覆盖数据库中的现有值）
    if (string.IsNullOrEmpty(user.Phone)) user.Phone = existing.Phone;
    // 修复: 客户端未发送 UserName 时保留原值
    if (string.IsNullOrEmpty(user.UserName)) user.UserName = existing.UserName;

    try
    {
        await repo.UpdateAsync(user);
        return ApiResponseHelper.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return ApiResponseHelper.Error(ex.Message);
    }
});

// POST /api/User/UpdatePicture
// Body: byte[]（图片二进制，Newtonsoft 序列化为 base64 字符串）
// Header: UserId + Token
app.MapPost("/api/User/UpdatePicture", async (HttpContext ctx, UserRepository repo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var bytes = JsonConvert.DeserializeObject<byte[]>(bodyText, ApiResponseHelper.Settings());
    // 安全审计修复（Med）：原实现 bytes 为 null 直接入库且无大小限制，可能写入无效数据或超大图片拖垮 DB
    if (bytes == null || bytes.Length == 0)
        return ApiResponseHelper.Error("图片数据不能为空");
    if (bytes.Length > 5 * 1024 * 1024)
        return ApiResponseHelper.Error("图片大小超过 5MB 上限");
    await repo.UpdatePictureAsync(userId, bytes);
    return ApiResponseHelper.JsonNet(true);
});

// POST /api/User/UpdatePhoneInfo
// Body: User 对象 JSON（含新 Phone）；Header: UserId + Token + MachineCode + ValidateCode
app.MapPost("/api/User/UpdatePhoneInfo", async (HttpContext ctx, UserRepository repo, SmsService sms) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var user = JsonConvert.DeserializeObject<UserDto>(bodyText, ApiResponseHelper.Settings());
    if (user == null) return ApiResponseHelper.Error("请求体无效");

    var newPhone = user.Phone;
    var code = HeaderParser.ParseValidateCode(ctx);
    if (!await sms.ValidateAsync(newPhone, code))
        return ApiResponseHelper.Unauthorized("验证码错误");

    await repo.UpdatePhoneAsync(userId, newPhone);
    return ApiResponseHelper.Ok();
});

// GET /api/User/FindPassword?userName={userName}&password={password}
// Header: MachineCode + ValidateCode
// 安全审计修复（High）：新密码通过 query string 传输，存在泄露风险；保留 GET 不破坏客户端契约，仅记录警告。
// 支持邮箱验证码：先尝试按邮箱验证，再尝试按手机号验证。
app.MapGet("/api/User/FindPassword", async (
    string userName,
    string password,
    HttpContext ctx,
    UserRepository repo,
    SmsService sms) =>
{
    if (ctx.Request.Query.ContainsKey("password"))
    {
        app.Logger.LogWarning("FindPassword: 新密码通过 query string 传输（建议改为 POST），IP={IP}", ctx.Connection.RemoteIpAddress);
    }

    var user = await repo.GetByUserNameAsync(userName);
    if (user == null) return ApiResponseHelper.Error("用户不存在");

    var code = HeaderParser.ParseValidateCode(ctx);
    
    bool isValid = false;
    if (!string.IsNullOrEmpty(user.Email))
    {
        isValid = await sms.ValidateAsync(user.Email, code);
    }
    if (!isValid && !string.IsNullOrEmpty(user.Phone))
    {
        isValid = await sms.ValidateAsync(user.Phone, code);
    }
    
    if (!isValid) return ApiResponseHelper.Unauthorized("验证码错误");

    // 阶段 9 Task 9.4：密码 SHA256+Salt 存储（重新生成 Salt）
    var salt = PasswordHasher.GenerateSalt();
    var hashedPassword = PasswordHasher.HashPassword(password, salt);
    await repo.UpdatePasswordWithSaltAsync(user.Id, hashedPassword, salt, 1);
    return ApiResponseHelper.Ok();
});

// GET /api/User/ResetPassword?oldPassword={oldPassword}&newPassword={newPassword}
// Header: UserId + Token + MachineCode + ValidateCode
// 安全审计修复（High）：新旧密码均通过 query string 传输，存在泄露风险；保留 GET 不破坏客户端契约，仅记录警告。
app.MapGet("/api/User/ResetPassword", async (
    string oldPassword,
    string newPassword,
    HttpContext ctx,
    UserRepository repo,
    SmsService sms) =>
{
    if (ctx.Request.Query.ContainsKey("oldPassword") || ctx.Request.Query.ContainsKey("newPassword"))
    {
        app.Logger.LogWarning("ResetPassword: 密码通过 query string 传输（建议改为 POST），UserId={UserId} IP={IP}",
            HeaderParser.ParseUserId(ctx), ctx.Connection.RemoteIpAddress);
    }

    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var user = await repo.GetByIdAsync(userId);
    if (user == null) return ApiResponseHelper.Unauthorized();

    // 阶段 9 Task 9.4：原密码校验使用 PasswordHasher（兼容 Legacy 直接比对）
    if (!VerifyPassword(user, oldPassword))
        return ApiResponseHelper.Error("原密码错误");

    var code = HeaderParser.ParseValidateCode(ctx);
    if (!string.IsNullOrEmpty(user.Phone))
    {
        if (!await sms.ValidateAsync(user.Phone, code))
            return ApiResponseHelper.Unauthorized("验证码错误");
    }

    // 新密码 SHA256+Salt 存储（重新生成 Salt）
    // 客户端发送密码时使用 Encrypts.SHA256Encrypt(password, isUrl: true) 进行 URL 编码，
    // 但 ASP.NET Core 会自动对 URL 查询参数进行解码，所以需要重新编码后再存储。
    var encodedNewPassword = HttpUtility.UrlEncode(newPassword);
    var salt = PasswordHasher.GenerateSalt();
    var hashedPassword = PasswordHasher.HashPassword(encodedNewPassword, salt);
    await repo.UpdatePasswordWithSaltAsync(userId, hashedPassword, salt, 1);
    return ApiResponseHelper.Ok();
});

// GET /api/User/ResetPasswordWithoutSMS?oldPassword={oldPassword}&newPassword={newPassword}
// Header: UserId + Token + MachineCode（免验证码重置）
// 安全审计修复（High）：新旧密码均通过 query string 传输，存在泄露风险；保留 GET 不破坏客户端契约，仅记录警告。
app.MapGet("/api/User/ResetPasswordWithoutSMS", async (
    string oldPassword,
    string newPassword,
    HttpContext ctx,
    UserRepository repo) =>
{
    if (ctx.Request.Query.ContainsKey("oldPassword") || ctx.Request.Query.ContainsKey("newPassword"))
    {
        app.Logger.LogWarning("ResetPasswordWithoutSMS: 密码通过 query string 传输（建议改为 POST），UserId={UserId} IP={IP}",
            HeaderParser.ParseUserId(ctx), ctx.Connection.RemoteIpAddress);
    }

    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var user = await repo.GetByIdAsync(userId);
    if (user == null) return ApiResponseHelper.Unauthorized();

    // 阶段 9 Task 9.4：原密码校验使用 PasswordHasher（兼容 Legacy 直接比对）
    if (!VerifyPassword(user, oldPassword))
        return ApiResponseHelper.Error("原密码错误");

    // 客户端发送密码时使用 Encrypts.SHA256Encrypt(password, isUrl: true) 进行 URL 编码，
    // 但 ASP.NET Core 会自动对 URL 查询参数进行解码，所以需要重新编码后再存储。
    var encodedNewPassword = HttpUtility.UrlEncode(newPassword);
    var salt = PasswordHasher.GenerateSalt();
    var hashedPassword = PasswordHasher.HashPassword(encodedNewPassword, salt);
    await repo.UpdatePasswordWithSaltAsync(userId, hashedPassword, salt, 1);
    return ApiResponseHelper.Ok();
});

// ============= 阶段 3 Task 3.1：团队管理接口（14 个 Project/* 端点） =============
// 客户端调用签名以 WebApiClient.cs 为准：Body 字段使用 camelCase 或 PascalCase 与客户端匿名对象属性名一致。

// POST /api/Project/CreateTeam
// Body: { teamName, type }（camelCase）；返回新 Team 对象
app.MapPost("/api/Project/CreateTeam", async (HttpContext ctx, TeamService svc, LicenseRepository licenseRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var teamName = data.Value<string>("teamName") ?? data.Value<string>("TeamName") ?? "";
    var type = data.Value<int?>("type") ?? 0;

    var team = await svc.CreateTeamAsync(userId, teamName, type);

    // 阶段 9 Task 26.1：为新团队自动创建 Trial License（7 天有效期，Seats=1）
    // 避免新团队无 License 被 LicenseMiddleware 拦截（402）
    await licenseRepo.CreateLicenseAsync(
        ownerType: "Team",
        ownerId: team.Id.ToString("D"),
        planType: 0,  // Trial
        seats: 1,
        maxProjects: 1,
        maxTemplates: 1,
        startDate: DateTime.Now,
        endDate: DateTime.Now.AddDays(7)
    );

    return ApiResponseHelper.JsonNet(team);
});

// POST /api/Project/DismissTeam
// Body: { teamId }（camelCase）；指定要解散的团队；返回 bool
// 修复：不再依赖 Users.TeamId（可能过期），改为客户端显式传入 teamId，
// 并校验用户必须是该团队成员才能解散（仅 Owner 可操作，由 DismissTeamAsync 内部校验）
app.MapPost("/api/Project/DismissTeam", async (HttpContext ctx, TeamService svc, TeamRepository teamRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var teamIdStr = data.Value<string>("teamId") ?? data.Value<string>("TeamId") ?? "";
    if (!Guid.TryParse(teamIdStr, out var teamId))
    {
        return ApiResponseHelper.Error("teamId 参数无效");
    }

    // 校验用户是该团队成员（防止解散不属于自己的团队）
    var teams = await teamRepo.GetUserTeamsAsync(userId);
    if (!teams.Any(t => t.Id == teamId))
    {
        return ApiResponseHelper.Error("您不是该团队成员，无法解散");
    }

    try
    {
        await svc.DismissTeamAsync(teamId, userId);
        return ApiResponseHelper.JsonNet(true);
    }
    catch (UnauthorizedAccessException)
    {
        return ApiResponseHelper.Unauthorized("仅团队所有者可解散团队");
    }
});

// POST /api/Project/UpdateTeamName
// Body: { teamName }（camelCase）；更新当前用户团队名称；返回 bool
app.MapPost("/api/Project/UpdateTeamName", async (HttpContext ctx, TeamService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var teamId = await svc.GetCurrentUserTeamIdAsync(userId);
    if (teamId == Guid.Empty) return ApiResponseHelper.Error("当前用户未绑定团队");

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var teamName = data.Value<string>("teamName") ?? data.Value<string>("TeamName") ?? "";

    await svc.UpdateTeamNameAsync(teamId, teamName);
    return ApiResponseHelper.JsonNet(true);
});

// POST /api/Project/UpdateCurrentTeam
// Body: { TeamId }（PascalCase）；切换用户当前团队；返回 JObject
app.MapPost("/api/Project/UpdateCurrentTeam", async (HttpContext ctx, TeamService svc, UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var teamIdStr = data.Value<string>("TeamId") ?? data.Value<string>("teamId");
    if (!Guid.TryParse(teamIdStr, out var teamId))
        return ApiResponseHelper.Error("无效的 TeamId");

    await svc.UpdateCurrentTeamAsync(userId, teamId);
    
    var user = await userRepo.GetByIdAsync(userId);
    return ApiResponseHelper.JsonNet(new 
    { 
        IsTeamAdmin = user?.IsTeamAdmin ?? false,
        IsLicenseOutOfDate = false
    });
});

// GET /api/Project/GetTeamUsers
// 无参数（使用当前用户 TeamId）；返回 IEnumerable<User>（直接 JSON 数组）
app.MapGet("/api/Project/GetTeamUsers", async (HttpContext ctx, TeamService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var teamId = await svc.GetCurrentUserTeamIdAsync(userId);
    if (teamId == Guid.Empty) return ApiResponseHelper.JsonNet(new List<UserDto>());

    var users = await svc.GetTeamUsersAsync(teamId);
    return ApiResponseHelper.JsonNet(users);
});

// GET /api/Project/GetTeamUserGroups
// 无参数；返回 { userGroups: [{ id, name, parentid }] }（lowercase 键）
app.MapGet("/api/Project/GetTeamUserGroups", async (HttpContext ctx, TeamService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var teamId = await svc.GetCurrentUserTeamIdAsync(userId);
    if (teamId == Guid.Empty) return ApiResponseHelper.JsonNet(new { userGroups = Array.Empty<object>() });

    var groups = await svc.GetTeamUserGroupsAsync(teamId);
    return ApiResponseHelper.JsonNet(new
    {
        userGroups = groups.Select(g => new
        {
            id = g.Id,
            name = g.Name,
            parentid = g.ParentId
        })
    });
});

// POST /api/Project/AddUserToTeam
// Body: { UserName, TeamId? }（PascalCase，TeamId 可选）；将目标用户加入指定团队（未指定则用当前用户主团队）；返回 User 对象
// 安全审计修复（High）：原实现任意已登录用户可调用，越权添加成员。修复：传入 operatorUserId 在服务层校验 Team Admin 权限。
// P1-9 修复：支持显式 TeamId 参数，避免多团队 admin 误将用户加入主团队（团队A）。
app.MapPost("/api/Project/AddUserToTeam", async (HttpContext ctx, TeamService svc, UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var userName = data.Value<string>("UserName") ?? data.Value<string>("userName");
    if (string.IsNullOrEmpty(userName)) return ApiResponseHelper.Error("UserName 不能为空");

    // 优先使用 Body 中显式指定的 TeamId；未指定则回退到当前用户主团队
    var teamIdStr = data.Value<string>("TeamId") ?? data.Value<string>("teamId");
    Guid teamId;
    if (!string.IsNullOrEmpty(teamIdStr) && Guid.TryParse(teamIdStr, out var explicitTeamId))
    {
        teamId = explicitTeamId;
    }
    else
    {
        teamId = await svc.GetCurrentUserTeamIdAsync(userId);
        if (teamId == Guid.Empty) return ApiResponseHelper.Error("当前用户未绑定团队");
    }

    var targetUser = await userRepo.GetByUserNameAsync(userName);
    if (targetUser == null) return ApiResponseHelper.Error("用户不存在");

    try
    {
        await svc.AddUserToTeamAsync(userId, targetUser.Id, teamId);
    }
    catch (UnauthorizedAccessException ex)
    {
        return ApiResponseHelper.Unauthorized(ex.Message);
    }
    return ApiResponseHelper.JsonNet(targetUser);
});

// POST /api/Project/RemoveUserFromTeam
// Body: { UserName, TeamId }（PascalCase，TeamId 可空）；返回 ok
// 安全审计修复（High）：原实现任意已登录用户可调用，越权移除成员。修复：传入 operatorUserId 在服务层校验 Team Admin 权限。
app.MapPost("/api/Project/RemoveUserFromTeam", async (HttpContext ctx, TeamService svc, UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var userName = data.Value<string>("UserName") ?? data.Value<string>("userName");
    if (string.IsNullOrEmpty(userName)) return ApiResponseHelper.Error("UserName 不能为空");

    var targetUser = await userRepo.GetByUserNameAsync(userName);
    if (targetUser == null) return ApiResponseHelper.Error("用户不存在");

    var teamIdStr = data.Value<string>("TeamId") ?? data.Value<string>("teamId");
    Guid teamId;
    if (Guid.TryParse(teamIdStr, out teamId))
    {
        try
        {
            await svc.RemoveUserFromTeamAsync(userId, targetUser.Id, teamId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiResponseHelper.Unauthorized(ex.Message);
        }
    }
    else
    {
        // 未指定 TeamId，使用当前用户的团队
        teamId = await svc.GetCurrentUserTeamIdAsync(userId);
        if (teamId == Guid.Empty) return ApiResponseHelper.Error("当前用户未绑定团队");
        try
        {
            await svc.RemoveUserFromTeamAsync(userId, targetUser.Id, teamId);
        }
        catch (UnauthorizedAccessException ex)
        {
            return ApiResponseHelper.Unauthorized(ex.Message);
        }
    }
    return ApiResponseHelper.Ok();
});

// POST /api/Project/AddUserGroup
// Body: { groupName, parentId }（camelCase）；返回 long（新分组 Id）
app.MapPost("/api/Project/AddUserGroup", async (HttpContext ctx, TeamService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var teamId = await svc.GetCurrentUserTeamIdAsync(userId);
    if (teamId == Guid.Empty) return ApiResponseHelper.Error("当前用户未绑定团队");

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var groupName = data.Value<string>("groupName") ?? data.Value<string>("GroupName") ?? "";
    var parentIdVal = data.Value<long?>("parentId") ?? 0;
    long? parentId = parentIdVal > 0 ? parentIdVal : null;

    var group = await svc.AddUserGroupAsync(groupName, teamId, parentId);
    return ApiResponseHelper.JsonNet(group.Id);
});

// POST /api/Project/MoveUserToGroup
// Body: { userId, groupId }（camelCase，字符串形式）；返回 JObject
// 安全审计修复（High）：原端点无任何权限校验，任意已登录用户可移动任意用户的分组。
// 补管理员校验（对照 InviteUser）+ 目标用户/分组团队归属校验（系统管理员豁免）。
app.MapPost("/api/Project/MoveUserToGroup", async (HttpContext ctx, TeamService svc, UserRepository userRepo, SqliteStorage db) =>
{
    var callerId = HeaderParser.ParseUserId(ctx);
    if (callerId == 0) return ApiResponseHelper.Unauthorized();
    var caller = await userRepo.GetByIdAsync(callerId);
    if (caller == null || (!caller.IsTeamAdmin && !caller.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    // 客户端以字符串形式传递 userId 和 groupId
    var userIdStr = (data["userId"] ?? data["UserId"])?.ToString();
    var groupIdStr = (data["groupId"] ?? data["GroupId"])?.ToString();
    if (!long.TryParse(userIdStr, out var targetUserId))
        return ApiResponseHelper.Error("无效的 userId");
    if (!long.TryParse(groupIdStr, out var groupId))
        return ApiResponseHelper.Error("无效的 groupId");

    // 团队归属校验：目标用户与目标分组都必须属于调用者的团队
    var targetUser = await userRepo.GetByIdAsync(targetUserId);
    if (targetUser == null) return ApiResponseHelper.Error("用户不存在");
    var groupTeamId = await QueryUserGroupTeamIdAsync(db, groupId);
    if (groupTeamId == null) return ApiResponseHelper.Error("分组不存在");
    if (!caller.IsSystemAdmin && (targetUser.TeamId != caller.TeamId || groupTeamId.Value != caller.TeamId))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅可管理本团队的分组与成员" }, 403);
    }

    await svc.MoveUserToGroupAsync(targetUserId, groupId);
    return ApiResponseHelper.JsonNet(new { });
});

// POST /api/Project/DeleteUserGroup
// Body: { groupId }（camelCase）；返回 bool
// 安全审计修复（High）：补管理员校验 + 分组团队归属校验（系统管理员豁免）。
app.MapPost("/api/Project/DeleteUserGroup", async (HttpContext ctx, TeamService svc, UserRepository userRepo, SqliteStorage db) =>
{
    var callerId = HeaderParser.ParseUserId(ctx);
    if (callerId == 0) return ApiResponseHelper.Unauthorized();
    var caller = await userRepo.GetByIdAsync(callerId);
    if (caller == null || (!caller.IsTeamAdmin && !caller.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var groupIdStr = (data["groupId"] ?? data["GroupId"])?.ToString();
    if (!long.TryParse(groupIdStr, out var groupId))
        return ApiResponseHelper.JsonNet(false);

    var groupTeamId = await QueryUserGroupTeamIdAsync(db, groupId);
    if (groupTeamId == null) return ApiResponseHelper.JsonNet(false);
    if (!caller.IsSystemAdmin && groupTeamId.Value != caller.TeamId)
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅可管理本团队的分组" }, 403);
    }

    await svc.DeleteUserGroupAsync(groupId);
    return ApiResponseHelper.JsonNet(true);
});

// POST /api/Project/RenameUserGroup
// Body: { groupId, groupName }（camelCase）；返回 bool
// 安全审计修复（High）：补管理员校验 + 分组团队归属校验（系统管理员豁免）。
app.MapPost("/api/Project/RenameUserGroup", async (HttpContext ctx, TeamService svc, UserRepository userRepo, SqliteStorage db) =>
{
    var callerId = HeaderParser.ParseUserId(ctx);
    if (callerId == 0) return ApiResponseHelper.Unauthorized();
    var caller = await userRepo.GetByIdAsync(callerId);
    if (caller == null || (!caller.IsTeamAdmin && !caller.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var groupIdStr = (data["groupId"] ?? data["GroupId"])?.ToString();
    var groupName = data.Value<string>("groupName") ?? data.Value<string>("GroupName") ?? "";
    if (!long.TryParse(groupIdStr, out var groupId))
        return ApiResponseHelper.JsonNet(false);

    var groupTeamId = await QueryUserGroupTeamIdAsync(db, groupId);
    if (groupTeamId == null) return ApiResponseHelper.JsonNet(false);
    if (!caller.IsSystemAdmin && groupTeamId.Value != caller.TeamId)
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅可管理本团队的分组" }, 403);
    }

    await svc.RenameUserGroupAsync(groupId, groupName);
    return ApiResponseHelper.JsonNet(true);
});

// POST /api/Project/UpdateJobTitle
// Body: { userId, jobTitle }（camelCase）；返回 bool
// 安全审计修复（High）：补管理员校验 + 目标用户团队归属校验（系统管理员豁免）。
app.MapPost("/api/Project/UpdateJobTitle", async (HttpContext ctx, TeamService svc, UserRepository userRepo) =>
{
    var callerId = HeaderParser.ParseUserId(ctx);
    if (callerId == 0) return ApiResponseHelper.Unauthorized();
    var caller = await userRepo.GetByIdAsync(callerId);
    if (caller == null || (!caller.IsTeamAdmin && !caller.IsSystemAdmin))
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可调用此端点" }, 403);
    }

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var userIdStr = (data["userId"] ?? data["UserId"])?.ToString();
    var jobTitle = data.Value<string>("jobTitle") ?? data.Value<string>("JobTitle") ?? "";
    if (!long.TryParse(userIdStr, out var targetUserId))
        return ApiResponseHelper.JsonNet(false);

    var targetUser = await userRepo.GetByIdAsync(targetUserId);
    if (targetUser == null) return ApiResponseHelper.JsonNet(false);
    if (!caller.IsSystemAdmin && targetUser.TeamId != caller.TeamId)
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅可管理本团队的成员" }, 403);
    }

    await svc.UpdateJobTitleAsync(targetUserId, jobTitle);
    return ApiResponseHelper.JsonNet(true);
});

// POST /api/Project/AllowTeamMerge
// Body: { desUserId }（camelCase，目标团队管理员 Id）；返回 bool（MVP 仅记录）
app.MapPost("/api/Project/AllowTeamMerge", async (HttpContext ctx, TeamService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var teamId = await svc.GetCurrentUserTeamIdAsync(userId);
    await svc.AllowTeamMergeAsync(teamId, true);
    return ApiResponseHelper.JsonNet(true);
});

// ============= 阶段 3 Task 3.2：用户权限与合并请求（3 个 User/* 端点） =============

// POST /api/User/GetTeamUserPermissions
// Body: JObject（含 teamId 或 userId）；返回 { Users: [{ userId, Name, Permissions }] }（mixed case）
app.MapPost("/api/User/GetTeamUserPermissions", async (HttpContext ctx, TeamService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);

    // Body 可能包含 teamId 或 userId，优先使用 teamId
    Guid teamId;
    var teamIdStr = (data["teamId"] ?? data["TeamId"])?.ToString();
    if (Guid.TryParse(teamIdStr, out teamId))
    {
        // 使用传入的 teamId
    }
    else
    {
        // 从 userId 推导团队
        var userIdStr = (data["userId"] ?? data["UserId"])?.ToString();
        if (long.TryParse(userIdStr, out var uid) && uid > 0)
        {
            teamId = await svc.GetCurrentUserTeamIdAsync(uid);
        }
        else
        {
            // 从当前登录用户推导
            var currentUserId = HeaderParser.ParseUserId(ctx);
            teamId = await svc.GetCurrentUserTeamIdAsync(currentUserId);
        }
    }

    if (teamId == Guid.Empty)
        return ApiResponseHelper.JsonNet(new { Users = Array.Empty<object>() });

    var users = await svc.GetTeamUsersAsync(teamId);
    return ApiResponseHelper.JsonNet(new
    {
        Users = users.Select(u => new
        {
            userId = u.Id,
            Name = u.Name,
            Permissions = u.Permissions
        })
    });
});

// POST /api/User/SetUserTeamPermissions
// Body: User 对象 JSON（含 Id 和 Permissions）；返回 ok
app.MapPost("/api/User/SetUserTeamPermissions", async (HttpContext ctx, TeamService svc) =>
{
    // V2-C-10 修复：原端点不解析调用者 userId，无 TeamAdmin/Owner 校验，
    // 任何已认证用户可为任意目标用户设置任意 Permissions JSON。
    // 现校验调用者是目标团队 Owner/IsTeamAdmin/IsSystemAdmin，teamId 取自调用者上下文。
    var callerId = HeaderParser.ParseUserId(ctx);
    if (callerId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);

    var userIdStr = (data["Id"] ?? data["id"])?.ToString();
    if (!long.TryParse(userIdStr, out var targetUserId))
        return ApiResponseHelper.Error("无效的 userId");

    // teamId 取自调用者上下文，而非目标用户（防止跨团队越权）
    var teamId = await svc.GetCurrentUserTeamIdAsync(callerId);
    if (teamId == Guid.Empty)
        return ApiResponseHelper.Error("调用者未关联任何团队");

    // 校验调用者是该团队的管理员（Team Owner / IsTeamAdmin / IsSystemAdmin）
    if (!await svc.IsTeamAdminAsync(callerId, teamId))
        return ApiResponseHelper.Forbidden("仅团队管理员可设置成员权限");

    // Permissions 可能是字符串或复杂对象，统一序列化为 JSON 字符串
    var permsToken = data["Permissions"] ?? data["permissions"];
    string? permissionsJson = null;
    if (permsToken != null)
    {
        permissionsJson = permsToken.Type == JTokenType.String
            ? permsToken.Value<string>()
            : permsToken.ToString(Formatting.None);
    }

    await svc.SetUserPermissionsAsync(targetUserId, teamId, permissionsJson ?? "");
    return ApiResponseHelper.Ok();
});

// POST /api/User/TeamMergeRequest
// Body: JObject（含 fromTeamId, toTeamId）；返回 ok（MVP 仅记录）
app.MapPost("/api/User/TeamMergeRequest", async (HttpContext ctx, TeamService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);

    var fromTeamIdStr = (data["fromTeamId"] ?? data["FromTeamId"])?.ToString();
    var toTeamIdStr = (data["toTeamId"] ?? data["ToTeamId"])?.ToString();

    if (Guid.TryParse(fromTeamIdStr, out var fromTeamId) && Guid.TryParse(toTeamIdStr, out var toTeamId))
    {
        await svc.TeamMergeRequestAsync(fromTeamId, toTeamId, userId);
    }
    // MVP：即使参数不完整也仅记录，不报错
    return ApiResponseHelper.Ok();
});

// ============= 阶段 3 Task 3.3：项目成员列表（1 个端点） =============

// GET /api/Project/GetProjectUsersWithPic?projectId={projectId}
// 返回 { users: [UserDto] }（PascalCase 用户字段，与客户端 GetProjectUsersWithPic 解析一致）
// 安全审计修复（High）：原实现完全无认证，任意客户端可枚举项目成员。
// 修复：端点层防御性校验 userId；通过 ProjectService.GetProjectDtoAsync 校验项目归属（Repository 已通过 TenantContextAccessor 过滤 TeamId）。
app.MapGet("/api/Project/GetProjectUsersWithPic", async (Guid projectId, HttpContext ctx, TeamService svc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 跨团队访问校验：项目不存在或不属于当前团队则返回 403
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var users = await svc.GetProjectUsersAsync(projectId);
    return ApiResponseHelper.JsonNet(new { users });
});

// ============= 阶段 4 Task 4.1：项目管理接口（15 个端点 + OpenProject 升级） =============
// 客户端调用签名以 WebApiClient.cs 为准：Body 字段命名兼容 camelCase/PascalCase。

// GET /api/Project/GetTemplates
// 返回 IsTemplate=1 的项目列表（IEnumerable<Project>）
app.MapGet("/api/Project/GetTemplates", async (ProjectService svc) =>
    ApiResponseHelper.JsonNet(await svc.GetTemplatesAsync()));

// GET /api/Project/GetRecycleProjects
// 返回 IsDeleted=1 的回收站项目列表
app.MapGet("/api/Project/GetRecycleProjects", async (ProjectService svc) =>
    ApiResponseHelper.JsonNet(await svc.GetRecycleProjectsAsync()));

// POST /api/Project/RestoreProjects
// Body: JArray of GUID 字符串数组；恢复指定项目从回收站
app.MapPost("/api/Project/RestoreProjects", async (HttpContext ctx, ProjectService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    var guids = new List<Guid>();
    var data = JObject.Parse(body);
    var arr = data["Ids"] as JArray ?? new JArray();
    foreach (var t in arr)
    {
        if (Guid.TryParse(t.ToString(), out var g)) guids.Add(g);
    }

    // 跨团队访问校验：任意一个 projectId 不属于当前团队则整体拒绝
    foreach (var pid in guids)
    {
        var accessDenied = await CheckProjectAccessAsync(ctx, pid, svc, app.Logger);
        if (accessDenied != null) return accessDenied;
    }

    await svc.RestoreProjectsAsync(guids);
    return ApiResponseHelper.Ok();
});

// POST /api/Project/DeleteProjectFromServer
// Body: { projectId } JObject 或 { Ids: [...] } JArray；物理删除 Projects + 关联数据
app.MapPost("/api/Project/DeleteProjectFromServer", async (HttpContext ctx, ProjectService svc, ProjectDbManager dbManager) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);

    // 支持单个 projectId 或 Ids 数组两种格式
    var ids = new List<Guid>();
    var singleId = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (Guid.TryParse(singleId, out var pid))
    {
        ids.Add(pid);
    }
    else
    {
        var idsArr = data["Ids"] as JArray;
        if (idsArr != null)
        {
            foreach (var t in idsArr)
            {
                if (Guid.TryParse(t.ToString(), out var g))
                    ids.Add(g);
            }
        }
    }

    if (ids.Count == 0)
        return ApiResponseHelper.Error("无效的 projectId");

    // 逐个校验并删除
    foreach (var id in ids)
    {
        var accessDenied = await CheckProjectAccessAsync(ctx, id, svc, app.Logger);
        if (accessDenied != null) return accessDenied;

        // V2-C-04 修复：必须先获取项目信息（TeamId/Type）再删主库行
        // 旧实现先 DeleteFromServerAsync 物理删除主库行，之后 GetProjectDtoAsync 返回 null，
        // 导致 DeleteProjectDb/DeleteTemplateDb 永不调用，.db 文件成为孤儿。
        var project = await svc.GetProjectDtoAsync(id);

        await svc.DeleteFromServerAsync(id);

        // 删除项目专属数据库文件
        if (project != null && project.TeamId.HasValue)
        {
            if (project.Type == 1)
                dbManager.DeleteTemplateDb(project.TeamId.Value, id);
            else
                dbManager.DeleteProjectDb(project.TeamId.Value, id);
        }
    }

    return ApiResponseHelper.Ok();
});

// POST /api/Project/CreateProject
// Body: Project 对象 JSON；返回新创建的 Project
app.MapPost("/api/Project/CreateProject", async (HttpContext ctx, ProjectService svc, TeamService teamSvc, ProjectDbManager dbManager, TaskService taskSvc, ProjectRepository projectRepo, SqliteStorage db) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var project = JsonConvert.DeserializeObject<ProjectDto>(body, ApiResponseHelper.Settings());
    if (project == null) return ApiResponseHelper.Error("请求体无效");

    var created = await svc.CreateProjectAsync(project, userId);
    app.Logger.LogInformation("CreateProject: creator={UserId} projectId={ProjectId} name={Name}", userId, created.Id, created.Name);

    // 生成 taskId 供客户端轮询
    var taskId = await taskSvc.GenerateTaskIdAsync(userId, "CreateProject");

    // 在后台任务中创建项目专属数据库文件
    taskSvc.StartBackgroundTask(taskId, async (progress, ct) =>
    {
        var teamId = created.TeamId ?? await teamSvc.GetCurrentUserTeamIdAsync(userId);
        try
        {
            progress.Report(0.1, "正在创建项目数据库");
            string projectDbPath;
            if (project.Type == 1)
            {
                dbManager.CreateEmptyTemplateDb(teamId, created.Id, created.Name ?? "");
                projectDbPath = dbManager.GetTemplateDbPath(teamId, created.Id);
            }
            else
            {
                dbManager.CreateEmptyProjectDb(teamId, created.Id, created.Name ?? "", project.TemplateId);
                projectDbPath = dbManager.GetProjectDbPath(teamId, created.Id);
            }
            // 将 .db 全量数据 seed 到主库，确保 SyncMainToProjectDbAsync 的 DELETE+INSERT 不会
            // 用主库的部分增量数据覆盖 .db 的全量模板数据，导致树形导航节点丢失
            try
            {
                await SeedMainDbFromProjectDbAsync(db, projectDbPath, created.Id);
                app.Logger.LogInformation("CreateProject: SeedMainDbFromProjectDbAsync 完成, projectId={ProjectId}", created.Id);
            }
            catch (Exception seedEx)
            {
                app.Logger.LogWarning(seedEx, "CreateProject: SeedMainDbFromProjectDbAsync 失败（不影响项目创建，客户端首次保存时会补传）: projectId={ProjectId}", created.Id);
            }
            progress.Report(1.0, "项目创建完成");
        }
        catch (Exception ex)
        {
            // V2-H-07 修复：后台任务失败时回滚主库 Projects 记录，避免主库记录存在但 .db 文件缺失的半残状态。
            app.Logger.LogError(ex, "CreateProject 后台任务失败，回滚主库记录: projectId={ProjectId} teamId={TeamId}", created.Id, teamId);
            try
            {
                await projectRepo.DeleteFromServerAsync(created.Id, teamId);
            }
            catch (Exception rollbackEx)
            {
                app.Logger.LogError(rollbackEx, "CreateProject 回滚主库记录失败: projectId={ProjectId}", created.Id);
            }
            throw; // 重新抛出以便 TaskService 捕获并标记任务为 Failed
        }
    });

    // 客户端期望返回 { taskId: long } 格式
    return ApiResponseHelper.JsonNet(new { taskId });
});

// POST /api/Project/ImportProject
// 通过 .auditai 归档文件导入项目：客户端解压归档后，将 project.db 二进制流上传到服务端，
// 服务端创建 Projects 主库记录 + 落盘 .db 文件，避免本地模式只在本地注册导致的同步丢失问题。
// QueryString: name, number, category, note, auditee, createTime(ISO8601), schemaVersion(int)
// Header: UserId, Token；Body: 二进制 SQLite 数据库流
// 限制：最大 100MB，与 UploadFile 一致
app.MapPost("/api/Project/ImportProject", async (
    HttpContext ctx,
    ProjectService svc,
    ProjectDbManager dbManager,
    TeamService teamSvc,
    ProjectRepository projectRepo,
    SqliteStorage db) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 1. 从 QueryString 解析元信息
    var name = ctx.Request.Query["name"].ToString();
    var number = ctx.Request.Query["number"].ToString();
    var category = ctx.Request.Query["category"].ToString();
    var note = ctx.Request.Query["note"].ToString();
    var auditee = ctx.Request.Query["auditee"].ToString();
    DateTime createTime = DateTime.Now;
    if (DateTime.TryParse(ctx.Request.Query["createTime"].ToString(), out var ct))
    {
        // 过滤旧版本项目数据库中的 2000-01-01 占位值
        createTime = ct <= new DateTime(2001, 1, 1) ? DateTime.Now : ct;
    }
    int schemaVersion = 0;
    if (int.TryParse(ctx.Request.Query["schemaVersion"].ToString(), out var sv)) schemaVersion = sv;
    const int CurrentSchemaVersion = 44;
    if (schemaVersion > CurrentSchemaVersion)
    {
        return ApiResponseHelper.Error($"项目文件版本过高（v{schemaVersion}），服务端支持的最高版本为 v{CurrentSchemaVersion}");
    }

    // 2. 大小预检（与 UploadFile 一致的 100MB 上限）
    const long MaxUploadFileSize = 104_857_600L;
    if (ctx.Request.Headers.TryGetValue("Content-Length", out var clValues) &&
        long.TryParse(clValues.ToString(), out var cl) && cl > MaxUploadFileSize)
    {
        ctx.Response.StatusCode = 413;
        return ApiResponseHelper.JsonNet(new { error = "payload_too_large", message = $"文件大小超过 {MaxUploadFileSize / (1024 * 1024)}MB 上限" }, 413);
    }

    // 3. 构造 ProjectDto（Type=0=Project, TemplateId=null 清除模板关联）
    var newProjectId = Guid.NewGuid();
    var projectDto = new ProjectDto
    {
        Id = newProjectId,
        Name = string.IsNullOrWhiteSpace(name) ? "导入项目" : name,
        Number = number,
        Category = category,
        Auditee = auditee,
        Note = note,
        Type = 0, // ProjectType.Project
        Version = 1,
        CreateTime = createTime,
        TemplateId = null
    };

    // 4. 创建主库 Projects 记录（不创建 .db 文件）
    ProjectDto created;
    try
    {
        created = await svc.CreateProjectAsync(projectDto, userId);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "ImportProject: 创建主库记录失败 userId={UserId}", userId);
        return ApiResponseHelper.Error("创建项目记录失败：" + ex.Message);
    }

    var teamId = created.TeamId ?? await teamSvc.GetCurrentUserTeamIdAsync(userId);
    var dbPath = dbManager.GetProjectDbPath(teamId, newProjectId);

    // 5. 流式写入 .db 文件（覆盖可能存在的空 .db）
    long bytesWritten = 0;
    var buffer = new byte[81920];
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        await using (var fsOut = new FileStream(dbPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            int read;
            while ((read = await ctx.Request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                bytesWritten += read;
                if (bytesWritten > MaxUploadFileSize)
                {
                    await fsOut.DisposeAsync();
                    TryDeleteFile(dbPath);
                    ctx.Response.StatusCode = 413;
                    // 主库记录已写入，回滚避免半残状态
                    try { await projectRepo.DeleteFromServerAsync(newProjectId, teamId); } catch { }
                    return ApiResponseHelper.JsonNet(new { error = "payload_too_large", message = $"文件大小超过 {MaxUploadFileSize / (1024 * 1024)}MB 上限" }, 413);
                }
                await fsOut.WriteAsync(buffer, 0, read);
            }
        }

        if (bytesWritten == 0)
        {
            TryDeleteFile(dbPath);
            try { await projectRepo.DeleteFromServerAsync(newProjectId, teamId); } catch { }
            return ApiResponseHelper.Error("上传的项目数据库为空");
        }

        // 6. 校验 .db 的 user_version，避免客户端导入高版本 schema 的 .db
        try
        {
            using var verifyConn = new SqliteConnection($"Data Source={dbPath};Pooling=False;Mode=ReadOnly;");
            await verifyConn.OpenAsync();
            using var verCmd = verifyConn.CreateCommand();
            verCmd.CommandText = "PRAGMA user_version;";
            var verObj = await verCmd.ExecuteScalarAsync();
            var actualVersion = Convert.ToInt32(verObj);
            if (actualVersion > CurrentSchemaVersion)
            {
                TryDeleteFile(dbPath);
                try { await projectRepo.DeleteFromServerAsync(newProjectId, teamId); } catch { }
                return ApiResponseHelper.Error($"项目数据库版本过高（v{actualVersion}），服务端支持的最高版本为 v{CurrentSchemaVersion}");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "ImportProject: 校验 .db schema 失败 projectId={ProjectId}", newProjectId);
            // 校验失败不阻止导入，仅记录日志（.db 文件本身可能合法但 PRAGMA 读取异常）
        }

        app.Logger.LogInformation("ImportProject: userId={UserId} projectId={ProjectId} teamId={TeamId} size={Size}",
            userId, newProjectId, teamId, bytesWritten);

        // 将 .db 全量数据 seed 到主库（与 CreateProject 一致，防止 SyncMainToProjectDbAsync 覆盖全量数据）
        try
        {
            await SeedMainDbFromProjectDbAsync(db, dbPath, newProjectId);
        }
        catch (Exception seedEx)
        {
            app.Logger.LogWarning(seedEx, "ImportProject: SeedMainDbFromProjectDbAsync 失败: projectId={ProjectId}", newProjectId);
        }

        return ApiResponseHelper.JsonNet(created);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "ImportProject: 写入 .db 失败 projectId={ProjectId}", newProjectId);
        TryDeleteFile(dbPath);
        try { await projectRepo.DeleteFromServerAsync(newProjectId, teamId); } catch { }
        return ApiResponseHelper.Error("导入项目失败：" + ex.Message);
    }
});

// POST /api/Project/ImportTemplate
// 通过 .auditaitemplate 归档文件导入模板：客户端解压归档 + dlgTemplateEditor 编辑后，
// 将 template.db 二进制流上传到服务端，服务端创建 Projects 主库记录（Type=1, IsTemplate=1）
// + 落盘到 Templates/ 目录 + SeedMainDbFromProjectDbAsync 同步主库 TreeGroup/TreeNode
// QueryString: name, number, category, note, createTime(ISO8601), schemaVersion(int), teamVisible(bool)
// Header: UserId, Token；Body: 二进制 SQLite 数据库流
// 限制：最大 100MB，与 ImportProject 一致
app.MapPost("/api/Project/ImportTemplate", async (
    HttpContext ctx,
    ProjectService svc,
    ProjectDbManager dbManager,
    TeamService teamSvc,
    ProjectRepository projectRepo,
    SqliteStorage db) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 1. 从 QueryString 解析元信息
    var name = ctx.Request.Query["name"].ToString();
    var number = ctx.Request.Query["number"].ToString();
    var category = ctx.Request.Query["category"].ToString();
    var note = ctx.Request.Query["note"].ToString();
    DateTime createTime = DateTime.Now;
    if (DateTime.TryParse(ctx.Request.Query["createTime"].ToString(), out var ct))
    {
        // 过滤旧版本模板数据库中的 2000-01-01 占位值
        createTime = ct <= new DateTime(2001, 1, 1) ? DateTime.Now : ct;
    }
    int schemaVersion = 0;
    if (int.TryParse(ctx.Request.Query["schemaVersion"].ToString(), out var sv)) schemaVersion = sv;
    bool teamVisible = false;
    if (bool.TryParse(ctx.Request.Query["teamVisible"].ToString(), out var tv)) teamVisible = tv;
    const int CurrentSchemaVersion = 44;
    if (schemaVersion > CurrentSchemaVersion)
    {
        return ApiResponseHelper.Error($"模板文件版本过高（v{schemaVersion}），服务端支持的最高版本为 v{CurrentSchemaVersion}");
    }

    // 2. 大小预检（与 ImportProject 一致的 100MB 上限）
    const long MaxUploadFileSize = 104_857_600L;
    if (ctx.Request.Headers.TryGetValue("Content-Length", out var clValues) &&
        long.TryParse(clValues.ToString(), out var cl) && cl > MaxUploadFileSize)
    {
        ctx.Response.StatusCode = 413;
        return ApiResponseHelper.JsonNet(new { error = "payload_too_large", message = $"文件大小超过 {MaxUploadFileSize / (1024 * 1024)}MB 上限" }, 413);
    }

    // 3. 构造 ProjectDto（Type=1=Template, TemplateId=null 清除模板关联）
    var newTemplateId = Guid.NewGuid();
    var templateDto = new ProjectDto
    {
        Id = newTemplateId,
        Name = string.IsNullOrWhiteSpace(name) ? "导入模板" : name,
        Number = number,
        Category = category,
        Auditee = "",
        Note = note,
        Type = 1, // ProjectType.Template
        Version = 1,
        CreateTime = createTime,
        TemplateId = null,
        TeamVisible = teamVisible
    };

    // 4. 创建主库 Projects 记录（CreateProjectAsync 仅创建主库记录，不创建 .db 文件）
    ProjectDto created;
    try
    {
        created = await svc.CreateProjectAsync(templateDto, userId);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "ImportTemplate: 创建主库记录失败 userId={UserId}", userId);
        return ApiResponseHelper.Error("创建模板记录失败：" + ex.Message);
    }

    var teamId = created.TeamId ?? await teamSvc.GetCurrentUserTeamIdAsync(userId);
    var dbPath = dbManager.GetTemplateDbPath(teamId, newTemplateId);

    // 5. 流式写入 .db 文件（覆盖可能存在的空 .db）
    long bytesWritten = 0;
    var buffer = new byte[81920];
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        await using (var fsOut = new FileStream(dbPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            int read;
            while ((read = await ctx.Request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                bytesWritten += read;
                if (bytesWritten > MaxUploadFileSize)
                {
                    await fsOut.DisposeAsync();
                    TryDeleteFile(dbPath);
                    try { await projectRepo.DeleteFromServerAsync(newTemplateId, teamId); } catch { }
                    ctx.Response.StatusCode = 413;
                    return ApiResponseHelper.JsonNet(new { error = "payload_too_large", message = $"文件大小超过 {MaxUploadFileSize / (1024 * 1024)}MB 上限" }, 413);
                }
                await fsOut.WriteAsync(buffer, 0, read);
            }
        }

        if (bytesWritten == 0)
        {
            TryDeleteFile(dbPath);
            try { await projectRepo.DeleteFromServerAsync(newTemplateId, teamId); } catch { }
            return ApiResponseHelper.Error("上传的模板数据库为空");
        }

        // 6. 校验 .db 的 user_version，避免客户端导入高版本 schema 的 .db
        try
        {
            using var verifyConn = new SqliteConnection($"Data Source={dbPath};Pooling=False;Mode=ReadOnly;");
            await verifyConn.OpenAsync();
            using var verCmd = verifyConn.CreateCommand();
            verCmd.CommandText = "PRAGMA user_version;";
            var verObj = await verCmd.ExecuteScalarAsync();
            var actualVersion = Convert.ToInt32(verObj);
            if (actualVersion > CurrentSchemaVersion)
            {
                TryDeleteFile(dbPath);
                try { await projectRepo.DeleteFromServerAsync(newTemplateId, teamId); } catch { }
                return ApiResponseHelper.Error($"模板数据库版本过高（v{actualVersion}），服务端支持的最高版本为 v{CurrentSchemaVersion}");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "ImportTemplate: 校验 .db schema 失败 templateId={TemplateId}", newTemplateId);
            // 校验失败不阻止导入，仅记录日志
        }

        // 7. 将 .db 全量数据 seed 到主库（关键！否则 SyncMainToProjectDbAsync 会用空主库覆盖 .db 完整数据）
        try
        {
            await SeedMainDbFromProjectDbAsync(db, dbPath, newTemplateId);
        }
        catch (Exception seedEx)
        {
            app.Logger.LogWarning(seedEx, "ImportTemplate: SeedMainDbFromProjectDbAsync 失败: templateId={TemplateId}", newTemplateId);
        }

        // 8. WAL checkpoint（与 ImportProject 一致，确保 .db 主文件包含全部数据）
        try
        {
            dbManager.WalCheckpointWithRetry(dbPath, "ImportTemplate");
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "ImportTemplate: WAL checkpoint 失败 templateId={TemplateId}", newTemplateId);
        }

        app.Logger.LogInformation("ImportTemplate: userId={UserId} templateId={TemplateId} teamId={TeamId} size={Size}",
            userId, newTemplateId, teamId, bytesWritten);

        return ApiResponseHelper.JsonNet(created);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "ImportTemplate: 写入 .db 失败 templateId={TemplateId}", newTemplateId);
        TryDeleteFile(dbPath);
        try { await projectRepo.DeleteFromServerAsync(newTemplateId, teamId); } catch { }
        return ApiResponseHelper.Error("导入模板失败：" + ex.Message);
    }
});

// POST /api/Project/UpdateProject
// Body: Project 对象 JSON；更新项目元数据
app.MapPost("/api/Project/UpdateProject", async (HttpContext ctx, ProjectService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    var project = JsonConvert.DeserializeObject<ProjectDto>(body, ApiResponseHelper.Settings());
    if (project == null || project.Id == Guid.Empty) return ApiResponseHelper.Error("请求体无效");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, project.Id, svc, app.Logger);
    if (accessDenied != null) return accessDenied;

    await svc.UpdateProjectAsync(project);
    return ApiResponseHelper.Ok();
});

// POST /api/Project/UpdateProjectMembers
// Body: Project 对象 JSON（Users 数组含成员 Id）；全量替换项目成员
app.MapPost("/api/Project/UpdateProjectMembers", async (HttpContext ctx, ProjectService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    var project = JsonConvert.DeserializeObject<ProjectDto>(body, ApiResponseHelper.Settings());
    if (project == null || project.Id == Guid.Empty) return ApiResponseHelper.Error("请求体无效");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, project.Id, svc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var members = (project.Users ?? Enumerable.Empty<UserDto>()).Select(u => (UserId: u.Id, Role: u.Role)).ToList();
    await svc.UpdateProjectMembersAsync(project.Id, members);
    return ApiResponseHelper.Ok();
});

// POST /api/Project/DeleteProject
// Body: Guid 序列化为 JSON 字符串（"\"guid\""）；软删除到回收站
app.MapPost("/api/Project/DeleteProject", async (HttpContext ctx, ProjectService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    // 客户端以 JSON 字符串形式传递 Guid，body 形如 "\"guid\"" 或直接 "guid"
    var pidStr = body.Trim().Trim('"').Trim('\'');
    if (!Guid.TryParse(pidStr, out var pid))
        return ApiResponseHelper.Error("无效的 projectId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, pid, svc, app.Logger);
    if (accessDenied != null) return accessDenied;

    await svc.DeleteProjectAsync(pid);
    return ApiResponseHelper.Ok();
});

// POST /api/Project/GetProjectDto
// Body: { ProjectId } JObject；返回项目详情（含 Creator + Users）
app.MapPost("/api/Project/GetProjectDto", async (HttpContext ctx, ProjectService svc, ReviewRepository reviewRepo) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var pidStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    if (!Guid.TryParse(pidStr, out var pid))
        return ApiResponseHelper.Error("无效的 ProjectId");

    // 跨团队访问校验：项目不属于当前团队则返回 403（而非 null，避免信息泄露）
    // Task 5.5：审批人以只读方式打开待审项目——常规校验失败时追加当前节点审批人放行
    var accessDenied = await CheckProjectAccessAsync(ctx, pid, svc, app.Logger, reviewRepo: reviewRepo, allowPendingReviewerAccess: true);
    if (accessDenied != null) return accessDenied;

    var project = await svc.GetProjectDtoAsync(pid);
    return ApiResponseHelper.JsonNet(project);
});

// POST /api/Project/DuplicateProject
// Body: { projectId, newName } JObject；返回新项目 Id
app.MapPost("/api/Project/DuplicateProject", async (HttpContext ctx, ProjectService svc, ProjectDbManager dbManager, SqliteStorage db) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var pidStr = (data["projectId"] ?? data["ProjectId"] ?? data["OldProject"])?.ToString();
    var newName = data.Value<string>("newName") ?? data.Value<string>("NewName")
        ?? data["NewProject"]?.Value<string>("Name") ?? "";
    if (!Guid.TryParse(pidStr, out var pid))
        return ApiResponseHelper.Error("无效的 projectId");

    // 跨团队访问校验：源项目不属于当前团队则拒绝复制
    // 系统模板例外：普通团队成员可复制生成团队副本（复制产物归其团队，无越权写风险）
    var accessDenied = await CheckProjectAccessAsync(ctx, pid, svc, app.Logger, allowMemberCopySystemTemplate: true);
    if (accessDenied != null) return accessDenied;

    var newId = await svc.DuplicateProjectAsync(pid, newName, userId);
    app.Logger.LogInformation("DuplicateProject: source={SourceId} new={NewId} creator={UserId}", pid, newId, userId);

    // 复制项目数据库文件
    // 用无租户查询：系统模板（TeamId=NULL）在租户过滤下查不到会导致 .db 复制被跳过
    var sourceProject = await svc.GetProjectDtoNoTenantAsync(pid);
    if (sourceProject != null && sourceProject.TeamId.HasValue)
    {
        var teamId = sourceProject.TeamId.Value;
        string newDbPath;
        if (sourceProject.Type == 1)
        {
            // V2-C-05 修复：模板复制必须写入 Templates/ 目录
            dbManager.CopyProjectToTemplateDb(pid.ToString(), teamId.ToString(), newId.ToString(), teamId.ToString());
            newDbPath = dbManager.GetTemplateDbPath(teamId, newId);
        }
        else
        {
            dbManager.CopyProjectDb(teamId, pid, newId, newName);
            newDbPath = dbManager.GetProjectDbPath(teamId, newId);
        }
        // 将 .db 全量数据 seed 到主库（与 CreateProject 一致，防止 SyncMainToProjectDbAsync 覆盖全量数据）
        try
        {
            await SeedMainDbFromProjectDbAsync(db, newDbPath, newId);
        }
        catch (Exception seedEx)
        {
            app.Logger.LogWarning(seedEx, "DuplicateProject: SeedMainDbFromProjectDbAsync 失败: projectId={ProjectId}", newId);
        }
    }
    else if (sourceProject != null && sourceProject.Type == 1)
    {
        // 系统模板（TeamId=NULL）→ 复制为当前团队模板：.db 从 _System/Templates/ 复制到团队 Templates/
        if (Guid.TryParse(TenantContextAccessor.Current?.TeamId, out var targetTeamId))
        {
            dbManager.CopyProjectToTemplateDb(pid.ToString(), "", newId.ToString(), targetTeamId.ToString());
            var sysNewDbPath = dbManager.GetTemplateDbPath(targetTeamId, newId);
            try
            {
                await SeedMainDbFromProjectDbAsync(db, sysNewDbPath, newId);
            }
            catch (Exception seedEx)
            {
                app.Logger.LogWarning(seedEx, "DuplicateProject(系统模板): SeedMainDbFromProjectDbAsync 失败: projectId={ProjectId}", newId);
            }
        }
        else
        {
            app.Logger.LogWarning("DuplicateProject: 系统模板复制缺少租户上下文 TeamId，跳过 .db 复制: projectId={ProjectId}", pid);
        }
    }

    return ApiResponseHelper.JsonNet(newId);
});

// POST /api/Project/SaveProjectAsTemplate
// Body: { OldProject: Guid, NewProject: ProjectDto, ClearPermissions: bool }；将项目保存为模板
app.MapPost("/api/Project/SaveProjectAsTemplate", async (HttpContext ctx, ProjectService svc, TeamService teamSvc, ProjectDbManager dbManager) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    
    var oldProjectIdStr = (data["OldProject"] ?? data["oldProject"])?.ToString();
    if (!Guid.TryParse(oldProjectIdStr, out var oldProjectId))
        return ApiResponseHelper.Error("无效的源项目ID");

    var newProject = data["NewProject"]?.ToObject<ProjectDto>();
    if (newProject == null) return ApiResponseHelper.Error("无效的目标模板");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, oldProjectId, svc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var sourceProject = await svc.GetProjectDtoAsync(oldProjectId);
    if (sourceProject == null)
        return ApiResponseHelper.Error("源项目不存在");

    var teamId = sourceProject.TeamId ?? await teamSvc.GetCurrentUserTeamIdAsync(userId);

    newProject.Type = 1;
    newProject.Version = 0;
    newProject.CreateTime = DateTime.Now;
    newProject.TeamId = teamId;

    await svc.CreateProjectAsync(newProject, userId);

    // 复制项目数据库文件到模板目录
    // V2-C-05 修复：目标 Type=1 是模板，必须写入 Templates/ 目录
    // 旧实现 CopyProjectDb 内部目标路径固定为 Projects/，导致模板 .db 永远为空。
    // CopyProjectToTemplateDb 内部根据目标 Type 选择 GetTemplateDbPath 写入正确位置。
    // 若源项目是系统模板（TeamId=NULL），sourceTeamId 传空字符串。
    if (sourceProject.TeamId.HasValue)
    {
        dbManager.CopyProjectToTemplateDb(oldProjectId.ToString(), sourceProject.TeamId.Value.ToString(), newProject.Id.ToString(), teamId.ToString());
    }

    app.Logger.LogInformation("SaveProjectAsTemplate: source={SourceId} new={NewId} creator={UserId}", oldProjectId, newProject.Id, userId);
    return ApiResponseHelper.JsonNet(newProject.Id);
});

// POST /api/Project/PushTemplateToAllTeams
// 仅 IsSystemAdmin 可调用；将指定模板推送到所有团队
app.MapPost("/api/Project/PushTemplateToAllTeams", async (HttpContext ctx, ProjectService svc, UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 权限校验：仅 IsSystemAdmin 可调用
    var caller = await userRepo.GetByIdAsync(userId);
    if (caller == null || !caller.IsSystemAdmin)
        return ApiResponseHelper.Error("仅系统管理员可调用此接口", 403);

    var body = await ReadBodyAsync(ctx);
    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
    if (data == null || !data.TryGetValue("projectId", out var pidStr) || !Guid.TryParse(pidStr, out var sourceProjectId))
        return ApiResponseHelper.Error("请求体无效，需要 projectId");

    var results = await svc.PushTemplateToAllTeamsAsync(sourceProjectId, userId);
    app.Logger.LogInformation("PushTemplateToAllTeams: operator={UserId} source={SourceId} teams={TeamCount}", userId, sourceProjectId, results.Count);
    return ApiResponseHelper.JsonNet(results);
});

// POST /api/Project/ShareProject
// Body: { projectId, userIds } JObject；分享项目给指定用户列表
app.MapPost("/api/Project/ShareProject", async (HttpContext ctx, ProjectService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var pidStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (!Guid.TryParse(pidStr, out var pid))
        return ApiResponseHelper.Error("无效的 projectId");

    // 跨团队访问校验：项目不属于当前团队则拒绝分享
    var accessDenied = await CheckProjectAccessAsync(ctx, pid, svc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var userIds = new List<long>();
    if (data["userIds"] is JArray arr)
    {
        foreach (var t in arr)
        {
            if (long.TryParse(t.ToString(), out var uid)) userIds.Add(uid);
        }
    }
    await svc.ShareProjectAsync(pid, userIds);
    return ApiResponseHelper.Ok();
});

// POST /api/Project/CreateDemo
// 无 Body；创建 IsDemo=1 的演示项目，返回 Project
app.MapPost("/api/Project/CreateDemo", async (HttpContext ctx, ProjectService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var project = await svc.CreateDemoAsync(userId, "演示项目");
    return ApiResponseHelper.JsonNet(project);
});

// POST /api/Project/GetProjectDescendants
// Body: { ProjectId } JObject；返回项目后代树（MVP：仅查一层子项目）
app.MapPost("/api/Project/GetProjectDescendants", async (HttpContext ctx, ProjectService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var pidStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    if (!Guid.TryParse(pidStr, out var pid))
        return ApiResponseHelper.Error("无效的 ProjectId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, pid, svc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var descendants = await svc.GetProjectDescendantsAsync(pid);
    return ApiResponseHelper.JsonNet(descendants);
});

// GET /api/Project/GetTeamPayedProjects?teamId={guid}
// 返回团队已付费项目列表（IEnumerable<Project>）
// V2-H-10 修复：校验 teamId 与当前用户 TeamId 一致（IsSystemAdmin 除外），防止跨租户枚举。
app.MapGet("/api/Project/GetTeamPayedProjects", async (Guid teamId, HttpContext ctx, ProjectService svc, TeamService teamSvc, UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var caller = await userRepo.GetByIdAsync(userId);
    if (caller == null) return ApiResponseHelper.Unauthorized();

    if (!caller.IsSystemAdmin)
    {
        var currentTeamId = await teamSvc.GetCurrentUserTeamIdAsync(userId);
        if (teamId != currentTeamId)
        {
            app.Logger.LogWarning("GetTeamPayedProjects 跨租户访问被拒: userId={UserId} 当前TeamId={CurrentTeamId} 请求TeamId={RequestTeamId}",
                userId, currentTeamId, teamId);
            return ApiResponseHelper.Forbidden("无权访问该团队的项目");
        }
    }

    return ApiResponseHelper.JsonNet(await svc.GetTeamPayedProjectsAsync(teamId));
});

// POST /api/Project/UpdateProjectVersion
// Body: { projectId } JObject；递增 Version，返回 { Version }
app.MapPost("/api/Project/UpdateProjectVersion", async (HttpContext ctx, ProjectService svc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var pidStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (!Guid.TryParse(pidStr, out var pid))
        return ApiResponseHelper.Error("无效的 projectId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, pid, svc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var newVersion = await svc.UpdateProjectVersionAsync(pid);
    return ApiResponseHelper.JsonNet(new { Version = newVersion });
});

// ============= 阶段 10：项目同步接口（PushProjectQuick / PushProject / PullProject / PullProjectDirect / DownloadPullProjectDirect） =============
// 客户端契约（WebApiClient.cs）：
// - PushProjectQuick：POST JObject { Action, Id, Version, Groups, Nodes, DataRefs, VFs }
//   返回 JObject { Result: "Success"|"NoContent"|"OutOfDate", Version: <int> }
// - PushProject：先 UploadTaskInputFile 上传 JSON，再 GET ?taskId&projectId，返回 JObject
// - PullProject：POST JObject { Action, Id, Version }，返回 JObject { Result: "Latest"|"NeedUpdate", ... }
// - PullProjectDirect：GET ?projectId，返回 { taskId }，客户端轮询任务后从 taskResult 解析 { Url, Length }
// - DownloadPullProjectDirect：GET ?projectId&fileName，返回 GZip 压缩的 SQLite 数据库流
//
// 服务端存储：ProjectTreeGroups / ProjectTreeNodes / ProjectDataReferences / ProjectValidationFormulas 4 张表

// ============= 稽核检查：项目级校验规则管理端点 =============
// 供客户端"稽核检查"独立页面在项目未打开时读取/编辑校验规则。
// GetProjectValidations 以 Get* 命名 → 系统模板只读白名单放行；
// Save/Delete 不在白名单 → 系统模板仍需 admin，天然防误改。

// GET /api/Project/GetProjectValidations?projectId={projectId}
// 返回 { formulas: [...], tables: [{Id, Name}] }，tables 供客户端把 TableId 映射为表格名
app.MapGet("/api/Project/GetProjectValidations", async (
    Guid projectId,
    HttpContext ctx,
    SqliteStorage db,
    ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var pid = projectId.ToString();
    using var conn = db.CreateConnection();

    var formulas = new JArray();
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT Id, LeftExpr, Operator, RightExpr, Note, TableId, DocumentFieldId FROM ProjectValidationFormulas WHERE ProjectId = @pid ORDER BY Id";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            formulas.Add(new JObject
            {
                ["Id"] = reader.GetInt64(0),
                ["LeftExpr"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ["Operator"] = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                ["RightExpr"] = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ["Note"] = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ["TableId"] = reader.IsDBNull(5) ? 0 : reader.GetInt64(5),
                ["DocumentFieldId"] = reader.IsDBNull(6) ? 0 : reader.GetInt64(6)
            });
        }
    }

    // 节点名映射（不筛 Type，客户端按 TableId 匹配；文档域规则 TableId=0 时归属显示"文档"）
    var tables = new JArray();
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT Id, Name FROM ProjectTreeNodes WHERE ProjectId = @pid AND Name IS NOT NULL";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(new JObject
            {
                ["Id"] = reader.GetInt64(0),
                ["Name"] = reader.GetString(1)
            });
        }
    }

    return ApiResponseHelper.JsonNet(new { formulas, tables });
});

// POST /api/Project/SaveProjectValidations
// Body: { ProjectId, Formulas: [{ Id, LeftExpr, Operator, RightExpr, Note, TableId, DocumentFieldId }] }
// 逐条 UPSERT，递增项目版本并写入 ProjectChanges（EntityType=3, Action=0/1 视为 Mod）
app.MapPost("/api/Project/SaveProjectValidations", async (
    HttpContext ctx,
    SqliteStorage db,
    ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    if (string.IsNullOrEmpty(body)) return ApiResponseHelper.Error("请求体为空");
    JObject data;
    try { data = JObject.Parse(body); }
    catch (Exception) { return ApiResponseHelper.Error("请求体不是有效的 JSON"); }

    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId");
    if (data["Formulas"] is not JArray formulas || formulas.Count == 0)
        return ApiResponseHelper.Error("Formulas 不能为空");

    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var pid = projectId.ToString();
    using var conn = db.CreateConnection();
    using var tx = conn.BeginTransaction();
    try
    {
        foreach (JObject v in formulas.OfType<JObject>())
        {
            var id = v.Value<long?>("Id") ?? 0;
            if (id == 0) continue;
            using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            cmd.CommandText = @"INSERT OR REPLACE INTO ProjectValidationFormulas
                (Id, ProjectId, LeftExpr, Operator, RightExpr, Note, TableId, DocumentFieldId, Status, Dirty)
                VALUES (@id, @pid, @le, @op, @re, @note, @tid, @dfid, 0, 0)";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.Parameters.AddWithValue("@le", (object?)v.Value<string>("LeftExpr") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@op", v.Value<int?>("Operator") ?? 0);
            cmd.Parameters.AddWithValue("@re", (object?)v.Value<string>("RightExpr") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@note", (object?)v.Value<string>("Note") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tid", (object?)v.Value<long?>("TableId") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dfid", (object?)v.Value<long?>("DocumentFieldId") ?? DBNull.Value);
            cmd.ExecuteNonQuery();

            using var ccmd = conn.CreateCommand();
            ccmd.Transaction = (SqliteTransaction)tx;
            // Payload 字段名归一化：客户端上传的 ValidationFormula DTO 序列化为 "Operator"，
            // 而 PullProject 增量通道的客户端合并逻辑（Syncer）期望 PushProject 报文风格的 "Op"，
            // 不归一会导致其他客户端拉取增量后运算符全部回落为 0（"="）
            var payload = (JObject)v.DeepClone();
            if (payload["Operator"] != null)
            {
                payload["Op"] = payload["Operator"];
                payload.Remove("Operator");
            }
            ccmd.CommandText = @"INSERT INTO ProjectChanges (ProjectId, Version, EntityType, Action, EntityId, Payload)
                VALUES (@pid, (SELECT Version + 1 FROM Projects WHERE Id = @pid2), 3, 1, @eid, @payload)";
            ccmd.Parameters.AddWithValue("@pid", pid);
            ccmd.Parameters.AddWithValue("@pid2", pid);
            ccmd.Parameters.AddWithValue("@eid", id);
            ccmd.Parameters.AddWithValue("@payload", payload.ToString(Newtonsoft.Json.Formatting.None));
            ccmd.ExecuteNonQuery();
        }

        using var verCmd = conn.CreateCommand();
        verCmd.Transaction = (SqliteTransaction)tx;
        verCmd.CommandText = "UPDATE Projects SET Version = Version + 1 WHERE Id = @pid";
        verCmd.Parameters.AddWithValue("@pid", pid);
        verCmd.ExecuteNonQuery();
        tx.Commit();
    }
    catch (Exception ex)
    {
        try { tx.Rollback(); } catch { }
        app.Logger.LogError(ex, "SaveProjectValidations 处理失败: ProjectId={ProjectId}", projectId);
        return ApiResponseHelper.Error($"保存失败: {ex.Message}");
    }
    app.Logger.LogInformation("SaveProjectValidations: ProjectId={ProjectId} Count={Count}", projectId, formulas.Count);
    return ApiResponseHelper.JsonNet(new { Result = "Success" });
});

// POST /api/Project/DeleteProjectValidations
// Body: { ProjectId, Ids: [long] }
app.MapPost("/api/Project/DeleteProjectValidations", async (
    HttpContext ctx,
    SqliteStorage db,
    ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    if (string.IsNullOrEmpty(body)) return ApiResponseHelper.Error("请求体为空");
    JObject data;
    try { data = JObject.Parse(body); }
    catch (Exception) { return ApiResponseHelper.Error("请求体不是有效的 JSON"); }

    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId");
    if (data["Ids"] is not JArray ids || ids.Count == 0)
        return ApiResponseHelper.Error("Ids 不能为空");

    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var pid = projectId.ToString();
    using var conn = db.CreateConnection();
    using var tx = conn.BeginTransaction();
    try
    {
        foreach (var idToken in ids)
        {
            var id = idToken.Value<long>();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            cmd.CommandText = "DELETE FROM ProjectValidationFormulas WHERE Id = @id AND ProjectId = @pid";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.ExecuteNonQuery();

            using var ccmd = conn.CreateCommand();
            ccmd.Transaction = (SqliteTransaction)tx;
            ccmd.CommandText = @"INSERT INTO ProjectChanges (ProjectId, Version, EntityType, Action, EntityId, Payload)
                VALUES (@pid, (SELECT Version + 1 FROM Projects WHERE Id = @pid2), 3, 2, @eid, @payload)";
            ccmd.Parameters.AddWithValue("@pid", pid);
            ccmd.Parameters.AddWithValue("@pid2", pid);
            ccmd.Parameters.AddWithValue("@eid", id);
            ccmd.Parameters.AddWithValue("@payload", new JObject { ["Id"] = id }.ToString(Newtonsoft.Json.Formatting.None));
            ccmd.ExecuteNonQuery();
        }

        using var verCmd = conn.CreateCommand();
        verCmd.Transaction = (SqliteTransaction)tx;
        verCmd.CommandText = "UPDATE Projects SET Version = Version + 1 WHERE Id = @pid";
        verCmd.Parameters.AddWithValue("@pid", pid);
        verCmd.ExecuteNonQuery();
        tx.Commit();
    }
    catch (Exception ex)
    {
        try { tx.Rollback(); } catch { }
        app.Logger.LogError(ex, "DeleteProjectValidations 处理失败: ProjectId={ProjectId}", projectId);
        return ApiResponseHelper.Error($"删除失败: {ex.Message}");
    }
    app.Logger.LogInformation("DeleteProjectValidations: ProjectId={ProjectId} Count={Count}", projectId, ids.Count);
    return ApiResponseHelper.JsonNet(new { Result = "Success" });
});

// POST /api/Project/PushProjectQuick
// Body: JObject { Action, Id (GUID), Version, Groups, Nodes, DataRefs, VFs }
// 返回 JObject { Result: "Success", Version: <int> }
app.MapPost("/api/Project/PushProjectQuick", async (
    HttpContext ctx,
    SqliteStorage db,
    ProjectService projSvc,
    IHubContext<ChatHub> hubContext) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    if (string.IsNullOrEmpty(body)) return ApiResponseHelper.Error("请求体为空");

    JObject data;
    try { data = JObject.Parse(body); }
    catch (Exception) { return ApiResponseHelper.Error("请求体不是有效的 JSON"); }

    var projectIdStr = (data["Id"] ?? data["id"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 Id (projectId)");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 解析客户端 Version（用于乐观锁检查，clientVersion=0 跳过检查兼容老客户端）
    var clientVersion = data.Value<int?>("Version") ?? 0;

    try
    {
        var (newVersion, status) = await ProcessPushProjectAsync(db, projectId, data, clientVersion);
        app.Logger.LogInformation("PushProjectQuick: ProjectId={ProjectId} NewVersion={Version} Status={Status}",
            projectId, newVersion, status);

        if (status == "OutOfDate")
            return ApiResponseHelper.JsonNet(new { Result = "OutOfDate", Version = newVersion });

        // 阶段 3：推送成功后主动广播 ProjectSynced(fromId, projectId, version) 给项目组其他客户端。
        // 与 PushTable/PushDocument 主动广播一致，避免依赖客户端额外调用 SignalRClient.SyncProject。
        // 广播失败不阻断业务（仅记录日志），与 TableSyncService 行为一致。
        try
        {
            var groupName = $"project_{projectId}";
            await hubContext.Clients.Group(groupName)
                .SendAsync("ProjectSynced", userId.ToString(), projectId.ToString(), newVersion.ToString());
        }
        catch (Exception exHub)
        {
            app.Logger.LogWarning(exHub, "PushProjectQuick 广播 ProjectSynced 失败: ProjectId={Pid}", projectId);
        }

        return ApiResponseHelper.JsonNet(new { Result = "Success", Version = newVersion });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "PushProjectQuick 处理失败: ProjectId={ProjectId}", projectId);
        return ApiResponseHelper.Error($"处理失败: {ex.Message}");
    }
});

// GET /api/Project/PushProject?taskId={taskId}&projectId={projectId}
// 客户端先通过 UploadTaskInputFile 上传 JSON 字符串到任务输入文件，再调用此端点
// 返回 JObject { Result: "Success", Version: <int> }
app.MapGet("/api/Project/PushProject", async (
    long taskId,
    Guid projectId,
    HttpContext ctx,
    SqliteStorage db,
    ProjectService projSvc,
    TaskService taskSvc,
    TaskRepository taskRepo,
    IHubContext<ChatHub> hubContext) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // V2-H-13 修复：调用 OpenTaskInput 前校验 taskId 归属当前用户，防止跨用户枚举 taskId 读取他人任务输入文件。
    var task = await taskRepo.GetByIdAsync(taskId);
    if (task == null || task.UserId != userId)
        return ApiResponseHelper.Forbidden("无权访问该任务");

    var cachePath = taskSvc.GetTaskCachePath(taskId);
    if (!File.Exists(cachePath))
        return ApiResponseHelper.Error("任务输入文件不存在");

    try
    {
        using var cacheStream = taskSvc.OpenTaskInput(taskId);
        // 客户端用 Protobuf CodedOutputStream.WriteString 写入 JSON 字符串
        // （格式：Varint 长度前缀 + UTF-8 字符串内容），不能直接用 StreamReader 当纯文本读，
        // 否则第一个字节是 Varint 长度前缀（如 0x0F=\u000f），JObject.Parse 会报
        // "Unexpected character encountered while parsing value: \u000f, Path '', line 0, position 0"
        using var cis = new CodedInputStream(cacheStream);
        var json = cis.ReadString();
        var data = JObject.Parse(json);

        // 解析客户端 Version（用于乐观锁检查，clientVersion=0 跳过检查兼容老客户端）
        var clientVersion = data.Value<int?>("Version") ?? 0;

        var (newVersion, status) = await ProcessPushProjectAsync(db, projectId, data, clientVersion);
        app.Logger.LogInformation("PushProject: ProjectId={ProjectId} TaskId={TaskId} NewVersion={Version} Status={Status}",
            projectId, taskId, newVersion, status);

        if (status == "OutOfDate")
            return ApiResponseHelper.JsonNet(new { Result = "OutOfDate", Version = newVersion });

        // 阶段 3：推送成功后主动广播 ProjectSynced(fromId, projectId, version) 给项目组其他客户端。
        // 与 PushProjectQuick 保持一致。广播失败不阻断业务（仅记录日志）。
        try
        {
            var groupName = $"project_{projectId}";
            await hubContext.Clients.Group(groupName)
                .SendAsync("ProjectSynced", userId.ToString(), projectId.ToString(), newVersion.ToString());
        }
        catch (Exception exHub)
        {
            app.Logger.LogWarning(exHub, "PushProject 广播 ProjectSynced 失败: ProjectId={Pid}", projectId);
        }

        return ApiResponseHelper.JsonNet(new { Result = "Success", Version = newVersion });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "PushProject 处理失败: ProjectId={ProjectId} TaskId={TaskId}", projectId, taskId);
        return ApiResponseHelper.Error($"处理失败: {ex.Message}");
    }
});

// POST /api/Project/PullProject
// Body: JObject { Action: "PullProject", Id (GUID), Version (int) }
// 返回 JObject { Result: "Latest" } 或 { Result: "NeedUpdate", Version, NewGroups, ModGroups, DelGroups, NewNodes, ModNodes, DelNodes, NewRefs, ModRefs, DelRefs, NewVFs, ModVFs, DelVFs }
app.MapPost("/api/Project/PullProject", async (HttpContext ctx, SqliteStorage db, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    if (string.IsNullOrEmpty(body)) return ApiResponseHelper.Error("请求体为空");

    JObject data;
    try { data = JObject.Parse(body); }
    catch (Exception) { return ApiResponseHelper.Error("请求体不是有效的 JSON"); }

    var projectIdStr = (data["Id"] ?? data["id"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 Id (projectId)");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var clientVersion = data.Value<int?>("Version") ?? data.Value<int?>("version") ?? 0;
    var pid = projectId.ToString();

    using var conn = db.CreateConnection();
    await conn.OpenAsync();

    // 查询服务端版本
    int serverVersion;
    using (var verCmd = conn.CreateCommand())
    {
        verCmd.CommandText = "SELECT Version FROM Projects WHERE Id = @pid";
        verCmd.Parameters.AddWithValue("@pid", pid);
        var verResult = await verCmd.ExecuteScalarAsync();
        serverVersion = verResult == null || verResult == DBNull.Value ? 0 : Convert.ToInt32(verResult);
    }

    app.Logger.LogInformation("PullProject: ProjectId={ProjectId} clientVersion={ClientVersion} serverVersion={ServerVersion}",
        projectId, clientVersion, serverVersion);

    if (serverVersion <= clientVersion)
    {
        app.Logger.LogInformation("PullProject: returning Latest (serverVersion <= clientVersion)");
        return ApiResponseHelper.JsonNet(new { Result = "Latest" });
    }

    // 查询增量变更并按实体合并
    // EntityType: 0=Group, 1=Node, 2=DataRef, 3=VF
    // Action: 0=New, 1=Mod, 2=Del
    // 合并规则（按 Version 升序遍历）：
    //   无 → New: 状态=New
    //   无 → Mod: 状态=New（防止客户端不存在该实体，用最新数据当作新建）
    //   无 → Del: 不返回（客户端本来就没有）
    //   New → Mod: 状态=New（用最新数据）
    //   New → Del: 移除（最终不存在）
    //   Mod → Del: 状态=Del
    //   Del → New: 状态=New（重新创建）
    //   Del → Mod: 状态=New（已删除又被修改，按新建处理）

    // 字典：<entityType, entityId> → (finalAction, latestPayload)
    var entityStates = new Dictionary<(int et, long eid), (int action, JObject? payload)>();
    // Mod 当 New 处理时，Payload 只含 dirty 字段，需要从主表补全全量字段
    var fullSnapshotNeeded = new HashSet<(int et, long eid)>();

    using (var chgCmd = conn.CreateCommand())
    {
        chgCmd.CommandText = @"SELECT EntityType, Action, EntityId, Payload
            FROM ProjectChanges
            WHERE ProjectId = @pid AND Version > @cv
            ORDER BY Version ASC, Id ASC";
        chgCmd.Parameters.AddWithValue("@pid", pid);
        chgCmd.Parameters.AddWithValue("@cv", clientVersion);
        using var reader = await chgCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var et = reader.GetInt32(0);
            var act = reader.GetInt32(1);
            var eid = reader.GetInt64(2);
            var payloadStr = reader.IsDBNull(3) ? null : reader.GetString(3);
            var payload = string.IsNullOrEmpty(payloadStr) ? null : JObject.Parse(payloadStr);

            var key = (et, eid);
            if (entityStates.TryGetValue(key, out var existing))
            {
                // 合并规则
                if (existing.action == 0) // 当前是 New
                {
                    if (act == 2) // New → Del: 移除
                    {
                        entityStates.Remove(key);
                    }
                    else // New → Mod/New: 保持 New，更新 payload
                    {
                        entityStates[key] = (0, payload ?? existing.payload);
                    }
                }
                else if (existing.action == 1) // 当前是 Mod
                {
                    if (act == 2) // Mod → Del
                    {
                        entityStates[key] = (2, existing.payload);
                    }
                    else // Mod → Mod/New: 保持 Mod，更新 payload
                    {
                        entityStates[key] = (1, payload ?? existing.payload);
                    }
                }
                else if (existing.action == 2) // 当前是 Del
                {
                    if (act == 0 || act == 1) // Del → New/Mod: 按新建处理
                    {
                        entityStates[key] = (0, payload ?? existing.payload);
                    }
                    // Del → Del: 保持 Del
                }
            }
            else
            {
                // 首次出现
                if (act == 2) // Del 但客户端本来就没有，跳过
                {
                    continue;
                }
                if (act == 1) // Mod 但客户端可能没有，当作 New 处理
                {
                    // 注意：Mod 的 Payload 只含客户端标记 dirty 的字段（如仅 Index），
                    // 缺少 RowWrite/RowRead/Name 等字段。若直接当 New 返回，
                    // 客户端用 (bool)node["RowWrite"] 强转会 ArgumentNullException。
                    // 标记为 NeedFullSnapshot，稍后从主表补全全量字段。
                    entityStates[key] = (0, payload ?? new JObject { ["Id"] = eid });
                    fullSnapshotNeeded.Add(key);
                }
                else // New
                {
                    entityStates[key] = (0, payload);
                }
            }
        }
    }

    // 如果 clientVersion == 0，可能是初次拉取，ProjectChanges 表可能没数据
    // 此时从主表查询全量数据作为 New 返回
    if (clientVersion == 0 && entityStates.Count == 0)
    {
        app.Logger.LogInformation("PullProject: clientVersion=0 且无增量变更，调用 BuildFullSnapshotAsync 从主表返回全量数据");
        await BuildFullSnapshotAsync(conn, pid, entityStates);
    }

    // 补全 "Mod 当 New" 实体的全量字段：从主表读取完整数据覆盖部分字段的 Payload
    if (fullSnapshotNeeded.Count > 0)
    {
        await FillFullSnapshotFromMainTablesAsync(conn, pid, entityStates, fullSnapshotNeeded);
    }

    // 按 EntityType + Action 分组构造响应
    var newGroups = new List<JObject>();
    var modGroups = new List<JObject>();
    var delGroups = new List<JObject>();
    var newNodes = new List<JObject>();
    var modNodes = new List<JObject>();
    var delNodes = new List<JObject>();
    var newRefs = new List<JObject>();
    var modRefs = new List<JObject>();
    var delRefs = new List<JObject>();
    var newVFs = new List<JObject>();
    var modVFs = new List<JObject>();
    var delVFs = new List<JObject>();

    foreach (var ((et, eid), (act, payload)) in entityStates)
    {
        var targetList = (et, act) switch
        {
            (0, 0) => newGroups,
            (0, 1) => modGroups,
            (0, 2) => delGroups,
            (1, 0) => newNodes,
            (1, 1) => modNodes,
            (1, 2) => delNodes,
            (2, 0) => newRefs,
            (2, 1) => modRefs,
            (2, 2) => delRefs,
            (3, 0) => newVFs,
            (3, 1) => modVFs,
            (3, 2) => delVFs,
            _ => null
        };
        if (targetList == null) continue;

        if (act == 2)
        {
            // Del 只需要 Id
            targetList.Add(new JObject { ["Id"] = eid });
        }
        else
        {
            // New/Mod 需要 payload 数据
            if (payload != null)
            {
                // 确保包含 Id 字段
                payload["Id"] = eid;
                targetList.Add(payload);
            }
        }
    }

    app.Logger.LogInformation("PullProject: returning NeedUpdate Version={Version} NewGroups={Ng} ModGroups={Mg} DelGroups={Dg} NewNodes={Nn} ModNodes={Mn} DelNodes={Dn} NewRefs={Nr} ModRefs={Mr} DelRefs={Dr} NewVFs={Nv} ModVFs={Mv} DelVFs={Dv}",
        serverVersion, newGroups.Count, modGroups.Count, delGroups.Count,
        newNodes.Count, modNodes.Count, delNodes.Count,
        newRefs.Count, modRefs.Count, delRefs.Count,
        newVFs.Count, modVFs.Count, delVFs.Count);

    return ApiResponseHelper.JsonNet(new
    {
        Result = "NeedUpdate",
        Version = serverVersion,
        NewGroups = newGroups,
        ModGroups = modGroups,
        DelGroups = delGroups,
        NewNodes = newNodes,
        ModNodes = modNodes,
        DelNodes = delNodes,
        NewRefs = newRefs,
        ModRefs = modRefs,
        DelRefs = delRefs,
        NewVFs = newVFs,
        ModVFs = modVFs,
        DelVFs = delVFs
    });
});

// 从主表查询全量数据作为初次拉取的快照
static async Task BuildFullSnapshotAsync(SqliteConnection conn, string pid,
    Dictionary<(int et, long eid), (int action, JObject? payload)> entityStates)
{
    // Groups
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT Id, Name, TreeIndex FROM ProjectTreeGroups WHERE ProjectId = @pid";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["Name"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ["Index"] = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            };
            entityStates[(0, eid)] = (0, payload);
        }
    }

    // Nodes
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"SELECT Id, GroupId, ParentId, Name, TreeIndex, Type, Number, Permissions, Visible, RowWrite, RowRead
            FROM ProjectTreeNodes WHERE ProjectId = @pid";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["GroupId"] = reader.GetInt64(1),
                ["ParentId"] = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                ["Name"] = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ["Index"] = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                ["Type"] = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                ["Number"] = reader.IsDBNull(6) ? "" : reader.GetString(6),
                ["Permissions"] = reader.IsDBNull(7) ? "" : reader.GetString(7),
                ["Visible"] = reader.IsDBNull(8) ? true : reader.GetInt32(8) != 0,
                ["RowWrite"] = reader.IsDBNull(9) ? false : reader.GetInt32(9) != 0,
                ["RowRead"] = reader.IsDBNull(10) ? false : reader.GetInt32(10) != 0
            };
            entityStates[(1, eid)] = (0, payload);
        }
    }

    // DataRefs
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT Id, Key, Value, Kind FROM ProjectDataReferences WHERE ProjectId = @pid";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["Key"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ["Value"] = reader.IsDBNull(2) ? "" : reader.GetString(2),
                ["Kind"] = reader.IsDBNull(3) ? 2 : reader.GetInt32(3)
            };
            entityStates[(2, eid)] = (0, payload);
        }
    }

    // VFs
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT Id, LeftExpr, Operator, RightExpr, Note, TableId, DocumentFieldId FROM ProjectValidationFormulas WHERE ProjectId = @pid";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["LeftExpr"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ["Op"] = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                ["RightExpr"] = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ["Note"] = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ["TableId"] = reader.IsDBNull(5) ? 0 : reader.GetInt64(5),
                ["DocumentFieldId"] = reader.IsDBNull(6) ? 0 : reader.GetInt64(6)
            };
            entityStates[(3, eid)] = (0, payload);
        }
    }
}

/// <summary>
/// 补全 "Mod 当 New" 实体的全量字段。
/// ProjectChanges.Payload 是客户端推送 Mod 时只含 dirty 字段的部分 JSON，
/// 当客户端拉取时若本地无此实体，服务器会把 Mod 当 New 返回。
/// 此时必须从主表读取完整数据覆盖部分 Payload，否则客户端用 (bool)node["RowWrite"] 强转 null 会抛 ArgumentNullException。
/// </summary>
static async Task FillFullSnapshotFromMainTablesAsync(SqliteConnection conn, string pid,
    Dictionary<(int et, long eid), (int action, JObject? payload)> entityStates,
    HashSet<(int et, long eid)> fullSnapshotNeeded)
{
    // 按 EntityType 分组查询，避免逐条查询
    var nodesNeeded = fullSnapshotNeeded.Where(x => x.et == 1).Select(x => x.eid).ToList();
    var groupsNeeded = fullSnapshotNeeded.Where(x => x.et == 0).Select(x => x.eid).ToList();
    var refsNeeded = fullSnapshotNeeded.Where(x => x.et == 2).Select(x => x.eid).ToList();
    var vfsNeeded = fullSnapshotNeeded.Where(x => x.et == 3).Select(x => x.eid).ToList();

    // Nodes
    if (nodesNeeded.Count > 0)
    {
        var ids = string.Join(",", nodesNeeded);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"SELECT Id, GroupId, ParentId, Name, TreeIndex, Type, Number, Permissions, Visible, RowWrite, RowRead
            FROM ProjectTreeNodes WHERE ProjectId = @pid AND Id IN ({ids})";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["GroupId"] = reader.GetInt64(1),
                ["ParentId"] = reader.IsDBNull(2) ? null : reader.GetInt64(2),
                ["Name"] = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ["Index"] = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                ["Type"] = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                ["Number"] = reader.IsDBNull(6) ? "" : reader.GetString(6),
                ["Permissions"] = reader.IsDBNull(7) ? "" : reader.GetString(7),
                ["Visible"] = reader.IsDBNull(8) ? true : reader.GetInt32(8) != 0,
                ["RowWrite"] = reader.IsDBNull(9) ? false : reader.GetInt32(9) != 0,
                ["RowRead"] = reader.IsDBNull(10) ? false : reader.GetInt32(10) != 0
            };
            entityStates[(1, eid)] = (0, payload);
        }
    }

    // Groups
    if (groupsNeeded.Count > 0)
    {
        var ids = string.Join(",", groupsNeeded);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Id, Name, TreeIndex FROM ProjectTreeGroups WHERE ProjectId = @pid AND Id IN ({ids})";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["Name"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ["Index"] = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            };
            entityStates[(0, eid)] = (0, payload);
        }
    }

    // DataRefs
    if (refsNeeded.Count > 0)
    {
        var ids = string.Join(",", refsNeeded);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Key, Value, Kind FROM ProjectDataReferences WHERE ProjectId = @pid AND Id IN (" + ids + ")";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["Key"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ["Value"] = reader.IsDBNull(2) ? "" : reader.GetString(2),
                ["Kind"] = reader.IsDBNull(3) ? 2 : reader.GetInt32(3)
            };
            entityStates[(2, eid)] = (0, payload);
        }
    }

    // VFs
    if (vfsNeeded.Count > 0)
    {
        var ids = string.Join(",", vfsNeeded);
        using var cmd = conn.CreateCommand();
        // 安全审计修复（High）：原 SQL 查询了不存在的 Name 列且漏了 Operator 列，运行时必抛
        // "no such column: Name"。对照建表语句（SqliteStorage ProjectValidationFormulas：
        // Id/ProjectId/LeftExpr/Operator/RightExpr/Note/TableId）与 BuildFullSnapshotAsync 的正确版本修正。
        cmd.CommandText = "SELECT Id, LeftExpr, Operator, RightExpr, Note, TableId, DocumentFieldId FROM ProjectValidationFormulas WHERE ProjectId = @pid AND Id IN (" + ids + ")";
        cmd.Parameters.AddWithValue("@pid", pid);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eid = reader.GetInt64(0);
            var payload = new JObject
            {
                ["Id"] = eid,
                ["LeftExpr"] = reader.IsDBNull(1) ? "" : reader.GetString(1),
                ["Op"] = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                ["RightExpr"] = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ["Note"] = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ["TableId"] = reader.IsDBNull(5) ? 0 : reader.GetInt64(5),
                ["DocumentFieldId"] = reader.IsDBNull(6) ? 0 : reader.GetInt64(6)
            };
            entityStates[(3, eid)] = (0, payload);
        }
    }
}

// GET /api/Project/PullProjectDirect?projectId={projectId}
// 立即返回 { taskId: <long> }，客户端轮询任务完成后从 taskResult 解析 { Url, Length }
app.MapGet("/api/Project/PullProjectDirect", async (
    Guid projectId,
    HttpContext ctx,
    SqliteStorage db,
    ProjectService projSvc,
    TaskService taskSvc,
    ProjectDbManager dbManager,
    ReviewRepository reviewRepo,
    IWebHostEnvironment env) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 跨团队访问校验
    // Task 5.5：审批人以只读方式打开待审项目——常规校验失败时追加当前节点审批人放行
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger, reviewRepo: reviewRepo, allowPendingReviewerAccess: true);
    if (accessDenied != null) return accessDenied;

    // 构造完整下载 URL 前缀（客户端 _ossClient 没有 BaseAddress，Url 必须是完整 URL）
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/api/Project";

    var taskId = await taskSvc.GenerateTaskIdAsync(userId, "PullProjectDirect");

    taskSvc.StartBackgroundTask(taskId, async (progress, ct) =>
    {
        progress.Report(0.1, "正在准备项目数据库");

        // 临时 SQLite 文件路径
        var tempDir = Path.Combine(env.ContentRootPath, "Files", "PullProjectDirect");
        Directory.CreateDirectory(tempDir);
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var dbFileName = $"{projectId}_{timestamp}.db";
        var dbPath = Path.Combine(tempDir, dbFileName);
        var gzPath = dbPath + ".gz";

        // 优先使用项目专属数据库文件
        // 直接查 Projects 表获取 TeamId/Type，不依赖 GetProjectDtoAsync（受 TenantContextAccessor 限制）
        var projectInfo = await QueryProjectForDirectAsync(db, projectId);
        if (projectInfo == null)
            throw new InvalidOperationException($"项目 {projectId} 不存在");

        bool hasProjectDb = false;

        // 解析 .db 源路径：团队项目/模板按 TeamId 查找；
        // 系统下发模板（TeamId=NULL）的 .db 存放在 _System/Templates/ 目录
        Guid? pullTeamId = projectInfo.Value.TeamId;
        string? sourceDbPath;
        if (pullTeamId.HasValue && pullTeamId.Value != Guid.Empty)
        {
            if (projectInfo.Value.Type == 1)
            {
                sourceDbPath = dbManager.GetTemplateDbPath(pullTeamId.Value, projectId);
                if (!File.Exists(sourceDbPath))
                {
                    sourceDbPath = dbManager.GetSystemTemplateDbPath(projectId);
                }
            }
            else
            {
                sourceDbPath = dbManager.GetProjectDbPath(pullTeamId.Value, projectId);
            }
        }
        else if (projectInfo.Value.Type == 1)
        {
            sourceDbPath = dbManager.GetSystemTemplateDbPath(projectId);
        }
        else
        {
            sourceDbPath = null;
        }

        if (sourceDbPath != null)
        {
            app.Logger.LogInformation("PullProjectDirect 查找数据库: ProjectId={ProjectId} TeamId={TeamId} Type={Type} Path={Path} Exists={Exists}",
                projectId, pullTeamId, projectInfo.Value.Type, sourceDbPath, File.Exists(sourceDbPath));

            if (File.Exists(sourceDbPath))
            {
                // 复制前先 checkpoint WAL 数据到主文件，避免 WAL 文件中的数据丢失
                // 重试 3 次，间隔 200ms，仍失败则记录 warning 但继续复制（避免阻断业务）
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        using var walConn = new SqliteConnection($"Data Source={sourceDbPath};Pooling=False;");
                        await walConn.OpenAsync();
                        using var walCmd = walConn.CreateCommand();
                        walCmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                        await walCmd.ExecuteNonQueryAsync();
                        walConn.Close();
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (attempt < 2)
                        {
                            await Task.Delay(200);
                        }
                        else
                        {
                            app.Logger.LogWarning("PullProjectDirect: wal_checkpoint 失败 3 次，继续复制可能丢失 WAL 数据: {Error}", ex.Message);
                        }
                    }
                }

                File.Copy(sourceDbPath, dbPath, overwrite: true);

                // 复制后对目标文件执行 checkpoint，确保 -wal 干净
                // File.Copy 可能复制了带有 WAL 标记的 .db 头，需要 checkpoint 合并
                try
                {
                    using var dstConn = new SqliteConnection($"Data Source={dbPath};Pooling=False;");
                    await dstConn.OpenAsync();
                    using var dstCmd = dstConn.CreateCommand();
                    dstCmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                    await dstCmd.ExecuteNonQueryAsync();
                    dstConn.Close();
                }
                catch { /* 目标 checkpoint 失败不影响流程 */ }

                hasProjectDb = true;

                // 把主库最新结构（TreeGroups/TreeNodes/DataReferences/ValidationFormulas）和 Version 同步到 .db 文件
                // 原因：File.Copy 仅复制旧 .db 内容，未反映 PushProject 后的主库变更
                // 解决：1) .db Project.Version 恒为 0 导致每次打开误报"数据有更新"
                //      2) PushProject 只写主库不写 .db，客户端看不到他人新增的 TreeNode
                try
                {
                    using var syncConn = new SqliteConnection($"Data Source={dbPath};Pooling=False;");
                    await syncConn.OpenAsync();
                    await SyncMainToProjectDbAsync(db, syncConn, projectId);
                    // 同步后 checkpoint，避免 WAL 残留
                    using var ckptCmd = syncConn.CreateCommand();
                    ckptCmd.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                    await ckptCmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    app.Logger.LogWarning(ex, "PullProjectDirect: SyncMainToProjectDbAsync 失败，继续使用 File.Copy 的旧 .db（Version 可能不准）");
                }

                progress.Report(0.5, "已复制项目数据库");
            }
        }

        if (!hasProjectDb)
        {
            // 项目 .db 文件不存在时，依赖 TemplateId 动态生成 .db 并复制模板内容（首次打开时 .db 可能尚未创建）
            // 无 TemplateId 直接报错，由客户端弹窗提示，不做兜底
            Guid effectiveTemplateId = projectInfo.Value.TemplateId ?? Guid.Empty;
            if (effectiveTemplateId == Guid.Empty)
            {
                app.Logger.LogError("PullProjectDirect: 项目 {ProjectId} 的 .db 文件不存在且无 TemplateId", projectId);
                throw new InvalidOperationException($"项目数据库文件不存在且无模板，请联系管理员重新创建项目。ProjectId={projectId}");
            }

            if (File.Exists(dbPath)) File.Delete(dbPath);
            var connStr = $"Data Source={dbPath};Pooling=False;";
            try
            {
                using (var projConn = new SqliteConnection(connStr))
                {
                    await projConn.OpenAsync();
                    await InitProjectDbSchemaAsync(projConn);
                    await WriteProjectRecordAsync(projConn, projectInfo.Value);

                    await CopyTemplateContentAsync(dbManager, projConn, effectiveTemplateId, projectInfo.Value.TeamId);

                    // V2-C-06 修复：CopyTemplateContentAsync 仅在模板文件不存在时写默认数据；
                    // 若模板 .db 存在但 TreeGroup/TreeNode 表为空（或 CopyTable 异常被吞掉），
                    // 结果 .db 中没有任何 TreeGroup/TreeNode，客户端打开后树形为空。
                    // 此处后置校验：若 TreeGroup/TreeNode 均为空，补写默认数据。
                    await EnsureDefaultTreeNodesAsync(projConn);

                    // 模板生成后也同步主库最新结构（覆盖模板默认节点为主库权威数据）
                    // 仅当主库已有该项目数据时同步（首次创建项目主库可能为空，SyncMainToProjectDbAsync 内部会判断）
                    try
                    {
                        await SyncMainToProjectDbAsync(db, projConn, projectId);
                    }
                    catch (Exception ex)
                    {
                        app.Logger.LogWarning(ex, "PullProjectDirect: 模板路径 SyncMainToProjectDbAsync 失败，使用模板内容");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            progress.Report(0.5, "已生成项目数据库");
        }

        progress.Report(0.7, "正在压缩数据库");

        // GZip 压缩
        using (var srcStream = File.OpenRead(dbPath))
        using (var dstStream = File.Create(gzPath))
        using (var gzStream = new GZipStream(dstStream, CompressionLevel.Optimal))
        {
            await srcStream.CopyToAsync(gzStream);
        }

        // 删除未压缩的临时文件
        try { File.Delete(dbPath); } catch { /* 忽略 */ }

        var gzFileName = Path.GetFileName(gzPath);
        var fileLength = new FileInfo(gzPath).Length;
        var downloadUrl = $"{baseUrl}/DownloadPullProjectDirect?projectId={projectId}&fileName={gzFileName}";

        progress.Report(1.0, "完成");

        var resultJson = JsonConvert.SerializeObject(new { Url = downloadUrl, Length = fileLength });
        app.Logger.LogInformation("PullProjectDirect 完成: ProjectId={ProjectId} TaskId={TaskId} File={File} Size={Size}",
            projectId, taskId, gzFileName, fileLength);
        return resultJson;
    });

    return ApiResponseHelper.JsonNet(new { taskId });
});

// GET /api/Project/DownloadPullProjectDirect?projectId={projectId}&fileName={fileName}
// 返回 GZip 压缩的 SQLite 数据库流（客户端用独立 _ossClient 下载）
app.MapGet("/api/Project/DownloadPullProjectDirect", async (
    Guid projectId,
    string fileName,
    HttpContext ctx,
    SqliteStorage db,
    ProjectService projSvc,
    IWebHostEnvironment env) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 防止路径遍历攻击：仅使用文件名，不含路径
    var safeFileName = Path.GetFileName(fileName);
    // 安全审计修复（Med）：校验文件名确实由该 projectId 生成（PullProjectDirect 的产物名为
    // "{projectId}_{timestamp}.db.gz"），防止已登录用户用他人项目的文件名跨项目拉取数据库。
    if (!safeFileName.StartsWith($"{projectId}_", StringComparison.OrdinalIgnoreCase))
    {
        app.Logger.LogWarning("DownloadPullProjectDirect 拒绝（文件名与项目不匹配）: userId={UserId} projectId={ProjectId} fileName={FileName}",
            userId, projectId, safeFileName);
        return ApiResponseHelper.NotFound("文件不存在");
    }
    var filePath = Path.Combine(env.ContentRootPath, "Files", "PullProjectDirect", safeFileName);
    if (!File.Exists(filePath))
        return ApiResponseHelper.NotFound("文件不存在");

    var fileBytes = await File.ReadAllBytesAsync(filePath);
    ctx.Response.ContentType = "application/gzip";
    ctx.Response.Headers.ContentDisposition = $"attachment; filename=\"{safeFileName}\"";
    ctx.Response.Headers["FileLength"] = fileBytes.Length.ToString();
    await ctx.Response.Body.WriteAsync(fileBytes);
    return Results.Empty;
});

// ============= 阶段 5 Task 5.1：异步任务系统接口（4 个 ServerTask/* 端点） =============
// 客户端通过 WebApiClient.SendTaskInputFileToServer / WaitingServerTaskRunOver 调用。
// 任务状态响应格式严格匹配：{progressValue, isTaskEnd, isTaskSuccess, isTimeOut, taskDesc, taskResult}（camelCase）。
// 失败时 taskDesc 必须以 "BadRequestMessage:" 开头（客户端据此识别错误并抛 NormalException）。
//
// 阶段 6 修正：客户端 WebApiClient.SendTaskInputFileToServer 中 `(long)(await SendAsObject<JObject>(...))["taskId"]`
// 强制要求 GenerateTaskId 返回 { taskId: <long> } 格式，且 taskId 必须为 long（非 GUID 字符串）。
// 内部使用 SQLite rowid（自增）作为 TaskId。

// GET /api/ServerTask/GenerateTaskId
// Header: UserId + Token；返回 { taskId: <long> }
app.MapGet("/api/ServerTask/GenerateTaskId", async (HttpContext ctx, TaskService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    var taskId = await svc.GenerateTaskIdAsync(userId, "Generic");
    return ApiResponseHelper.JsonNet(new { taskId });
});

// GET /api/ServerTask/GetTaskRunningStatus?taskId={taskId}
// 返回 TaskStatusDto（progressValue, isTaskEnd, isTaskSuccess, isTimeOut, taskDesc, taskResult）
// 安全审计修复（High）：原实现任意已登录用户可查询任意 taskId。修复：传入 userId 在服务层校验任务归属。
app.MapGet("/api/ServerTask/GetTaskRunningStatus", async (long taskId, HttpContext ctx, TaskService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    try
    {
        var status = await svc.GetTaskStatusAsync(taskId, userId);
        return ApiResponseHelper.JsonNet(status);
    }
    catch (UnauthorizedAccessException ex)
    {
        return ApiResponseHelper.Unauthorized(ex.Message);
    }
});

// POST /api/ServerTask/UploadTaskInputFile?taskId={taskId}&offset={offset}
// Body: 二进制流（分片，最大 2MB/片），按 offset 随机写入临时文件
// 安全审计修复（High）：原实现任意已登录用户可上传到任意 taskId。修复：传入 userId 在服务层校验任务归属。
app.MapPost("/api/ServerTask/UploadTaskInputFile", async (long taskId, long offset, HttpContext ctx, TaskService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    try
    {
        await svc.UploadChunkAsync(taskId, offset, ctx.Request.Body, userId);
        return ApiResponseHelper.Ok();
    }
    catch (UnauthorizedAccessException ex)
    {
        return ApiResponseHelper.Unauthorized(ex.Message);
    }
});

// GET /api/ServerTask/ClearTaskCacheData?taskId={taskId}
// 删除临时文件 + ServerTasks 记录 + 内存状态
// 安全审计修复（High）：原实现任意已登录用户可清理任意 taskId。修复：传入 userId 在服务层校验任务归属。
app.MapGet("/api/ServerTask/ClearTaskCacheData", async (long taskId, HttpContext ctx, TaskService svc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    try
    {
        await svc.ClearTaskCacheAsync(taskId, userId);
        return ApiResponseHelper.Ok();
    }
    catch (UnauthorizedAccessException ex)
    {
        return ApiResponseHelper.Unauthorized(ex.Message);
    }
});

// GET /api/ServerTask/DownloadTaskResult?taskId={taskId}
// V2-H-01 修复：注册缺失的下载端点。GetTableRevertDiffAsync / GetDocumentRevertDiffAsync 返回的 url 指向此端点，
// 之前因未注册导致客户端按返回 url 下载得到 404。
// 安全：校验 taskId 归属（ServerTasks.UserId == 当前用户），防止跨用户枚举 taskId 下载他人任务结果。
app.MapGet("/api/ServerTask/DownloadTaskResult", async (
    long taskId,
    HttpContext ctx,
    TaskRepository taskRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 校验任务归属
    var task = await taskRepo.GetByIdAsync(taskId);
    if (task == null || task.UserId != userId)
        return ApiResponseHelper.Forbidden("无权访问该任务结果");

    // 优先返回 OutputFilePath 指向的文件（异步任务持久化的结果文件）
    if (!string.IsNullOrEmpty(task.OutputFilePath) && File.Exists(task.OutputFilePath))
    {
        var fileBytes = await File.ReadAllBytesAsync(task.OutputFilePath);
        ctx.Response.ContentType = "application/octet-stream";
        ctx.Response.Headers.ContentDisposition = $"attachment; filename=\"task_{taskId}.bin\"";
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Response.Headers["FileLength"] = fileBytes.Length.ToString();
        await ctx.Response.Body.WriteAsync(fileBytes);
        return Results.Empty;
    }

    // 任务无文件输出时返回 Result 字段内容（MVP 模式：直接返回 PullTable 字节，结果保存在 Result 字段）
    if (!string.IsNullOrEmpty(task.Result))
    {
        var resultBytes = System.Text.Encoding.UTF8.GetBytes(task.Result);
        ctx.Response.ContentType = "application/octet-stream";
        ctx.Response.Headers.ContentDisposition = $"attachment; filename=\"task_{taskId}.bin\"";
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Response.Headers["FileLength"] = resultBytes.Length.ToString();
        await ctx.Response.Body.WriteAsync(resultBytes);
        return Results.Empty;
    }

    return ApiResponseHelper.NotFound("任务结果文件不存在");
});

// ============= 阶段 6 Task 6.3 + 6.4：表格/文档 Protobuf 同步接口（15 个端点） =============
// 客户端调用流程：
// - PushTableQuick：cells.Count ≤ 1000 时直接 POST PushTable Protobuf 字节流，返回 JObject { Result, Version }
// - PushTable：cells.Count > 1000 时先 UploadTaskInputFile，再 GET 调用携带 taskId/projectId/tableId/version
// - PullTable：POST { projectId, tableId, version } JSON，返回 PullTable Protobuf 字节流
// - RevertTable/GetTableRevertDiff：POST JObject，返回 { taskId, url? } 供客户端轮询
// - GetTableTimeline/QueryTableVersions/GetTableColumns：POST JObject，返回 JArray

// POST /api/Project/PushTableQuick
// Body: PushTable Protobuf 字节流（client 检测到 Body 为 IMessage 时使用 CodedOutputStream 序列化）
// 返回 JObject { Result: "OK", Version: <int> }
app.MapPost("/api/Project/PushTableQuick", async (HttpContext ctx, TableSyncService svc, TaskService taskSvc, ProjectService projSvc, ArchiveRepository archiveRepo, ReviewRepository reviewRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    using var ms = new MemoryStream();
    await ctx.Request.Body.CopyToAsync(ms);
    var bytes = ms.ToArray();
    if (bytes.Length == 0) return ApiResponseHelper.Error("请求体为空");

    // 解析 Protobuf（仅一次）：同时用于提取 projectId 访问校验 + 业务处理
    PushTable pushTable;
    try { pushTable = PushTable.Parser.ParseFrom(bytes); }
    catch (InvalidProtocolBufferException ex)
    {
        // 诊断日志：记录前 32 字节 hex 便于排查客户端是否发错了格式
        byte[] head = bytes.Length >= 32 ? bytes.AsSpan(0, 32).ToArray() : bytes;
        app.Logger.LogWarning(ex,
            "PushTableQuick Protobuf 解析失败: BytesLength={Len}, HeadHex={Head}, UserId={UserId}",
            bytes.Length, Convert.ToHexString(head), userId);
        return ApiResponseHelper.Error("无效的 Protobuf 数据: " + ex.Message);
    }
    var projectId = new Guid(pushTable.ProjectId.ToByteArray());
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 归档锁定：已归档项目禁止推送（Archived）
    if (await archiveRepo.IsProjectArchivedAsync(projectId))
        return ApiResponseHelper.JsonNet(new { Result = "Archived", code = "Archived", error = "项目已归档，禁止修改", message = "项目已归档，禁止修改" }, 409);

    // 审批人只读会话（Task 5.5）：非项目成员的当前节点审批人（只读访问者）禁止推送
    if (await reviewRepo.GetPendingSubmissionForReviewerAsync(projectId, userId) != null
        && !await reviewRepo.IsProjectMemberAsync(projectId, userId))
        return ApiResponseHelper.JsonNet(new { Result = "ReadOnly", code = "ReadOnly", error = "审批人只读会话，禁止修改", message = "审批人只读会话，禁止修改" }, 409);

    // 直接传入已解析的 PushTable 对象，避免 TableSyncService 里重复 ParseFrom 同一份 bytes
    var result = await svc.PushTableQuickAsync(pushTable, userId);
    return ApiResponseHelper.JsonNet(result);
});

// GET /api/Project/PushTable?taskId={taskId}&projectId={projectId}&tableId={tableId}&version={version}
// 返回 JObject { Result: "OK", Version: <int> } 或 { Result: "WaitingTaskEnd:{...}" }
app.MapGet("/api/Project/PushTable", async (
    long taskId,
    Guid projectId,
    Guid tableId,
    int version,
    HttpContext ctx,
    TableSyncService svc,
    TaskService taskSvc,
    TaskRepository taskRepo,
    ProjectService projSvc,
    ArchiveRepository archiveRepo,
    ReviewRepository reviewRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // V2-H-13 修复：调用 OpenTaskInput 前校验 taskId 归属当前用户，防止跨用户枚举 taskId 读取他人任务输入文件。
    var task = await taskRepo.GetByIdAsync(taskId);
    if (task == null || task.UserId != userId)
        return ApiResponseHelper.Forbidden("无权访问该任务");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 归档锁定：已归档项目禁止推送（Archived）
    if (await archiveRepo.IsProjectArchivedAsync(projectId))
        return ApiResponseHelper.JsonNet(new { Result = "Archived", code = "Archived", error = "项目已归档，禁止修改", message = "项目已归档，禁止修改" }, 409);

    // 审批人只读会话（Task 5.5）：非项目成员的当前节点审批人（只读访问者）禁止推送
    if (await reviewRepo.GetPendingSubmissionForReviewerAsync(projectId, userId) != null
        && !await reviewRepo.IsProjectMemberAsync(projectId, userId))
        return ApiResponseHelper.JsonNet(new { Result = "ReadOnly", code = "ReadOnly", error = "审批人只读会话，禁止修改", message = "审批人只读会话，禁止修改" }, 409);

    var cachePath = taskSvc.GetTaskCachePath(taskId);
    if (!File.Exists(cachePath))
        return ApiResponseHelper.Error("任务输入文件不存在");

    using var cacheStream = taskSvc.OpenTaskInput(taskId);
    // 安全修复（跨租户越权写入）：TableSyncService.PushTableAsync 的参数仅用于日志，
    // 实际写入目标取自 protobuf 内的 ProjectId（TableSyncService.cs 中 new Guid(pushTable.ProjectId...)）。
    // 上面的访问校验却基于 URL 上的 projectId。两者必须一致，否则可用自己有权访问的 projectId
    // 通过校验，却在 body 里填入他人项目 GUID 完成越权写入。
    using var uploadedTableBody = new MemoryStream();
    await cacheStream.CopyToAsync(uploadedTableBody);
    uploadedTableBody.Position = 0;
    PushTable uploadedPushTable;
    try { uploadedPushTable = PushTable.Parser.ParseFrom(uploadedTableBody); }
    catch (InvalidProtocolBufferException ex)
    {
        app.Logger.LogWarning(ex, "PushTable(GET) Protobuf 解析失败: TaskId={TaskId}", taskId);
        return ApiResponseHelper.Error("无效的 Protobuf 数据: " + ex.Message);
    }
    var bodyProjectId = new Guid(uploadedPushTable.ProjectId.ToByteArray());
    if (bodyProjectId != projectId)
    {
        app.Logger.LogWarning("PushTable(GET) 拒绝：请求体项目({BodyProjectId})与 URL 项目({UrlProjectId})不一致 UserId={UserId}",
            bodyProjectId, projectId, userId);
        return ApiResponseHelper.Forbidden("请求体中的项目与 URL 中的项目不一致");
    }
    uploadedTableBody.Position = 0;
    var result = await svc.PushTableAsync(taskId, projectId, tableId, version, userId, uploadedTableBody);
    return ApiResponseHelper.JsonNet(result);
});

// POST /api/Project/PullTable
// Body: { projectId, tableId, version } JSON
// 返回 PullTable Protobuf 字节流（Content-Type: application/x-protobuf）
app.MapPost("/api/Project/PullTable", async (HttpContext ctx, TableSyncService svc, ProjectService projSvc) =>
{
    // V2-H-03 修复：补 ParseUserId 与项目成员校验，防止跨租户 IDOR 数据泄露。
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var tableIdStr = (data["tableId"] ?? data["TableId"])?.ToString();
    var clientVersion = data.Value<int?>("version") ?? data.Value<int?>("Version") ?? 0;

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(tableIdStr, out var tableId))
        return ApiResponseHelper.Error("无效的 tableId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var (bytes, isEmpty) = await svc.PullTableAsync(projectId, tableId, clientVersion);
    ctx.Response.ContentType = "application/x-protobuf";
    await ctx.Response.Body.WriteAsync(bytes);
    return Results.Empty;
});

// POST /api/Project/AcquireTableLock
// 节点级强锁：客户端打开表格节点时调用，获取对该表格的编辑锁。
// Body: { ProjectId, TableId }
// 返回 { Result: "Success", Locker: <userId> } 或 { Result: "Locked", Locker: <userId>, LockerName: <name> }
app.MapPost("/api/Project/AcquireTableLock", async (
    HttpContext ctx,
    SqliteStorage db,
    ProjectDbManager projDbMgr,
    ProjectService projSvc,
    IHubContext<ChatHub> hubContext) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    if (string.IsNullOrEmpty(body)) return ApiResponseHelper.Error("请求体为空");
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体不是有效的 JSON"); }

    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    var tableIdStr = (data["TableId"] ?? data["tableId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId");
    if (!long.TryParse(tableIdStr, out var tableId))
        return ApiResponseHelper.Error("无效的 TableId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 获取 project.TeamId（无租户查询：系统模板 TeamId=NULL，租户过滤下查不到会导致误判"未关联团队"）
    var projectDto = await projSvc.GetProjectDtoNoTenantAsync(projectId);
    var teamId = projectDto?.TeamId ?? Guid.Empty;

    // 系统模板：OpenProjectDb 内部回退到 _System/Templates/ 路径
    using var conn = projDbMgr.OpenProjectDb(teamId, projectId);

    // 安全审计修复（High，TOCTOU）：原实现"先 SELECT Locker 判断、再无条件 UPDATE"两步非原子，
    // 并发下两个用户可同时通过判断双双获锁。改为单条原子 CAS UPDATE：
    // 仅当 Locker=0 / Locker=本人（同用户多端）/ 无获取时间 / 已过期（30 分钟，UTC 存储、字符串比较与
    // 原 IsLockExpiredForProgram 的过期阈值一致）时才获锁，按受影响行数判定 Success / Locked。
    var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    var staleTime = DateTime.UtcNow.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss");
    long affected;
    using (var updCmd = conn.CreateCommand())
    {
        updCmd.CommandText = @"
            UPDATE `Table` SET `Locker`=@userId, `LockerAcquiredAt`=@now
            WHERE Id=@id AND (`Locker`=0 OR `Locker`=@userId OR `LockerAcquiredAt` IS NULL OR `LockerAcquiredAt` < @staleTime)";
        updCmd.Parameters.AddWithValue("@userId", userId);
        updCmd.Parameters.AddWithValue("@now", now);
        updCmd.Parameters.AddWithValue("@id", tableId);
        updCmd.Parameters.AddWithValue("@staleTime", staleTime);
        affected = await updCmd.ExecuteNonQueryAsync();
    }

    if (affected == 0)
    {
        // 查询 Locker 用户名（仅用于返回提示）
        long currentLocker = 0;
        using (var readCmd = conn.CreateCommand())
        {
            readCmd.CommandText = "SELECT `Locker` FROM `Table` WHERE Id=@id";
            readCmd.Parameters.AddWithValue("@id", tableId);
            var lockerObj = await readCmd.ExecuteScalarAsync();
            currentLocker = (lockerObj == null || lockerObj == DBNull.Value) ? 0 : Convert.ToInt64(lockerObj);
        }
        string lockerName = await GetUserNameAsync(db, currentLocker);
        app.Logger.LogInformation("AcquireTableLock 拒绝: TableId={TableId} 被用户 {Locker} 锁定", tableId, currentLocker);
        return ApiResponseHelper.JsonNet(new { Result = "Locked", Locker = currentLocker, LockerName = lockerName });
    }

    // 尽力合并 WAL
    try
    {
        using var ckptCmd = conn.CreateCommand();
        ckptCmd.CommandText = "PRAGMA wal_checkpoint(PASSIVE);";
        await ckptCmd.ExecuteNonQueryAsync();
    }
    catch { /* PASSIVE checkpoint 失败不阻断业务 */ }

    app.Logger.LogInformation("AcquireTableLock 成功: TableId={TableId} UserId={UserId}", tableId, userId);

    // 阶段 B 实时广播：通知项目组其他客户端 PeerTableLockChanged
    try
    {
        var groupName = $"project_{projectId}";
        await hubContext.Clients.Group(groupName)
            .SendAsync("PeerTableLockChanged", projectId.ToString(), tableId.ToString(), userId.ToString());
    }
    catch (Exception exHub)
    {
        app.Logger.LogWarning(exHub, "AcquireTableLock 广播 PeerTableLockChanged 失败: TableId={Tid}", tableId);
    }

    return ApiResponseHelper.JsonNet(new { Result = "Success", Locker = userId });
});

// POST /api/Project/ReleaseTableLock
// 节点级强锁：客户端切换节点/关闭项目/退出应用时调用，释放对该表格的编辑锁。
// 仅当 Locker==userId 才释放（避免误释放他人锁）。
// Body: { ProjectId, TableId }
// 返回 { Result: "Success" }
app.MapPost("/api/Project/ReleaseTableLock", async (
    HttpContext ctx,
    SqliteStorage db,
    ProjectDbManager projDbMgr,
    ProjectService projSvc,
    IHubContext<ChatHub> hubContext) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    if (string.IsNullOrEmpty(body)) return ApiResponseHelper.Error("请求体为空");
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体不是有效的 JSON"); }

    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    var tableIdStr = (data["TableId"] ?? data["tableId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId");
    if (!long.TryParse(tableIdStr, out var tableId))
        return ApiResponseHelper.Error("无效的 TableId");

    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var projectDto = await projSvc.GetProjectDtoNoTenantAsync(projectId);
    var teamId = projectDto?.TeamId ?? Guid.Empty;

    // 系统模板：OpenProjectDb 内部回退到 _System/Templates/ 路径
    using var conn = projDbMgr.OpenProjectDb(teamId, projectId);

    // 仅当 Locker==userId 才释放（避免误释放他人锁）
    using (var updCmd = conn.CreateCommand())
    {
        updCmd.CommandText = "UPDATE `Table` SET `Locker`=0, `LockerAcquiredAt`=NULL WHERE Id=@id AND `Locker`=@userId";
        updCmd.Parameters.AddWithValue("@id", tableId);
        updCmd.Parameters.AddWithValue("@userId", userId);
        await updCmd.ExecuteNonQueryAsync();
    }

    // WAL checkpoint
    try
    {
        using var ckptCmd = conn.CreateCommand();
        ckptCmd.CommandText = "PRAGMA wal_checkpoint(PASSIVE);";
        await ckptCmd.ExecuteNonQueryAsync();
    }
    catch { }

    app.Logger.LogInformation("ReleaseTableLock: TableId={TableId} UserId={UserId}", tableId, userId);

    // 阶段 B 实时广播：lockerUserId=0 表示释放
    try
    {
        var groupName = $"project_{projectId}";
        await hubContext.Clients.Group(groupName)
            .SendAsync("PeerTableLockChanged", projectId.ToString(), tableId.ToString(), "0");
    }
    catch (Exception exHub)
    {
        app.Logger.LogWarning(exHub, "ReleaseTableLock 广播 PeerTableLockChanged 失败: TableId={Tid}", tableId);
    }

    return ApiResponseHelper.JsonNet(new { Result = "Success" });
});

// 节点级强锁辅助：查询用户显示名（Name 或 UserName）
static async Task<string> GetUserNameAsync(SqliteStorage db, long userId)
{
    try
    {
        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(Name, UserName) FROM Users WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", userId);
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString() ?? $"用户{userId}";
    }
    catch
    {
        return $"用户{userId}";
    }
}

// POST /api/Project/RevertTable
// Body: { projectId, tableId, targetVersion } JSON
// 返回 JObject { taskId: <long> }
app.MapPost("/api/Project/RevertTable", async (HttpContext ctx, TableSyncService svc, TaskService taskSvc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var tableIdStr = (data["tableId"] ?? data["TableId"])?.ToString();
    var targetVersion = data.Value<int?>("targetVersion") ?? data.Value<int?>("TargetVersion") ?? data.Value<int?>("revertVersion") ?? data.Value<int?>("RevertVersion") ?? 0;

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(tableIdStr, out var tableId))
        return ApiResponseHelper.Error("无效的 tableId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var result = await svc.RevertTableAsync(projectId, tableId, targetVersion, userId, taskSvc);
    // 若 result 包含 error 字段则返回错误响应
    var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(result);
    var resultObj = JObject.Parse(resultJson);
    var errToken = resultObj["error"];
    if (errToken != null)
        return ApiResponseHelper.Error(errToken.ToString());
    return ApiResponseHelper.JsonNet(result);
});

// POST /api/Project/GetTableRevertDiff
// Body: { projectId, tableId, targetVersion } JSON
// 返回 JObject { taskId: <long>, url: <string> }
app.MapPost("/api/Project/GetTableRevertDiff", async (HttpContext ctx, TableSyncService svc, TaskService taskSvc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var tableIdStr = (data["tableId"] ?? data["TableId"])?.ToString();
    var targetVersion = data.Value<int?>("targetVersion") ?? data.Value<int?>("TargetVersion") ?? data.Value<int?>("revertVersion") ?? data.Value<int?>("RevertVersion") ?? 0;

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(tableIdStr, out var tableId))
        return ApiResponseHelper.Error("无效的 tableId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var (bytes, taskId, url) = await svc.GetTableRevertDiffAsync(projectId, tableId, targetVersion, userId, taskSvc);
    // MVP：直接返回 taskId 和 url（客户端会等待任务完成后下载结果文件）
    return ApiResponseHelper.JsonNet(new { taskId, url });
});

// POST /api/Project/GetTableTimeline
// Body: { projectId, tableId } JSON
// 返回 JArray [{ id, version, changeType, createdAt, createdBy }]
app.MapPost("/api/Project/GetTableTimeline", async (HttpContext ctx, TableSyncService svc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var tableIdStr = (data["tableId"] ?? data["TableId"])?.ToString();

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(tableIdStr, out var tableId))
        return ApiResponseHelper.Error("无效的 tableId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var timeline = await svc.GetTableTimelineAsync(projectId, tableId);
    return ApiResponseHelper.JsonNet(timeline);
});

// POST /api/Project/QueryTableVersions
// Body: { projectId, TableVersions: [{Id}] } JSON（批量查询每个表格的最新版本）
//   返回 JArray [{ Id, Version }]，与 QueryImageVersions/QueryPdfVersions 契约一致。
//   客户端 Syncer.QueryVersion 用此结果判断哪些表格需要同步。
// 兼容旧契约：{ projectId, tableId } → 返回 JArray [{ version, changeType, createdAt }]（版本历史），
//   供 MCP/Scenario 工具（cloud_query_table_versions）使用。
app.MapPost("/api/Project/QueryTableVersions", async (HttpContext ctx, SqliteStorage db, TableSyncService svc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.JsonNet(new JArray());

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 批量查询：{ ProjectId, TableVersions: [{Id}] } → [{ Id, Version }]
    var versionsArr = data["TableVersions"] ?? data["tableVersions"];
    if (versionsArr is JArray arr)
    {
        // 表格 Id 为 long，VersionHistory.TargetId 以 LongToGuid(id) 的 Guid 字符串存储，
        // 因此查询前需将 long 转换为对应 Guid 字符串。
        var result = new JArray();
        // 先收集 long → Guid 字符串映射，再一次性批量查询（原为 foreach 逐条新建连接查询），
        // 项目表很多时逐条查询会打满客户端 30 秒超时并中断整批同步。
        var pending = new List<(long? LongId, string RawId, string TargetIdForQuery)>();
        foreach (var item in arr)
        {
            var idStr = (item["Id"] ?? item["id"])?.ToString() ?? "";
            if (string.IsNullOrEmpty(idStr)) continue;
            string targetIdForQuery = idStr;
            bool isLong = long.TryParse(idStr, out var longId);
            if (isLong)
            {
                var bytes = new byte[16];
                BitConverter.GetBytes(longId).CopyTo(bytes, 0);
                targetIdForQuery = new Guid(bytes).ToString();
            }
            pending.Add((isLong ? longId : (long?)null, idStr, targetIdForQuery));
        }
        var versionMap = await QueryLatestTargetVersionsAsync(db, projectId, "Table", pending.Select(p => p.TargetIdForQuery));
        foreach (var p in pending)
        {
            versionMap.TryGetValue(p.TargetIdForQuery, out var v);
            result.Add(new JObject { ["Id"] = p.LongId.HasValue ? (JToken)p.LongId.Value : p.RawId, ["Version"] = v });
        }
        return ApiResponseHelper.JsonNet(result);
    }

    // 兼容旧契约：{ projectId, tableId } → 版本历史 [{ version, changeType, createdAt }]
    var tableIdStr = (data["tableId"] ?? data["TableId"])?.ToString();
    if (Guid.TryParse(tableIdStr, out var tableId))
    {
        var versions = await svc.QueryTableVersionsAsync(projectId, tableId);
        return ApiResponseHelper.JsonNet(versions);
    }
    return ApiResponseHelper.JsonNet(new JArray());
});

// POST /api/Project/GetTableColumns
// Body: { projectId, tableId } JSON
// 返回 JArray（MVP：返回原始 BLOB 字节数组 base64 编码，或空数组）
app.MapPost("/api/Project/GetTableColumns", async (HttpContext ctx, TableSyncService svc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var tableIdStr = (data["tableId"] ?? data["TableId"])?.ToString();

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(tableIdStr, out var tableId))
        return ApiResponseHelper.Error("无效的 tableId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var columnsBlob = await svc.GetTableColumnsAsync(projectId, tableId);
    if (columnsBlob == null || columnsBlob.Length == 0)
        return ApiResponseHelper.JsonNet(new JArray());

    // MVP：尝试解析为 JArray；解析失败则返回 base64 字符串包装对象
    try
    {
        var json = System.Text.Encoding.UTF8.GetString(columnsBlob);
        var arr = JArray.Parse(json);
        return ApiResponseHelper.JsonNet(arr);
    }
    catch
    {
        return ApiResponseHelper.JsonNet(new JArray(new JObject { ["data"] = Convert.ToBase64String(columnsBlob) }));
    }
});

// ============= 文档同步接口（7 个端点） =============

// POST /api/Project/PushDocumentQuick
// Body: PushDocument Protobuf 字节流
// 返回 JObject { Result: "OK", Version: <int> }
app.MapPost("/api/Project/PushDocumentQuick", async (HttpContext ctx, DocumentSyncService svc, ProjectService projSvc, ArchiveRepository archiveRepo, ReviewRepository reviewRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    using var ms = new MemoryStream();
    await ctx.Request.Body.CopyToAsync(ms);
    var bytes = ms.ToArray();
    if (bytes.Length == 0) return ApiResponseHelper.Error("请求体为空");

    // 解析 Protobuf 提取 projectId 进行跨团队访问校验
    PushDocument pushDoc;
    try { pushDoc = PushDocument.Parser.ParseFrom(bytes); }
    catch { return ApiResponseHelper.Error("无效的 Protobuf 数据"); }
    var projectId = new Guid(pushDoc.ProjectId.ToByteArray());
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 归档锁定：已归档项目禁止推送（Archived）
    if (await archiveRepo.IsProjectArchivedAsync(projectId))
        return ApiResponseHelper.JsonNet(new { Result = "Archived", code = "Archived", error = "项目已归档，禁止修改", message = "项目已归档，禁止修改" }, 409);

    // 审批人只读会话（Task 5.5）：非项目成员的当前节点审批人（只读访问者）禁止推送
    if (await reviewRepo.GetPendingSubmissionForReviewerAsync(projectId, userId) != null
        && !await reviewRepo.IsProjectMemberAsync(projectId, userId))
        return ApiResponseHelper.JsonNet(new { Result = "ReadOnly", code = "ReadOnly", error = "审批人只读会话，禁止修改", message = "审批人只读会话，禁止修改" }, 409);

    var result = await svc.PushDocumentQuickAsync(bytes, userId);
    return ApiResponseHelper.JsonNet(result);
});

// GET /api/Project/PushDocument?taskId={taskId}&projectId={projectId}&documentId={documentId}&version={version}
// 返回 JObject { Result: "OK", Version: <int> }
app.MapGet("/api/Project/PushDocument", async (
    long taskId,
    Guid projectId,
    Guid documentId,
    int version,
    HttpContext ctx,
    DocumentSyncService svc,
    TaskService taskSvc,
    TaskRepository taskRepo,
    ProjectService projSvc,
    ArchiveRepository archiveRepo,
    ReviewRepository reviewRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    // 校验 taskId 归属当前用户，防止跨用户枚举 taskId 读取他人任务输入文件
    var task = await taskRepo.GetByIdAsync(taskId);
    if (task == null || task.UserId != userId)
        return ApiResponseHelper.Forbidden("无权访问该任务");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 归档锁定：已归档项目禁止推送（Archived）
    if (await archiveRepo.IsProjectArchivedAsync(projectId))
        return ApiResponseHelper.JsonNet(new { Result = "Archived", code = "Archived", error = "项目已归档，禁止修改", message = "项目已归档，禁止修改" }, 409);

    // 审批人只读会话（Task 5.5）：非项目成员的当前节点审批人（只读访问者）禁止推送
    if (await reviewRepo.GetPendingSubmissionForReviewerAsync(projectId, userId) != null
        && !await reviewRepo.IsProjectMemberAsync(projectId, userId))
        return ApiResponseHelper.JsonNet(new { Result = "ReadOnly", code = "ReadOnly", error = "审批人只读会话，禁止修改", message = "审批人只读会话，禁止修改" }, 409);

    var cachePath = taskSvc.GetTaskCachePath(taskId);
    if (!File.Exists(cachePath))
        return ApiResponseHelper.Error("任务输入文件不存在");

    using var cacheStream = taskSvc.OpenTaskInput(taskId);
    // 安全修复（跨租户越权写入）：DocumentSyncService 内部同样以 protobuf 内的 ProjectId
    // 作为实际写入目标（DocumentSyncService.cs 中 new Guid(pushDoc.ProjectId...)），
    // 必须与上面已通过访问校验的 projectId 一致。
    using var uploadedDocBody = new MemoryStream();
    await cacheStream.CopyToAsync(uploadedDocBody);
    uploadedDocBody.Position = 0;
    PushDocument uploadedPushDocument;
    try { uploadedPushDocument = PushDocument.Parser.ParseFrom(uploadedDocBody); }
    catch (InvalidProtocolBufferException ex)
    {
        app.Logger.LogWarning(ex, "PushDocument(GET) Protobuf 解析失败: TaskId={TaskId}", taskId);
        return ApiResponseHelper.Error("无效的 Protobuf 数据: " + ex.Message);
    }
    var bodyProjectId = new Guid(uploadedPushDocument.ProjectId.ToByteArray());
    if (bodyProjectId != projectId)
    {
        app.Logger.LogWarning("PushDocument(GET) 拒绝：请求体项目({BodyProjectId})与 URL 项目({UrlProjectId})不一致 UserId={UserId}",
            bodyProjectId, projectId, userId);
        return ApiResponseHelper.Forbidden("请求体中的项目与 URL 中的项目不一致");
    }
    uploadedDocBody.Position = 0;
    var result = await svc.PushDocumentAsync(taskId, projectId, documentId, version, userId, uploadedDocBody);
    return ApiResponseHelper.JsonNet(result);
});

// POST /api/Project/PullDocument
// Body: { projectId, documentId, version } JSON
// 返回 PullDocument Protobuf 字节流
app.MapPost("/api/Project/PullDocument", async (HttpContext ctx, DocumentSyncService svc, ProjectService projSvc) =>
{
    // V2-H-03 修复：补 ParseUserId 与项目成员校验，防止跨租户 IDOR 数据泄露。
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var documentIdStr = (data["documentId"] ?? data["DocumentId"])?.ToString();
    var clientVersion = data.Value<int?>("version") ?? data.Value<int?>("Version") ?? 0;

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(documentIdStr, out var documentId))
        return ApiResponseHelper.Error("无效的 documentId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var (bytes, isEmpty) = await svc.PullDocumentAsync(projectId, documentId, clientVersion);
    ctx.Response.ContentType = "application/x-protobuf";
    await ctx.Response.Body.WriteAsync(bytes);
    return Results.Empty;
});

// POST /api/Project/RevertDocument
// Body: { projectId, documentId, targetVersion } JSON
// 返回 JObject { taskId: <long>, url: <string> }
app.MapPost("/api/Project/RevertDocument", async (HttpContext ctx, DocumentSyncService svc, TaskService taskSvc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var documentIdStr = (data["documentId"] ?? data["DocumentId"])?.ToString();
    var targetVersion = data.Value<int?>("targetVersion") ?? data.Value<int?>("TargetVersion") ?? data.Value<int?>("revertVersion") ?? data.Value<int?>("RevertVersion") ?? 0;

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(documentIdStr, out var documentId))
        return ApiResponseHelper.Error("无效的 documentId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var result = await svc.RevertDocumentAsync(projectId, documentId, targetVersion, userId, taskSvc);
    var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(result);
    var resultObj = JObject.Parse(resultJson);
    var errToken = resultObj["error"];
    if (errToken != null)
        return ApiResponseHelper.Error(errToken.ToString());
    return ApiResponseHelper.JsonNet(result);
});

// POST /api/Project/GetDocumentRevertDiff
// Body: { projectId, documentId, targetVersion } JSON
// 返回 JObject { taskId: <long>, url: <string> }
app.MapPost("/api/Project/GetDocumentRevertDiff", async (HttpContext ctx, DocumentSyncService svc, TaskService taskSvc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var documentIdStr = (data["documentId"] ?? data["DocumentId"])?.ToString();
    var targetVersion = data.Value<int?>("targetVersion") ?? data.Value<int?>("TargetVersion") ?? data.Value<int?>("revertVersion") ?? data.Value<int?>("RevertVersion") ?? 0;

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(documentIdStr, out var documentId))
        return ApiResponseHelper.Error("无效的 documentId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var (bytes, taskId, url) = await svc.GetDocumentRevertDiffAsync(projectId, documentId, targetVersion, userId, taskSvc);
    return ApiResponseHelper.JsonNet(new { taskId, url });
});

// POST /api/Project/GetDocumentTimeline
// Body: { projectId, documentId } JSON
// 返回 JArray [{ id, version, changeType, createdAt, createdBy }]
app.MapPost("/api/Project/GetDocumentTimeline", async (HttpContext ctx, DocumentSyncService svc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    var documentIdStr = (data["documentId"] ?? data["DocumentId"])?.ToString();

    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    if (!Guid.TryParse(documentIdStr, out var documentId))
        return ApiResponseHelper.Error("无效的 documentId");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var timeline = await svc.GetDocumentTimelineAsync(projectId, documentId);
    return ApiResponseHelper.JsonNet(timeline);
});

// POST /api/Project/QueryDocumentVersions
// Body: { projectId, DocVersions: [{Id}] } JSON（批量查询每个文档的最新版本）
//   返回 JArray [{ Id, Version }]，与 QueryImageVersions/QueryPdfVersions/QueryTableVersions 契约一致。
//   客户端 Syncer.QueryVersion 用此结果判断哪些文档需要同步。
// 兼容旧契约：{ projectId, documentId } → 返回 JArray [{ version, changeType, createdAt }]（版本历史），
//   供 MCP/Scenario 工具（cloud_query_document_versions）使用。
app.MapPost("/api/Project/QueryDocumentVersions", async (HttpContext ctx, SqliteStorage db, DocumentSyncService svc, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);
    var projectIdStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.JsonNet(new JArray());

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 批量查询：{ ProjectId, DocVersions: [{Id}] } → [{ Id, Version }]
    var versionsArr = data["DocVersions"] ?? data["docVersions"];
    if (versionsArr is JArray arr)
    {
        // 文档 Id 为 long，VersionHistory.TargetId 以 LongToGuid(id) 的 Guid 字符串存储，
        // 因此查询前需将 long 转换为对应 Guid 字符串。
        var result = new JArray();
        // 批量查询：原为 foreach 逐条新建连接查询，文档很多时会打满客户端 30 秒超时
        var pending = new List<(long? LongId, string RawId, string TargetIdForQuery)>();
        foreach (var item in arr)
        {
            var idStr = (item["Id"] ?? item["id"])?.ToString() ?? "";
            if (string.IsNullOrEmpty(idStr)) continue;
            string targetIdForQuery = idStr;
            bool isLong = long.TryParse(idStr, out var longId);
            if (isLong)
            {
                var bytes = new byte[16];
                BitConverter.GetBytes(longId).CopyTo(bytes, 0);
                targetIdForQuery = new Guid(bytes).ToString();
            }
            pending.Add((isLong ? longId : (long?)null, idStr, targetIdForQuery));
        }
        var versionMap = await QueryLatestTargetVersionsAsync(db, projectId, "Document", pending.Select(p => p.TargetIdForQuery));
        foreach (var p in pending)
        {
            versionMap.TryGetValue(p.TargetIdForQuery, out var v);
            result.Add(new JObject { ["Id"] = p.LongId.HasValue ? (JToken)p.LongId.Value : p.RawId, ["Version"] = v });
        }
        return ApiResponseHelper.JsonNet(result);
    }

    // 兼容旧契约：{ projectId, documentId } → 版本历史 [{ version, changeType, createdAt }]
    var documentIdStr = (data["documentId"] ?? data["DocumentId"])?.ToString();
    if (Guid.TryParse(documentIdStr, out var documentId))
    {
        var versions = await svc.QueryDocumentVersionsAsync(projectId, documentId);
        return ApiResponseHelper.JsonNet(versions);
    }
    return ApiResponseHelper.JsonNet(new JArray());
});

// ============= 阶段 7 Task 7.1：图片/PDF 同步接口（6 个端点） =============
// 客户端调用流程（参考 Syncer.cs）：
// - PushImage/PushPdf：POST JObject { Action, Id, ProjectId, Version, FileId?, ZoomFactor?, ... }
//   若 Version=0 表示新建，客户端先 UploadFile 上传二进制，再 PushImage 提交元数据
//   返回 JObject { Result: "Success"|"OutOfDate", Version: <int> }
// - PullImage/PullPdf：POST JObject { Action, Id, ProjectId, Version }
//   返回 { Result: "NeedUpdate"|"Latest"|"NotExist", Version, + 元数据字段 }
// - QueryImageVersions/QueryPdfVersions：POST JObject { Action, ProjectId, ImageVersions|PdfVersions: [{Id}] }
//   返回 JArray [{ Id, Version }]
// 图片/PDF 元数据 + 版本号存储在 VersionHistory 表（TargetType="Image"|"Pdf"，TargetId=Id.ToString()，
// Snapshot=JSON 元数据字节流）

// POST /api/Project/PushImage
app.MapPost("/api/Project/PushImage", async (HttpContext ctx, SqliteStorage db, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var idStr = (data["Id"] ?? data["id"])?.ToString() ?? "";
    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    var clientVersion = data.Value<int?>("Version") ?? data.Value<int?>("version") ?? 0;

    if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId 或 Id");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    // 构建元数据快照（仅包含客户端提交的字段）
    var meta = new JObject();
    if (data["FileId"] != null) meta["FileId"] = data["FileId"];
    if (data["ZoomFactor"] != null) meta["ZoomFactor"] = data["ZoomFactor"];
    if (data["CenterX"] != null) meta["CenterX"] = data["CenterX"];
    if (data["CenterY"] != null) meta["CenterY"] = data["CenterY"];
    if (data["PageSetup"] != null) meta["PageSetup"] = data["PageSetup"];
    if (data["RotateFlip"] != null) meta["RotateFlip"] = data["RotateFlip"];
    var snapshotBytes = System.Text.Encoding.UTF8.GetBytes(meta.ToString(Formatting.None));

    // 乐观锁（修复）：原先读取了 clientVersion 但从未使用，多客户端并发修改同一图片元数据时
    // 后写者静默覆盖先写者，客户端却收到 Success。与 PushTable/PushDocument 对齐：
    // 客户端版本落后时拒绝并回传服务端最新版本，由客户端 Pull 合并后重试（clientVersion=0 兼容旧客户端）。
    var serverVersion = await QueryLatestTargetVersionAsync(db, projectId, idStr, "Image");
    if (clientVersion > 0 && clientVersion < serverVersion)
    {
        app.Logger.LogWarning("PushImage 拒绝：版本落后（客户端={ClientVersion} 服务端={ServerVersion}）Id={Id} UserId={UserId}",
            clientVersion, serverVersion, idStr, userId);
        return ApiResponseHelper.JsonNet(new { Result = "OutOfDate", Version = serverVersion });
    }

    // 写入版本快照
    // 安全审计修复（Med）：版本号在单条 INSERT..SELECT 内原子计算（不再先 SELECT MAX 再 INSERT），
    // 写入后读回实际版本号用于响应，消除并发推送时的版本竞态。
    await SaveImagePdfSnapshotAsync(db, projectId, idStr, "Image", "Push", snapshotBytes, userId);
    var newVersion = await QueryLatestTargetVersionAsync(db, projectId, idStr, "Image");

    app.Logger.LogInformation("PushImage: Id={Id} ProjectId={ProjectId} v{Version}->{NewVersion}", idStr, projectId, clientVersion, newVersion);
    return ApiResponseHelper.JsonNet(new { Result = "Success", Version = newVersion });
});

// POST /api/Project/PullImage
app.MapPost("/api/Project/PullImage", async (HttpContext ctx, SqliteStorage db, ProjectService projSvc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var idStr = (data["Id"] ?? data["id"])?.ToString() ?? "";
    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    var clientVersion = data.Value<int?>("Version") ?? data.Value<int?>("version") ?? 0;

    if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId 或 Id");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var snapshot = await QueryTargetSnapshotAsync(db, projectId, idStr, "Image");
    if (snapshot == null)
        return ApiResponseHelper.JsonNet(new { Result = "NotExist" });

    if (snapshot.Version <= clientVersion)
        return ApiResponseHelper.JsonNet(new { Result = "Latest", Version = snapshot.Version });

    // 解析元数据 + 合并 Version
    var metaJson = System.Text.Encoding.UTF8.GetString(snapshot.Snapshot ?? Array.Empty<byte>());
    var response = string.IsNullOrEmpty(metaJson) ? new JObject() : JObject.Parse(metaJson);
    response["Result"] = "NeedUpdate";
    response["Version"] = snapshot.Version;
    return ApiResponseHelper.JsonNet(response);
});

// POST /api/Project/QueryImageVersions
app.MapPost("/api/Project/QueryImageVersions", async (HttpContext ctx, SqliteStorage db, ProjectService projSvc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.JsonNet(new JArray());

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var versionsArr = data["ImageVersions"] ?? data["imageVersions"];
    var result = new JArray();
    if (versionsArr is JArray arr)
    {
        // 批量查询（原为逐条新建连接查询）
        var ids = new List<string>();
        foreach (var item in arr)
        {
            var idStr = (item["Id"] ?? item["id"])?.ToString() ?? "";
            if (!string.IsNullOrEmpty(idStr)) ids.Add(idStr);
        }
        var versionMap = await QueryLatestTargetVersionsAsync(db, projectId, "Image", ids);
        foreach (var idStr in ids)
        {
            versionMap.TryGetValue(idStr, out var v);
            result.Add(new JObject { ["Id"] = long.TryParse(idStr, out var lid) ? lid : idStr, ["Version"] = v });
        }
    }
    return ApiResponseHelper.JsonNet(result);
});

// POST /api/Project/PushPdf
app.MapPost("/api/Project/PushPdf", async (HttpContext ctx, SqliteStorage db, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var idStr = (data["Id"] ?? data["id"])?.ToString() ?? "";
    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    var clientVersion = data.Value<int?>("Version") ?? data.Value<int?>("version") ?? 0;

    if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId 或 Id");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var meta = new JObject();
    if (data["FileId"] != null) meta["FileId"] = data["FileId"];
    var snapshotBytes = System.Text.Encoding.UTF8.GetBytes(meta.ToString(Formatting.None));

    // 乐观锁（修复）：同 PushImage，原先 clientVersion 读取后从未使用，并发推送静默覆盖。
    var serverVersion = await QueryLatestTargetVersionAsync(db, projectId, idStr, "Pdf");
    if (clientVersion > 0 && clientVersion < serverVersion)
    {
        app.Logger.LogWarning("PushPdf 拒绝：版本落后（客户端={ClientVersion} 服务端={ServerVersion}）Id={Id} UserId={UserId}",
            clientVersion, serverVersion, idStr, userId);
        return ApiResponseHelper.JsonNet(new { Result = "OutOfDate", Version = serverVersion });
    }

    // 安全审计修复（Med）：版本号在单条 INSERT..SELECT 内原子计算（不再先 SELECT MAX 再 INSERT），
    // 写入后读回实际版本号用于响应，消除并发推送时的版本竞态。
    await SaveImagePdfSnapshotAsync(db, projectId, idStr, "Pdf", "Push", snapshotBytes, userId);
    var newVersion = await QueryLatestTargetVersionAsync(db, projectId, idStr, "Pdf");

    app.Logger.LogInformation("PushPdf: Id={Id} ProjectId={ProjectId} v{Version}->{NewVersion}", idStr, projectId, clientVersion, newVersion);
    return ApiResponseHelper.JsonNet(new { Result = "Success", Version = newVersion });
});

// POST /api/Project/PullPdf
app.MapPost("/api/Project/PullPdf", async (HttpContext ctx, SqliteStorage db, ProjectService projSvc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var idStr = (data["Id"] ?? data["id"])?.ToString() ?? "";
    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    var clientVersion = data.Value<int?>("Version") ?? data.Value<int?>("version") ?? 0;

    if (string.IsNullOrEmpty(idStr) || !Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.Error("无效的 ProjectId 或 Id");

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var snapshot = await QueryTargetSnapshotAsync(db, projectId, idStr, "Pdf");
    if (snapshot == null)
        return ApiResponseHelper.JsonNet(new { Result = "NotExist" });

    if (snapshot.Version <= clientVersion)
        return ApiResponseHelper.JsonNet(new { Result = "Latest", Version = snapshot.Version });

    var metaJson = System.Text.Encoding.UTF8.GetString(snapshot.Snapshot ?? Array.Empty<byte>());
    var response = string.IsNullOrEmpty(metaJson) ? new JObject() : JObject.Parse(metaJson);
    response["Result"] = "NeedUpdate";
    response["Version"] = snapshot.Version;
    return ApiResponseHelper.JsonNet(response);
});

// POST /api/Project/QueryPdfVersions
app.MapPost("/api/Project/QueryPdfVersions", async (HttpContext ctx, SqliteStorage db, ProjectService projSvc) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var projectIdStr = (data["ProjectId"] ?? data["projectId"])?.ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
        return ApiResponseHelper.JsonNet(new JArray());

    // 跨团队访问校验
    var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
    if (accessDenied != null) return accessDenied;

    var versionsArr = data["PdfVersions"] ?? data["pdfVersions"];
    var result = new JArray();
    if (versionsArr is JArray arr)
    {
        // 批量查询（原为逐条新建连接查询）
        var ids = new List<string>();
        foreach (var item in arr)
        {
            var idStr = (item["Id"] ?? item["id"])?.ToString() ?? "";
            if (!string.IsNullOrEmpty(idStr)) ids.Add(idStr);
        }
        var versionMap = await QueryLatestTargetVersionsAsync(db, projectId, "Pdf", ids);
        foreach (var idStr in ids)
        {
            versionMap.TryGetValue(idStr, out var v);
            result.Add(new JObject { ["Id"] = long.TryParse(idStr, out var lid) ? lid : idStr, ["Version"] = v });
        }
    }
    return ApiResponseHelper.JsonNet(result);
});

// ============= 阶段 7 Task 7.2：文件附件接口（2 个端点） =============
// 客户端契约（WebApiClient.cs）：
// - UploadFile(FileId, Stream)：POST 二进制流，Header 含 FileId，返回 void（200 OK）
// - DownloadFile(FileId)：GET，Header 含 FileId，返回 Stream + FileLength 响应头

// POST /api/Project/UploadFile
// Header: FileId；Body: 二进制流
// V2-H-11 修复：流式直接写盘，避免 MemoryStream 全量缓冲；超 100MB 返回 413。
// V2-H-12 修复：扩展名 + Content-Type 双校验；仅接受白名单扩展名。
app.MapPost("/api/Project/UploadFile", async (HttpContext ctx, FileStorageService fs, FileRepository repo, ProjectService projSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    // 安全审计修复（Med）：原实现 userId 可为 0（未登录）也能上传，且可覆盖他人文件
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var fileId = HeaderParser.ParseFileId(ctx);
    if (fileId == null) return ApiResponseHelper.Error("缺少 FileId Header");

    // ProjectId 从 QueryString 获取（UploadFile 仅按 FileId 索引，projectId 用于存储路径分组）
    var projectIdStr = ctx.Request.Query["projectId"].ToString();
    if (!Guid.TryParse(projectIdStr, out var projectId))
    {
        // 未指定 ProjectId 时使用 Empty GUID 作为兜底目录
        projectId = Guid.Empty;
    }

    // 安全审计修复（Med）：先查已有记录。若未指定 ProjectId 但记录已存在，
    // 沿用记录中的 ProjectId 做访问校验与存储定位，避免记录与落盘路径错位。
    var existing = await repo.GetByIdAsync(fileId.Value);
    if (projectId == Guid.Empty && existing != null)
        projectId = existing.ProjectId;

    // 跨团队访问校验：仅当指定了有效 projectId 时检查（Guid.Empty 表示无项目归属的附件）
    if (projectId != Guid.Empty)
    {
        var accessDenied = await CheckProjectAccessAsync(ctx, projectId, projSvc, app.Logger);
        if (accessDenied != null) return accessDenied;
    }

    // 安全审计修复（Med）：覆盖已有文件时校验归属——
    // ① FileId 已归属其他项目时禁止覆盖（防止把 A 项目 FileId 内容写到 B 项目路径下）；
    // ② 通过 CheckProjectAccessAsync 校验调用者对该文件所属项目的团队访问权（含上面 projectId 取自 existing.ProjectId 的场景）。
    if (existing != null && existing.ProjectId != projectId)
    {
        ctx.Response.StatusCode = 403;
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "FileId 已归属其他项目，禁止覆盖" }, 403);
    }

    // V2-H-12 修复：从 QueryString 读取 fileName 用于扩展名校验（与服务端存储路径解耦）。
    var fileName = ctx.Request.Query["fileName"].ToString() ?? "";
    var extension = Path.GetExtension(fileName);
    if (string.IsNullOrEmpty(extension) || !FileUploadWhitelist.AllowedFileExtensions.Contains(extension))
    {
        ctx.Response.StatusCode = 400;
        return ApiResponseHelper.JsonNet(new { error = "unsupported_file_type", message = "不支持的文件类型" }, 400);
    }

    // 校验 Content-Type 与扩展名一致（若客户端提供了 ContentType）
    var clientContentType = ctx.Request.ContentType ?? "";
    if (!string.IsNullOrEmpty(clientContentType))
    {
        var semicolonIndex = clientContentType.IndexOf(';');
        var mainType = (semicolonIndex >= 0 ? clientContentType.Substring(0, semicolonIndex) : clientContentType).Trim();
        if (FileUploadWhitelist.ExtensionToContentType.TryGetValue(extension, out var expected) &&
            !string.Equals(mainType, expected, StringComparison.OrdinalIgnoreCase))
        {
            ctx.Response.StatusCode = 400;
            return ApiResponseHelper.JsonNet(new { error = "content_type_mismatch", message = "Content-Type 与文件扩展名不一致" }, 400);
        }
    }

    // V2-H-11 修复：硬性大小限制 100MB。
    const long MaxUploadFileSize = 104_857_600L;
    if (ctx.Request.Headers.TryGetValue("Content-Length", out var clValues) &&
        long.TryParse(clValues.ToString(), out var cl) && cl > MaxUploadFileSize)
    {
        ctx.Response.StatusCode = 413;
        return ApiResponseHelper.JsonNet(new { error = "payload_too_large", message = $"文件大小超过 {MaxUploadFileSize / (1024 * 1024)}MB 上限" }, 413);
    }

    // 流式直接写盘：避免 MemoryStream 全量缓冲导致 OOM。
    // 拷贝过程中按累计字节硬性截断，超限返回 413 并删除已写入的部分文件。
    var storagePath = fs.GetStoragePath(projectId, fileId.Value);
    var dir = Path.GetDirectoryName(storagePath);
    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

    long bytesWritten = 0;
    var buffer = new byte[81920];
    try
    {
        await using (var fsOut = new FileStream(storagePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            int read;
            while ((read = await ctx.Request.Body.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                bytesWritten += read;
                if (bytesWritten > MaxUploadFileSize)
                {
                    await fsOut.DisposeAsync();
                    TryDeleteFile(storagePath);
                    ctx.Response.StatusCode = 413;
                    return ApiResponseHelper.JsonNet(new { error = "payload_too_large", message = $"文件大小超过 {MaxUploadFileSize / (1024 * 1024)}MB 上限" }, 413);
                }
                await fsOut.WriteAsync(buffer, 0, read);
            }
        }

        var fileSize = bytesWritten;
        // 若无记录则新建（使用 Header FileId，确保 DownloadFile 能按 FileId 查到记录）。
        // 已有记录（existing 在端点开头查询并做了归属校验）则仅覆盖磁盘内容。
        if (existing == null)
        {
            // 服务端固定 contentType 为 application/octet-stream，避免客户端伪造导致存储型 XSS（V2-H-12 修复）
            await repo.CreateAsync(fileId.Value, projectId, fileName, fileSize,
                "application/octet-stream", storagePath, userId);
        }
        app.Logger.LogInformation("UploadFile: FileId={FileId} Size={Size}", fileId, fileSize);
        return ApiResponseHelper.Ok();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "UploadFile 写盘失败: FileId={FileId}", fileId);
        TryDeleteFile(storagePath);
        throw;
    }
});

// GET /api/Project/DownloadFile
// Header: FileId；返回二进制流 + FileLength 响应头
// 安全审计修复（High）：原实现任意已登录用户可下载任意 FileId。修复：传入 userId 在服务层校验文件所属项目的成员归属。
// V2-H-12 修复：响应固定 Content-Type: application/octet-stream + X-Content-Type-Options: nosniff，避免存储型 XSS。
app.MapGet("/api/Project/DownloadFile", async (HttpContext ctx, FileStorageService fs) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();

    var fileId = HeaderParser.ParseFileId(ctx);
    if (fileId == null) return ApiResponseHelper.Error("缺少 FileId Header");

    ProjectFileDto file;
    Stream stream;
    try
    {
        (file, stream) = await fs.DownloadFileAsync(fileId.Value, userId);
    }
    catch (UnauthorizedAccessException ex)
    {
        return ApiResponseHelper.Unauthorized(ex.Message);
    }
    catch (FileNotFoundException)
    {
        return ApiResponseHelper.NotFound("文件不存在");
    }

    // 安全审计修复（Med）：原实现仅在成功路径释放 stream，CopyToAsync 抛异常（如客户端中断）
    // 时句柄泄漏；且响应已开始后 catch(FileNotFoundException) 再写 JSON 404 会破坏响应。
    // 改为 try/finally 确保释放；文件不存在已在上方 DownloadFileAsync 阶段统一处理。
    try
    {
        // 固定为 application/octet-stream，避免客户端存储的 Content-Type 被浏览器解析为 HTML 触发 XSS
        ctx.Response.ContentType = "application/octet-stream";
        ctx.Response.Headers.ContentDisposition = $"attachment; filename=\"{file.FileName ?? file.Id.ToString("D")}\"";
        ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Response.Headers["FileLength"] = file.FileSize.ToString();
        await stream.CopyToAsync(ctx.Response.Body);
        return Results.Empty;
    }
    finally
    {
        await stream.DisposeAsync();
    }
});

// ============= 阶段 7 Task 7.3：数据字典接口（3 个端点） =============
// 客户端契约（WebApiClient.cs）：GET ?version={version}，返回 JObject
// 客户端解析：ret["update"] == "0" 或 null 表示无更新；ret["version"] 为新版本号；
// 其余字段为字典数据（如 ct_tablenametoobject、ct_tablenametoaccount 等）。
// 服务端存储：DataDictionary.Data 存储字典数据 JSON（不含 update/version 包装）。
// 版本号比较：客户端 version >= 服务端最新版本时返回 { update: "0" }，否则返回完整数据。

// GET /api/DataSource/TableCollectDic?version={version}
app.MapGet("/api/DataSource/TableCollectDic", async (int version, DictionaryRepository repo) =>
    await BuildDictionaryResponseAsync(repo, "TableCollect", version));

// GET /api/DataSource/CellCollectDic?version={version}
app.MapGet("/api/DataSource/CellCollectDic", async (int version, DictionaryRepository repo) =>
    await BuildDictionaryResponseAsync(repo, "CellCollect", version));

// GET /api/DataSource/LedgerValidateDic?version={version}
app.MapGet("/api/DataSource/LedgerValidateDic", async (int version, DictionaryRepository repo) =>
    await BuildDictionaryResponseAsync(repo, "LedgerValidate", version));

// ============= 阶段 4 Task 14：License 管理端点（5 个端点） =============
// LicenseMiddleware 已将 /api/License/* 加入白名单（不拦截），故这些端点不会被 License 校验阻断。
// 管理员权限校验通过 UserRepository.GetByIdAsync 查询 IsTeamAdmin/IsSystemAdmin 字段实现。

// POST /api/License/Create
// Body: { ownerType, ownerId, planType, seats, maxProjects, maxTemplates, durationYears }
// 权限：仅 TeamAdmin/IsSystemAdmin 可调用
app.MapPost("/api/License/Create", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0)
        return ApiResponseHelper.JsonNet(new { error = "unauthorized", message = "未登录" }, 401);

    var caller = await userRepo.GetByIdAsync(userId);
    if (caller == null || !(caller.IsTeamAdmin || caller.IsSystemAdmin))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可创建 License" }, 403);

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var ownerType = data.Value<string>("ownerType") ?? data.Value<string>("OwnerType") ?? "Team";
    var ownerId = data.Value<string>("ownerId") ?? data.Value<string>("OwnerId") ?? "";
    var planType = data.Value<int?>("planType") ?? data.Value<int?>("PlanType") ?? 0;
    var seats = data.Value<int?>("seats") ?? data.Value<int?>("Seats") ?? 5;
    var maxProjects = data.Value<int?>("maxProjects") ?? data.Value<int?>("MaxProjects") ?? 10;
    var maxTemplates = data.Value<int?>("maxTemplates") ?? data.Value<int?>("MaxTemplates") ?? 5;
    var durationYears = data.Value<int?>("durationYears") ?? data.Value<int?>("DurationYears") ?? 1;

    if (string.IsNullOrEmpty(ownerId) || seats <= 0 || durationYears <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    var start = DateTime.Now;
    var end = start.AddYears(durationYears);
    var license = await licenseRepo.CreateLicenseAsync(ownerType, ownerId, planType,
        seats, maxProjects, maxTemplates, start, end);

    // V2-M-34 修复：移除日志中的 LicenseKey 明文，仅记录 Id/OwnerType/OwnerId/EndDate
    app.Logger.LogInformation("License 创建: Id={Id} Owner={OwnerType}:{OwnerId} EndDate={EndDate}",
        license.Id, ownerType, ownerId, end);

    return ApiResponseHelper.JsonNet(new { licenseKey = license.LicenseKey, licenseId = license.Id });
});

// POST /api/License/Activate
// Body: { licenseKey, machineCode }
// 逻辑：校验 LicenseKey → 校验 Seats 未满 → 写入 Activations → 更新 Teams.LicenseDate = license.EndDate
app.MapPost("/api/License/Activate", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var licenseKey = data.Value<string>("licenseKey") ?? data.Value<string>("LicenseKey") ?? "";
    var machineCode = data.Value<string>("machineCode") ?? data.Value<string>("MachineCode") ?? "";

    if (string.IsNullOrEmpty(licenseKey) || string.IsNullOrEmpty(machineCode))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    var license = await licenseRepo.GetLicenseByKeyAsync(licenseKey);
    if (license == null)
        return ApiResponseHelper.JsonNet(new { error = "license_not_found", message = "LicenseKey 无效" }, 404);

    // 安全审计修复（Med）：原实现知道 LicenseKey 即可激活任意团队的 License。
    // 仅系统管理员或 License 归属者（团队成员/用户本人）可激活。
    var callerId = HeaderParser.ParseUserId(ctx);
    if (callerId == 0)
        return ApiResponseHelper.JsonNet(new { error = "unauthorized", message = "未登录" }, 401);
    if (!await IsCurrentUserSystemAdminAsync(ctx, db) &&
        !await IsLicenseOwnedByCallerAsync(db, callerId, license.OwnerType, license.OwnerId))
    {
        app.Logger.LogWarning("License 激活被拒（无归属权）: LicenseId={Id} CallerId={CallerId}", license.Id, callerId);
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "无权操作该 License" }, 403);
    }

    if (!await licenseRepo.CheckSeatsAvailableAsync(license.Id))
        return ApiResponseHelper.JsonNet(new { error = "seats_exceeded", message = "座位数已达上限" }, 429);

    try
    {
        await licenseRepo.ActivateMachineAsync(license.Id, machineCode);
    }
    catch (InvalidOperationException)
    {
        // 并发场景下再次确认座位已满
        return ApiResponseHelper.JsonNet(new { error = "seats_exceeded", message = "座位数已达上限" }, 429);
    }

    // 更新 Teams.LicenseDate = license.EndDate（若该 License 关联到 Team）
    if (license.OwnerType == "Team" && !string.IsNullOrEmpty(license.OwnerId))
    {
        await UpdateTeamLicenseDateAsync(db, license.OwnerId, license.EndDate);
    }

    app.Logger.LogInformation("License 激活: LicenseId={Id} MachineCode={MachineCode} EndDate={EndDate}",
        license.Id, machineCode, license.EndDate);

    return ApiResponseHelper.JsonNet(new { activated = true, endDate = license.EndDate.ToString("yyyy-MM-dd") });
});

// GET /api/License/Status
// 返回当前用户 License 状态（PlanType/Seats/EndDate/Activations/daysRemaining/isExpired/inGracePeriod）
app.MapGet("/api/License/Status", async (
    HttpContext ctx,
    LicenseRepository licenseRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0)
        return ApiResponseHelper.JsonNet(new { error = "unauthorized", message = "未登录" }, 401);

    var status = await licenseRepo.GetLicenseStatusAsync((int)userId);
    if (status == null)
        return ApiResponseHelper.JsonNet(new { error = "no_license", message = "未找到有效 License" }, 404);

    // 计算剩余天数/过期/宽限期（与 LicenseMiddleware 保持 7 天宽限期一致）
    var now = DateTime.Now;
    var daysRemaining = (int)Math.Ceiling((status.EndDate - now).TotalDays);
    var isExpired = status.EndDate < now;
    var inGracePeriod = isExpired && status.EndDate >= now - TimeSpan.FromDays(7);
    status.DaysRemaining = daysRemaining;
    status.IsExpired = isExpired;
    status.InGracePeriod = inGracePeriod;

    return ApiResponseHelper.JsonNet(status);
});

// POST /api/License/Renew
// Body: { licenseId, years }
// 权限：仅 TeamAdmin/IsSystemAdmin 可调用
app.MapPost("/api/License/Renew", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0)
        return ApiResponseHelper.JsonNet(new { error = "unauthorized", message = "未登录" }, 401);

    var caller = await userRepo.GetByIdAsync(userId);
    if (caller == null || !(caller.IsTeamAdmin || caller.IsSystemAdmin))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可续费 License" }, 403);

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var licenseIdStr = (data["licenseId"] ?? data["LicenseId"])?.ToString();
    var yearsVal = data.Value<int?>("years") ?? data.Value<int?>("Years") ?? 0;

    if (!int.TryParse(licenseIdStr, out var licenseId) || licenseId <= 0 || yearsVal <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    try
    {
        var renewed = await licenseRepo.RenewLicenseAsync(licenseId, yearsVal);
        app.Logger.LogInformation("License 续费: Id={Id} Years={Years} NewEnd={NewEnd}",
            licenseId, yearsVal, renewed.EndDate);
        return ApiResponseHelper.JsonNet(new { renewed = true, newEndDate = renewed.EndDate.ToString("yyyy-MM-dd") });
    }
    catch (InvalidOperationException)
    {
        return ApiResponseHelper.JsonNet(new { error = "license_not_found", message = "License 不存在" }, 404);
    }
});

// POST /api/License/Deactivate
// Body: { licenseId, machineCode }
app.MapPost("/api/License/Deactivate", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var licenseIdStr = (data["licenseId"] ?? data["LicenseId"])?.ToString();
    var machineCode = data.Value<string>("machineCode") ?? data.Value<string>("MachineCode") ?? "";

    if (!int.TryParse(licenseIdStr, out var licenseId) || licenseId <= 0 || string.IsNullOrEmpty(machineCode))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    // 安全审计修复（Med）：原实现无任何鉴权，知道 licenseId 即可解绑他人机器。
    // 查询 License 归属，仅系统管理员或 License 归属者（团队成员/用户本人）可解绑。
    string? ownerType = null;
    string? ownerId = null;
    using (var conn = db.CreateConnection())
    {
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT OwnerType, OwnerId FROM Licenses WHERE Id = @lid";
        cmd.Parameters.AddWithValue("@lid", licenseId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            ownerType = reader.IsDBNull(0) ? null : reader.GetString(0);
            ownerId = reader.IsDBNull(1) ? null : reader.GetString(1);
        }
    }
    if (ownerId == null)
        return ApiResponseHelper.JsonNet(new { error = "license_not_found", message = "License 不存在" }, 404);

    var callerId = HeaderParser.ParseUserId(ctx);
    if (callerId == 0)
        return ApiResponseHelper.JsonNet(new { error = "unauthorized", message = "未登录" }, 401);
    if (!await IsCurrentUserSystemAdminAsync(ctx, db) &&
        !await IsLicenseOwnedByCallerAsync(db, callerId, ownerType, ownerId))
    {
        app.Logger.LogWarning("License 解绑被拒（无归属权）: LicenseId={Id} CallerId={CallerId}", licenseId, callerId);
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "无权操作该 License" }, 403);
    }

    var success = await licenseRepo.DeactivateMachineAsync(licenseId, machineCode);
    if (!success)
        return ApiResponseHelper.JsonNet(new { error = "activation_not_found", message = "未找到激活记录" }, 404);

    app.Logger.LogInformation("License 解绑机器: LicenseId={Id} MachineCode={MachineCode}", licenseId, machineCode);
    return ApiResponseHelper.JsonNet(new { deactivated = true });
});

// ============= 管理面板 Admin 端点 =============
// 所有 /api/Admin/* 端点均需校验当前请求用户为 IsSystemAdmin。
// LicenseMiddleware / QuotaMiddleware 已对 /api/Admin/* 放行（见 Task 1）。

// GET /api/Admin/Stats → 返回 AdminStatsDto
app.MapGet("/api/Admin/Stats", async (HttpContext ctx, SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    var now = DateTime.Now;
    cmd.CommandText = @"
        SELECT
          (SELECT COUNT(1) FROM Users) AS TotalUsers,
          (SELECT COUNT(1) FROM Users WHERE IsActive = 1) AS ActiveUsers,
          (SELECT COUNT(1) FROM Teams WHERE IsDeleted = 0 OR IsDeleted IS NULL) AS TotalTeams,
          (SELECT COUNT(1) FROM Projects WHERE IsDeleted = 0 OR IsDeleted IS NULL) AS TotalProjects,
          (SELECT COUNT(1) FROM Licenses WHERE IsActive = 1) AS TotalLicenses,
          (SELECT COUNT(1) FROM Licenses WHERE IsActive = 1 AND EndDate >= @now AND EndDate < @threshold) AS ExpiringLicenses,
          (SELECT COUNT(1) FROM TeamInvitations WHERE Status = 0) AS PendingInvitations,
          (SELECT COUNT(1) FROM Users WHERE CreateTime >= @weekAgo) AS NewUsersLast7Days";
    cmd.Parameters.AddWithValue("@now", now.ToString("yyyy-MM-dd HH:mm:ss"));
    cmd.Parameters.AddWithValue("@threshold", now.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss"));
    cmd.Parameters.AddWithValue("@weekAgo", now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"));
    using var reader = await cmd.ExecuteReaderAsync();
    var stats = new AdminStatsDto();
    if (await reader.ReadAsync())
    {
        stats.TotalUsers = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
        stats.ActiveUsers = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
        stats.TotalTeams = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));
        stats.TotalProjects = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
        stats.TotalLicenses = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4));
        stats.ExpiringLicenses = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5));
        stats.PendingInvitations = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6));
        stats.NewUsersLast7Days = reader.IsDBNull(7) ? 0 : Convert.ToInt32(reader.GetValue(7));
    }
    return ApiResponseHelper.JsonNet(stats);
});

// ============= Admin: 用户管理 =============

// GET /api/Admin/Users → 分页搜索用户
app.MapGet("/api/Admin/Users", async (
    HttpContext ctx,
    UserRepository userRepo,
    SqliteStorage db,
    int page = 1,
    int pageSize = 20,
    string? keyword = null,
    Guid? teamId = null,
    bool? isActive = null) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var (users, total) = await userRepo.SearchUsersAsync(keyword, teamId, isActive, page, pageSize);
    return ApiResponseHelper.JsonNet(new { users, total, page, pageSize });
});

// POST /api/Admin/UpdateUser → 管理员更新用户字段（支持修改用户名）
app.MapPost("/api/Admin/UpdateUser", async (
    HttpContext ctx,
    UserRepository userRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var userId = data.Value<long?>("userId") ?? data.Value<long?>("UserId") ?? 0;
    if (userId <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "userId 无效" }, 400);

    var userName = data.Value<string?>("userName") ?? data.Value<string?>("UserName");
    var name = data.Value<string?>("name") ?? data.Value<string?>("Name");
    var phone = data.Value<string?>("phone") ?? data.Value<string?>("Phone");
    var email = data.Value<string?>("email") ?? data.Value<string?>("Email");
    var role = data.Value<int?>("role") ?? data.Value<int?>("Role") ?? 0;
    var isTeamAdmin = data.Value<bool?>("isTeamAdmin") ?? data.Value<bool?>("IsTeamAdmin") ?? false;
    var isDataAdmin = data.Value<bool?>("isDataAdmin") ?? data.Value<bool?>("IsDataAdmin") ?? false;
    var isActive = data.Value<bool?>("isActive") ?? data.Value<bool?>("IsActive") ?? true;
    var teamIdStr = data.Value<string?>("teamId") ?? data.Value<string?>("TeamId");
    Guid? teamId = !string.IsNullOrEmpty(teamIdStr) && Guid.TryParse(teamIdStr, out var tid) ? tid : null;

    var ok = await userRepo.AdminUpdateUserAsync(userId, userName, name, phone, email, role, isTeamAdmin, isDataAdmin, isActive, teamId);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "user_not_found", message = "用户不存在或用户名已被占用" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// POST /api/Admin/ResetUserPassword → 管理员重置用户密码
app.MapPost("/api/Admin/ResetUserPassword", async (
    HttpContext ctx,
    UserRepository userRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var userId = data.Value<long?>("userId") ?? data.Value<long?>("UserId") ?? 0;
    var newPassword = data.Value<string?>("newPassword") ?? data.Value<string?>("NewPassword") ?? "";
    if (userId <= 0 || string.IsNullOrEmpty(newPassword))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    var ok = await userRepo.ResetPasswordByAdminAsync(userId, newPassword);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "user_not_found", message = "用户不存在" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// POST /api/Admin/ToggleUserActive → 切换用户启用/禁用
app.MapPost("/api/Admin/ToggleUserActive", async (
    HttpContext ctx,
    UserRepository userRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var userId = data.Value<long?>("userId") ?? data.Value<long?>("UserId") ?? 0;
    if (userId <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "userId 无效" }, 400);

    // 禁止管理员停用自己
    var currentUserId = HeaderParser.ParseUserId(ctx);
    if (userId == currentUserId)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "不能停用自己的账号" }, 400);

    var newActive = await userRepo.ToggleUserActiveAsync(userId);
    return ApiResponseHelper.JsonNet(new { isActive = newActive });
});

// POST /api/Admin/DeleteUser → 硬删除用户
app.MapPost("/api/Admin/DeleteUser", async (
    HttpContext ctx,
    UserRepository userRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var userId = data.Value<long?>("userId") ?? data.Value<long?>("UserId") ?? 0;
    if (userId <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "userId 无效" }, 400);

    // 禁止管理员删除自己
    var currentUserId = HeaderParser.ParseUserId(ctx);
    if (userId == currentUserId)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "不能删除自己的账号" }, 400);

    var ok = await userRepo.DeleteUserAsync(userId);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "user_not_found", message = "用户不存在" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// POST /api/Admin/CreateUser → 管理员创建新用户
app.MapPost("/api/Admin/CreateUser", async (
    HttpContext ctx,
    UserRepository userRepo,
    TeamRepository teamRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var bodyText = await ReadBodyAsync(ctx);
    var data = JObject.Parse(bodyText);

    var userName = data.Value<string>("userName") ?? data.Value<string>("UserName") ?? "";
    var name = data.Value<string>("name") ?? data.Value<string>("Name") ?? "";
    var password = data.Value<string>("password") ?? data.Value<string>("Password") ?? "";
    var teamId = data.Value<string>("teamId") ?? data.Value<string>("TeamId") ?? "";
    var isSystemAdmin = data.Value<bool?>("isSystemAdmin") ?? data.Value<bool?>("IsSystemAdmin") ?? false;
    var isTeamAdmin = data.Value<bool?>("isTeamAdmin") ?? data.Value<bool?>("IsTeamAdmin") ?? false;

    if (string.IsNullOrEmpty(userName))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "用户名不能为空" }, 400);
    if (string.IsNullOrEmpty(password))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "密码不能为空" }, 400);

    // 校验 UserName 不重复（Users.UserName 有 UNIQUE 约束，提前检查给出友好错误）
    var existing = await userRepo.GetByUserNameAsync(userName);
    if (existing != null)
        return ApiResponseHelper.JsonNet(new { error = "duplicate_user", message = "用户名已存在" }, 409);

    var parsedTeamId = Guid.TryParse(teamId, out var tid) ? tid : Guid.Empty;

    var user = new UserDto
    {
        UserName = userName,
        Name = name,
        TeamId = parsedTeamId,
        IsSystemAdmin = isSystemAdmin,
        IsTeamAdmin = isTeamAdmin,
        IsActive = true,
        Role = 0
    };

    var salt = PasswordHasher.GenerateSalt();
    user.Password = PasswordHasher.HashPassword(password, salt);
    user.PasswordSalt = salt;
    user.PasswordHashAlgorithm = 1;

    var id = await userRepo.CreateAsync(user);

    // 同步建立 UserTeams 关联，确保新用户出现在团队成员列表中
    if (parsedTeamId != Guid.Empty)
    {
        await teamRepo.AddUserToTeamAsync(id, parsedTeamId);
    }

    app.Logger.LogInformation("管理员创建用户成功: userName={UserName} id={Id} teamId={TeamId}", user.UserName, id, parsedTeamId);
    return ApiResponseHelper.JsonNet(new { success = true, userId = id });
});

// ============= Admin: License 管理 =============

// GET /api/Admin/Licenses → 分页搜索 License
app.MapGet("/api/Admin/Licenses", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db,
    int page = 1,
    int pageSize = 20,
    string? keyword = null) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var (licenses, total) = await licenseRepo.SearchLicensesAsync(keyword, page, pageSize);
    return ApiResponseHelper.JsonNet(new { licenses, total });
});

// POST /api/Admin/CreateLicense → 为团队创建 License
app.MapPost("/api/Admin/CreateLicense", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var teamIdStr = data.Value<string?>("teamId") ?? data.Value<string?>("TeamId") ?? "";
    var planType = data.Value<int?>("planType") ?? data.Value<int?>("PlanType") ?? 0;
    var seats = data.Value<int?>("seats") ?? data.Value<int?>("Seats") ?? 5;
    var maxProjects = data.Value<int?>("maxProjects") ?? data.Value<int?>("MaxProjects") ?? 10;
    var maxTemplates = data.Value<int?>("maxTemplates") ?? data.Value<int?>("MaxTemplates") ?? 5;
    var endDateStr = data.Value<string?>("endDate") ?? data.Value<string?>("EndDate") ?? "";

    if (!Guid.TryParse(teamIdStr, out var teamId) || string.IsNullOrEmpty(endDateStr))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);
    if (!DateTime.TryParse(endDateStr, out var endDate))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "endDate 格式无效" }, 400);

    var license = await licenseRepo.CreateLicenseAsync(
        "Team", teamId.ToString(), planType, seats, maxProjects, maxTemplates, DateTime.Now, endDate);

    // V2-M-34 修复：移除日志中的 LicenseKey 明文，仅记录 Id/TeamId/EndDate
    app.Logger.LogInformation("管理员创建 License: Id={Id} TeamId={TeamId} EndDate={EndDate}",
        license.Id, teamId, endDate);

    return ApiResponseHelper.JsonNet(new { licenseKey = license.LicenseKey, licenseId = license.Id });
});

// POST /api/Admin/RenewLicense → 续费 License（按天）
app.MapPost("/api/Admin/RenewLicense", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var licenseId = data.Value<int?>("licenseId") ?? data.Value<int?>("LicenseId") ?? 0;
    var extendDays = data.Value<int?>("extendDays") ?? data.Value<int?>("ExtendDays") ?? 0;

    if (licenseId <= 0 || extendDays <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    try
    {
        var renewed = await licenseRepo.RenewLicenseByDaysAsync(licenseId, extendDays);
        app.Logger.LogInformation("管理员续费 License: Id={Id} Days={Days} NewEnd={NewEnd}",
            licenseId, extendDays, renewed.EndDate);
        return ApiResponseHelper.JsonNet(new { renewed = true, newEndDate = renewed.EndDate.ToString("yyyy-MM-dd") });
    }
    catch (InvalidOperationException)
    {
        return ApiResponseHelper.JsonNet(new { error = "license_not_found", message = "License 不存在" }, 404);
    }
});

// POST /api/Admin/UpdateLicenseQuota → 更新 License 配额
app.MapPost("/api/Admin/UpdateLicenseQuota", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var licenseId = data.Value<long?>("licenseId") ?? data.Value<long?>("LicenseId") ?? 0;
    var seats = data.Value<int?>("seats") ?? data.Value<int?>("Seats") ?? 0;
    var maxProjects = data.Value<int?>("maxProjects") ?? data.Value<int?>("MaxProjects") ?? 0;
    var maxTemplates = data.Value<int?>("maxTemplates") ?? data.Value<int?>("MaxTemplates") ?? 0;

    if (licenseId <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "licenseId 无效" }, 400);

    var ok = await licenseRepo.UpdateLicenseQuotaAsync(licenseId, seats, maxProjects, maxTemplates);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "license_not_found", message = "License 不存在" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// POST /api/Admin/DeactivateMachine → 解绑机器
app.MapPost("/api/Admin/DeactivateMachine", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var licenseId = data.Value<int?>("licenseId") ?? data.Value<int?>("LicenseId") ?? 0;
    var machineCode = data.Value<string?>("machineCode") ?? data.Value<string?>("MachineCode") ?? "";

    if (licenseId <= 0 || string.IsNullOrEmpty(machineCode))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    var ok = await licenseRepo.DeactivateMachineAsync(licenseId, machineCode);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "activation_not_found", message = "未找到激活记录" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// GET /api/Admin/Licenses/Expiring → 即将到期的 License 列表
app.MapGet("/api/Admin/Licenses/Expiring", async (
    HttpContext ctx,
    LicenseRepository licenseRepo,
    SqliteStorage db,
    int days = 30) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var list = await licenseRepo.GetExpiringLicensesAsync(days);
    return ApiResponseHelper.JsonNet(new { licenses = list });
});

// ============= Admin: 激活码管理 =============
// 阶段 3 Task 6：激活码管理端点（5 个），供管理面板批量导入/生成/查询/禁用/删除激活码。
// 所有 /api/Admin/* 端点均需 IsSystemAdmin 权限校验（LicenseMiddleware / QuotaMiddleware 已放行）。

// POST /api/Admin/ActivationCodes/Import
// Body: { "codes": "ABCD-1234-WXYZ-9F8G\nEFGH-5678-IJKL-0MNP\n..." }（多行字符串，每行一个激活码）
// 输出: { "Success": <插入数量>, "Total": <输入数量>, "BatchId": "<batchId>" }
app.MapPost("/api/Admin/ActivationCodes/Import", async (
    HttpContext ctx,
    ActivationCodeRepository activationCodeRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可访问" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var codesText = data.Value<string>("codes") ?? data.Value<string>("Codes") ?? "";

    // 按 \n 分割，trim 每行，过滤空行
    var lines = codesText
        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(l => l.Trim())
        .Where(l => l.Length > 0)
        .ToList();

    if (lines.Count == 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "未提供任何激活码" }, 400);

    var batchId = $"import-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    // BatchInsertAsync 内部已做去重和排除已存在，返回成功插入数量
    var inserted = await activationCodeRepo.BatchInsertAsync(lines, batchId);

    app.Logger.LogInformation("管理员批量导入激活码: 输入={Input} 成功={Inserted} BatchId={BatchId}",
        lines.Count, inserted, batchId);

    return ApiResponseHelper.JsonNet(new { Success = inserted, Total = lines.Count, BatchId = batchId });
});

// POST /api/Admin/ActivationCodes/Generate
// Body: { "count": 100 }
// 输出: { "Codes": [...], "BatchId": "<batchId>", "Count": <数量> }
app.MapPost("/api/Admin/ActivationCodes/Generate", async (
    HttpContext ctx,
    ActivationCodeRepository activationCodeRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可访问" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var count = data.Value<int?>("count") ?? data.Value<int?>("Count") ?? 0;

    // count 范围校验：1-1000，超出返回 400
    if (count < 1 || count > 1000)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "count 范围 1-1000" }, 400);

    var codes = await activationCodeRepo.GenerateCodesAsync(count);
    var batchId = $"gen-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

    app.Logger.LogInformation("管理员批量生成激活码: Count={Count} BatchId={BatchId}", codes.Count, batchId);

    return ApiResponseHelper.JsonNet(new { Codes = codes, BatchId = batchId, Count = codes.Count });
});

// GET /api/Admin/ActivationCodes/List
// Query: ?status=0|1|2（可选，不传则返回全部）
// 输出: { "Items": [...ActivationCodeRecord], "Total": <数量> }
app.MapGet("/api/Admin/ActivationCodes/List", async (
    HttpContext ctx,
    ActivationCodeRepository activationCodeRepo,
    SqliteStorage db,
    string? status = null) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可访问" }, 403);

    // status 为 null 或空字符串时传 null，表示返回全部
    int? statusFilter = null;
    if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out var s))
        statusFilter = s;

    var items = await activationCodeRepo.GetAllAsync(statusFilter);
    return ApiResponseHelper.JsonNet(new { Items = items, Total = items.Count });
});

// POST /api/Admin/ActivationCodes/Disable
// Body: { "id": 123 }
// 输出: 成功 { "Success": true }；失败（id 不存在或已使用）404 { "error":"not_found","message":"激活码不存在或已使用" }
app.MapPost("/api/Admin/ActivationCodes/Disable", async (
    HttpContext ctx,
    ActivationCodeRepository activationCodeRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可访问" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var id = data.Value<int?>("id") ?? data.Value<int?>("Id") ?? 0;

    if (id <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "id 无效" }, 400);

    var ok = await activationCodeRepo.DisableAsync(id);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "not_found", message = "激活码不存在或已使用" }, 404);
    return ApiResponseHelper.JsonNet(new { Success = true });
});

// POST /api/Admin/ActivationCodes/Delete
// Body: { "id": 123 }
// 输出: 成功 { "Success": true }；失败（id 不存在）404 { "error":"not_found","message":"激活码不存在" }
app.MapPost("/api/Admin/ActivationCodes/Delete", async (
    HttpContext ctx,
    ActivationCodeRepository activationCodeRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "仅管理员可访问" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var id = data.Value<int?>("id") ?? data.Value<int?>("Id") ?? 0;

    if (id <= 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "id 无效" }, 400);

    var ok = await activationCodeRepo.DeleteAsync(id);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "not_found", message = "激活码不存在" }, 404);
    return ApiResponseHelper.JsonNet(new { Success = true });
});

// ============= Admin: 模板管理 =============

// GET /api/Admin/AllTemplates → 列出所有模板（含系统模板和团队模板）
app.MapGet("/api/Admin/AllTemplates", async (
    HttpContext ctx,
    ProjectRepository projectRepo,
    UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var caller = await userRepo.GetByIdAsync(userId);
    if (caller == null || !caller.IsSystemAdmin)
        return ApiResponseHelper.Error("仅系统管理员可访问", 403);

    var keyword = ctx.Request.Query["keyword"].ToString();
    var templates = await projectRepo.GetAllTemplatesAdminAsync(keyword);
    return ApiResponseHelper.JsonNet(new { templates });
});

// POST /api/Admin/PushTemplateToAllTeams → 将模板推送到所有团队
app.MapPost("/api/Admin/PushTemplateToAllTeams", async (
    HttpContext ctx,
    ProjectService svc,
    UserRepository userRepo,
    ProjectRepository projectRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var caller = await userRepo.GetByIdAsync(userId);
    if (caller == null || !caller.IsSystemAdmin)
        return ApiResponseHelper.Error("仅系统管理员可访问", 403);

    var body = await ReadBodyAsync(ctx);
    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
    if (data == null || !data.TryGetValue("projectId", out var pidStr) || !Guid.TryParse(pidStr, out var sourceProjectId))
        return ApiResponseHelper.Error("请求体无效，需要 projectId");

    // 校验模板存在
    var template = await projectRepo.GetByIdAsync(sourceProjectId);
    if (template == null || template.Type != 1) // ProjectType.Template
        return ApiResponseHelper.Error("模板不存在");

    var results = await svc.PushTemplateToAllTeamsAsync(sourceProjectId, userId);
    app.Logger.LogInformation("AdminPushTemplateToAllTeams: operator={UserId} source={SourceId} teams={TeamCount}", userId, sourceProjectId, results.Count);
    return ApiResponseHelper.JsonNet(new { success = true, teams = results });
});

// ============= Admin: 团队管理 =============

// POST /api/Admin/PromoteToSystemTemplate → 将团队模板提升为系统模板
app.MapPost("/api/Admin/PromoteToSystemTemplate", async (
    HttpContext ctx,
    ProjectRepository projectRepo,
    UserRepository userRepo) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var caller = await userRepo.GetByIdAsync(userId);
    if (caller == null || !caller.IsSystemAdmin)
        return ApiResponseHelper.Error("仅系统管理员可访问", 403);

    var body = await ReadBodyAsync(ctx);
    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
    if (data == null || !data.TryGetValue("projectId", out var pidStr) || !Guid.TryParse(pidStr, out var projectId))
        return ApiResponseHelper.Error("请求体无效，需要 projectId");

    try
    {
        await projectRepo.PromoteToSystemTemplateAsync(projectId, userId);
        app.Logger.LogInformation("AdminPromoteToSystemTemplate: operator={UserId} project={ProjectId}", userId, projectId);
        return ApiResponseHelper.JsonNet(new { success = true });
    }
    catch (UnauthorizedAccessException ex)
    {
        return ApiResponseHelper.Error(ex.Message, 403);
    }
    catch (InvalidOperationException ex)
    {
        return ApiResponseHelper.Error(ex.Message, 400);
    }
});

// GET /api/Admin/Teams → 分页搜索团队
app.MapGet("/api/Admin/Teams", async (
    HttpContext ctx,
    TeamRepository teamRepo,
    SqliteStorage db,
    int page = 1,
    int pageSize = 20,
    string? keyword = null) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var (teams, total) = await teamRepo.SearchTeamsAsync(keyword, page, pageSize);
    return ApiResponseHelper.JsonNet(new { teams, total });
});

// POST /api/Admin/UpdateTeam → 管理员更新团队
app.MapPost("/api/Admin/UpdateTeam", async (
    HttpContext ctx,
    TeamRepository teamRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var teamIdStr = data.Value<string?>("teamId") ?? data.Value<string?>("TeamId") ?? "";
    var name = data.Value<string?>("name") ?? data.Value<string?>("Name");
    var level = data.Value<int?>("level") ?? data.Value<int?>("Level") ?? 3;
    var payStatus = data.Value<int?>("payStatus") ?? data.Value<int?>("PayStatus") ?? 1;
    var maxUsers = data.Value<int?>("maxUsers") ?? data.Value<int?>("MaxUsers") ?? 5;
    var maxProjects = data.Value<int?>("maxProjects") ?? data.Value<int?>("MaxProjects") ?? 10;
    var maxTemplates = data.Value<int?>("maxTemplates") ?? data.Value<int?>("MaxTemplates") ?? 5;

    if (!Guid.TryParse(teamIdStr, out var teamId))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "teamId 无效" }, 400);

    var ok = await teamRepo.AdminUpdateTeamAsync(teamId, name, level, payStatus, maxUsers, maxProjects, maxTemplates);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "team_not_found", message = "团队不存在" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// POST /api/Admin/CreateTeam → 管理员创建新团队（自动创建试用 License）
app.MapPost("/api/Admin/CreateTeam", async (
    HttpContext ctx,
    TeamService teamSvc,
    LicenseRepository licenseRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var teamName = data.Value<string>("teamName") ?? data.Value<string>("TeamName") ?? "";
    var type = data.Value<int?>("type") ?? 0;

    if (string.IsNullOrEmpty(teamName))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "团队名称不能为空" }, 400);

    var adminUserId = HeaderParser.ParseUserId(ctx);
    var team = await teamSvc.CreateTeamAsync(adminUserId, teamName, type);

    await licenseRepo.CreateLicenseAsync(
        ownerType: "Team",
        ownerId: team.Id.ToString("D"),
        planType: 0,
        seats: 1,
        maxProjects: 1,
        maxTemplates: 1,
        startDate: DateTime.Now,
        endDate: DateTime.Now.AddDays(7)
    );

    app.Logger.LogInformation("管理员创建团队成功: teamName={TeamName} teamId={TeamId}", teamName, team.Id);
    return ApiResponseHelper.JsonNet(new { success = true, teamId = team.Id, teamName = team.Name });
});

// GET /api/Admin/Teams/{teamId}/Members → 获取团队成员列表
app.MapGet("/api/Admin/Teams/{teamId}/Members", async (
    HttpContext ctx,
    Guid teamId,
    TeamRepository teamRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var members = await teamRepo.GetTeamMembersAsync(teamId);
    return ApiResponseHelper.JsonNet(new { members });
});

// POST /api/Admin/RemoveTeamMember → 移除团队成员
app.MapPost("/api/Admin/RemoveTeamMember", async (
    HttpContext ctx,
    TeamRepository teamRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var userId = data.Value<long?>("userId") ?? data.Value<long?>("UserId") ?? 0;
    var teamIdStr = data.Value<string?>("teamId") ?? data.Value<string?>("TeamId") ?? "";

    if (userId <= 0 || !Guid.TryParse(teamIdStr, out var teamId))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    var ok = await teamRepo.RemoveTeamMemberAsync(userId, teamId);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "not_found", message = "成员或团队不存在" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// POST /api/Admin/AddUserToTeam → 管理员将用户加入指定团队（多团队配置）
// 与 /api/Project/AddUserToTeam 不同：管理端直接调用 TeamRepository，不校验调用者是否 TeamAdmin
// （IsCurrentUserSystemAdminAsync 已校验调用者是系统管理员）
app.MapPost("/api/Admin/AddUserToTeam", async (
    HttpContext ctx,
    TeamRepository teamRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var userId = data.Value<long?>("userId") ?? data.Value<long?>("UserId") ?? 0;
    var teamIdStr = data.Value<string?>("teamId") ?? data.Value<string?>("TeamId") ?? "";

    if (userId <= 0 || !Guid.TryParse(teamIdStr, out var teamId))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "参数错误" }, 400);

    // 校验团队存在且未软删除
    using (var conn = db.CreateConnection())
    {
        await conn.OpenAsync();
        using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(1) FROM Teams WHERE Id = @tid AND (IsDeleted = 0 OR IsDeleted IS NULL)";
        checkCmd.Parameters.AddWithValue("@tid", teamId.ToString("D"));
        if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) == 0)
            return ApiResponseHelper.JsonNet(new { error = "not_found", message = "团队不存在或已删除" }, 404);
    }

    await teamRepo.AddUserToTeamAsync(userId, teamId);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// POST /api/Admin/DeleteTeam → 管理员删除团队（软删除）
app.MapPost("/api/Admin/DeleteTeam", async (
    HttpContext ctx,
    TeamRepository teamRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var teamIdStr = data.Value<string?>("teamId") ?? data.Value<string?>("TeamId") ?? "";

    if (!Guid.TryParse(teamIdStr, out var teamId))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "teamId 无效" }, 400);

    var ok = await teamRepo.AdminDeleteTeamAsync(teamId);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "team_not_found", message = "团队不存在" }, 404);

    app.Logger.LogInformation("管理员删除团队: TeamId={TeamId}", teamId);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// ============= Admin: 邀请管理 =============

// GET /api/Admin/Invitations → 分页获取待接受邀请
app.MapGet("/api/Admin/Invitations", async (
    HttpContext ctx,
    InvitationRepository invitationRepo,
    SqliteStorage db,
    int page = 1,
    int pageSize = 20) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var (invitations, total) = await invitationRepo.GetAllPendingInvitationsAsync(page, pageSize);
    return ApiResponseHelper.JsonNet(new { invitations, total });
});

// POST /api/Admin/RevokeInvitation → 撤销邀请
app.MapPost("/api/Admin/RevokeInvitation", async (
    HttpContext ctx,
    InvitationRepository invitationRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden" }, 403);

    var data = JObject.Parse(await ReadBodyAsync(ctx));
    var inviteToken = data.Value<string?>("inviteToken") ?? data.Value<string?>("InviteToken") ?? "";

    if (string.IsNullOrEmpty(inviteToken))
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "inviteToken 不能为空" }, 400);

    var ok = await invitationRepo.RevokeInvitationAsync(inviteToken);
    if (!ok)
        return ApiResponseHelper.JsonNet(new { error = "not_found", message = "邀请不存在或已处理" }, 404);
    return ApiResponseHelper.JsonNet(new { success = true });
});

app.MapPost("/api/Admin/ChangePassword", async (
    HttpContext ctx,
    UserRepository userRepo,
    SqliteStorage db) =>
{
    if (!await IsCurrentUserSystemAdminAsync(ctx, db))
        return ApiResponseHelper.JsonNet(new { error = "forbidden", message = "无管理员权限" }, 403);

    var body = await ReadBodyAsync(ctx);
    var data = JObject.Parse(body);
    var oldPassword = data.Value<string?>("oldPassword") ?? data.Value<string?>("OldPassword");
    var newPassword = data.Value<string?>("newPassword") ?? data.Value<string?>("NewPassword");

    if (oldPassword == null || oldPassword.Length == 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "原密码不能为空" }, 400);
    if (newPassword == null || newPassword.Length == 0)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "新密码不能为空" }, 400);
    if (newPassword.Length < 6)
        return ApiResponseHelper.JsonNet(new { error = "invalid_request", message = "新密码长度至少6位" }, 400);

    var userId = HeaderParser.ParseUserId(ctx);
    var user = await userRepo.GetByIdAsync(userId);
    if (user == null)
        return ApiResponseHelper.JsonNet(new { error = "not_found", message = "用户不存在" }, 404);

    // 修复 2026-07-21：dashboard.js 发送明文密码（未做 SHA256），但 VerifyPassword 期望 Base64(SHA256(明文))，
    // 此前直接传明文会导致原密码校验失败；即使校验通过，存储格式 PBKDF2(UrlEncode(明文)) 与登录验证
    // 期望的 PBKDF2(UrlEncode(Base64(SHA256(明文)))) 不一致，导致修改后桌面客户端无法用新密码登录。
    // 修复方案：与 ResetPasswordByAdminAsync (UserRepository.cs:451) 保持一致，
    // 在服务端计算 SHA256+Base64 后传入 VerifyPassword / HashPassword。
    var oldSha256Base64 = Convert.ToBase64String(
        SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(oldPassword)));
    if (!VerifyPassword(user, oldSha256Base64))
        return ApiResponseHelper.JsonNet(new { error = "invalid_password", message = "原密码错误" }, 401);

    // 与 ResetPasswordByAdminAsync 一致：SHA256(明文) → Base64 → UrlEncode → PBKDF2
    var newSha256Base64 = Convert.ToBase64String(
        SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(newPassword)));
    var encodedNewPwd = HttpUtility.UrlEncode(newSha256Base64);
    var salt = PasswordHasher.GenerateSalt();
    var newHash = PasswordHasher.HashPassword(encodedNewPwd, salt);

    await userRepo.UpdatePasswordWithSaltAsync(userId, newHash, salt, 1);
    return ApiResponseHelper.JsonNet(new { success = true });
});

// ============= 阶段 7 辅助方法 =============

/// <summary>
/// 构建数据字典响应。客户端版本号 >= 服务端最新版本时返回 { update: "0" }，
/// 否则返回 { update: "1", version, ...字典数据 }。
/// </summary>
static async Task<IResult> BuildDictionaryResponseAsync(DictionaryRepository repo, string dicType, int clientVersion)
{
    var latest = await repo.GetAsync(dicType, clientVersion);
    if (latest == null)
        return ApiResponseHelper.JsonNet(new { update = "0" });

    // 解析存储的字典数据 JSON，合并 update + version 字段
    var dataJson = latest.Data ?? "{}";
    var response = JObject.Parse(dataJson);
    response["update"] = "1";
    response["version"] = latest.Version;
    return ApiResponseHelper.JsonNet(response);
}

/// <summary>
/// 查询指定目标（Image/Pdf）的最新版本号。无记录返回 0。
/// TargetId 以字符串形式存储（兼容 long 类型的 image.Id 与 Guid 类型的 table.Id）。
/// </summary>
static async Task<int> QueryLatestTargetVersionAsync(SqliteStorage db, Guid projectId, string targetId, string targetType)
{
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT MAX(Version) FROM VersionHistory
        WHERE ProjectId = @pid AND TargetId = @tid AND TargetType = @ttype";
    cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
    cmd.Parameters.AddWithValue("@tid", targetId);
    cmd.Parameters.AddWithValue("@ttype", targetType);
    var result = await cmd.ExecuteScalarAsync();
    if (result == null || result == DBNull.Value) return 0;
    return Convert.ToInt32(result);
}

/// <summary>
/// 批量查询多个目标的最新版本号：单次连接 + 分组聚合 SQL（GROUP BY TargetId）。
/// 逐条调用 QueryLatestTargetVersionAsync 时每个目标都要新建并 Open 一个 SQLite 连接，
/// 项目节点多时（数百~上千张表/文档）串行累加，客户端 QueryVersion 的 30 秒超时会被打满，
/// 超时异常会中断整批同步（客户端 MainForm.SyncProjectImpl 的版本查询段）。
/// 返回 TargetId → 最新版本号；未命中的目标不在字典中（调用方按 0 处理）。
/// </summary>
static async Task<Dictionary<string, int>> QueryLatestTargetVersionsAsync(
    SqliteStorage db, Guid projectId, string targetType, IEnumerable<string>? targetIds)
{
    var result = new Dictionary<string, int>(StringComparer.Ordinal);
    if (targetIds == null) return result;

    var ids = new List<string>();
    var seen = new HashSet<string>(StringComparer.Ordinal);
    foreach (var id in targetIds)
    {
        if (string.IsNullOrEmpty(id)) continue;
        if (seen.Add(id)) ids.Add(id);
    }
    if (ids.Count == 0) return result;

    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    var pid = projectId.ToString("D");
    // SQLite 默认变量上限 999：每批 400 个 Id + ProjectId + TargetType，留足余量
    const int batchSize = 400;
    for (var offset = 0; offset < ids.Count; offset += batchSize)
    {
        var chunk = ids.GetRange(offset, Math.Min(batchSize, ids.Count - offset));
        using var cmd = conn.CreateCommand();
        var inList = new List<string>(chunk.Count);
        for (var i = 0; i < chunk.Count; i++)
        {
            var name = "@t" + i;
            inList.Add(name);
            cmd.Parameters.AddWithValue(name, chunk[i]);
        }
        cmd.CommandText = $@"
        SELECT TargetId, MAX(Version) FROM VersionHistory
        WHERE ProjectId = @pid AND TargetType = @ttype AND TargetId IN ({string.Join(",", inList)})
        GROUP BY TargetId";
        cmd.Parameters.AddWithValue("@pid", pid);
        cmd.Parameters.AddWithValue("@ttype", targetType);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (reader.IsDBNull(0)) continue;
            var tid = reader.GetString(0);
            if (string.IsNullOrEmpty(tid)) continue;
            result[tid] = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
        }
    }
    return result;
}

/// <summary>
/// 查询指定目标的最新快照记录。无记录返回 null。
/// </summary>
static async Task<VersionHistoryDto?> QueryTargetSnapshotAsync(SqliteStorage db, Guid projectId, string targetId, string targetType)
{
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT Id, ProjectId, TargetId, TargetType, Version, ChangeType, Snapshot, CreatedAt, CreatedBy
        FROM VersionHistory
        WHERE ProjectId = @pid AND TargetId = @tid AND TargetType = @ttype
        ORDER BY Version DESC LIMIT 1";
    cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
    cmd.Parameters.AddWithValue("@tid", targetId);
    cmd.Parameters.AddWithValue("@ttype", targetType);
    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;
    return new VersionHistoryDto
    {
        Id = reader.GetInt64(0),
        ProjectId = Guid.Parse(reader.GetString(1)),
        TargetId = Guid.Empty, // TargetId 可能是 long 转字符串，不解析为 Guid
        TargetType = reader.IsDBNull(3) ? null : reader.GetString(3),
        Version = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
        ChangeType = reader.IsDBNull(5) ? null : reader.GetString(5),
        Snapshot = reader.IsDBNull(6) ? null : (byte[])reader.GetValue(6),
        CreatedAt = reader.IsDBNull(7) ? DateTime.Now : DateTime.Parse(reader.GetString(7)),
        CreatedBy = reader.IsDBNull(8) ? null : reader.GetInt64(8)
    };
}

/// <summary>
/// 保存图片/PDF 版本快照到 VersionHistory 表。TargetId 以字符串形式存储。
/// </summary>
static async Task SaveImagePdfSnapshotAsync(
    SqliteStorage db, Guid projectId, string targetId, string targetType,
    string changeType, byte[]? snapshot, long? createdBy)
{
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    // 安全审计修复（Med）：原实现由调用方"SELECT MAX(Version) → +1 → INSERT"两步完成，
    // 并发推送同一目标时会产生重复版本号。改为单条 INSERT..SELECT，
    // 新版本号 = COALESCE(MAX(Version),0)+1 在写入瞬间原子计算（SQLite 单写者下语句级原子）。
    cmd.CommandText = @"
        INSERT INTO VersionHistory (ProjectId, TargetId, TargetType, Version, ChangeType, Snapshot, CreatedAt, CreatedBy)
        SELECT @pid, @tid, @ttype,
               COALESCE((SELECT MAX(Version) FROM VersionHistory
                         WHERE ProjectId = @pid AND TargetId = @tid AND TargetType = @ttype), 0) + 1,
               @ctype, @snap, @cts, @cby;";
    cmd.Parameters.AddWithValue("@pid", projectId.ToString("D"));
    cmd.Parameters.AddWithValue("@tid", targetId);
    cmd.Parameters.AddWithValue("@ttype", targetType);
    cmd.Parameters.AddWithValue("@ctype", (object?)changeType ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@snap", (object?)snapshot ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@cts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    cmd.Parameters.AddWithValue("@cby", (object?)createdBy ?? DBNull.Value);
    await cmd.ExecuteNonQueryAsync();
}

// ============= 辅助方法 =============

static long ParseUserId(HttpContext ctx)
{
    var headers = ctx.Request.Headers;
    if (long.TryParse(headers["UserId"].ToString(), out var uid)) return uid;
    if (long.TryParse(headers["userid"].ToString(), out uid)) return uid;
    return 0;
}

// 解析 hasProcess 查询参数（客户端传 "True"/"False" 字符串）→ 0/1
static int ParseHasProcess(HttpContext ctx)
{
    var v = ctx.Request.Query["hasProcess"].ToString();
    if (bool.TryParse(v, out var b)) return b ? 1 : 0;
    if (int.TryParse(v, out var i)) return i;
    return 0;
}

// 读取请求 Body 为字符串（用于 Newtonsoft.Json 反序列化）
// 自动检测并解压 gzip 压缩的请求体（兼容 Content-Encoding 缺失的情况）
static async Task<string> ReadBodyAsync(HttpContext ctx)
{
    // 启用请求缓冲，允许多次读取请求体
    ctx.Request.EnableBuffering();

    var bodyStream = ctx.Request.Body;

    // 读取前两个字节检测是否为 gzip（魔数 0x1F 0x8B）
    var buffer = new byte[2];
    var bytesRead = await bodyStream.ReadAtLeastAsync(buffer, 2, throwOnEndOfStream: false);
    if (bytesRead >= 2 && buffer[0] == 0x1F && buffer[1] == 0x8B)
    {
        // 是 gzip 数据，解压后读取
        bodyStream.Seek(0, SeekOrigin.Begin);
        // 安全审计修复（Med）：解压炸弹防护。原实现 ReadToEndAsync 无大小上限，
        // 恶意构造的高压缩比请求体（zip bomb）可将数百 MB 内存放大为数 GB。改为按 64KB 块复制，
        // 累计超过 200MB 中止并抛出异常（由全局异常处理器转 400）。
        const long MaxDecompressedBytes = 209_715_200L; // 200MB
        using var gzipStream = new GZipStream(bodyStream, CompressionMode.Decompress, leaveOpen: true);
        using var ms = new MemoryStream();
        var chunk = new byte[64 * 1024];
        long totalDecompressed = 0;
        int n;
        while ((n = await gzipStream.ReadAsync(chunk, 0, chunk.Length)) > 0)
        {
            totalDecompressed += n;
            if (totalDecompressed > MaxDecompressedBytes)
                throw new ArgumentException($"请求体解压后超过 {MaxDecompressedBytes / (1024 * 1024)}MB 上限");
            await ms.WriteAsync(chunk, 0, n);
        }
        bodyStream.Seek(0, SeekOrigin.Begin);
        return System.Text.Encoding.UTF8.GetString(ms.ToArray());
    }

    // 普通文本，直接读取
    bodyStream.Seek(0, SeekOrigin.Begin);
    using var sr2 = new StreamReader(bodyStream, leaveOpen: true);
    var bodyText = await sr2.ReadToEndAsync();
    bodyStream.Seek(0, SeekOrigin.Begin);
    return bodyText;
}

/// <summary>
/// 管理面板权限校验：当前请求用户必须为 IsSystemAdmin 且 IsActive=1。
/// 供所有 /api/Admin/* 端点统一调用。
/// </summary>
static async Task<bool> IsCurrentUserSystemAdminAsync(HttpContext ctx, SqliteStorage db)
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return false;
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT IsSystemAdmin FROM Users WHERE Id = @id AND IsActive = 1";
    cmd.Parameters.AddWithValue("@id", userId);
    var result = await cmd.ExecuteScalarAsync();
    return result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;
}

// 安全审计修复（High）辅助：查询用户分组所属团队 Id。分组不存在返回 null。
static async Task<Guid?> QueryUserGroupTeamIdAsync(SqliteStorage db, long groupId)
{
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT TeamId FROM UserGroups WHERE Id = @gid";
    cmd.Parameters.AddWithValue("@gid", groupId);
    var v = await cmd.ExecuteScalarAsync();
    if (v == null || v == DBNull.Value) return null;
    return Guid.TryParse(Convert.ToString(v), out var g) ? g : null;
}

// 安全审计修复（Med）辅助：校验调用者对 License 的归属（系统管理员豁免由调用侧判断）。
// OwnerType="Team"：OwnerId 为团队 GUID 字符串，校验调用者是该团队成员；
// OwnerType="User"：校验 OwnerId 即调用者 Id。
static async Task<bool> IsLicenseOwnedByCallerAsync(SqliteStorage db, long callerId, string? ownerType, string? ownerId)
{
    if (callerId <= 0 || string.IsNullOrEmpty(ownerId)) return false;
    if (ownerType == "User") return ownerId == callerId.ToString();

    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"SELECT COUNT(1) FROM UserTeams
                        WHERE UserId = @uid AND TeamId = @oid
                        AND EXISTS (SELECT 1 FROM Teams t WHERE t.Id = UserTeams.TeamId AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL))";
    cmd.Parameters.AddWithValue("@uid", callerId);
    cmd.Parameters.AddWithValue("@oid", ownerId);
    return Convert.ToInt64(await cmd.ExecuteScalarAsync()) > 0;
}

// 跨团队访问校验：基于 TenantContextAccessor.TeamId 过滤的项目归属检查。
// 返回 null 表示通过；返回 IResult（403）表示拒绝（项目不存在或不属于当前团队）。
// 改造：使用 GetProjectDtoNoTenantAsync 绕过租户过滤，支持系统模板（TeamId=NULL）。
//   - 系统模板（IsTemplate=1 且 TeamId=NULL）：仅系统管理员（IsSystemAdmin=1）可写；
//     只读端点（SystemTemplateReadonlyEndpoints 白名单）对已登录用户放行只读查看；
//     allowMemberCopySystemTemplate=true 时放行普通团队成员（仅用于「复制模板」——
//     复制产物归调用者团队，无越权写风险，对应客户端引导文案"复制模板创建团队副本后编辑"）
//   - 团队项目/模板：校验 TeamId 与当前租户匹配
static async Task<IResult?> CheckProjectAccessAsync(HttpContext ctx, Guid projectId, ProjectService svc, ILogger logger, bool allowMemberCopySystemTemplate = false, ReviewRepository? reviewRepo = null, bool allowPendingReviewerAccess = false)
{
    var userId = HeaderParser.ParseUserId(ctx);
    var project = await svc.GetProjectDtoNoTenantAsync(projectId);
    if (project == null)
    {
        logger.LogWarning("跨团队访问被拒（项目不存在）: userId={UserId} projectId={ProjectId} endpoint={Path} method={Method}",
            userId, projectId, ctx.Request.Path, ctx.Request.Method);
        return ApiResponseHelper.Forbidden("无权访问该项目");
    }

    // 系统下发的模板（TeamId=NULL 且 Type=1 模板）：仅系统管理员可编辑
    if (project.Type == 1 && (project.TeamId == null || project.TeamId == Guid.Empty))
    {
        if (userId == 0)
        {
            logger.LogWarning("系统模板访问被拒（未登录）: projectId={ProjectId} endpoint={Path}",
                projectId, ctx.Request.Path);
            return ApiResponseHelper.Unauthorized();
        }
        var isSystemAdmin = await svc.IsSystemAdminAsync(userId);
        if (!isSystemAdmin)
        {
            // 只读查看放行：读路径白名单端点对已登录用户开放；写路径仍要求系统管理员
            var reqPath = ctx.Request.Path.Value?.TrimEnd('/');
            if (reqPath != null && SystemTemplateReadonlyEndpoints.Paths.Contains(reqPath))
            {
                logger.LogInformation("系统模板只读访问放行: userId={UserId} projectId={ProjectId} endpoint={Path}",
                    userId, projectId, ctx.Request.Path);
                return null;
            }
            if (allowMemberCopySystemTemplate)
            {
                logger.LogInformation("系统模板复制放行（团队成员创建团队副本）: userId={UserId} projectId={ProjectId} endpoint={Path}",
                    userId, projectId, ctx.Request.Path);
                return null;
            }
            logger.LogWarning("系统模板访问被拒（非系统管理员）: userId={UserId} projectId={ProjectId} endpoint={Path}",
                userId, projectId, ctx.Request.Path);
            return ApiResponseHelper.Forbidden("系统下发的模板仅系统管理员可编辑，请使用「复制模板」创建团队副本后编辑");
        }
        return null;  // 系统管理员放行
    }

    // 团队项目/模板：校验 TeamId 与当前租户匹配
    // 安全审计修复（High）：原实现在 currentTeamId 为空时跳过校验直接放行（fail-open），
    // 租户上下文缺失（如 TenantIsolationFilter 解析失败）即可跨团队访问。改为 fail-closed。
    var currentTeamId = TenantContextAccessor.Current?.TeamId;
    var projectTeamId = project.TeamId?.ToString() ?? "";
    if (string.IsNullOrEmpty(currentTeamId) || projectTeamId != currentTeamId)
    {
        // 审批人只读放行（Task 5.5）：仅读端点显式开启——常规校验失败后追加检查，
        // 若当前用户是该进行中审核单的当前节点审批人（非项目成员的只读访问者），放行只读访问
        if (allowPendingReviewerAccess && userId > 0 && reviewRepo != null
            && await reviewRepo.GetPendingSubmissionForReviewerAsync(projectId, userId) != null)
        {
            logger.LogInformation("审批人只读访问放行: userId={UserId} projectId={ProjectId} endpoint={Path}",
                userId, projectId, ctx.Request.Path);
            return null;
        }
        logger.LogWarning("跨团队访问被拒: userId={UserId} projectId={ProjectId} projectTeamId={ProjectTeamId} currentTeamId={CurrentTeamId} endpoint={Path}",
            userId, projectId, projectTeamId, currentTeamId, ctx.Request.Path);
        return ApiResponseHelper.Forbidden("无权访问该项目");
    }

    return null;
}

// V2-H-22 辅助：邮箱脱敏（保留前 2 位 + @ 域名）
static string MaskEmail(string email)
{
    if (string.IsNullOrEmpty(email) || !email.Contains('@')) return email;
    var at = email.IndexOf('@');
    var local = email.Substring(0, at);
    var domain = email.Substring(at);
    var visible = local.Length <= 2 ? local : local.Substring(0, 2);
    return $"{visible}***{domain}";
}

// V2-H-22 辅助：手机号脱敏（保留前 3 位 + 后 4 位）
static string MaskPhone(string phone)
{
    if (string.IsNullOrEmpty(phone) || phone.Length < 7) return phone;
    return phone.Substring(0, 3) + "****" + phone.Substring(phone.Length - 4);
}

// V2-H-11 辅助：尝试删除文件，吞掉异常（用于上传失败时清理已写入的部分文件）。
static void TryDeleteFile(string path)
{
    try
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            File.Delete(path);
        }
    }
    catch (Exception)
    {
        // 清理失败不抛出，避免掩盖原始错误
    }
}

// ============= 阶段 10：项目同步辅助方法 =============

// DefaultPermissions 常量（与客户端 ProjectDAL.DefaultPermissions 一致）
// {"Read":{"GrantAll":true},"Write":{"GrantAll":true},"Schema":{"GrantAll":true}}
const string DefaultProjectPermissions = "{\"Read\":{\"GrantAll\":true},\"Write\":{\"GrantAll\":true},\"Schema\":{\"GrantAll\":true}}";

// 处理 PushProjectQuick / PushProject 的 JObject 请求体
// 解析 Groups/Nodes/DataRefs/VFs 数组，根据 Action 字段执行 INSERT/UPDATE/DELETE
// 处理完后递增 Projects.Version，返回 (newVersion, status)
// status: "Success" | "OutOfDate"（clientVersion < serverVersion 时返回 OutOfDate，客户端 Pull 后重试）
// 乐观锁：clientVersion > 0 时校验 clientVersion >= serverVersion；CAS 递增 Version 失败时重试 maxRetry 次
static async Task<(int newVersion, string status)> ProcessPushProjectAsync(
    SqliteStorage db, Guid projectId, JObject data, int clientVersion, int maxRetry = 5)
{
    var pid = projectId.ToString();

    for (int attempt = 0; attempt < maxRetry; attempt++)
    {
        using var conn = db.CreateConnection();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();

        // 读取服务端当前 Version（乐观锁 expected version）
        int serverVersion;
        using (var verReadCmd = conn.CreateCommand())
        {
            verReadCmd.Transaction = (SqliteTransaction)tx;
            verReadCmd.CommandText = "SELECT Version FROM Projects WHERE Id = @pid";
            verReadCmd.Parameters.AddWithValue("@pid", pid);
            var verResult = await verReadCmd.ExecuteScalarAsync();
            serverVersion = verResult == null || verResult == DBNull.Value ? 0 : Convert.ToInt32(verResult);
        }

        // 乐观锁检查：clientVersion > 0 时校验，clientVersion=0 跳过（兼容老客户端首次推送）
        if (clientVersion > 0 && clientVersion < serverVersion)
        {
            await tx.RollbackAsync();
            // 乐观锁冲突日志由端点层记录（静态方法无法访问 app.Logger）
            return (serverVersion, "OutOfDate");
        }

        try
        {
    // 处理 Groups
    if (data["Groups"] is JArray groups)
    {
        foreach (JObject g in groups.OfType<JObject>())
        {
            var action = g.Value<string>("Action") ?? "";
            var id = g.Value<long?>("Id") ?? 0;
            if (id == 0) continue;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            if (action == "New")
            {
                cmd.CommandText = @"INSERT OR REPLACE INTO ProjectTreeGroups (Id, ProjectId, Name, TreeIndex, ServerIndex, Status, Dirty)
                    VALUES (@id, @pid, @name, @idx, @sidx, 0, 0)";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
                cmd.Parameters.AddWithValue("@name", (object?)g.Value<string>("Name") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idx", g.Value<int?>("Index") ?? 0);
                cmd.Parameters.AddWithValue("@sidx", g.Value<int?>("Index") ?? 0);
            }
            else if (action == "Mod")
            {
                var sets = new List<string>();
                if (g["Name"] != null) sets.Add("Name = @name");
                if (g["Index"] != null) sets.Add("TreeIndex = @idx, ServerIndex = @sidx");
                if (sets.Count == 0) continue;
                cmd.CommandText = $"UPDATE ProjectTreeGroups SET {string.Join(", ", sets)} WHERE Id = @id AND ProjectId = @pid";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
                if (g["Name"] != null) cmd.Parameters.AddWithValue("@name", (object?)g.Value<string>("Name") ?? DBNull.Value);
                if (g["Index"] != null)
                {
                    var idx = g.Value<int?>("Index") ?? 0;
                    cmd.Parameters.AddWithValue("@idx", idx);
                    cmd.Parameters.AddWithValue("@sidx", idx);
                }
                var grpAffected = await cmd.ExecuteNonQueryAsync();
                // 修复：UPDATE 影响 0 行时，如果 JSON 中缺少 Name（只有 Index），
                // 不应用默认值 INSERT OR REPLACE，跳过等 New action 再写入。
                if (grpAffected == 0)
                {
                    if (g["Name"] == null)
                    {
                        continue;
                    }
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"INSERT OR REPLACE INTO ProjectTreeGroups (Id, ProjectId, Name, TreeIndex, ServerIndex, Status, Dirty)
                        VALUES (@id, @pid, @name, @idx, @sidx, 0, 0)";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@pid", pid);
                    cmd.Parameters.AddWithValue("@name", (object?)g.Value<string>("Name") ?? DBNull.Value);
                    var grpIdx = g.Value<int?>("Index") ?? 0;
                    cmd.Parameters.AddWithValue("@idx", grpIdx);
                    cmd.Parameters.AddWithValue("@sidx", grpIdx);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            else if (action == "Del")
            {
                cmd.CommandText = "DELETE FROM ProjectTreeGroups WHERE Id = @id AND ProjectId = @pid";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
            }
            else continue;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // 处理 Nodes
    if (data["Nodes"] is JArray nodes)
    {
        foreach (JObject n in nodes.OfType<JObject>())
        {
            var action = n.Value<string>("Action") ?? "";
            var id = n.Value<long?>("Id") ?? 0;
            if (id == 0) continue;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            if (action == "New")
            {
                cmd.CommandText = @"INSERT OR REPLACE INTO ProjectTreeNodes
                    (Id, ProjectId, GroupId, ParentId, Name, TreeIndex, ServerIndex, Status, Dirty,
                     Type, Level, Version, Number, Permissions, Visible, RowWrite, RowRead)
                    VALUES (@id, @pid, @gid, @parid, @name, @idx, @sidx, 0, 0,
                            @type, @level, 0, @num, @perm, @vis, @rw, @rr)";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
                cmd.Parameters.AddWithValue("@gid", n.Value<long?>("GroupId") ?? 0);
                cmd.Parameters.AddWithValue("@parid", (object?)n.Value<long?>("ParentId") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@name", (object?)n.Value<string>("Name") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idx", n.Value<int?>("Index") ?? 0);
                cmd.Parameters.AddWithValue("@sidx", n.Value<int?>("Index") ?? 0);
                cmd.Parameters.AddWithValue("@type", n.Value<int?>("Type") ?? 0);
                cmd.Parameters.AddWithValue("@level", n.Value<int?>("Level") ?? 0);
                cmd.Parameters.AddWithValue("@num", (object?)n.Value<string>("Number") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@perm", (object?)n.Value<string>("Permissions") ?? DefaultProjectPermissions);
                cmd.Parameters.AddWithValue("@vis", n.Value<bool?>("Visible") ?? true ? 1 : 0);
                cmd.Parameters.AddWithValue("@rw", n.Value<bool?>("RowWrite") ?? false ? 1 : 0);
                cmd.Parameters.AddWithValue("@rr", n.Value<bool?>("RowRead") ?? false ? 1 : 0);
            }
            else if (action == "Mod")
            {
                var sets = new List<string>();
                if (n["Name"] != null) sets.Add("Name = @name");
                if (n["Index"] != null) sets.Add("TreeIndex = @idx, ServerIndex = @sidx");
                if (n["Number"] != null) sets.Add("Number = @num");
                if (n["GroupId"] != null) sets.Add("GroupId = @gid");
                if (n["ParentId"] != null) sets.Add("ParentId = @parid");
                if (n["Permissions"] != null) sets.Add("Permissions = @perm");
                if (n["Visible"] != null) sets.Add("Visible = @vis");
                if (n["RowWrite"] != null) sets.Add("RowWrite = @rw");
                if (n["RowRead"] != null) sets.Add("RowRead = @rr");
                if (sets.Count == 0) continue;
                cmd.CommandText = $"UPDATE ProjectTreeNodes SET {string.Join(", ", sets)} WHERE Id = @id AND ProjectId = @pid";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
                if (n["Name"] != null) cmd.Parameters.AddWithValue("@name", (object?)n.Value<string>("Name") ?? DBNull.Value);
                if (n["Index"] != null)
                {
                    var idx = n.Value<int?>("Index") ?? 0;
                    cmd.Parameters.AddWithValue("@idx", idx);
                    cmd.Parameters.AddWithValue("@sidx", idx);
                }
                if (n["Number"] != null) cmd.Parameters.AddWithValue("@num", (object?)n.Value<string>("Number") ?? DBNull.Value);
                if (n["GroupId"] != null) cmd.Parameters.AddWithValue("@gid", n.Value<long?>("GroupId") ?? 0);
                if (n["ParentId"] != null) cmd.Parameters.AddWithValue("@parid", (object?)n.Value<long?>("ParentId") ?? DBNull.Value);
                if (n["Permissions"] != null) cmd.Parameters.AddWithValue("@perm", (object?)n.Value<string>("Permissions") ?? DefaultProjectPermissions);
                if (n["Visible"] != null) cmd.Parameters.AddWithValue("@vis", n.Value<bool?>("Visible") ?? true ? 1 : 0);
                if (n["RowWrite"] != null) cmd.Parameters.AddWithValue("@rw", n.Value<bool?>("RowWrite") ?? false ? 1 : 0);
                if (n["RowRead"] != null) cmd.Parameters.AddWithValue("@rr", n.Value<bool?>("RowRead") ?? false ? 1 : 0);
                var affected = await cmd.ExecuteNonQueryAsync();
                // 修复：项目从模板创建时 .db 中 TreeNode.Status=Synced，客户端推送 Mod
                // 但主表 ProjectTreeNodes 从未有该行（客户端从未推送过 New），UPDATE 影响 0 行。
                // 如果 JSON 中缺少 GroupId/Name 等关键字段（只有 Index），说明是 IsIndexDirty
                // 触发的部分推送，不能用默认值 INSERT OR REPLACE，否则主库会被空字段污染。
                // 跳过，等客户端发 New 时再写入完整数据。
                if (affected == 0)
                {
                    if (n["GroupId"] == null && n["Name"] == null)
                    {
                        continue;
                    }
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"INSERT OR REPLACE INTO ProjectTreeNodes
                        (Id, ProjectId, GroupId, ParentId, Name, TreeIndex, ServerIndex, Status, Dirty,
                         Type, Level, Version, Number, Permissions, Visible, RowWrite, RowRead)
                        VALUES (@id, @pid, @gid, @parid, @name, @idx, @sidx, 0, 0,
                                @type, @level, 0, @num, @perm, @vis, @rw, @rr)";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@pid", pid);
                    cmd.Parameters.AddWithValue("@gid", n.Value<long?>("GroupId") ?? 0);
                    cmd.Parameters.AddWithValue("@parid", (object?)n.Value<long?>("ParentId") ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@name", (object?)n.Value<string>("Name") ?? DBNull.Value);
                    var modIdx = n.Value<int?>("Index") ?? 0;
                    cmd.Parameters.AddWithValue("@idx", modIdx);
                    cmd.Parameters.AddWithValue("@sidx", modIdx);
                    cmd.Parameters.AddWithValue("@type", n.Value<int?>("Type") ?? 0);
                    cmd.Parameters.AddWithValue("@level", n.Value<int?>("Level") ?? 0);
                    cmd.Parameters.AddWithValue("@num", (object?)n.Value<string>("Number") ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@perm", (object?)n.Value<string>("Permissions") ?? DefaultProjectPermissions);
                    cmd.Parameters.AddWithValue("@vis", n.Value<bool?>("Visible") ?? true ? 1 : 0);
                    cmd.Parameters.AddWithValue("@rw", n.Value<bool?>("RowWrite") ?? false ? 1 : 0);
                    cmd.Parameters.AddWithValue("@rr", n.Value<bool?>("RowRead") ?? false ? 1 : 0);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            else if (action == "Del")
            {
                cmd.CommandText = "DELETE FROM ProjectTreeNodes WHERE Id = @id AND ProjectId = @pid";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
            }
            else continue;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // 处理 DataRefs
    if (data["DataRefs"] is JArray refs)
    {
        foreach (JObject r in refs.OfType<JObject>())
        {
            var action = r.Value<string>("Action") ?? "";
            var id = r.Value<long?>("Id") ?? 0;
            if (id == 0) continue;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            if (action == "New")
            {
                cmd.CommandText = @"INSERT OR REPLACE INTO ProjectDataReferences (Id, ProjectId, Key, Value, Kind, Status, Dirty)
                    VALUES (@id, @pid, @key, @val, @kind, 0, 0)";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
                cmd.Parameters.AddWithValue("@key", (object?)r.Value<string>("Key") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@val", (object?)r.Value<string>("Value") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@kind", r.Value<int?>("Kind") ?? 2);
            }
            else if (action == "Mod")
            {
                var sets = new List<string>();
                if (r["Key"] != null) sets.Add("Key = @key");
                if (r["Value"] != null) sets.Add("Value = @val");
                if (sets.Count == 0) continue;
                cmd.CommandText = $"UPDATE ProjectDataReferences SET {string.Join(", ", sets)} WHERE Id = @id AND ProjectId = @pid";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
                if (r["Key"] != null) cmd.Parameters.AddWithValue("@key", (object?)r.Value<string>("Key") ?? DBNull.Value);
                if (r["Value"] != null) cmd.Parameters.AddWithValue("@val", (object?)r.Value<string>("Value") ?? DBNull.Value);
            }
            else if (action == "Del")
            {
                cmd.CommandText = "DELETE FROM ProjectDataReferences WHERE Id = @id AND ProjectId = @pid";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
            }
            else continue;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // 处理 VFs (ValidationFormulas)
    if (data["VFs"] is JArray vfs)
    {
        foreach (JObject v in vfs.OfType<JObject>())
        {
            var action = v.Value<string>("Action") ?? "";
            var id = v.Value<long?>("Id") ?? 0;
            if (id == 0) continue;

            using var cmd = conn.CreateCommand();
            cmd.Transaction = (SqliteTransaction)tx;
            if (action == "New" || action == "Mod")
            {
                cmd.CommandText = @"INSERT OR REPLACE INTO ProjectValidationFormulas
                    (Id, ProjectId, LeftExpr, Operator, RightExpr, Note, TableId, DocumentFieldId, Status, Dirty)
                    VALUES (@id, @pid, @le, @op, @re, @note, @tid, @dfid, 0, 0)";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
                cmd.Parameters.AddWithValue("@le", (object?)v.Value<string>("LeftExpr") ?? DBNull.Value);
                // Op 为字符串操作符代码，尝试解析为整数，失败则存 0
                var opStr = v.Value<string>("Op") ?? v.Value<string>("Operator");
                var opVal = 0;
                if (!string.IsNullOrEmpty(opStr) && !int.TryParse(opStr, out opVal)) opVal = 0;
                cmd.Parameters.AddWithValue("@op", opVal);
                cmd.Parameters.AddWithValue("@re", (object?)v.Value<string>("RightExpr") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@note", (object?)v.Value<string>("Note") ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tid", (object?)v.Value<long?>("TableId") ?? DBNull.Value);
                // 修复：此前 DocumentFieldId 硬编码 NULL，Formula 域稽核规则的域绑定经云端同步后丢失
                cmd.Parameters.AddWithValue("@dfid", (object?)v.Value<long?>("DocumentFieldId") ?? DBNull.Value);
            }
            else if (action == "Del")
            {
                cmd.CommandText = "DELETE FROM ProjectValidationFormulas WHERE Id = @id AND ProjectId = @pid";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pid", pid);
            }
            else continue;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    // 递增 Projects.Version（CAS 乐观锁：WHERE Version = @expected）
    int newVersion;
    using (var verCmd = conn.CreateCommand())
    {
        verCmd.Transaction = (SqliteTransaction)tx;
        verCmd.CommandText = @"
            UPDATE Projects SET Version = Version + 1 WHERE Id = @pid AND Version = @expected;
            SELECT Version FROM Projects WHERE Id = @pid;";
        verCmd.Parameters.AddWithValue("@pid", pid);
        verCmd.Parameters.AddWithValue("@expected", serverVersion);
        var verResult = await verCmd.ExecuteScalarAsync();
        newVersion = verResult == null || verResult == DBNull.Value ? 0 : Convert.ToInt32(verResult);
    }

    // CAS 失败：Version 被其他请求修改，回滚重试
    if (newVersion != serverVersion + 1)
    {
        await tx.RollbackAsync();
        // CAS 重试日志由端点层记录（静态方法无法访问 app.Logger）
        continue;
    }

    // 记录变更历史到 ProjectChanges 表，用于 PullProject 增量返回
    // EntityType: 0=Group, 1=Node, 2=DataRef, 3=VF
    // Action: 0=New, 1=Mod, 2=Del
    foreach (var (entityType, arrayName) in new[] { (0, "Groups"), (1, "Nodes"), (2, "DataRefs"), (3, "VFs") })
    {
        if (data[arrayName] is not JArray arr) continue;
        foreach (JObject item in arr.OfType<JObject>())
        {
            var actionStr = item.Value<string>("Action") ?? "";
            var entityId = item.Value<long?>("Id") ?? 0;
            if (entityId == 0) continue;
            var actionCode = actionStr == "New" ? 0 : actionStr == "Mod" ? 1 : actionStr == "Del" ? 2 : -1;
            if (actionCode < 0) continue;

            // 存储时去掉 Action 字段（PullProject 返回时不需要）
            var clone = (JObject)item.DeepClone();
            clone.Remove("Action");

            using var ccmd = conn.CreateCommand();
            ccmd.Transaction = (SqliteTransaction)tx;
            ccmd.CommandText = @"INSERT INTO ProjectChanges (ProjectId, Version, EntityType, Action, EntityId, Payload)
                VALUES (@pid, @ver, @et, @act, @eid, @payload)";
            ccmd.Parameters.AddWithValue("@pid", pid);
            ccmd.Parameters.AddWithValue("@ver", newVersion);
            ccmd.Parameters.AddWithValue("@et", entityType);
            ccmd.Parameters.AddWithValue("@act", actionCode);
            ccmd.Parameters.AddWithValue("@eid", entityId);
            ccmd.Parameters.AddWithValue("@payload", clone.ToString(Newtonsoft.Json.Formatting.None));
            await ccmd.ExecuteNonQueryAsync();
        }
    }

            await tx.CommitAsync();
            return (newVersion, "Success");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    } // end for

    throw new InvalidOperationException($"PushProject 乐观锁冲突，重试 {maxRetry} 次后失败: ProjectId={projectId}");
}

// 查询项目元数据（用于 PullProjectDirect 生成 SQLite 文件）
static async Task<(Guid Id, string Name, Guid? ParentId, int Version, string Number, string Category, string Note, DateTime CreateTime, Guid? TemplateId, Guid? TeamId, int Type)?>
    QueryProjectForDirectAsync(SqliteStorage db, Guid projectId)
{
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Id, Name, ParentId, Version, Number, Category, Note, CreateTime, TemplateId, TeamId, Type FROM Projects WHERE Id = @pid";
    cmd.Parameters.AddWithValue("@pid", projectId.ToString());
    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;

    var id = Guid.TryParse(reader.GetString(0), out var g) ? g : Guid.Empty;
    var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
    Guid? parentId = null;
    if (!reader.IsDBNull(2) && Guid.TryParse(reader.GetString(2), out var pg)) parentId = pg;
    var version = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
    var number = reader.IsDBNull(4) ? "" : reader.GetString(4);
    var category = reader.IsDBNull(5) ? "" : reader.GetString(5);
    var note = reader.IsDBNull(6) ? "" : reader.GetString(6);
    var createTime = reader.IsDBNull(7) ? DateTime.Now : DateTime.TryParse(reader.GetString(7), out var ct) ? ct : DateTime.Now;
    Guid? templateId = null;
    if (!reader.IsDBNull(8) && Guid.TryParse(reader.GetString(8), out var tId)) templateId = tId;
    Guid? teamId = null;
    if (!reader.IsDBNull(9) && Guid.TryParse(reader.GetString(9), out var tmId)) teamId = tmId;
    var type = reader.IsDBNull(10) ? 0 : reader.GetInt32(10);

    return (id, name, parentId, version, number, category, note, createTime, templateId, teamId, type);
}

// 初始化 PullProjectDirect 的 SQLite 项目库 schema（与客户端 ProjectDAL.CreateConfig + UpdateSchema user_version=44 一致）
// 客户端打开时会校验 user_version；若 < 44 则重新执行 ALTER 升级，可能导致列已存在冲突
static async Task InitProjectDbSchemaAsync(SqliteConnection conn)
{
    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS `Project`(
`Id` GUID PRIMARY KEY,
`Name` TEXT NOT NULL,
`Parent` GUID,
`Version` INTEGER NOT NULL DEFAULT 0,
`Number` TEXT NOT NULL,
`Category` TEXT NOT NULL,
`Note` TEXT NOT NULL,
`CreateTime` TEXT NOT NULL DEFAULT '2000-01-01',
`CustomFillConfig` TEXT);

CREATE TABLE IF NOT EXISTS `TreeGroup`(
`Id` INTEGER PRIMARY KEY,
`Name` TEXT NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL);

CREATE TABLE IF NOT EXISTS `TreeNode`(
`Id` INTEGER PRIMARY KEY,
`GroupId` INTEGER NOT NULL,
`ParentId` INTEGER,
`Name` TEXT NOT NULL,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Type` INTEGER NOT NULL,
`Level` INTEGER NOT NULL,
`Version` INTEGER NOT NULL,
`Number` TEXT,
`Permissions` TEXT NOT NULL DEFAULT '" + DefaultProjectPermissions + @"',
`Visible` INTEGER NOT NULL DEFAULT 1,
`RowWrite` INTEGER NOT NULL DEFAULT 0,
`RowRead` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Table`(
`Id` INTEGER PRIMARY KEY,
`Title` TEXT NOT NULL,
`PageSetup` TEXT NOT NULL,
`Dirty` INTEGER NOT NULL,
`Note` TEXT NOT NULL,
`HeaderHeights` TEXT,
`DefaultStyleId` INTEGER NOT NULL,
`ConsolidateSettings` TEXT,
`BorderStyle` INTEGER NOT NULL DEFAULT 0,
`FrozenCols` INTEGER NOT NULL DEFAULT 0,
`HeaderMode` INTEGER NOT NULL DEFAULT 0,
`CollectSource` TEXT,
`Locker` INTEGER,
`FilterInfo` TEXT,
`Foot` TEXT NOT NULL DEFAULT '{}',
`RowOwnerExclusive` INTEGER NOT NULL DEFAULT 0,
`RowOwnerLoad` INTEGER NOT NULL DEFAULT 0,
`RowOwnerLoadShare` BLOB NOT NULL DEFAULT '',
`Ticket` TEXT NOT NULL DEFAULT '',
`ControlFormula` TEXT NOT NULL DEFAULT '',
`CustomBorderStyle` TEXT);

CREATE TABLE IF NOT EXISTS `Column`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Caption` TEXT NOT NULL,
`CaptionStyle` TEXT NOT NULL,
`Width` INTEGER NOT NULL,
`Visible` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`ConsolidateAttribs` TEXT,
`SubtotalAttribs` INTEGER NOT NULL DEFAULT 0,
`Formula` TEXT,
`StyleId` INTEGER,
`Permissions` TEXT NOT NULL DEFAULT '" + DefaultProjectPermissions + @"',
`CaptionFormula` TEXT DEFAULT '',
`CrossAttributes` BLOB NOT NULL DEFAULT '');

CREATE TABLE IF NOT EXISTS `Row`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Height` INTEGER NOT NULL,
`Visible` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Locked` INTEGER,
`Role` INTEGER,
`Permissions` TEXT NOT NULL DEFAULT '" + DefaultProjectPermissions + @"',
`Creator` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Cell`(
`Id` INTEGER PRIMARY KEY,
`RowId` INTEGER NOT NULL,
`ColumnId` INTEGER NOT NULL,
`Value` BLOB NOT NULL,
`StyleId` INTEGER,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Formula` TEXT,
`CollectSource` TEXT,
`HeaderFormula` TEXT DEFAULT '');

CREATE TABLE IF NOT EXISTS `CellStyle`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`FontFamily` TEXT,
`FontSize` REAL,
`ForeColor` INTEGER,
`BackColor` INTEGER,
`Align` INTEGER,
`Margin` INTEGER,
`Bold` INTEGER,
`Italic` INTEGER,
`Underline` INTEGER,
`DataType` INTEGER,
`Format` TEXT,
`Status` INTEGER NOT NULL,
`Locked` INTEGER,
`DefaultValue` TEXT,
`Comment` TEXT);

CREATE TABLE IF NOT EXISTS `Document`(
`Id` INTEGER PRIMARY KEY,
`Locker` INTEGER,
`SectPr` TEXT,
`MergeTable` INTEGER NOT NULL DEFAULT 0,
`Dirty` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Paragraph`(
`Id` INTEGER PRIMARY KEY,
`DocumentId` INTEGER NOT NULL,
`Index` INTEGER NOT NULL,
`Stream` BLOB NOT NULL,
`ServerIndex` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Section` BLOB,
`Comment` TEXT);

CREATE TABLE IF NOT EXISTS `Merge`(
`Id` INTEGER PRIMARY KEY,
`TableId` INTEGER NOT NULL,
`TopLeft` INTEGER NOT NULL,
`BottomRight` INTEGER NOT NULL,
`Status` INTEGER NOT NULL);

CREATE TABLE IF NOT EXISTS `DataReference`(
`Id` INTEGER PRIMARY KEY,
`Key` TEXT NOT NULL,
`Value` TEXT,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Kind` INTEGER NOT NULL DEFAULT 2);

CREATE TABLE IF NOT EXISTS `ValidationFormula`(
`Id` INTEGER PRIMARY KEY,
`LeftExpr` TEXT NOT NULL,
`Operator` INT NOT NULL,
`RightExpr` TEXT NOT NULL,
`Note` TEXT NOT NULL,
`Status` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`TableId` INTEGER,
`DocumentFieldId` INTEGER);

CREATE TABLE IF NOT EXISTS `Image`(
`Id` INTEGER PRIMARY KEY,
`FileId` GUID NOT NULL,
`Dirty` INTEGER NOT NULL DEFAULT 0,
`CenterX` REAL NOT NULL DEFAULT 0.5,
`CenterY` REAL NOT NULL DEFAULT 0.5,
`ZoomFactor` REAL NOT NULL DEFAULT 1.0,
`PageSetup` TEXT,
`RotateFlip` INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS `Pdf`(
`Id` INTEGER PRIMARY KEY,
`FileId` GUID NOT NULL);

CREATE TABLE IF NOT EXISTS `Snapshot`(
`Id` INTEGER PRIMARY KEY,
`TreeNodeId` INTEGER NOT NULL,
`DateTime` TEXT NOT NULL,
`Size` INTEGER NOT NULL,
`Kind` INTEGER NOT NULL,
`Name` TEXT NOT NULL,
`Deleted` INTEGER NOT NULL);

CREATE TABLE IF NOT EXISTS `CellProp` (
`TableId` INTEGER NOT NULL,
`CellId` INTEGER NOT NULL,
`Dirty` INTEGER NOT NULL,
`Status` INTEGER NOT NULL,
`Attachments` BLOB NOT NULL DEFAULT '',
PRIMARY KEY (`TableId`,`CellId`));

CREATE INDEX `idx_Cell_RowId` ON `Cell` (`RowId`);
CREATE INDEX `idx_Row_TableId` ON `Row` (`TableId`);
CREATE INDEX IF NOT EXISTS `idx_Column_TableId` ON `Column` (`TableId`);

PRAGMA user_version = 44;";
    await cmd.ExecuteNonQueryAsync();
}

// 写入 Project 表 1 条记录（从服务端 Projects 表查询的元数据）
static async Task WriteProjectRecordAsync(SqliteConnection conn,
    (Guid Id, string Name, Guid? ParentId, int Version, string Number, string Category, string Note, DateTime CreateTime, Guid? TemplateId, Guid? TeamId, int Type) project)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"INSERT OR REPLACE INTO `Project`(`Id`,`Name`,`Parent`,`Version`,`Number`,`Category`,`Note`,`CreateTime`,`CustomFillConfig`)
        VALUES (@Id,@Name,@ParentId,@Version,@Number,@Category,@Note,@CreateTime,@CustomFillConfig)";
    cmd.Parameters.AddWithValue("@Id", project.Id.ToByteArray());
    cmd.Parameters.AddWithValue("@Name", project.Name ?? "");
    cmd.Parameters.AddWithValue("@ParentId", project.ParentId.HasValue ? (object)project.ParentId.Value.ToByteArray() : DBNull.Value);
    cmd.Parameters.AddWithValue("@Version", project.Version);
    cmd.Parameters.AddWithValue("@Number", project.Number ?? "");
    cmd.Parameters.AddWithValue("@Category", project.Category ?? "");
    cmd.Parameters.AddWithValue("@Note", project.Note ?? "");
    cmd.Parameters.AddWithValue("@CreateTime", project.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    // 修复 bug：此前误将 TemplateId 写入 CustomFillConfig 字段。参数元组中无 CustomFillConfig，应写 DBNull.Value
    cmd.Parameters.AddWithValue("@CustomFillConfig", DBNull.Value);
    await cmd.ExecuteNonQueryAsync();
}

/// <summary>
/// 将项目 .db 中的 TreeGroup/TreeNode 全量数据写入主库（ProjectTreeGroups/ProjectTreeNodes）。
///
/// 修复核心问题：CreateProject 时 CreateEmptyProjectDb 从模板复制了全量数据到 .db 文件，
/// 但主库 ProjectTreeNodes 表为空。当用户推送增量修改到主库后，重新打开项目时
/// SyncMainToProjectDbAsync 会执行 DELETE+INSERT，用主库的增量数据覆盖 .db 的全量数据，
/// 导致 .db 中模板原有节点丢失，树形导航栏不完整。
///
/// 本方法在 CreateProject 后调用，将 .db 的全量数据 seed 到主库，确保主库从一开始就有完整数据。
/// 这样 SyncMainToProjectDbAsync 的 DELETE+INSERT 不会丢数据（主库 == .db 全量数据）。
/// </summary>
static async Task SeedMainDbFromProjectDbAsync(SqliteStorage db, string dbPath, Guid projectId)
{
    if (!File.Exists(dbPath)) return;
    var pid = projectId.ToString();

    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var tx = await conn.BeginTransactionAsync();

    try
    {
        using var projConn = new SqliteConnection($"Data Source={dbPath};Pooling=False;");
        await projConn.OpenAsync();

        // 1. TreeGroup: .db → 主库 ProjectTreeGroups
        using (var readCmd = projConn.CreateCommand())
        {
            readCmd.CommandText = "SELECT `Id`,`Name`,`Index`,`ServerIndex`,`Status`,`Dirty` FROM `TreeGroup`";
            using var reader = await readCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                using var insCmd = conn.CreateCommand();
                insCmd.Transaction = (SqliteTransaction)tx;
                insCmd.CommandText = @"INSERT OR REPLACE INTO ProjectTreeGroups (Id, ProjectId, Name, TreeIndex, ServerIndex, Status, Dirty)
                    VALUES (@id, @pid, @name, @idx, @sidx, 0, 0)";
                insCmd.Parameters.AddWithValue("@id", reader.IsDBNull(0) ? 0 : reader.GetInt64(0));
                insCmd.Parameters.AddWithValue("@pid", pid);
                insCmd.Parameters.AddWithValue("@name", reader.IsDBNull(1) ? "" : reader.GetString(1));
                insCmd.Parameters.AddWithValue("@idx", reader.IsDBNull(2) ? 0 : reader.GetInt32(2));
                insCmd.Parameters.AddWithValue("@sidx", reader.IsDBNull(3) ? 0 : reader.GetInt32(3));
                await insCmd.ExecuteNonQueryAsync();
            }
        }

        // 2. TreeNode: .db → 主库 ProjectTreeNodes
        using (var readCmd = projConn.CreateCommand())
        {
            readCmd.CommandText = @"SELECT `Id`,`GroupId`,`ParentId`,`Name`,`Index`,`ServerIndex`,`Status`,`Dirty`,
                `Type`,`Level`,`Version`,`Number`,`Permissions`,`Visible`,`RowWrite`,`RowRead` FROM `TreeNode`";
            using var reader = await readCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                using var insCmd = conn.CreateCommand();
                insCmd.Transaction = (SqliteTransaction)tx;
                insCmd.CommandText = @"INSERT OR REPLACE INTO ProjectTreeNodes
                    (Id, ProjectId, GroupId, ParentId, Name, TreeIndex, ServerIndex, Status, Dirty,
                     Type, Level, Version, Number, Permissions, Visible, RowWrite, RowRead)
                    VALUES (@id, @pid, @gid, @parid, @name, @idx, @sidx, 0, 0,
                            @type, @level, @ver, @num, @perm, @vis, @rw, @rr)";
                insCmd.Parameters.AddWithValue("@id", reader.IsDBNull(0) ? 0 : reader.GetInt64(0));
                insCmd.Parameters.AddWithValue("@pid", pid);
                insCmd.Parameters.AddWithValue("@gid", reader.IsDBNull(1) ? 0 : reader.GetInt64(1));
                insCmd.Parameters.AddWithValue("@parid", reader.IsDBNull(2) ? DBNull.Value : (object)reader.GetInt64(2));
                insCmd.Parameters.AddWithValue("@name", reader.IsDBNull(3) ? "" : reader.GetString(3));
                insCmd.Parameters.AddWithValue("@idx", reader.IsDBNull(4) ? 0 : reader.GetInt32(4));
                insCmd.Parameters.AddWithValue("@sidx", reader.IsDBNull(5) ? 0 : reader.GetInt32(5));
                insCmd.Parameters.AddWithValue("@type", reader.IsDBNull(8) ? 0 : reader.GetInt32(8));
                insCmd.Parameters.AddWithValue("@level", reader.IsDBNull(9) ? 0 : reader.GetInt32(9));
                insCmd.Parameters.AddWithValue("@ver", reader.IsDBNull(10) ? 0 : reader.GetInt32(10));
                insCmd.Parameters.AddWithValue("@num", reader.IsDBNull(11) ? DBNull.Value : (object)reader.GetString(11));
                insCmd.Parameters.AddWithValue("@perm", reader.IsDBNull(12) ? DefaultProjectPermissions : reader.GetString(12));
                insCmd.Parameters.AddWithValue("@vis", reader.IsDBNull(13) ? 1 : reader.GetInt32(13));
                insCmd.Parameters.AddWithValue("@rw", reader.IsDBNull(14) ? 0 : reader.GetInt32(14));
                insCmd.Parameters.AddWithValue("@rr", reader.IsDBNull(15) ? 0 : reader.GetInt32(15));
                await insCmd.ExecuteNonQueryAsync();
            }
        }

        await tx.CommitAsync();
    }
    catch
    {
        await tx.RollbackAsync();
        throw;
    }
}

// 把主库 ProjectTreeGroups/ProjectTreeNodes/ProjectDataReferences/ProjectValidationFormulas 全量同步到
// PullProjectDirect 生成的 .db 文件，并写入正确的 Project.Version。
// 解决两个问题：
//   1. .db 文件 Project.Version 恒为 0 导致客户端每次打开都误报"数据有更新"
//   2. PushProject 只写主库不写 .db 文件，客户端打开项目看不到他人新增的 TreeNode
// 设计要点：
//   - DELETE + INSERT（非 INSERT OR REPLACE）：主库为权威源，需清理 .db 中已删除的"幽灵节点"
//   - 事务保护：所有 DELETE/INSERT 包在 projConn.BeginTransaction() 内，异常 Rollback 保留 File.Copy 的旧 .db
//   - 字段映射：主库 TreeIndex → .db Index；过滤 ProjectId（.db 无此列）
//   - 空数据保护：若主库 ProjectTreeNodes 该项目记录数为 0，跳过同步（保留模板内容）
static async Task SyncMainToProjectDbAsync(SqliteStorage db, SqliteConnection projConn, Guid projectId)
{
    var pid = projectId.ToString();

    // 1. 查询主库 Project 元数据（含 Version）
    var projectInfo = await QueryProjectForDirectAsync(db, projectId);
    if (projectInfo == null) return; // 项目不存在，上层会抛异常

    // 2. 检查主库是否有该项目 TreeNode 数据（避免误清空模板生成的节点）
    int mainNodeCount;
    using (var cntConn = db.CreateConnection())
    {
        await cntConn.OpenAsync();
        using var cntCmd = cntConn.CreateCommand();
        cntCmd.CommandText = "SELECT COUNT(*) FROM ProjectTreeNodes WHERE ProjectId = @pid";
        cntCmd.Parameters.AddWithValue("@pid", pid);
        mainNodeCount = Convert.ToInt32(await cntCmd.ExecuteScalarAsync());
    }
    if (mainNodeCount == 0)
    {
        // 主库无 TreeNode 数据（项目刚创建尚未 PushProject），仅更新 Project.Version
        using var verOnlyTx = projConn.BeginTransaction();
        try
        {
            using var verCmd = projConn.CreateCommand();
            verCmd.Transaction = verOnlyTx;
            verCmd.CommandText = "UPDATE `Project` SET Version = @ver";
            verCmd.Parameters.AddWithValue("@ver", projectInfo.Value.Version);
            await verCmd.ExecuteNonQueryAsync();
            await verOnlyTx.CommitAsync();
        }
        catch
        {
            await verOnlyTx.RollbackAsync();
            throw;
        }
        return;
    }

    // 3. 全量同步（事务保护）
    using var tx = projConn.BeginTransaction();
    try
    {
        // 3.1 同步 TreeGroup（主库 ProjectTreeGroups → .db TreeGroup，TreeIndex → Index）
        using (var delCmd = projConn.CreateCommand())
        {
            delCmd.Transaction = tx;
            delCmd.CommandText = "DELETE FROM `TreeGroup`";
            await delCmd.ExecuteNonQueryAsync();
        }
        using (var srcConn = db.CreateConnection())
        {
            await srcConn.OpenAsync();
            using var readCmd = srcConn.CreateCommand();
            readCmd.CommandText = "SELECT Id, Name, TreeIndex, ServerIndex, Status, Dirty FROM ProjectTreeGroups WHERE ProjectId = @pid";
            readCmd.Parameters.AddWithValue("@pid", pid);
            using var reader = await readCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                using var insCmd = projConn.CreateCommand();
                insCmd.Transaction = tx;
                insCmd.CommandText = @"INSERT INTO `TreeGroup`(`Id`,`Name`,`Index`,`ServerIndex`,`Status`,`Dirty`)
                    VALUES (@id,@name,@idx,@sidx,@status,@dirty)";
                insCmd.Parameters.AddWithValue("@id", reader.IsDBNull(0) ? 0 : reader.GetInt64(0));
                insCmd.Parameters.AddWithValue("@name", reader.IsDBNull(1) ? "" : reader.GetString(1));
                insCmd.Parameters.AddWithValue("@idx", reader.IsDBNull(2) ? 0 : reader.GetInt32(2));
                insCmd.Parameters.AddWithValue("@sidx", reader.IsDBNull(3) ? 0 : reader.GetInt32(3));
                insCmd.Parameters.AddWithValue("@status", reader.IsDBNull(4) ? 0 : reader.GetInt32(4));
                insCmd.Parameters.AddWithValue("@dirty", reader.IsDBNull(5) ? 0 : reader.GetInt32(5));
                await insCmd.ExecuteNonQueryAsync();
            }
        }

        // 3.2 同步 TreeNode（主库 ProjectTreeNodes → .db TreeNode，TreeIndex → Index）
        using (var delCmd = projConn.CreateCommand())
        {
            delCmd.Transaction = tx;
            delCmd.CommandText = "DELETE FROM `TreeNode`";
            await delCmd.ExecuteNonQueryAsync();
        }
        using (var srcConn = db.CreateConnection())
        {
            await srcConn.OpenAsync();
            using var readCmd = srcConn.CreateCommand();
            readCmd.CommandText = @"SELECT Id, GroupId, ParentId, Name, TreeIndex, ServerIndex, Status, Dirty,
                Type, Level, Version, Number, Permissions, Visible, RowWrite, RowRead
                FROM ProjectTreeNodes WHERE ProjectId = @pid";
            readCmd.Parameters.AddWithValue("@pid", pid);
            using var reader = await readCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                using var insCmd = projConn.CreateCommand();
                insCmd.Transaction = tx;
                insCmd.CommandText = @"INSERT INTO `TreeNode`(`Id`,`GroupId`,`ParentId`,`Name`,`Status`,`Dirty`,`Index`,
                    `ServerIndex`,`Type`,`Level`,`Version`,`Number`,`Permissions`,`Visible`,`RowWrite`,`RowRead`)
                    VALUES (@id,@gid,@parid,@name,@status,@dirty,@idx,@sidx,@type,@level,@ver,@num,@perm,@vis,@rw,@rr)";
                insCmd.Parameters.AddWithValue("@id", reader.IsDBNull(0) ? 0 : reader.GetInt64(0));
                insCmd.Parameters.AddWithValue("@gid", reader.IsDBNull(1) ? 0 : reader.GetInt64(1));
                insCmd.Parameters.AddWithValue("@parid", reader.IsDBNull(2) ? DBNull.Value : (object)reader.GetInt64(2));
                insCmd.Parameters.AddWithValue("@name", reader.IsDBNull(3) ? "" : reader.GetString(3));
                insCmd.Parameters.AddWithValue("@status", reader.IsDBNull(6) ? 0 : reader.GetInt32(6));
                insCmd.Parameters.AddWithValue("@dirty", reader.IsDBNull(7) ? 0 : reader.GetInt32(7));
                insCmd.Parameters.AddWithValue("@idx", reader.IsDBNull(4) ? 0 : reader.GetInt32(4));
                insCmd.Parameters.AddWithValue("@sidx", reader.IsDBNull(5) ? 0 : reader.GetInt32(5));
                insCmd.Parameters.AddWithValue("@type", reader.IsDBNull(8) ? 0 : reader.GetInt32(8));
                insCmd.Parameters.AddWithValue("@level", reader.IsDBNull(9) ? 0 : reader.GetInt32(9));
                insCmd.Parameters.AddWithValue("@ver", reader.IsDBNull(10) ? 0 : reader.GetInt32(10));
                insCmd.Parameters.AddWithValue("@num", reader.IsDBNull(11) ? DBNull.Value : (object)reader.GetString(11));
                insCmd.Parameters.AddWithValue("@perm", reader.IsDBNull(12) ? DefaultProjectPermissions : reader.GetString(12));
                insCmd.Parameters.AddWithValue("@vis", reader.IsDBNull(13) ? 1 : reader.GetInt32(13));
                insCmd.Parameters.AddWithValue("@rw", reader.IsDBNull(14) ? 0 : reader.GetInt32(14));
                insCmd.Parameters.AddWithValue("@rr", reader.IsDBNull(15) ? 0 : reader.GetInt32(15));
                await insCmd.ExecuteNonQueryAsync();
            }
        }

        // 3.3 同步 DataReference（主库 ProjectDataReferences → .db DataReference，1:1 字段）
        await SyncSimpleTableAsync(db, projConn, tx, pid, "ProjectDataReferences", "DataReference",
            "Id, Key, Value, Kind, Status, Dirty");

        // 3.4 同步 ValidationFormula（主库 ProjectValidationFormulas → .db ValidationFormula，1:1 字段）
        await SyncSimpleTableAsync(db, projConn, tx, pid, "ProjectValidationFormulas", "ValidationFormula",
            "Id, LeftExpr, Operator, RightExpr, Note, Status, Dirty, TableId, DocumentFieldId");

        // 3.5 写入正确的 Version 到 .db Project 表
        await WriteProjectRecordAsync(projConn, projectInfo.Value);

        await tx.CommitAsync();
    }
    catch
    {
        await tx.RollbackAsync();
        throw;
    }
}

// 简单同字段表通用同步（DELETE + INSERT，事务由调用方传入）
// mainTable 主库表名（带 ProjectId），dbTable .db 表名（无 ProjectId），cols 逗号分隔的同名字段
static async Task SyncSimpleTableAsync(SqliteStorage db, SqliteConnection projConn, SqliteTransaction tx,
    string pid, string mainTable, string dbTable, string cols)
{
    using (var delCmd = projConn.CreateCommand())
    {
        delCmd.Transaction = tx;
        delCmd.CommandText = $"DELETE FROM `{dbTable}`";
        await delCmd.ExecuteNonQueryAsync();
    }
    using var srcConn = db.CreateConnection();
    await srcConn.OpenAsync();
    using var readCmd = srcConn.CreateCommand();
    readCmd.CommandText = $"SELECT {cols} FROM {mainTable} WHERE ProjectId = @pid";
    readCmd.Parameters.AddWithValue("@pid", pid);
    using var reader = await readCmd.ExecuteReaderAsync();
    var colList = cols.Split(',').Select(c => c.Trim()).ToList();
    while (await reader.ReadAsync())
    {
        using var insCmd = projConn.CreateCommand();
        insCmd.Transaction = tx;
        var fields = string.Join(",", colList.Select(c => $"`{c}`"));
        var pars = string.Join(",", colList.Select((_, i) => $"@p{i}"));
        insCmd.CommandText = $"INSERT INTO `{dbTable}`({fields}) VALUES ({pars})";
        for (int i = 0; i < colList.Count; i++)
        {
            insCmd.Parameters.AddWithValue($"@p{i}", reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i));
        }
        await insCmd.ExecuteNonQueryAsync();
    }
}

// 写入 TreeGroup 表 1 条默认记录（与客户端本地模式创建项目时一致）
static async Task WriteDefaultTreeGroupAsync(SqliteConnection conn)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"INSERT OR REPLACE INTO `TreeGroup`(`Id`,`Name`,`Index`,`Status`,`Dirty`,`ServerIndex`)
        VALUES (@Id,@Name,@Index,@Status,@Dirty,@ServerIndex)";
    cmd.Parameters.AddWithValue("@Id", 1L);
    cmd.Parameters.AddWithValue("@Name", "工作底稿");
    cmd.Parameters.AddWithValue("@Index", 0);
    cmd.Parameters.AddWithValue("@Status", 0);
    cmd.Parameters.AddWithValue("@Dirty", 0);
    cmd.Parameters.AddWithValue("@ServerIndex", 0);
    await cmd.ExecuteNonQueryAsync();
}

// 写入默认 TreeNode 和 Document 记录（与 ProjectDbManager.CreateDefaultDocumentNode 一致）
static async Task WriteDefaultDocumentNodeAsync(SqliteConnection conn)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"INSERT INTO `TreeNode`(`Id`,`GroupId`,`ParentId`,`Name`,`Type`,`Status`,`Dirty`,`Index`,`Level`,`Version`,`ServerIndex`)
        VALUES (@Id,@GroupId,@ParentId,@Name,@Type,@Status,@Dirty,@Index,@Level,@Version,@ServerIndex)";
    cmd.Parameters.AddWithValue("@Id", 2L);
    cmd.Parameters.AddWithValue("@GroupId", 1L);
    cmd.Parameters.AddWithValue("@ParentId", DBNull.Value);
    cmd.Parameters.AddWithValue("@Name", "审计报告");
    cmd.Parameters.AddWithValue("@Type", 2);
    cmd.Parameters.AddWithValue("@Status", 0);
    cmd.Parameters.AddWithValue("@Dirty", 0);
    cmd.Parameters.AddWithValue("@Index", 0);
    cmd.Parameters.AddWithValue("@Level", 0);
    cmd.Parameters.AddWithValue("@Version", 0);
    cmd.Parameters.AddWithValue("@ServerIndex", 0);
    await cmd.ExecuteNonQueryAsync();

    cmd.Parameters.Clear();
    cmd.CommandText = @"INSERT INTO `Document`(`Id`,`Locker`,`MergeTable`,`Dirty`)
        VALUES (@Id,@Locker,@MergeTable,@Dirty)";
    cmd.Parameters.AddWithValue("@Id", 2L);
    cmd.Parameters.AddWithValue("@Locker", 0);
    cmd.Parameters.AddWithValue("@MergeTable", 0);
    cmd.Parameters.AddWithValue("@Dirty", 0);
    await cmd.ExecuteNonQueryAsync();
}

// V2-C-06 修复：校验目标 .db 中 TreeGroup/TreeNode 是否为空，若空则补写默认数据。
// CopyTemplateContentAsync 仅在模板文件不存在时写默认数据；模板 .db 存在但表为空时
// 不会补写，导致客户端打开后树形为空。本方法提供后置兜底。
static async Task EnsureDefaultTreeNodesAsync(SqliteConnection conn)
{
    long treeGroupCount;
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT COUNT(*) FROM TreeGroup";
        treeGroupCount = Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    long treeNodeCount;
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT COUNT(*) FROM TreeNode";
        treeNodeCount = Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    // 任一表为空即视为模板复制失败/为空，补写默认数据
    if (treeGroupCount == 0 || treeNodeCount == 0)
    {
        await WriteDefaultTreeGroupAsync(conn);
        await WriteDefaultDocumentNodeAsync(conn);
    }
}

// 从服务端模板数据复制内容到新项目数据库（TreeGroups/TreeNodes/DataRefs/VFs）
static async Task CopyTemplateContentAsync(ProjectDbManager dbManager, SqliteConnection projConn, Guid templateId, Guid? teamId)
{
    // 使用 ProjectDbManager.CopyTemplateContent 从模板 .db 文件逐表复制
    // 该方法会先查找团队模板，再查找系统模板，找不到则写入默认树结构
    var effectiveTeamId = teamId ?? Guid.Empty;
    await Task.Run(() => dbManager.CopyTemplateContent(effectiveTeamId, templateId, projConn));
}

// 阶段 4 Task 14.2：更新 Teams.LicenseDate = license.EndDate
// teamIdStr 为 Team 的 GUID 字符串，licenseDate 为新到期时间
static async Task UpdateTeamLicenseDateAsync(SqliteStorage db, string teamIdStr, DateTime licenseDate)
{
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "UPDATE Teams SET LicenseDate = @ld WHERE Id = @tid";
    cmd.Parameters.AddWithValue("@ld", licenseDate.ToString("yyyy-MM-dd HH:mm:ss"));
    cmd.Parameters.AddWithValue("@tid", teamIdStr);
    await cmd.ExecuteNonQueryAsync();
}

static async Task<UserDto?> QueryUserAsync(SqliteStorage db, string userName)
{
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    // 阶段 9 Task 9.4：附加 PasswordSalt、PasswordHashAlgorithm 用于密码校验
    // 修复 BUG: 此前 SQL 缺失 IsActive 与 Permissions 列，导致 ReadUser 无法映射 IsActive（登录响应 IsActive=false）
    // 列顺序必须与 QueryTeamUsersAsync (line 5681) 完全一致，因为 ReadUser 使用索引访问
    cmd.CommandText = "SELECT Id, UserName, Password, Name, Email, Phone, Role, TeamId, IsTeamAdmin, IsSystemAdmin, IsDataAdmin, IsActive, LicenseDate, PasswordSalt, PasswordHashAlgorithm, Permissions FROM Users WHERE UserName = @u";
    cmd.Parameters.AddWithValue("@u", userName);
    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;
    return ReadUser(reader);
}

static UserDto ReadUser(SqliteDataReader reader)
{
    var user = new UserDto
    {
        Id = reader.GetInt64(0),
        UserName = reader.IsDBNull(1) ? null : reader.GetString(1),
        Password = reader.IsDBNull(2) ? null : reader.GetString(2),
        Name = reader.IsDBNull(3) ? null : reader.GetString(3),
        Email = reader.IsDBNull(4) ? null : reader.GetString(4),
        Phone = reader.IsDBNull(5) ? null : reader.GetString(5),
        Role = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
        IsTeamAdmin = reader.IsDBNull(8) ? false : reader.GetInt32(8) == 1,
        IsSystemAdmin = reader.IsDBNull(9) ? false : reader.GetInt32(9) == 1,
        IsDataAdmin = reader.IsDBNull(10) ? false : reader.GetInt32(10) == 1,
        // 修复 BUG: 此前未映射 IsActive 字段，导致 QueryUserAsync/QueryTeamUsersAsync 返回的 UserDto.IsActive 始终为 false
        IsActive = reader.IsDBNull(11) ? false : reader.GetInt32(11) == 1,
        LicenseDate = reader.IsDBNull(12) ? DateTime.MaxValue : DateTime.Parse(reader.GetString(12)),
        // 阶段 9 Task 9.4：密码安全字段（从新增列读取；老库通过 EnsureColumn 已添加）
        PasswordSalt = reader.IsDBNull(13) ? null : reader.GetString(13),
        PasswordHashAlgorithm = reader.IsDBNull(14) ? 0 : Convert.ToInt32(reader.GetValue(14))
    };
    if (!reader.IsDBNull(7) && Guid.TryParse(reader.GetString(7), out var tid)) user.TeamId = tid;
    // Permissions 列（索引 15）：客户端 UserTeamPermissions.Deserialize 期望 JSON 字符串。
    // 老库可能无此列或为 NULL，统一容错（客户端也做了 null 防御）。
    if (reader.FieldCount > 15 && !reader.IsDBNull(15))
    {
        user.Permissions = reader.GetString(15);
    }
    return user;
}

static bool VerifyPassword(UserDto user, string clientInput)
{
    if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(clientInput)) return false;

    // 客户端发送密码时使用 Encrypts.SHA256Encrypt(password, isUrl: true) 进行 URL 编码，
    // 但 ASP.NET Core 会自动对 URL 查询参数进行解码，所以需要重新编码后再验证。
    var encodedClientInput = HttpUtility.UrlEncode(clientInput);

    // 阶段 9 Task 9.4：根据 PasswordHashAlgorithm 选择校验方式
    if (user.PasswordHashAlgorithm == 1 && !string.IsNullOrEmpty(user.PasswordSalt))
    {
        // 新算法：PBKDF2。encodedClientInput 是 URL 编码后的 Base64(SHA256(明文))，
        // 与数据库中存储的格式一致。admin 和普通用户使用完全相同的验证方式。
        return PasswordHasher.VerifyPassword(encodedClientInput, user.Password, user.PasswordSalt);
    }

    // Legacy 明文比对已废弃（V2-M-15 修复）：明文大小写不敏感比对在 DB 泄露后直接暴露密码。
    // 所有用户（含 admin）应通过 PBKDF2 加盐哈希存储（PasswordHashAlgorithm=1 且 PasswordSalt 非空）。
    // SeedAdmin 已使用 PBKDF2 存储 admin 密码，无需 Legacy 回退；老用户需通过重置密码升级到 PBKDF2。
    return false;
}

static async Task<List<TeamDto>> QueryUserTeamsAsync(SqliteStorage db, long userId)
{
    var list = new List<TeamDto>();
    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        SELECT t.Id, t.Name, t.Level, t.PayStatus, t.LicenseDate, t.OwnerUserId
        FROM Teams t
        INNER JOIN UserTeams ut ON ut.TeamId = t.Id
        WHERE ut.UserId = @u";
    cmd.Parameters.AddWithValue("@u", userId);
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var teamId = Guid.Parse(reader.GetString(0));
        var teamName = reader.IsDBNull(1) ? null : reader.GetString(1);
        var level = reader.IsDBNull(2) ? 3 : reader.GetInt32(2);
        var payStatus = reader.IsDBNull(3) ? 1 : reader.GetInt32(3);
        var licenseDate = reader.IsDBNull(4) ? DateTime.MaxValue : DateTime.Parse(reader.GetString(4));
        var ownerUserId = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);

        list.Add(new TeamDto
        {
            Id = teamId,
            Name = teamName,
            Level = level,
            PayStatus = payStatus,
            LicenseDate = licenseDate,
            OwnerUserId = ownerUserId
        });
    }
    return list;
}

static async Task<List<ProjectDto>> QueryProjectsAsync(SqliteStorage db)
{
    var list = new List<ProjectDto>();
    // 多租户隔离：仅返回当前租户上下文 TeamId 下的项目；无租户上下文返回空集合。
    var currentTeamId = TenantContextAccessor.Current?.TeamId;
    if (string.IsNullOrEmpty(currentTeamId)) return list;

    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Id, Number, Name, Category, Auditee, Note, ParentId, CreatorId, Version, Type, ChargeType, TeamVisible, TemplateId, CreateTime, TeamId FROM Projects WHERE TeamId = @teamId AND COALESCE(IsTemplate, 0) = 0 AND (IsDeleted = 0 OR IsDeleted IS NULL)";
    cmd.Parameters.AddWithValue("@teamId", currentTeamId);
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var creator = new UserDto { Id = reader.IsDBNull(7) ? 0 : reader.GetInt64(7), Name = "管理员", UserName = "admin" };
        list.Add(new ProjectDto
        {
            Id = Guid.Parse(reader.GetString(0)),
            Number = reader.IsDBNull(1) ? null : reader.GetString(1),
            Name = reader.IsDBNull(2) ? null : reader.GetString(2),
            Category = reader.IsDBNull(3) ? null : reader.GetString(3),
            Auditee = reader.IsDBNull(4) ? null : reader.GetString(4),
            Note = reader.IsDBNull(5) ? null : reader.GetString(5),
            ParentId = reader.IsDBNull(6) ? null : Guid.Parse(reader.GetString(6)),
            Creator = creator,
            Version = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
            Type = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
            ChargeType = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
            TeamVisible = reader.IsDBNull(11) ? false : reader.GetInt32(11) == 1,
            TemplateId = reader.IsDBNull(12) ? null : Guid.Parse(reader.GetString(12)),
            CreateTime = reader.IsDBNull(13) ? DateTime.Now : DateTime.Parse(reader.GetString(13)),
            TeamId = reader.IsDBNull(14) ? null : Guid.TryParse(reader.GetString(14), out var tid) ? tid : null,
            Users = Enumerable.Empty<UserDto>()
        });
    }
    return list;
}

static async Task<List<UserDto>> QueryTeamUsersAsync(SqliteStorage db)
{
    var list = new List<UserDto>();
    // 多租户隔离：仅返回当前租户上下文 TeamId 下的用户（通过 UserTeams 关联）；无租户上下文返回空集合。
    var currentTeamId = TenantContextAccessor.Current?.TeamId;
    if (string.IsNullOrEmpty(currentTeamId)) return list;

    using var conn = db.CreateConnection();
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT u.Id, u.UserName, u.Password, u.Name, u.Email, u.Phone, u.Role, u.TeamId, u.IsTeamAdmin, u.IsSystemAdmin, u.IsDataAdmin, u.IsActive, u.LicenseDate, u.PasswordSalt, u.PasswordHashAlgorithm, u.Permissions FROM Users u INNER JOIN UserTeams ut ON ut.UserId = u.Id WHERE ut.TeamId = @teamId LIMIT 100";
    cmd.Parameters.AddWithValue("@teamId", currentTeamId);
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        list.Add(ReadUser(reader));
    }
    return list;
}

// ============= 阶段 8 Task 8.4：SignalR Hub 路由注册 =============
// 客户端 SignalRClient.cs 中 HubConnection(hubAddress) 直传 AppServer URL，
// 默认拼接 /signalr/ 端点（v2 协议）；ASP.NET Core SignalR 使用 MapHub 注册的精确路径。
// Hub 名称 "ChatHub" 与客户端 _hc.CreateHubProxy("ChatHub") 一致。
app.MapHub<ChatHub>("/ChatHub");

// ============= Task 4：审批流程 SignalR 通知接线 =============
// ReviewService.WorkflowEvent / ArchiveService.WorkflowEvent（均为 Singleton）→ ChatHub 推送：
// - ReviewSubmitted/ReviewAdvanced/ReviewApproved/ReviewRejected → payload.targetUserIds 逐用户推送（多端登录均收到）
// - ProjectArchived → payload.teamId 团队全体成员广播
// 客户端按事件名订阅，单参数 payload（匿名对象 JSON，camelCase）。
var workflowHubContext = app.Services.GetRequiredService<IHubContext<ChatHub>>();
var workflowTeamSvc = app.Services.GetRequiredService<TeamService>();
var workflowLogger = app.Logger;

app.Services.GetRequiredService<ReviewService>().WorkflowEvent += (eventName, payload) =>
    _ = PushWorkflowEventAsync(workflowHubContext, workflowTeamSvc, workflowLogger, eventName, payload);
app.Services.GetRequiredService<ArchiveService>().WorkflowEvent += (eventName, payload) =>
    _ = PushWorkflowEventAsync(workflowHubContext, workflowTeamSvc, workflowLogger, eventName, payload);

// Task 4 辅助：按事件名路由推送（ProjectArchived → teamId 团队广播；其余 → targetUserIds 逐用户推送）
static async Task PushWorkflowEventAsync(
    IHubContext<ChatHub> hubContext, TeamService teamSvc, ILogger logger, string eventName, object payload)
{
    try
    {
        var json = JObject.FromObject(payload);
        if (eventName == "ProjectArchived")
        {
            var teamId = json["teamId"]?.ToObject<Guid>() ?? Guid.Empty;
            if (teamId == Guid.Empty)
            {
                logger.LogWarning("ProjectArchived 事件缺少 teamId，无法团队广播");
                return;
            }
            // 团队广播：不依赖客户端当前激活的 team_ 组，按团队成员 userId 逐人推送（含多端登录）
            foreach (var member in await teamSvc.GetTeamUsersAsync(teamId))
                await SendWorkflowEventToUserAsync(hubContext, member.Id, eventName, payload);
            return;
        }

        var targetUserIds = json["targetUserIds"]?.ToObject<long[]>() ?? Array.Empty<long>();
        foreach (var userId in targetUserIds.Distinct())
            await SendWorkflowEventToUserAsync(hubContext, userId, eventName, payload);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "审批流程事件推送失败: {EventName}", eventName);
    }
}

// Task 4 辅助：向指定用户全部在线连接推送事件（多端登录均收到；离线用户自然跳过）
static async Task SendWorkflowEventToUserAsync(
    IHubContext<ChatHub> hubContext, long userId, string eventName, object payload)
{
    foreach (var connId in ChatHub.GetConnectionIdsForUser(userId.ToString(CultureInfo.InvariantCulture)))
        await hubContext.Clients.Client(connId).SendAsync(eventName, payload);
}

// ===== 上报审核与归档（Review/Archive）=====
// 规格：.trae/specs/review-archive-workflow/spec.md（数据模型 / API 设计 / 权限矩阵）
// 说明：所有端点从 Header 解析当前用户（HeaderParser.ParseUserId），TeamId 优先取
// TenantContextAccessor（TenantIsolationFilter 中间件按 UserTeams 解析），失败回退
// TeamService.GetCurrentUserTeamIdAsync。错误映射：InvalidOperationException→400，
// UnauthorizedAccessException→403。SignalR 通知钩子由 Task 4 接入（ReviewService.WorkflowEvent）。

// POST /api/Review/Submit
// Body: { projectId, reportType, reportNo, opinionType, note, reviewers: [{ level, userId }], templateId? }
// templateId 可选（review-flow-template）：提供时按模板展开审批节点，
// reviewers 仅作为"发起人自选"级别的补充；未提供时 reviewers 即完整审批链（原行为）。
// 成功返回新审核单（含 Nodes）；Projects.ReviewStatus=1
app.MapPost("/api/Review/Submit", async (HttpContext ctx, ReviewService svc, TeamService teamSvc, ReviewFlowService flowSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var pidStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (!Guid.TryParse(pidStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");

    var reviewers = new List<ReviewerInput>();
    var reviewersToken = data["reviewers"] ?? data["Reviewers"];
    if (reviewersToken is JArray arr)
    {
        foreach (var item in arr)
        {
            reviewers.Add(new ReviewerInput
            {
                Level = item.Value<int?>("level") ?? item.Value<int?>("Level") ?? 0,
                UserId = item.Value<long?>("userId") ?? item.Value<long?>("UserId") ?? 0
            });
        }
    }

    // 可选模板：提供时按模板展开审批链（覆盖 reviewers），并留痕 TemplateId
    string? templateId = (data["templateId"] ?? data["TemplateId"])?.ToString();
    string? usedTemplateId = null;
    if (!string.IsNullOrWhiteSpace(templateId))
    {
        try
        {
            var (expanded, template) = await flowSvc.ExpandTemplateAsync(teamId.Value, templateId, reviewers);
            reviewers = expanded;
            usedTemplateId = template.Id;
        }
        catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
        catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
    }

    try
    {
        var submission = await svc.SubmitAsync(
            userId, teamId.Value, projectId,
            data.Value<int?>("reportType") ?? data.Value<int?>("ReportType") ?? 0,
            (string?)(data["reportNo"] ?? data["ReportNo"]),
            data.Value<int?>("opinionType") ?? data.Value<int?>("OpinionType") ?? 0,
            (string?)(data["note"] ?? data["Note"]),
            reviewers, usedTemplateId);
        return ApiResponseHelper.Ok(submission);
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
    catch (ArgumentException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// GET /api/Review/GetSubmissions?scope=pending|mine|all&projectId={可选}
// pending=待我审批（当前节点 ReviewerId=我 且 Status=0）；mine=我提交的；
// all=TeamAdmin 全部（普通成员为自己提交的或参与项目的）
app.MapGet("/api/Review/GetSubmissions", async (HttpContext ctx, string? scope, Guid? projectId, ReviewService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    scope = (scope ?? "pending").Trim().ToLowerInvariant();
    if (scope is not ("pending" or "mine" or "all"))
        return ApiResponseHelper.Error("无效的 scope，须为 pending/mine/all");

    try
    {
        var isTeamAdmin = await svc.IsTeamAdminAsync(userId, teamId.Value);
        var list = await svc.GetSubmissionsAsync(teamId.Value, scope, userId, isTeamAdmin, projectId);
        return ApiResponseHelper.Ok(list);
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// POST /api/Review/Approve
// Body: { submissionId, comment }；当前节点通过；最后节点通过 → ReviewStatus=2
// 返回 { finished: true/false } 表示是否最终通过
app.MapPost("/api/Review/Approve", async (HttpContext ctx, ReviewService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var submissionId = (data["submissionId"] ?? data["SubmissionId"])?.ToString();
    if (string.IsNullOrWhiteSpace(submissionId))
        return ApiResponseHelper.Error("无效的 submissionId");
    var comment = (string?)(data["comment"] ?? data["Comment"]);

    try
    {
        var finished = await svc.ApproveAsync(userId, submissionId, comment);
        return ApiResponseHelper.Ok(new { finished });
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// POST /api/Review/Reject
// Body: { submissionId, comment }；当前节点退回 → Submission.Status=2、ReviewStatus=3
app.MapPost("/api/Review/Reject", async (HttpContext ctx, ReviewService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var submissionId = (data["submissionId"] ?? data["SubmissionId"])?.ToString();
    if (string.IsNullOrWhiteSpace(submissionId))
        return ApiResponseHelper.Error("无效的 submissionId");
    var comment = (string?)(data["comment"] ?? data["Comment"]);

    try
    {
        await svc.RejectAsync(userId, submissionId, comment);
        return ApiResponseHelper.Ok();
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// POST /api/Review/Withdraw
// Body: { submissionId }；仅提交人且所有节点均未审批 → Submission.Status=3、ReviewStatus=0
app.MapPost("/api/Review/Withdraw", async (HttpContext ctx, ReviewService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var submissionId = (data["submissionId"] ?? data["SubmissionId"])?.ToString();
    if (string.IsNullOrWhiteSpace(submissionId))
        return ApiResponseHelper.Error("无效的 submissionId");

    try
    {
        await svc.WithdrawAsync(userId, submissionId);
        return ApiResponseHelper.Ok();
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// GET /api/Review/GetHistory?projectId={guid}
// 返回该项目全部轮次审核单（每轮含 Nodes 节点明细），供审批记录查看
app.MapGet("/api/Review/GetHistory", async (HttpContext ctx, Guid projectId, ReviewService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    try
    {
        var list = await svc.GetHistoryAsync(projectId, teamId.Value);
        return ApiResponseHelper.Ok(list);
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// ===== 审批流程模板（review-flow-template）=====
// 管理员预置审批链；上报时按模板展开审批节点。错误映射同 Review 端点。

// GET /api/ReviewFlow/GetTemplates?includeDisabled=1
// 返回当前团队模板列表（含节点明细）。includeDisabled=1/true 供管理界面查看停用项（TeamAdmin 校验在端点内）。
// 注：用 string 接收后手工解析，兼容 1/true（minimal API 的 bool? 绑定不接受 "1"，会 400）。
app.MapGet("/api/ReviewFlow/GetTemplates", async (HttpContext ctx, string? includeDisabled, ReviewFlowService svc, ReviewService reviewSvc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    try
    {
        var requested = string.Equals(includeDisabled, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(includeDisabled, "true", StringComparison.OrdinalIgnoreCase);
        var include = requested && await reviewSvc.IsTeamAdminAsync(userId, teamId.Value);
        var list = await svc.GetTemplatesAsync(teamId.Value, include);
        return ApiResponseHelper.Ok(list);
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// POST /api/ReviewFlow/SaveTemplate（仅 TeamAdmin）
// Body: { id?, name, totalLevel, enabled, nodes: [{ level, assigneeType, reviewerId }] }
// id 为空新建，否则更新；校验失败 400，非管理员 403。返回保存后的模板（含节点）。
app.MapPost("/api/ReviewFlow/SaveTemplate", async (HttpContext ctx, ReviewFlowService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var template = new ReviewFlowTemplateDto
    {
        Id = (string?)(data["id"] ?? data["Id"]) ?? "",
        Name = (string?)(data["name"] ?? data["Name"]) ?? "",
        TotalLevel = data.Value<int?>("totalLevel") ?? data.Value<int?>("TotalLevel") ?? 0,
        Enabled = data.Value<int?>("enabled") ?? data.Value<int?>("Enabled") ?? 1
    };
    if ((data["nodes"] ?? data["Nodes"]) is JArray nodesArr)
    {
        foreach (var item in nodesArr)
        {
            template.Nodes.Add(new ReviewFlowNodeDto
            {
                Level = item.Value<int?>("level") ?? item.Value<int?>("Level") ?? 0,
                AssigneeType = item.Value<int?>("assigneeType") ?? item.Value<int?>("AssigneeType") ?? 0,
                ReviewerId = item.Value<long?>("reviewerId") ?? item.Value<long?>("ReviewerId") ?? 0
            });
        }
    }

    try
    {
        var saved = await svc.SaveAsync(userId, teamId.Value, template);
        return ApiResponseHelper.Ok(saved);
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// POST /api/ReviewFlow/DeleteTemplate（仅 TeamAdmin）
// Body: { id }
app.MapPost("/api/ReviewFlow/DeleteTemplate", async (HttpContext ctx, ReviewFlowService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var id = (data["id"] ?? data["Id"])?.ToString();
    if (string.IsNullOrWhiteSpace(id))
        return ApiResponseHelper.Error("无效的模板 id");

    try
    {
        await svc.DeleteAsync(userId, teamId.Value, id);
        return ApiResponseHelper.Ok();
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// POST /api/ReviewFlow/SetTemplateEnabled（仅 TeamAdmin）
// Body: { id, enabled }；仅翻转启用状态，不做节点校验——
// 指定审批人离职导致模板不合法时，管理员仍可停用（全量保存校验会拒绝该场景）。
app.MapPost("/api/ReviewFlow/SetTemplateEnabled", async (HttpContext ctx, ReviewFlowService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var id = (data["id"] ?? data["Id"])?.ToString();
    if (string.IsNullOrWhiteSpace(id))
        return ApiResponseHelper.Error("无效的模板 id");
    var enabled = data.Value<int?>("enabled") ?? data.Value<int?>("Enabled") ?? -1;

    try
    {
        await svc.SetEnabledAsync(userId, teamId.Value, id, enabled);
        return ApiResponseHelper.Ok();
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// POST /api/Archive/Submit
// Body: { projectId, note }；校验 ReviewStatus=2 且未归档；操作人须为 TeamAdmin 或项目创建人
// 成功返回归档记录（含 ArchiveNo = AR-yyyy-NNNN）；置 IsArchived=1（归档锁定）
app.MapPost("/api/Archive/Submit", async (HttpContext ctx, ArchiveService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var pidStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (!Guid.TryParse(pidStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");
    var note = (string?)(data["note"] ?? data["Note"]);

    try
    {
        var archive = await svc.ArchiveAsync(userId, teamId.Value, projectId, note);
        return ApiResponseHelper.Ok(archive);
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// GET /api/Archive/GetCandidates
// 返回可归档候选项目列表（ReviewStatus=2 且 IsArchived=0 且未删除）
app.MapGet("/api/Archive/GetCandidates", async (HttpContext ctx, ArchiveService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    return ApiResponseHelper.Ok(await svc.GetCandidatesAsync(teamId.Value));
});

// GET /api/Archive/GetArchives
// 返回本团队归档记录列表（含项目名称/编号、操作人快照）
app.MapGet("/api/Archive/GetArchives", async (HttpContext ctx, ArchiveService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    return ApiResponseHelper.Ok(await svc.GetArchivesAsync(teamId.Value));
});

// POST /api/Archive/Cancel
// Body: { projectId }；仅 TeamAdmin；置 IsArchived=0（ReviewStatus 保留 2 可重新归档），归档记录保留
app.MapPost("/api/Archive/Cancel", async (HttpContext ctx, ArchiveService svc, TeamService teamSvc) =>
{
    var userId = HeaderParser.ParseUserId(ctx);
    if (userId == 0) return ApiResponseHelper.Unauthorized();
    var teamId = await ResolveCurrentTeamIdAsync(userId, teamSvc);
    if (teamId == null) return ApiResponseHelper.Error("无法解析当前团队");

    var body = await ReadBodyAsync(ctx);
    JObject data;
    try { data = JObject.Parse(body); }
    catch { return ApiResponseHelper.Error("请求体无效"); }

    var pidStr = (data["projectId"] ?? data["ProjectId"])?.ToString();
    if (!Guid.TryParse(pidStr, out var projectId))
        return ApiResponseHelper.Error("无效的 projectId");

    try
    {
        await svc.CancelAsync(userId, teamId.Value, projectId);
        return ApiResponseHelper.Ok();
    }
    catch (UnauthorizedAccessException ex) { return ApiResponseHelper.Forbidden(ex.Message); }
    catch (InvalidOperationException ex) { return ApiResponseHelper.Error(ex.Message); }
});

// 解析当前请求用户所属团队 Id：优先 TenantContextAccessor（TenantIsolationFilter 按 UserTeams 解析），
// 失败回退 TeamService.GetCurrentUserTeamIdAsync（Users.TeamId）。
static async Task<Guid?> ResolveCurrentTeamIdAsync(long userId, TeamService teamSvc)
{
    var ctxTeamId = TenantContextAccessor.Current?.TeamId;
    if (!string.IsNullOrEmpty(ctxTeamId) && Guid.TryParse(ctxTeamId, out var tid)) return tid;
    var fallback = await teamSvc.GetCurrentUserTeamIdAsync(userId);
    return fallback == Guid.Empty ? null : fallback;
}
// ===== 上报审核与归档（Review/Archive）结束 =====

// 阶段 9 Task 9.2：404 兜底中间件（在所有 MapXxx 之后，未匹配路由返回统一 NotFound 响应）
// 注意：不能用 app.Run(handler)，那是终端中间件会阻止 EndpointMiddleware 自动添加；
// 改用 app.Use：先调用 next() 让端点路由匹配，未匹配（StatusCode==404 且未开始写入）时再返回 JSON。
app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        ctx.Response.ContentType = "application/json; charset=utf-8";
        var payload = new { error = "NotFound", code = 404, message = $"未找到路径: {ctx.Request.Path}" };
        var json = JsonConvert.SerializeObject(payload, ApiResponseHelper.Settings());
        await ctx.Response.WriteAsync(json);
    }
});

app.Run();

public partial class Program { }

// V2-H-12 修复：文件上传扩展名白名单与 MIME 映射（必须在类型声明中，因 top-level 语句中不允许 static readonly 字段）。
// 覆盖常见图片/文档/表格/压缩包类型；可执行文件（.exe/.sh/.aspx 等）一律拒绝。
public static class FileUploadWhitelist
{
    public static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp",
        ".pdf", ".xlsx", ".xls", ".docx", ".doc", ".pptx", ".ppt",
        ".zip", ".rar", ".7z", ".tar", ".gz",
        ".txt", ".csv", ".json", ".xml",
        ".mp4", ".mp3", ".wav",
        ".bin"
    };

    public static readonly Dictionary<string, string> ExtensionToContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".bmp"] = "image/bmp",
        [".webp"] = "image/webp",
        [".pdf"] = "application/pdf",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".xls"] = "application/vnd.ms-excel",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".doc"] = "application/msword",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".ppt"] = "application/vnd.ms-powerpoint",
        [".zip"] = "application/zip",
        [".rar"] = "application/vnd.rar",
        [".7z"] = "application/x-7z-compressed",
        [".tar"] = "application/x-tar",
        [".gz"] = "application/gzip",
        [".txt"] = "text/plain",
        [".csv"] = "text/csv",
        [".json"] = "application/json",
        [".xml"] = "application/xml",
        [".mp4"] = "video/mp4",
        [".mp3"] = "audio/mpeg",
        [".wav"] = "audio/wav",
        [".bin"] = "application/octet-stream"
    };
}

// 系统模板只读访问放行的端点白名单（读路径）。
// 系统下发模板（TeamId=NULL）对已登录用户开放只读查看；写路径不在名单内，仍要求系统管理员。
// 新增只读端点时在此追加，勿在调用点逐个传参。
static class SystemTemplateReadonlyEndpoints
{
    public static readonly System.Collections.Generic.HashSet<string> Paths = new()
    {
        "/api/Project/OpenProject", "/api/Project/GetProjectDto", "/api/Project/GetProjectDescendants",
        "/api/Project/GetProjectUsersWithPic", "/api/Project/PullProject", "/api/Project/PullProjectDirect",
        "/api/Project/DownloadPullProjectDirect", "/api/Project/PullTable", "/api/Project/PullDocument",
        "/api/Project/PullImage", "/api/Project/PullPdf", "/api/Project/DownloadFile",
        "/api/Project/GetTableColumns", "/api/Project/GetTableTimeline", "/api/Project/QueryTableVersions",
        "/api/Project/GetTableRevertDiff", "/api/Project/GetDocumentTimeline", "/api/Project/QueryDocumentVersions",
        "/api/Project/GetDocumentRevertDiff", "/api/Project/QueryImageVersions", "/api/Project/QueryPdfVersions"
    };
}
