using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketBorderBottom : AppCommandButton
{
	public override string Text => " 下边框 ";

	public override Image LargeIcon => Resources.TicketBorderBottom;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderBottom();
	}
}
