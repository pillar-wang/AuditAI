using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketPrevious : AppCommandButton
{
	public override string Text => "上一个表单";

	public override Image LargeIcon => Resources.PreviousError;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.PreviousRecord();
	}
}
