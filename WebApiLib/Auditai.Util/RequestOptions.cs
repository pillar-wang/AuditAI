using System;
using System.Net.Http;
using System.Threading;

namespace Auditai.Util;

public class RequestOptions
{
	public HttpMethod Method { get; set; } = HttpMethod.Post;


	public string Url { get; set; }

	/// <summary>请求超时，默认 100 秒（与 TimeoutHandler 兜底一致）；长耗时操作应显式设置更大的值</summary>
	public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100.0);


	public bool WithAuthorization { get; set; }

	public bool IsUpdateToken { get; set; }

	public object Body { get; set; }

	public bool WithMachineCode { get; set; }

	public bool WithMachineSign { get; set; }

	public string ValidationCode { get; set; }

	/// <summary>激活码，注册时通过 Header 传递给服务端</summary>
	public string ActivationCode { get; set; }

	public int OutFileLength { get; set; }

	public Guid? FileId { get; set; }
}
