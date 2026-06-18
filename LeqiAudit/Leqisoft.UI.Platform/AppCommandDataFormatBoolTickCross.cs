using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatBoolTickCross : AppCommandButton
{
	public override string Text => "√/×";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatBoolean(DataFormatType.BoolTickCross);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatBoolean(DataFormatType.BoolTickCross);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatBoolean(DataFormatType.BoolTickCross);
			break;
		}
	}
}
