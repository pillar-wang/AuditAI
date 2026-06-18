using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatRmb : AppCommandButton
{
	public override string Text => "￥1,234.56";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatNumeric(DataFormatType.NumRmb);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatNumeric(DataFormatType.NumRmb);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatNumeric(DataFormatType.NumRmb);
			break;
		}
	}
}
