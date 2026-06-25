namespace Auditai.UI.Platform;

public class AppCommandDataFormatBoolean : AppCommandMenu
{
	public override string Text => "判断格式";

	public AppCommandDataFormatBoolean()
		: base(new AppCommandBase[2]
		{
			AppCommands.DataFormatBoolCheckBox,
			AppCommands.DataFormatBoolOnOff
		})
	{
	}
}
