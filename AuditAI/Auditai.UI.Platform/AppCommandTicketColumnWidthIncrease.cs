using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketColumnWidthIncrease : AppCommandButton
{
	public override string Text => "增加列宽";

	public override Image LargeIcon => Resources.IncreaseColumnWidth;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.IncreaseColumnWidth();
	}
}
