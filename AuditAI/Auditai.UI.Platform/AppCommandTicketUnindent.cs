using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketUnindent : AppCommandButton
{
	public override string Text => "左缩进";

	public override Image LargeIcon => Resources.UnindentCell;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.Unindent();
	}
}
