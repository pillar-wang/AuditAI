using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupPrintMisc : AppCommandGroup
{
	public override string Text => "其他设置";

	public override Image Image => Resources.OtherSetup;

	public AppGroupPrintMisc()
	{
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[2]
		{
			AppCommands.StartPage,
			AppCommands.FootBorder
		}));
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[2]
		{
			AppCommands.FixedColumns,
			AppCommands.Monochrome
		}));
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
	}
}
