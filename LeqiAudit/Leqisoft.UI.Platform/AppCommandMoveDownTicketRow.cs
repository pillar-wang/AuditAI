using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMoveDownTicketRow : AppCommandButton
{
	public override string Text => "下移行";

	public override Image LargeIcon => Resources.RowDown;

	protected override void Clicked()
	{
		Program.MainForm.TicketInputEditor.MoveDownTicketRow();
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
