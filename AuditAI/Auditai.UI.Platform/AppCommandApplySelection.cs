using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandApplySelection : AppCommandToggleButton
{
	public override string Text => "选定区域";

	public override Image LargeIcon => Resources.DocPrintSettingSelection;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.ToggleButton.CanDepress = false;
		base.IsPressed = true;
	}

	protected override void Pressed()
	{
		Program.MainForm.CurrentDocumentEditor.PageSettingTargetSelectionClicked();
	}

	protected override void Unpressed()
	{
		Program.MainForm.CurrentDocumentEditor.PageSettingTargetSelectionClicked();
	}
}
