using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAlignTopLeft : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignTopLeft;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
			Program.MainForm.TableEditor.RibbonTopLeftClicked();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonTopLeftClicked();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetAlign(CellTextAlign.TopLeft);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetAlign(CellTextAlign.TopLeft);
			break;
		}
	}
}
