using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketBorderNone : AppCommandButton
{
	public override string Text => " 无边框 ";

	public override Image LargeIcon => Resources.TicketBorderNone;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderNone();
	}
}
