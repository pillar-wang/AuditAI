using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandZeroFormatZero : AppCommandButton
{
	public override string Text => "显示为零值";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetZeroFormat(ZeroFormat.Zero);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetZeroFormat(ZeroFormat.Zero);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetZeroFormat(ZeroFormat.Zero);
			break;
		}
	}
}
