using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMergeCells : AppCommandButton
{
	public override string Text => "合并单元格";

	public override Image LargeIcon => Resources.MergeCells;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.MergeCells();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.MergeCells();
			break;
		}
	}
}
