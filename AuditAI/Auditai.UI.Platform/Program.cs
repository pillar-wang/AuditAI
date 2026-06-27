﻿﻿﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Auditai.DTO;
using Auditai.Model;
using Auditai.PlatformResource;
using Auditai.SignalR;
using Auditai.UI.Controls;
using Auditai.UI.LedgerView;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

internal static class Program
{
	public static bool ApplicationExitMark;

	private static bool _IsInProcessUnhandleException;

	public static Action<Guid> UserGetTeamCallback;

	public static MainForm MainForm { get; private set; }

	public static bool HasLoggedOut { get; private set; }

	public static bool IsOnPremise { get; private set; }

	public static PlatformType ClientPlatformType { get; private set; }

	public static bool IsNeedCreateSamePlateformTeamOnLoginIn { get; set; }

	static Program()
	{
		try
		{
			// CefSharp.Core assembly not available in this project
			// CefSettings cefSettings = new CefSettings();
			// cefSettings.Locale = "zh-CN";
			// cefSettings.DisableGpuAcceleration();
			// cefSettings.RemoteDebuggingPort = 1024;
			// Cef.Initialize(cefSettings);

			// ★ 尽早注册 AssemblyLoad 事件，确保任何 C1 程序集被加载时都能执行补丁
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
		}
		catch (Exception ex)
		{
			ex.Log("系统启动时，初始化Cef控件失败");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "初始化Cef控件失败: \r\n" + ex.ToString());
			throw ex;
		}
	}

	[STAThread]
	private static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: true);
		
		// ★ 必须放在最前面：抑制 C1 评估弹窗，在任何可能加载 C1 程序集的代码之前执行
		SuppressC1EvalDialog();
		
		// ★ 新增：初始化本地存储（必须在 StartAuditaiPlatform 之前）
		Auditai.LocalDataStore.StorageRouter.Initialize();
		
		// ★ 本地模式：设置 WebApiClient 本地 API 处理器
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			SetupLocalApiHandler();
			
		}
		// ★ 新增：修复鼠标滚轮不滚动问题（WinForms 默认将滚轮消息发给焦点控件而非鼠标下方控件）
		Application.AddMessageFilter(new MouseWheelMessageFilter());
		
		FixCurrentDirectory();
		
		ParseClientInfo();
		
		InitPlatformData();
		
		Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
		ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object a, X509Certificate b, X509Chain c, SslPolicyErrors d) => true));
		ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		Application.ApplicationExit += Application_ApplicationExit;
		UserSet.Load();
		
		UserSet.InitializeDefaultTheme(GetPlatformDefaultThemeId());
		InitDefaultUserSet();
		Theme.SelectedThemeById(UserSet.Config.CurrentTheme);
		
		StartAuditaiPlatform();
		if (!ApplicationExitMark)
		{
			Application.Run();
		}
		UserSet.Save();
		ProjectInfoManager.GetInstance().Save();
	}

	private static void InitDefaultUserSet()
	{
		if (UserSet.IsCreateNew())
		{
			if (ClientPlatformType == PlatformType.EnterpriseManagerPlatform || ClientPlatformType == PlatformType.TableDevelopPlatform || ClientPlatformType == PlatformType.ProductionCostAccountingSystem || ClientPlatformType == PlatformType.ContractLedgerManagementSystem || ClientPlatformType == PlatformType.RDExpenseLedgerSystem || ClientPlatformType == PlatformType.SalesOrderManagementSystem || ClientPlatformType == PlatformType.PSIManagementSystem || ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
			{
				InitDefaultUserSet_ManagerPlatform();
			}
			else if (ClientPlatformType == PlatformType.EnterpriseReportPlatform)
			{
				InitDefaultUserSet_ReportPlatform();
			}
			else if (ClientPlatformType == PlatformType.Custom)
			{
				InitDefaultUserSet_Custom();
			}
		}
	}

	private static void InitDefaultUserSet_ManagerPlatform()
	{
		UserSet.Config.TableStyle.MainTitleHeight = 45;
		UserSet.Config.TableStyle.TitleStyle.FontSize = 14f;
		UserSet.Config.TableStyle.TitleStyle.Bold = true;
		UserSet.Config.TableStyle.TitleStyle.FontColor = Color.FromArgb(49, 133, 156);
		UserSet.Config.TableStyle.TitleStyle.FontFamily = "微软雅黑";
		UserSet.Config.TableStyle.SubTitleHeight = 30;
		UserSet.Config.TableStyle.SubTitleStyle.FontSize = 10.5f;
		UserSet.Config.TableStyle.SubTitleStyle.Bold = false;
		UserSet.Config.TableStyle.SubTitleStyle.FontColor = Color.FromArgb(64, 64, 64);
		UserSet.Config.TableStyle.SubTitleStyle.FontFamily = "微软雅黑";
		UserSet.Config.TableStyle.TableRowHeight = 30;
		UserSet.Config.TableStyle.FontStyle.FontSize = 10.5f;
		UserSet.Config.TableStyle.FontStyle.Bold = false;
		UserSet.Config.TableStyle.FontStyle.FontColor = Color.FromArgb(64, 64, 64);
		UserSet.Config.TableStyle.FontStyle.FontFamily = "微软雅黑";
	}

	private static void InitDefaultUserSet_ReportPlatform()
	{
		UserSet.Config.TableStyle.MainTitleHeight = 45;
		UserSet.Config.TableStyle.TitleStyle.FontSize = 14f;
		UserSet.Config.TableStyle.TitleStyle.Bold = true;
		UserSet.Config.TableStyle.TitleStyle.FontColor = Color.FromArgb(0, 102, 0);
		UserSet.Config.TableStyle.TitleStyle.FontFamily = "微软雅黑";
		UserSet.Config.TableStyle.SubTitleHeight = 30;
		UserSet.Config.TableStyle.SubTitleStyle.FontSize = 10.5f;
		UserSet.Config.TableStyle.SubTitleStyle.Bold = false;
		UserSet.Config.TableStyle.SubTitleStyle.FontColor = Color.FromArgb(64, 64, 64);
		UserSet.Config.TableStyle.SubTitleStyle.FontFamily = "微软雅黑";
		UserSet.Config.TableStyle.TableRowHeight = 30;
		UserSet.Config.TableStyle.FontStyle.FontSize = 10.5f;
		UserSet.Config.TableStyle.FontStyle.Bold = false;
		UserSet.Config.TableStyle.FontStyle.FontColor = Color.FromArgb(64, 64, 64);
		UserSet.Config.TableStyle.FontStyle.FontFamily = "微软雅黑";
	}

	private static Color GetClientCustomizeOptionValue_Color(ClientCustomizeData setting, string optionId, Color defaultValue)
	{
		ClientCustomizeData.Color optionValueInSettingIniFile_Color = setting.GetOptionValueInSettingIniFile_Color(optionId, new ClientCustomizeData.Color(defaultValue.R, defaultValue.G, defaultValue.B, defaultValue.A));
		return Color.FromArgb(optionValueInSettingIniFile_Color.Alpha, optionValueInSettingIniFile_Color.Red, optionValueInSettingIniFile_Color.Green, optionValueInSettingIniFile_Color.Blue);
	}

	private static void InitDefaultUserSet_Custom()
	{
		ClientCustomizeData current = ClientCustomizeData.Current;
		UserSet.Config.TableStyle.MainTitleHeight = current.GetOptionValueInSettingIniFile_Int("TableStyle.MainTitleHeight", 45);
		UserSet.Config.TableStyle.TitleStyle.FontSize = current.GetOptionValueInSettingIniFile_Float("TableStyle_TitleStyle_FontSize", 14f);
		UserSet.Config.TableStyle.TitleStyle.Bold = current.GetOptionValueInSettingIniFile_Bool("TableStyle_TitleStyle_Bold", defaultValue: true);
		UserSet.Config.TableStyle.TitleStyle.FontColor = GetClientCustomizeOptionValue_Color(current, "TableStyle_TitleStyle_FontColor", Color.FromArgb(0, 102, 0));
		UserSet.Config.TableStyle.TitleStyle.FontFamily = current.GetOptionValueInSettingIniFile_String("TableStyle_TitleStyle_FontFamily", "微软雅黑");
		UserSet.Config.TableStyle.SubTitleHeight = current.GetOptionValueInSettingIniFile_Int("TableStyle_SubTitleHeight ", 30);
		UserSet.Config.TableStyle.SubTitleStyle.FontSize = current.GetOptionValueInSettingIniFile_Float("TableStyle_SubTitleStyle_FontSize", 10.5f);
		UserSet.Config.TableStyle.SubTitleStyle.Bold = current.GetOptionValueInSettingIniFile_Bool("TableStyle_SubTitleStyle_Bold", defaultValue: false);
		UserSet.Config.TableStyle.SubTitleStyle.FontColor = GetClientCustomizeOptionValue_Color(current, "TableStyle_SubTitleStyle_FontColor", Color.FromArgb(64, 64, 64));
		UserSet.Config.TableStyle.SubTitleStyle.FontFamily = current.GetOptionValueInSettingIniFile_String("TableStyle_SubTitleStyle_FontFamily", "微软雅黑");
		UserSet.Config.TableStyle.TableRowHeight = current.GetOptionValueInSettingIniFile_Int("TableStyle_TableRowHeight", 30);
		UserSet.Config.TableStyle.FontStyle.FontSize = current.GetOptionValueInSettingIniFile_Float("TableStyle_FontStyle_FontSize", 10.5f);
		UserSet.Config.TableStyle.FontStyle.Bold = current.GetOptionValueInSettingIniFile_Bool("TableStyle_FontStyle_Bold", defaultValue: false);
		UserSet.Config.TableStyle.FontStyle.FontColor = GetClientCustomizeOptionValue_Color(current, "TableStyle_FontStyle_FontColor", Color.FromArgb(64, 64, 64));
		UserSet.Config.TableStyle.FontStyle.FontFamily = current.GetOptionValueInSettingIniFile_String("TableStyle_FontStyle_FontFamily", "微软雅黑");
	}

	public static int GetCurrentPlatformSupporterTeamType()
	{
		return ClientPlatformType switch
		{
			PlatformType.AuditPlatform => AppEditions.Audit.Code, 
			PlatformType.EnterpriseReportPlatform => AppEditions.EnterpriseReport.Code, 
			PlatformType.EnterpriseManagerPlatform => AppEditions.EnterpriseManager.Code, 
			PlatformType.TableDevelopPlatform => AppEditions.TableDevelop.Code, 
			PlatformType.ProductionCostAccountingSystem => AppEditions.ProductionCostAccountingSystem.Code, 
			PlatformType.ContractLedgerManagementSystem => AppEditions.ContractLedgerManagementSystem.Code, 
			PlatformType.RDExpenseLedgerSystem => AppEditions.RDExpenseLedgerSystem.Code, 
			PlatformType.SalesOrderManagementSystem => AppEditions.SalesOrderManagementSystem.Code, 
			PlatformType.PSIManagementSystem => AppEditions.PSIManagementSystem.Code, 
			PlatformType.ProjectLedgerManagementSystem => AppEditions.ProjectLedgerManagementSystem.Code, 
			PlatformType.Custom => AppEditions.CustomSystem.Code, 
			_ => -1, 
		};
	}

	private static void StartAuditaiPlatform()
	{
		
		FormulaEvaluationVisitor formulaEvaluationVisitor = new FormulaEvaluationVisitor(null);
		StringConstBase.Current = StringConstEditions.Audit;
		InitAppEditions();
		if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			CreateOrRestoreUserAndTeam();
		}
		StartAuditaiPlatform();
		static void CreateOrRestoreUserAndTeam()
		{
			// 注意：此方法仅在非本地模式（云端模式）下调用，用于创建/恢复用户和团队信息。
			// 本地模式下的用户/团队由 LocalDataStore.Initialize() 创建。
			Guid teamId = Guid.NewGuid();
			UserTeam.Current = new UserTeam
			{
				Id = teamId,
				Name = "本地团队",
				Type = AppEditions.Audit.Code,
				LicenseDate = DateTime.Now.AddYears(10)
			};
			UserTeam.Teams = new List<UserTeam>();
			UserTeam.Teams.Add(UserTeam.Current);
			UserTeam.CurrentTeamIsPayByProject = true;
			Auditai.Model.User.Current = new Auditai.Model.User
			{
				Id = 1,
				Name = "管理员",
				TelPhone = "13800138000",
				TeamId = teamId,
				IsTeamAdmin = true,
				IsSystemSupporter = true
			};
		}
		static void StartAuditaiPlatform()
		{
			
			CreateMainForm();
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode)
			{
				if (!UserTeam.CurrentTeamIsPayByProject)
				{
					double totalDays = (UserTeam.Current.LicenseDate - DateTime.Now).TotalDays;
					if (totalDays < 0.0)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品已于{UserTeam.Current.LicenseDate:yyyy年MM月dd日}到期，无法新建及同步{StringConstBase.Current.Project}，您可致电官方客服电话：400-690-6500，联系购买或续期！");
					}
					else if (totalDays < 30.0)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"尊敬的用户：\r\n您的产品将于{UserTeam.Current.LicenseDate:yyyy年MM月dd日}到期，建议您致电官方客服电话：400-690-6500，联系购买或续期！");
					}
				}
				int type = UserTeam.Current.Type;
				StringConstBase.Current = StringConstEditions.GetByCode(type);
				MainForm.CurrentEdition = AppEditions.GetByCode(type);
				UserSet.Config.AppEdition = type;
				UserSet.InitializeForEdition(MainForm.CurrentEdition.SubTitleDefaultRows, MainForm.CurrentEdition.EnableLedger);
			}
			else
			{
				// 本地模式直接设置
				StringConstBase.Current = StringConstEditions.Audit;
				MainForm.CurrentEdition = AppEditions.Audit;
				UserSet.Config.AppEdition = AppEditions.Audit.Code;
				UserSet.InitializeForEdition(MainForm.CurrentEdition.SubTitleDefaultRows, MainForm.CurrentEdition.EnableLedger);
			}
			AppCommands.TeamUsers.Button.Text = (Auditai.Model.User.Current.IsTeamAdmin ? "同事管理" : "我的同事");
			MainForm.RibbonAdded = true;
			MainForm.View.Icon = IconGenerator.LoadFromFile(ApplicationIconManger.GetApplicationIconFilePath(ClientPlatformType));
			LedgerViewer.LicenseCheckHandleOnCopyLedgerData = SoftwareLicenseManager.IsLicenseAllowToCopyLedgerData;
			LedgerViewer.LicenseCheckHandleIsOpenedLedgerCountInLimit = SoftwareLicenseManager.IsOpenedLedgerCountInLicenseLimit;
			LedgerViewer.IsAuditPlatform = ClientPlatformType == PlatformType.AuditPlatform;
			frmTableCollect2.LicenseCheckHandleOnCopyLedgerData = SoftwareLicenseManager.IsLicenseAllowToCopyLedgerData;
			FormProjectManage formProjectManage = new FormProjectManage();
			if (formProjectManage.ShowDialog() == DialogResult.OK)
			{
				MainForm.View.Show();
			}
			else
			{
				ApplicationExit();
			}
		}
	}

	private static string GetPlatformDefaultThemeId()
	{
		switch (ClientPlatformType)
		{
		case PlatformType.AuditPlatform:
			return "0";
		case PlatformType.EnterpriseManagerPlatform:
		case PlatformType.TableDevelopPlatform:
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			return "1";
		case PlatformType.EnterpriseReportPlatform:
			return "2";
		case PlatformType.Custom:
			return ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("default_theme_id", "1");
		default:
			return "3";
		}
	}

	public static void ApplicationExit()
	{
		ApplicationExitMark = true;
		Application.Exit();
	}

	internal static void CreateMainForm()
	{
		MainForm = new MainForm();
	}

	public static void LaunchCrawler()
	{
		string text = Path.Combine(Application.StartupPath, "AuditAI采数器", "AuditAI采数器.exe");
		if (!File.Exists(text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "找不到程序文件");
			return;
		}
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo(text)
		{
			WorkingDirectory = Path.Combine(Application.StartupPath, "AuditAI采数器"),
			UseShellExecute = false
		};
		try
		{
			process.Start();
		}
		catch (Win32Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	internal static Task Logout()
	{
		if (HasLoggedOut)
		{
			return Task.CompletedTask;
		}
		HasLoggedOut = true;
		return Task.CompletedTask;
	}

	public static void ManageUsers()
	{
		new dlgTeamUserManagement().ShowDialog();
	}

	public static bool CheckUpdaterExeExist(bool isExistApplication)
	{
		string location = Assembly.GetExecutingAssembly().Location;
		string path = Path.Combine(Path.GetDirectoryName(location), "auditaiupdater.exe");
		if (!File.Exists(path))
		{
			frmDownloadInstallPackage frmDownloadInstallPackage2 = new frmDownloadInstallPackage(isExistApplication);
			frmDownloadInstallPackage2.ShowDialog();
			if (isExistApplication)
			{
				Application.Exit();
			}
			return false;
		}
		return true;
	}

	private static void Application_ApplicationExit(object sender, EventArgs e)
	{
	}

	private static string GetOpeningProjectDebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			if (Auditai.Model.Project.Current != null)
			{
				stringBuilder.Append("当前打开的项目: " + Auditai.Model.Project.Current.Name + ", ID=" + Auditai.Model.Project.Current.Id.ToString("D") + "\n");
			}
			if (MainForm != null)
			{
				TableEditor createdTableEditor = MainForm.GetCreatedTableEditor();
				if (createdTableEditor == null)
				{
					stringBuilder.Append("TableEditor对象尚未生成!\n");
				}
				else if (createdTableEditor.Table == null)
				{
					stringBuilder.Append("TableEditor尚未打开任何表格");
				}
				else
				{
					stringBuilder.Append($"当前打开的表格: {createdTableEditor.Table.TreeNode?.Name}, ID={createdTableEditor.Table.Id}\n");
				}
			}
		}
		catch (Exception)
		{
		}
		return stringBuilder.ToString();
	}

	private static string GetAppVersionInfo()
	{
		try
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			return "AppVersion:" + version.ToString() + "\n";
		}
		catch
		{
			return "AppVersion:\n";
		}
	}

	private static string GetAppServerInfo()
	{
		try
		{
			string text = System.Configuration.ConfigurationManager.AppSettings.Get("AppServer");
			if (!string.IsNullOrWhiteSpace(text))
			{
				return "AppServer:" + text + "\n";
			}
			return string.Empty;
		}
		catch
		{
			return "";
		}
	}

	private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		string rootStatckTrace = null;
		if (_IsInProcessUnhandleException)
		{
			return;
		}
		try
		{
			_IsInProcessUnhandleException = true;
			Exception ex = (Exception)e.ExceptionObject;
			if (ex is TargetInvocationException ex2)
			{
				ex = ex2.InnerException;
			}
			string exceptStr = GetOpeningProjectDebugInfo() + $"UserId:\t{Auditai.Model.User.Current?.Id}\n{GetAppVersionInfo()}{GetAppServerInfo()}\n{ex}";
			Exception innerException = ex.InnerException;
			if (innerException != null)
			{
				while (innerException.InnerException != null)
				{
					innerException = innerException.InnerException;
				}
				rootStatckTrace = "\r\n根异常的堆栈信息:" + innerException.ToString();
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "程序因发生异常将强行关闭，您可稍候再次重启！\r\n" + exceptStr + rootStatckTrace);
		}
		catch (Exception exception)
		{
			exception.Log();
			if (rootStatckTrace != null)
			{
				try
				{
					new Exception(rootStatckTrace).Log("上报异常数据时发生了未预期的异常");
				}
				catch (Exception)
				{
				}
			}
		}
		finally
		{
			_IsInProcessUnhandleException = false;
		}
		try
		{
			_IsInProcessUnhandleException = true;
			await Logout();
		}
		catch (Exception)
		{
		}
		finally
		{
			_IsInProcessUnhandleException = false;
		}
	}

	public static async Task<List<UserTeam>> GetUserTeams(bool withNotice = true)
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return UserTeam.Teams ?? new List<UserTeam>();
		}
		try
		{
			int clientSupportTeamType = GetCurrentPlatformSupporterTeamType();
			List<UserTeam> teams = new List<UserTeam>();
			foreach (JToken item in await WebApiClient.GetUserTeams())
			{
				if (UserGetTeamCallback != null)
				{
					UserGetTeamCallback(Guid.Parse(item.Value<string>("teamId")));
				}
				if (item.Value<int>("type") == clientSupportTeamType)
				{
					int num = item.Value<int>("payStatus");
					teams.Add(new UserTeam
					{
						Id = Guid.Parse(item.Value<string>("teamId")),
						ManagerId = item.Value<long>("managerId"),
						Name = item.Value<string>("teamName"),
						Type = item.Value<int>("type"),
						PayStatus = num switch
						{
							2 => PayStatus.Free, 
							1 => PayStatus.Payed, 
							_ => PayStatus.Trial, 
						},
						LicenseDate = item.Value<DateTime>("licenseDate"),
						Level = (TeamLevel)item.Value<int>("Level")
					});
				}
			}
			UserTeam.Teams = teams;
			return teams;
		}
		catch (NormalException ex)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			return null;
		}
		catch (TimeoutException ex2)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
			return null;
		}
		catch (ServerException ex3)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.ToString());
			}
			return null;
		}
		catch (HttpRequestException ex4)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.InnerException.Message);
			}
			return null;
		}
	}

	public static async Task<bool> OpenTeam(Guid teamId, bool withNotice = true)
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return true;
		}
		UserTeam userTeam = UserTeam.Teams.FirstOrDefault((UserTeam t) => t.Id == teamId);
		if (userTeam == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开失败，您不在该组织中，请刷新组织列表");
			return false;
		}
		if (!IsClientPlatformMatchToTeamType(userTeam))
		{
			ShowAbleOpenTeamClientPlatformMessage(userTeam);
			return false;
		}
		try
		{
			JObject jObject = await WebApiClient.UpdateCurrentTeam(teamId);
			Auditai.Model.User current = Auditai.Model.User.Current;
			UserTeam.Current = userTeam;
			current.TeamId = teamId;
			current.LicenseDate = userTeam.LicenseDate;
			current.IsTeamAdmin = (bool)jObject["IsTeamAdmin"];
			current.IsLicenseOutOfDate = !IsOnPremise && (bool)jObject["IsLicenseOutOfDate"];
			if (ClientPlatformType == PlatformType.Custom && !IsOnPremise)
			{
				UserTeam.CurrentTeamIsPayByProject = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("is_pay_by_project", defaultValue: true);
			}
			await ChatManager.Login();
			await ChatManager.UpdateTeamMember();
			return true;
		}
		catch (NormalException ex)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			return false;
		}
		catch (TimeoutException ex2)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
			return false;
		}
		catch (ServerException ex3)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.ToString());
			}
			return false;
		}
		catch (HttpRequestException ex4)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.InnerException.Message);
			}
			return false;
		}
	}

	private static void ShowAbleOpenTeamClientPlatformMessage(UserTeam userTeam)
	{
		string text = "";
		if (userTeam.Type == AppEditions.Audit.Code || userTeam.Type == AppEditions.Tax.Code)
		{
			text = AppEditions.Audit.PlatformName;
		}
		else if (userTeam.Type == AppEditions.EnterpriseReport.Code)
		{
			text = AppEditions.EnterpriseReport.PlatformName;
		}
		else if (userTeam.Type == AppEditions.EnterpriseReport.Code)
		{
			text = AppEditions.EnterpriseReport.PlatformName;
		}
		else
		{
			for (int i = 0; i < AppEditions.Editions.Count; i++)
			{
				if (userTeam.Type == AppEditions.Editions[i].Code)
				{
					text = AppEditions.Editions[i].PlatformName;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前客户端已不支持该类型的组织，请致电官方客服电话：400-690-6500 进行处理。");
				return;
			}
		}
		string text2 = ((UserTeam.Teams.Count > 1) ? ("当前客户端不支持该组织，请登录 " + text + " 进行操作。") : ("当前客户端不支持用户所在的组织，请登录 " + text + " 进行操作。"));
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text2);
	}

	public static bool IsClientPlatformMatchToTeamType(UserTeam userTeam, bool isAllowUseSystemSupporterRule = true)
	{
		if (ClientPlatformType == PlatformType.AuditPlatform)
		{
			if (isAllowUseSystemSupporterRule && Auditai.Model.User.Current.IsSystemSupporter)
			{
				return true;
			}
			if (userTeam.Type == AppEditions.Audit.Code || userTeam.Type == AppEditions.Tax.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.EnterpriseReportPlatform)
		{
			if (userTeam.Type == AppEditions.EnterpriseReport.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.EnterpriseManagerPlatform)
		{
			if (userTeam.Type == AppEditions.EnterpriseManager.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.TableDevelopPlatform)
		{
			if (userTeam.Type == AppEditions.TableDevelop.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.ProductionCostAccountingSystem)
		{
			if (userTeam.Type == AppEditions.ProductionCostAccountingSystem.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.ContractLedgerManagementSystem)
		{
			if (userTeam.Type == AppEditions.ContractLedgerManagementSystem.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.RDExpenseLedgerSystem)
		{
			if (userTeam.Type == AppEditions.RDExpenseLedgerSystem.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.SalesOrderManagementSystem)
		{
			if (userTeam.Type == AppEditions.SalesOrderManagementSystem.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.PSIManagementSystem)
		{
			if (userTeam.Type == AppEditions.PSIManagementSystem.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
		{
			if (userTeam.Type == AppEditions.ProjectLedgerManagementSystem.Code)
			{
				return true;
			}
			return false;
		}
		if (ClientPlatformType == PlatformType.Custom)
		{
			if (userTeam.Type == AppEditions.CustomSystem.Code)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static async Task<bool> ExitTeam(string userName, Guid teamId, bool withNotice = true)
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return true;
		}
		_ = 1;
		try
		{
			Auditai.Model.User current = Auditai.Model.User.Current;
			await WebApiClient.RemoveUserFromTeam(userName, teamId);
			if (userName == current.UserName && current.TeamId == teamId)
			{
				current.TeamId = Guid.Empty;
				current.IsTeamAdmin = false;
				current.LicenseDate = DateTime.MaxValue;
				await SignalRClient.ChangeTeamMember(current.Id.ToString(), teamId.ToString(), current.TeamId.ToString());
			}
			return true;
		}
		catch (NormalException ex)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			return false;
		}
		catch (TimeoutException ex2)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
			return false;
		}
		catch (ServerException ex3)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.ToString());
			}
			return false;
		}
		catch (HttpRequestException ex4)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.InnerException.Message);
			}
			return false;
		}
	}

	public static async Task<bool> DismissTeam(bool withNotice = true)
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return true;
		}
		_ = 1;
		try
		{
			Auditai.Model.User current = Auditai.Model.User.Current;
			Guid oldTeamId = current.TeamId;
			await WebApiClient.DismissTeam();
			current.TeamId = Guid.Empty;
			current.IsTeamAdmin = false;
			current.LicenseDate = DateTime.MaxValue;
			await SignalRClient.ChangeTeamMember(current.Id.ToString(), oldTeamId.ToString(), current.TeamId.ToString());
			Application.Exit();
			return true;
		}
		catch (NormalException ex)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			return false;
		}
		catch (TimeoutException ex2)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
			return false;
		}
		catch (ServerException ex3)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.ToString());
			}
			return false;
		}
		catch (HttpRequestException ex4)
		{
			if (withNotice)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.InnerException.Message);
			}
			return false;
		}
	}

	private static void FixCurrentDirectory()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		Environment.CurrentDirectory = Path.GetDirectoryName(executingAssembly.Location);
	}

	private static void ParseClientInfo()
	{
		ClientPlatformType = PlatformType.AuditPlatform;
	}

	private static void InitPlatformData()
	{
		try
		{
			if (ClientPlatformType == PlatformType.EnterpriseManagerPlatform || ClientPlatformType == PlatformType.TableDevelopPlatform || ClientPlatformType == PlatformType.ProductionCostAccountingSystem || ClientPlatformType == PlatformType.ContractLedgerManagementSystem || ClientPlatformType == PlatformType.RDExpenseLedgerSystem || ClientPlatformType == PlatformType.SalesOrderManagementSystem || ClientPlatformType == PlatformType.PSIManagementSystem || ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
			{
				DeleteAuditaiCaishuQiDir();
			}
			else if (ClientPlatformType == PlatformType.Custom)
			{
				InitCustomPlatformData();
				if (ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("delete_auditai_cai_shu_qi_folder", defaultValue: true))
				{
					DeleteAuditaiCaishuQiDir();
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static void InitCustomPlatformData()
	{
		try
		{
			InitCustomStringConst();
		}
		catch (Exception ex)
		{
			ex.Log("客户端配置数据初始化失败");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, ex.ToString(), MessageBoxButtons.OK, "系统配置参数错误!");
			Application.Exit();
		}
		static void InitCustomStringConst()
		{
			ClientCustomizeData current = ClientCustomizeData.Current;
			StringConstCustomEdition stringConstCustomEdition = new StringConstCustomEdition(current.TeamType);
			stringConstCustomEdition.DisplayFormat_AppName = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayFormat_AppName", stringConstCustomEdition.DisplayFormat_AppName);
			stringConstCustomEdition.DisplayName_Auditee = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_Auditee", stringConstCustomEdition.DisplayName_Auditee);
			stringConstCustomEdition.DisplayName_TableNote = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_TableNote", stringConstCustomEdition.DisplayName_TableNote);
			stringConstCustomEdition.DisplayName_Project = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_Project", stringConstCustomEdition.DisplayName_Project);
			stringConstCustomEdition.DisplayName_Manager = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_Manager", stringConstCustomEdition.DisplayName_Manager);
			stringConstCustomEdition.DisplayName_Assistant = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_Assistant", stringConstCustomEdition.DisplayName_Assistant);
			stringConstCustomEdition.DisplayName_Template = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_Template", stringConstCustomEdition.DisplayName_Template);
			stringConstCustomEdition.DisplayName_SelectTemplate = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_SelectTemplate", stringConstCustomEdition.DisplayName_SelectTemplate);
			stringConstCustomEdition.DisplayName_NotUseTemplate = current.GetOptionValueInSettingIniFile_String("ConstString_DisplayName_NotUseTemplate", stringConstCustomEdition.DisplayName_NotUseTemplate);
			StringConstEditions.Custom = stringConstCustomEdition;
		}
	}

	private static void InitAppEditions()
	{
		if (ClientPlatformType != PlatformType.Custom)
		{
			return;
		}
		try
		{
			ClientCustomizeData current = ClientCustomizeData.Current;
			string optionValueInSettingIniFile_String = current.GetOptionValueInSettingIniFile_String("client_platform_application_name", "AuditAI");
			AppEditionCustomSystem appEditionCustomSystem = (AppEditions.CustomSystem = new AppEditionCustomSystem(current.TeamType, optionValueInSettingIniFile_String));
			AppEditions.Editions.Add(appEditionCustomSystem);
			appEditionCustomSystem.Team_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\team_icon.png")));
			appEditionCustomSystem.Current_Project_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\current_project_icon.png")));
			appEditionCustomSystem.Current_System_Template_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\current_system_template_icon.png")));
			appEditionCustomSystem.Current_Custom_Template_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\current_custom_template_icon.png")));
			appEditionCustomSystem.Project_Tile_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\project_tile_icon.png")));
			appEditionCustomSystem.System_Template_Tile_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\system_template_tile_icon.png")));
			appEditionCustomSystem.Vip_System_Template_Tile_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\vip_system_template_tile_icon.png")));
			appEditionCustomSystem.Custom_Template_Tile_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\custom_template_tile_icon.png")));
			appEditionCustomSystem.Use_Empty_Template_Tile_Icon = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\use_empty_template_tile_icon.png")));
			byte[] fileData = ClientCustomizeData.Current.GetFileData("image\\template_tile_corner_icon_payed.png");
			if (fileData != null)
			{
				appEditionCustomSystem.Payed_Template_Tile_Corner_Icon = System.Drawing.Image.FromStream(new MemoryStream(fileData));
			}
			fileData = ClientCustomizeData.Current.GetFileData("image\\template_tile_corner_icon_unpay.png");
			if (fileData != null)
			{
				appEditionCustomSystem.UnPay_Template_Tile_Corner_Icon = System.Drawing.Image.FromStream(new MemoryStream(fileData));
			}
		}
		catch (Exception ex)
		{
			ex.Log("客户端配置数据初始化失败");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, ex.ToString(), MessageBoxButtons.OK, "系统配置参数错误!");
			Application.Exit();
		}
	}

	private static void DeleteAuditaiCaishuQiDir()
	{
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		DeleteDirectory(Path.Combine(directoryName, "AuditAI采数器"));
		DeleteDirectory(Path.Combine(directoryName, "演示账套"));
	}

	private static void DeleteDirectory(string path)
	{
		if (Directory.Exists(path))
		{
			try
			{
				Directory.Delete(path, recursive: true);
			}
			catch
			{
			}
		}
	}

	// ★ 新增：C1 评估版抑制方案（双层防护）
	// 方案一：通过 AssemblyLoad 事件 patch C1 组件的许可证字段
	// 方案二：Win32 窗口扫描作为后备方案

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	private const uint WM_CLOSE = 0x0010;

	/// <summary>
	/// 设置 WebApiClient 的本地 API 处理器，所有 HTTP 请求走本地逻辑
	/// </summary>
	private static void SetupLocalApiHandler()
	{
		WebApiClient.LocalApiHandler = async (url) =>
		{
			return await HandleLocalApi(url);
		};
	}

	private static async Task<Stream> HandleLocalApi(string url)
	{
		byte[] jsonBytes;

		// ===== 字典同步 API =====
		if (url.Contains("TableCollectDic"))
		{
			var result = await Auditai.LocalDataStore.StorageRouter.GetTableCollectDic();
			jsonBytes = Encoding.UTF8.GetBytes(result.ToString());
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("CellCollectDic"))
		{
			var result = await Auditai.LocalDataStore.StorageRouter.GetCellCollectDic();
			jsonBytes = Encoding.UTF8.GetBytes(result.ToString());
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("LedgerValidateDic"))
		{
			var result = await Auditai.LocalDataStore.StorageRouter.GetLedgerValidateDic();
			jsonBytes = Encoding.UTF8.GetBytes(result.ToString());
			return new MemoryStream(jsonBytes);
		}

		// ===== 项目管理 API =====
		if (url.Contains("Project/GetProjects"))
		{
			var projects = await Auditai.LocalDataStore.StorageRouter.GetProjects();
			jsonBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(projects));
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/GetTemplates"))
		{
			var templates = await Auditai.LocalDataStore.StorageRouter.GetTemplates();
			jsonBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(templates));
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/OpenProject"))
		{
			var result = await Auditai.LocalDataStore.StorageRouter.OpenProject(Guid.Empty); // projectId 从 URL 解析
			var jo = new JObject { ["Item1"] = result.Item1, ["Item2"] = result.Item2 };
			jsonBytes = Encoding.UTF8.GetBytes(jo.ToString());
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/GetUserTeams"))
		{
			var result = await Auditai.LocalDataStore.StorageRouter.GetUserTeams();
			jsonBytes = Encoding.UTF8.GetBytes(result.ToString());
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/GetTeamUsersWithPic"))
		{
			var users = await Auditai.LocalDataStore.StorageRouter.GetTeamUsersWithPic();
			jsonBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(users));
			return new MemoryStream(jsonBytes);
		}

		// ===== Push/Pull 同步 API：本地模式直接返回"成功" =====
		if (url.Contains("PushProject") || url.Contains("PushProjectQuick"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{\"Result\":\"ok\"}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("PullProject"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{\"Result\":\"ok\"}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("PushTable") || url.Contains("PushTableQuick"))
		{
			var result = await Auditai.LocalDataStore.StorageRouter.PushTable(null);
			jsonBytes = Encoding.UTF8.GetBytes(result?.ToString() ?? "{\"Result\":\"ok\"}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("PullTable"))
		{
			// 返回空的 PullTable protobuf
			return new MemoryStream(new byte[0]);
		}
		if (url.Contains("PushDocument") || url.Contains("PushDocumentQuick"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{\"Result\":\"ok\"}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("PullDocument"))
		{
			return new MemoryStream(new byte[0]);
		}
		if (url.Contains("PushImage") || url.Contains("PullImage") ||
			url.Contains("PushPdf") || url.Contains("PullPdf"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{\"Result\":\"ok\"}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("RevertTable") || url.Contains("RevertDocument") ||
			url.Contains("GetTableRevertDiff") || url.Contains("GetDocumentRevertDiff"))
		{
			return new MemoryStream(new byte[0]);
		}

		// ===== 版本历史 API：返回空数组 =====
		if (url.Contains("Timeline") || url.Contains("Versions") || url.Contains("Columns"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("[]");
			return new MemoryStream(jsonBytes);
		}

		// ===== 用户/团队管理 API =====
		if (url.Contains("User/AccountLogin") || url.Contains("User/WechatLogin") ||
			url.Contains("User/QQLogin") || url.Contains("User/AccountLoginBySMS"))
		{
			// 本地登录：返回默认用户
			var token = new JObject { ["TokenValue"] = "local-token", ["ExpireTime"] = DateTime.MaxValue.ToString() };
			var user = new JObject { ["Id"] = 1, ["Name"] = "管理员", ["UserName"] = "admin", ["Role"] = 4 };
			var result = new JArray { token, user };
			jsonBytes = Encoding.UTF8.GetBytes(result.ToString());
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("User/UpdateToken"))
		{
			var token = new JObject { ["TokenValue"] = "local-token", ["ExpireTime"] = DateTime.MaxValue.ToString() };
			jsonBytes = Encoding.UTF8.GetBytes(token.ToString());
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("User/GetUserById") || url.Contains("User/GetUserByName"))
		{
			var user = new JObject { ["Id"] = 1, ["Name"] = "管理员", ["UserName"] = "admin" };
			jsonBytes = Encoding.UTF8.GetBytes(user.ToString());
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("User/ClientQuit"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{}");
			return new MemoryStream(jsonBytes);
		}

		// ===== 项目操作 API =====
		if (url.Contains("Project/CreateProject") || url.Contains("Project/DeleteProject"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/UpdateProject") || url.Contains("Project/UpdateProjectVersion"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{\"Result\":\"ok\"}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/GetProjectDto"))
		{
			var projects = await Auditai.LocalDataStore.StorageRouter.GetProjects();
			var first = projects.FirstOrDefault();
			jsonBytes = Encoding.UTF8.GetBytes(first != null ? Newtonsoft.Json.JsonConvert.SerializeObject(first) : "{}");
			return new MemoryStream(jsonBytes);
		}

		// ===== 团队操作 API =====
		if (url.Contains("Project/CreateTeam") || url.Contains("Project/UpdateCurrentTeam"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/GetTeamUsers"))
		{
			var users = await Auditai.LocalDataStore.StorageRouter.GetTeamUsersWithPic();
			jsonBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(users));
			return new MemoryStream(jsonBytes);
		}

		// ===== 文件上传/下载 API =====
		if (url.Contains("Project/UploadFile"))
		{
			jsonBytes = Encoding.UTF8.GetBytes("{}");
			return new MemoryStream(jsonBytes);
		}
		if (url.Contains("Project/DownloadFile"))
		{
			return new MemoryStream(new byte[0]);
		}
		if (url.Contains("Project/PullProjectDirect"))
		{
			return new MemoryStream(new byte[0]);
		}

		// ===== 兜底：返回空 JSON =====
		
		jsonBytes = Encoding.UTF8.GetBytes("{}");
		return new MemoryStream(jsonBytes);
	}

	private static void SuppressC1EvalDialog()
	{
		// 预加载所有 C1 程序集，确保在组件使用前就完成 patch
		// 这样可以避免首次使用时弹窗（AssemblyLoad 事件在静态构造函数中已注册）
		// 注意：AssemblyLoad 事件已在静态构造函数中注册，此处不再重复注册

		try
		{
			string appDir = AppDomain.CurrentDomain.BaseDirectory;
			var c1Assemblies = System.IO.Directory.GetFiles(appDir, "C1.*.dll")
				.Concat(System.IO.Directory.GetFiles(appDir, "C1.Win.*.dll"))
				.Concat(System.IO.Directory.GetFiles(appDir, "C1.C1*.dll"))
				.Distinct()
				.ToArray();
			foreach (var dllPath in c1Assemblies)
			{
				try
				{
					var asm = Assembly.LoadFrom(dllPath);
					PatchC1LicenseFields(asm);
				}
				catch { }
			}
		}
		catch { }

		// Patch 已经加载的 C1 程序集
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			string name = asm.GetName().Name;
			if (name.StartsWith("C1.Win") || name.StartsWith("C1.C1"))
			{
				PatchC1LicenseFields(asm);
			}
		}

		// 方案二：后台线程扫描窗口作为后备方案
		var thread = new System.Threading.Thread(() =>
		{
			uint currentPid = (uint)Process.GetCurrentProcess().Id;
			while (true)
			{
				try
				{
					EnumWindows((hWnd, lParam) =>
					{
						try
						{
							GetWindowThreadProcessId(hWnd, out uint pid);
							if (pid != currentPid) return true;

							var sb = new System.Text.StringBuilder(256);
							GetWindowText(hWnd, sb, 256);
							string title = sb.ToString();

							if (title.Contains("ComponentOne") ||
								title.Contains("评估") ||
								title.IndexOf("Evaluation", StringComparison.OrdinalIgnoreCase) >= 0 ||
								title.IndexOf("Trial", StringComparison.OrdinalIgnoreCase) >= 0 ||
								title.IndexOf("License", StringComparison.OrdinalIgnoreCase) >= 0 ||
								title.IndexOf("About", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								// 发送关闭消息
								SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
								// 额外发送 WM_DESTROY 确保窗口关闭
								SendMessage(hWnd, 0x0002, IntPtr.Zero, IntPtr.Zero);
							}
						}
						catch { }
						return true;
					}, IntPtr.Zero);
				}
				catch { }
				System.Threading.Thread.Sleep(300);
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
	{
		try
		{
			Assembly loadedAssembly = args.LoadedAssembly;
			string assemblyName = loadedAssembly.GetName().Name;

			// 处理 C1 相关的程序集
			if (assemblyName.StartsWith("C1.Win") || assemblyName.StartsWith("C1.C1"))
			{
				PatchC1LicenseFields(loadedAssembly);
			}

			// 处理 TXTextControl 程序集：patch 许可证验证
			if (assemblyName.StartsWith("TXTextControl"))
			{
				PatchTXTextControlLicense(loadedAssembly);
			}
		}
		catch (Exception ex)
		{
			ex.Log("程序集许可证 Patch 失败");
		}
	}

	/// <summary>
	/// 确保 TXTextControl 许可证已注入到 RuntimeLicenseContext。
	/// 每次创建 TXTextControl 控件之前调用，避免因 AssemblyLoad 时序问题导致授权失败。
	/// </summary>
	public static void EnsureTXTextControlLicense()
	{
		PreloadLicensesFromEntryAssembly(force: true);
	}

	private static void PatchTXTextControlLicense(Assembly assembly)
	{
		try
		{
			// 注入嵌入的 .licenses 资源到 RuntimeLicenseContext
			// 绕过 cryptoKey 校验（反编译后程序集名变更导致 cryptoKey 不匹配）
			PreloadLicensesFromEntryAssembly(force: false);
		}
		catch (Exception ex)
		{
			ex.Log("TXTextControl License Patch failed: " + ex.Message);
		}
	}

	/// <summary>
	/// 从入口程序集的嵌入资源中读取 .licenses 文件，
	/// 绕过 cryptoKey 校验直接注入到 RuntimeLicenseContext.savedLicenseKeys。
	/// </summary>
	/// <param name="force">如果为 true，则强制覆盖已有的 savedLicenseKeys；否则仅在为空时注入。</param>
	private static void PreloadLicensesFromEntryAssembly(bool force = false)
	{
		try
		{
			var entryAsm = Assembly.GetEntryAssembly();
			if (entryAsm == null) return;

			// RuntimeLicenseContext 是 LicenseManager.CurrentContext 的内部实现类
			var context = System.ComponentModel.LicenseManager.CurrentContext;
			if (context == null) return;

			var contextType = context.GetType();
			if (contextType.Name != "RuntimeLicenseContext") return;

			// 读取 savedLicenseKeys 字段
			var keysField = contextType.GetField("savedLicenseKeys",
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Public);
			if (keysField == null) return;

			// 非强制模式下，如果已经加载过了就直接返回
			if (!force && keysField.GetValue(context) != null) return;

			// 查找 .licenses 嵌入资源
			// 资源名可能是 AuditAI.exe.licenses、AuditAI.dll.licenses、或程序集短名.licenses
			string resourceName = FindLicensesResource(entryAsm);
			if (resourceName == null) return;

			using (var stream = entryAsm.GetManifestResourceStream(resourceName))
			{
				if (stream == null) return;

				var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				object obj = formatter.Deserialize(stream);

				// lc.exe 输出格式: object[] { cryptoKey_string, Hashtable_of_license_keys }
				if (obj is object[] arr && arr.Length >= 2 && arr[1] is System.Collections.Hashtable licenseKeys)
				{
					keysField.SetValue(context, licenseKeys);
				}
			}
		}
		catch (Exception ex)
		{
			ex.Log("PreloadLicensesFromEntryAssembly failed: " + ex.Message);
		}
	}

	/// <summary>在入口程序集中查找 .licenses 资源</summary>
	private static string FindLicensesResource(Assembly assembly)
	{
		string[] names = assembly.GetManifestResourceNames();
		string asmName = assembly.GetName().Name;

		// 按优先级查找
		return names.FirstOrDefault(n =>
			n.Equals(asmName + ".exe.licenses", StringComparison.OrdinalIgnoreCase) ||
			n.Equals(asmName + ".dll.licenses", StringComparison.OrdinalIgnoreCase) ||
			n.EndsWith(".licenses", StringComparison.OrdinalIgnoreCase));
	}

	private static void PatchC1LicenseFields(Assembly assembly)
	{
		PatchC1LicenseFieldsCore(assembly);
	}

	/// <summary>公开的 C1 许可证 Patch 方法，供其他模块（如账套生成器）调用</summary>
	public static void PatchC1LicenseFieldsPublic(Assembly assembly)
	{
		PatchC1LicenseFieldsCore(assembly);
	}

	/// <summary>
	/// C1 许可证 Patch 核心逻辑。
	/// 使用泛型方式扫描 C1.Util.Licensing 命名空间下的所有类型，同时支持非混淆命名
	///（ProviderInfo/SafeLicenseContext）和混淆命名（单字母如 g/d），兼容所有 C1 程序集。
	/// </summary>
	private static void PatchC1LicenseFieldsCore(Assembly assembly)
	{
		try
		{
			// ---- 方案 A：通过 GetTypes() 泛型扫描所有 Licensing 类型 ----
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException rtle)
			{
				types = rtle.Types.Where(t => t != null).ToArray();
			}

			foreach (Type type in types)
			{
				string ns = type.Namespace ?? "";
				if (!ns.StartsWith("C1.Util.Licensing"))
					continue;

				// 1. 如果类型继承自 LicenseProvider（无论是 ProviderInfo 还是混淆的 g），
				//    将其所有静态 bool 字段设为 true，跳过 Nag 弹窗逻辑
				if (type.BaseType != null &&
					type.BaseType.FullName == "System.ComponentModel.LicenseProvider")
				{
					foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
					{
						if (field.FieldType == typeof(bool))
						{
							field.SetValue(null, true);
						}
					}
				}

				// 1b. ★ 即使不是 LicenseProvider 类型，也搜索所有静态 bool 字段设为 true
				//    处理那些可能没有继承 LicenseProvider 但仍有 bool 标志位的授权辅助类
				foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
				{
					if (field.FieldType == typeof(bool))
					{
						field.SetValue(null, true);
					}
				}

				// 2. 查找所有静态 Hashtable 字段（许可证密钥缓存），确保已初始化
				//    非混淆：SafeLicenseContext.m_a
				//    混淆：d.m_a
				foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
				{
					if (field.FieldType == typeof(System.Collections.Hashtable))
					{
						var cache = field.GetValue(null) as System.Collections.Hashtable;
						if (cache == null)
						{
							cache = new System.Collections.Hashtable();
							field.SetValue(null, cache);
						}
					}
				}

				// 3. 触发所有 Licensing 类型的静态构造函数
				if (!type.IsGenericType && !type.IsNestedPrivate)
				{
					try
					{
						System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
					}
					catch { }
				}
			}

			// ---- 方案 B：如果方案 A 没有找到任何已知类型，尝试通过已知的类型名直接补丁 ----
			// 非混淆名
			TryPatchAllBools(assembly, "C1.Util.Licensing.ProviderInfo");
			TryPatchAllBools(assembly, "C1.Util.Licensing.SafeLicenseContext");
			TryPatchAllBools(assembly, "C1.Util.Licensing.LicenseInfo");
			TryPatchAllBools(assembly, "C1.Util.Licensing.ProductLicense");
			TryPatchAllBools(assembly, "C1.Util.Licensing.DTStorage");
			// 混淆名
			TryPatchAllBools(assembly, "C1.Util.Licensing.g");
			TryPatchAllBools(assembly, "C1.Util.Licensing.d");
			TryPatchAllBools(assembly, "C1.Util.Licensing.e");
			TryPatchAllBools(assembly, "C1.Util.Licensing.c");
			TryPatchAllBools(assembly, "C1.Util.Licensing.a");

			// 总是尝试触发 ProductLicense 和 DTStorage 类型初始化
			RunStaticCtor(assembly, "C1.Util.Licensing.ProductLicense");
			RunStaticCtor(assembly, "C1.Util.Licensing.DTStorage");
		}
		catch (Exception ex)
		{
			ex.Log("C1 License Patch failed for assembly: " + assembly.FullName);
		}
	}

	/// <summary>尝试按类型名查找，并将该类型中所有静态 bool 字段设为 true</summary>
	private static void TryPatchAllBools(Assembly assembly, string typeName)
	{
		try
		{
			Type t = assembly.GetType(typeName);
			if (t == null) return;

			foreach (var field in t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
			{
				if (field.FieldType == typeof(bool))
				{
					field.SetValue(null, true);
				}
			}
		}
		catch { }
	}

	/// <summary>触发指定类型的静态构造函数</summary>
	private static void RunStaticCtor(Assembly assembly, string typeName)
	{
		try
		{
			Type t = assembly.GetType(typeName);
			if (t != null)
			{
				System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
			}
		}
		catch { }
	}

}

/// <summary>
/// 全局鼠标滚轮消息过滤器，将 WM_MOUSEWHEEL 消息重定向到鼠标下方的控件。
/// WinForms 默认将滚轮消息发给焦点控件，导致鼠标悬停在滚动区域但焦点不在该控件时无法滚动。
/// </summary>
internal class MouseWheelMessageFilter : IMessageFilter
{
	private const int WM_MOUSEWHEEL = 0x20A;
	private const int WM_MOUSEHWHEEL = 0x20E;

	public bool PreFilterMessage(ref Message m)
	{
		if (m.Msg != WM_MOUSEWHEEL && m.Msg != WM_MOUSEHWHEEL)
			return false;

		// 获取鼠标屏幕坐标
		int x = (short)((m.LParam.ToInt32() << 16) >> 16);
		int y = (short)(m.LParam.ToInt32() >> 16);
		var screenPos = new Point(x, y);

		// 找到鼠标下方的控件
		var hwndUnderMouse = Win32.WindowFromPoint(screenPos);
		if (hwndUnderMouse == IntPtr.Zero || hwndUnderMouse == m.HWnd)
			return false;

		// 如果目标控件就是消息的目标，不需要重定向
		var ctrlFromHandle = Control.FromChildHandle(m.HWnd);
		var ctrlUnderMouse = Control.FromChildHandle(hwndUnderMouse);
		if (ctrlUnderMouse == null || ctrlUnderMouse == ctrlFromHandle)
			return false;

		// 将消息转发给鼠标下方的控件
		Win32.SendMessage(hwndUnderMouse, m.Msg, m.WParam, m.LParam);
		return true;
	}

	private static class Win32
	{
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern IntPtr WindowFromPoint(Point pt);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
	}
}

	public static class AppSettingsFallback
	{
		public static System.Collections.Specialized.NameValueCollection AppSettings { get; } = new System.Collections.Specialized.NameValueCollection();
	}