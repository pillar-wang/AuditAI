using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandZeroFormatEmpty : AppCommandButton
{
	public override string Text => "显示为空值";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetZeroFormat(ZeroFormat.Empty);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetZeroFormat(ZeroFormat.Empty);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetZeroFormat(ZeroFormat.Empty);
			break;
		}
	}
}
