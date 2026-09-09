using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace AuditApiServer.Infra;

/// <summary>
/// 统一 HTTP 响应工厂。
/// 使用 Newtonsoft.Json + DefaultNamingStrategy（PascalCase）与客户端 WebApiClient 反序列化行为保持一致。
/// Tuple&lt;T1,T2&gt; 等结构序列化为 {Item1, Item2}，由 Newtonsoft 默认行为决定。
/// </summary>
public static class ApiResponseHelper
{
    public static JsonSerializerSettings Settings() => new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver { NamingStrategy = new DefaultNamingStrategy() },
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.Local
    };

    public static IResult JsonNet(object? value, int? statusCode = null)
    {
        var json = JsonConvert.SerializeObject(value, Settings());
        return Results.Text(json, "application/json", Encoding.UTF8, statusCode);
    }

    public static IResult Ok(object? value = null) => JsonNet(value ?? new { Result = "ok" });

    public static IResult Error(string message, int statusCode = 400) =>
        JsonNet(new { error = message }, statusCode);

    public static IResult Unauthorized(string message = "未授权") =>
        JsonNet(new { error = message }, 401);

    public static IResult NotFound(string message = "未找到") =>
        JsonNet(new { error = message }, 404);

    public static IResult Forbidden(string message = "禁止访问") =>
        JsonNet(new { error = message }, 403);
}
