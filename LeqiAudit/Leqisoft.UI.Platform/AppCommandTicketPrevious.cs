using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketPrevious : AppCommandButton
{
	public override string Text => "上一个表单";

	public override Image LargeIcon => Resources.PreviousError;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.PreviousRecord();
	}
}
