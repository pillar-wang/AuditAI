namespace Auditai.UI.Platform;

public class AppGroupTicketBorder : AppCommandGroup
{
	public override string Text => "边框样式";

	public AppGroupTicketBorder()
	{
		base.Commands.Add(AppCommands.TicketBorderTop);
		base.Commands.Add(AppCommands.TicketBorderBottom);
		base.Commands.Add(AppCommands.TicketBorderLeft);
		base.Commands.Add(AppCommands.TicketBorderRight);
		base.Commands.Add(AppCommands.TicketBorderNone);
		base.Commands.Add(AppCommands.TicketBorderAll);
		base.Commands.Add(AppCommands.TicketBorder1);
		base.Commands.Add(AppCommands.TicketBorder2);
	}
}
