using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketDesign : AppCommandButton
{
	public override string Text => "设计表单";

	public override Image LargeIcon => Resources.TicketDesign;

	protected override void Clicked()
	{
		Program.MainForm.SwitchToTicketDesignView();
	}
}
