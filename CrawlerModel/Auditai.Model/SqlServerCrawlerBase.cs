namespace Auditai.Model;

public abstract class SqlServerCrawlerBase : CrawlerBase
{
	public sealed override LSDb.DbProvider DbProvider => LSDb.DbProvider.SqlServer;
}
