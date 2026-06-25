using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketAdd : AppCommandButton
{
	public override string Text => "新增表单";

	public override Image LargeIcon => Resources.CreateTemplate;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.AddRecord();
	}
}
