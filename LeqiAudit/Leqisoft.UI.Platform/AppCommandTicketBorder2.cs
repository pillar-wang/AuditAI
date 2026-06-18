using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketBorder2 : AppCommandButton
{
	public override string Text => "样式框线2";

	public override Image LargeIcon => Resources.TableStyle3;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderStyle2();
	}
}
