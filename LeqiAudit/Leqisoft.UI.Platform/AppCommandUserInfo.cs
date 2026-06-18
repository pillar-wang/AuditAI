using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandUserInfo : AppCommandButton
{
	public override string Text => "用户资料";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.SwitchUser;

	protected override string Tooltip => TipResource.用户资料按钮;

	protected override void Clicked()
	{
		Program.MainForm.AlterInfo();
	}
}
