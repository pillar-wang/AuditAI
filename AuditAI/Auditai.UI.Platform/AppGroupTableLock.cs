using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupTableLock : AppCommandGroup
{
	public override string Text => "表格权限保护";

	public override Image Image => Resources.ToggleLockTable;

	public AppGroupTableLock()
	{
		base.Commands.Add(AppCommands.RowOwnerExclusive);
		base.Commands.Add(AppCommands.RowOwnerLoad);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table;
	}
}
