using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketImportExcel : AppCommandButton
{
	public override string Text => "导入Excel单据样式";

	public override Image LargeIcon => Resources.ExportExcel;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.ImportXlsx();
	}
}
