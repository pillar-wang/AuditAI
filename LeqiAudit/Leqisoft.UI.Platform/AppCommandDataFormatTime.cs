namespace Leqisoft.UI.Platform;

public class AppCommandDataFormatTime : AppCommandMenu
{
	public override string Text => "时间格式";

	public AppCommandDataFormatTime()
		: base(new AppCommandBase[4]
		{
			AppCommands.DataFormatTimeLong,
			AppCommands.DataFormatTimeLongChinese,
			AppCommands.DataFormatTimeShort,
			AppCommands.DataFormatTimeShortChinese
		})
	{
	}
}
