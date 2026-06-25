using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandFormatBrush : AppCommandToggleButton
{
	public override string Text => "格式刷";

	public override Image LargeIcon => Resources.FormatPainter;

	protected override void Pressed()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.BeginFormatBrush();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.BeginFormatPainter();
			break;
		}
	}

	protected override void Unpressed()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.FormatBrush:
			Program.MainForm.TableEditor.EndFormatBrush();
			break;
		case MainFormView.DocFormatBrush:
			Program.MainForm.CurrentDocumentEditor.EndFormatPainter();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind != MainFormView.EditingNote;
	}
}
