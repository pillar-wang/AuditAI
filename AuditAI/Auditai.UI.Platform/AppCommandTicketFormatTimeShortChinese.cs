using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormatTimeShortChinese : AppCommandButton
{
	public override string Text => "10时20分";

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetFormatDefault(DataFormatType.TimeShortChinese);
	}
}
