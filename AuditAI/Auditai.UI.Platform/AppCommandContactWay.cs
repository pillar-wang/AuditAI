using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandContactWay : AppCommandButton
{
	public override Image SmallIcon => Auditai.UI.Platform.Properties.Resources.contactWay;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_联系方式;

	protected override void Clicked()
	{
		Program.MainForm.ShowContactWayForm();
	}
}
