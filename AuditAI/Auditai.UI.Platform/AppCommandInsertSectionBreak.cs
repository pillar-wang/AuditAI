namespace Auditai.UI.Platform;

public class AppCommandInsertSectionBreak : AppCommandButton
{
	public override string Text => "插入分节符";

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.InsertSectionBreak();
	}
}
