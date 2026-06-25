using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTheme : AppCommandButton
{
	public override Image SmallIcon => Auditai.UI.Platform.Properties.Resources.Theme;

	protected override string Tooltip => TipResource.主题设置按钮;

	protected override void Clicked()
	{
		Program.MainForm.SelectTheme();
	}
}
