using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Newtonsoft.Json;

namespace CrawlerForm;

public class Updater
{
	private const string BASEURI = "";

	private readonly WebClient _client;

	private Action<List<CrawlerModuleInfo>> _continuation;

	private readonly Dictionary<CrawlerModuleInfo, Action<CrawlerModuleInfo>> _continuations = new Dictionary<CrawlerModuleInfo, Action<CrawlerModuleInfo>>();

	public Updater()
	{
		_client = new WebClient();
		_client.BaseAddress = "";
	}

	public void BeginGetCrawlerList(Action<List<CrawlerModuleInfo>> continuation)
	{
		// 本地模式不从远程下载爬虫模块
		_continuation = continuation;
		_continuation(new List<CrawlerModuleInfo>());
	}

	private void _client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
	{
		_client.DownloadStringCompleted -= _client_DownloadStringCompleted;
		if (e.Error == null)
		{
			UpdaterInfo updaterInfo = JsonConvert.DeserializeObject<UpdaterInfo>(e.Result);
			_continuation(updaterInfo.Crawlers);
		}
		else
		{
			_continuation(new List<CrawlerModuleInfo>());
		}
	}

	public void BeginDownloadCrawler(CrawlerModuleInfo info, string filename, Action<CrawlerModuleInfo> continuation)
	{
		_client.DownloadFileCompleted += _client_DownloadFileCompleted;
		_continuations.Add(info, continuation);
		_client.DownloadFileAsync(new Uri(info.Name + ".dll", UriKind.Relative), filename, info);
	}

	private void _client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
	{
		_client.DownloadFileCompleted -= _client_DownloadFileCompleted;
		CrawlerModuleInfo crawlerModuleInfo = (CrawlerModuleInfo)e.UserState;
		_continuations[crawlerModuleInfo](crawlerModuleInfo);
	}
}
