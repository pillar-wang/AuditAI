using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketFormatTimeShort : AppCommandButton
{
	public override string Text => "10:20";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.TimeShort);
	}
}
