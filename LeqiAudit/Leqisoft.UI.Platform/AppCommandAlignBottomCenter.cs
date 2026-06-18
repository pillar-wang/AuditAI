using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAlignBottomCenter : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.AlignBottomCenter;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
			Program.MainForm.TableEditor.RibbonBottomCenterClicked();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonBottomCenterClicked();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetAlign(CellTextAlign.BottomCenter);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetAlign(CellTextAlign.BottomCenter);
			break;
		}
	}
}
