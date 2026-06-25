namespace Auditai.UI.Platform;

public class AppCommandDataFormatNumeric : AppCommandMenu
{
	public override string Text => "数值格式";

	public AppCommandDataFormatNumeric()
		: base(new AppCommandBase[5]
		{
			AppCommands.DataFormatNumber,
			AppCommands.DataFormatComma,
			AppCommands.DataFormatDollar,
			AppCommands.DataFormatRmb,
			AppCommands.DataFormatPercent
		})
	{
	}
}
