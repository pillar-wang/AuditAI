using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatDateYearMonthSlash : AppCommandButton
{
	public override string Text => "2020/12";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatDateYearMonth(DataFormatType.DateYearMonthSlash);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatDateYearMonth(DataFormatType.DateYearMonthSlash);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatDateYearMonth(DataFormatType.DateYearMonthSlash);
			break;
		}
	}
}
