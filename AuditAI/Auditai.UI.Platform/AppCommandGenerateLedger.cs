using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandGenerateLedger : AppCommandButton
{
	public override string Text => "账套生成器";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.OpenExcelLedger;

	protected override string Tooltip => TipResource.打开序时账按钮;

	protected override void Clicked()
	{
		Program.MainForm.GenerateLedger();
	}

	public override void OnAppStateChanged(AppState state)
	{
		Visible = !(Program.MainForm.CurrentEdition is AppEditionGeneral);
		base.OnAppStateChanged(state);
	}
}
