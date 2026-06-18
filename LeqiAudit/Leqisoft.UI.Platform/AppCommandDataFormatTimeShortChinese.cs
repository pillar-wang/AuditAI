using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatTimeShortChinese : AppCommandButton
{
	public override string Text => "10时20分";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatTime(DataFormatType.TimeShortChinese);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatTime(DataFormatType.TimeShortChinese);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatTime(DataFormatType.TimeShortChinese);
			break;
		}
	}
}
