﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Leqisoft.Util;

public static class BugReportClient
{
	private const string baseAddress = "http://leqisoft.com:10001/BugReport/ReportBug/";

	private static HttpClient _hc = new HttpClient
	{
		BaseAddress = new Uri("http://leqisoft.com:10001/BugReport/ReportBug/"),
		Timeout = TimeSpan.FromMinutes(1.0)
	};

	public static Task ReportBug(string stackTrace)
	{
		return Task.FromResult(0);
	}
}
