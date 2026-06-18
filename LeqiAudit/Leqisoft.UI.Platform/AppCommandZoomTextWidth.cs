using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandZoomTextWidth : AppCommandButton
{
	public override string Text => "行宽";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetZoomFactor((int)ZoomOption.TextWidth);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
