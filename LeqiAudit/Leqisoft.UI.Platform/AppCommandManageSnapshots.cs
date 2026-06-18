using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandManageSnapshots : AppCommandButton
{
	public override string Text => "历史版本";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.Snapshots;

	protected override Func<Task> ClickedTask => async delegate
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
		case MainFormView.TicketInput:
			await Program.MainForm.RevertTableDialog();
			break;
		case MainFormView.Document:
			await Program.MainForm.RevertDocumentDialog();
			break;
		}
	};

	protected override string Tooltip => TipResource.历史版本功能说明;

	protected override void Clicked()
	{
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TicketInput;
	}
}
