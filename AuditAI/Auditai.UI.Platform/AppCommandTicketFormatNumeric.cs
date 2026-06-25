namespace Auditai.UI.Platform;

public class AppCommandTicketFormatNumeric : AppCommandMenu
{
	public override string Text => "数值格式";

	public AppCommandTicketFormatNumeric()
		: base(new AppCommandBase[5]
		{
			AppCommands.TicketFormatNumber,
			AppCommands.TicketFormatComma,
			AppCommands.TicketFormatDollar,
			AppCommands.TicketFormatRmb,
			AppCommands.TicketFormatPercent
		})
	{
	}
}
