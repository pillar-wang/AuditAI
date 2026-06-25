using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketColumnMoveLeft : AppCommandButton
{
	public override string Text => "左移列";

	public override Image LargeIcon => Resources.ColumnLeft;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.MoveLeftColumn();
	}
}
