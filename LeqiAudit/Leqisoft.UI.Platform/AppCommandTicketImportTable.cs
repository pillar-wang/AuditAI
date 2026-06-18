using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketImportTable : AppCommandButton
{
	public override string Text => "导入他表单据样式";

	public override Image LargeIcon => Resources.TicketMode;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.ImportTable();
	}
}
