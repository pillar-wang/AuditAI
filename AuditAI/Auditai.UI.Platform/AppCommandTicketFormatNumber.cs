using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormatNumber : AppCommandButton
{
	public override string Text => "1234.56";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatNumeric(DataFormatType.Number);
	}
}
