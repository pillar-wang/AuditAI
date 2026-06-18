namespace Leqisoft.UI.Platform;

public class AppCommandAboveSpacing0 : AppCommandButton
{
	public override string Text => "默认行距";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetParaAboveSpacing(0.0);
		}
	}
}
