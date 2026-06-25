using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupHeaderMargin : AppCommandGroup
{
	public override string Text => "页眉页脚边距";

	public override Image Image => Resources.HFMargin;

	public AppGroupHeaderMargin()
	{
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.HeaderMargin }));
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.FooterMargin }));
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
	}
}
