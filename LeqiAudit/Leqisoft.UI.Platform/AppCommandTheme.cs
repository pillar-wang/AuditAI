using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTheme : AppCommandButton
{
	public override Image SmallIcon => Leqisoft.UI.Platform.Properties.Resources.Theme;

	protected override string Tooltip => TipResource.主题设置按钮;

	protected override void Clicked()
	{
		Program.MainForm.SelectTheme();
	}
}
