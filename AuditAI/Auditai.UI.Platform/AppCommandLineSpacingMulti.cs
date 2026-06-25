namespace Auditai.UI.Platform;

public class AppCommandLineSpacingMulti : AppCommandButton
{
	public override string Text => "多倍行距...";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetLineSpacingDialog();
		}
	}
}
