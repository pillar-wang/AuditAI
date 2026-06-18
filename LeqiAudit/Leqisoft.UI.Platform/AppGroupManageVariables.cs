using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
