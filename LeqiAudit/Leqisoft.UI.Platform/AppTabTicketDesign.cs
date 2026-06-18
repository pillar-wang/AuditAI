namespace Leqisoft.UI.Platform;

public class AppTabTicketDesign : AppCommandTab
{
	public override string Text => "单据设计";

	public AppTabTicketDesign()
	{
		base.Groups.Add(AppCommandGroups.TicketColumnMove);
		base.Groups.Add(AppCommandGroups.TicketFont);
		base.Groups.Add(AppCommandGroups.TicketAlign);
		base.Groups.Add(AppCommandGroups.TicketIndent);
		base.Groups.Add(AppCommandGroups.TicketBorder);
		base.Groups.Add(AppCommandGroups.TicketFormat);
		base.Groups.Add(AppCommandGroups.TicketImport);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.Visible = state.ViewKind == MainFormView.TicketDesign;
	}
}
