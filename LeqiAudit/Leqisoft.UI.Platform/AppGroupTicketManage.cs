namespace Leqisoft.UI.Platform;

public class AppGroupTicketManage : AppCommandGroup
{
	public override string Text => "设计表单";

	public AppGroupTicketManage()
	{
		base.Commands.Add(AppCommands.TicketDesign);
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
			base.Visible = state.ViewKind == MainFormView.TicketInput;
		}
	}
}
