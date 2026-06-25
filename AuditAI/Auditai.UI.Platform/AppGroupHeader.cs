using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupHeader : AppCommandGroup
{
	public override string Text => "页眉页脚";

	public override Image Image => Resources.Portrait;

	public AppGroupHeader()
	{
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[3]
		{
			AppCommands.HeaderLeft,
			AppCommands.HeaderCenter,
			AppCommands.HeaderRight
		}));
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[3]
		{
			AppCommands.FooterLeft,
			AppCommands.FooterCenter,
			AppCommands.FooterRight
		}));
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
	}
}
