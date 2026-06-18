using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketDelete : AppCommandButton
{
	public override string Text => "删除表单";

	public override Image LargeIcon => Resources.RemoveProject;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.DeleteRecord();
	}
}
