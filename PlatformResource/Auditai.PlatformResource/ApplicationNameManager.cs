﻿namespace Auditai.PlatformResource;

public class ApplicationNameManager
{
	public static string GetApplicationName(PlatformType type)
	{
		string text = "";
		switch (type)
		{
		case PlatformType.AuditPlatform:
			return "AuditAI云审计协作平台" + text;
		case PlatformType.EnterpriseReportPlatform:
			return "AuditAI云集团报表平台" + text;
		case PlatformType.EnterpriseManagerPlatform:
			return "AuditAI云业务管控平台" + text;
		case PlatformType.TableDevelopPlatform:
			return "AuditAI云表格开发平台" + text;
		case PlatformType.ProductionCostAccountingSystem:
			return "AuditAI云生产成本核算系统" + text;
		case PlatformType.ContractLedgerManagementSystem:
			return "AuditAI云合同台账管理系统" + text;
		case PlatformType.RDExpenseLedgerSystem:
			return "AuditAI云研发费用台账系统" + text;
		case PlatformType.SalesOrderManagementSystem:
			return "AuditAI云销售订单管控系统" + text;
		case PlatformType.PSIManagementSystem:
			return "AuditAI云进销存管理系统" + text;
		case PlatformType.ProjectLedgerManagementSystem:
			return "AuditAI云项目台账管理系统" + text;
		case PlatformType.Custom:
		{
			string optionValueInSettingIniFile_String = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("client_platform_application_name", "AuditAI云协作平台");
			return optionValueInSettingIniFile_String + text;
		}
		default:
			return "AuditAI云协作平台" + text;
		}
	}
}
