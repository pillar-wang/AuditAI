using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandOpenLedger : AppCommandButton
{
	public override string Text => "打开账套";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.OpenLedger;

	protected override string Tooltip => TipResource.打开账套按钮;

	protected override void Clicked()
	{
		Program.MainForm.OpendLedger_Click();
	}
}
