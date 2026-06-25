using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandIndentFirstLine : AppCommandButton
{
	public override string Text => "段首增缩进";

	public override Image LargeIcon => Resources.Indent1stLine;

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.FirstIndentIncrease();
		}
	}
}
