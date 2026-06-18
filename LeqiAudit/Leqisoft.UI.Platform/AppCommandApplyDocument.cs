using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
