using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketMorePrecision : AppCommandButton
{
	public override string Text => "增小数位";

	public override Image LargeIcon => Resources.MorePrecision;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.MorePrecision();
	}
}
