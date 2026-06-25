using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandSystemSettings : AppCommandButton
{
	public override string Text => "系统设置";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.Settings;

	protected override string Tooltip => TipResource.系统设置;

	protected override void Clicked()
	{
		Program.MainForm.ShowSettings();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = SoftwareLicenseManager.IsAllowShowSystemSettingMenu();
	}
}
