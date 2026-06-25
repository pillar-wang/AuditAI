using System.Drawing;

namespace Auditai.PlatformResource;

public class PlatformColorManager
{
	public static Color GetMainColorDark(PlatformType platformType)
	{
		switch (platformType)
		{
		case PlatformType.AuditPlatform:
			return Color.FromArgb(0, 101, 189);
		case PlatformType.EnterpriseReportPlatform:
			return Color.FromArgb(16, 153, 104);
		case PlatformType.EnterpriseManagerPlatform:
		case PlatformType.TableDevelopPlatform:
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			return Color.FromArgb(0, 104, 125);
		case PlatformType.Custom:
		{
			ClientCustomizeData.Color optionValueInSettingIniFile_Color = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Color("client_platform_login_window_main_color_dark", new ClientCustomizeData.Color(byte.MaxValue, byte.MaxValue, byte.MaxValue));
			return Color.FromArgb(optionValueInSettingIniFile_Color.Red, optionValueInSettingIniFile_Color.Green, optionValueInSettingIniFile_Color.Blue);
		}
		default:
			return Color.FromArgb(0, 195, 245);
		}
	}

	public static Color GetMainColorLight(PlatformType platformType)
	{
		switch (platformType)
		{
		case PlatformType.AuditPlatform:
			return Color.FromArgb(23, 131, 217);
		case PlatformType.EnterpriseReportPlatform:
			return Color.FromArgb(56, 178, 127);
		case PlatformType.EnterpriseManagerPlatform:
		case PlatformType.TableDevelopPlatform:
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			return Color.FromArgb(56, 168, 178);
		case PlatformType.Custom:
		{
			ClientCustomizeData.Color optionValueInSettingIniFile_Color = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Color("client_platform_login_window_main_color_light", new ClientCustomizeData.Color(byte.MaxValue, byte.MaxValue, byte.MaxValue));
			return Color.FromArgb(optionValueInSettingIniFile_Color.Red, optionValueInSettingIniFile_Color.Green, optionValueInSettingIniFile_Color.Blue);
		}
		default:
			return Color.FromArgb(0, 195, 245);
		}
	}

	public static Color GetMainColorButton(PlatformType platformType)
	{
		switch (platformType)
		{
		case PlatformType.AuditPlatform:
			return Color.FromArgb(23, 131, 217);
		case PlatformType.EnterpriseReportPlatform:
			return Color.FromArgb(27, 156, 108);
		case PlatformType.EnterpriseManagerPlatform:
		case PlatformType.TableDevelopPlatform:
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			return Color.FromArgb(38, 150, 163);
		case PlatformType.Custom:
		{
			ClientCustomizeData.Color optionValueInSettingIniFile_Color = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Color("client_platform_login_window_button_color", new ClientCustomizeData.Color(byte.MaxValue, byte.MaxValue, byte.MaxValue));
			return Color.FromArgb(optionValueInSettingIniFile_Color.Red, optionValueInSettingIniFile_Color.Green, optionValueInSettingIniFile_Color.Blue);
		}
		default:
			return Color.FromArgb(0, 195, 245);
		}
	}
}
