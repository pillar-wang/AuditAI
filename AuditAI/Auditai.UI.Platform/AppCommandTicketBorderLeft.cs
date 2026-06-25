using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBorderLeft : AppCommandButton
{
	public override string Text => " 左边框 ";

	public override Image LargeIcon => Resources.TicketBorderLeft;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderLeft();
	}
}
