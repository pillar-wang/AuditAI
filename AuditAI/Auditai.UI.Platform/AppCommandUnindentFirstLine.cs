using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandUnindentFirstLine : AppCommandButton
{
	public override Image LargeIcon => Resources.Unindent1stLine;

	public override string Text => "段首减缩进";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.FirstIndentDecrease();
		}
	}
}
