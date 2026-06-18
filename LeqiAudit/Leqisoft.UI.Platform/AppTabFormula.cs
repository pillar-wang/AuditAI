namespace Leqisoft.UI.Platform;

public class AppTabFormula : AppCommandTab
{
	public override string Text => "公式编辑";

	public AppTabFormula()
	{
		base.Groups.Add(AppCommandGroups.FormulaTip);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.EditingFormula || state.ViewKind == MainFormView.EditingValidation || state.ViewKind == MainFormView.TicketFormula;
	}
}
