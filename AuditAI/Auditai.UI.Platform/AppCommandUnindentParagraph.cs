using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandUnindentParagraph : AppCommandButton
{
	public override string Text => "整段减缩进";

	public override Image LargeIcon => Resources.UnindentPara;

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.Unindent();
		}
	}
}
