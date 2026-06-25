using System.Drawing;
using Auditai.DTO;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowHelp : AppCommandToggleButton
{
	public override string Text => "软件向导";

	public override System.Drawing.Image LargeIcon => Auditai.UI.Platform.Properties.Resources.Tooltip32;

	protected override string Tooltip => TipResource.显示设置菜单_软件向导;

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
