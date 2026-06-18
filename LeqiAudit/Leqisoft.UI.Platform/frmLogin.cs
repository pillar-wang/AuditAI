﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.PlatformResource;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;

namespace Leqisoft.UI.Platform;

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

	private Leqisoft.Model.User _loginedUser;

	private Color _leqiMainColor = Color.FromArgb(0, 195, 245);

	private Color _leqiMainColorButton = Color.FromArgb(0, 195, 245);

	private LoginType _loginType;

	private Font _loginLinkNormalFont = new Font("微软雅黑", 10.5f);

	private Font _loginLinkFocusFont = new Font("微软雅黑", 10.5f, FontStyle.Bold);

	private Font _passwordEmptyFont = new Font("微软雅黑", 9f);

	private Font _passwordExistValueFont = new Font("微软雅黑", 9f, FontStyle.Bold);

	private Color _loginLinkNormalColor = Color.DimGray;

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

	private C1Button btnQQLogin;

	private C1SuperTooltip c1SuperTooltip1;

	private Panel panel1;

	private C1Label c1Label1;

	private C1Button btnWechatLogin;

	private Splitter splitter1;

	private WinformProgressBarEx progressBar1;

	private C1Label lblVersion;

	private C1Label lblHelpDoc;

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

	public frmLogin()
	{
		InitializeComponent();
		base.Shown += FrmLogin_Shown;
		Initialize();
	}

	private void FrmLogin_Shown(object sender, EventArgs e)
	{
	}

	private void Initialize()
	{
		if (Program.IsOnPremise)
		{
			_tooltipPleaseInputUserName = "请输入用户名";
		}
		InitPlatformStyle();
		txtUserName.TextDetached = true;
		txtPassword.TextDetached = true;
		lblVersion.TextDetached = true;
		lblVersion.Text = "版本号：" + WebApiClient.AppVersion;
		c1SuperTooltip1.SetToolTip(btnClose, "关闭");
		c1SuperTooltip1.SetToolTip(btnQQLogin, "QQ登录");
		c1SuperTooltip1.SetToolTip(btnWechatLogin, "微信登录");
		lblHelpDoc.Text = "官方客服电话：400-690-6500";
		lblHelpDoc.ForeColor = Color.Red;
		base.StartPosition = FormStartPosition.CenterScreen;
		string code = MachineCode.Code;
		UserToken token = TokenTimer.Token;
		linkForgetPwd.Visible = !Program.IsOnPremise;
		linkRegister.Visible = !Program.IsOnPremise;
		panel1.Visible = !Program.IsOnPremise;
		c1Label1.Visible = !Program.IsOnPremise;
		btnQQLogin.Visible = !Program.IsOnPremise;
		btnWechatLogin.Visible = !Program.IsOnPremise;
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
		_leqiMainColorButton = PlatformColorManager.GetMainColorButton(Program.ClientPlatformType);
		_leqiMainColor = PlatformColorManager.GetMainColorDark(Program.ClientPlatformType);
		btnLogin.BackColor = _leqiMainColorButton;
		linkRegister.LinkColor = _leqiMainColor;
		progressBar1.ProgressBarColor = _leqiMainColor;
	}

	private void InitPlatform_Audit()
	{
		BackgroundImage = Resources.login_bg_audit;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Audit);
	}

	private void InitPlatform_Report()
	{
		BackgroundImage = Resources.login_bg_report;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Report);
	}

	private void InitPlatform_Manager()
	{
		BackgroundImage = Resources.login_bg_manager;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Manager);
	}

	private void InitPlatform_TableDevelop()
	{
		BackgroundImage = Resources.login_bg_table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_ProductionCostAccountingSystem()
	{
		BackgroundImage = Resources.login_bg_production_cost;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_ContractLedgerManagementSystem()
	{
		BackgroundImage = Resources.login_bg_contract_ledger;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_RDExpenseLedgerSystem()
	{
		BackgroundImage = Resources.login_bg_rd_expense;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_SalesOrderManagementSystem()
	{
		BackgroundImage = Resources.login_bg_sales_order;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_PSIManagementSystem()
	{
		BackgroundImage = Resources.login_bg_psi_management;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_ProjectLedgerManagementSystem()
	{
		BackgroundImage = Resources.login_bg_project_ledger;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmLoginIcon_Table);
	}

	private void InitPlatform_Custom()
	{
		BackgroundImage = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\login_form_bg.png")));
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
		labelSperator1.BackColor = _leqiMainColor;
		labelSperator2.BackColor = _leqiMainColor;
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
			linkLabelLoginByCode.ActiveLinkColor = _leqiMainColor;
			labelUnderLineLoginByPassword.Visible = true;
			labelUnderLineLoginByPassword.BackColor = _leqiMainColor;
			linkLabelLoginByPassword.LinkColor = _leqiMainColor;
			linkLabelLoginByPassword.Font = _loginLinkFocusFont;
			linkLabelLoginByPassword.ActiveLinkColor = _leqiMainColor;
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
			linkLabelLoginByPassword.ActiveLinkColor = _leqiMainColor;
			labelUnderLineLoginByCode.Visible = true;
			linkLabelLoginByCode.Visible = true;
			labelUnderLineLoginByCode.BackColor = _leqiMainColor;
			linkLabelLoginByCode.LinkColor = _leqiMainColor;
			linkLabelLoginByCode.Font = _loginLinkFocusFont;
			linkLabelLoginByCode.LinkColor = _leqiMainColor;
			btnSendCode.Visible = true;
			btnSendCode.BackColor = _leqiMainColorButton;
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
			btnQQLogin.Enabled = !Program.IsOnPremise;
			btnWechatLogin.Enabled = !Program.IsOnPremise;
			progressBar1.Visible = false;
			btnLogin.Text = "登录";
			break;
		case Status.Logining:
			SetControlEnabled(btnLogin, enabled: false);
			btnQQLogin.Enabled = false;
			btnWechatLogin.Enabled = false;
			progressBar1.Visible = false;
			btnLogin.Text = "登录中...";
			break;
		case Status.Checking:
			SetControlEnabled(btnLogin, enabled: false);
			btnQQLogin.Enabled = false;
			btnWechatLogin.Enabled = false;
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
					Task<Tuple<UserToken, Leqisoft.DTO.User>> task = WebApiClient.AccountLoginBySMS(text, text2);
					return await (await task.ContinueWith(async delegate(Task<Tuple<UserToken, Leqisoft.DTO.User>> t)
					{
						Leqisoft.DTO.User item2 = (await t).Item2;
						_loginedUser = new Leqisoft.Model.User
						{
							Id = item2.Id,
							Name = item2.Name,
							UserName = item2.UserName,
							TelPhone = item2.Phone,
							IsSystemAdmin = item2.IsSystemAdmin
						};
						Leqisoft.Model.User.Current = _loginedUser;
						return (object)null;
					}));
				}
				string userName = txtUserName.Text;
				string inputPassword = _inputPassword;
				Task<Tuple<UserToken, Leqisoft.DTO.User>> task2 = WebApiClient.AccountLogin(userName, inputPassword);
				return await (await task2.ContinueWith(async delegate(Task<Tuple<UserToken, Leqisoft.DTO.User>> t)
				{
					Leqisoft.DTO.User item = (await t).Item2;
					_loginedUser = new Leqisoft.Model.User
					{
						Id = item.Id,
						Name = item.Name,
						UserName = item.UserName,
						TelPhone = item.Phone,
						IsSystemAdmin = item.IsSystemAdmin
					};
					Leqisoft.Model.User.Current = _loginedUser;
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
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			SwitchStatusTo(Status.Normal);
		}
		catch (ServerException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
			SwitchStatusTo(Status.Normal);
		}
		catch (HttpRequestException ex3)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
			SwitchStatusTo(Status.Normal);
		}
		catch (TimeoutException ex4)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
			SwitchStatusTo(Status.Normal);
		}
		catch (Exception ex5)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.ToString());
			SwitchStatusTo(Status.Normal);
		}
	}

	private async void btnQQLogin_Click(object sender, EventArgs e)
	{
		QQLoginForm qQLoginForm = new QQLoginForm();
		Tuple<UserToken, Leqisoft.DTO.User> tuple = qQLoginForm.ShowDialog();
		if (tuple == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "QQ 授权失败");
		}
		else if (tuple.Item1 == null)
		{
			frmRegister frmRegister2 = new frmRegister(thirdLogin: true);
			frmRegister2.QQId = tuple.Item2.QQId;
			frmRegister2._Picture = tuple.Item2.Picture;
			if (frmRegister2.ShowDialog() == DialogResult.OK)
			{
				SwitchStatusTo(Status.Logining);
				try
				{
					_loginedUser = new Leqisoft.Model.User
					{
						Id = frmRegister2.UserId,
						Name = frmRegister2.Truename,
						UserName = frmRegister2.UserName,
						TelPhone = frmRegister2.TelPhone
					};
					Leqisoft.Model.User.Current = _loginedUser;
					_loginedUser.CreateProfileFolderIfNotExist();
					LoginInfo loginInfo = new LoginInfo
					{
						userId = frmRegister2.UserId,
						userName = frmRegister2.UserName,
						password = frmRegister2.QQId,
						LoginMode = LoginMode.QQ
					};
					TokenTimer.LoginInfo = loginInfo;
					Tuple<UserToken, Leqisoft.DTO.User> tuple2 = await WebApiClient.QQRelogin(frmRegister2.UserId, frmRegister2.QQId);
					TokenTimer.Token = tuple2.Item1;
					Leqisoft.Model.User.Current.TelPhone = tuple2.Item2.Phone;
					if (await GetAndOpenTeam())
					{
						base.DialogResult = DialogResult.OK;
						Close();
					}
					else
					{
						SwitchStatusTo(Status.Normal);
					}
					return;
				}
				catch (Exception)
				{
					SwitchStatusTo(Status.Normal);
					return;
				}
			}
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "注册失败 暂不能使用QQ登录");
		}
		else if (tuple.Item1 != null)
		{
			SwitchStatusTo(Status.Logining);
			Leqisoft.DTO.User item = tuple.Item2;
			_loginedUser = new Leqisoft.Model.User
			{
				Id = item.Id,
				Name = item.Name,
				UserName = item.UserName,
				TelPhone = item.Phone
			};
			Leqisoft.Model.User.Current = _loginedUser;
			_loginedUser.CreateProfileFolderIfNotExist();
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
	}

	private async void btnWechatLogin_Click(object sender, EventArgs e)
	{
		WechatLogin wechatLogin = new WechatLogin();
		Tuple<UserToken, Leqisoft.DTO.User> tuple = wechatLogin.ShowDialog();
		if (tuple == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "微信登录授权失败");
		}
		else if (tuple.Item1 == null)
		{
			frmRegister frmRegister2 = new frmRegister(thirdLogin: true);
			frmRegister2.WechatId = tuple.Item2.WechatId;
			frmRegister2._Picture = tuple.Item2.Picture;
			if (frmRegister2.ShowDialog() == DialogResult.OK)
			{
				SwitchStatusTo(Status.Logining);
				try
				{
					_loginedUser = new Leqisoft.Model.User
					{
						Id = frmRegister2.UserId,
						Name = frmRegister2.Truename,
						UserName = frmRegister2.UserName,
						TelPhone = frmRegister2.TelPhone
					};
					Leqisoft.Model.User.Current = _loginedUser;
					_loginedUser.CreateProfileFolderIfNotExist();
					LoginInfo loginInfo = new LoginInfo
					{
						userId = frmRegister2.UserId,
						userName = frmRegister2.UserName,
						password = frmRegister2.WechatId,
						LoginMode = LoginMode.Wechat
					};
					TokenTimer.LoginInfo = loginInfo;
					Tuple<UserToken, Leqisoft.DTO.User> tuple2 = await WebApiClient.WechatRelogin(frmRegister2.UserId, frmRegister2.WechatId);
					TokenTimer.Token = tuple2.Item1;
					Leqisoft.Model.User.Current.TelPhone = tuple2.Item2.Phone;
					if (await GetAndOpenTeam())
					{
						base.DialogResult = DialogResult.OK;
						Close();
					}
					else
					{
						SwitchStatusTo(Status.Normal);
					}
					return;
				}
				catch (Exception)
				{
					SwitchStatusTo(Status.Normal);
					return;
				}
			}
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "注册失败 暂不能使用微信登录");
		}
		else if (tuple.Item1 != null)
		{
			SwitchStatusTo(Status.Logining);
			_loginedUser = new Leqisoft.Model.User
			{
				Id = tuple.Item2.Id,
				UserName = tuple.Item2.UserName,
				Name = tuple.Item2.Name,
				TelPhone = tuple.Item2.Phone
			};
			Leqisoft.Model.User.Current = _loginedUser;
			_loginedUser.CreateProfileFolderIfNotExist();
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
		Leqisoft.Model.User.Current.IsSystemSupporter = isSystemSupporter;
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
			_loginedUser = new Leqisoft.Model.User
			{
				Id = frmRegister2.UserId,
				Name = frmRegister2.Truename,
				UserName = frmRegister2.UserName,
				TelPhone = frmRegister2.TelPhone
			};
			Leqisoft.Model.User.Current = _loginedUser;
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
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
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
			ctnUserName.BorderColor = _leqiMainColor;
		};
		pnlUserName.MouseLeave += delegate
		{
			ctnUserName.BorderColor = Color.LightGray;
		};
		ctnUserName.MouseEnter += delegate
		{
			ctnUserName.BorderColor = _leqiMainColor;
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
			pnlPassword.BorderColor = _leqiMainColor;
		}
		void MouseEnter_Phone(object s3, EventArgs e3)
		{
			pnlUserName.BorderColor = _leqiMainColor;
		}
		void MouseEnter_UserName(object s3, EventArgs e3)
		{
			pnlUserName.BorderColor = _leqiMainColor;
		}
		void MouseEnter_ValidateCode(object s3, EventArgs e3)
		{
			pnlPassword.BorderColor = _leqiMainColor;
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

	private void butQQLogin_MouseEnter(object sender, EventArgs e)
	{
		Cursor = Cursors.Hand;
	}

	private void butQQLogin_MouseLeave(object sender, EventArgs e)
	{
		Cursor = Cursors.Default;
	}

	private void btn_wx_login_MouseEnter(object sender, EventArgs e)
	{
		Cursor = Cursors.Hand;
	}

	private void btn_wx_login_MouseLeave(object sender, EventArgs e)
	{
		Cursor = Cursors.Default;
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
		Pen pen = new Pen(_leqiMainColor, 1f);
		e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
	}

	private void lblHelpDoc_Click(object sender, EventArgs e)
	{
		try
		{
			string url = "";
			Leqisoft.UI.Controls.Util.ShellExecuteUrl(url);
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
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
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "手机号码格式不正确，请检查是否是11位数字！");
				return;
			}
			btnSendCode.Start(120);
			await WebApiClient.GetCodeByName(await WebApiClient.GetUsernameByPhone(text), "5");
			isSuccessSend = true;
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (Exception ex3)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
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
		this.txtPhoneNumber = new Leqisoft.UI.Controls.C1TextBoxEx();
		this.picUserName = new C1.Win.C1Input.C1PictureBox();
		this.txtUserName = new Leqisoft.UI.Controls.C1TextBoxEx();
		this.ctnPassword = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlPassword = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtValidateCode = new Leqisoft.UI.Controls.C1TextBoxEx();
		this.btnSendCode = new Leqisoft.UI.Controls.TimerButton();
		this.picPassword = new C1.Win.C1Input.C1PictureBox();
		this.txtPassword = new Leqisoft.UI.Controls.C1TextBoxEx();
		this.linkForgetPwd = new System.Windows.Forms.LinkLabel();
		this.linkRegister = new System.Windows.Forms.LinkLabel();
		this.RememberPwd = new C1.Win.C1Input.C1CheckBox();
		this.c1SuperTooltip1 = new C1.Win.C1SuperTooltip.C1SuperTooltip(this.components);
		this.panel1 = new System.Windows.Forms.Panel();
		this.btnWechatLogin = new C1.Win.C1Input.C1Button();
		this.btnQQLogin = new C1.Win.C1Input.C1Button();
		this.splitter1 = new System.Windows.Forms.Splitter();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.btnClose = new C1.Win.C1Input.C1Button();
		this.lblVersion = new C1.Win.C1Input.C1Label();
		this.lblHelpDoc = new C1.Win.C1Input.C1Label();
		this.progressBar1 = new Leqisoft.UI.Controls.WinformProgressBarEx();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.linkLabelLoginByPassword = new System.Windows.Forms.LinkLabel();
		this.linkLabelLoginByCode = new System.Windows.Forms.LinkLabel();
		this.labelUnderLineLoginByPassword = new System.Windows.Forms.Label();
		this.labelUnderLineLoginByCode = new System.Windows.Forms.Label();
		this.labelSperator1 = new System.Windows.Forms.Label();
		this.labelSperator2 = new System.Windows.Forms.Label();
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
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnWechatLogin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnQQLogin).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblVersion).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblHelpDoc).BeginInit();
		base.SuspendLayout();
		this.btnLogin.BackColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.btnLogin.FlatAppearance.BorderSize = 0;
		this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnLogin.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnLogin.ForeColor = System.Drawing.Color.White;
		this.btnLogin.Location = new System.Drawing.Point(398, 289);
		this.btnLogin.Name = "btnLogin";
		this.btnLogin.Size = new System.Drawing.Size(260, 44);
		this.btnLogin.TabIndex = 3;
		this.btnLogin.Text = "登录";
		this.btnLogin.UseVisualStyleBackColor = false;
		this.btnLogin.Click += new System.EventHandler(btnLogin_Click);
		this.ctnUserName.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnUserName.BackColor = System.Drawing.Color.Transparent;
		this.ctnUserName.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnUserName.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnUserName.Location = new System.Drawing.Point(398, 104);
		this.ctnUserName.Name = "ctnUserName";
		this.ctnUserName.Panels.Add(this.pnlUserName);
		this.ctnUserName.Size = new System.Drawing.Size(260, 40);
		this.ctnUserName.TabIndex = 6;
		this.ctnUserName.UseParentVisualStyle = false;
		this.pnlUserName.BackColor = System.Drawing.Color.Transparent;
		this.pnlUserName.BorderColor = System.Drawing.Color.LightGray;
		this.pnlUserName.BorderWidth = 1;
		this.pnlUserName.Controls.Add(this.picturePhone);
		this.pnlUserName.Controls.Add(this.txtPhoneNumber);
		this.pnlUserName.Controls.Add(this.picUserName);
		this.pnlUserName.Controls.Add(this.txtUserName);
		this.pnlUserName.Height = 40;
		this.pnlUserName.Location = new System.Drawing.Point(1, 1);
		this.pnlUserName.Name = "pnlUserName";
		this.pnlUserName.Size = new System.Drawing.Size(258, 38);
		this.pnlUserName.TabIndex = 0;
		this.pnlUserName.Click += new System.EventHandler(pnlUserName_Click);
		this.picturePhone.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.phoneLogin;
		this.picturePhone.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.picturePhone.Location = new System.Drawing.Point(10, 10);
		this.picturePhone.Name = "picturePhone";
		this.picturePhone.Size = new System.Drawing.Size(18, 18);
		this.picturePhone.TabIndex = 2;
		this.picturePhone.TabStop = false;
		this.txtPhoneNumber.AutoSize = false;
		this.txtPhoneNumber.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtPhoneNumber.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhoneNumber.Location = new System.Drawing.Point(45, 3);
		this.txtPhoneNumber.Name = "txtPhoneNumber";
		this.txtPhoneNumber.Size = new System.Drawing.Size(208, 32);
		this.txtPhoneNumber.TabIndex = 0;
		this.txtPhoneNumber.Tag = null;
		this.txtPhoneNumber.TextDetached = true;
		this.txtPhoneNumber.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.picUserName.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.userlogin;
		this.picUserName.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.picUserName.Location = new System.Drawing.Point(10, 10);
		this.picUserName.Name = "picUserName";
		this.picUserName.Size = new System.Drawing.Size(18, 18);
		this.picUserName.TabIndex = 1;
		this.picUserName.TabStop = false;
		this.txtUserName.AutoSize = false;
		this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtUserName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtUserName.Location = new System.Drawing.Point(45, 3);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.Size = new System.Drawing.Size(208, 32);
		this.txtUserName.TabIndex = 0;
		this.txtUserName.Tag = null;
		this.txtUserName.TextDetached = true;
		this.txtUserName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.ctnPassword.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnPassword.BackColor = System.Drawing.Color.Transparent;
		this.ctnPassword.BorderColor = System.Drawing.Color.LightGray;
		this.ctnPassword.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnPassword.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnPassword.Location = new System.Drawing.Point(398, 171);
		this.ctnPassword.Name = "ctnPassword";
		this.ctnPassword.Panels.Add(this.pnlPassword);
		this.ctnPassword.Size = new System.Drawing.Size(260, 40);
		this.ctnPassword.TabIndex = 7;
		this.ctnPassword.UseParentVisualStyle = false;
		this.pnlPassword.BorderColor = System.Drawing.Color.LightGray;
		this.pnlPassword.BorderWidth = 1;
		this.pnlPassword.Controls.Add(this.txtValidateCode);
		this.pnlPassword.Controls.Add(this.btnSendCode);
		this.pnlPassword.Controls.Add(this.picPassword);
		this.pnlPassword.Controls.Add(this.txtPassword);
		this.pnlPassword.Height = 40;
		this.pnlPassword.Location = new System.Drawing.Point(1, 1);
		this.pnlPassword.Name = "pnlPassword";
		this.pnlPassword.Size = new System.Drawing.Size(258, 38);
		this.pnlPassword.TabIndex = 0;
		this.pnlPassword.Click += new System.EventHandler(pnlPassword_Click);
		this.txtValidateCode.AutoSize = false;
		this.txtValidateCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtValidateCode.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtValidateCode.Location = new System.Drawing.Point(45, 3);
		this.txtValidateCode.Name = "txtValidateCode";
		this.txtValidateCode.Size = new System.Drawing.Size(110, 32);
		this.txtValidateCode.TabIndex = 1;
		this.txtValidateCode.Tag = null;
		this.txtValidateCode.TextDetached = true;
		this.txtValidateCode.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.btnSendCode.BackColor = System.Drawing.Color.LightGray;
		this.btnSendCode.FlatAppearance.BorderSize = 0;
		this.btnSendCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSendCode.ForeColor = System.Drawing.Color.White;
		this.btnSendCode.Format = "(0s)";
		this.btnSendCode.Location = new System.Drawing.Point(164, 5);
		this.btnSendCode.Name = "btnSendCode";
		this.btnSendCode.Size = new System.Drawing.Size(90, 30);
		this.btnSendCode.TabIndex = 2;
		this.btnSendCode.Text = "获取验证码";
		this.btnSendCode.UseVisualStyleBackColor = false;
		this.btnSendCode.Click += new System.EventHandler(btnSendCode_Click);
		this.picPassword.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.password;
		this.picPassword.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.picPassword.Location = new System.Drawing.Point(11, 10);
		this.picPassword.Name = "picPassword";
		this.picPassword.Size = new System.Drawing.Size(18, 18);
		this.picPassword.TabIndex = 1;
		this.picPassword.TabStop = false;
		this.txtPassword.AutoSize = false;
		this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtPassword.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword.Location = new System.Drawing.Point(45, 3);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '●';
		this.txtPassword.Size = new System.Drawing.Size(208, 32);
		this.txtPassword.TabIndex = 1;
		this.txtPassword.Tag = null;
		this.txtPassword.TextDetached = true;
		this.txtPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.linkForgetPwd.AutoSize = true;
		this.linkForgetPwd.BackColor = System.Drawing.Color.Transparent;
		this.linkForgetPwd.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkForgetPwd.LinkColor = System.Drawing.Color.DimGray;
		this.linkForgetPwd.Location = new System.Drawing.Point(486, 247);
		this.linkForgetPwd.Name = "linkForgetPwd";
		this.linkForgetPwd.Size = new System.Drawing.Size(104, 17);
		this.linkForgetPwd.TabIndex = 1;
		this.linkForgetPwd.TabStop = true;
		this.linkForgetPwd.Text = "忘记用户名或密码";
		this.linkForgetPwd.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkForgetPwd_LinkClicked);
		this.linkForgetPwd.MouseEnter += new System.EventHandler(linkForgetPwd_MouseEnter);
		this.linkForgetPwd.MouseLeave += new System.EventHandler(linkForgetPwd_MouseLeave);
		this.linkRegister.AutoSize = true;
		this.linkRegister.BackColor = System.Drawing.Color.Transparent;
		this.linkRegister.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkRegister.LinkColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.linkRegister.Location = new System.Drawing.Point(602, 247);
		this.linkRegister.Name = "linkRegister";
		this.linkRegister.Size = new System.Drawing.Size(56, 17);
		this.linkRegister.TabIndex = 2;
		this.linkRegister.TabStop = true;
		this.linkRegister.Text = "立即注册";
		this.linkRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkRegister_LinkClicked);
		this.linkRegister.MouseEnter += new System.EventHandler(linkRegister_MouseEnter);
		this.linkRegister.MouseLeave += new System.EventHandler(linkRegister_MouseLeave);
		this.RememberPwd.BackColor = System.Drawing.Color.Transparent;
		this.RememberPwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.RememberPwd.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.RememberPwd.Location = new System.Drawing.Point(400, 245);
		this.RememberPwd.Name = "RememberPwd";
		this.RememberPwd.Size = new System.Drawing.Size(75, 24);
		this.RememberPwd.TabIndex = 8;
		this.RememberPwd.Text = "记住密码";
		this.RememberPwd.UseVisualStyleBackColor = false;
		this.RememberPwd.Value = null;
		this.c1SuperTooltip1.Font = new System.Drawing.Font("Tahoma", 8f);
		this.c1SuperTooltip1.RightToLeft = System.Windows.Forms.RightToLeft.Inherit;
		this.c1SuperTooltip1.Shadow = false;
		this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.panel1.Controls.Add(this.btnWechatLogin);
		this.panel1.Controls.Add(this.btnQQLogin);
		this.panel1.Controls.Add(this.splitter1);
		this.panel1.Location = new System.Drawing.Point(398, 359);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(260, 75);
		this.panel1.TabIndex = 12;
		this.btnWechatLogin.BackColor = System.Drawing.Color.Transparent;
		this.btnWechatLogin.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.wechat_img;
		this.btnWechatLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.btnWechatLogin.FlatAppearance.BorderColor = System.Drawing.Color.White;
		this.btnWechatLogin.FlatAppearance.BorderSize = 0;
		this.btnWechatLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
		this.btnWechatLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
		this.btnWechatLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnWechatLogin.Location = new System.Drawing.Point(173, 27);
		this.btnWechatLogin.Name = "btnWechatLogin";
		this.btnWechatLogin.Size = new System.Drawing.Size(32, 32);
		this.btnWechatLogin.TabIndex = 13;
		this.btnWechatLogin.UseVisualStyleBackColor = false;
		this.btnWechatLogin.Click += new System.EventHandler(btnWechatLogin_Click);
		this.btnWechatLogin.MouseEnter += new System.EventHandler(btn_wx_login_MouseEnter);
		this.btnWechatLogin.MouseLeave += new System.EventHandler(btn_wx_login_MouseLeave);
		this.btnQQLogin.BackColor = System.Drawing.Color.Transparent;
		this.btnQQLogin.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.qq_img;
		this.btnQQLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.btnQQLogin.FlatAppearance.BorderColor = System.Drawing.Color.White;
		this.btnQQLogin.FlatAppearance.BorderSize = 0;
		this.btnQQLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
		this.btnQQLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
		this.btnQQLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnQQLogin.Location = new System.Drawing.Point(60, 28);
		this.btnQQLogin.Name = "btnQQLogin";
		this.btnQQLogin.Size = new System.Drawing.Size(32, 32);
		this.btnQQLogin.TabIndex = 10;
		this.btnQQLogin.UseVisualStyleBackColor = false;
		this.btnQQLogin.Click += new System.EventHandler(btnQQLogin_Click);
		this.btnQQLogin.MouseEnter += new System.EventHandler(butQQLogin_MouseEnter);
		this.btnQQLogin.MouseLeave += new System.EventHandler(butQQLogin_MouseLeave);
		this.splitter1.BackColor = System.Drawing.Color.DarkGray;
		this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
		this.splitter1.Location = new System.Drawing.Point(0, 0);
		this.splitter1.Name = "splitter1";
		this.splitter1.Size = new System.Drawing.Size(260, 1);
		this.splitter1.TabIndex = 12;
		this.splitter1.TabStop = false;
		this.c1Label1.AutoSize = true;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.ForeColor = System.Drawing.Color.Gray;
		this.c1Label1.Location = new System.Drawing.Point(471, 351);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(116, 17);
		this.c1Label1.TabIndex = 13;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "使用第三方账号登录";
		this.c1Label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.c1Label1.TextDetached = true;
		this.btnClose.BackColor = System.Drawing.Color.Transparent;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Image = Leqisoft.UI.Platform.Properties.Resources.close2;
		this.btnClose.Location = new System.Drawing.Point(705, 0);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(25, 25);
		this.btnClose.TabIndex = 5;
		this.btnClose.UseVisualStyleBackColor = false;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.lblVersion.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.lblVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVersion.Font = new System.Drawing.Font("微软雅黑", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblVersion.ForeColor = System.Drawing.Color.Gray;
		this.lblVersion.Location = new System.Drawing.Point(431, 455);
		this.lblVersion.Name = "lblVersion";
		this.lblVersion.Size = new System.Drawing.Size(292, 18);
		this.lblVersion.TabIndex = 15;
		this.lblVersion.Tag = null;
		this.lblVersion.Text = "Version:";
		this.lblVersion.TextAlign = System.Drawing.ContentAlignment.BottomRight;
		this.lblVersion.TextDetached = true;
		this.lblHelpDoc.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.lblHelpDoc.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblHelpDoc.Font = new System.Drawing.Font("微软雅黑", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblHelpDoc.ForeColor = System.Drawing.Color.Gray;
		this.lblHelpDoc.Location = new System.Drawing.Point(320, 455);
		this.lblHelpDoc.Name = "lblHelpDoc";
		this.lblHelpDoc.Size = new System.Drawing.Size(200, 18);
		this.lblHelpDoc.TabIndex = 16;
		this.lblHelpDoc.Tag = null;
		this.lblHelpDoc.Text = "帮助文档";
		this.lblHelpDoc.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		this.lblHelpDoc.TextDetached = true;
		this.progressBar1.BackColor = System.Drawing.Color.White;
		this.progressBar1.ColorChunkLength = 100;
		this.progressBar1.ColorChunkMoveSpeed = 150;
		this.progressBar1.Location = new System.Drawing.Point(1, 474);
		this.progressBar1.MarqueeAnimationSpeed = 5;
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.ProgressBarColor = System.Drawing.Color.Green;
		this.progressBar1.Size = new System.Drawing.Size(728, 5);
		this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.progressBar1.TabIndex = 14;
		this.timer1.Interval = 5;
		this.linkLabelLoginByPassword.ActiveLinkColor = System.Drawing.Color.Silver;
		this.linkLabelLoginByPassword.BackColor = System.Drawing.Color.Transparent;
		this.linkLabelLoginByPassword.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkLabelLoginByPassword.ForeColor = System.Drawing.Color.Silver;
		this.linkLabelLoginByPassword.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
		this.linkLabelLoginByPassword.LinkColor = System.Drawing.Color.Silver;
		this.linkLabelLoginByPassword.Location = new System.Drawing.Point(395, 57);
		this.linkLabelLoginByPassword.Name = "linkLabelLoginByPassword";
		this.linkLabelLoginByPassword.Size = new System.Drawing.Size(130, 26);
		this.linkLabelLoginByPassword.TabIndex = 17;
		this.linkLabelLoginByPassword.TabStop = true;
		this.linkLabelLoginByPassword.Text = "账号密码登录";
		this.linkLabelLoginByPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.linkLabelLoginByCode.ActiveLinkColor = System.Drawing.Color.Silver;
		this.linkLabelLoginByCode.BackColor = System.Drawing.Color.Transparent;
		this.linkLabelLoginByCode.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkLabelLoginByCode.ForeColor = System.Drawing.Color.Silver;
		this.linkLabelLoginByCode.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
		this.linkLabelLoginByCode.LinkColor = System.Drawing.Color.Silver;
		this.linkLabelLoginByCode.Location = new System.Drawing.Point(554, 57);
		this.linkLabelLoginByCode.Name = "linkLabelLoginByCode";
		this.linkLabelLoginByCode.Size = new System.Drawing.Size(100, 26);
		this.linkLabelLoginByCode.TabIndex = 18;
		this.linkLabelLoginByCode.TabStop = true;
		this.linkLabelLoginByCode.Text = "验证码登录";
		this.linkLabelLoginByCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelUnderLineLoginByPassword.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.labelUnderLineLoginByPassword.Location = new System.Drawing.Point(411, 83);
		this.labelUnderLineLoginByPassword.Name = "labelUnderLineLoginByPassword";
		this.labelUnderLineLoginByPassword.Size = new System.Drawing.Size(96, 3);
		this.labelUnderLineLoginByPassword.TabIndex = 19;
		this.labelUnderLineLoginByPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelUnderLineLoginByCode.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.labelUnderLineLoginByCode.Location = new System.Drawing.Point(562, 85);
		this.labelUnderLineLoginByCode.Name = "labelUnderLineLoginByCode";
		this.labelUnderLineLoginByCode.Size = new System.Drawing.Size(83, 3);
		this.labelUnderLineLoginByCode.TabIndex = 20;
		this.labelUnderLineLoginByCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.labelSperator1.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.labelSperator1.Location = new System.Drawing.Point(478, 248);
		this.labelSperator1.Name = "labelSperator1";
		this.labelSperator1.Size = new System.Drawing.Size(2, 15);
		this.labelSperator1.TabIndex = 21;
		this.labelSperator2.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.labelSperator2.Location = new System.Drawing.Point(595, 248);
		this.labelSperator2.Name = "labelSperator2";
		this.labelSperator2.Size = new System.Drawing.Size(2, 15);
		this.labelSperator2.TabIndex = 22;
		base.AcceptButton = this.btnLogin;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.White;
		this.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.loginpicture1;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.CancelButton = this.btnClose;
		base.ClientSize = new System.Drawing.Size(730, 480);
		base.Controls.Add(this.labelSperator2);
		base.Controls.Add(this.labelSperator1);
		base.Controls.Add(this.labelUnderLineLoginByCode);
		base.Controls.Add(this.labelUnderLineLoginByPassword);
		base.Controls.Add(this.linkLabelLoginByCode);
		base.Controls.Add(this.linkLabelLoginByPassword);
		base.Controls.Add(this.lblHelpDoc);
		base.Controls.Add(this.lblVersion);
		base.Controls.Add(this.progressBar1);
		base.Controls.Add(this.c1Label1);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.RememberPwd);
		base.Controls.Add(this.linkForgetPwd);
		base.Controls.Add(this.linkRegister);
		base.Controls.Add(this.ctnPassword);
		base.Controls.Add(this.ctnUserName);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.btnLogin);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
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
		this.panel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnWechatLogin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnQQLogin).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblVersion).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblHelpDoc).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
