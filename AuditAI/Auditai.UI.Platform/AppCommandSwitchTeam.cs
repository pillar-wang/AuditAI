using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandSwitchTeam : AppCommandButton
{
	public override Image LargeIcon => Resources.largeSwitchTeam;

	public override string Text => "切换组织";

	protected override string Tooltip => string.Empty;

	protected override void Clicked()
	{
		Program.MainForm.SwitchTeam();
	}
}
