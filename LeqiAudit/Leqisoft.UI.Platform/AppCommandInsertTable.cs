using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
