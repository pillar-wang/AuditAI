namespace Leqisoft.UI.Platform;

public class AppCommandBelowSpacingAbsolute : AppCommandButton
{
	public override string Text => "固定行距...";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetAbsoluteParaBelowSpacingDialog();
		}
	}
}
