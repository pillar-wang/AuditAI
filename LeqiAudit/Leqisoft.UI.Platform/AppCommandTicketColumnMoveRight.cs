using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketColumnMoveRight : AppCommandButton
{
	public override string Text => "右移列";

	public override Image LargeIcon => Resources.ColumnRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.MoveRightColumn();
	}
}
