using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketGrowFont : AppCommandButton
{
	public override Image SmallIcon => Resources.GrowFont;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.GrowFont();
	}
}
