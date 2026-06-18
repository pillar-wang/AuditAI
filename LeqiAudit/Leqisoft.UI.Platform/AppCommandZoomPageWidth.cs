using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandZoomPageWidth : AppCommandButton
{
	public override string Text => "页宽";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetZoomFactor((int)ZoomOption.PageWidth);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
