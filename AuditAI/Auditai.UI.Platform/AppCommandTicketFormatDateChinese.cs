using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormatDateChinese : AppCommandButton
{
	public override string Text => "2020年12月31日";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.DateChinese);
	}
}
