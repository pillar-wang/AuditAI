using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatBoolRightWrong : AppCommandButton
{
	public override string Text => "对/错";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatBoolean(DataFormatType.BoolRightWrong);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatBoolean(DataFormatType.BoolRightWrong);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatBoolean(DataFormatType.BoolRightWrong);
			break;
		}
	}
}
