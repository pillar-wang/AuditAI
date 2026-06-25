using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBorderRight : AppCommandButton
{
	public override string Text => " 右边框 ";

	public override Image LargeIcon => Resources.TicketBorderRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderRight();
	}
}
