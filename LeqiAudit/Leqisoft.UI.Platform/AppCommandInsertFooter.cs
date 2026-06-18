namespace Leqisoft.UI.Platform;

public class AppCommandInsertFooter : AppCommandButton
{
	public override string Text => "插入页脚";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.InsertFooter();
	}
}
