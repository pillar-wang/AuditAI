using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormatDateYearMonthDash : AppCommandButton
{
	public override string Text => "2020-12";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.DateYearMonthDash);
	}
}
