using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupTicketLock : AppCommandGroup
{
	public override string Text => "单据权限保护";

	public override Image Image => Resources.ToggleLockTable;

	public AppGroupTicketLock()
	{
		base.Commands.Add(AppCommands.RowOwnerExclusive);
		base.Commands.Add(AppCommands.RowOwnerLoad);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
	}
}
