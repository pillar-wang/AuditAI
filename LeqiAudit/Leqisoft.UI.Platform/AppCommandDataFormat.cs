using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandDataFormat : AppCommandMenu
{
	public override string Text => "数据格式";

	public override Image LargeImage => Resources.DataFormat;

	public AppCommandDataFormat()
		: base(new AppCommandBase[5]
		{
			AppCommands.DataFormatText,
			AppCommands.DataFormatNumeric,
			AppCommands.DataFormatDate,
			AppCommands.DataFormatTime,
			AppCommands.DataFormatBoolean
		})
	{
	}
}
