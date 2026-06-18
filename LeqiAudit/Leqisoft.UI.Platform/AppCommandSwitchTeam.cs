using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
