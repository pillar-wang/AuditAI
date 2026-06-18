using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowNodeNumber : AppCommandToggleButton
{
	public override string Text => "索引号";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.IndexNumber;

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
