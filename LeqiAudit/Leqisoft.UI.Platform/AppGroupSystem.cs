using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupSystem : AppCommandGroup
{
	public override string Text => "系统";

	public override Image Image => Resources.Quit;

	public AppGroupSystem()
	{
		base.Commands.Add(AppCommands.Quit);
	}
}
