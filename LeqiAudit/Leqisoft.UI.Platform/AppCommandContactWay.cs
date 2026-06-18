using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandContactWay : AppCommandButton
{
	public override Image SmallIcon => Leqisoft.UI.Platform.Properties.Resources.contactWay;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_联系方式;

	protected override void Clicked()
	{
		Program.MainForm.ShowContactWayForm();
	}
}
