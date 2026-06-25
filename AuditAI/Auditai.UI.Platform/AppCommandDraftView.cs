using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandDraftView : AppCommandButton
{
	public override string Text => "草稿模式";

	public override Image LargeIcon => Resources.DraftView;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.DraftMode = !Program.MainForm.CurrentDocumentEditor.DraftMode;
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
