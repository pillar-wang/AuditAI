using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketShrinkFont : AppCommandButton
{
	public override Image SmallIcon => Resources.ShrinkFont;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.ShrinkFont();
	}
}
