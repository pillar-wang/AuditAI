using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandLaunchCollector : AppCommandButton
{
	public override string Text => "采集数据";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.Crawl;

	protected override string Tooltip => TipResource.采集数据按钮;

	protected override void Clicked()
	{
		Program.LaunchCrawler();
	}
}
