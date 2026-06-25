using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketImportTable : AppCommandButton
{
	public override string Text => "导入他表单据样式";

	public override Image LargeIcon => Resources.TicketMode;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.ImportTable();
	}
}
