namespace Auditai.UI.Platform;

public class AppCommandTicketFormatTime : AppCommandMenu
{
	public override string Text => "时间格式";

	public AppCommandTicketFormatTime()
		: base(new AppCommandBase[4]
		{
			AppCommands.TicketFormatTimeLong,
			AppCommands.TicketFormatTimeLongChinese,
			AppCommands.TicketFormatTimeShort,
			AppCommands.TicketFormatTimeShortChinese
		})
	{
	}
}
