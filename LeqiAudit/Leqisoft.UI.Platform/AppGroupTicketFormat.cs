using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupTicketFormat : AppCommandGroup
{
	public override string Text => "数据格式";

	public override Image Image => Resources.DataFormat;

	public AppGroupTicketFormat()
	{
		base.Commands.Add(AppCommands.TicketFormat);
		base.Commands.Add(AppCommands.TicketZeroFormat);
		base.Commands.Add(AppCommands.TicketMorePrecision);
		base.Commands.Add(AppCommands.TicketLessPrecision);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.TicketDesign;
	}
}
