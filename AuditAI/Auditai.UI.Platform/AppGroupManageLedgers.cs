using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
