namespace Leqisoft.UI.Platform;

public class AppCommandBelowSpacing05 : AppCommandButton
{
	public override string Text => "0.5倍行距";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetParaBelowSpacing(0.5);
		}
	}
}
