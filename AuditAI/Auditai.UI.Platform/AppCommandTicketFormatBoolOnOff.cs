using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormatBoolOnOff : AppCommandButton
{
	public override string Text => "开关钮";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.BoolOnOff);
	}
}
