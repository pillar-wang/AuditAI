using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketColumnMoveLeft : AppCommandButton
{
	public override string Text => "左移列";

	public override Image LargeIcon => Resources.ColumnLeft;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.MoveLeftColumn();
	}
}
