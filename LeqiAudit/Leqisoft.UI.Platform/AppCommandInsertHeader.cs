namespace Leqisoft.UI.Platform;

public class AppCommandInsertHeader : AppCommandButton
{
	public override string Text => "插入页眉";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.InsertHeader();
	}
}
