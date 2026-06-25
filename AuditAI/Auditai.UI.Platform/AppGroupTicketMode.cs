using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
