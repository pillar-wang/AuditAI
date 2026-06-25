using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupPaper : AppCommandGroup
{
	public override string Text => "纸张设置";

	public override Image Image => Resources.A4;

	public AppGroupPaper()
	{
		base.Commands.Add(AppCommands.Paper);
		base.Commands.Add(AppCommands.PaperDirection);
		base.Commands.Add(AppCommands.PageColumns);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
	}
}
