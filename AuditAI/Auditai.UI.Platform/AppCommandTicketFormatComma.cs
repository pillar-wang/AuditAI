using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormatComma : AppCommandButton
{
	public override string Text => "1,234.56";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatNumeric(DataFormatType.Comma);
	}
}
