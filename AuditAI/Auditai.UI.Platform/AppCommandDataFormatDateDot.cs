using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatDateDot : AppCommandButton
{
	public override string Text => "2020.12.31";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatDate(DataFormatType.DateDot);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatDate(DataFormatType.DateDot);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatDate(DataFormatType.DateDot);
			break;
		}
	}
}
