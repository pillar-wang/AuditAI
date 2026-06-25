namespace Auditai.UI.Platform;

public class AppTabLedger : AppCommandTab
{
	public override string Text => "账务数据";

	public AppTabLedger()
	{
		base.Groups.Add(AppCommandGroups.MakeLedger);
		base.Groups.Add(AppCommandGroups.ManageLedgers);
		base.Groups.Add(AppCommandGroups.RecentLedgers);
		base.Groups.Add(AppCommandGroups.LedgerQuery);
		base.Groups.Add(AppCommandGroups.LedgerAnalysis);
		base.Groups.Add(AppCommandGroups.FillFromLedger);
		base.Groups.Add(AppCommandGroups.LedgerPrint);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (SoftwareLicenseManager.IsLedgerModuleEnable())
		{
			base.Visible = state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.Ledger || state.ViewKind == MainFormView.TicketInput;
		}
		else
		{
			base.Visible = false;
		}
	}

	protected override void Selected()
	{
		Program.MainForm.SwitchFinanceView();
	}
}
