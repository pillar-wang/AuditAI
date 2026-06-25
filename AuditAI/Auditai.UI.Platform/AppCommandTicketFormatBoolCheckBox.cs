using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormatBoolCheckBox : AppCommandButton
{
	public override string Text => "复选框";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.BoolCheckBox);
	}
}
