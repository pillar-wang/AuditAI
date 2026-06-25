namespace Auditai.UI.Platform;

public class AppCommandBelowSpacing0 : AppCommandButton
{
	public override string Text => "默认行距";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetParaBelowSpacing(0.0);
		}
	}
}
