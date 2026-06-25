using System.Diagnostics;
using Auditai.PlatformResource;

namespace Auditai.UI.Platform;

public class HelpCenterUtil
{
	public static void OpenHelpCenterHomePage()
	{
		string text = null;
		switch (Program.ClientPlatformType)
		{
		case PlatformType.AuditPlatform:
			text = "audit_help";
			break;
		case PlatformType.EnterpriseReportPlatform:
			text = "report_help";
			break;
		case PlatformType.EnterpriseManagerPlatform:
			text = "table_help";
			break;
		case PlatformType.TableDevelopPlatform:
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			text = "table_help";
			break;
		case PlatformType.Custom:
			text = ClientCustomizeData.Current.TeamType + "_help";
			break;
		default:
			text = "table_help";
			break;
		}
		OpenHelpPage(text);
	}

	public static void OpenHelpPage(string pageId)
	{
		string fileName = "about:blank";
		Process.Start(fileName);
	}

	public static string GetHelpCenterHomePage()
	{
		switch (Program.ClientPlatformType)
		{
		case PlatformType.AuditPlatform:
			return "about:blank";
		case PlatformType.EnterpriseReportPlatform:
			return "about:blank";
		case PlatformType.EnterpriseManagerPlatform:
			return "about:blank";
		case PlatformType.TableDevelopPlatform:
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			return "about:blank";
		case PlatformType.Custom:
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("help_center_home_page_url", string.Empty);
		default:
			return "about:blank";
		}
	}

	public static string GetHelpCenterIndexPage()
	{
		return "about:blank";
	}
}
