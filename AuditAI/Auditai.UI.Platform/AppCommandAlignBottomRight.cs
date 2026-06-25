using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandAlignBottomRight : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignBottomRight;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
			Program.MainForm.TableEditor.RibbonBottomRightClicked();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonBottomRightClicked();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetAlign(CellTextAlign.BottomRight);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetAlign(CellTextAlign.BottomRight);
			break;
		}
	}
}
