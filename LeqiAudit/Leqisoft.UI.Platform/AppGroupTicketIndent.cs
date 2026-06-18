using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
