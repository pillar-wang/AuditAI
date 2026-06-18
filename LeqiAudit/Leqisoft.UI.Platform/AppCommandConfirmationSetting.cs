using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandConfirmationSetting : AppCommandButton
{
	public override string Text => "函证设置";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ConfirmationSet;

	protected override string Tooltip => TipResource.高级功能菜单_文档_函证设置;

	protected override void Clicked()
	{
		Program.MainForm.ConfirmationSetting();
	}
}
