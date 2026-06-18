using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketRowHeightIncrease : AppCommandButton
{
	public override string Text => "增加行高";

	public override Image LargeIcon => Resources.IncreaseRowHeight;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.IncreaseRowHeight();
	}
}
