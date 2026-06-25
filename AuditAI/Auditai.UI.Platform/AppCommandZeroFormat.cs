using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandZeroFormat : AppCommandMenu
{
	public override string Text => "显示零值";

	public override Image LargeImage => Auditai.UI.Platform.Properties.Resources.ToggleZero;

	protected override string Tooltip => TipResource.显示零值;

	public AppCommandZeroFormat()
		: base(new AppCommandBase[3]
		{
			AppCommands.ZeroFormatEmpty,
			AppCommands.ZeroFormatZero,
			AppCommands.ZeroFormatDash
		})
	{
	}
}
