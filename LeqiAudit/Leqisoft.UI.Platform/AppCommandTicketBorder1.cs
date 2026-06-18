using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketBorder1 : AppCommandButton
{
	public override string Text => "样式框线1";

	public override Image LargeIcon => Resources.TableStyle1;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderStyle1();
	}
}
