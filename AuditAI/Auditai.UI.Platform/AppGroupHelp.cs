using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupHelp : AppCommandGroup
{
	public override string Text => "帮助";

	public override Image Image => Resources.Settings;

	public AppGroupHelp()
	{
		base.Commands.Add(AppCommands.Help);
		base.Commands.Add(AppCommands.SystemSettings);
		base.Commands.Add(AppCommands.CheckUpdate);
		base.Commands.Add(AppCommands.About);
	}
}
