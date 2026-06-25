using System;

namespace Auditai.Model;

public class CrawlerScanProgressEventArgs : EventArgs
{
	public string ModuleName { get; set; }

	public CrawlerScanProgressEventArgs(string moduleName)
	{
		ModuleName = moduleName;
	}
}
