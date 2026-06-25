using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandIndent : AppCommandButton
{
	public override string Text => "右缩进";

	public override Image LargeIcon => Resources.IndentCell;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.Indent();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.Indent();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.Indent();
			break;
		}
	}
}
