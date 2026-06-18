using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShrinkFont : AppCommandButton
{
	public override Image SmallIcon => Resources.ShrinkFont;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.EditingColHeader:
		case MainFormView.EditingTitle:
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.ShrinkFont();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.ShrinkFont();
			break;
		}
	}
}
