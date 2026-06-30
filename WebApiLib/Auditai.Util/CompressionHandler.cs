using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Auditai.Util;

public class CompressionHandler : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// 请求方向：不压缩请求体（ASP.NET Core minimal API 默认不支持 GZip 请求解压，
		// 压缩后服务端 JObject.Parse 会失败）

		HttpResponseMessage resp = await base.SendAsync(request, cancellationToken);

		// 响应方向：只在服务端返回 Content-Encoding: gzip 时解压
		// 这样无论服务端是否启用 ResponseCompression 都能正确处理
		HttpContent content2 = resp.Content;
		if (content2 != null
			&& content2.Headers.ContentEncoding != null
			&& content2.Headers.ContentEncoding.Any(e => string.Equals(e, "gzip", StringComparison.OrdinalIgnoreCase)))
		{
			GZipStream content3 = new GZipStream(await content2.ReadAsStreamAsync(), CompressionMode.Decompress);
			resp.Content = new StreamContent(content3);
			// 清除 Content-Encoding 标记，避免后续重复解压
			resp.Content.Headers.ContentEncoding.Clear();
		}
		return resp;
	}
}
