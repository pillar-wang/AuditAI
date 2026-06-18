using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowParagraphMarkers : AppCommandToggleButton
{
	public override string Text => "段落标记";

	public override Image LargeIcon => Resources.ToggleMarker;

	protected override void Pressed()
	{
		Program.MainForm.CurrentDocumentEditor.ShowMarks();
	}

	protected override void Unpressed()
	{
		Program.MainForm.CurrentDocumentEditor.HideMarks();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
		RibbonItem.Group.Items[RibbonItem.Group.Items.IndexOf(RibbonItem) - 1].Visible = Visible;
	}
}
