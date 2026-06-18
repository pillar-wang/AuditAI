using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAlignMiddleLeft : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignMiddleLeft;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
			Program.MainForm.TableEditor.RibbonMiddleLeftClicked();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonMiddleLeftClicked();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetAlign(CellTextAlign.MiddleLeft);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetAlign(CellTextAlign.MiddleLeft);
			break;
		}
	}
}
