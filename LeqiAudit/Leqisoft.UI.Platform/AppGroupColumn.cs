using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupColumn : AppCommandGroup
{
	public override string Text => "列操作";

	public override Image Image => Resources.ColumnLeft;

	public AppGroupColumn()
	{
		base.Commands.Add(AppCommands.MoveLeftColumn);
		base.Commands.Add(AppCommands.MoveRightColumn);
		base.Commands.Add(AppCommands.IncreaseColumnWidth);
		base.Commands.Add(AppCommands.DecreaseColumnWidth);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			base.Visible = state.ViewKind == MainFormView.Document;
		}
		else
		{
			base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingNote || state.ViewKind == MainFormView.Document;
		}
	}
}
