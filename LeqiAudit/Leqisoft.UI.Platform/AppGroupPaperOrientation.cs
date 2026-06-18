using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupPaperOrientation : AppCommandGroup
{
	public override string Text => "页面设置";

	public override Image Image => Resources.Portrait;

	public AppGroupPaperOrientation()
	{
		base.Commands.Add(AppCommands.Portrait);
		base.Commands.Add(AppCommands.Landscape);
		base.Commands.Add(AppCommands.PageColumns);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
	}
}
