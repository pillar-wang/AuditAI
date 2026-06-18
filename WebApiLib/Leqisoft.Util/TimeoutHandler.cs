using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leqisoft.Util;

public class TimeoutHandler : DelegatingHandler
{
	public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(100.0);


	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		using CancellationTokenSource cts = GetCancellationTokenSource(request, cancellationToken);
		try
		{
			return await base.SendAsync(request, cts?.Token ?? cancellationToken);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			TimeoutException ex = new TimeoutException();
			throw new HttpRequestException(ex.Message, ex);
		}
	}

	private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		TimeSpan timeSpan = GetTimeout(request) ?? DefaultTimeout;
		if (timeSpan == Timeout.InfiniteTimeSpan)
		{
			return null;
		}
		CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		cancellationTokenSource.CancelAfter(timeSpan);
		return cancellationTokenSource;
	}

	private static TimeSpan? GetTimeout(HttpRequestMessage request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (request.Properties.TryGetValue("RequestTimeout", out var value) && value is TimeSpan)
		{
			return (TimeSpan)value;
		}
		return null;
	}

	private static bool GetProgressCallbackId(HttpRequestMessage request, out Guid? id)
	{
		id = null;
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (request.Properties.TryGetValue("ProgressCallbackId", out var value) && value is Guid value2)
		{
			id = value2;
			return true;
		}
		return false;
	}

	private static CancellationTokenSource GetProgressUpdateCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(180.0));
		return cancellationTokenSource;
	}
}
