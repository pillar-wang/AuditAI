using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMorePrecision : AppCommandButton
{
	public override string Text => "增小数位";

	public override Image LargeIcon => Resources.MorePrecision;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.MorePrecision();
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.MorePrecision();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.MorePrecision();
			break;
		}
	}
}
