namespace Auditai.UI.Platform;

public class AppTabView : AppCommandTab
{
	public override string Text => "显示设置";

	public AppTabView()
	{
		base.Groups.Add(AppCommandGroups.TableStyle);
		base.Groups.Add(AppCommandGroups.DisplayOptions);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.Pdf;
	}
}
