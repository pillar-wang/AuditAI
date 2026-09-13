﻿namespace Auditai.UI.Platform;

public class AppTabLedger : AppCommandTab
{
	public override string Text => "账务数据";

	// 账务窗口工具栏标签：不随主 Ribbon 显示，其 RibbonTab 挂到账务窗口顶部工具栏
	public override bool InMainRibbon => false;

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
		if (!SoftwareLicenseManager.IsLedgerModuleEnable())
		{
			base.Visible = false;
			return;
		}
		// 工具栏随账务窗口走：窗口显示期间命令保持常显，不随主窗口视图模式（编辑标题/公式等）隐藏
		MainForm mainForm = Program.MainForm;
		base.Visible = (mainForm != null && mainForm.LedgerWindowActive) || state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview || state.ViewKind == MainFormView.Ledger || state.ViewKind == MainFormView.TicketInput;
	}

	protected override void Selected()
	{
		Program.MainForm.ShowLedgerWindow();
	}
}