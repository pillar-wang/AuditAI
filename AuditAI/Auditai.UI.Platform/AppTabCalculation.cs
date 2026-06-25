namespace Auditai.UI.Platform;

public class AppTabCalculation : AppCommandTab
{
	public override string Text => "运算校验";

	public AppTabCalculation()
	{
		base.Groups.Add(AppCommandGroups.CalculateTable);
		base.Groups.Add(AppCommandGroups.ValidateTable);
		base.Groups.Add(AppCommandGroups.ValidateDocument);
		base.Groups.Add(AppCommandGroups.DocValidationDomain);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.TicketInput;
	}
}
