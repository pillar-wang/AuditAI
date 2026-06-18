using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketUnindent : AppCommandButton
{
	public override string Text => "左缩进";

	public override Image LargeIcon => Resources.UnindentCell;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.Unindent();
	}
}
