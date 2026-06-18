using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandZoomWholePage : AppCommandButton
{
	public override string Text => "整页";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetZoomFactor((int)ZoomOption.WholePage);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
