using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandLaunchCollector : AppCommandButton
{
	public override string Text => "采集数据";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.Crawl;

	protected override string Tooltip => TipResource.采集数据按钮;

	protected override void Clicked()
	{
		Program.LaunchCrawler();
	}
}
