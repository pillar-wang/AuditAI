namespace Leqisoft.UI.Platform;

public class AppCommandParagraphAlignJustify : AppCommandButton
{
	public override string Text => "两端对齐";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.AlignJustify();
		}
	}
}
