using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupHelp : AppCommandGroup
{
	public override string Text => "设置";

	public override Image Image => IconRes.Settings;

	public AppGroupHelp()
	{
		base.Commands.Add(AppCommands.SystemSettings);
		base.Commands.Add(AppCommands.StandardAccountDic);
		base.Commands.Add(AppCommands.CheckUpdate);
		base.Commands.Add(AppCommands.About);
	}
}
