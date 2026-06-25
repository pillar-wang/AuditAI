using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupManageVariables : AppCommandGroup
{
	public override string Text => "变量管理";

	public override Image Image => Resources.ReferenceManager;

	public AppGroupManageVariables()
	{
		base.Commands.Add(AppCommands.ManageVariables);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = SoftwareLicenseManager.IsAllowShowVariableManageMenu();
	}
}
