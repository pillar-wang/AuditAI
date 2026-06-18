using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandFormulaMap : AppCommandButton
{
	public override Image SmallIcon => Leqisoft.UI.Platform.Properties.Resources.FormulaMap16;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_流程图;

	protected override void Clicked()
	{
		Program.MainForm.ShowFormulaMap();
	}
}
