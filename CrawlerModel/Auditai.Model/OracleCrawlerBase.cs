namespace Auditai.Model;

public abstract class OracleCrawlerBase : CrawlerBase
{
	public sealed override LSDb.DbProvider DbProvider => LSDb.DbProvider.Oracle;
}
