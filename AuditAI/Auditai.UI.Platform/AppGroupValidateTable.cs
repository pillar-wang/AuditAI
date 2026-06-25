using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupValidateTable : AppCommandGroup
{
	public override string Text => "表格校验";

	public override Image Image => Resources.ValidateTable;

	public AppGroupValidateTable()
	{
		base.Commands.Add(AppCommands.ValidateCurrentTable);
		base.Commands.Add(AppCommands.ValidateAllTables);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.PreviousError);
		base.Commands.Add(AppCommands.NextError);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.TicketInput;
	}
}
