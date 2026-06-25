using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandAboveSpacing : AppCommandMenu
{
	public override string Text => "段前行距";

	public override Image LargeImage => Resources.ParaAboveSpacing;

	public AppCommandAboveSpacing()
		: base(new AppCommandBase[6]
		{
			AppCommands.AboveSpacing0,
			AppCommands.AboveSpacing05,
			AppCommands.AboveSpacing1,
			AppCommands.AboveSpacing15,
			AppCommands.AboveSpacingMulti,
			AppCommands.AboveSpacingAbsolute
		})
	{
	}
}
