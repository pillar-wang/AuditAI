using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupMakeLedger : AppCommandGroup
{
	public override string Text => "数据采集";

	public override Image Image => Resources.Crawl;

	public AppGroupMakeLedger()
	{
		base.Commands.Add(AppCommands.MakeUsbCollector);
		base.Commands.Add(AppCommands.LaunchCollector);
		base.Commands.Add(AppCommands.GenerateLedger);
	}
}
