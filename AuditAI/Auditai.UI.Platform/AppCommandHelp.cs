using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandHelp : AppCommandButton
{
	public override string Text => "帮助中心";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.HelpCenter;

	protected override string Tooltip => TipResource.帮助;

	protected override void Clicked()
	{
		Program.MainForm.ShowHelpCenter();
	}

	public override void OnAppStateChanged(AppState state)
	{
		Visible = SoftwareLicenseManager.IsShowHelpDocumentButton();
	}
}
