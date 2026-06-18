using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandInsertTextFrame : AppCommandButton
{
	public override string Text => "插入文本框";

	public override Image SmallIcon => ContextResources.ctxInsertFrame;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.InsertTextFrame();
	}
}
