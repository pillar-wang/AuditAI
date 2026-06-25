using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketItalic : AppCommandToggleButton
{
	public override Image SmallIcon => Resources.Italic;

	protected override void Pressed()
	{
		Program.MainForm.TicketDesignEditor.SetItalic(true);
	}

	protected override void Unpressed()
	{
		Program.MainForm.TicketDesignEditor.SetItalic(false);
	}
}
