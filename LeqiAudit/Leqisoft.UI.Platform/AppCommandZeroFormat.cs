using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandZeroFormat : AppCommandMenu
{
	public override string Text => "显示零值";

	public override Image LargeImage => Leqisoft.UI.Platform.Properties.Resources.ToggleZero;

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
