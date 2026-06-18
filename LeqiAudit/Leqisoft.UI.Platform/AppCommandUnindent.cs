using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandUnindent : AppCommandButton
{
	public override string Text => "左缩进";

	public override Image LargeIcon => Resources.UnindentCell;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.Unindent();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.Unindent();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.Unindent();
			break;
		}
	}
}
