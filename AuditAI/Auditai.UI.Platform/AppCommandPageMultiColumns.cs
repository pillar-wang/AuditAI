namespace Auditai.UI.Platform;

public class AppCommandPageMultiColumns : AppCommandButton
{
	public override string Text => "多栏...";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetColumnCountDialog();
	}
}
