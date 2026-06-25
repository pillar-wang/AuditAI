using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandConfirmationSetting : AppCommandButton
{
	public override string Text => "函证设置";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ConfirmationSet;

	protected override string Tooltip => TipResource.高级功能菜单_文档_函证设置;

	protected override void Clicked()
	{
		Program.MainForm.ConfirmationSetting();
	}
}
