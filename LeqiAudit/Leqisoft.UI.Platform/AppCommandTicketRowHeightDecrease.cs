using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketRowHeightDecrease : AppCommandButton
{
	public override string Text => "减少行高";

	public override Image LargeIcon => Resources.DecreaseRowHeight;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.DecreaseRowHeight();
	}
}
