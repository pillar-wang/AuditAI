namespace Auditai.Model;

public static class StringConstEditions
{
	public static StringConstGeneralEdition General { get; } = new StringConstGeneralEdition();


	public static StringConstAuditEdition Audit { get; } = new StringConstAuditEdition();


	public static StringConstTaxEdition Tax { get; } = new StringConstTaxEdition();


	public static StringConstAppraiserEdition Appraiser { get; } = new StringConstAppraiserEdition();


	public static StringConstEnterpriseReportEdition Report { get; } = new StringConstEnterpriseReportEdition();


	public static StringConstEnterpriseManagerEdition Manager { get; } = new StringConstEnterpriseManagerEdition();


	public static StringConstTableDevelopEdition TableDevelop { get; } = new StringConstTableDevelopEdition();


	public static StringConstProductionCostAccountingSystemEdition ProductionCostAccountingSystem { get; } = new StringConstProductionCostAccountingSystemEdition();


	public static StringConstContractLedgerManagementSystemEdition ContractLedgerManagementSystem { get; } = new StringConstContractLedgerManagementSystemEdition();


	public static StringConstRDExpenseLedgerSystemEdition RDExpenseLedgerSystem { get; } = new StringConstRDExpenseLedgerSystemEdition();


	public static StringConstSalesOrderManagementSystemEdition SalesOrderManagementSystem { get; } = new StringConstSalesOrderManagementSystemEdition();


	public static StringConstPSISystemEdition PSIManagementSystem { get; } = new StringConstPSISystemEdition();


	public static StringConstProjectLedgerManagementSystemEdition ProjectLedgerManagementSystem { get; } = new StringConstProjectLedgerManagementSystemEdition();


	public static StringConstCustomEdition Custom { get; set; }

	public static StringConstBase GetByCode(int code)
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
			return Report;
		case 6:
			return Manager;
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
			if (Custom != null && Custom.EditionCode == code)
			{
				return Custom;
			}
			return TableDevelop;
		}
	}
}
