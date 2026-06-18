using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatTimeLongChinese : AppCommandButton
{
	public override string Text => "10时20分30秒";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatTime(DataFormatType.TimeLongChinese);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatTime(DataFormatType.TimeLongChinese);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatTime(DataFormatType.TimeLongChinese);
			break;
		}
	}
}
