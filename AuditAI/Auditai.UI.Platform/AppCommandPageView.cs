using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandPageView : AppCommandButton
{
	public override string Text => "页面模式";

	public override Image LargeIcon => Resources.PageView;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.PageMode();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
