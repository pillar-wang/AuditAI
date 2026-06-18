namespace Leqisoft.UI.Platform;

public class AppGroupControlFormula : AppCommandGroup
{
	public override string Text => "控制公式";

	public AppGroupControlFormula()
	{
		base.Commands.Add(AppCommands.ControlFormula);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			base.Visible = false;
		}
		else
		{
			base.Visible = state.ViewKind == MainFormView.Table;
		}
	}
}
