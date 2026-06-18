using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketBorderTop : AppCommandButton
{
	public override string Text => " 上边框 ";

	public override Image LargeIcon => Resources.TicketBorderTop;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderTop();
	}
}
