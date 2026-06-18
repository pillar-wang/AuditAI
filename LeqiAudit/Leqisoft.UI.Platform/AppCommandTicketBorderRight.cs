using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketBorderRight : AppCommandButton
{
	public override string Text => " 右边框 ";

	public override Image LargeIcon => Resources.TicketBorderRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderRight();
	}
}
