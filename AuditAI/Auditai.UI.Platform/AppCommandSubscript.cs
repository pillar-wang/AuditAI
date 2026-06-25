using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandSubscript : AppCommandToggleButton
{
	public override Image SmallIcon => Resources.Subscript;

	protected override void Pressed()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.Subscript();
		}
	}

	protected override void Unpressed()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.Subscript(false);
		}
	}
}
