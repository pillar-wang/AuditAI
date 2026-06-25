using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandLessPrecision : AppCommandButton
{
	public override string Text => "减小数位";

	public override Image LargeIcon => Resources.LessPrecision;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.LessPrecision();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.LessPrecision();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.LessPrecision();
			break;
		}
	}
}
