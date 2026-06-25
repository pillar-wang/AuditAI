using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMakeUsbCollector : AppCommandButton
{
	public override string Text => "生成U盘采数器";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.MakeCrawler;

	protected override string Tooltip => TipResource.生成U盘采数器按钮;

	protected override void Clicked()
	{
		Program.MainForm.MakeUsbCollector();
	}
}
