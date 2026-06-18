using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketShrinkFont : AppCommandButton
{
	public override Image SmallIcon => Resources.ShrinkFont;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.ShrinkFont();
	}
}
