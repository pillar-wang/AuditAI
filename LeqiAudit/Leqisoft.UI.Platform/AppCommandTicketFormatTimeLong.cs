using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketFormatTimeLong : AppCommandButton
{
	public override string Text => "10:20:30";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.TimeLong);
	}
}
