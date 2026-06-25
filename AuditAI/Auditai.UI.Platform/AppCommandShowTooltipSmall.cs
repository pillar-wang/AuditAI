using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowTooltipSmall : AppCommandToggleButton
{
	public override Image SmallIcon => ContextResources.ctxParagraphComment;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_动态提示;

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
