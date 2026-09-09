namespace AuditApiServer.Infra;

/// <summary>
/// 通用 HTTP Header 解析器。
/// 客户端 WebApiClient 在请求 Header 中携带 UserId、Token、MachineCode、ValidateCode、FileId 等字段，
/// 大小写可能不一致，这里统一兼容。
/// </summary>
public static class HeaderParser
{
    public static long ParseUserId(HttpContext ctx)
    {
        var headers = ctx.Request.Headers;
        if (long.TryParse(headers["UserId"].ToString(), out var uid)) return uid;
        if (long.TryParse(headers["userid"].ToString(), out uid)) return uid;
        return 0;
    }

    public static string? ParseToken(HttpContext ctx)
    {
        var headers = ctx.Request.Headers;
        return headers["Token"].ToString() ?? headers["token"].ToString();
    }

    public static string? ParseMachineCode(HttpContext ctx)
    {
        return ctx.Request.Headers["MachineCode"].ToString();
    }

    public static string? ParseValidateCode(HttpContext ctx)
    {
        return ctx.Request.Headers["ValidateCode"].ToString();
    }

    public static Guid? ParseFileId(HttpContext ctx)
    {
        var v = ctx.Request.Headers["FileId"].ToString();
        return Guid.TryParse(v, out var fid) ? fid : null;
    }

    public static string? ParseMachineSign(HttpContext ctx)
    {
        return ctx.Request.Headers["cookie_machine_sign"].ToString();
    }
}
