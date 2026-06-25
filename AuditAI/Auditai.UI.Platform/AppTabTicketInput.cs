namespace Auditai.UI.Platform;

public class AppTabTicketInput : AppCommandTab
{
	public override string Text => "单据编辑";

	public AppTabTicketInput()
	{
		base.Groups.Add(AppCommandGroups.TicketManage);
		base.Groups.Add(AppCommandGroups.TicketLock);
		base.Groups.Add(AppCommandGroups.TicketRow);
		base.Groups.Add(AppCommandGroups.TicketColumn);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.TicketInput;
	}

	protected override void Selected()
	{
		base.Selected();
		if (Program.MainForm.TicketInputEditor != null)
		{
			Program.MainForm.TicketInputEditor.RefreshTicketLockShowStatus();
		}
	}
}
