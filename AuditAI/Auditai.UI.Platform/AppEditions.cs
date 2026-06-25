using System;
using System.Collections.Generic;

namespace Auditai.UI.Platform;

public static class AppEditions
{
	public static AppEditionCustomSystem CustomSystem;

	public static AppEditionGeneral General { get; } = new AppEditionGeneral();


	public static AppEditionAudit Audit { get; } = new AppEditionAudit();


	public static AppEditionTax Tax { get; } = new AppEditionTax();


	public static AppEditionAppraiser Appraiser { get; } = new AppEditionAppraiser();


	public static AppEditionEnterpriseReport EnterpriseReport { get; } = new AppEditionEnterpriseReport();


	public static AppEditionEnterpriseManager EnterpriseManager { get; } = new AppEditionEnterpriseManager();


	public static AppEditionTableDevelop TableDevelop { get; } = new AppEditionTableDevelop();


	public static AppEditionProductionCostAccountingSystem ProductionCostAccountingSystem { get; } = new AppEditionProductionCostAccountingSystem();


	public static AppEditionContractLedgerManagementSystem ContractLedgerManagementSystem { get; } = new AppEditionContractLedgerManagementSystem();


	public static AppEditionRDExpenseLedgerSystem RDExpenseLedgerSystem { get; } = new AppEditionRDExpenseLedgerSystem();


	public static AppEditionSalesOrderManagementSystem SalesOrderManagementSystem { get; } = new AppEditionSalesOrderManagementSystem();


	public static AppEditionPSIManagementSystem PSIManagementSystem { get; } = new AppEditionPSIManagementSystem();


	public static AppEditionProjectLedgerManagementSystem ProjectLedgerManagementSystem { get; } = new AppEditionProjectLedgerManagementSystem();


	public static List<AppEditionBase> Editions { get; } = new List<AppEditionBase>
	{
		General, Audit, Tax, EnterpriseReport, EnterpriseManager, TableDevelop, ProductionCostAccountingSystem, ContractLedgerManagementSystem, RDExpenseLedgerSystem, SalesOrderManagementSystem,
		PSIManagementSystem, ProjectLedgerManagementSystem
	};


	public static AppEditionBase GetByCode(int code)
	{
		switch (code)
		{
		case 1:
			return General;
		case 2:
			return Audit;
		case 3:
			return Tax;
		case 4:
			return Appraiser;
		case 5:
			return EnterpriseReport;
		case 6:
			return EnterpriseManager;
		case 7:
			return TableDevelop;
		case 8:
			return ProductionCostAccountingSystem;
		case 9:
			return ContractLedgerManagementSystem;
		case 10:
			return RDExpenseLedgerSystem;
		case 11:
			return SalesOrderManagementSystem;
		case 12:
			return PSIManagementSystem;
		case 13:
			return ProjectLedgerManagementSystem;
		default:
			if (CustomSystem != null && CustomSystem.Code == code)
			{
				return CustomSystem;
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}
