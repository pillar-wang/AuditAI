using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBorderNone : AppCommandButton
{
	public override string Text => " 无边框 ";

	public override Image LargeIcon => Resources.TicketBorderNone;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderNone();
	}
}
