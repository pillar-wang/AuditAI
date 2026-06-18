using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketAdd : AppCommandButton
{
	public override string Text => "新增表单";

	public override Image LargeIcon => Resources.CreateTemplate;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.AddRecord();
	}
}
