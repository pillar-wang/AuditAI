using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAlignTopCenter : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignTopCenter;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
			Program.MainForm.TableEditor.RibbonTopCenterClicked();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonTopCenterClicked();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetAlign(CellTextAlign.TopCenter);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetAlign(CellTextAlign.TopCenter);
			break;
		}
	}
}
