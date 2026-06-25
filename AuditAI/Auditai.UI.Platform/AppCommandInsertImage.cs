using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandInsertImage : AppCommandButton
{
	public override string Text => "插入图片";

	public override Image LargeIcon => ContextResources.ctxInsertImage;

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.InsertImage();
		}
	}
}
