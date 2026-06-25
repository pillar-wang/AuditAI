using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandValidateCurrentTable : AppCommandButton
{
	public override string Text => "当前表校验";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ValidateTable;

	protected override string Tooltip => TipResource.当前表校验;

	protected override void Clicked()
	{
		Program.MainForm.ValidateCurrentTable();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TicketInput;
	}
}
