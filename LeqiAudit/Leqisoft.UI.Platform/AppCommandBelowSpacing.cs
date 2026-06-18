using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandBelowSpacing : AppCommandMenu
{
	public override string Text => "段后行距";

	public override Image LargeImage => Resources.ParaBelowSpacing;

	public AppCommandBelowSpacing()
		: base(new AppCommandBase[6]
		{
			AppCommands.BelowSpacing0,
			AppCommands.BelowSpacing05,
			AppCommands.BelowSpacing1,
			AppCommands.BelowSpacing15,
			AppCommands.BelowSpacingMulti,
			AppCommands.BelowSpacingAbsolute
		})
	{
	}
}
