using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandLineSpacing : AppCommandMenu
{
	public override string Text => "段内行距";

	public override Image LargeImage => Resources.LineSpacing;

	public AppCommandLineSpacing()
		: base(new AppCommandBase[5]
		{
			AppCommands.LineSpacing1,
			AppCommands.LineSpacing15,
			AppCommands.LineSpacing2,
			AppCommands.LineSpacingMulti,
			AppCommands.LineSpacingAbsolute
		})
	{
	}
}
