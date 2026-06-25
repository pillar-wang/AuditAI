namespace Auditai.UI.Platform;

public class AppCommandDataFormatDate : AppCommandMenu
{
	public override string Text => "日期格式";

	public AppCommandDataFormatDate()
		: base(new AppCommandBase[9]
		{
			AppCommands.DataFormatDateChinese,
			AppCommands.DataFormatDateDash,
			AppCommands.DataFormatDateSlash,
			AppCommands.DataFormatDateDot,
			new AppCommandSeparator(),
			AppCommands.DataFormatDateYearMonthChinese,
			AppCommands.DataFormatDateYearMonthDash,
			AppCommands.DataFormatDateYearMonthSlash,
			AppCommands.DataFormatDateYearMonthDot
		})
	{
	}
}
