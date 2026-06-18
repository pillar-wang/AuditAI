namespace Leqisoft.UI.Platform;

public class AppGroupTicketRow : AppCommandGroup
{
	public override string Text => "行操作";

	public AppGroupTicketRow()
	{
		base.Commands.Add(AppCommands.MoveUpTicketRow);
		base.Commands.Add(AppCommands.MoveDownTicketRow);
		base.Commands.Add(AppCommands.TicketRowHeightIncrease);
		base.Commands.Add(AppCommands.TicketRowHeightDecrease);
	}
}
