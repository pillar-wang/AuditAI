using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandLedgerWindow : AppCommandButton
{
	public override string Text => "账务数据";

	public override Image LargeIcon => IconRes.LedgerWindow;

	protected override void Clicked()
	{
		Program.MainForm.ShowLedgerWindow();
	}
}