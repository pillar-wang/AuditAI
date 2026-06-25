namespace Auditai.PlatformResource;

public class ApplicationNameManager
{
	public static string GetApplicationName(PlatformType type)
	{
		string text = "";
		switch (type)
		{
		case PlatformType.AuditPlatform:
			return "乐其云审计协作平台" + text;
		case PlatformType.EnterpriseReportPlatform:
			return "乐其云集团报表平台" + text;
		case PlatformType.EnterpriseManagerPlatform:
			return "乐其云业务管控平台" + text;
		case PlatformType.TableDevelopPlatform:
			return "乐其云表格开发平台" + text;
		case PlatformType.ProductionCostAccountingSystem:
			return "乐其云生产成本核算系统" + text;
		case PlatformType.ContractLedgerManagementSystem:
			return "乐其云合同台账管理系统" + text;
		case PlatformType.RDExpenseLedgerSystem:
			return "乐其云研发费用台账系统" + text;
		case PlatformType.SalesOrderManagementSystem:
			return "乐其云销售订单管控系统" + text;
		case PlatformType.PSIManagementSystem:
			return "乐其云进销存管理系统" + text;
		case PlatformType.ProjectLedgerManagementSystem:
			return "乐其云项目台账管理系统" + text;
		case PlatformType.Custom:
		{
			string optionValueInSettingIniFile_String = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("client_platform_application_name", "乐其云协作平台");
			return optionValueInSettingIniFile_String + text;
		}
		default:
			return "乐其云协作平台" + text;
		}
	}
}
