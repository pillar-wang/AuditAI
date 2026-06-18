using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketImportExcel : AppCommandButton
{
	public override string Text => "导入Excel单据样式";

	public override Image LargeIcon => Resources.ExportExcel;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.ImportXlsx();
	}
}
