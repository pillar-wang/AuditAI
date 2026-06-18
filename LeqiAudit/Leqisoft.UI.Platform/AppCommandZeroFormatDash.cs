using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandZeroFormatDash : AppCommandButton
{
	public override string Text => "显示为－值";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetZeroFormat(ZeroFormat.Dash);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetZeroFormat(ZeroFormat.Dash);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetZeroFormat(ZeroFormat.Dash);
			break;
		}
	}
}
