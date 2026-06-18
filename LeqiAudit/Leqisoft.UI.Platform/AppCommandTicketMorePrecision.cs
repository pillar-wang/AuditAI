using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketMorePrecision : AppCommandButton
{
	public override string Text => "增小数位";

	public override Image LargeIcon => Resources.MorePrecision;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.MorePrecision();
	}
}
