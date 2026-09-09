using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketNext : AppCommandButton
{
	public override string Text => "下一个表单";

	public override Image LargeIcon => IconRes.NextError;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.NextRecord();
	}
}
