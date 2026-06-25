using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandInsertTextFrame : AppCommandButton
{
	public override string Text => "插入文本框";

	public override Image SmallIcon => ContextResources.ctxInsertFrame;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.InsertTextFrame();
	}
}
