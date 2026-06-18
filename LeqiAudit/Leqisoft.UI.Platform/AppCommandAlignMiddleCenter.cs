using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAlignMiddleCenter : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignMiddleCenter;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
			Program.MainForm.TableEditor.RibbonMiddleCenterClicked();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonMiddleCenterClicked();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetAlign(CellTextAlign.MiddleCenter);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetAlign(CellTextAlign.MiddleCenter);
			break;
		}
	}
}
