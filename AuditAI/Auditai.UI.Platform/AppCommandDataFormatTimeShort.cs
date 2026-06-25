using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatTimeShort : AppCommandButton
{
	public override string Text => "10:20";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatTime(DataFormatType.TimeShort);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatTime(DataFormatType.TimeShort);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatTime(DataFormatType.TimeShort);
			break;
		}
	}
}
