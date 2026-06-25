using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBorderBottom : AppCommandButton
{
	public override string Text => " 下边框 ";

	public override Image LargeIcon => Resources.TicketBorderBottom;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderBottom();
	}
}
