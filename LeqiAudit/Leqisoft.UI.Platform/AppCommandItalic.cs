using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandItalic : AppCommandToggleButton
{
	public override Image SmallIcon => Resources.Italic;

	protected override void Pressed()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
		case MainFormView.EditingTitle:
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.SetItalic(v: true);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.SetItalic(italic: true);
			break;
		}
	}

	protected override void Unpressed()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
		case MainFormView.EditingTitle:
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.SetItalic(v: false);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.SetItalic(italic: false);
			break;
		}
	}
}
