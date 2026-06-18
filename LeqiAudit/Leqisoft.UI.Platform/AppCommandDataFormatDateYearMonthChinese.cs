using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatDateYearMonthChinese : AppCommandButton
{
	public override string Text => "2020年12月";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatDateYearMonth(DataFormatType.DateYearMonthChinese);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatDateYearMonth(DataFormatType.DateYearMonthChinese);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatDateYearMonth(DataFormatType.DateYearMonthChinese);
			break;
		}
	}
}
