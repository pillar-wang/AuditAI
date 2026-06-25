using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatBoolCheckBox : AppCommandButton
{
	public override string Text => "复选框";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatBoolean(DataFormatType.BoolCheckBox);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatBoolean(DataFormatType.BoolCheckBox);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatBoolean(DataFormatType.BoolCheckBox);
			break;
		}
	}
}
