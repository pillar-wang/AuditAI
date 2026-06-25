using System;
using System.Net.Http;

namespace Auditai.Util;

public static class HttpRequestMessageExtensions
{
	public const string TimeoutPropertyKey = "RequestTimeout";

	public const string ProgressCallbackId = "ProgressCallbackId";

	public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		request.Properties["RequestTimeout"] = timeout;
	}

	public static void SetProgressCallbackId(this HttpRequestMessage request, Guid? id)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (id.HasValue)
		{
			request.Properties["ProgressCallbackId"] = id.Value;
		}
	}
}
