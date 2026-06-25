using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketIndent : AppCommandButton
{
	public override string Text => "右缩进";

	public override Image LargeIcon => Resources.IndentCell;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.Indent();
	}
}
