using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketZeroFormatZero : AppCommandButton
{
	public override string Text => "显示为零值";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetZeroFormat(ZeroFormat.Zero);
	}
}
