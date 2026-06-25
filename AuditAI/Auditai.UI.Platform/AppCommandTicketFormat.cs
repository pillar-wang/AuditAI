using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketFormat : AppCommandMenu
{
	public override string Text => "数据格式";

	public override Image LargeImage => Resources.DataFormat;

	public AppCommandTicketFormat()
		: base(new AppCommandBase[5]
		{
			AppCommands.TicketFormatText,
			AppCommands.TicketFormatNumeric,
			AppCommands.TicketFormatDate,
			AppCommands.TicketFormatTime,
			AppCommands.TicketFormatBoolean
		})
	{
	}
}
