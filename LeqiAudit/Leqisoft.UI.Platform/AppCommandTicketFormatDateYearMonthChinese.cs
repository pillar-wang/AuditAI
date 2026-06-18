using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketFormatDateYearMonthChinese : AppCommandButton
{
	public override string Text => "2020年12月";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.DateYearMonthChinese);
	}
}
