using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandUserInfo : AppCommandButton
{
	public override string Text => "用户资料";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.SwitchUser;

	protected override string Tooltip => TipResource.用户资料按钮;

	protected override void Clicked()
	{
		Program.MainForm.AlterInfo();
	}
}
