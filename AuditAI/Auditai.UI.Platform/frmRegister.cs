﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
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

	private static Color _auditaiMainColor = Color.FromArgb(50, 150, 220);

	private float _scaleFactor = 1.5f;

	private static Color _auditaiMainColorButton = Color.FromArgb(50, 150, 220);

	private ValidateCodeCreator validateCreator;

	private bool _whetherTxtPass = true;

	private bool _thirdLogin;

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

	private C1TextBoxEx txtVerification;

	private C1Button btnRegister;

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

	private Panel pnlCard;

	private Panel pnlLeftColumn;

	private Panel pnlRightColumn;

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
		ApplyRoundedRegion(12);
		ApplyRoundedButton(btnRegister, 9);
		ApplyRoundedButton(btnGetValidateCode, 9);
		ApplyRoundedButton(btnClose, 9);
	}

	private void FrmRegister_Shown(object sender, EventArgs e)
	{
	}

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

	private void Initialize(bool thirdLogin)
	{
		InitPlatformStyle();
		BackgroundImage = null;
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
		_auditaiMainColor = Color.FromArgb(50, 150, 220);
		_auditaiMainColorButton = Color.FromArgb(50, 150, 220);
		btnRegister.BackColor = _auditaiMainColorButton;
		btnRegister.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 120, 190);
		btnRegister.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 170, 240);
		btnGetValidateCode.BackColor = _auditaiMainColorButton;
		btnGetValidateCode.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 120, 190);
		btnGetValidateCode.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 170, 240);
	}

	private void InitPlatform_Audit()
	{
	}

	private void InitPlatform_Report()
	{
	}

	private void InitPlatform_Manager()
	{
	}

	private void InitPlatform_TableDevelop()
	{
	}

	private void InitPlatform_ProductionCostAccountingSystem()
	{
	}

	private void InitPlatform_ContractLedgerManagementSystem()
	{
	}

	private void InitPlatform_RDExpenseLedgerSystem()
	{
	}

	private void InitPlatform_SalesOrderManagementSystem()
	{
	}

	private void InitPlatform_PSIManagementSystem()
	{
	}

	private void InitPlatform_ProjectLedgerManagementSystem()
	{
	}

	private void InitPlatform_Custom()
	{
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
		foreach (object control in pnlCard.Controls)
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
					tb.BorderColor = Color.FromArgb(210, 210, 210);
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
				Company = txtCompany.Text.Trim(),
				Sex = "m",
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
				// 修复 BUG: 此前使用 isUrl: false，返回未 URL 编码的 Base64(SHA256(明文))，
				// 而 frmLogin.UpdateInputPassword 使用 isUrl: true（已 URL 编码）。
				// 这导致注册后自动登录时，AccountLogin 收到的 hashPassword 格式不一致：
				// - 服务器 ASP.NET Core 自动 URL 解码，将 Base64 中的 "+" 解码为空格，破坏哈希值
				// - 若密码哈希恰好包含 "+" 字符，登录会失败
				// 统一使用 isUrl: true 与 frmLogin.UpdateInputPassword 保持一致
				Password = Encrypts.SHA256Encrypt(txtPassword.Text.Trim(), isUrl: true);
			}
			else
			{
				if (string.IsNullOrEmpty(txtVerification.Text.Trim()))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "激活码不能为空！");
					return;
				}
				UserId = await WebApiClient.Register(user, txtVerification.Text.Trim());
				UserName = txtUserName.Text.Trim();
				TelPhone = txtPhone.Text.Trim();
				Truename = txtName.Text.Trim();
				// 修复 BUG: 同上，统一使用 isUrl: true 与 frmLogin.UpdateInputPassword 保持一致
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException?.Message ?? ex3.Message);
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException?.Message ?? ex3.Message);
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
		var g = e.Graphics;
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		Rectangle bgRect = new Rectangle(0, 0, base.Width, base.Height);
		using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			bgRect, Color.FromArgb(240, 248, 255), Color.FromArgb(225, 240, 255),
			System.Drawing.Drawing2D.LinearGradientMode.Vertical))
		{
			g.FillRectangle(bgBrush, bgRect);
		}

		using (var bgBrush2 = new System.Drawing.Drawing2D.LinearGradientBrush(
			new Point(base.Width, 0),
			new Point(0, base.Height),
			Color.FromArgb(150, 220, 235, 255),
			Color.FromArgb(150, 240, 248, 255)))
		{
			g.FillRectangle(bgBrush2, bgRect);
		}

		Rectangle topBar = new Rectangle(0, 0, base.Width, (int)(3 * _scaleFactor));
		using (var topBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			topBar, Color.FromArgb(80, 170, 240), Color.FromArgb(50, 150, 220),
			System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
		{
			g.FillRectangle(topBrush, topBar);
		}

		int cardX = (int)(30 * _scaleFactor), cardY = (int)(30 * _scaleFactor);
		int cardW = (int)(740 * _scaleFactor), cardH = (int)(480 * _scaleFactor);
		int radius = (int)(12 * _scaleFactor);
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			path.AddArc(cardX, cardY, radius * 2, radius * 2, 180, 90);
			path.AddArc(cardX + cardW - radius * 2, cardY, radius * 2, radius * 2, 270, 90);
			path.AddArc(cardX + cardW - radius * 2, cardY + cardH - radius * 2, radius * 2, radius * 2, 0, 90);
			path.AddArc(cardX, cardY + cardH - radius * 2, radius * 2, radius * 2, 90, 90);
			path.CloseFigure();

			using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 100, 150, 200)))
			{
				g.TranslateTransform(2, 3);
				g.FillPath(shadowBrush, path);
				g.TranslateTransform(-2, -3);
			}
			using (var cardBrush = new SolidBrush(Color.White))
			{
				g.FillPath(cardBrush, path);
			}
			using (var borderPen = new Pen(Color.FromArgb(180, 215, 245), 1f))
			{
				g.DrawPath(borderPen, path);
			}
		}
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
		this.txtVerification = new Auditai.UI.Controls.C1TextBoxEx();
		this.btnRegister = new C1.Win.C1Input.C1Button();
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
		this.pnlCard = new System.Windows.Forms.Panel();
		this.pnlLeftColumn = new System.Windows.Forms.Panel();
		this.pnlRightColumn = new System.Windows.Forms.Panel();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPassword2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtCompany).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).BeginInit();
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
		this.pnlCard.SuspendLayout();
		this.pnlLeftColumn.SuspendLayout();
		this.pnlRightColumn.SuspendLayout();
		base.SuspendLayout();

		// pnlCard
		this.pnlCard.BackColor = System.Drawing.Color.White;
		this.pnlCard.Controls.Add(this.lblRegister);
		this.pnlCard.Controls.Add(this.pnlLeftColumn);
		this.pnlCard.Controls.Add(this.pnlRightColumn);
		this.pnlCard.Controls.Add(this.dockverify);
		this.pnlCard.Controls.Add(this.btnRegister);
		this.pnlCard.Location = new System.Drawing.Point(58, 58);
		this.pnlCard.Name = "pnlCard";
		this.pnlCard.Size = new System.Drawing.Size(962, 624);
		this.pnlCard.TabIndex = 0;

		// lblRegister
		this.lblRegister.AutoSize = false;
		this.lblRegister.BackColor = System.Drawing.Color.Transparent;
		this.lblRegister.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblRegister.Font = new System.Drawing.Font("Noto Sans SC", 16f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblRegister.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.lblRegister.Location = new System.Drawing.Point(351, 32);
		this.lblRegister.Name = "lblRegister";
		this.lblRegister.Size = new System.Drawing.Size(260, 52);
		this.lblRegister.TabIndex = 30;
		this.lblRegister.Tag = null;
		this.lblRegister.Text = "注册账号";
		this.lblRegister.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblRegister.TextDetached = true;

		// pnlLeftColumn - 左列：账号信息
		this.pnlLeftColumn.Controls.Add(this.lblUserName);
		this.pnlLeftColumn.Controls.Add(this.txtUserName);
		this.pnlLeftColumn.Controls.Add(this.warnUserName);
		this.pnlLeftColumn.Controls.Add(this.lblMustInputStar1);
		this.pnlLeftColumn.Controls.Add(this.lblPassword);
		this.pnlLeftColumn.Controls.Add(this.txtPassword);
		this.pnlLeftColumn.Controls.Add(this.warnPassword);
		this.pnlLeftColumn.Controls.Add(this.lblMustInputStar2);
		this.pnlLeftColumn.Controls.Add(this.lblPassword2);
		this.pnlLeftColumn.Controls.Add(this.txtPassword2);
		this.pnlLeftColumn.Controls.Add(this.warnPassword2);
		this.pnlLeftColumn.Controls.Add(this.lblMustInputStar3);
		this.pnlLeftColumn.Location = new System.Drawing.Point(46, 98);
		this.pnlLeftColumn.Name = "pnlLeftColumn";
		this.pnlLeftColumn.Size = new System.Drawing.Size(429, 416);
		this.pnlLeftColumn.TabIndex = 0;

		// lblUserName
		this.lblUserName.AutoSize = true;
		this.lblUserName.BackColor = System.Drawing.Color.Transparent;
		this.lblUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblUserName.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.lblUserName.Location = new System.Drawing.Point(0, 0);
		this.lblUserName.Name = "lblUserName";
		this.lblUserName.Size = new System.Drawing.Size(104, 34);
		this.lblUserName.TabIndex = 18;
		this.lblUserName.Tag = null;
		this.lblUserName.Text = "用户名";
		this.lblUserName.TextDetached = true;

		// txtUserName
		this.txtUserName.AutoSize = false;
		this.txtUserName.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtUserName.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtUserName.Location = new System.Drawing.Point(0, 39);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.Size = new System.Drawing.Size(429, 57);
		this.txtUserName.TabIndex = 0;
		this.txtUserName.Tag = null;
		this.txtUserName.TextDetached = true;
		this.txtUserName.Value = "";
		this.txtUserName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtUserName.Enter += new System.EventHandler(txtUserName_Enter);
		this.txtUserName.Validated += new System.EventHandler(txtUserName_Validated);

		// warnUserName
		this.warnUserName.AutoSize = true;
		this.warnUserName.BackColor = System.Drawing.Color.Transparent;
		this.warnUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnUserName.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.warnUserName.Location = new System.Drawing.Point(0, 99);
		this.warnUserName.Name = "warnUserName";
		this.warnUserName.Size = new System.Drawing.Size(390, 29);
		this.warnUserName.TabIndex = 13;
		this.warnUserName.Tag = null;
		this.warnUserName.Text = "长度在2-20个字符不区分大小写";
		this.warnUserName.TextDetached = true;
		this.warnUserName.Visible = false;

		// lblMustInputStar1
		this.lblMustInputStar1.AutoSize = true;
		this.lblMustInputStar1.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar1.ForeColor = System.Drawing.Color.FromArgb(220, 80, 80);
		this.lblMustInputStar1.Location = new System.Drawing.Point(104, 0);
		this.lblMustInputStar1.Name = "lblMustInputStar1";
		this.lblMustInputStar1.Size = new System.Drawing.Size(26, 34);
		this.lblMustInputStar1.TabIndex = 34;
		this.lblMustInputStar1.Tag = null;
		this.lblMustInputStar1.Text = "*";
		this.lblMustInputStar1.TextDetached = true;

		// lblPassword
		this.lblPassword.AutoSize = true;
		this.lblPassword.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.lblPassword.Location = new System.Drawing.Point(0, 136);
		this.lblPassword.Name = "lblPassword";
		this.lblPassword.Size = new System.Drawing.Size(104, 34);
		this.lblPassword.TabIndex = 27;
		this.lblPassword.Tag = null;
		this.lblPassword.Text = "登录密码";
		this.lblPassword.TextDetached = true;

		// txtPassword
		this.txtPassword.AutoSize = false;
		this.txtPassword.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtPassword.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword.Location = new System.Drawing.Point(0, 176);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '·';
		this.txtPassword.Size = new System.Drawing.Size(429, 57);
		this.txtPassword.TabIndex = 1;
		this.txtPassword.Tag = null;
		this.txtPassword.TextDetached = true;
		this.txtPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPassword.Enter += new System.EventHandler(txtPassword_Enter);
		this.txtPassword.Validated += new System.EventHandler(txtPassword_Validated);

		// warnPassword
		this.warnPassword.AutoSize = true;
		this.warnPassword.BackColor = System.Drawing.Color.Transparent;
		this.warnPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnPassword.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnPassword.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.warnPassword.Location = new System.Drawing.Point(0, 235);
		this.warnPassword.Name = "warnPassword";
		this.warnPassword.Size = new System.Drawing.Size(390, 29);
		this.warnPassword.TabIndex = 14;
		this.warnPassword.Tag = null;
		this.warnPassword.Text = "长度在6-20个字母或数字";
		this.warnPassword.TextDetached = true;
		this.warnPassword.Visible = false;

		// lblMustInputStar2
		this.lblMustInputStar2.AutoSize = true;
		this.lblMustInputStar2.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar2.ForeColor = System.Drawing.Color.FromArgb(220, 80, 80);
		this.lblMustInputStar2.Location = new System.Drawing.Point(104, 136);
		this.lblMustInputStar2.Name = "lblMustInputStar2";
		this.lblMustInputStar2.Size = new System.Drawing.Size(26, 34);
		this.lblMustInputStar2.TabIndex = 35;
		this.lblMustInputStar2.Tag = null;
		this.lblMustInputStar2.Text = "*";
		this.lblMustInputStar2.TextDetached = true;

		// lblPassword2
		this.lblPassword2.AutoSize = true;
		this.lblPassword2.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword2.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword2.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.lblPassword2.Location = new System.Drawing.Point(0, 273);
		this.lblPassword2.Name = "lblPassword2";
		this.lblPassword2.Size = new System.Drawing.Size(104, 34);
		this.lblPassword2.TabIndex = 19;
		this.lblPassword2.Tag = null;
		this.lblPassword2.Text = "确认密码";
		this.lblPassword2.TextDetached = true;

		// txtPassword2
		this.txtPassword2.AutoSize = false;
		this.txtPassword2.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtPassword2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPassword2.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtPassword2.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword2.Location = new System.Drawing.Point(0, 312);
		this.txtPassword2.Name = "txtPassword2";
		this.txtPassword2.PasswordChar = '·';
		this.txtPassword2.Size = new System.Drawing.Size(429, 57);
		this.txtPassword2.TabIndex = 2;
		this.txtPassword2.Tag = null;
		this.txtPassword2.TextDetached = true;
		this.txtPassword2.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPassword2.Enter += new System.EventHandler(txtPassword2_Enter);
		this.txtPassword2.Validated += new System.EventHandler(txtPassword2_Validated);

		// warnPassword2
		this.warnPassword2.AutoSize = true;
		this.warnPassword2.BackColor = System.Drawing.Color.Transparent;
		this.warnPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnPassword2.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnPassword2.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.warnPassword2.Location = new System.Drawing.Point(0, 372);
		this.warnPassword2.Name = "warnPassword2";
		this.warnPassword2.Size = new System.Drawing.Size(390, 29);
		this.warnPassword2.TabIndex = 15;
		this.warnPassword2.Tag = null;
		this.warnPassword2.Text = "与上面输入要一致";
		this.warnPassword2.TextDetached = true;
		this.warnPassword2.Visible = false;

		// lblMustInputStar3
		this.lblMustInputStar3.AutoSize = true;
		this.lblMustInputStar3.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar3.ForeColor = System.Drawing.Color.FromArgb(220, 80, 80);
		this.lblMustInputStar3.Location = new System.Drawing.Point(104, 273);
		this.lblMustInputStar3.Name = "lblMustInputStar3";
		this.lblMustInputStar3.Size = new System.Drawing.Size(26, 34);
		this.lblMustInputStar3.TabIndex = 36;
		this.lblMustInputStar3.Tag = null;
		this.lblMustInputStar3.Text = "*";
		this.lblMustInputStar3.TextDetached = true;

		// pnlRightColumn - 右列：个人信息
		this.pnlRightColumn.Controls.Add(this.lblName);
		this.pnlRightColumn.Controls.Add(this.txtName);
		this.pnlRightColumn.Controls.Add(this.lblwarnName);
		this.pnlRightColumn.Controls.Add(this.c1Label4);
		this.pnlRightColumn.Controls.Add(this.lblPhone);
		this.pnlRightColumn.Controls.Add(this.txtPhone);
		this.pnlRightColumn.Controls.Add(this.warnPhone);
		this.pnlRightColumn.Controls.Add(this.c1Label5);
		this.pnlRightColumn.Controls.Add(this.lblEmail);
		this.pnlRightColumn.Controls.Add(this.txtEmail);
		this.pnlRightColumn.Controls.Add(this.lblCompany);
		this.pnlRightColumn.Controls.Add(this.txtCompany);
		this.pnlRightColumn.Location = new System.Drawing.Point(494, 98);
		this.pnlRightColumn.Name = "pnlRightColumn";
		this.pnlRightColumn.Size = new System.Drawing.Size(429, 416);
		this.pnlRightColumn.TabIndex = 1;

		// lblName
		this.lblName.AutoSize = true;
		this.lblName.BackColor = System.Drawing.Color.Transparent;
		this.lblName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblName.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.lblName.Location = new System.Drawing.Point(0, 0);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(78, 34);
		this.lblName.TabIndex = 20;
		this.lblName.Tag = null;
		this.lblName.Text = "姓名";
		this.lblName.TextDetached = true;

		// txtName
		this.txtName.AutoSize = false;
		this.txtName.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtName.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtName.Location = new System.Drawing.Point(0, 39);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(429, 57);
		this.txtName.TabIndex = 6;
		this.txtName.Tag = null;
		this.txtName.TextDetached = true;
		this.txtName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtName.Enter += new System.EventHandler(txtName_Enter);
		this.txtName.Validated += new System.EventHandler(txtName_Validated);

		// lblwarnName
		this.lblwarnName.AutoSize = true;
		this.lblwarnName.BackColor = System.Drawing.Color.Transparent;
		this.lblwarnName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblwarnName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblwarnName.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.lblwarnName.Location = new System.Drawing.Point(0, 99);
		this.lblwarnName.Name = "lblwarnName";
		this.lblwarnName.Size = new System.Drawing.Size(390, 29);
		this.lblwarnName.TabIndex = 48;
		this.lblwarnName.Tag = null;
		this.lblwarnName.Text = "长度在2-20个字符";
		this.lblwarnName.TextDetached = true;
		this.lblwarnName.Visible = false;

		// c1Label4
		this.c1Label4.AutoSize = true;
		this.c1Label4.BackColor = System.Drawing.Color.Transparent;
		this.c1Label4.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label4.ForeColor = System.Drawing.Color.FromArgb(220, 80, 80);
		this.c1Label4.Location = new System.Drawing.Point(78, 0);
		this.c1Label4.Name = "c1Label4";
		this.c1Label4.Size = new System.Drawing.Size(26, 34);
		this.c1Label4.TabIndex = 47;
		this.c1Label4.Tag = null;
		this.c1Label4.Text = "*";
		this.c1Label4.TextDetached = true;

		// lblPhone
		this.lblPhone.AutoSize = true;
		this.lblPhone.BackColor = System.Drawing.Color.Transparent;
		this.lblPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPhone.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPhone.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.lblPhone.Location = new System.Drawing.Point(0, 136);
		this.lblPhone.Name = "lblPhone";
		this.lblPhone.Size = new System.Drawing.Size(78, 34);
		this.lblPhone.TabIndex = 28;
		this.lblPhone.Tag = null;
		this.lblPhone.Text = "手机号";
		this.lblPhone.TextDetached = true;

		// txtPhone
		this.txtPhone.AutoSize = false;
		this.txtPhone.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPhone.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtPhone.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhone.Location = new System.Drawing.Point(0, 176);
		this.txtPhone.Name = "txtPhone";
		this.txtPhone.Size = new System.Drawing.Size(429, 57);
		this.txtPhone.TabIndex = 7;
		this.txtPhone.Tag = null;
		this.txtPhone.TextDetached = true;
		this.txtPhone.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPhone.TextChanged += new System.EventHandler(txtPhone_TextChanged);
		this.txtPhone.Enter += new System.EventHandler(txtPhone_Enter);
		this.txtPhone.Validated += new System.EventHandler(txtPhone_Validated);

		// warnPhone
		this.warnPhone.AutoSize = true;
		this.warnPhone.BackColor = System.Drawing.Color.Transparent;
		this.warnPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnPhone.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnPhone.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.warnPhone.Location = new System.Drawing.Point(0, 235);
		this.warnPhone.Name = "warnPhone";
		this.warnPhone.Size = new System.Drawing.Size(429, 29);
		this.warnPhone.TabIndex = 17;
		this.warnPhone.Tag = null;
		this.warnPhone.Text = "找回密码唯一途径";
		this.warnPhone.TextDetached = true;
		this.warnPhone.Visible = false;

		// c1Label5
		this.c1Label5.AutoSize = true;
		this.c1Label5.BackColor = System.Drawing.Color.Transparent;
		this.c1Label5.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label5.ForeColor = System.Drawing.Color.FromArgb(220, 80, 80);
		this.c1Label5.Location = new System.Drawing.Point(78, 136);
		this.c1Label5.Name = "c1Label5";
		this.c1Label5.Size = new System.Drawing.Size(26, 34);
		this.c1Label5.TabIndex = 49;
		this.c1Label5.Tag = null;
		this.c1Label5.Text = "*";
		this.c1Label5.TextDetached = true;

		// lblEmail
		this.lblEmail.AutoSize = true;
		this.lblEmail.BackColor = System.Drawing.Color.Transparent;
		this.lblEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEmail.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEmail.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.lblEmail.Location = new System.Drawing.Point(0, 273);
		this.lblEmail.Name = "lblEmail";
		this.lblEmail.Size = new System.Drawing.Size(78, 34);
		this.lblEmail.TabIndex = 21;
		this.lblEmail.Tag = null;
		this.lblEmail.Text = "邮箱";
		this.lblEmail.TextDetached = true;

		// txtEmail
		this.txtEmail.AutoSize = false;
		this.txtEmail.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtEmail.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtEmail.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtEmail.Location = new System.Drawing.Point(0, 312);
		this.txtEmail.Name = "txtEmail";
		this.txtEmail.Size = new System.Drawing.Size(429, 57);
		this.txtEmail.TabIndex = 8;
		this.txtEmail.Tag = null;
		this.txtEmail.TextDetached = true;
		this.txtEmail.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;

		// lblCompany
		this.lblCompany.AutoSize = true;
		this.lblCompany.BackColor = System.Drawing.Color.Transparent;
		this.lblCompany.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCompany.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblCompany.Location = new System.Drawing.Point(0, 384);
		this.lblCompany.Name = "lblCompany";
		this.lblCompany.Size = new System.Drawing.Size(130, 34);
		this.lblCompany.TabIndex = 22;
		this.lblCompany.Tag = null;
		this.lblCompany.Text = "所在单位";
		this.lblCompany.TextDetached = true;
		this.lblCompany.Visible = false;

		// txtCompany
		this.txtCompany.AutoSize = false;
		this.txtCompany.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtCompany.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCompany.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtCompany.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtCompany.Location = new System.Drawing.Point(0, 384);
		this.txtCompany.Name = "txtCompany";
		this.txtCompany.Size = new System.Drawing.Size(429, 57);
		this.txtCompany.TabIndex = 3;
		this.txtCompany.Tag = null;
		this.txtCompany.TextDetached = true;
		this.txtCompany.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtCompany.Visible = false;

		// dockverify - 验证码区（跨列）
		this.dockverify.BackColor = System.Drawing.Color.White;
		this.dockverify.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dockverify.Controls.Add(this.tabImage);
		this.dockverify.Controls.Add(this.tabSMS);
		this.dockverify.Location = new System.Drawing.Point(46, 520);
		this.dockverify.Name = "dockverify";
		this.dockverify.ShowTabs = false;
		this.dockverify.Size = new System.Drawing.Size(878, 78);
		this.dockverify.TabIndex = 46;
		this.dockverify.TabsSpacing = 0;

		// tabImage
		this.tabImage.Controls.Add(this.c1Label2);
		this.tabImage.Controls.Add(this.VerifyImg);
		this.tabImage.Controls.Add(this.c1Label1);
		this.tabImage.Controls.Add(this.txtValidateCode);
		this.tabImage.BackColor = System.Drawing.Color.White;
		this.tabImage.Location = new System.Drawing.Point(0, 3);
		this.tabImage.Name = "tabImage";
		this.tabImage.Size = new System.Drawing.Size(878, 73);
		this.tabImage.TabIndex = 0;
		this.tabImage.Text = "第1页";

		this.c1Label2.AutoSize = true;
		this.c1Label2.BackColor = System.Drawing.Color.Transparent;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.ForeColor = System.Drawing.Color.FromArgb(220, 80, 80);
		this.c1Label2.Location = new System.Drawing.Point(10, 23);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(26, 34);
		this.c1Label2.TabIndex = 47;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "*";
		this.c1Label2.TextDetached = true;

		this.VerifyImg.Location = new System.Drawing.Point(702, 8);
		this.VerifyImg.Name = "VerifyImg";
		this.VerifyImg.Size = new System.Drawing.Size(156, 57);
		this.VerifyImg.TabIndex = 29;
		this.VerifyImg.TabStop = false;
		this.VerifyImg.Click += new System.EventHandler(VerifyImg_Click);

		this.c1Label1.AutoSize = true;
		this.c1Label1.BackColor = System.Drawing.Color.Transparent;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.c1Label1.Location = new System.Drawing.Point(39, 23);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(86, 34);
		this.c1Label1.TabIndex = 28;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "验证码";
		this.c1Label1.TextDetached = true;

		this.txtValidateCode.AutoSize = false;
		this.txtValidateCode.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtValidateCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtValidateCode.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtValidateCode.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtValidateCode.Location = new System.Drawing.Point(130, 8);
		this.txtValidateCode.Name = "txtValidateCode";
		this.txtValidateCode.Size = new System.Drawing.Size(260, 57);
		this.txtValidateCode.TabIndex = 26;
		this.txtValidateCode.Tag = null;
		this.txtValidateCode.TextDetached = true;
		this.txtValidateCode.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;

		// tabSMS
		this.tabSMS.BackColor = System.Drawing.Color.White;
		this.tabSMS.Controls.Add(this.c1Label3);
		this.tabSMS.Controls.Add(this.lblVerification);
		this.tabSMS.Controls.Add(this.txtVerification);
		this.tabSMS.Controls.Add(this.btnGetValidateCode);
		this.tabSMS.Location = new System.Drawing.Point(0, 3);
		this.tabSMS.Name = "tabSMS";
		this.tabSMS.Size = new System.Drawing.Size(878, 73);
		this.tabSMS.TabIndex = 1;
		this.tabSMS.Text = "第2页";

		this.c1Label3.AutoSize = true;
		this.c1Label3.BackColor = System.Drawing.Color.Transparent;
		this.c1Label3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label3.ForeColor = System.Drawing.Color.FromArgb(220, 80, 80);
		this.c1Label3.Location = new System.Drawing.Point(10, 23);
		this.c1Label3.Name = "c1Label3";
		this.c1Label3.Size = new System.Drawing.Size(26, 34);
		this.c1Label3.TabIndex = 48;
		this.c1Label3.Tag = null;
		this.c1Label3.Text = "*";
		this.c1Label3.TextDetached = true;

		this.lblVerification.AutoSize = true;
		this.lblVerification.BackColor = System.Drawing.Color.Transparent;
		this.lblVerification.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVerification.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblVerification.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.lblVerification.Location = new System.Drawing.Point(39, 23);
		this.lblVerification.Name = "lblVerification";
		this.lblVerification.Size = new System.Drawing.Size(104, 34);
		this.lblVerification.TabIndex = 25;
		this.lblVerification.Tag = null;
		this.lblVerification.Text = "激活码";
		this.lblVerification.TextDetached = true;

		this.txtVerification.AutoSize = false;
		this.txtVerification.BorderColor = System.Drawing.Color.FromArgb(210, 210, 210);
		this.txtVerification.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtVerification.ForeColor = System.Drawing.Color.FromArgb(51, 51, 51);
		this.txtVerification.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtVerification.Location = new System.Drawing.Point(156, 8);
		this.txtVerification.Name = "txtVerification";
		this.txtVerification.Size = new System.Drawing.Size(260, 57);
		this.txtVerification.TabIndex = 10;
		this.txtVerification.Tag = null;
		this.txtVerification.TextDetached = true;
		this.txtVerification.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;

		this.btnGetValidateCode.BackColor = System.Drawing.Color.FromArgb(50, 150, 220);
		this.btnGetValidateCode.FlatAppearance.BorderSize = 0;
		this.btnGetValidateCode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, 170, 240);
		this.btnGetValidateCode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(30, 120, 190);
		this.btnGetValidateCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnGetValidateCode.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnGetValidateCode.ForeColor = System.Drawing.Color.White;
		this.btnGetValidateCode.Format = null;
		this.btnGetValidateCode.Location = new System.Drawing.Point(702, 8);
		this.btnGetValidateCode.Name = "btnGetValidateCode";
		this.btnGetValidateCode.Size = new System.Drawing.Size(156, 57);
		this.btnGetValidateCode.TabIndex = 11;
		this.btnGetValidateCode.Text = "获取验证码";
		this.btnGetValidateCode.UseVisualStyleBackColor = false;
		this.btnGetValidateCode.Visible = false;
		this.btnGetValidateCode.Click += new System.EventHandler(btnGetValidateCode_Click);

		// btnRegister
		this.btnRegister.BackColor = System.Drawing.Color.FromArgb(50, 150, 220);
		this.btnRegister.FlatAppearance.BorderSize = 0;
		this.btnRegister.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(80, 170, 240);
		this.btnRegister.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(30, 120, 190);
		this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnRegister.Font = new System.Drawing.Font("Noto Sans SC", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnRegister.ForeColor = System.Drawing.Color.White;
		this.btnRegister.Location = new System.Drawing.Point(364, 533);
		this.btnRegister.Name = "btnRegister";
		this.btnRegister.Size = new System.Drawing.Size(234, 70);
		this.btnRegister.TabIndex = 12;
		this.btnRegister.Text = "注册";
		this.btnRegister.UseVisualStyleBackColor = false;
		this.btnRegister.Click += new System.EventHandler(btnRegister_Click);

		// btnClose
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.BackColor = System.Drawing.Color.Transparent;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(255, 200, 200);
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Image = Auditai.UI.Platform.Properties.Resources.close2;
		this.btnClose.Location = new System.Drawing.Point(988, 16);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(49, 49);
		this.btnClose.TabIndex = 40;
		this.btnClose.UseVisualStyleBackColor = false;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);

		// warnName
		this.warnName.AutoSize = true;
		this.warnName.BackColor = System.Drawing.Color.Transparent;
		this.warnName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnName.ForeColor = System.Drawing.Color.FromArgb(200, 60, 60);
		this.warnName.Location = new System.Drawing.Point(0, 99);
		this.warnName.Name = "warnName";
		this.warnName.Size = new System.Drawing.Size(0, 34);
		this.warnName.TabIndex = 16;
		this.warnName.Tag = null;
		this.warnName.TextDetached = true;
		this.warnName.Visible = false;

		base.AcceptButton = this.btnRegister;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
		this.BackgroundImage = null;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.ClientSize = new System.Drawing.Size(1066, 741);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.pnlCard);
		this.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
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
		this.pnlCard.ResumeLayout(false);
		this.pnlCard.PerformLayout();
		this.pnlLeftColumn.ResumeLayout(false);
		this.pnlLeftColumn.PerformLayout();
		this.pnlRightColumn.ResumeLayout(false);
		this.pnlRightColumn.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}