using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatNumber : AppCommandButton
{
	public override string Text => "1234.56";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatNumeric(DataFormatType.Number);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatNumeric(DataFormatType.Number);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatNumeric(DataFormatType.Number);
			break;
		}
	}
}
