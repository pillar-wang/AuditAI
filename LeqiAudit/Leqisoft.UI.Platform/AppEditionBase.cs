using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using C1.Win.C1Ribbon;
using Leqisoft.PlatformResource;

namespace Leqisoft.UI.Platform;

public abstract class AppEditionBase
{
	public C1Ribbon Ribbon { get; private set; }

	public List<AppCommandTab> Tabs { get; } = new List<AppCommandTab>();


	public List<AppCommandBase> QatCommands { get; } = new List<AppCommandBase>();


	public List<AppCommandBase> ConfigToolbarCommands { get; } = new List<AppCommandBase>();


	public virtual bool EnableLedger
	{
		get
		{
			switch (Program.ClientPlatformType)
			{
			case PlatformType.AuditPlatform:
			case PlatformType.EnterpriseReportPlatform:
				return true;
			case PlatformType.EnterpriseManagerPlatform:
			case PlatformType.TableDevelopPlatform:
			case PlatformType.ProductionCostAccountingSystem:
			case PlatformType.ContractLedgerManagementSystem:
			case PlatformType.RDExpenseLedgerSystem:
			case PlatformType.SalesOrderManagementSystem:
			case PlatformType.PSIManagementSystem:
			case PlatformType.ProjectLedgerManagementSystem:
				return false;
			case PlatformType.Custom:
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("enable_ledger", defaultValue: false);
			default:
				return false;
			}
		}
	}

	public virtual int SubTitleDefaultRows
	{
		get
		{
			switch (Program.ClientPlatformType)
			{
			case PlatformType.AuditPlatform:
				return 2;
			case PlatformType.EnterpriseReportPlatform:
			case PlatformType.EnterpriseManagerPlatform:
			case PlatformType.TableDevelopPlatform:
			case PlatformType.ProductionCostAccountingSystem:
			case PlatformType.ContractLedgerManagementSystem:
			case PlatformType.RDExpenseLedgerSystem:
			case PlatformType.SalesOrderManagementSystem:
			case PlatformType.PSIManagementSystem:
			case PlatformType.ProjectLedgerManagementSystem:
				return 0;
			case PlatformType.Custom:
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Int("sub_title_default_rows", 0);
			default:
				return 0;
			}
		}
	}

	public abstract int Code { get; }

	public abstract Image Icon { get; }

	public abstract string Name { get; }

	public abstract string Tooltip { get; }

	public abstract string PlatformName { get; }

	public abstract Image CurrentProjectIcon { get; }

	public abstract Image CurrentSystemTemplateIcon { get; }

	public abstract Image CurrentCustomTemplateIcon { get; }

	public abstract Image ProjectTileIcon { get; }

	public abstract Image SystemTemplateTileIcon { get; }

	public abstract Image VipSystemTemplateTileIcon { get; }

	public abstract Image CustomTemplateTileIcon { get; }

	public abstract Image UseEmptyTemplateTileIcon { get; }

	public virtual Image PayedTemplateTileCornerIcon { get; }

	public virtual Image UnPayTemplateTileCornerIcon { get; }

	public virtual void GenerateRibbon()
	{
		Ribbon = new C1Ribbon();
		Ribbon.AllowContextMenu = false;
		Ribbon.ApplicationMenu.Visible = false;
		Ribbon.BeginUpdate();
		foreach (AppCommandTab tab in Tabs)
		{
			tab.GenerateRibbonTab();
			Ribbon.Tabs.Add(tab.RibbonTab);
			foreach (AppCommandGroup group in tab.Groups)
			{
				group.GenerateRibbonGroup();
				tab.RibbonTab.Groups.Add(group.RibbonGroup);
				foreach (AppCommandBase command in group.Commands)
				{
					command.GenerateRibbonItem();
					group.RibbonGroup.Items.Add(command.RibbonItem);
				}
			}
		}
		Ribbon.Qat.MenuVisible = false;
		foreach (AppCommandBase qatCommand in QatCommands)
		{
			qatCommand.GenerateRibbonItem();
			Ribbon.Qat.Items.Add(qatCommand.RibbonItem);
		}
		foreach (AppCommandBase configToolbarCommand in ConfigToolbarCommands)
		{
			configToolbarCommand.GenerateRibbonItem();
			Ribbon.ConfigToolBar.Items.Add(configToolbarCommand.RibbonItem);
		}
		Ribbon.EndUpdate();
	}

	public void DetachRibbon()
	{
		Ribbon.Tabs.Clear();
	}

	public virtual void OnAppStateChanged(AppState state)
	{
		Ribbon.BeginUpdate();
		AppCommandTab appCommandTab = Tabs.FirstOrDefault((AppCommandTab t) => t.RibbonTab == Ribbon.SelectedTab);
		if (appCommandTab != null)
		{
			foreach (AppCommandTab item in Tabs.Except(new AppCommandTab[1] { appCommandTab }))
			{
				item.OnAppStateChanged(state);
			}
			appCommandTab.OnAppStateChanged(state);
		}
		else
		{
			foreach (AppCommandTab tab in Tabs)
			{
				tab.OnAppStateChanged(state);
			}
		}
		foreach (AppCommandBase qatCommand in QatCommands)
		{
			qatCommand.OnAppStateChanged(state);
		}
		Ribbon.ConfigToolBar.Enabled = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.Empty || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.TicketInput;
		Ribbon.EndUpdate();
	}

	public AppEditionBase()
	{
		Tabs.Add(AppCommandTabs.Projects);
		Tabs.Add(AppCommandTabs.File);
		Tabs.Add(AppCommandTabs.Members);
		Tabs.Add(AppCommandTabs.Ledger);
		Tabs.Add(AppCommandTabs.View);
		Tabs.Add(AppCommandTabs.Table);
		Tabs.Add(AppCommandTabs.Document);
		Tabs.Add(AppCommandTabs.Advanced);
		Tabs.Add(AppCommandTabs.TicketDesign);
		Tabs.Add(AppCommandTabs.TicketInput);
		Tabs.Add(AppCommandTabs.Calculation);
		Tabs.Add(AppCommandTabs.Print);
		Tabs.Add(AppCommandTabs.Settings);
		Tabs.Add(AppCommandTabs.Formula);
		QatCommands.Add(new AppCommandSeparator());
		QatCommands.Add(AppCommands.Undo);
		QatCommands.Add(AppCommands.Redo);
		ConfigToolbarCommands.Add(AppCommands.ProjectMemberEditSmall);
		ConfigToolbarCommands.Add(new AppCommandSeparator());
		ConfigToolbarCommands.Add(AppCommands.SystemMessage);
		ConfigToolbarCommands.Add(AppCommands.Information);
		ConfigToolbarCommands.Add(AppCommands.Back);
		ConfigToolbarCommands.Add(AppCommands.Forward);
		ConfigToolbarCommands.Add(AppCommands.Reload);
		ConfigToolbarCommands.Add(AppCommands.SaveProject);
		ConfigToolbarCommands.Add(AppCommands.SyncProjectSmall);
		ConfigToolbarCommands.Add(new AppCommandSeparator());
		ConfigToolbarCommands.Add(AppCommands.ShowTooltipSmall);
		ConfigToolbarCommands.Add(AppCommands.ToggleFullscreenSmall);
		ConfigToolbarCommands.Add(AppCommands.Theme);
		ConfigToolbarCommands.Add(AppCommands.ContactWay);
	}
}
