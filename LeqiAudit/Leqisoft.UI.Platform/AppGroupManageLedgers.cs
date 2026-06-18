using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupManageLedgers : AppCommandGroup
{
	public override string Text => "账套管理";

	public override Image Image => Resources.OpenLedger;

	public AppGroupManageLedgers()
	{
		base.Commands.Add(AppCommands.OpenLedger);
		base.Commands.Add(AppCommands.MergeLedgers);
	}
}
