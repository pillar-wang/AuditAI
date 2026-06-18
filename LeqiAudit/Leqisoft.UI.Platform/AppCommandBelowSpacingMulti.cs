namespace Leqisoft.UI.Platform;

public class AppCommandBelowSpacingMulti : AppCommandButton
{
	public override string Text => "多倍行距...";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetParaBelowSpacingDialog();
		}
	}
}
