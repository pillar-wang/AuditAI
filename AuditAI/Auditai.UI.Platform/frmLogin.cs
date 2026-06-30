﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using C1.Win.C1SuperTooltip;
using Auditai.DTO;
using Auditai.Model;
using Auditai.PlatformResource;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class frmLogin : Form
{
	private enum Status
	{
		Normal,
		Logining,
		Checking
	}

	protected enum LoginType
	{
		LoginByPassword,
		LoginBySMS
	}

	private Status _status;

	private Auditai.Model.User _loginedUser;

	private Color _auditaiMainColor = Color.FromArgb(74, 144, 226);

	// UI 缩放因子（用于 Paint 事件绘制坐标）
	private float _scaleFactor = 1.5f;

	private Color _auditaiMainColorButton = Color.FromArgb(74, 144, 226);

	private LoginType _loginType;

	private Font _loginLinkNormalFont = new Font("Noto Sans SC", 10.5f);

	private Font _loginLinkFocusFont = new Font("Noto Sans SC", 10.5f, FontStyle.Bold);

	private Font _passwordEmptyFont = new Font("Noto Sans SC", 9f);

	private Font _passwordExistValueFont = new Font("Noto Sans SC", 9f, FontStyle.Bold);

	private Color _loginLinkNormalColor = Color.FromArgb(140, 140, 140);

	private const string _tooltipPleaseInputPhoneNumber = "请输入手机号";

	private const string _tooltipPleaseInputValidateCode = "验证码";

	private string _tooltipPleaseInputUserName = "请输入用户名或手机号";

	private const string _tooltipPleaseInputPasword = "密码";

	private string _inputPassword = string.Empty;

	public const int WM_SYSCOMMAND = 274;

	public const int SC_MOVE = 61456;

	public const int HTCAPTION = 2;

	public const int GWL_STYLE = -16;

	public const int WS_DISABLED = 134217728;

	public const int AW_HOR_POSITIVE = 1;

	public const int AW_HOR_NEGATIVE = 2;

	public const int AW_VER_POSITIVE = 4;

	public const int AW_VER_NEGATIVE = 8;

	public const int AW_CENTER = 16;

	public const int AW_HIDE = 65536;

	public const int AW_ACTIVATE = 131072;

	public const int AW_SLIDE = 262144;

	public const int AW_BLEND = 524288;

	private IContainer components;

	private C1Button btnLogin;

	private C1TextBoxEx txtUserName;

	private C1TextBoxEx txtPassword;

	private C1Button btnClose;

	private C1SplitContainer ctnUserName;

	private C1SplitterPanel pnlUserName;

	private C1PictureBox picUserName;

	private C1PictureBox picPassword;

	private C1SplitContainer ctnPassword;

	private C1SplitterPanel pnlPassword;

	private LinkLabel linkForgetPwd;

	private LinkLabel linkRegister;

	private C1CheckBox RememberPwd;

	private C1SuperTooltip c1SuperTooltip1;

	private WinformProgressBarEx progressBar1;

	private C1Label lblVersion;

	private Timer timer1;

	private LinkLabel linkLabelLoginByPassword;

	private LinkLabel linkLabelLoginByCode;

	private Label labelUnderLineLoginByPassword;

	private Label labelUnderLineLoginByCode;

	private TimerButton btnSendCode;

	private Label labelSperator1;

	private Label labelSperator2;

	private C1TextBoxEx txtPhoneNumber;

	private C1TextBoxEx txtValidateCode;

	private C1PictureBox picturePhone;

	private Panel pnlBrand;

	private Panel pnlLoginRight;

	private PictureBox picLogo;

	private Label lblProductName;

	private Label lblEnterpriseName;

	private Label lblWelcomeTitle;

	public frmLogin()
	{
		InitializeComponent();
		base.Shown += FrmLogin_Shown;
		Initialize();
		ApplyRoundedRegion(16);
		ApplyRoundedButton(btnLogin, 9);
		ApplyRoundedButton(btnSendCode, 9);
		ApplyRoundedButton(btnClose, 9);
	}

	private void FrmLogin_Shown(object sender, EventArgs e)
	{
	}

	/// <summary>给窗体应用圆角区域（WinForms Region 裁剪，无抗锯齿）</summary>
	private void ApplyRoundedRegion(int radius)
	{
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
			path.AddArc(base.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
			path.AddArc(base.Width - radius * 2, base.Height - radius * 2, radius * 2, radius * 2, 0, 90);
			path.AddArc(0, base.Height - radius * 2, radius * 2, radius * 2, 90, 90);
			path.CloseFigure();
			base.Region = new System.Drawing.Region(path);
		}
	}

	/// <summary>给按钮应用圆角区域</summary>
	private void ApplyRoundedButton(Control btn, int radius)
	{
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
			path.AddArc(btn.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
			path.AddArc(btn.Width - radius * 2, btn.Height - radius * 2, radius * 2, radius * 2, 0, 90);
			path.AddArc(0, btn.Height - radius * 2, radius * 2, radius * 2, 90, 90);
			path.CloseFigure();
			btn.Region = new System.Drawing.Region(path);
		}
	}

	private void Initialize()
	{
		if (Program.IsOnPremise)
		{
			_tooltipPleaseInputUserName = "请输入用户名";
		}
		InitPlatformStyle();
		// 小清新：替换为高清软件图标
		try
		{
			string iconPath = System.IO.Path.Combine(Application.StartupPath, "icon", "app_icon.png");
			if (!System.IO.File.Exists(iconPath))
			{
				iconPath = @"e:\lq\软件图标.png";
			}
			if (System.IO.File.Exists(iconPath))
			{
				var bmp = new System.Drawing.Bitmap(iconPath);
				picLogo.Image = bmp;
				base.Icon = IconGenerator.CreateFromImage(bmp);
			}
		}
		catch { }
		txtUserName.TextDetached = true;
		txtPassword.TextDetached = true;
		lblVersion.TextDetached = true;
		lblVersion.Text = "版本号：" + WebApiClient.AppVersion;
		c1SuperTooltip1.SetToolTip(btnClose, "关闭");
		lblWelcomeTitle.Text = "欢迎登录";
		lblProductName.Text = (StringConstBase.Current != null && !string.IsNullOrEmpty(StringConstBase.Current.AppName)) ? StringConstBase.Current.AppName : "审计AI";
		lblEnterpriseName.Text = string.Empty;
		base.StartPosition = FormStartPosition.CenterScreen;
		string code = MachineCode.Code;
		UserToken token = TokenTimer.Token;
		linkForgetPwd.Visible = !Program.IsOnPremise;
		linkRegister.Visible = !Program.IsOnPremise;
		linkLabelLoginByCode.Click += LinkLabelLoginByCode_Click;
		linkLabelLoginByPassword.Click += LinkLabelLoginByPassword_Click;
		txtPhoneNumber.GotFocus += TxtPhoneNumber_GotFocus;
		txtPhoneNumber.LostFocus += TxtPhoneNumber_LostFocus;
		txtValidateCode.GotFocus += TxtValidateCode_GotFocus;
		txtValidateCode.LostFocus += TxtValidateCode_LostFocus;
		txtUserName.GotFocus += TxtUserName_GotFocus;
		txtUserName.LostFocus += TxtUserName_LostFocus;
		txtPassword.GotFocus += TxtPassword_GotFocus;
		txtPassword.LostFocus += TxtPassword_LostFocus;
		txtUserName.InitialSelection = InitialSelectionEnum.CaretAtEnd;
		txtPassword.InitialSelection = InitialSelectionEnum.CaretAtEnd;
		txtPhoneNumber.InitialSelection = InitialSelectionEnum.CaretAtEnd;
		txtValidateCode.InitialSelection = InitialSelectionEnum.CaretAtEnd;
		SwitchLoginType(_loginType);
	}

	private void TxtPassword_LostFocus(object sender, EventArgs e)
	{
		if (txtPassword.Text.Trim() == "")
		{
			txtPassword.PasswordChar = '\0';
			txtPassword.Text = "密码";
			txtPassword.ForeColor = _loginLinkNormalColor;
			txtPassword.Font = _passwordEmptyFont;
		}
	}

	private void TxtPassword_GotFocus(object sender, EventArgs e)
	{
		if (txtPassword.Text.Trim() == "密码")
		{
			txtPassword.Text = "";
			txtPassword.PasswordChar = '●';
			txtPassword.ForeColor = Color.Black;
			txtPassword.Font = _passwordExistValueFont;
		}
	}

	private void TxtUserName_LostFocus(object sender, EventArgs e)
	{
		if (txtUserName.Text.Trim() == "")
		{
			txtUserName.Text = _tooltipPleaseInputUserName;
			txtUserName.ForeColor = _loginLinkNormalColor;
		}
	}

	private void TxtUserName_GotFocus(object sender, EventArgs e)
	{
		if (txtUserName.Text.Trim() == _tooltipPleaseInputUserName)
		{
			txtUserName.Text = "";
			txtUserName.ForeColor = Color.Black;
		}
	}

	private void TxtValidateCode_LostFocus(object sender, EventArgs e)
	{
		if (txtValidateCode.Text.Trim() == "")
		{
			txtValidateCode.Text = "验证码";
			txtValidateCode.ForeColor = _loginLinkNormalColor;
		}
	}

	private void TxtValidateCode_GotFocus(object sender, EventArgs e)
	{
		if (txtValidateCode.Text.Trim() == "验证码")
		{
			txtValidateCode.Text = "";
			txtValidateCode.ForeColor = Color.Black;
		}
	}

	private void TxtPhoneNumber_LostFocus(object sender, EventArgs e)
	{
		if (txtPhoneNumber.Text.Trim() == "")
		{
			txtPhoneNumber.Text = "请输入手机号";
			txtPhoneNumber.ForeColor = _loginLinkNormalColor;
		}
	}

	private void TxtPhoneNumber_GotFocus(object sender, EventArgs e)
	{
		if (txtPhoneNumber.Text.Trim() == "请输入手机号")
		{
			txtPhoneNumber.Text = "";
			txtPhoneNumber.ForeColor = Color.Black;
		}
	}

	private void LinkLabelLoginByPassword_Click(object sender, EventArgs e)
	{
		SwitchLoginType(LoginType.LoginByPassword);
	}

	private void LinkLabelLoginByCode_Click(object sender, EventArgs e)
	{
		SwitchLoginType(LoginType.LoginBySMS);
	}

	private void InitPlatformStyle()
	{
		progressBar1.SetAnimationTrigger(timer1);
		timer1.Start();
		progressBar1.VisibleChanged += ProgressBar1_VisibleChanged;
		InitColor();
		switch (Program.ClientPlatformType)
		{
		case PlatformType.AuditPlatform:
			InitPlatform_Audit();
			break;
		case PlatformType.EnterpriseReportPlatform:
			InitPlatform_Report();
			break;
		case PlatformType.EnterpriseManagerPlatform:
			InitPlatform_Manager();
			break;
		case PlatformType.TableDevelopPlatform:
			InitPlatform_TableDevelop();
			break;
		case PlatformType.ProductionCostAccountingSystem:
			InitPlatform_ProductionCostAccountingSystem();
			break;
		case PlatformType.ContractLedgerManagementSystem:
			InitPlatform_ContractLedgerManagementSystem();
			break;
		case PlatformType.RDExpenseLedgerSystem:
			InitPlatform_RDExpenseLedgerSystem();
			break;
		case PlatformType.SalesOrderManagementSystem:
			InitPlatform_SalesOrderManagementSystem();
			break;
		case PlatformType.PSIManagementSystem:
			InitPlatform_PSIManagementSystem();
			break;
		case PlatformType.ProjectLedgerManagementSystem:
			InitPlatform_ProjectLedgerManagementSystem();
			break;
		case PlatformType.Custom:
			InitPlatform_Custom();
			break;
		}
	}

	private void ProgressBar1_VisibleChanged(object sender, EventArgs e)
	{
		if (!progressBar1.Visible)
		{
			timer1.Enabled = false;
		}
		else if (!timer1.Enabled)
		{
			timer1.Enabled = true;
		}
	}

	private void InitColor()
	{
		_auditaiMainColor = Color.FromArgb(74, 144, 226);
		_auditaiMainColorButton = Color.FromArgb(74, 144, 226);
		btnLogin.BackColor = _auditaiMainColorButton;
		btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(53, 123, 189);
		btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 160, 230);
		linkRegister.LinkColor = _auditaiMainColor;
		progressBar1.ProgressBarColor = _auditaiMainColor;
		labelUnderLineLoginByPassword.BackColor = _auditaiMainColor;
		labelUnderLineLoginByCode.BackColor = _auditaiMainColor;
		labelSperator1.BackColor = Color.FromArgb(208, 215, 222);
		labelSperator2.BackColor = Color.FromArgb(208, 215, 222);
		btnSendCode.BackColor = _auditaiMainColorButton;
		btnSendCode.FlatAppearance.MouseDownBackColor = Color.FromArgb(53, 123, 189);
		btnSendCode.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 160, 230);
	}

	private void InitPlatform_Audit()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_audit;
		picLogo.Image = Resources.frmLoginIcon_Audit;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Audit);
	}

	private void InitPlatform_Report()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_report;
		picLogo.Image = Resources.frmLoginIcon_Report;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Report);
	}

	private void InitPlatform_Manager()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_manager;
		picLogo.Image = Resources.frmLoginIcon_Manager;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Manager);
	}

	private void InitPlatform_TableDevelop()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_table;
		picLogo.Image = Resources.frmLoginIcon_Table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_ProductionCostAccountingSystem()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_production_cost;
		picLogo.Image = Resources.frmLoginIcon_Table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_ContractLedgerManagementSystem()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_contract_ledger;
		picLogo.Image = Resources.frmLoginIcon_Table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_RDExpenseLedgerSystem()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_rd_expense;
		picLogo.Image = Resources.frmLoginIcon_Table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_SalesOrderManagementSystem()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_sales_order;
		picLogo.Image = Resources.frmLoginIcon_Table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_PSIManagementSystem()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_psi_management;
		picLogo.Image = Resources.frmLoginIcon_Table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_ProjectLedgerManagementSystem()
	{
		pnlBrand.BackgroundImage = Resources.login_bg_project_ledger;
		picLogo.Image = Resources.frmLoginIcon_Table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_Custom()
	{
		pnlBrand.BackgroundImage = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\login_form_bg.png")));
		picLogo.Image = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\login_form_icon.png")));
		base.Icon = IconGenerator.CreateFromImage((Bitmap)System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\login_form_icon.png"))));
	}

	private void SwitchLoginType(LoginType type)
	{
		if (Program.IsOnPremise)
		{
			btnSendCode.Visible = false;
			labelUnderLineLoginByCode.Visible = false;
			labelUnderLineLoginByPassword.Visible = false;
			linkLabelLoginByCode.Visible = false;
			linkLabelLoginByPassword.Visible = false;
			labelSperator1.Visible = false;
			labelSperator2.Visible = false;
			txtPhoneNumber.Visible = false;
			txtValidateCode.Visible = false;
			picturePhone.Visible = false;
			return;
		}
		_loginType = type;
		labelSperator1.BackColor = Color.FromArgb(208, 215, 222);
		labelSperator2.BackColor = Color.FromArgb(208, 215, 222);
		switch (type)
		{
		case LoginType.LoginByPassword:
			btnSendCode.Visible = false;
			txtPhoneNumber.Visible = false;
			txtValidateCode.Visible = false;
			txtUserName.Visible = true;
			txtPassword.Visible = true;
			picUserName.Visible = true;
			picturePhone.Visible = false;
			labelUnderLineLoginByCode.Visible = false;
			linkLabelLoginByCode.LinkColor = _loginLinkNormalColor;
			linkLabelLoginByCode.Font = _loginLinkNormalFont;
			linkLabelLoginByCode.ActiveLinkColor = _auditaiMainColor;
			labelUnderLineLoginByPassword.Visible = true;
			labelUnderLineLoginByPassword.BackColor = _auditaiMainColor;
			linkLabelLoginByPassword.LinkColor = _auditaiMainColor;
			linkLabelLoginByPassword.Font = _loginLinkFocusFont;
			linkLabelLoginByPassword.ActiveLinkColor = _auditaiMainColor;
			break;
		case LoginType.LoginBySMS:
			txtPhoneNumber.Visible = true;
			txtValidateCode.Visible = true;
			txtUserName.Visible = false;
			txtPassword.Visible = false;
			picUserName.Visible = false;
			picturePhone.Visible = true;
			labelUnderLineLoginByPassword.Visible = false;
			linkLabelLoginByPassword.LinkColor = _loginLinkNormalColor;
			linkLabelLoginByPassword.Font = _loginLinkNormalFont;
			linkLabelLoginByPassword.ActiveLinkColor = _auditaiMainColor;
			labelUnderLineLoginByCode.Visible = true;
			linkLabelLoginByCode.Visible = true;
			labelUnderLineLoginByCode.BackColor = _auditaiMainColor;
			linkLabelLoginByCode.LinkColor = _auditaiMainColor;
			linkLabelLoginByCode.Font = _loginLinkFocusFont;
			linkLabelLoginByCode.LinkColor = _auditaiMainColor;
			btnSendCode.Visible = true;
			btnSendCode.BackColor = _auditaiMainColorButton;
			break;
		}
	}

	private void SwitchStatusTo(Status status)
	{
		_status = status;
		switch (_status)
		{
		case Status.Normal:
			SetControlEnabled(btnLogin, enabled: true);
			progressBar1.Visible = false;
			btnLogin.Text = "登录";
			break;
		case Status.Logining:
			SetControlEnabled(btnLogin, enabled: false);
			progressBar1.Visible = false;
			btnLogin.Text = "登录中...";
			break;
		case Status.Checking:
			SetControlEnabled(btnLogin, enabled: false);
			progressBar1.Visible = true;
			btnLogin.Text = "检查更新...";
			break;
		}
	}

	private void SaveLogin(string userName, string password, string phoneNumher, bool isLoginByPhoneNumber = false)
	{
		UserSet.Config.UserName = userName;
		UserSet.Config.Password = password;
		UserSet.Config.Machine = MachineCode.Code;
		UserSet.Config.PhoneNumber = phoneNumher;
		UserSet.Config.LoginType = (int)_loginType;
		UserSet.Config.IsLoginByPhoneNumber = isLoginByPhoneNumber;
	}

	private void InitTextboxInitValue()
	{
		if (string.IsNullOrEmpty(txtUserName.Text.Trim()))
		{
			txtUserName.Text = _tooltipPleaseInputUserName;
			txtUserName.ForeColor = _loginLinkNormalColor;
		}
		if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
		{
			txtPassword.PasswordChar = '\0';
			txtPassword.Text = "密码";
			txtPassword.ForeColor = _loginLinkNormalColor;
			txtPassword.Font = _passwordEmptyFont;
		}
		if (string.IsNullOrEmpty(txtPhoneNumber.Text.Trim()))
		{
			txtPhoneNumber.Text = "请输入手机号";
			txtPhoneNumber.ForeColor = _loginLinkNormalColor;
		}
		if (string.IsNullOrEmpty(txtValidateCode.Text.Trim()))
		{
			txtValidateCode.Text = "验证码";
			txtValidateCode.ForeColor = _loginLinkNormalColor;
		}
	}

	private void LoadLogin()
	{
		UserConfig config = UserSet.Config;
		if (config.LoginType == 1)
		{
			txtUserName.Text = string.Empty;
			txtPassword.Text = string.Empty;
			SwitchLoginType(LoginType.LoginBySMS);
			if (!string.IsNullOrWhiteSpace(config.PhoneNumber))
			{
				RememberPwd.Checked = true;
				txtPhoneNumber.Text = config.PhoneNumber;
				InitTextboxInitValue();
			}
			else
			{
				RememberPwd.Checked = false;
				txtValidateCode.Text = string.Empty;
				InitTextboxInitValue();
			}
			return;
		}
		SwitchLoginType(LoginType.LoginByPassword);
		txtPhoneNumber.Text = string.Empty;
		txtValidateCode.Text = string.Empty;
		if (!string.IsNullOrWhiteSpace(config.Password))
		{
			RememberPwd.Checked = true;
			txtUserName.Text = (config.IsLoginByPhoneNumber ? config.PhoneNumber : config.UserName);
			txtPassword.Text = "********";
			txtPassword.Value = config.Password;
			_inputPassword = config.Password;
			InitTextboxInitValue();
		}
		else
		{
			RememberPwd.Checked = false;
			txtPassword.Value = string.Empty;
			InitTextboxInitValue();
		}
	}

	private void ResetLogin()
	{
		UserSet.Config.Password = null;
		UserSet.Config.Machine = null;
		UserSet.Config.PhoneNumber = null;
		UserSet.Config.IsLoginByPhoneNumber = false;
	}

	private async void btnLogin_Click(object sender, EventArgs e)
	{
		if (_status != 0)
		{
			return;
		}
		try
		{
			bool isLoginByPhoneNumber = false;
			string loginPhoneNumber = string.Empty;
			SwitchStatusTo(Status.Logining);
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> progress)
			{
				progress.Report(new ProgressInfo
				{
					MainCaption = "正在登录，请稍候...",
					MainProgress = 100
				});
				if (_loginType == LoginType.LoginBySMS)
				{
					string text = txtPhoneNumber.Text.Trim();
					string text2 = txtValidateCode.Text.Trim();
					if (text == "")
					{
						throw new NormalException("手机号不允许为空！");
					}
					if (text2 == "")
					{
						throw new NormalException("验证码不允许为空！");
					}
					loginPhoneNumber = text;
					Task<Tuple<UserToken, Auditai.DTO.User>> task = WebApiClient.AccountLoginBySMS(text, text2);
					return await (await task.ContinueWith(async delegate(Task<Tuple<UserToken, Auditai.DTO.User>> t)
					{
						Tuple<UserToken, Auditai.DTO.User> result = await t;
						if (result == null || result.Item2 == null)
						{
							throw new ServerException { ExceptionMessage = "登录失败：服务器返回无效数据", ExceptionType = "NullResponse" };
						}
						Auditai.DTO.User item2 = result.Item2;
						_loginedUser = new Auditai.Model.User
						{
							Id = item2.Id,
							Name = item2.Name,
							UserName = item2.UserName,
							TelPhone = item2.Phone,
							IsSystemAdmin = item2.IsSystemAdmin
						};
						Auditai.Model.User.Current = _loginedUser;
						return (object)null;
					}));
				}
				string userName = txtUserName.Text;
				string inputPassword = _inputPassword;
				Task<Tuple<UserToken, Auditai.DTO.User>> task2 = WebApiClient.AccountLogin(userName, inputPassword);
				return await (await task2.ContinueWith(async delegate(Task<Tuple<UserToken, Auditai.DTO.User>> t)
				{
					Tuple<UserToken, Auditai.DTO.User> result = await t;
					if (result == null || result.Item2 == null)
					{
						throw new ServerException { ExceptionMessage = "登录失败：服务器返回无效数据", ExceptionType = "NullResponse" };
					}
					Auditai.DTO.User item = result.Item2;
					_loginedUser = new Auditai.Model.User
					{
						Id = item.Id,
						Name = item.Name,
						UserName = item.UserName,
						TelPhone = item.Phone,
						IsSystemAdmin = item.IsSystemAdmin
					};
					Auditai.Model.User.Current = _loginedUser;
					isLoginByPhoneNumber = userName == item.Phone;
					loginPhoneNumber = item.Phone;
					return (object)null;
				}));
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			_loginedUser.CreateProfileFolderIfNotExist();
			if (TokenTimer.LoginInfo != null && TokenTimer.LoginInfo.LoginMode == LoginMode.SMS)
			{
				UserSet.LoginPassword = string.Empty;
				UserSet.LoginPhone = loginPhoneNumber;
			}
			else
			{
				UserSet.LoginPassword = _inputPassword;
				UserSet.LoginPhone = string.Empty;
			}
			if (RememberPwd.Checked)
			{
				SaveLogin(_loginedUser.UserName, _inputPassword, loginPhoneNumber, isLoginByPhoneNumber);
			}
			else
			{
				ResetLogin();
			}
			if (await GetAndOpenTeam())
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				SwitchStatusTo(Status.Normal);
			}
		}
		catch (NormalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			SwitchStatusTo(Status.Normal);
		}
		catch (ServerException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
			SwitchStatusTo(Status.Normal);
		}
		catch (HttpRequestException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
			SwitchStatusTo(Status.Normal);
		}
		catch (TimeoutException ex4)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
			SwitchStatusTo(Status.Normal);
		}
		catch (Exception ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.ToString());
			SwitchStatusTo(Status.Normal);
		}
	}

	private async Task<bool> GetAndOpenTeam()
	{
		Guid supporterTeamId = new Guid("00000000-0000-0000-0000-000000000001");
		bool isSystemSupporter = false;
		List<UserTeam> list = null;
		try
		{
			if (!Program.IsOnPremise)
			{
				Program.UserGetTeamCallback = UpdateIsSystemSupporter;
			}
			list = await Program.GetUserTeams();
			if (list == null)
			{
				return false;
			}
		}
		finally
		{
			Program.UserGetTeamCallback = null;
		}
		Auditai.Model.User.Current.IsSystemSupporter = isSystemSupporter;
		if (list.Count == 1)
		{
			return await Program.OpenTeam(list[0].Id);
		}
		return true;
		void UpdateIsSystemSupporter(Guid teamId)
		{
			if (teamId == supporterTeamId)
			{
				isSystemSupporter = true;
			}
		}
	}

	private void linkForgetPwd_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		frmFindPwd frmFindPwd2 = new frmFindPwd();
		frmFindPwd2.ShowDialog();
	}

	private async void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		frmRegister frmRegister2 = new frmRegister();
		if (DialogResult.OK != frmRegister2.ShowDialog())
		{
			return;
		}
		SwitchStatusTo(Status.Logining);
		try
		{
			_loginedUser = new Auditai.Model.User
			{
				Id = frmRegister2.UserId,
				Name = frmRegister2.Truename,
				UserName = frmRegister2.UserName,
				TelPhone = frmRegister2.TelPhone
			};
			Auditai.Model.User.Current = _loginedUser;
			_loginedUser.CreateProfileFolderIfNotExist();
			LoginInfo loginInfo = new LoginInfo
			{
				userId = frmRegister2.UserId,
				userName = frmRegister2.UserName,
				password = frmRegister2.Password,
				LoginMode = LoginMode.Password
			};
			TokenTimer.LoginInfo = loginInfo;
			SaveLogin(frmRegister2.UserName, frmRegister2.Password, string.Empty);
			await WebApiClient.AccountLogin(frmRegister2.UserName, frmRegister2.Password);
			if (await GetAndOpenTeam())
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				SwitchStatusTo(Status.Normal);
			}
		}
		catch (Exception)
		{
			SwitchStatusTo(Status.Normal);
		}
	}

	private void txtPassword_TextChanged(object sender, EventArgs e)
	{
		UpdateInputPassword(txtPassword.Text.Trim());
	}

	private void UpdateInputPassword(string text)
	{
		_inputPassword = Encrypts.SHA256Encrypt(text.Trim(), isUrl: true);
	}

	public static string GetPasswordEncryptValue(string value)
	{
		return Encrypts.SHA256Encrypt(value.Trim(), isUrl: true);
	}

	private void frmLogin_Shown(object sender, EventArgs e)
	{
		InitUIDefine();
		LoadLogin();
		SwitchStatusTo(Status.Normal);
		try
		{
			Program.CreateMainForm();
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		txtPassword.TextChanged += txtPassword_TextChanged;
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	[DllImport("user32.dll")]
	public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);

	[DllImport("user32.dll")]
	public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	public static void SetControlEnabled(Control c, bool enabled)
	{
		if (enabled)
		{
			SetWindowLong(c.Handle, -16, -134217729 & GetWindowLong(c.Handle, -16));
		}
		else
		{
			SetWindowLong(c.Handle, -16, 0x8000000 | GetWindowLong(c.Handle, -16));
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

	private void InitUIDefine()
	{
		AnimateWindow(base.Handle, 100, 524288);
		Refresh();
		pnlUserName.MouseEnter += delegate
		{
			ctnUserName.BorderColor = _auditaiMainColor;
		};
		pnlUserName.MouseLeave += delegate
		{
			ctnUserName.BorderColor = Color.LightGray;
		};
		ctnUserName.MouseEnter += delegate
		{
			ctnUserName.BorderColor = _auditaiMainColor;
		};
		ctnUserName.MouseLeave += delegate
		{
		};
		txtUserName.MouseEnter += MouseEnter_UserName;
		txtUserName.MouseLeave += MouseLeave_UserName;
		picUserName.MouseEnter += MouseEnter_UserName;
		picUserName.MouseLeave += MouseLeave_UserName;
		txtPassword.MouseEnter += MouseEnter_Password;
		txtPassword.MouseLeave += MouseLeave_Password;
		picPassword.MouseEnter += MouseEnter_Password;
		picPassword.MouseLeave += MouseLeave_Password;
		txtPhoneNumber.MouseEnter += MouseEnter_Phone;
		txtPhoneNumber.MouseLeave += MouseLeave_Phone;
		txtValidateCode.MouseEnter += MouseEnter_ValidateCode;
		txtValidateCode.MouseLeave += MouseLeave_ValidateCode;
		void MouseEnter_Password(object s3, EventArgs e3)
		{
			pnlPassword.BorderColor = _auditaiMainColor;
		}
		void MouseEnter_Phone(object s3, EventArgs e3)
		{
			pnlUserName.BorderColor = _auditaiMainColor;
		}
		void MouseEnter_UserName(object s3, EventArgs e3)
		{
			pnlUserName.BorderColor = _auditaiMainColor;
		}
		void MouseEnter_ValidateCode(object s3, EventArgs e3)
		{
			pnlPassword.BorderColor = _auditaiMainColor;
		}
		void MouseLeave_Password(object s3, EventArgs e3)
		{
			pnlPassword.BorderColor = Color.LightGray;
		}
		void MouseLeave_Phone(object s3, EventArgs e3)
		{
			pnlUserName.BorderColor = Color.LightGray;
		}
		void MouseLeave_UserName(object s3, EventArgs e3)
		{
			pnlUserName.BorderColor = Color.LightGray;
		}
		void MouseLeave_ValidateCode(object s3, EventArgs e3)
		{
			pnlPassword.BorderColor = Color.LightGray;
		}
	}

	private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			AnimateWindow(base.Handle, 100, 851968);
		}
	}

	private void linkForgetPwd_MouseEnter(object sender, EventArgs e)
	{
		Cursor = Cursors.Hand;
		linkForgetPwd.Font = new Font(linkForgetPwd.Font, FontStyle.Bold);
	}

	private void linkForgetPwd_MouseLeave(object sender, EventArgs e)
	{
		Cursor = Cursors.Default;
		linkForgetPwd.Font = new Font(linkForgetPwd.Font, FontStyle.Regular);
	}

	private void linkRegister_MouseEnter(object sender, EventArgs e)
	{
		Cursor = Cursors.Hand;
		linkRegister.Font = new Font(linkRegister.Font, FontStyle.Bold);
	}

	private void linkRegister_MouseLeave(object sender, EventArgs e)
	{
		Cursor = Cursors.Default;
		linkRegister.Font = new Font(linkRegister.Font, FontStyle.Regular);
	}

	private void frmLogin_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	private void pnlUserName_Click(object sender, EventArgs e)
	{
		if (_loginType == LoginType.LoginByPassword)
		{
			txtUserName.Focus();
		}
		else
		{
			txtPhoneNumber.Focus();
		}
	}

	private void pnlPassword_Click(object sender, EventArgs e)
	{
		if (_loginType == LoginType.LoginByPassword)
		{
			txtPassword.Focus();
		}
		else
		{
			txtValidateCode.Focus();
		}
	}

	private void frmLogin_Paint(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		// 1. 浅蓝渐变背景（参考网页: linear-gradient(135deg, #E3F0FF 0%, #F5F8FA 50%, #DCEBFF 100%)）
		Rectangle bgRect = new Rectangle(0, 0, base.Width, base.Height);
		using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			new Point(0, 0),
			new Point(base.Width, base.Height),
			Color.FromArgb(227, 240, 255),
			Color.FromArgb(245, 248, 250)))
		{
			g.FillRectangle(bgBrush, bgRect);
		}
		// 添加第二层渐变增强效果
		using (var bgBrush2 = new System.Drawing.Drawing2D.LinearGradientBrush(
			new Point(base.Width, 0),
			new Point(0, base.Height),
			Color.FromArgb(150, 220, 235, 255),
			Color.FromArgb(150, 240, 248, 255)))
		{
			g.FillRectangle(bgBrush2, bgRect);
		}

		// 2. 顶部细光带（天蓝）
		Rectangle topBar = new Rectangle(0, 0, base.Width, (int)(3 * _scaleFactor));
		using (var topBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			topBar, Color.FromArgb(90, 160, 230), Color.FromArgb(74, 144, 226),
			System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
		{
			g.FillRectangle(topBrush, topBar);
		}

		// 3. 白色圆角卡片（圆角按缩放因子调整，参考网页 radius: 16px）
		int cardX = (int)(50 * _scaleFactor), cardY = (int)(50 * _scaleFactor);
		int cardW = (int)(380 * _scaleFactor), cardH = (int)(480 * _scaleFactor);
		int radius = (int)(16 * _scaleFactor);
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			path.AddArc(cardX, cardY, radius * 2, radius * 2, 180, 90);
			path.AddArc(cardX + cardW - radius * 2, cardY, radius * 2, radius * 2, 270, 90);
			path.AddArc(cardX + cardW - radius * 2, cardY + cardH - radius * 2, radius * 2, radius * 2, 0, 90);
			path.AddArc(cardX, cardY + cardH - radius * 2, radius * 2, radius * 2, 90, 90);
			path.CloseFigure();

			// 卡片阴影（参考网页: box-shadow: 0 8px 32px rgba(74, 144, 226, 0.18)）
			using (var shadowBrush = new SolidBrush(Color.FromArgb(25, 74, 144, 226)))
			{
				g.TranslateTransform(4, 8);
				g.FillPath(shadowBrush, path);
				g.TranslateTransform(-4, -8);
			}
			// 卡片白色背景
			using (var cardBrush = new SolidBrush(Color.White))
			{
				g.FillPath(cardBrush, path);
			}
			// 卡片浅蓝边框
			using (var borderPen = new Pen(Color.FromArgb(180, 215, 245), 1f))
			{
				g.DrawPath(borderPen, path);
			}
		}
	}

	private void pnlBrand_Paint(object sender, PaintEventArgs e)
	{
		Rectangle rect = new Rectangle(0, 0, pnlBrand.Width, pnlBrand.Height);
		Color darkColor = Color.FromArgb(Math.Max(0, _auditaiMainColor.R - 60), Math.Max(0, _auditaiMainColor.G - 60), Math.Max(0, _auditaiMainColor.B - 60));
		using (LinearGradientBrush brush = new LinearGradientBrush(rect, _auditaiMainColor, darkColor, LinearGradientMode.Vertical))
		{
			e.Graphics.FillRectangle(brush, rect);
		}
	}

	private async void btnSendCode_Click(object sender, EventArgs e)
	{
		bool isSuccessSend = false;
		try
		{
			string text = txtPhoneNumber.Text.Trim();
			if (text == "")
			{
				throw new NormalException("手机号不允许为空！");
			}
			if (!Regex.IsMatch(text, "^[0-9]{11,11}$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "手机号码格式不正确，请检查是否是11位数字！");
				return;
			}
			btnSendCode.Start(120);
			await WebApiClient.GetCodeByName(await WebApiClient.GetUsernameByPhone(text), "5");
			isSuccessSend = true;
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (Exception ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
		}
		finally
		{
			if (!isSuccessSend)
			{
				btnSendCode.Reset();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.btnLogin = new C1.Win.C1Input.C1Button();
		this.ctnUserName = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlUserName = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.picturePhone = new C1.Win.C1Input.C1PictureBox();
		this.txtPhoneNumber = new Auditai.UI.Controls.C1TextBoxEx();
		this.picUserName = new C1.Win.C1Input.C1PictureBox();
		this.txtUserName = new Auditai.UI.Controls.C1TextBoxEx();
		this.ctnPassword = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlPassword = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtValidateCode = new Auditai.UI.Controls.C1TextBoxEx();
		this.btnSendCode = new Auditai.UI.Controls.TimerButton();
		this.picPassword = new C1.Win.C1Input.C1PictureBox();
		this.txtPassword = new Auditai.UI.Controls.C1TextBoxEx();
		this.linkForgetPwd = new System.Windows.Forms.LinkLabel();
		this.linkRegister = new System.Windows.Forms.LinkLabel();
		this.RememberPwd = new C1.Win.C1Input.C1CheckBox();
		this.c1SuperTooltip1 = new C1.Win.C1SuperTooltip.C1SuperTooltip(this.components);
		this.btnClose = new C1.Win.C1Input.C1Button();
		this.lblVersion = new C1.Win.C1Input.C1Label();
		this.progressBar1 = new Auditai.UI.Controls.WinformProgressBarEx();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.linkLabelLoginByPassword = new System.Windows.Forms.LinkLabel();
		this.linkLabelLoginByCode = new System.Windows.Forms.LinkLabel();
		this.labelUnderLineLoginByPassword = new System.Windows.Forms.Label();
		this.labelUnderLineLoginByCode = new System.Windows.Forms.Label();
		this.labelSperator1 = new System.Windows.Forms.Label();
		this.labelSperator2 = new System.Windows.Forms.Label();
		this.pnlBrand = new System.Windows.Forms.Panel();
		this.pnlLoginRight = new System.Windows.Forms.Panel();
		this.picLogo = new System.Windows.Forms.PictureBox();
		this.lblProductName = new System.Windows.Forms.Label();
		this.lblEnterpriseName = new System.Windows.Forms.Label();
		this.lblWelcomeTitle = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.btnLogin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnUserName).BeginInit();
		this.ctnUserName.SuspendLayout();
		this.pnlUserName.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.picturePhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhoneNumber).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnPassword).BeginInit();
		this.ctnPassword.SuspendLayout();
		this.pnlPassword.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtValidateCode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnSendCode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.RememberPwd).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblVersion).BeginInit();
		this.pnlBrand.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.picLogo).BeginInit();
		this.pnlLoginRight.SuspendLayout();
		base.SuspendLayout();
		this.btnLogin.BackColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.btnLogin.FlatAppearance.BorderSize = 0;
		this.btnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(53, 123, 189);
		this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(90, 160, 230);
		this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnLogin.Font = new System.Drawing.Font("Noto Sans SC", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnLogin.ForeColor = System.Drawing.Color.White;
		this.btnLogin.Location = new System.Drawing.Point(60, 562);
		this.btnLogin.Name = "btnLogin";
		this.btnLogin.Size = new System.Drawing.Size(450, 66);
		this.btnLogin.TabIndex = 3;
		this.btnLogin.Text = "登录";
		this.btnLogin.UseVisualStyleBackColor = false;
		this.btnLogin.Click += new System.EventHandler(btnLogin_Click);
		this.ctnUserName.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnUserName.BackColor = System.Drawing.Color.Transparent;
		this.ctnUserName.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnUserName.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnUserName.Location = new System.Drawing.Point(60, 338);
		this.ctnUserName.Name = "ctnUserName";
		this.ctnUserName.Panels.Add(this.pnlUserName);
		this.ctnUserName.Size = new System.Drawing.Size(450, 63);
		this.ctnUserName.TabIndex = 6;
		this.ctnUserName.UseParentVisualStyle = false;
		this.pnlUserName.BackColor = System.Drawing.Color.White;
		this.pnlUserName.BorderColor = System.Drawing.Color.FromArgb(208, 215, 222);
		this.pnlUserName.BorderWidth = 1;
		this.pnlUserName.Controls.Add(this.picturePhone);
		this.pnlUserName.Controls.Add(this.txtPhoneNumber);
		this.pnlUserName.Controls.Add(this.picUserName);
		this.pnlUserName.Controls.Add(this.txtUserName);
		this.pnlUserName.Height = 42;
		this.pnlUserName.Location = new System.Drawing.Point(2, 2);
		this.pnlUserName.Name = "pnlUserName";
		this.pnlUserName.Size = new System.Drawing.Size(447, 60);
		this.pnlUserName.TabIndex = 0;
		this.pnlUserName.Click += new System.EventHandler(pnlUserName_Click);
		this.picturePhone.BackgroundImage = Auditai.UI.Platform.Properties.Resources.phoneLogin;
		this.picturePhone.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.picturePhone.Location = new System.Drawing.Point(18, 18);
		this.picturePhone.Name = "picturePhone";
		this.picturePhone.Size = new System.Drawing.Size(27, 27);
		this.picturePhone.TabIndex = 2;
		this.picturePhone.TabStop = false;
		this.txtPhoneNumber.AutoSize = false;
		this.txtPhoneNumber.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtPhoneNumber.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhoneNumber.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtPhoneNumber.Location = new System.Drawing.Point(60, 8);
		this.txtPhoneNumber.Name = "txtPhoneNumber";
		this.txtPhoneNumber.Size = new System.Drawing.Size(372, 48);
		this.txtPhoneNumber.TabIndex = 0;
		this.txtPhoneNumber.Tag = null;
		this.txtPhoneNumber.TextDetached = true;
		this.txtPhoneNumber.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.picUserName.BackgroundImage = Auditai.UI.Platform.Properties.Resources.userlogin;
		this.picUserName.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.picUserName.Location = new System.Drawing.Point(18, 18);
		this.picUserName.Name = "picUserName";
		this.picUserName.Size = new System.Drawing.Size(27, 27);
		this.picUserName.TabIndex = 1;
		this.picUserName.TabStop = false;
		this.txtUserName.AutoSize = false;
		this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtUserName.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtUserName.Location = new System.Drawing.Point(60, 8);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.Size = new System.Drawing.Size(372, 48);
		this.txtUserName.TabIndex = 0;
		this.txtUserName.Tag = null;
		this.txtUserName.TextDetached = true;
		this.txtUserName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.ctnPassword.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnPassword.BackColor = System.Drawing.Color.Transparent;
		this.ctnPassword.BorderColor = System.Drawing.Color.LightGray;
		this.ctnPassword.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnPassword.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnPassword.Location = new System.Drawing.Point(60, 420);
		this.ctnPassword.Name = "ctnPassword";
		this.ctnPassword.Panels.Add(this.pnlPassword);
		this.ctnPassword.Size = new System.Drawing.Size(450, 63);
		this.ctnPassword.TabIndex = 7;
		this.ctnPassword.UseParentVisualStyle = false;
		this.pnlPassword.BackColor = System.Drawing.Color.White;
		this.pnlPassword.BorderColor = System.Drawing.Color.FromArgb(208, 215, 222);
		this.pnlPassword.BorderWidth = 1;
		this.pnlPassword.Controls.Add(this.txtValidateCode);
		this.pnlPassword.Controls.Add(this.btnSendCode);
		this.pnlPassword.Controls.Add(this.picPassword);
		this.pnlPassword.Controls.Add(this.txtPassword);
		this.pnlPassword.Height = 42;
		this.pnlPassword.Location = new System.Drawing.Point(2, 2);
		this.pnlPassword.Name = "pnlPassword";
		this.pnlPassword.Size = new System.Drawing.Size(447, 60);
		this.pnlPassword.TabIndex = 0;
		this.pnlPassword.Click += new System.EventHandler(pnlPassword_Click);
		this.txtValidateCode.AutoSize = false;
		this.txtValidateCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtValidateCode.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtValidateCode.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtValidateCode.Location = new System.Drawing.Point(60, 8);
		this.txtValidateCode.Name = "txtValidateCode";
		this.txtValidateCode.Size = new System.Drawing.Size(225, 48);
		this.txtValidateCode.TabIndex = 1;
		this.txtValidateCode.Tag = null;
		this.txtValidateCode.TextDetached = true;
		this.txtValidateCode.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.btnSendCode.BackColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.btnSendCode.FlatAppearance.BorderSize = 0;
		this.btnSendCode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(53, 123, 189);
		this.btnSendCode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(90, 160, 230);
		this.btnSendCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSendCode.ForeColor = System.Drawing.Color.White;
		this.btnSendCode.Format = "(0s)";
		this.btnSendCode.Location = new System.Drawing.Point(300, 9);
		this.btnSendCode.Name = "btnSendCode";
		this.btnSendCode.Size = new System.Drawing.Size(135, 45);
		this.btnSendCode.TabIndex = 2;
		this.btnSendCode.Text = "获取验证码";
		this.btnSendCode.UseVisualStyleBackColor = false;
		this.btnSendCode.Click += new System.EventHandler(btnSendCode_Click);
		this.picPassword.BackgroundImage = Auditai.UI.Platform.Properties.Resources.password;
		this.picPassword.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.picPassword.Location = new System.Drawing.Point(18, 18);
		this.picPassword.Name = "picPassword";
		this.picPassword.Size = new System.Drawing.Size(27, 27);
		this.picPassword.TabIndex = 1;
		this.picPassword.TabStop = false;
		this.txtPassword.AutoSize = false;
		this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtPassword.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtPassword.Location = new System.Drawing.Point(60, 8);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '●';
		this.txtPassword.Size = new System.Drawing.Size(372, 48);
		this.txtPassword.TabIndex = 1;
		this.txtPassword.Tag = null;
		this.txtPassword.TextDetached = true;
		this.txtPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.linkForgetPwd.ActiveLinkColor = System.Drawing.Color.FromArgb(90, 160, 230);
		this.linkForgetPwd.AutoSize = true;
		this.linkForgetPwd.BackColor = System.Drawing.Color.Transparent;
		this.linkForgetPwd.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkForgetPwd.ForeColor = System.Drawing.Color.FromArgb(107, 119, 133);
		this.linkForgetPwd.LinkColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.linkForgetPwd.Location = new System.Drawing.Point(248, 507);
		this.linkForgetPwd.Name = "linkForgetPwd";
		this.linkForgetPwd.Size = new System.Drawing.Size(165, 26);
		this.linkForgetPwd.TabIndex = 1;
		this.linkForgetPwd.TabStop = true;
		this.linkForgetPwd.Text = "忘记用户名或密码";
		this.linkForgetPwd.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkForgetPwd_LinkClicked);
		this.linkForgetPwd.MouseEnter += new System.EventHandler(linkForgetPwd_MouseEnter);
		this.linkForgetPwd.MouseLeave += new System.EventHandler(linkForgetPwd_MouseLeave);
		this.linkRegister.ActiveLinkColor = System.Drawing.Color.FromArgb(90, 160, 230);
		this.linkRegister.AutoSize = true;
		this.linkRegister.BackColor = System.Drawing.Color.Transparent;
		this.linkRegister.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkRegister.ForeColor = System.Drawing.Color.FromArgb(107, 119, 133);
		this.linkRegister.LinkColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.linkRegister.Location = new System.Drawing.Point(435, 507);
		this.linkRegister.Name = "linkRegister";
		this.linkRegister.Size = new System.Drawing.Size(90, 26);
		this.linkRegister.TabIndex = 2;
		this.linkRegister.TabStop = true;
		this.linkRegister.Text = "立即注册";
		this.linkRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkRegister_LinkClicked);
		this.linkRegister.MouseEnter += new System.EventHandler(linkRegister_MouseEnter);
		this.linkRegister.MouseLeave += new System.EventHandler(linkRegister_MouseLeave);
		this.RememberPwd.BackColor = System.Drawing.Color.Transparent;
		this.RememberPwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.RememberPwd.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.RememberPwd.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.RememberPwd.Location = new System.Drawing.Point(60, 502);
		this.RememberPwd.Name = "RememberPwd";
		this.RememberPwd.Size = new System.Drawing.Size(150, 36);
		this.RememberPwd.TabIndex = 8;
		this.RememberPwd.Text = "记住密码";
		this.RememberPwd.UseVisualStyleBackColor = false;
		this.RememberPwd.Value = null;
		this.c1SuperTooltip1.Font = new System.Drawing.Font("Tahoma", 8f);
		this.c1SuperTooltip1.RightToLeft = System.Windows.Forms.RightToLeft.Inherit;
		this.c1SuperTooltip1.Shadow = false;
		this.btnClose.BackColor = System.Drawing.Color.Transparent;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(255, 200, 200);
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Image = Auditai.UI.Platform.Properties.Resources.close2;
		this.btnClose.Location = new System.Drawing.Point(518, 12);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(38, 38);
		this.btnClose.TabIndex = 5;
		this.btnClose.UseVisualStyleBackColor = false;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.lblVersion.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
		this.lblVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVersion.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(107, 119, 133);
		this.lblVersion.Location = new System.Drawing.Point(60, 682);
		this.lblVersion.Name = "lblVersion";
		this.lblVersion.Size = new System.Drawing.Size(450, 27);
		this.lblVersion.TabIndex = 15;
		this.lblVersion.Tag = null;
		this.lblVersion.Text = "版本号：";
		this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblVersion.TextDetached = true;
		this.progressBar1.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
		this.progressBar1.ColorChunkLength = 100;
		this.progressBar1.ColorChunkMoveSpeed = 150;
		this.progressBar1.Location = new System.Drawing.Point(0, 712);
		this.progressBar1.MarqueeAnimationSpeed = 5;
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.ProgressBarColor = System.Drawing.Color.FromArgb(50, 150, 220);
		this.progressBar1.Size = new System.Drawing.Size(570, 6);
		this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.progressBar1.TabIndex = 14;
		this.timer1.Interval = 5;
		this.linkLabelLoginByPassword.ActiveLinkColor = System.Drawing.Color.FromArgb(90, 160, 230);
		this.linkLabelLoginByPassword.BackColor = System.Drawing.Color.Transparent;
		this.linkLabelLoginByPassword.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkLabelLoginByPassword.ForeColor = System.Drawing.Color.FromArgb(107, 119, 133);
		this.linkLabelLoginByPassword.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
		this.linkLabelLoginByPassword.LinkColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.linkLabelLoginByPassword.Location = new System.Drawing.Point(142, 270);
		this.linkLabelLoginByPassword.Name = "linkLabelLoginByPassword";
		this.linkLabelLoginByPassword.Size = new System.Drawing.Size(150, 39);
		this.linkLabelLoginByPassword.TabIndex = 17;
		this.linkLabelLoginByPassword.TabStop = true;
		this.linkLabelLoginByPassword.Text = "账号密码登录";
		this.linkLabelLoginByPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.linkLabelLoginByCode.ActiveLinkColor = System.Drawing.Color.FromArgb(90, 160, 230);
		this.linkLabelLoginByCode.BackColor = System.Drawing.Color.Transparent;
		this.linkLabelLoginByCode.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkLabelLoginByCode.ForeColor = System.Drawing.Color.FromArgb(107, 119, 133);
		this.linkLabelLoginByCode.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
		this.linkLabelLoginByCode.LinkColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.linkLabelLoginByCode.Location = new System.Drawing.Point(292, 270);
		this.linkLabelLoginByCode.Name = "linkLabelLoginByCode";
		this.linkLabelLoginByCode.Size = new System.Drawing.Size(150, 39);
		this.linkLabelLoginByCode.TabIndex = 18;
		this.linkLabelLoginByCode.TabStop = true;
		this.linkLabelLoginByCode.Text = "验证码登录";
		this.linkLabelLoginByCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelUnderLineLoginByPassword.BackColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.labelUnderLineLoginByPassword.Location = new System.Drawing.Point(165, 309);
		this.labelUnderLineLoginByPassword.Name = "labelUnderLineLoginByPassword";
		this.labelUnderLineLoginByPassword.Size = new System.Drawing.Size(105, 4);
		this.labelUnderLineLoginByPassword.TabIndex = 19;
		this.labelUnderLineLoginByPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelUnderLineLoginByCode.BackColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.labelUnderLineLoginByCode.Location = new System.Drawing.Point(315, 309);
		this.labelUnderLineLoginByCode.Name = "labelUnderLineLoginByCode";
		this.labelUnderLineLoginByCode.Size = new System.Drawing.Size(105, 4);
		this.labelUnderLineLoginByCode.TabIndex = 20;
		this.labelUnderLineLoginByCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelSperator1.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
		this.labelSperator1.Location = new System.Drawing.Point(232, 507);
		this.labelSperator1.Name = "labelSperator1";
		this.labelSperator1.Size = new System.Drawing.Size(3, 22);
		this.labelSperator1.TabIndex = 21;
		this.labelSperator2.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
		this.labelSperator2.Location = new System.Drawing.Point(420, 507);
		this.labelSperator2.Name = "labelSperator2";
		this.labelSperator2.Size = new System.Drawing.Size(3, 22);
		this.labelSperator2.TabIndex = 22;
		// pnlBrand (隐藏，仅保留字段供 InitPlatform_* 设置 BackgroundImage)
		this.pnlBrand.BackColor = System.Drawing.Color.FromArgb(30, 80, 160);
		this.pnlBrand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.pnlBrand.Dock = System.Windows.Forms.DockStyle.None;
		this.pnlBrand.Location = new System.Drawing.Point(0, 0);
		this.pnlBrand.Name = "pnlBrand";
		this.pnlBrand.Size = new System.Drawing.Size(0, 0);
		this.pnlBrand.TabIndex = 23;
		this.pnlBrand.Paint += new System.Windows.Forms.PaintEventHandler(pnlBrand_Paint);
		this.pnlBrand.Visible = false;
		// picLogo (品牌区 Logo)
		this.picLogo.BackColor = System.Drawing.Color.Transparent;
		this.picLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.picLogo.Location = new System.Drawing.Point(240, 38);
		this.picLogo.Name = "picLogo";
		this.picLogo.Size = new System.Drawing.Size(90, 90);
		this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.picLogo.TabIndex = 0;
		this.picLogo.TabStop = false;
		// lblProductName (品牌区 产品名)
		this.lblProductName.AutoSize = false;
		this.lblProductName.BackColor = System.Drawing.Color.Transparent;
		this.lblProductName.Font = new System.Drawing.Font("Noto Sans SC", 14f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblProductName.ForeColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.lblProductName.Location = new System.Drawing.Point(60, 135);
		this.lblProductName.Name = "lblProductName";
		this.lblProductName.Size = new System.Drawing.Size(450, 38);
		this.lblProductName.TabIndex = 1;
		this.lblProductName.Text = "审计AI";
		this.lblProductName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		// lblEnterpriseName (品牌区 企业名称，预留)
		this.lblEnterpriseName.AutoSize = false;
		this.lblEnterpriseName.BackColor = System.Drawing.Color.Transparent;
		this.lblEnterpriseName.Font = new System.Drawing.Font("Noto Sans SC", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEnterpriseName.ForeColor = System.Drawing.Color.FromArgb(107, 119, 133);
		this.lblEnterpriseName.Location = new System.Drawing.Point(60, 172);
		this.lblEnterpriseName.Name = "lblEnterpriseName";
		this.lblEnterpriseName.Size = new System.Drawing.Size(450, 30);
		this.lblEnterpriseName.TabIndex = 2;
		this.lblEnterpriseName.Text = "";
		this.lblEnterpriseName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		// lblWelcomeTitle (登录区 "欢迎登录" 标题)
		this.lblWelcomeTitle.AutoSize = false;
		this.lblWelcomeTitle.BackColor = System.Drawing.Color.Transparent;
		this.lblWelcomeTitle.Font = new System.Drawing.Font("Noto Sans SC", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblWelcomeTitle.ForeColor = System.Drawing.Color.FromArgb(74, 144, 226);
		this.lblWelcomeTitle.Location = new System.Drawing.Point(60, 210);
		this.lblWelcomeTitle.Name = "lblWelcomeTitle";
		this.lblWelcomeTitle.Size = new System.Drawing.Size(450, 45);
		this.lblWelcomeTitle.TabIndex = 23;
		this.lblWelcomeTitle.Text = "欢迎登录";
		this.lblWelcomeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		// pnlLoginRight (居中卡片 380x480)
		this.pnlLoginRight.BackColor = System.Drawing.Color.Transparent;
		this.pnlLoginRight.Controls.Add(this.lblWelcomeTitle);
		this.pnlLoginRight.Controls.Add(this.lblVersion);
		this.pnlLoginRight.Controls.Add(this.labelSperator2);
		this.pnlLoginRight.Controls.Add(this.labelSperator1);
		this.pnlLoginRight.Controls.Add(this.labelUnderLineLoginByCode);
		this.pnlLoginRight.Controls.Add(this.labelUnderLineLoginByPassword);
		this.pnlLoginRight.Controls.Add(this.linkLabelLoginByCode);
		this.pnlLoginRight.Controls.Add(this.linkLabelLoginByPassword);
		this.pnlLoginRight.Controls.Add(this.progressBar1);
		this.pnlLoginRight.Controls.Add(this.RememberPwd);
		this.pnlLoginRight.Controls.Add(this.linkForgetPwd);
		this.pnlLoginRight.Controls.Add(this.linkRegister);
		this.pnlLoginRight.Controls.Add(this.ctnPassword);
		this.pnlLoginRight.Controls.Add(this.ctnUserName);
		this.pnlLoginRight.Controls.Add(this.btnClose);
		this.pnlLoginRight.Controls.Add(this.btnLogin);
		this.pnlLoginRight.Controls.Add(this.lblEnterpriseName);
		this.pnlLoginRight.Controls.Add(this.lblProductName);
		this.pnlLoginRight.Controls.Add(this.picLogo);
		this.pnlLoginRight.Dock = System.Windows.Forms.DockStyle.None;
		this.pnlLoginRight.Location = new System.Drawing.Point(75, 75);
		this.pnlLoginRight.Name = "pnlLoginRight";
		this.pnlLoginRight.Size = new System.Drawing.Size(570, 720);
		this.pnlLoginRight.TabIndex = 24;
		base.AcceptButton = this.btnLogin;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
		base.CancelButton = this.btnClose;
		base.ClientSize = new System.Drawing.Size(720, 870);
		base.Controls.Add(this.pnlLoginRight);
		this.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "frmLogin";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "登录";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmLogin_FormClosing);
		base.Shown += new System.EventHandler(frmLogin_Shown);
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmLogin_Paint);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(frmLogin_MouseDown);
		((System.ComponentModel.ISupportInitialize)this.btnLogin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnUserName).EndInit();
		this.ctnUserName.ResumeLayout(false);
		this.pnlUserName.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.picturePhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhoneNumber).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnPassword).EndInit();
		this.ctnPassword.ResumeLayout(false);
		this.pnlPassword.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txtValidateCode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnSendCode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.RememberPwd).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblVersion).EndInit();
		this.pnlBrand.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.picLogo).EndInit();
		this.pnlLoginRight.ResumeLayout(false);
		this.pnlLoginRight.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
