using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowTooltip : AppCommandToggleButton
{
	public override string Text => "动态提示";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.EditComment;

	protected override string Tooltip => TipResource.显示设置菜单_动态提示;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.IsPressed = true;
	}

	protected override void Pressed()
	{
		Program.MainForm.SetShowHelperTooltipAndRefresh(show: true);
	}

	protected override void Unpressed()
	{
		Program.MainForm.SetShowHelperTooltipAndRefresh(show: false);
	}
}
