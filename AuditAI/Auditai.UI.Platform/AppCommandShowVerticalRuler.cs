using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowVerticalRuler : AppCommandToggleButton
{
	public override string Text => "纵向标尺";

	public override Image LargeIcon => Resources.ToggleVertRuler;

	protected override void Pressed()
	{
		Program.MainForm.CurrentDocumentEditor.ShowVerticalRuler();
	}

	protected override void Unpressed()
	{
		Program.MainForm.CurrentDocumentEditor.HideVerticalRuler();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
