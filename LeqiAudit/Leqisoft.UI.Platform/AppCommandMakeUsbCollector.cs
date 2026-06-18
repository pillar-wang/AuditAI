using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMakeUsbCollector : AppCommandButton
{
	public override string Text => "生成U盘采数器";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.MakeCrawler;

	protected override string Tooltip => TipResource.生成U盘采数器按钮;

	protected override void Clicked()
	{
		Program.MainForm.MakeUsbCollector();
	}
}
