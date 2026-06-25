using System.Drawing;
using Auditai.DTO;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowHelpSmall : AppCommandToggleButton
{
	public override System.Drawing.Image SmallIcon => Auditai.UI.Platform.Properties.Resources.Tooltip16;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_软件向导;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.IsPressed = UserSet.Config.Tooltip;
	}

	protected override void Pressed()
	{
		Program.MainForm.ToggleTooltip();
	}

	protected override void Unpressed()
	{
		Program.MainForm.ToggleTooltip();
	}
}
