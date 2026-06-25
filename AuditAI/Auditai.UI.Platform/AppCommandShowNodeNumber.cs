using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowNodeNumber : AppCommandToggleButton
{
	public override string Text => "索引号";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.IndexNumber;

	protected override string Tooltip => TipResource.显示设置菜单_索引号;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.IsPressed = true;
	}

	protected override void Pressed()
	{
		Program.MainForm.ProjectHierarchy.ShowNumber();
	}

	protected override void Unpressed()
	{
		Program.MainForm.ProjectHierarchy.HideNumber();
	}
}
