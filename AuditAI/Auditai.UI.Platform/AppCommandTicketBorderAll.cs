using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBorderAll : AppCommandButton
{
	public override string Text => "全部框线";

	public override Image LargeIcon => Resources.TicketBorderAll;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderAll();
	}
}
