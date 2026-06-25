namespace Auditai.UI.Platform;

public class AppCommandTicketFormatDate : AppCommandMenu
{
	public override string Text => "日期格式";

	public AppCommandTicketFormatDate()
		: base(new AppCommandBase[9]
		{
			AppCommands.TicketFormatDateChinese,
			AppCommands.TicketFormatDateDash,
			AppCommands.TicketFormatDateSlash,
			AppCommands.TicketFormatDateDot,
			new AppCommandSeparator(),
			AppCommands.TicketFormatDateYearMonthChinese,
			AppCommands.TicketFormatDateYearMonthDash,
			AppCommands.TicketFormatDateYearMonthSlash,
			AppCommands.TicketFormatDateYearMonthDot
		})
	{
	}
}
