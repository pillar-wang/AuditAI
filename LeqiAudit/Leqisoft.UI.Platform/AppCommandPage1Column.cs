namespace Leqisoft.UI.Platform;

public class AppCommandPage1Column : AppCommandButton
{
	public override string Text => "一栏";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetColumnCount(1);
	}
}
