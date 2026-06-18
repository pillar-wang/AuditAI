namespace Leqisoft.UI.Platform;

public class AppGroupTicketColumnMove : AppCommandGroup
{
	public override string Text => "列操作";

	public AppGroupTicketColumnMove()
	{
		base.Commands.Add(AppCommands.MoveLeftTicketColumn);
		base.Commands.Add(AppCommands.MoveRightTicketColumn);
	}
}
