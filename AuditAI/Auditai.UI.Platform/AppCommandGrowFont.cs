using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandGrowFont : AppCommandButton
{
	public override Image SmallIcon => Resources.GrowFont;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
		case MainFormView.EditingTitle:
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.GrowFont();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.GrowFont();
			break;
		}
	}
}
