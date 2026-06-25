namespace Auditai.UI.Platform;

public class AppTabDocument : AppCommandTab
{
	public override string Text => "文档编辑";

	public AppTabDocument()
	{
		base.Groups.Add(AppCommandGroups.DocumentLock);
		base.Groups.Add(AppCommandGroups.Clipboard);
		base.Groups.Add(AppCommandGroups.DocumentCharFormat);
		base.Groups.Add(AppCommandGroups.ParagraphFormat);
		base.Groups.Add(AppCommandGroups.Insert);
		base.Groups.Add(AppCommandGroups.DocFind);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.EditingNote || state.ViewKind == MainFormView.DocFormatBrush;
	}
}
