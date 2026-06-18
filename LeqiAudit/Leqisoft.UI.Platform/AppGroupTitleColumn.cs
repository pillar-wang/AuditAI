namespace Leqisoft.UI.Platform;

public class AppGroupTitleColumn : AppCommandGroup
{
	public override string Text => "列操作";

	public AppGroupTitleColumn()
	{
		base.Commands.Add(AppCommands.TitleIncreaseColumnWidth);
		base.Commands.Add(AppCommands.TitleDecreaseColumnWidth);
		base.Commands.Add(AppCommands.TitleUnifyColumnWidth);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
