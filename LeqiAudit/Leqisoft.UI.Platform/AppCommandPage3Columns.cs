namespace Leqisoft.UI.Platform;

public class AppCommandPage3Columns : AppCommandButton
{
	public override string Text => "三栏";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetColumnCount(3);
	}
}
