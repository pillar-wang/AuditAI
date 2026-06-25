using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppTabProjects : AppCommandTab
{
	public override string Text => StringConstBase.Current.Project + "管理";

	public AppTabProjects()
	{
		base.Groups.Add(AppCommandGroups.Projects);
		base.Groups.Add(AppCommandGroups.RecentProjects);
		base.Groups.Add(AppCommandGroups.RecentTemplates);
		base.Groups.Add(AppCommandGroups.ManageVariables);
		base.Groups.Add(AppCommandGroups.System);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.Ledger || state.ViewKind == MainFormView.TicketInput;
	}
}
