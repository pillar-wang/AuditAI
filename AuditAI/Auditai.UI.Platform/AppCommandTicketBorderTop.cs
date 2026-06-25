using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBorderTop : AppCommandButton
{
	public override string Text => " 上边框 ";

	public override Image LargeIcon => Resources.TicketBorderTop;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderTop();
	}
}
