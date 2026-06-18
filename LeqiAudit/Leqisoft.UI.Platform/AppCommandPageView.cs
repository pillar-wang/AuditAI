using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
