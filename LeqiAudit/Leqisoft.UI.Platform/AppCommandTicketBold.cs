using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
