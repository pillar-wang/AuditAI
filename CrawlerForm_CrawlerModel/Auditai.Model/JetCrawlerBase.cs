namespace Auditai.Model;

public abstract class JetCrawlerBase : CrawlerBase
{
	public sealed override LSDb.DbProvider DbProvider => LSDb.DbProvider.Jet;

	public abstract string ScanPath { get; }

	public abstract string ScanFilePattern { get; }
}
