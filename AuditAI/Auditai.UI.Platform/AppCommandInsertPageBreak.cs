namespace Auditai.UI.Platform;

public class AppCommandInsertPageBreak : AppCommandButton
{
	public override string Text => "插入分页符";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.InsertPageBreak();
	}
}
