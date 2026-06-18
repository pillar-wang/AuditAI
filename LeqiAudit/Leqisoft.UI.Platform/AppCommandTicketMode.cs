using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketMode : AppCommandButton
{
	public override string Text => "设计单据";

	public override System.Drawing.Image LargeIcon => Resources.TicketMode;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.Table.Ticket.Level = TicketLevel.Receipt;
		Program.MainForm.SwitchToTicketDesignView();
	}

	public override void OnAppStateChanged(AppState state)
	{
		Visible = false;
	}
}
