using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
