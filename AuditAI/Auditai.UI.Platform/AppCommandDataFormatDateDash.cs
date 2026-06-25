using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatDateDash : AppCommandButton
{
	public override string Text => "2020-12-31";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatDate(DataFormatType.DateDash);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatDate(DataFormatType.DateDash);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatDate(DataFormatType.DateDash);
			break;
		}
	}
}
