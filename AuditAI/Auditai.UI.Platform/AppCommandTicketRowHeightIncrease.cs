using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketRowHeightIncrease : AppCommandButton
{
	public override string Text => "增加行高";

	public override Image LargeIcon => Resources.IncreaseRowHeight;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.IncreaseRowHeight();
	}
}
