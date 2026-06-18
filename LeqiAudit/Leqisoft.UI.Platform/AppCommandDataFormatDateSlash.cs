using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatDateSlash : AppCommandButton
{
	public override string Text => "2020/12/31";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatDate(DataFormatType.DateSlash);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatDate(DataFormatType.DateSlash);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatDate(DataFormatType.DateSlash);
			break;
		}
	}
}
