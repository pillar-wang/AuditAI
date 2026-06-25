using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandCollectByCell : AppCommandButton
{
	public override string Text => "单元格采账设置";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.CellCollect;

	protected override string Tooltip => TipResource.单元格采数设置按钮;

	protected override void Clicked()
	{
		Program.MainForm.CellCollectSet();
	}
}
