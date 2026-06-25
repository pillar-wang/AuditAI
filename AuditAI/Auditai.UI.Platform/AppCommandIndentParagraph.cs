using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandIndentParagraph : AppCommandButton
{
	public override string Text => "整段增缩进";

	public override Image LargeIcon => Resources.IndentPara;

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.Indent();
		}
	}
}
