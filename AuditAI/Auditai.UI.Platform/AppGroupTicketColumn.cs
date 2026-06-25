namespace Auditai.UI.Platform;

public class AppGroupTicketColumn : AppCommandGroup
{
	public override string Text => "列操作";

	public AppGroupTicketColumn()
	{
		base.Commands.Add(AppCommands.TicketColumnWidthIncrease);
		base.Commands.Add(AppCommands.TicketColumnWidthDecrease);
	}
}
