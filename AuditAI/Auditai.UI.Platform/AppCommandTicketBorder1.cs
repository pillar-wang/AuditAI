using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBorder1 : AppCommandButton
{
	public override string Text => "样式框线1";

	public override Image LargeIcon => Resources.TableStyle1;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.BorderStyle1();
	}
}
