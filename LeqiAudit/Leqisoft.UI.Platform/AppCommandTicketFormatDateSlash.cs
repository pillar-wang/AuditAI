using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketFormatDateSlash : AppCommandButton
{
	public override string Text => "2020/12/31";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.DateSlash);
	}
}
