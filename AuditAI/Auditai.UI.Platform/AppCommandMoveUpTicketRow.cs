using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMoveUpTicketRow : AppCommandButton
{
	public override string Text => "上移行";

	public override Image LargeIcon => Resources.RowUp;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.MoveUpTicketRow();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.TicketInput;
		if (Program.MainForm == null || Program.MainForm.TicketInputEditor == null)
		{
			Enabled = false;
			Visible = false;
		}
		else
		{
			Enabled = Program.MainForm.TicketInputEditor.IsMoveTicketRowButtonVisible();
			Visible = Program.MainForm.TicketInputEditor.IsMoveTicketRowButtonVisible();
		}
	}
}
