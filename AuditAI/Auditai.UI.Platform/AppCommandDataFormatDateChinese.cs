using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatDateChinese : AppCommandButton
{
	public override string Text => "2020年12月31日";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatDate(DataFormatType.DateChinese);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatDate(DataFormatType.DateChinese);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatDate(DataFormatType.DateChinese);
			break;
		}
	}
}
