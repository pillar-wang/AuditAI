using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandInsertRefTable : AppCommandButton
{
	public override string Text => "引用表格";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.Intelliref;

	protected override string Tooltip => TipResource.引用表格按钮;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.ImportTable();
	}
}
