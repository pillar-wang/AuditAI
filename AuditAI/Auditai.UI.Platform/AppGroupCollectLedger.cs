using System.Drawing;
using Auditai.PlatformResource;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupCollectLedger : AppCommandGroup
{
	public override string Text => "自财务数据中填充数据";

	public override Image Image => Resources.TableCollect;

	public AppGroupCollectLedger()
	{
		base.Commands.Add(AppCommands.CollectByColumn);
		base.Commands.Add(AppCommands.CollectByCell);
		base.Commands.Add(AppCommands.ExecuteCollect);
		base.Commands.Add(AppCommands.OneClickCollect);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (SoftwareLicenseManager.IsLedgerModuleEnable() && Program.ClientPlatformType == PlatformType.AuditPlatform)
		{
			base.Visible = state.ViewKind == MainFormView.Table;
		}
		else
		{
			base.Visible = false;
		}
	}
}
