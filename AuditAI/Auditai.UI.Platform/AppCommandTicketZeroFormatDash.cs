using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketZeroFormatDash : AppCommandButton
{
	public override string Text => "显示为－值";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetZeroFormat(ZeroFormat.Dash);
	}
}
