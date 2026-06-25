using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandOpenLedger : AppCommandButton
{
	public override string Text => "打开账套";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.OpenLedger;

	protected override string Tooltip => TipResource.打开账套按钮;

	protected override void Clicked()
	{
		Program.MainForm.OpendLedger_Click();
	}
}
