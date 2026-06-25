using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketColumnWidthDecrease : AppCommandButton
{
	public override string Text => "减少列宽";

	public override Image LargeIcon => Resources.DecreaseColumnWidth;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.DecreaseColumnWidth();
	}
}
