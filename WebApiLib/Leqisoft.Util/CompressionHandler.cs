using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Leqisoft.Util;

public class CompressionHandler : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		HttpContent content = request.Content;
		if (content != null)
		{
			request.Content = new PushStreamContent(async delegate(Stream s, HttpContent h, TransportContext t)
			{
				using GZipStream comp = new GZipStream(s, CompressionMode.Compress);
				await content.CopyToAsync(comp);
			});
		}
		HttpResponseMessage resp = await base.SendAsync(request, cancellationToken);
		HttpContent content2 = resp.Content;
		if (content2.Headers.ContentLength != 0)
		{
			GZipStream content3 = new GZipStream(await content2.ReadAsStreamAsync(), CompressionMode.Decompress);
			resp.Content = new StreamContent(content3);
		}
		return resp;
	}
}
