using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatBoolOnOff : AppCommandButton
{
	public override string Text => "开关钮";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatBoolean(DataFormatType.BoolOnOff);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatBoolean(DataFormatType.BoolOnOff);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatBoolean(DataFormatType.BoolOnOff);
			break;
		}
	}
}
