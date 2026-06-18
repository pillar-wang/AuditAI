namespace Leqisoft.UI.Platform;

public class AppCommandParagraphAlignLeft : AppCommandButton
{
	public override string Text => "左对齐";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.AlignLeft();
		}
	}
}
