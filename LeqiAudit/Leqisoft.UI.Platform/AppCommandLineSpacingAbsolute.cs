namespace Leqisoft.UI.Platform;

public class AppCommandLineSpacingAbsolute : AppCommandButton
{
	public override string Text => "固定行距...";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetAbsoluteLineSpacingDialog();
		}
	}
}
