namespace Leqisoft.UI.Platform;

public class AppTabMembers : AppCommandTab
{
	public override string Text => "成员管理";

	public AppTabMembers()
	{
		base.Groups.Add(AppCommandGroups.UserManage);
		base.Groups.Add(AppCommandGroups.MemberManage);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.Visible = state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.Ledger || state.ViewKind == MainFormView.TicketInput;
	}
}
