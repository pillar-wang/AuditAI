using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandApplyDocument : AppCommandToggleButton
{
	public override string Text => "整篇文档";

	public override Image LargeIcon => Resources.DocPrintSettingDocument;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		base.ToggleButton.CanDepress = false;
	}

	protected override void Pressed()
	{
		Program.MainForm.CurrentDocumentEditor.PageSettingTargetDocumentClicked();
	}

	protected override void Unpressed()
	{
		Program.MainForm.CurrentDocumentEditor.PageSettingTargetDocumentClicked();
	}
}
