using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBold : AppCommandToggleButton
{
	public override Image SmallIcon => Resources.Bold;

	protected override void Pressed()
	{
		Program.MainForm.TicketDesignEditor.SetBold(true);
	}

	protected override void Unpressed()
	{
		Program.MainForm.TicketDesignEditor.SetBold(false);
	}
}
