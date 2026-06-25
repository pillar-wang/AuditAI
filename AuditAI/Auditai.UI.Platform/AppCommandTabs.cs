namespace Auditai.UI.Platform;

public static class AppCommandTabs
{
	public static AppTabProjects Projects { get; } = new AppTabProjects();


	public static AppTabFile File { get; } = new AppTabFile();


	public static AppTabLedger Ledger { get; } = new AppTabLedger();


	public static AppTabView View { get; } = new AppTabView();


	public static AppTabTable Table { get; } = new AppTabTable();


	public static AppTabDocument Document { get; } = new AppTabDocument();


	public static AppTabAdvanced Advanced { get; } = new AppTabAdvanced();


	public static AppTabCalculation Calculation { get; } = new AppTabCalculation();


	public static AppTabPrint Print { get; } = new AppTabPrint();


	public static AppTabSettings Settings { get; } = new AppTabSettings();


	public static AppTabFormula Formula { get; } = new AppTabFormula();


	public static AppTabMembers Members { get; } = new AppTabMembers();


	public static AppTabTicketDesign TicketDesign { get; } = new AppTabTicketDesign();


	public static AppTabTicketInput TicketInput { get; } = new AppTabTicketInput();

}
