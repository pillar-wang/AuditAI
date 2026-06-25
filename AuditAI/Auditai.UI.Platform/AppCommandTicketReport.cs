using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketReport : AppCommandButton
{
	public override string Text => "设计表单";

	public override System.Drawing.Image LargeIcon => Resources.TicketMode;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.Table.Ticket.Level = TicketLevel.Report;
		Program.MainForm.SwitchToTicketDesignView();
	}
}
