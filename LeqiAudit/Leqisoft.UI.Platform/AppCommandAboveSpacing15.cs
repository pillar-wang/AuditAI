namespace Leqisoft.UI.Platform;

public class AppCommandAboveSpacing15 : AppCommandButton
{
	public override string Text => "1.5倍行距";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetParaAboveSpacing(1.5);
		}
	}
}
