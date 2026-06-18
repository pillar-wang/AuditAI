namespace Leqisoft.UI.Platform;

public class AppCommandLineSpacing2 : AppCommandButton
{
	public override string Text => "2倍行距";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetLineSpacing(200);
		}
	}
}
