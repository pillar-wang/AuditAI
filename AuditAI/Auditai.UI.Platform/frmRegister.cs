using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1Input;
using Auditai.DTO;
using Auditai.PlatformResource;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class frmRegister : Form
{
	private enum Status
	{
		Normal,
		Registing
	}

	private static Color _auditaiMainColor = Color.FromArgb(0, 195, 245);

	private static Color _auditaiMainColorButton = Color.FromArgb(0, 195, 245);

	private ValidateCodeCreator validateCreator;

	private bool _whetherTxtPass = true;

	private bool _thirdLogin;

	private bool _getsms;

	private string _identCode;

	private Status _status;

	public const int WM_SYSCOMMAND = 274;

	public const int SC_MOVE = 61456;

	public const int HTCAPTION = 2;

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

	private C1TextBoxEx txtUserName;

	private C1TextBoxEx txtPassword;

	private C1TextBoxEx txtPassword2;

	private C1TextBoxEx txtName;

	private C1TextBoxEx txtEmail;

	private C1TextBoxEx txtCompany;

	private C1TextBoxEx txtPhone;

	private C1TextBoxEx txtCity;

	private C1TextBoxEx txtVerification;

	private C1Button btnRegister;

	private WinformRadioButtonEx radMale;

	private WinformRadioButtonEx radFemale;

	private C1Label warnUserName;

	private C1Label warnPassword;

	private C1Label warnPassword2;

	private C1Label warnName;

	private C1Label warnPhone;

	private C1Label lblUserName;

	private C1Label lblPassword2;

	private C1Label lblName;

	private C1Label lblEmail;

	private C1Label lblCompany;

	private C1Label lblSex;

	private C1Label lblCity;

	private C1Label lblVerification;

	private C1Label lblPassword;

	private C1Label lblPhone;

	private C1Label lblRegister;

	private C1Label lblMustInputStar1;

	private C1Label lblMustInputStar2;

	private C1Label lblMustInputStar3;

	private C1Button btnClose;

	private TimerButton btnGetValidateCode;

	private C1DockingTab dockverify;

	private C1DockingTabPage tabImage;

	private C1PictureBox VerifyImg;

	private C1Label c1Label1;

	private C1TextBoxEx txtValidateCode;

	private C1DockingTabPage tabSMS;

	private C1Label c1Label2;

	private C1Label c1Label3;

	private C1Label c1Label4;

	private C1Label lblwarnName;

	private C1Label c1Label5;

	public long UserId { get; set; }

	public string QQId { get; set; }

	public string WechatId { get; set; }

	public string Password { get; set; }

	public string UserName { get; set; }

	public string TelPhone { get; set; }

	public string Truename { get; set; }

	public byte[] _Picture { get; set; }

	public frmRegister(bool thirdLogin = false)
	{
		InitializeComponent();
		base.Shown += FrmRegister_Shown;
		Initialize(thirdLogin);
		dockverify.SelectedTab = tabSMS;
	}

	private void FrmRegister_Shown(object sender, EventArgs e)
	{
	}

	private void Initialize(bool thirdLogin)
	{
		InitPlatformStyle();
		VerifyImg.Cursor = Cursors.Hand;
		dockverify.SelectedTab = tabImage;
		validateCreator = new ValidateCodeCreator
		{
			Length = 4
		};
		VerifyImg.Image = validateCreator.Create(out _identCode, VerifyImg.Width, VerifyImg.Height);
		_thirdLogin = thirdLogin;
		if (_thirdLogin)
		{
			lblMustInputStar2.Visible = false;
			lblMustInputStar3.Visible = false;
			txtPassword.Enabled = false;
			txtPassword2.Enabled = false;
		}
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void InitPlatformStyle()
	{
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

	private void InitColor()
	{
		_auditaiMainColor = PlatformColorManager.GetMainColorDark(Program.ClientPlatformType);
		_auditaiMainColorButton = PlatformColorManager.GetMainColorButton(Program.ClientPlatformType);
		btnRegister.BackColor = _auditaiMainColorButton;
		btnGetValidateCode.BackColor = _auditaiMainColorButton;
		radMale.RadioCircleMouseOverColor = _auditaiMainColor;
		radMale.RadioCircleCheckedColor = _auditaiMainColor;
		radFemale.RadioCircleMouseOverColor = _auditaiMainColor;
		radFemale.RadioCircleCheckedColor = _auditaiMainColor;
	}

	private void InitPlatform_Audit()
	{
		BackgroundImage = Resources.register_bg_audit;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Audit);
	}

	private void InitPlatform_Report()
	{
		BackgroundImage = Resources.register_bg_report;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Report);
	}

	private void InitPlatform_Manager()
	{
		BackgroundImage = Resources.register_bg_manager;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Manager);
	}

	private void InitPlatform_TableDevelop()
	{
		BackgroundImage = Resources.register_bg_table;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Table);
	}

	private void InitPlatform_ProductionCostAccountingSystem()
	{
		BackgroundImage = Resources.register_bg_production_cost;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Table);
	}

	private void InitPlatform_ContractLedgerManagementSystem()
	{
		BackgroundImage = Resources.register_bg_contract_ledger;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Table);
	}

	private void InitPlatform_RDExpenseLedgerSystem()
	{
		BackgroundImage = Resources.register_bg_rd_expense;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Table);
	}

	private void InitPlatform_SalesOrderManagementSystem()
	{
		BackgroundImage = Resources.register_bg_sales_order;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Table);
	}

	private void InitPlatform_PSIManagementSystem()
	{
		BackgroundImage = Resources.register_bg_psi_management;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Table);
	}

	private void InitPlatform_ProjectLedgerManagementSystem()
	{
		BackgroundImage = Resources.register_bg_project_ledger;
		base.Icon = IconGenerator.CreateFromImage(Resources.frmRegisterIcon_Table);
	}

	private void InitPlatform_Custom()
	{
		BackgroundImage = System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\register_form_bg.png")));
		base.Icon = IconGenerator.CreateFromImage((Bitmap)System.Drawing.Image.FromStream(new MemoryStream(ClientCustomizeData.Current.GetFileData("image\\register_form_icon.png"))));
	}

	private bool ValidateAllText()
	{
		_whetherTxtPass = true;
		txtUserName.ValidateText();
		txtPhone.ValidateText();
		txtName.ValidateText();
		if (!_thirdLogin)
		{
			txtPassword.ValidateText();
			txtPassword2.ValidateText();
		}
		return _whetherTxtPass;
	}

	private void SwitchStatusTo(Status status)
	{
		_status = status;
		switch (_status)
		{
		case Status.Normal:
			btnRegister.Enabled = true;
			btnRegister.Text = "注册";
			break;
		case Status.Registing:
			btnRegister.Text = "注册中...";
			btnRegister.Enabled = false;
			break;
		}
	}

	private void frmRegister_Load(object sender, EventArgs e)
	{
		AnimateWindow(base.Handle, 100, 524288);
		Refresh();
		foreach (object control in base.Controls)
		{
			C1TextBox tb = control as C1TextBox;
			if (tb != null)
			{
				tb.MouseEnter += delegate
				{
					tb.BorderColor = _auditaiMainColor;
				};
				tb.MouseLeave += delegate
				{
					tb.BorderColor = Color.LightGray;
				};
			}
		}
		txtUserName.Focus();
		base.AcceptButton = btnRegister;
		base.AutoScaleMode = AutoScaleMode.None;
	}

	private async void btnRegister_Click(object sender, EventArgs e)
	{
		try
		{
			SwitchStatusTo(Status.Registing);
			if (!ValidateAllText())
			{
				return;
			}
			if (Regex.IsMatch(txtUserName.Text.Trim(), "^[0-9]+$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名格式不正确，请至少包含一个字母！");
				return;
			}
			User user = new User
			{
				UserName = txtUserName.Text.Trim(),
				Password = txtPassword.Text.Trim(),
				Name = txtName.Text.Trim(),
				Email = txtEmail.Text.Trim(),
				Phone = txtPhone.Text.Trim(),
				City = txtCity.Text.Trim(),
				Company = txtCompany.Text.Trim(),
				Sex = (radMale.Checked ? "m" : (radFemale.Checked ? "f" : "m")),
				WechatId = WechatId,
				QQId = QQId,
				Picture = _Picture
			};
			if (user.Phone.Length == 0)
			{
				string text = txtValidateCode.Text.Trim();
				if (text.ToLower() != _identCode.ToLower())
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "验证码不正确！请重试");
					VerifyImg.Image = validateCreator.Create(out _identCode, VerifyImg.Width, VerifyImg.Height);
					return;
				}
				user.Phone = null;
				UserId = await WebApiClient.SingleRegister(user);
				UserName = txtUserName.Text.Trim();
				TelPhone = txtPhone.Text.Trim();
				Truename = txtName.Text.Trim();
				Password = Encrypts.SHA256Encrypt(txtPassword.Text.Trim(), isUrl: true);
			}
			else
			{
				if (!_getsms)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "绑定手机号需要先请求短信验证码");
					return;
				}
				if (!Regex.IsMatch(txtVerification.Text.Trim(), "^\\w+$"))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "验证码格式不正确！");
					return;
				}
				UserId = await WebApiClient.Register(user, txtVerification.Text.Trim());
				UserName = txtUserName.Text.Trim();
				TelPhone = txtPhone.Text.Trim();
				Truename = txtName.Text.Trim();
				Password = Encrypts.SHA256Encrypt(txtPassword.Text.Trim(), isUrl: true);
			}
			base.DialogResult = DialogResult.OK;
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "注册成功");
			Close();
		}
		catch (NormalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (ServerException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
		}
		catch (HttpRequestException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
		}
		catch (TimeoutException ex4)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
		finally
		{
			SwitchStatusTo(Status.Normal);
		}
	}

	private async void btnGetValidateCode_Click(object sender, EventArgs e)
	{
		btnGetValidateCode.Format = "(0s)";
		try
		{
			if (ValidateAllText())
			{
				if (!Regex.IsMatch(txtPhone.Text.Trim(), "^[0-9]{11,11}$"))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "手机号码格式不正确，请检查是否是11位数字");
					return;
				}
				if (await WebApiClient.UserNameExists(txtUserName.Text.Trim()))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名已存在！请直接登录或选择其他用户名");
					txtUserName.Focus();
					return;
				}
				if (await WebApiClient.PhoneExists(txtPhone.Text.Trim()))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "手机号已注册！请直接登录或选择其他手机号");
					txtPhone.Focus();
					return;
				}
				btnGetValidateCode.Start(120);
				await WebApiClient.GetValidateCode(txtPhone.Text.Trim(), "1");
				txtPhone.ReadOnly = true;
				txtValidateCode.Focus();
				_getsms = true;
			}
		}
		catch (NormalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (ServerException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
		}
		catch (HttpRequestException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
		}
		catch (TimeoutException ex4)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void txtUserName_Enter(object sender, EventArgs e)
	{
		warnUserName.Visible = true;
	}

	private void txtPassword_Enter(object sender, EventArgs e)
	{
		warnPassword.Visible = true;
	}

	private void txtPassword2_Enter(object sender, EventArgs e)
	{
		warnPassword2.Visible = true;
	}

	private void txtName_Enter(object sender, EventArgs e)
	{
		warnName.Visible = true;
	}

	private void txtPhone_Enter(object sender, EventArgs e)
	{
		warnPhone.Visible = true;
	}

	private void txtUserName_Validated(object sender, EventArgs e)
	{
		string input = txtUserName.Text.Trim();
		if (Regex.IsMatch(input, "^.{2,20}$"))
		{
			SetCorrect(txtUserName, warnUserName);
		}
		else
		{
			SetError(txtUserName, warnUserName);
		}
	}

	private void txtPhone_Validated(object sender, EventArgs e)
	{
		string input = txtPhone.Text.Trim();
		if (Regex.IsMatch(input, "^[0-9]{11}$"))
		{
			SetCorrect(txtPhone, warnPhone);
		}
		else
		{
			SetError(txtPhone, warnPhone);
		}
	}

	private void txtPassword_Validated(object sender, EventArgs e)
	{
		if (!_thirdLogin)
		{
			string input = txtPassword.Text.Trim();
			if (Regex.IsMatch(input, "^\\w{6,20}$"))
			{
				SetCorrect(txtPassword, warnPassword);
			}
			else
			{
				SetError(txtPassword, warnPassword);
			}
		}
	}

	private void txtPassword2_Validated(object sender, EventArgs e)
	{
		if (!_thirdLogin)
		{
			if (txtPassword.Text.Trim() == txtPassword2.Text.Trim())
			{
				SetCorrect(txtPassword2, warnPassword2);
			}
			else
			{
				SetError(txtPassword2, warnPassword2);
			}
		}
	}

	private void txtName_Validated(object sender, EventArgs e)
	{
		string input = txtName.Text.Trim();
		if (Regex.IsMatch(input, "^.{2,20}$"))
		{
			SetCorrect(txtName, lblwarnName);
		}
		else
		{
			SetError(txtName, lblwarnName);
		}
	}

	private void txtPhone_TextChanged(object sender, EventArgs e)
	{
	}

	private void VerifyImg_Click(object sender, EventArgs e)
	{
		VerifyImg.Image = validateCreator.Create(out _identCode, VerifyImg.Width, VerifyImg.Height);
	}

	private void SetCorrect(C1TextBox inputBox, Label warnLable)
	{
		warnLable.ForeColor = Color.Gray;
		warnLable.Visible = false;
		inputBox.BorderColor = Color.LightGray;
	}

	private void SetError(C1TextBox inputBox, Label warnLable)
	{
		_whetherTxtPass = false;
		warnLable.ForeColor = Color.Red;
		warnLable.Visible = true;
		inputBox.BorderColor = Color.Red;
	}

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	private void frmRegister_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

	private void frmRegister_Paint(object sender, PaintEventArgs e)
	{
		Pen pen = new Pen(_auditaiMainColor, 1f);
		e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
	}

	private void frmRegister_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			AnimateWindow(base.Handle, 100, 851968);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Platform.frmRegister));
		this.txtUserName = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtPassword = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtPassword2 = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtName = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtEmail = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtCompany = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtPhone = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtCity = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtVerification = new Auditai.UI.Controls.C1TextBoxEx();
		this.btnRegister = new C1.Win.C1Input.C1Button();
		this.radMale = new Auditai.UI.Controls.WinformRadioButtonEx();
		this.radFemale = new Auditai.UI.Controls.WinformRadioButtonEx();
		this.warnUserName = new C1.Win.C1Input.C1Label();
		this.warnPassword = new C1.Win.C1Input.C1Label();
		this.warnPassword2 = new C1.Win.C1Input.C1Label();
		this.warnName = new C1.Win.C1Input.C1Label();
		this.warnPhone = new C1.Win.C1Input.C1Label();
		this.lblUserName = new C1.Win.C1Input.C1Label();
		this.lblPassword2 = new C1.Win.C1Input.C1Label();
		this.lblName = new C1.Win.C1Input.C1Label();
		this.lblEmail = new C1.Win.C1Input.C1Label();
		this.lblCompany = new C1.Win.C1Input.C1Label();
		this.lblSex = new C1.Win.C1Input.C1Label();
		this.lblCity = new C1.Win.C1Input.C1Label();
		this.lblVerification = new C1.Win.C1Input.C1Label();
		this.lblPassword = new C1.Win.C1Input.C1Label();
		this.lblPhone = new C1.Win.C1Input.C1Label();
		this.lblRegister = new C1.Win.C1Input.C1Label();
		this.lblMustInputStar1 = new C1.Win.C1Input.C1Label();
		this.lblMustInputStar2 = new C1.Win.C1Input.C1Label();
		this.lblMustInputStar3 = new C1.Win.C1Input.C1Label();
		this.btnClose = new C1.Win.C1Input.C1Button();
		this.dockverify = new C1.Win.C1Command.C1DockingTab();
		this.tabImage = new C1.Win.C1Command.C1DockingTabPage();
		this.c1Label2 = new C1.Win.C1Input.C1Label();
		this.VerifyImg = new C1.Win.C1Input.C1PictureBox();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.txtValidateCode = new Auditai.UI.Controls.C1TextBoxEx();
		this.tabSMS = new C1.Win.C1Command.C1DockingTabPage();
		this.c1Label3 = new C1.Win.C1Input.C1Label();
		this.btnGetValidateCode = new Auditai.UI.Controls.TimerButton();
		this.c1Label4 = new C1.Win.C1Input.C1Label();
		this.lblwarnName = new C1.Win.C1Input.C1Label();
		this.c1Label5 = new C1.Win.C1Input.C1Label();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtCompany).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtCity).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnRegister).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.warnUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.warnPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.warnPassword2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.warnName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.warnPhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCompany).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblSex).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCity).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblRegister).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dockverify).BeginInit();
		this.dockverify.SuspendLayout();
		this.tabImage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.VerifyImg).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtValidateCode).BeginInit();
		this.tabSMS.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetValidateCode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblwarnName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label5).BeginInit();
		base.SuspendLayout();
		this.txtUserName.AutoSize = false;
		this.txtUserName.BorderColor = System.Drawing.Color.LightGray;
		this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtUserName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtUserName.Location = new System.Drawing.Point(108, 229);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.Size = new System.Drawing.Size(230, 32);
		this.txtUserName.TabIndex = 0;
		this.txtUserName.Tag = null;
		this.txtUserName.TextDetached = true;
		this.txtUserName.Value = "";
		this.txtUserName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtUserName.Enter += new System.EventHandler(txtUserName_Enter);
		this.txtUserName.Validated += new System.EventHandler(txtUserName_Validated);
		this.txtPassword.AutoSize = false;
		this.txtPassword.BorderColor = System.Drawing.Color.LightGray;
		this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPassword.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword.Location = new System.Drawing.Point(108, 288);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '·';
		this.txtPassword.Size = new System.Drawing.Size(230, 32);
		this.txtPassword.TabIndex = 1;
		this.txtPassword.Tag = null;
		this.txtPassword.TextDetached = true;
		this.txtPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPassword.Enter += new System.EventHandler(txtPassword_Enter);
		this.txtPassword.Validated += new System.EventHandler(txtPassword_Validated);
		this.txtPassword2.AutoSize = false;
		this.txtPassword2.BorderColor = System.Drawing.Color.LightGray;
		this.txtPassword2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPassword2.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword2.Location = new System.Drawing.Point(108, 348);
		this.txtPassword2.Name = "txtPassword2";
		this.txtPassword2.PasswordChar = '·';
		this.txtPassword2.Size = new System.Drawing.Size(230, 32);
		this.txtPassword2.TabIndex = 2;
		this.txtPassword2.Tag = null;
		this.txtPassword2.TextDetached = true;
		this.txtPassword2.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPassword2.Enter += new System.EventHandler(txtPassword2_Enter);
		this.txtPassword2.Validated += new System.EventHandler(txtPassword2_Validated);
		this.txtName.AutoSize = false;
		this.txtName.BorderColor = System.Drawing.Color.LightGray;
		this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtName.Location = new System.Drawing.Point(429, 229);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(230, 32);
		this.txtName.TabIndex = 6;
		this.txtName.Tag = null;
		this.txtName.TextDetached = true;
		this.txtName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtName.Enter += new System.EventHandler(txtName_Enter);
		this.txtName.Validated += new System.EventHandler(txtName_Validated);
		this.txtEmail.AutoSize = false;
		this.txtEmail.BorderColor = System.Drawing.Color.LightGray;
		this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtEmail.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtEmail.Location = new System.Drawing.Point(429, 348);
		this.txtEmail.Name = "txtEmail";
		this.txtEmail.Size = new System.Drawing.Size(230, 32);
		this.txtEmail.TabIndex = 8;
		this.txtEmail.Tag = null;
		this.txtEmail.TextDetached = true;
		this.txtEmail.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtCompany.AutoSize = false;
		this.txtCompany.BorderColor = System.Drawing.Color.LightGray;
		this.txtCompany.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCompany.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtCompany.Location = new System.Drawing.Point(105, 118);
		this.txtCompany.Name = "txtCompany";
		this.txtCompany.Size = new System.Drawing.Size(230, 32);
		this.txtCompany.TabIndex = 3;
		this.txtCompany.Tag = null;
		this.txtCompany.TextDetached = true;
		this.txtCompany.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtCompany.Visible = false;
		this.txtPhone.AutoSize = false;
		this.txtPhone.BorderColor = System.Drawing.Color.LightGray;
		this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPhone.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhone.Location = new System.Drawing.Point(429, 288);
		this.txtPhone.Name = "txtPhone";
		this.txtPhone.Size = new System.Drawing.Size(230, 32);
		this.txtPhone.TabIndex = 7;
		this.txtPhone.Tag = null;
		this.txtPhone.TextDetached = true;
		this.txtPhone.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPhone.TextChanged += new System.EventHandler(txtPhone_TextChanged);
		this.txtPhone.Enter += new System.EventHandler(txtPhone_Enter);
		this.txtPhone.Validated += new System.EventHandler(txtPhone_Validated);
		this.txtCity.AutoSize = false;
		this.txtCity.BorderColor = System.Drawing.Color.LightGray;
		this.txtCity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCity.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtCity.Location = new System.Drawing.Point(429, 408);
		this.txtCity.Name = "txtCity";
		this.txtCity.Size = new System.Drawing.Size(230, 32);
		this.txtCity.TabIndex = 9;
		this.txtCity.Tag = null;
		this.txtCity.TextDetached = true;
		this.txtCity.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtVerification.AutoSize = false;
		this.txtVerification.BorderColor = System.Drawing.Color.LightGray;
		this.txtVerification.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtVerification.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtVerification.Location = new System.Drawing.Point(85, 6);
		this.txtVerification.Name = "txtVerification";
		this.txtVerification.Size = new System.Drawing.Size(139, 32);
		this.txtVerification.TabIndex = 10;
		this.txtVerification.Tag = null;
		this.txtVerification.TextDetached = true;
		this.txtVerification.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.btnRegister.FlatAppearance.BorderSize = 0;
		this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnRegister.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnRegister.ForeColor = System.Drawing.Color.White;
		this.btnRegister.Location = new System.Drawing.Point(245, 516);
		this.btnRegister.Name = "btnRegister";
		this.btnRegister.Size = new System.Drawing.Size(230, 43);
		this.btnRegister.TabIndex = 12;
		this.btnRegister.Text = "注册";
		this.btnRegister.UseVisualStyleBackColor = false;
		this.btnRegister.Click += new System.EventHandler(btnRegister_Click);
		this.radMale.BackColor = System.Drawing.Color.Transparent;
		this.radMale.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.radMale.Location = new System.Drawing.Point(108, 413);
		this.radMale.Name = "radMale";
		this.radMale.RadioCheckedCircleOffset = 2;
		this.radMale.RadioCircleBackgroundColor = System.Drawing.Color.White;
		this.radMale.RadioCircleCheckedColor = System.Drawing.Color.Black;
		this.radMale.RadioCircleMouseOverColor = System.Drawing.Color.Black;
		this.radMale.RadioCircleNormalColor = System.Drawing.Color.Black;
		this.radMale.RadioCircleSize = 12;
		this.radMale.Size = new System.Drawing.Size(38, 20);
		this.radMale.SpaceBetweenRadiocCirleAndText = 5;
		this.radMale.TabIndex = 4;
		this.radMale.Text = "男";
		this.radMale.UseVisualStyleBackColor = false;
		this.radFemale.BackColor = System.Drawing.Color.Transparent;
		this.radFemale.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.radFemale.Location = new System.Drawing.Point(179, 413);
		this.radFemale.Name = "radFemale";
		this.radFemale.RadioCheckedCircleOffset = 2;
		this.radFemale.RadioCircleBackgroundColor = System.Drawing.Color.White;
		this.radFemale.RadioCircleCheckedColor = System.Drawing.Color.Black;
		this.radFemale.RadioCircleMouseOverColor = System.Drawing.Color.Black;
		this.radFemale.RadioCircleNormalColor = System.Drawing.Color.Black;
		this.radFemale.RadioCircleSize = 12;
		this.radFemale.Size = new System.Drawing.Size(38, 20);
		this.radFemale.SpaceBetweenRadiocCirleAndText = 3;
		this.radFemale.TabIndex = 5;
		this.radFemale.Text = "女";
		this.radFemale.UseVisualStyleBackColor = false;
		this.warnUserName.AutoSize = true;
		this.warnUserName.BackColor = System.Drawing.Color.Transparent;
		this.warnUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnUserName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnUserName.ForeColor = System.Drawing.Color.Gray;
		this.warnUserName.Location = new System.Drawing.Point(112, 266);
		this.warnUserName.Name = "warnUserName";
		this.warnUserName.Size = new System.Drawing.Size(178, 17);
		this.warnUserName.TabIndex = 13;
		this.warnUserName.Tag = null;
		this.warnUserName.Text = "长度在2-20个字符不区分大小写";
		this.warnUserName.TextDetached = true;
		this.warnUserName.Visible = false;
		this.warnPassword.AutoSize = true;
		this.warnPassword.BackColor = System.Drawing.Color.Transparent;
		this.warnPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnPassword.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnPassword.ForeColor = System.Drawing.Color.Gray;
		this.warnPassword.Location = new System.Drawing.Point(111, 325);
		this.warnPassword.Name = "warnPassword";
		this.warnPassword.Size = new System.Drawing.Size(142, 17);
		this.warnPassword.TabIndex = 14;
		this.warnPassword.Tag = null;
		this.warnPassword.Text = "长度在6-20个字母或数字";
		this.warnPassword.TextDetached = true;
		this.warnPassword.Visible = false;
		this.warnPassword2.AutoSize = true;
		this.warnPassword2.BackColor = System.Drawing.Color.Transparent;
		this.warnPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnPassword2.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnPassword2.ForeColor = System.Drawing.Color.Gray;
		this.warnPassword2.Location = new System.Drawing.Point(113, 385);
		this.warnPassword2.Name = "warnPassword2";
		this.warnPassword2.Size = new System.Drawing.Size(104, 17);
		this.warnPassword2.TabIndex = 15;
		this.warnPassword2.Tag = null;
		this.warnPassword2.Text = "与上面输入要一致";
		this.warnPassword2.TextDetached = true;
		this.warnPassword2.Visible = false;
		this.warnName.AutoSize = true;
		this.warnName.BackColor = System.Drawing.Color.Transparent;
		this.warnName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnName.ForeColor = System.Drawing.Color.Gray;
		this.warnName.Location = new System.Drawing.Point(434, 266);
		this.warnName.Name = "warnName";
		this.warnName.Size = new System.Drawing.Size(0, 17);
		this.warnName.TabIndex = 16;
		this.warnName.Tag = null;
		this.warnName.TextDetached = true;
		this.warnName.Visible = false;
		this.warnPhone.AutoSize = true;
		this.warnPhone.BackColor = System.Drawing.Color.Transparent;
		this.warnPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnPhone.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnPhone.ForeColor = System.Drawing.Color.Gray;
		this.warnPhone.Location = new System.Drawing.Point(434, 325);
		this.warnPhone.Name = "warnPhone";
		this.warnPhone.Size = new System.Drawing.Size(260, 17);
		this.warnPhone.TabIndex = 17;
		this.warnPhone.Tag = null;
		this.warnPhone.Text = "找回密码唯一途径（我们比您更注重保护隐私）";
		this.warnPhone.TextDetached = true;
		this.warnPhone.Visible = false;
		this.lblUserName.AutoSize = true;
		this.lblUserName.BackColor = System.Drawing.Color.Transparent;
		this.lblUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblUserName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblUserName.Location = new System.Drawing.Point(58, 236);
		this.lblUserName.Name = "lblUserName";
		this.lblUserName.Size = new System.Drawing.Size(44, 17);
		this.lblUserName.TabIndex = 18;
		this.lblUserName.Tag = null;
		this.lblUserName.Text = "用户名";
		this.lblUserName.TextDetached = true;
		this.lblPassword2.AutoSize = true;
		this.lblPassword2.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword2.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword2.Location = new System.Drawing.Point(46, 355);
		this.lblPassword2.Name = "lblPassword2";
		this.lblPassword2.Size = new System.Drawing.Size(56, 17);
		this.lblPassword2.TabIndex = 19;
		this.lblPassword2.Tag = null;
		this.lblPassword2.Text = "确认密码";
		this.lblPassword2.TextDetached = true;
		this.lblName.AutoSize = true;
		this.lblName.BackColor = System.Drawing.Color.Transparent;
		this.lblName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblName.Location = new System.Drawing.Point(391, 235);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(32, 17);
		this.lblName.TabIndex = 20;
		this.lblName.Tag = null;
		this.lblName.Text = "姓名";
		this.lblName.TextDetached = true;
		this.lblEmail.AutoSize = true;
		this.lblEmail.BackColor = System.Drawing.Color.Transparent;
		this.lblEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEmail.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEmail.Location = new System.Drawing.Point(391, 354);
		this.lblEmail.Name = "lblEmail";
		this.lblEmail.Size = new System.Drawing.Size(32, 17);
		this.lblEmail.TabIndex = 21;
		this.lblEmail.Tag = null;
		this.lblEmail.Text = "邮箱";
		this.lblEmail.TextDetached = true;
		this.lblCompany.AutoSize = true;
		this.lblCompany.BackColor = System.Drawing.Color.Transparent;
		this.lblCompany.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCompany.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblCompany.Location = new System.Drawing.Point(19, 125);
		this.lblCompany.Name = "lblCompany";
		this.lblCompany.Size = new System.Drawing.Size(80, 17);
		this.lblCompany.TabIndex = 22;
		this.lblCompany.Tag = null;
		this.lblCompany.Text = "所在单位全称";
		this.lblCompany.TextDetached = true;
		this.lblCompany.Visible = false;
		this.lblSex.AutoSize = true;
		this.lblSex.BackColor = System.Drawing.Color.Transparent;
		this.lblSex.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblSex.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSex.Location = new System.Drawing.Point(70, 415);
		this.lblSex.Name = "lblSex";
		this.lblSex.Size = new System.Drawing.Size(32, 17);
		this.lblSex.TabIndex = 23;
		this.lblSex.Tag = null;
		this.lblSex.Text = "性别";
		this.lblSex.TextDetached = true;
		this.lblCity.AutoSize = true;
		this.lblCity.BackColor = System.Drawing.Color.Transparent;
		this.lblCity.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCity.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblCity.Location = new System.Drawing.Point(367, 415);
		this.lblCity.Name = "lblCity";
		this.lblCity.Size = new System.Drawing.Size(56, 17);
		this.lblCity.TabIndex = 24;
		this.lblCity.Tag = null;
		this.lblCity.Text = "所在城市";
		this.lblCity.TextDetached = true;
		this.lblVerification.AutoSize = true;
		this.lblVerification.BackColor = System.Drawing.Color.Transparent;
		this.lblVerification.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVerification.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblVerification.Location = new System.Drawing.Point(11, 14);
		this.lblVerification.Name = "lblVerification";
		this.lblVerification.Size = new System.Drawing.Size(68, 17);
		this.lblVerification.TabIndex = 25;
		this.lblVerification.Tag = null;
		this.lblVerification.Text = "短信验证码";
		this.lblVerification.TextDetached = true;
		this.lblPassword.AutoSize = true;
		this.lblPassword.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword.Location = new System.Drawing.Point(46, 293);
		this.lblPassword.Name = "lblPassword";
		this.lblPassword.Size = new System.Drawing.Size(56, 17);
		this.lblPassword.TabIndex = 27;
		this.lblPassword.Tag = null;
		this.lblPassword.Text = "登录密码";
		this.lblPassword.TextDetached = true;
		this.lblPhone.AutoSize = true;
		this.lblPhone.BackColor = System.Drawing.Color.Transparent;
		this.lblPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPhone.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPhone.Location = new System.Drawing.Point(379, 295);
		this.lblPhone.Name = "lblPhone";
		this.lblPhone.Size = new System.Drawing.Size(44, 17);
		this.lblPhone.TabIndex = 28;
		this.lblPhone.Tag = null;
		this.lblPhone.Text = "手机号";
		this.lblPhone.TextDetached = true;
		this.lblRegister.AutoSize = true;
		this.lblRegister.BackColor = System.Drawing.Color.Transparent;
		this.lblRegister.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblRegister.Font = new System.Drawing.Font("微软雅黑", 15.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblRegister.Location = new System.Drawing.Point(306, 172);
		this.lblRegister.Name = "lblRegister";
		this.lblRegister.Size = new System.Drawing.Size(96, 28);
		this.lblRegister.TabIndex = 30;
		this.lblRegister.Tag = null;
		this.lblRegister.Text = "注册信息";
		this.lblRegister.TextDetached = true;
		this.lblMustInputStar1.AutoSize = true;
		this.lblMustInputStar1.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar1.ForeColor = System.Drawing.Color.Red;
		this.lblMustInputStar1.Location = new System.Drawing.Point(46, 238);
		this.lblMustInputStar1.Name = "lblMustInputStar1";
		this.lblMustInputStar1.Size = new System.Drawing.Size(13, 17);
		this.lblMustInputStar1.TabIndex = 34;
		this.lblMustInputStar1.Tag = null;
		this.lblMustInputStar1.Text = "*";
		this.lblMustInputStar1.TextDetached = true;
		this.lblMustInputStar2.AutoSize = true;
		this.lblMustInputStar2.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar2.ForeColor = System.Drawing.Color.Red;
		this.lblMustInputStar2.Location = new System.Drawing.Point(34, 296);
		this.lblMustInputStar2.Name = "lblMustInputStar2";
		this.lblMustInputStar2.Size = new System.Drawing.Size(13, 17);
		this.lblMustInputStar2.TabIndex = 35;
		this.lblMustInputStar2.Tag = null;
		this.lblMustInputStar2.Text = "*";
		this.lblMustInputStar2.TextDetached = true;
		this.lblMustInputStar3.AutoSize = true;
		this.lblMustInputStar3.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar3.ForeColor = System.Drawing.Color.Red;
		this.lblMustInputStar3.Location = new System.Drawing.Point(36, 358);
		this.lblMustInputStar3.Name = "lblMustInputStar3";
		this.lblMustInputStar3.Size = new System.Drawing.Size(13, 17);
		this.lblMustInputStar3.TabIndex = 36;
		this.lblMustInputStar3.Tag = null;
		this.lblMustInputStar3.Text = "*";
		this.lblMustInputStar3.TextDetached = true;
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.BackColor = System.Drawing.Color.Transparent;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Image = Auditai.UI.Platform.Properties.Resources.close2;
		this.btnClose.Location = new System.Drawing.Point(708, 0);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(25, 25);
		this.btnClose.TabIndex = 40;
		this.btnClose.UseVisualStyleBackColor = false;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.dockverify.BackColor = System.Drawing.Color.White;
		this.dockverify.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dockverify.Controls.Add(this.tabImage);
		this.dockverify.Controls.Add(this.tabSMS);
		this.dockverify.Location = new System.Drawing.Point(22, 444);
		this.dockverify.Name = "dockverify";
		this.dockverify.ShowTabs = false;
		this.dockverify.Size = new System.Drawing.Size(340, 63);
		this.dockverify.TabIndex = 46;
		this.dockverify.TabsSpacing = 0;
		this.tabImage.Controls.Add(this.c1Label2);
		this.tabImage.Controls.Add(this.VerifyImg);
		this.tabImage.Controls.Add(this.c1Label1);
		this.tabImage.Controls.Add(this.txtValidateCode);
		this.tabImage.Location = new System.Drawing.Point(0, 1);
		this.tabImage.Name = "tabImage";
		this.tabImage.Size = new System.Drawing.Size(340, 62);
		this.tabImage.TabIndex = 0;
		this.tabImage.Text = "第1页";
		this.c1Label2.AutoSize = true;
		this.c1Label2.BackColor = System.Drawing.Color.Transparent;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.ForeColor = System.Drawing.Color.Red;
		this.c1Label2.Location = new System.Drawing.Point(23, 15);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(13, 17);
		this.c1Label2.TabIndex = 47;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "*";
		this.c1Label2.TextDetached = true;
		this.VerifyImg.Location = new System.Drawing.Point(237, 6);
		this.VerifyImg.Name = "VerifyImg";
		this.VerifyImg.Size = new System.Drawing.Size(76, 32);
		this.VerifyImg.TabIndex = 29;
		this.VerifyImg.TabStop = false;
		this.VerifyImg.Click += new System.EventHandler(VerifyImg_Click);
		this.c1Label1.AutoSize = true;
		this.c1Label1.BackColor = System.Drawing.Color.Transparent;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.Location = new System.Drawing.Point(37, 14);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(44, 17);
		this.c1Label1.TabIndex = 28;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "验证码";
		this.c1Label1.TextDetached = true;
		this.txtValidateCode.AutoSize = false;
		this.txtValidateCode.BorderColor = System.Drawing.Color.LightGray;
		this.txtValidateCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtValidateCode.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtValidateCode.Location = new System.Drawing.Point(92, 6);
		this.txtValidateCode.Name = "txtValidateCode";
		this.txtValidateCode.Size = new System.Drawing.Size(139, 32);
		this.txtValidateCode.TabIndex = 26;
		this.txtValidateCode.Tag = null;
		this.txtValidateCode.TextDetached = true;
		this.txtValidateCode.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.tabSMS.BackColor = System.Drawing.Color.White;
		this.tabSMS.Controls.Add(this.c1Label3);
		this.tabSMS.Controls.Add(this.lblVerification);
		this.tabSMS.Controls.Add(this.txtVerification);
		this.tabSMS.Controls.Add(this.btnGetValidateCode);
		this.tabSMS.Location = new System.Drawing.Point(0, 1);
		this.tabSMS.Name = "tabSMS";
		this.tabSMS.Size = new System.Drawing.Size(340, 62);
		this.tabSMS.TabIndex = 1;
		this.tabSMS.Text = "第2页";
		this.c1Label3.AutoSize = true;
		this.c1Label3.BackColor = System.Drawing.Color.Transparent;
		this.c1Label3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label3.ForeColor = System.Drawing.Color.Red;
		this.c1Label3.Location = new System.Drawing.Point(-1, 16);
		this.c1Label3.Name = "c1Label3";
		this.c1Label3.Size = new System.Drawing.Size(13, 17);
		this.c1Label3.TabIndex = 48;
		this.c1Label3.Tag = null;
		this.c1Label3.Text = "*";
		this.c1Label3.TextDetached = true;
		this.btnGetValidateCode.BackColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.btnGetValidateCode.FlatAppearance.BorderSize = 0;
		this.btnGetValidateCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnGetValidateCode.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnGetValidateCode.ForeColor = System.Drawing.Color.White;
		this.btnGetValidateCode.Format = null;
		this.btnGetValidateCode.Location = new System.Drawing.Point(239, 6);
		this.btnGetValidateCode.Name = "btnGetValidateCode";
		this.btnGetValidateCode.Size = new System.Drawing.Size(76, 32);
		this.btnGetValidateCode.TabIndex = 11;
		this.btnGetValidateCode.Text = "获取验证码";
		this.btnGetValidateCode.UseVisualStyleBackColor = false;
		this.btnGetValidateCode.Click += new System.EventHandler(btnGetValidateCode_Click);
		this.c1Label4.AutoSize = true;
		this.c1Label4.BackColor = System.Drawing.Color.Transparent;
		this.c1Label4.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label4.ForeColor = System.Drawing.Color.Red;
		this.c1Label4.Location = new System.Drawing.Point(379, 238);
		this.c1Label4.Name = "c1Label4";
		this.c1Label4.Size = new System.Drawing.Size(13, 17);
		this.c1Label4.TabIndex = 47;
		this.c1Label4.Tag = null;
		this.c1Label4.Text = "*";
		this.c1Label4.TextDetached = true;
		this.lblwarnName.AutoSize = true;
		this.lblwarnName.BackColor = System.Drawing.Color.Transparent;
		this.lblwarnName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblwarnName.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblwarnName.ForeColor = System.Drawing.Color.Gray;
		this.lblwarnName.Location = new System.Drawing.Point(429, 266);
		this.lblwarnName.Name = "lblwarnName";
		this.lblwarnName.Size = new System.Drawing.Size(106, 17);
		this.lblwarnName.TabIndex = 48;
		this.lblwarnName.Tag = null;
		this.lblwarnName.Text = "长度在2-20个字符";
		this.lblwarnName.TextDetached = true;
		this.lblwarnName.Visible = false;
		this.c1Label5.AutoSize = true;
		this.c1Label5.BackColor = System.Drawing.Color.Transparent;
		this.c1Label5.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label5.ForeColor = System.Drawing.Color.Red;
		this.c1Label5.Location = new System.Drawing.Point(368, 297);
		this.c1Label5.Name = "c1Label5";
		this.c1Label5.Size = new System.Drawing.Size(13, 17);
		this.c1Label5.TabIndex = 49;
		this.c1Label5.Tag = null;
		this.c1Label5.Text = "*";
		this.c1Label5.TextDetached = true;
		base.AcceptButton = this.btnRegister;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.White;
		this.BackgroundImage = Auditai.UI.Platform.Properties.Resources.register;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.ClientSize = new System.Drawing.Size(733, 588);
		base.Controls.Add(this.c1Label5);
		base.Controls.Add(this.lblwarnName);
		base.Controls.Add(this.c1Label4);
		base.Controls.Add(this.dockverify);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.lblMustInputStar3);
		base.Controls.Add(this.lblMustInputStar2);
		base.Controls.Add(this.lblMustInputStar1);
		base.Controls.Add(this.lblRegister);
		base.Controls.Add(this.lblPhone);
		base.Controls.Add(this.lblPassword);
		base.Controls.Add(this.lblCity);
		base.Controls.Add(this.lblSex);
		base.Controls.Add(this.lblCompany);
		base.Controls.Add(this.lblEmail);
		base.Controls.Add(this.lblName);
		base.Controls.Add(this.lblPassword2);
		base.Controls.Add(this.lblUserName);
		base.Controls.Add(this.warnPhone);
		base.Controls.Add(this.warnName);
		base.Controls.Add(this.warnPassword2);
		base.Controls.Add(this.warnPassword);
		base.Controls.Add(this.warnUserName);
		base.Controls.Add(this.radFemale);
		base.Controls.Add(this.radMale);
		base.Controls.Add(this.btnRegister);
		base.Controls.Add(this.txtCity);
		base.Controls.Add(this.txtPhone);
		base.Controls.Add(this.txtCompany);
		base.Controls.Add(this.txtEmail);
		base.Controls.Add(this.txtName);
		base.Controls.Add(this.txtPassword2);
		base.Controls.Add(this.txtPassword);
		base.Controls.Add(this.txtUserName);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "frmRegister";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = " 注册";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmRegister_FormClosing);
		base.Load += new System.EventHandler(frmRegister_Load);
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmRegister_Paint);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(frmRegister_MouseDown);
		((System.ComponentModel.ISupportInitialize)this.txtUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtCompany).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtCity).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtVerification).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnRegister).EndInit();
		((System.ComponentModel.ISupportInitialize)this.warnUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.warnPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.warnPassword2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.warnName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.warnPhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCompany).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblSex).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCity).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblVerification).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblRegister).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dockverify).EndInit();
		this.dockverify.ResumeLayout(false);
		this.tabImage.ResumeLayout(false);
		this.tabImage.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.VerifyImg).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtValidateCode).EndInit();
		this.tabSMS.ResumeLayout(false);
		this.tabSMS.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetValidateCode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblwarnName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label5).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
