using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandInsertTable : AppCommandButton
{
	public override string Text => "插入表格";

	public override Image LargeIcon => ContextResources.ctxInsertTable;

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.InsertTable();
		}
	}
}
