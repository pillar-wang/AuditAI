using System.Drawing;
using Leqisoft.DTO;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowHelp : AppCommandToggleButton
{
	public override string Text => "软件向导";

	public override System.Drawing.Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.Tooltip32;

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
