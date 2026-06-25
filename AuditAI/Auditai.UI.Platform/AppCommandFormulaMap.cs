using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandFormulaMap : AppCommandButton
{
	public override Image SmallIcon => Auditai.UI.Platform.Properties.Resources.FormulaMap16;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_流程图;

	protected override void Clicked()
	{
		Program.MainForm.ShowFormulaMap();
	}
}
