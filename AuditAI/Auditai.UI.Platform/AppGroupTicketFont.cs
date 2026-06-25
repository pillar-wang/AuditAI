namespace Auditai.UI.Platform;

public class AppGroupTicketFont : AppCommandGroup
{
	public override string Text => "字体样式";

	public AppGroupTicketFont()
	{
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.TicketFont }));
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[1] { AppCommands.TicketFontSize }));
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.TicketForeColor);
		base.Commands.Add(AppCommands.TicketBold);
		base.Commands.Add(AppCommands.TicketGrowFont);
		base.Commands.Add(AppCommands.TicketBackColor);
		base.Commands.Add(AppCommands.TicketItalic);
		base.Commands.Add(AppCommands.TicketShrinkFont);
	}
}
