using Auditai.Model;

namespace CrawlerForm;

internal class ProgressInfo
{
	private int _current;

	public int Max => Crawler.MaxProgress + 1;

	public int Current
	{
		get
		{
			return _current;
		}
		set
		{
			_current = value;
			Failed = false;
		}
	}

	public CrawlerBase Crawler { get; set; }

	public LedgerInfo LedgerInfo { get; set; }

	public string FilePath { get; set; }

	public bool Failed { get; set; } = false;

}
