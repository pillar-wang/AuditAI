using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketColumnMoveRight : AppCommandButton
{
	public override string Text => "右移列";

	public override Image LargeIcon => Resources.ColumnRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.MoveRightColumn();
	}
}
