using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandForward : AppCommandButton
{
	public override Image SmallIcon => Auditai.UI.Platform.Properties.Resources.forward;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_前进;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		Enabled = false;
	}

	protected override void Clicked()
	{
		Program.MainForm.Forward();
	}
}
