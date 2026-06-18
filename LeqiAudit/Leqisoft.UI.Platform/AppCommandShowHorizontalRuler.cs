using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowHorizontalRuler : AppCommandToggleButton
{
	public override string Text => "横向标尺";

	public override Image LargeIcon => Resources.ToggleHorzRuler;

	protected override void Pressed()
	{
		Program.MainForm.CurrentDocumentEditor.ShowHorizontalRuler();
	}

	protected override void Unpressed()
	{
		Program.MainForm.CurrentDocumentEditor.HideHorizontalRuler();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
