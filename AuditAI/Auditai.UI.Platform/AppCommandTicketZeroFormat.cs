using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketZeroFormat : AppCommandMenu
{
	public override string Text => "显示零值";

	public override Image LargeImage => Auditai.UI.Platform.Properties.Resources.ToggleZero;

	protected override string Tooltip => TipResource.显示零值;

	public AppCommandTicketZeroFormat()
		: base(new AppCommandBase[3]
		{
			AppCommands.TicketZeroFormatEmpty,
			AppCommands.TicketZeroFormatZero,
			AppCommands.TicketZeroFormatDash
		})
	{
	}
}
