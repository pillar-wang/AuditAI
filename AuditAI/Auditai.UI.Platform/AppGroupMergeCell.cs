using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupMergeCell : AppCommandGroup
{
	public override string Text => "单元格操作";

	public override Image Image => Resources.MergeCells;

	public AppGroupMergeCell()
	{
		base.Commands.Add(AppCommands.MergeCells);
		base.Commands.Add(AppCommands.SplitCells);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document;
	}
}
