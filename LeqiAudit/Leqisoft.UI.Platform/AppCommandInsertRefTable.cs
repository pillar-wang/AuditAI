using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandInsertRefTable : AppCommandButton
{
	public override string Text => "引用表格";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.Intelliref;

	protected override string Tooltip => TipResource.引用表格按钮;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.ImportTable();
	}
}
