using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupSystem : AppCommandGroup
{
	public override string Text => "系统";

	public override Image Image => Resources.Quit;

	public AppGroupSystem()
	{
		base.Commands.Add(AppCommands.Quit);
	}
}
