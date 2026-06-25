using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketDelete : AppCommandButton
{
	public override string Text => "删除表单";

	public override Image LargeIcon => Resources.RemoveProject;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.DeleteRecord();
	}
}
