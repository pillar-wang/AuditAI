using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandBold : AppCommandToggleButton
{
	public override Image SmallIcon => Resources.Bold;

	protected override void Pressed()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
		case MainFormView.EditingTitle:
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.SetBold(v: true);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.SetBold(bold: true);
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
			Program.MainForm.TableEditor.SetBold(v: false);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.SetBold(bold: false);
			break;
		}
	}
}
