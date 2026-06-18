namespace Leqisoft.UI.Platform;

public class AppCommandBelowSpacing1 : AppCommandButton
{
	public override string Text => "1倍行距";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetParaBelowSpacing(1.0);
		}
	}
}
