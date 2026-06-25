﻿﻿﻿﻿﻿using System;
using System.Windows.Forms;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using Auditai.PlatformResource;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class SoftwareLicenseManager
{
	public const int TRIAL_VERSION_FILE_SIZE_LIMIT_ON_IMPORT = 1;

	public const int PAY_VERSION_FILE_SIZE_LIMIT_ON_IMPORT = 50;

	public const int TRIAL_VERSION_FILE_SIZE_LIMIT_ON_UPLOAD_ATTACHMENT = 1;

	public const int PAY_VERSION_FILE_SIZE_LIMIT_ON_UPLOAD_ATTACHMENT = 50;

	public const int TRIAL_VERSION_TABLE_ROWS_LIMIT = 1000;

	public const int TRIAL_VERSION_TABLE_ALLOW_PASTE_ROWS_LIMIT = 10;

	public const int TRIAL_VERSION_TABLE_COLUMNS_LIMIT = 50;

	public const int TRIAL_VERSION_FILE_BATCH_EXPORT_COUNT_LIMIT = 3;

	public const int TRIAL_VERSION_FILE_BATCH_PRINT_COUNT_LIMIT = 3;

	public const int TRIAL_VERSION_LEDGER_DATA_ALLOW_COPY_ROWS_LIMIT = 10000;

	public const int TRIAL_VERSION_TREE_NODE_MAX_COUNT = 10;

	protected static bool IsTeamPayed()
	{
		if (UserTeam.Current.PayStatus == PayStatus.Payed)
		{
			return true;
		}
		return false;
	}

	public static bool IsFreeTeam()
	{
		if (UserTeam.Current.Level <= TeamLevel.Standard)
		{
			return true;
		}
		return false;
	}

	protected static bool IsStandardEdition()
	{
		if (UserTeam.Current.Level <= TeamLevel.Standard)
		{
			return true;
		}
		return false;
	}

	protected static bool IsProfessionalEdition()
	{
		if (UserTeam.Current.Level == TeamLevel.Professional)
		{
			return true;
		}
		return false;
	}

	protected static bool IsNoLimit()
	{
		return true;
	}

	public static string GetUnPayedLicenseDisplayName()
	{
		return "免费版";
	}

	public static string GetPayedLicenseDisplayName()
	{
		return "付费版";
	}

	public static string GetUnlimitLicenseDisplayName()
	{
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("license_display_name_on_ultimate_edition", "企业协同版");
		}
		return "企业协同版";
	}

	protected static string GetUnlimitLicenseDisplayName2()
	{
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("license_display_name_on_ultimate_edition", "企业协同版");
		}
		return "企业协同版";
	}

	public static string GetCurrentLicenseDisplayName()
	{
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (IsPayByProject())
			{
				if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
				{
					return "正式版";
				}
				return "免费版";
			}
			switch (UserTeam.Current.Level)
			{
			case TeamLevel.None:
			case TeamLevel.Standard:
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("license_display_name_on_standard_edition", "基础版");
			case TeamLevel.Professional:
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("license_display_name_on_professional_edition", "定制版");
			case TeamLevel.Ultimate:
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("license_display_name_on_ultimate_edition", "企业协同版");
			default:
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("license_display_name_on_ultimate_edition", "企业协同版");
			}
		}
		switch (UserTeam.Current.Level)
		{
		case TeamLevel.None:
		case TeamLevel.Standard:
			return "个人免费版";
		case TeamLevel.Professional:
			return "定制版";
		case TeamLevel.Ultimate:
			return "企业协同版";
		default:
			return "企业协同版";
		}
	}

	public static bool IsNeedPrintWaterMark()
	{
		if (IsPayByProject())
		{
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				return false;
			}
			return true;
		}
		if (IsStandardEdition())
		{
			return true;
		}
		return false;
	}

	public static bool IsNoTableRowsLimitLicense()
	{
		if (IsPayByProject())
		{
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				return true;
			}
			return false;
		}
		return UserTeam.Current.Level >= TeamLevel.Ultimate;
	}

	public static string GetApplicationNamePostfix()
	{
		if (Program.ClientPlatformType == PlatformType.TableDevelopPlatform || Program.ClientPlatformType == PlatformType.EnterpriseReportPlatform)
		{
			if (IsStandardEdition())
			{
				return "【个人免费版】";
			}
			if (IsProfessionalEdition())
			{
				return "【定制版】";
			}
			return "【企业协同版】";
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (IsStandardEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("application_name_postfix_on_standard_edition", string.Empty);
			}
			if (IsProfessionalEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("application_name_postfix_on_professional_edition", string.Empty);
			}
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("application_name_postfix_on_ultimate_edition", string.Empty);
		}
		return string.Empty;
	}

	public static bool IsNeedShowExpiredDate()
	{
		if (Program.ClientPlatformType == PlatformType.TableDevelopPlatform || Program.ClientPlatformType == PlatformType.EnterpriseReportPlatform || Program.ClientPlatformType == PlatformType.EnterpriseManagerPlatform)
		{
			if (IsStandardEdition())
			{
				return false;
			}
		}
		else if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (UserTeam.CurrentTeamIsPayByProject)
			{
				if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
				{
					return true;
				}
				return false;
			}
			if (IsStandardEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_expired_date_on_standard_edition", defaultValue: true);
			}
		}
		return true;
	}

	public static bool IsTemplateTileShowRightTopImage()
	{
		if (Program.ClientPlatformType == PlatformType.ProductionCostAccountingSystem || Program.ClientPlatformType == PlatformType.ContractLedgerManagementSystem || Program.ClientPlatformType == PlatformType.RDExpenseLedgerSystem || Program.ClientPlatformType == PlatformType.SalesOrderManagementSystem || Program.ClientPlatformType == PlatformType.PSIManagementSystem || Program.ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("template_tile_show_right_top_image", defaultValue: false);
		}
		return false;
	}

	public static bool IsShowHelpDocumentButton()
	{
		if (Program.ClientPlatformType == PlatformType.AuditPlatform || Program.ClientPlatformType == PlatformType.EnterpriseReportPlatform || Program.ClientPlatformType == PlatformType.TableDevelopPlatform)
		{
			return true;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_help_center_button", defaultValue: false);
		}
		return false;
	}

	public static bool IsAllowShowShareProjectButton()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (Program.ClientPlatformType == PlatformType.ProductionCostAccountingSystem || Program.ClientPlatformType == PlatformType.ContractLedgerManagementSystem || Program.ClientPlatformType == PlatformType.RDExpenseLedgerSystem || Program.ClientPlatformType == PlatformType.SalesOrderManagementSystem || Program.ClientPlatformType == PlatformType.PSIManagementSystem || Program.ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (IsStandardEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_share_project_button_on_standard_edition", defaultValue: false);
			}
			if (IsProfessionalEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_share_project_button_on_professional_edition", defaultValue: false);
			}
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_share_project_button_on_ultimate_edition", defaultValue: false);
		}
		if (IsProfessionalEdition())
		{
			return false;
		}
		return true;
	}

	public static bool IsAllowShowEmptyTemplateTile()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (UserTeam.CurrentTeamIsPayByProject)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.ProductionCostAccountingSystem || Program.ClientPlatformType == PlatformType.ContractLedgerManagementSystem || Program.ClientPlatformType == PlatformType.RDExpenseLedgerSystem || Program.ClientPlatformType == PlatformType.SalesOrderManagementSystem || Program.ClientPlatformType == PlatformType.PSIManagementSystem || Program.ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (IsStandardEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_empty_template_tile_on_standard_edition", defaultValue: false);
			}
			if (IsProfessionalEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_empty_template_tile_on_professional_edition", defaultValue: false);
			}
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_empty_template_tile_on_ultimate_edition", defaultValue: false);
		}
		if (IsProfessionalEdition())
		{
			return false;
		}
		return true;
	}

	public static bool IsAllowShowAddOrDeleteFileButton()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (Program.ClientPlatformType == PlatformType.ProductionCostAccountingSystem || Program.ClientPlatformType == PlatformType.ContractLedgerManagementSystem || Program.ClientPlatformType == PlatformType.RDExpenseLedgerSystem || Program.ClientPlatformType == PlatformType.SalesOrderManagementSystem || Program.ClientPlatformType == PlatformType.PSIManagementSystem || Program.ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (UserTeam.CurrentTeamIsPayByProject)
			{
				return false;
			}
			if (IsStandardEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_or_delete_file_button_on_standard_edition", defaultValue: true);
			}
			if (IsProfessionalEdition())
			{
				return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_or_delete_file_button_on_professional_edition", defaultValue: false);
			}
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_or_delete_file_button_on_ultimate_edition", defaultValue: true);
		}
		if (IsProfessionalEdition())
		{
			return false;
		}
		return true;
	}

	public static bool IsSharePayProjectOutOfLicenseLimit()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (!IsTeamPayed())
		{
			return true;
		}
		return false;
	}

	public static bool IsUsePayProjectOutOfLicenseLimit()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (!IsTeamPayed())
		{
			return true;
		}
		return false;
	}

	public static bool IsAddProjectOutOfLicenseLimit()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (IsStandardEdition())
			{
				return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_project_button_on_standard_edition", defaultValue: true);
			}
			if (IsProfessionalEdition())
			{
				return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_project_button_on_professional_edition", defaultValue: false);
			}
			return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_project_button_on_ultimate_edition", defaultValue: true);
		}
		if (IsProfessionalEdition())
		{
			return true;
		}
		return false;
	}

	public static bool IsDuplicateProjectOutOfLicenseLimit()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (IsStandardEdition())
			{
				return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_duplicate_project_button_on_standard_edition", defaultValue: true);
			}
			if (IsProfessionalEdition())
			{
				return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_duplicate_project_button_on_professional_edition", defaultValue: true);
			}
			return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_duplicate_project_button_on_ultimate_edition", defaultValue: true);
		}
		return false;
	}

	public static bool IsAddFileOutOfLicenseLimit()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (Program.ClientPlatformType == PlatformType.ProductionCostAccountingSystem || Program.ClientPlatformType == PlatformType.ContractLedgerManagementSystem || Program.ClientPlatformType == PlatformType.RDExpenseLedgerSystem || Program.ClientPlatformType == PlatformType.SalesOrderManagementSystem || Program.ClientPlatformType == PlatformType.PSIManagementSystem || Program.ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
		{
			return true;
		}
		if (Program.ClientPlatformType == PlatformType.Custom)
		{
			if (UserTeam.CurrentTeamIsPayByProject)
			{
				return true;
			}
			if (IsStandardEdition())
			{
				return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_or_delete_file_button_on_standard_edition", defaultValue: true);
			}
			if (IsProfessionalEdition())
			{
				return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_or_delete_file_button_on_professional_edition", defaultValue: false);
			}
			return !ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("show_add_or_delete_file_button_on_ultimate_edition", defaultValue: true);
		}
		if (IsProfessionalEdition())
		{
			return true;
		}
		return false;
	}

	public static int GetTableMaxRowsCountLimit()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return 1000000;
		}
		if (IsPayByProject())
		{
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				return 1000000;
			}
			return GetPayByProjectFreeProjectMaxTableRowsCount();
		}
		if (!IsStandardEdition())
		{
			return 1000000;
		}
		return 1000;
	}

	public static bool IsAllowDesignTicket()
	{
		if (Auditai.Model.User.Current == null)
		{
			return false;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (IsProfessionalEdition())
		{
			return false;
		}
		return true;
	}

	public static bool IsAllowEditFormula()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (IsProfessionalEdition())
		{
			return false;
		}
		return true;
	}

	public static bool IsAllowAddColumn(bool showDialog = false)
	{
		if (Auditai.Model.User.Current == null)
		{
			return true;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (IsProfessionalEdition())
		{
			return false;
		}
		if (IsTeamReachExpireDate())
		{
			if (showDialog)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{UserTeam.Current.LicenseDate:yyyy年MM月dd日}到期，无增加表格列的权限, 建议您致电官方客服电话：400-690-6500，联系购买或续期！");
			}
			return false;
		}
		return true;
	}

	public static bool IsAllowModifyTableStruct()
	{
		if (Auditai.Model.User.Current == null)
		{
			return true;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (IsProfessionalEdition())
		{
			return false;
		}
		if (IsTeamReachExpireDate())
		{
			return false;
		}
		return true;
	}

	public static bool IsAllowAddTableRows(bool showDialog = true)
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (IsPayByProjectReachExpireDate())
		{
			if (showDialog)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{Auditai.Model.Project.Current.ProjectLicenseDate:yyyy年MM月dd日}到期，无增加表格行的权限, 建议您致电官方客服电话：400-690-6500，联系购买或续期！");
			}
			return false;
		}
		if (IsTeamReachExpireDate())
		{
			if (showDialog)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{UserTeam.Current.LicenseDate:yyyy年MM月dd日}到期，无增加表格行的权限, 建议您致电官方客服电话：400-690-6500，联系购买或续期！");
			}
			return false;
		}
		return true;
	}

	protected static int GetPayByProjectFreeProjectMaxTableRowsCount()
	{
		if (ClientCustomizeData.Current != null)
		{
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Int("free_edition_max_table_rows", 1000);
		}
		return 1000;
	}

	public static bool IsCurrentProjectBeFreeProjectOnPayByProject()
	{
		if (!IsPayByProject())
		{
			return false;
		}
		if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
		{
			return false;
		}
		return true;
	}

	public static bool IsTableRowsCountOutOfLicenseLimit(Auditai.Model.Table table, int newRowsCount)
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (IsPayByProject())
		{
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				if (table.Rows.Count + newRowsCount > 1000000)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
					return true;
				}
				return false;
			}
			int payByProjectFreeProjectMaxTableRowsCount = GetPayByProjectFreeProjectMaxTableRowsCount();
			if (table.Rows.Count + newRowsCount > payByProjectFreeProjectMaxTableRowsCount)
			{
				if (IsFreeTeam())
				{
					ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeTeam("单表格的最大行数限制为" + payByProjectFreeProjectMaxTableRowsCount + "行，请联系官方客服升级为正式版，不受该限制！");
					return true;
				}
				ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeProject("单表格的最大行数限制为" + payByProjectFreeProjectMaxTableRowsCount + "行，请联系官方客服升级为正式" + StringConstBase.Current.Project + "，不受该限制！");
				return true;
			}
			return false;
		}
		if (!IsStandardEdition())
		{
			if (table.Rows.Count + newRowsCount > 1000000)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
				return true;
			}
			return false;
		}
		if (table.Rows.Count + newRowsCount > 1000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n您是" + GetCurrentLicenseDisplayName() + "用户，单表格的最大行数限制为" + 1000 + "行，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		return false;
	}

	public static bool IsTableColsCountOutOfLicenseLimit(Auditai.Model.Table table, int newColsCount)
	{
		if (IsProfessionalEdition() && newColsCount > 0)
		{
			return true;
		}
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		if (table.Columns.Count + newColsCount > 50)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n您是" + GetCurrentLicenseDisplayName() + "用户，单表格的最大列数限制为" + 50 + "列，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		return false;
	}

	public static bool ShowPromptDialogOnOutOfLicenseLimit(string msg)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n您是" + GetCurrentLicenseDisplayName() + "用户，" + msg);
		return true;
	}

	public static void ShowOutOfLicenseLimitPromptDialog(string msg)
	{
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n您是" + GetCurrentLicenseDisplayName() + "用户，" + msg);
	}

	protected static void ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeProject(string msg)
	{
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n当前" + StringConstBase.Current.Project + "为体验" + StringConstBase.Current.Project + "，" + msg);
	}

	protected static void ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeTeam(string msg)
	{
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n您是免费版用户，" + msg);
	}

	public static bool IsTableAttachmentOutOfLicenseLimit(long fileLength)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (IsStandardEdition() && fileLength > 1048576)
		{
			ShowOutOfLicenseLimitPromptDialog("上传附件的大小限制为" + 1 + "M，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		if (fileLength > 52428800)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "附件大小不能超过" + 50 + "MB");
			return true;
		}
		return false;
	}

	public static bool IsImportPDFFileOutOfLicenseLimit(double fileLength)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (IsStandardEdition() && fileLength > 1.0)
		{
			ShowOutOfLicenseLimitPromptDialog("导入PDF文件的大小限制为" + 1 + "M，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		if (fileLength > 50.0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您导入的PDF文件过大，无法为您导入！");
			return true;
		}
		return false;
	}

	public static bool IsImportImageFileOutOfLicenseLimit(double fileLength)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (IsStandardEdition() && fileLength > 1.0)
		{
			ShowOutOfLicenseLimitPromptDialog("导入图片的大小限制为" + 1 + "M，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		if (fileLength > 50.0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您导入的图片文件过大，无法为您导入！");
			return true;
		}
		return false;
	}

	public static bool IsTableRowsOutOfLicenseLimit(int rowsCount)
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			if (rowsCount > 1000000)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
				return true;
			}
			return false;
		}
		if (IsPayByProject())
		{
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				if (rowsCount > 1000000)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
					return true;
				}
				return false;
			}
			int payByProjectFreeProjectMaxTableRowsCount = GetPayByProjectFreeProjectMaxTableRowsCount();
			if (rowsCount > payByProjectFreeProjectMaxTableRowsCount)
			{
				if (IsFreeTeam())
				{
					ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeTeam("单表格的最大行数限制为" + payByProjectFreeProjectMaxTableRowsCount + "行，请联系官方客服升级为正式版，不受该限制！");
					return true;
				}
				ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeProject("单表格的最大行数限制为" + payByProjectFreeProjectMaxTableRowsCount + "行，请联系官方客服升级为正式" + StringConstBase.Current.Project + "，不受该限制！");
				return true;
			}
			return false;
		}
		if (!IsStandardEdition())
		{
			if (rowsCount > 1000000)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
				return true;
			}
			return false;
		}
		if (rowsCount > 1000)
		{
			ShowOutOfLicenseLimitPromptDialog("单表格的最大行数限制为" + 1000 + "行，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		return false;
	}

	public static bool IsTableColsOutOfLicenseLimit(int colsCount)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		if (colsCount > 50)
		{
			ShowOutOfLicenseLimitPromptDialog("单表格的最大列数限制为" + 50 + "列，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		return false;
	}

	public static bool IsPayByProject()
	{
		return UserTeam.CurrentTeamIsPayByProject;
	}

	public static bool IsPayByProjectReachExpireDate(Auditai.Model.Project project)
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (UserTeam.CurrentTeamIsPayByProject && project != null)
		{
			double totalDays = (project.ProjectLicenseDate - DateTime.Now).TotalDays;
			if (totalDays < 0.0)
			{
				return true;
			}
		}
		return false;
	}

	protected static bool IsPayByProjectReachExpireDate()
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (UserTeam.CurrentTeamIsPayByProject && Auditai.Model.Project.Current != null)
		{
			double totalDays = (Auditai.Model.Project.Current.ProjectLicenseDate - DateTime.Now).TotalDays;
			if (totalDays < 0.0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPayByProjectWillReachExpireDate(Auditai.Model.Project project)
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (UserTeam.CurrentTeamIsPayByProject && project != null)
		{
			double totalDays = (project.ProjectLicenseDate - DateTime.Now).TotalDays;
			if (totalDays < 30.0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsTeamReachExpireDate()
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return false;
		}
		if (Auditai.Model.User.Current == null)
		{
			return false;
		}
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return false;
		}
		if (UserTeam.Current != null)
		{
			double totalDays = (UserTeam.Current.LicenseDate - DateTime.Now).TotalDays;
			if (totalDays > 0.0)
			{
				return false;
			}
		}
		return true;
	}

	public static int GetCurrentLicenseAllowPasteTableRowsCount(int wantToPageRowsCount)
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return int.MaxValue;
		}
		if (!IsStandardEdition())
		{
			return int.MaxValue;
		}
		if (wantToPageRowsCount > 10)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尊敬的用户：\r\n您是" + GetCurrentLicenseDisplayName() + "用户，粘贴的最大行数限制为" + 10 + "行，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
		}
		return 10;
	}

	public static bool IsTableRowsAndColsOutOfLicenseLimit(int rowsCount, int colsCount)
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			if (rowsCount > 1000000)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
				return true;
			}
			return false;
		}
		if (IsPayByProject())
		{
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				if (rowsCount > 1000000)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
					return true;
				}
				return false;
			}
			int payByProjectFreeProjectMaxTableRowsCount = GetPayByProjectFreeProjectMaxTableRowsCount();
			if (rowsCount > payByProjectFreeProjectMaxTableRowsCount)
			{
				if (IsFreeTeam())
				{
					ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeTeam("单表格的最大行数限制为" + payByProjectFreeProjectMaxTableRowsCount + "行，请联系官方客服升级为正式版，不受该限制！");
					return true;
				}
				ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeProject("单表格的最大行数限制为" + payByProjectFreeProjectMaxTableRowsCount + "行，请联系官方客服升级为正式" + StringConstBase.Current.Project + "，不受该限制！");
				return true;
			}
			return false;
		}
		if (!IsStandardEdition())
		{
			if (rowsCount > 1000000)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "单表格的最大行数限制为" + 1000000 + "行！");
				return true;
			}
			return false;
		}
		if (rowsCount > 1000)
		{
			ShowOutOfLicenseLimitPromptDialog("单表格的最大行数限制为" + 1000 + "行，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，不受该限制！");
			return true;
		}
		return false;
	}

	public static bool IsRecycleFileOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + AppCommands.RecycleNode.Text + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsRevertHistoryDataOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + AppCommands.ManageSnapshots.Text + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsExportProjectOutOfLicenseLimit(string funName)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + funName + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsCurrentLiceneseAllowBatchExport(string functionName)
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (IsPayByProject())
		{
			if (IsFreeTeam())
			{
				ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeTeam("无使用【" + functionName + "】功能的权限，请联系官方客服升级为正式版，再使用该功能！");
				return false;
			}
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				return true;
			}
			ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeProject("无使用【" + functionName + "】功能的权限，请联系官方客服升级为正式" + StringConstBase.Current.Project + "，再使用该功能！");
			return false;
		}
		if (!IsStandardEdition())
		{
			return true;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + functionName + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return false;
	}

	public static bool IsCurrentLiceneseAllowBatchPrint(string functionName)
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (IsPayByProject())
		{
			if (IsFreeTeam())
			{
				ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeTeam("无使用【" + functionName + "】功能的权限，请联系官方客服升级为正式版，再使用该功能！");
				return false;
			}
			if (Auditai.Model.Project.Current != null && Auditai.Model.Project.Current.ProjectChargeType == ChargeType.Pay)
			{
				return true;
			}
			ShowOutOfLicenseLimitPromptDialog_PayByProjectDueToFreeProject("无使用【" + functionName + "】功能的权限，请联系官方客服升级为正式" + StringConstBase.Current.Project + "，再使用该功能！");
			return false;
		}
		if (!IsStandardEdition())
		{
			return true;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + functionName + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return false;
	}

	public static bool IsBatchExportOutOfLicenseLimit(int exportCount)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		if (exportCount <= 3)
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("【" + AppCommands.BatchExport.Text + "】功能一次仅可导出" + 3 + "个，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，批量导出文件将不受个数限制！");
		return true;
	}

	public static bool IsBatchPrintOutOfLicenseLimit(int printCount)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		if (printCount <= 3)
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("【" + AppCommands.TableBatchPrint.Text + "】功能一次仅可打印" + 3 + "个，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，批量打印文件将不受个数限制！");
		return true;
	}

	public static bool IsLedgerOneClickCollectOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + AppCommands.LedgerOneClickCollect.Text + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsValidateAllTableOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + AppCommands.ValidateAllTables.Text + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsExportLedgerOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【账套数据导出】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsPrintLedgerOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【账套数据打印】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsMergeLedgerOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【" + AppCommands.MergeLedgers.Text + "】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsOpenedLedgerCountInLicenseLimit(int count)
	{
		if (IsNoLimit())
		{
			return true;
		}
		if (!IsStandardEdition())
		{
			return true;
		}
		if (count <= 1)
		{
			return true;
		}
		ShowOutOfLicenseLimitPromptDialog("无同时打开多个账套的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return false;
	}

	public static bool IsConsolidateOutOfLicenseLimit()
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		ShowOutOfLicenseLimitPromptDialog("无使用【合并报表】功能的权限，请联系官方客服升级为" + GetUnlimitLicenseDisplayName() + "用户，再使用该功能！");
		return true;
	}

	public static bool IsLicenseAllowToCopyLedgerData(int rowsCount)
	{
		if (IsNoLimit())
		{
			return true;
		}
		if (!IsStandardEdition())
		{
			return true;
		}
		if (rowsCount > 10000)
		{
			ShowOutOfLicenseLimitPromptDialog($"【复制账套数据】功能限制为{10000}行以内，请联系官方客服升级为{GetUnlimitLicenseDisplayName()}用户，再使用该功能！");
			return false;
		}
		return true;
	}

	public static bool IsLedgerModuleEnable()
	{
		if (Program.MainForm == null || Program.MainForm.CurrentEdition == null)
		{
			return false;
		}
		return Program.MainForm.CurrentEdition.EnableLedger;
	}

	public static bool IsProjectHierarchyTreeNodesCountOutOfLimit(Func<int> getAddCountHandle = null)
	{
		if (IsNoLimit())
		{
			return false;
		}
		if (!IsStandardEdition())
		{
			return false;
		}
		int allFileNodesTotalCount = Program.MainForm.ProjectHierarchy.GetAllFileNodesTotalCount();
		if (allFileNodesTotalCount + (getAddCountHandle?.Invoke() ?? 1) > 10)
		{
			ShowOutOfLicenseLimitPromptDialog($"表格和文档的总数量限制为{10}个以内，请联系官方客服升级为{GetUnlimitLicenseDisplayName2()}用户，文件个数将不受限制！");
			return true;
		}
		return false;
	}

	public static bool IsAllowShowVariableManageMenu()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (UserTeam.CurrentTeamIsPayByProject)
		{
			return false;
		}
		return true;
	}

	public static bool IsAllowShowSystemSettingMenu()
	{
		if (Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (UserTeam.CurrentTeamIsPayByProject)
		{
			return false;
		}
		return true;
	}
}
