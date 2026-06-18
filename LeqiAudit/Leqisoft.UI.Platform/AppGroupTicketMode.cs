using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupTicketMode : AppCommandGroup
{
	public override string Text => "设计表单";

	public override Image Image => Resources.TicketReport16;

	public AppGroupTicketMode()
	{
		base.Commands.Add(AppCommands.TicketMode);
		base.Commands.Add(AppCommands.TicketReport);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!SoftwareLicenseManager.IsAllowDesignTicket())
		{
			base.Visible = false;
		}
		else
		{
			base.Visible = state.ViewKind == MainFormView.Table;
		}
	}
}
