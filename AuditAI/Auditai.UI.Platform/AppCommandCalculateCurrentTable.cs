using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandCalculateCurrentTable : AppCommandButton
{
	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.CalculateTable;

	public override string Text => "当前表运算";

	protected override string Tooltip => TipResource.当前表运算;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.CalcCurrentTable();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TicketInput;
	}
}
