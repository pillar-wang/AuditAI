using System.Drawing;
using Leqisoft.PlatformResource;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupFillFromLedger : AppCommandGroup
{
	public override string Text => "智能填充";

	public override Image Image => Resources.batchFill;

	public AppGroupFillFromLedger()
	{
		base.Commands.Add(AppCommands.FillToTable);
		base.Commands.Add(AppCommands.LedgerOneClickCollect);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = Program.ClientPlatformType == PlatformType.AuditPlatform;
	}
}
