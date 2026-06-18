using System;
using System.Net.Http;
using System.Threading;

namespace Leqisoft.Util;

public class RequestOptions
{
	public HttpMethod Method { get; set; } = HttpMethod.Post;


	public string Url { get; set; }

	public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;


	public bool WithAuthorization { get; set; }

	public bool IsUpdateToken { get; set; }

	public object Body { get; set; }

	public bool WithMachineCode { get; set; }

	public bool WithMachineSign { get; set; }

	public string ValidationCode { get; set; }

	public int OutFileLength { get; set; }

	public Guid? FileId { get; set; }
}
