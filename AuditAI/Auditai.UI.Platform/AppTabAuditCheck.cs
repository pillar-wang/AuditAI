namespace Auditai.UI.Platform;

public class AppTabAuditCheck : AppCommandTab
{
	public override string Text => "审计检查";

	public AppTabAuditCheck()
	{
		// 首组：账务数据打开按钮（账套查看器独立窗口入口）
		base.Groups.Add(AppCommandGroups.LedgerWindow);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!SoftwareLicenseManager.IsLedgerModuleEnable())
		{
			base.Visible = false;
			return;
		}
		// 可见性与其他普通标签一致，仅随视图模式切换
		base.Visible = state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.Ledger || state.ViewKind == MainFormView.TicketInput;
	}
}