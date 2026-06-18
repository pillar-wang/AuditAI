namespace Leqisoft.UI.Platform;

public class AppCommandPage2Columns : AppCommandButton
{
	public override string Text => "两栏";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.SetColumnCount(2);
	}
}
