using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketZeroFormat : AppCommandMenu
{
	public override string Text => "显示零值";

	public override Image LargeImage => Leqisoft.UI.Platform.Properties.Resources.ToggleZero;

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
