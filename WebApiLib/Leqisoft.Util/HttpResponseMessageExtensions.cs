using System.Net.Http;

namespace Leqisoft.Util;

public static class HttpResponseMessageExtensions
{
	public static long? GetContentLength(this HttpResponseMessage resp)
	{
		return resp.Content.Headers.ContentLength;
	}
}
