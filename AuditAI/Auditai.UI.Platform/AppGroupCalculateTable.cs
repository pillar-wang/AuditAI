using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupCalculateTable : AppCommandGroup
{
	public override string Text => "表格运算";

	public override Image Image => Resources.CalculateTable;

	public AppGroupCalculateTable()
	{
		base.Commands.Add(AppCommands.CalculateCurrentTable);
		base.Commands.Add(AppCommands.CalculateAllTables);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.TicketInput;
	}
}
