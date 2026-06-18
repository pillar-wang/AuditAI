namespace Leqisoft.UI.Platform;

public class AppCommandTicketFormatBoolean : AppCommandMenu
{
	public override string Text => "判断格式";

	public AppCommandTicketFormatBoolean()
		: base(new AppCommandBase[2]
		{
			AppCommands.TicketFormatBoolCheckBox,
			AppCommands.TicketFormatBoolOnOff
		})
	{
	}
}
