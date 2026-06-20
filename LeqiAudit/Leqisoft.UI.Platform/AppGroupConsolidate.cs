﻿using System.Drawing;
using Leqisoft.DTO;
using Leqisoft.PlatformResource;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupConsolidate : AppCommandGroup
{
	public override string Text => "合并报表";

	public override System.Drawing.Image Image => Resources.ConsolidateStatements;

	public AppGroupConsolidate()
	{
		base.Commands.Add(AppCommands.ConsolidateSetting);
		base.Commands.Add(AppCommands.ExecuteConsolidate);
		base.Commands.Add(AppCommands.RefreshConsolidate);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = Program.MainForm.CurrentProject != null && Program.MainForm.CurrentProject.Kind == ProjectType.Project && state.ViewKind == MainFormView.Table;
		switch (Program.ClientPlatformType)
		{
		case PlatformType.EnterpriseReportPlatform:
		case PlatformType.EnterpriseManagerPlatform:
		case PlatformType.TableDevelopPlatform:
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			base.Visible = false;
			break;
		case PlatformType.Custom:
			base.Visible = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_consolidate_setting", defaultValue: false);
			break;
		}
	}
}
