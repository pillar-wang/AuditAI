using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketSave : AppCommandButton
{
	public override string Text => "保存表单";

	public override Image LargeIcon => Resources.SaveProject;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.SaveRecord();
	}
}
