using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupTicketAlign : AppCommandGroup
{
	public override string Text => "对齐设置";

	public override Image Image => Resources.tb_AlignTopLeft;

	public AppGroupTicketAlign()
	{
		base.Commands.Add(AppCommands.TicketAlignTopLeft);
		base.Commands.Add(AppCommands.TicketAlignMiddleLeft);
		base.Commands.Add(AppCommands.TicketAlignBottomLeft);
		base.Commands.Add(AppCommands.TicketAlignTopCenter);
		base.Commands.Add(AppCommands.TicketAlignMiddleCenter);
		base.Commands.Add(AppCommands.TicketAlignBottomCenter);
		base.Commands.Add(AppCommands.TicketAlignTopRight);
		base.Commands.Add(AppCommands.TicketAlignMiddleRight);
		base.Commands.Add(AppCommands.TicketAlignBottomRight);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingColHeader || state.ViewKind == MainFormView.Document;
	}
}
