using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatBoolYesNo : AppCommandButton
{
	public override string Text => "是/否";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatBoolean(DataFormatType.BoolYesNo);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatBoolean(DataFormatType.BoolYesNo);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatBoolean(DataFormatType.BoolYesNo);
			break;
		}
	}
}
