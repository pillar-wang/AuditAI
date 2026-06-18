using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketFormatTimeLongChinese : AppCommandButton
{
	public override string Text => "10时20分30秒";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.TimeLongChinese);
	}
}
