namespace Auditai.UI.Platform;

public class AppCommandParagraphAlignRight : AppCommandButton
{
	public override string Text => "右对齐";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.AlignRight();
		}
	}
}
