using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandSplitCells : AppCommandButton
{
	public override string Text => "拆分单元格";

	public override Image LargeIcon => Resources.SplitCells;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.UnmergeCells();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.UnmergeCells();
			break;
		}
	}
}
