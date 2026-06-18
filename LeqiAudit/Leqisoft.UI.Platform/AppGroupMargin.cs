using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupMargin : AppCommandGroup
{
	public override string Text => "页面边距";

	public override Image Image => Resources.PageMargin;

	public AppGroupMargin()
	{
		base.Commands.Add(new AppCommandLabel(string.Empty));
		base.Commands.Add(AppCommands.MarginLeft);
		base.Commands.Add(new AppCommandLabel(string.Empty));
		base.Commands.Add(AppCommands.MarginTop);
		base.Commands.Add(new AppCommandLabel(string.Empty));
		base.Commands.Add(AppCommands.MarginBottom);
		base.Commands.Add(new AppCommandLabel(string.Empty));
		base.Commands.Add(AppCommands.MarginRight);
		base.Commands.Add(new AppCommandLabel(string.Empty));
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
	}
}
