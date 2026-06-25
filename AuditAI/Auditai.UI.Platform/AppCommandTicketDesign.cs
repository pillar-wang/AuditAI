using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketDesign : AppCommandButton
{
	public override string Text => "设计表单";

	public override Image LargeIcon => Resources.TicketDesign;

	protected override void Clicked()
	{
		Program.MainForm.SwitchToTicketDesignView();
	}
}
