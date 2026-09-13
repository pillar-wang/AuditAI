using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupLedgerWindow : AppCommandGroup
{
	public override string Text => "账务数据";

	public override Image Image => IconRes.LedgerWindow;

	public AppGroupLedgerWindow()
	{
		base.Commands.Add(AppCommands.LedgerWindow);
	}
}