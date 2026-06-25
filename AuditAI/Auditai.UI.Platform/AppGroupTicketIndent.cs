using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupTicketIndent : AppCommandGroup
{
	public override string Text => "缩进";

	public override Image Image => Resources.UnindentCell;

	public AppGroupTicketIndent()
	{
		base.Commands.Add(AppCommands.TicketUnindent);
		base.Commands.Add(AppCommands.TicketIndent);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table;
	}
}
