using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
