using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketGrowFont : AppCommandButton
{
	public override Image SmallIcon => Resources.GrowFont;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.GrowFont();
	}
}
