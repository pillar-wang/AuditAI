using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketLessPrecision : AppCommandButton
{
	public override string Text => "减小数位";

	public override Image LargeIcon => Resources.LessPrecision;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.LessPrecision();
	}
}
