using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupTitleRow : AppCommandGroup
{
	public override string Text => "行操作";

	public override Image Image => Resources.RowUp;

	public AppGroupTitleRow()
	{
		base.Commands.Add(AppCommands.TitleIncreaseRowHeight);
		base.Commands.Add(AppCommands.TitleDecreaseRowHeight);
		base.Commands.Add(AppCommands.TitleUnifyRowHeight);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
