using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupRow : AppCommandGroup
{
	public override string Text => "行操作";

	public override Image Image => Resources.RowUp;

	public AppGroupRow()
	{
		base.Commands.Add(AppCommands.MoveUpRow);
		base.Commands.Add(AppCommands.MoveDownRow);
		base.Commands.Add(AppCommands.IncreaseRowHeight);
		base.Commands.Add(AppCommands.DecreaseRowHeight);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingNote || state.ViewKind == MainFormView.Document;
	}
}
