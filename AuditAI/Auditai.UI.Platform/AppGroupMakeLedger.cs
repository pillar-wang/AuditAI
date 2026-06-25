using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
