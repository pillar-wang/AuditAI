namespace Leqisoft.UI.Platform;

public class AppCommandParagraphAlignCenter : AppCommandButton
{
	public override string Text => "居中对齐";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.AlignCenter();
		}
	}
}
