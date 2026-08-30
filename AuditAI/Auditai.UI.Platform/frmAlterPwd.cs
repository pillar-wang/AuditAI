﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class frmAlterPwd : C1RibbonForm
{
	// Google Blue 主色（用于输入框聚焦边框等）
	private readonly Color _auditaiBlue = AuditTheme.Brand;

	private const int TIMEDOWNTOTAL = 120;

	private bool _parasValid = true;

	private bool _bindPhone;

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

	private C1TextBoxEx txtPassword;

	private C1TextBoxEx txtNewPassword;

	private C1TextBoxEx txtNewPassword2;

	private C1TextBoxEx txtVerification;

	private C1Button btnCertain;

	private C1Label warnNewPassword;

	private C1Label warnNewPassword2;

	private C1Label lblPassword;

	private C1Label lblPassword1;

	private C1Label lblVerification;

	private C1Label lblPassword2;

	private C1Label lblMustInputStar1;

	private C1Label lblMustInputStar2;

	private C1Label lblMustInputStar3;

	private C1Button btnCancel;

	private TimerButton btnGetValidateCode;

	private C1Label lblPhone;

	private C1TextBoxEx txtPhone;

	/// <summary>给窗体应用圆角区域（统一样式调整，参考 frmFindPwd 实现）</summary>
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

	/// <summary>给按钮应用圆角区域（统一样式调整）</summary>
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

	/// <summary>
	/// 统一样式调整：重新应用主/次按钮配色。
	/// C1Button 的皮肤(VisualStyle)在控件初始化完成时会覆盖设计时 BackColor（实测被刷成皮肤灰），
	/// 必须在 InitializeComponent 之后重设一次。
	/// </summary>
	private void RefreshButtonStyles()
	{
		// 主按钮：Brand 蓝底白字
		btnCertain.BackColor = AuditTheme.Brand;
		btnCertain.ForeColor = Color.White;
		btnCertain.FlatStyle = FlatStyle.Flat;
		btnCertain.FlatAppearance.BorderSize = 0;
		btnCertain.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		btnCertain.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		// 次按钮：白底灰字 + 浅蓝边框
		btnCancel.BackColor = Color.White;
		btnCancel.ForeColor = Color.FromArgb(30, 41, 59);
		btnCancel.FlatStyle = FlatStyle.Flat;
		btnCancel.FlatAppearance.BorderSize = 1;
		btnCancel.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
		btnGetValidateCode.BackColor = Color.White;
		btnGetValidateCode.ForeColor = Color.FromArgb(30, 41, 59);
		btnGetValidateCode.FlatStyle = FlatStyle.Flat;
		btnGetValidateCode.FlatAppearance.BorderSize = 1;
		btnGetValidateCode.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
	}

	/// <summary>窗体显示后重新应用圆角：C1RibbonForm 创建句柄时会重置 Region，构造时设置的圆角会失效</summary>
	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		ApplyRoundedRegion(AuditTheme.CardRadius);
	}

	public frmAlterPwd()
	{
		InitializeComponent();

		// 统一样式调整：窗体 12px 圆角 + 按钮 8px 圆角，须在布局/尺寸确定后应用
		ApplyRoundedRegion(AuditTheme.CardRadius);
		ApplyRoundedButton(btnCertain, AuditTheme.ButtonRadius);
		ApplyRoundedButton(btnCancel, AuditTheme.ButtonRadius);
		ApplyRoundedButton(btnGetValidateCode, AuditTheme.ButtonRadius);

		Initialize();
		// 统一样式调整：C1Button 皮肤会覆盖设计时配色，构造末尾重设主/次按钮样式
		RefreshButtonStyles();
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentTree(this);
		return base.ShowDialog();
	}

	private async void Initialize()
	{
		base.StartPosition = FormStartPosition.CenterScreen;
		AnimateWindow(base.Handle, 100, 524288);
		Refresh();
		foreach (object control in base.Controls)
		{
			C1TextBox tb = control as C1TextBox;
			if (tb != null)
			{
				tb.MouseEnter += delegate
				{
					tb.BorderColor = _auditaiBlue;
				};
				tb.MouseLeave += delegate
				{
					// 统一样式调整：hover 离开边框改用主题令牌，替代硬编码 LightGray
					tb.BorderColor = AuditTheme.BorderStrong;
				};
			}
		}
		base.AcceptButton = btnCertain;
		txtPassword.Focus();
		string result = null;
		try
		{
			result = await WebApiClient.GetFuzzyPhone(Auditai.Model.User.Current.UserName);
			_bindPhone = Regex.IsMatch(result, "^[0-9]{11,11}$");
		}
		catch (HttpRequestException)
		{
			_bindPhone = false;
		}
		if (_bindPhone)
		{
			txtPhone.Text = result.Remove(3, 4).Insert(3, "****");
			txtVerification.Enabled = true;
			btnGetValidateCode.Enabled = true;
		}
		else
		{
			txtVerification.Enabled = false;
			btnGetValidateCode.Enabled = false;
			txtPhone.Text = string.Empty;
		}
		if (!_bindPhone && !Program.IsOnPremise && Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您的账号未捆绑手机号，出于保护账号安全和遗忘密码后可安全找回的需要，建议您捆绑手机号，需要马上捆绑手机号吗？", MessageBoxButtons.OKCancel) == DialogResult.OK)
		{
			Close();
			frmAlterInfo frmAlterInfo2 = new frmAlterInfo();
			frmAlterInfo2.FocusPhone();
			if (frmAlterInfo2.ShowDialog() == DialogResult.OK)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改成功！");
			}
		}
	}

	private bool ValidateAllText()
	{
		_parasValid = true;
		txtNewPassword.ValidateText();
		txtNewPassword2.ValidateText();
		return _parasValid;
	}

	private void SetError(C1TextBox inputBox, Label warnLable)
	{
		_parasValid = false;
		warnLable.ForeColor = Color.Red;
		warnLable.Visible = true;
		inputBox.BorderColor = Color.Red;
	}

	private void SetCorrect(C1TextBox inputBox, Label warnLable)
	{
		warnLable.ForeColor = Color.Gray;
		warnLable.Visible = false;
		inputBox.BorderColor = Color.LightGray;
	}

	private async void btnCertain_Click(object sender, EventArgs e)
	{
		// 修复 BUG: 此前缺少防重复点击机制，用户在 await ResetPassword 期间可重复点击按钮，
		// 导致多次发送修改密码请求。现在禁用按钮直到异步操作完成。
		btnCertain.Enabled = false;
		try
		{
			if (_bindPhone && !Regex.IsMatch(txtVerification.Text.Trim(), "^\\w+$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "验证码格式不正确！");
			}
			else if (ValidateAllText())
			{
				string oldPassword = txtPassword.Text.Trim();
				string newpassword = txtNewPassword.Text.Trim();
				if (!_bindPhone)
				{
					await WebApiClient.ResetPasswordWithoutSMS(oldPassword, newpassword);
				}
				else
				{
					string validateCode = txtVerification.Text.Trim();
					await WebApiClient.ResetPassword(oldPassword, newpassword, validateCode);
				}
				UserSet.LoginPassword = frmLogin.GetPasswordEncryptValue(newpassword);
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改成功！");
				Close();
			}
		}
		catch (NormalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			if (_bindPhone)
			{
				btnGetValidateCode.Reset("获取验证码");
			}
		}
		catch (ServerException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
			if (_bindPhone)
			{
				btnGetValidateCode.Reset("获取验证码");
			}
		}
		catch (HttpRequestException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException?.Message ?? ex3.Message);
			if (_bindPhone)
			{
				btnGetValidateCode.Reset("获取验证码");
			}
		}
		catch (TimeoutException ex4)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
			if (_bindPhone)
			{
				btnGetValidateCode.Reset("获取验证码");
			}
		}
		finally
		{
			btnCertain.Enabled = true;
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private async void btnGetValidateCode_Click(object sender, EventArgs e)
	{
		try
		{
			if (txtPassword.Text.Length == 0)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先输入原密码！");
			}
			else if (ValidateAllText())
			{
				_ = string.Empty;
				string phone = (await WebApiClient.GetUserById(Auditai.Model.User.Current.Id)).Phone;
				btnGetValidateCode.Start(120);
				await WebApiClient.GetValidateCode(phone, "2");
			}
		}
		catch (NormalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			btnGetValidateCode.Reset("获取验证码");
		}
		catch (ServerException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
			btnGetValidateCode.Reset("获取验证码");
		}
		catch (HttpRequestException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException?.Message ?? ex3.Message);
			btnGetValidateCode.Reset("获取验证码");
		}
		catch (TimeoutException ex4)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
			btnGetValidateCode.Reset("获取验证码");
		}
	}

	private void txtNewPassword_Validated(object sender, EventArgs e)
	{
		if (Regex.IsMatch(txtNewPassword.Text.Trim(), "^\\w{6,20}$"))
		{
			SetCorrect(txtNewPassword, warnNewPassword);
		}
		else
		{
			SetError(txtNewPassword, warnNewPassword);
		}
	}

	private void txtNewPassword2_Validated(object sender, EventArgs e)
	{
		if (txtNewPassword2.Text.Trim() == txtNewPassword.Text.Trim())
		{
			SetCorrect(txtNewPassword2, warnNewPassword2);
		}
		else
		{
			SetError(txtNewPassword2, warnNewPassword2);
		}
	}

	private void txtPassword_Enter(object sender, EventArgs e)
	{
		warnNewPassword.Visible = true;
	}

	private void txtPassword2_Enter(object sender, EventArgs e)
	{
		warnNewPassword2.Visible = true;
	}

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	private void frmAlterPwd_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

	private void frmAlterPwd_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			AnimateWindow(base.Handle, 100, 851968);
		}
	}

	private void frmAlterPwd_Load(object sender, EventArgs e)
	{
	}

	// 统一样式调整：浅蓝渐变背景 + 顶部 3px 主色光带（参考 frmFindPwd_Paint）
	private void frmAlterPwd_Paint(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		Rectangle bgRect = new Rectangle(0, 0, base.Width, base.Height);
		// 垂直渐变：浅天蓝 → 浅灰白
		using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			bgRect, Color.FromArgb(227, 240, 255), Color.FromArgb(245, 248, 250),
			System.Drawing.Drawing2D.LinearGradientMode.Vertical))
		{
			g.FillRectangle(bgBrush, bgRect);
		}
		// 第二层对角半透明渐变增强
		using (var bgBrush2 = new System.Drawing.Drawing2D.LinearGradientBrush(
			new Point(base.Width, 0),
			new Point(0, base.Height),
			Color.FromArgb(150, 220, 235, 255),
			Color.FromArgb(150, 240, 248, 255)))
		{
			g.FillRectangle(bgBrush2, bgRect);
		}
		// 顶部 3px 主色光带（水平渐变）
		Rectangle topBar = new Rectangle(0, 0, base.Width, 3);
		using (var topBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			topBar, Color.FromArgb(90, 160, 230), Color.FromArgb(74, 144, 226),
			System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
		{
			g.FillRectangle(topBrush, topBar);
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
		this.txtPassword = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtNewPassword = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtNewPassword2 = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtVerification = new Auditai.UI.Controls.C1TextBoxEx();
		this.btnCertain = new C1.Win.C1Input.C1Button();
		this.warnNewPassword = new C1.Win.C1Input.C1Label();
		this.warnNewPassword2 = new C1.Win.C1Input.C1Label();
		this.lblPassword = new C1.Win.C1Input.C1Label();
		this.lblPassword1 = new C1.Win.C1Input.C1Label();
		this.lblVerification = new C1.Win.C1Input.C1Label();
		this.lblPassword2 = new C1.Win.C1Input.C1Label();
		this.lblMustInputStar1 = new C1.Win.C1Input.C1Label();
		this.lblMustInputStar2 = new C1.Win.C1Input.C1Label();
		this.lblMustInputStar3 = new C1.Win.C1Input.C1Label();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnGetValidateCode = new Auditai.UI.Controls.TimerButton();
		this.lblPhone = new C1.Win.C1Input.C1Label();
		this.txtPhone = new Auditai.UI.Controls.C1TextBoxEx();
		((System.ComponentModel.ISupportInitialize)this.txtPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.warnNewPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.warnNewPassword2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar3).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetValidateCode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).BeginInit();
		base.SuspendLayout();
		this.txtPassword.AutoSize = false;
		this.txtPassword.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPassword.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword.Location = new System.Drawing.Point(126, 26);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '●';
		this.txtPassword.Size = new System.Drawing.Size(299, 42);
		this.txtPassword.TabIndex = 0;
		this.txtPassword.Tag = null;
		this.txtPassword.TextDetached = true;
		this.txtPassword.Value = "";
		this.txtPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtNewPassword.AutoSize = false;
		this.txtNewPassword.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNewPassword.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtNewPassword.Location = new System.Drawing.Point(126, 226);
		this.txtNewPassword.Name = "txtNewPassword";
		this.txtNewPassword.PasswordChar = '●';
		this.txtNewPassword.Size = new System.Drawing.Size(299, 42);
		this.txtNewPassword.TabIndex = 1;
		this.txtNewPassword.Tag = null;
		this.txtNewPassword.TextDetached = true;
		this.txtNewPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtNewPassword.Enter += new System.EventHandler(txtPassword_Enter);
		this.txtNewPassword.Validated += new System.EventHandler(txtNewPassword_Validated);
		this.txtNewPassword2.AutoSize = false;
		this.txtNewPassword2.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtNewPassword2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNewPassword2.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtNewPassword2.Location = new System.Drawing.Point(126, 304);
		this.txtNewPassword2.Name = "txtNewPassword2";
		this.txtNewPassword2.PasswordChar = '●';
		this.txtNewPassword2.Size = new System.Drawing.Size(299, 42);
		this.txtNewPassword2.TabIndex = 2;
		this.txtNewPassword2.Tag = null;
		this.txtNewPassword2.TextDetached = true;
		this.txtNewPassword2.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtNewPassword2.Enter += new System.EventHandler(txtPassword2_Enter);
		this.txtNewPassword2.Validated += new System.EventHandler(txtNewPassword2_Validated);
		this.txtVerification.AutoSize = false;
		this.txtVerification.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtVerification.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtVerification.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtVerification.Location = new System.Drawing.Point(126, 156);
		this.txtVerification.Name = "txtVerification";
		this.txtVerification.Size = new System.Drawing.Size(192, 42);
		this.txtVerification.TabIndex = 3;
		this.txtVerification.Tag = null;
		this.txtVerification.TextDetached = true;
		this.txtVerification.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		// 统一样式调整：主按钮 = Brand 蓝底白字 + Flat 无边框
		this.btnCertain.BackColor = AuditTheme.Brand;
		this.btnCertain.ForeColor = System.Drawing.Color.White;
		this.btnCertain.FlatAppearance.BorderSize = 0;
		this.btnCertain.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		this.btnCertain.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		this.btnCertain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCertain.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		// 统一样式调整：主按钮高度统一 40
		this.btnCertain.Location = new System.Drawing.Point(81, 410);
		this.btnCertain.Name = "btnCertain";
		this.btnCertain.Size = new System.Drawing.Size(130, 40);
		this.btnCertain.TabIndex = 5;
		this.btnCertain.Text = "确定";
		this.btnCertain.UseVisualStyleBackColor = false;
		this.btnCertain.Click += new System.EventHandler(btnCertain_Click);
		this.warnNewPassword.AutoSize = true;
		this.warnNewPassword.BackColor = System.Drawing.Color.Transparent;
		this.warnNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnNewPassword.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnNewPassword.ForeColor = System.Drawing.Color.Black;
		this.warnNewPassword.Location = new System.Drawing.Point(133, 273);
		this.warnNewPassword.Name = "warnNewPassword";
		this.warnNewPassword.Size = new System.Drawing.Size(216, 22);
		this.warnNewPassword.TabIndex = 14;
		this.warnNewPassword.Tag = null;
		this.warnNewPassword.Text = "长度在6-20个字符区分大小写";
		this.warnNewPassword.TextDetached = true;
		this.warnNewPassword.Visible = false;
		this.warnNewPassword2.AutoSize = true;
		this.warnNewPassword2.BackColor = System.Drawing.Color.Transparent;
		this.warnNewPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnNewPassword2.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnNewPassword2.ForeColor = System.Drawing.Color.Black;
		this.warnNewPassword2.Location = new System.Drawing.Point(133, 351);
		this.warnNewPassword2.Name = "warnNewPassword2";
		this.warnNewPassword2.Size = new System.Drawing.Size(151, 22);
		this.warnNewPassword2.TabIndex = 15;
		this.warnNewPassword2.Tag = null;
		this.warnNewPassword2.Text = "请再输入一次密码！";
		this.warnNewPassword2.TextDetached = true;
		this.warnNewPassword2.Visible = false;
		this.lblPassword.AutoSize = true;
		this.lblPassword.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword.ForeColor = System.Drawing.Color.Black;
		this.lblPassword.Location = new System.Drawing.Point(46, 34);
		this.lblPassword.Name = "lblPassword";
		this.lblPassword.Size = new System.Drawing.Size(73, 22);
		this.lblPassword.TabIndex = 18;
		this.lblPassword.Tag = null;
		this.lblPassword.Text = "登录密码";
		this.lblPassword.TextDetached = true;
		this.lblPassword1.AutoSize = true;
		this.lblPassword1.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword1.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword1.ForeColor = System.Drawing.Color.Black;
		this.lblPassword1.Location = new System.Drawing.Point(31, 304);
		this.lblPassword1.Name = "lblPassword1";
		this.lblPassword1.Size = new System.Drawing.Size(88, 22);
		this.lblPassword1.TabIndex = 19;
		this.lblPassword1.Tag = null;
		this.lblPassword1.Text = "确认新密码";
		this.lblPassword1.TextDetached = true;
		this.lblVerification.AutoSize = true;
		this.lblVerification.BackColor = System.Drawing.Color.Transparent;
		this.lblVerification.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVerification.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblVerification.ForeColor = System.Drawing.Color.Black;
		this.lblVerification.Location = new System.Drawing.Point(30, 164);
		this.lblVerification.Name = "lblVerification";
		this.lblVerification.Size = new System.Drawing.Size(88, 22);
		this.lblVerification.TabIndex = 25;
		this.lblVerification.Tag = null;
		this.lblVerification.Text = "短信验证码";
		this.lblVerification.TextDetached = true;
		this.lblPassword2.AutoSize = true;
		this.lblPassword2.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword2.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword2.ForeColor = System.Drawing.Color.Black;
		// 修复：此"新密码"标签原误置于 y181，与上方的"短信验证码"标签( y164–186,
		// AutoSize) 重叠约 6px，两者文本相互碰撞。将其移到"新密码"输入框 txtNewPassword
		// (y226) 行同高对齐（y=输入y+8 居中，与 lblPassword/txtPassword 的对齐规则一致），
		// 既消除碰撞，也补回了新密码输入项原本缺失的标签。
		this.lblPassword2.Location = new System.Drawing.Point(46, 234);
		this.lblPassword2.Name = "lblPassword2";
		this.lblPassword2.Size = new System.Drawing.Size(57, 22);
		this.lblPassword2.TabIndex = 27;
		this.lblPassword2.Tag = null;
		this.lblPassword2.Text = "新密码";
		this.lblPassword2.TextDetached = true;
		this.lblMustInputStar1.AutoSize = true;
		this.lblMustInputStar1.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar1.ForeColor = System.Drawing.Color.Black;
		this.lblMustInputStar1.Location = new System.Drawing.Point(31, 26);
		this.lblMustInputStar1.Name = "lblMustInputStar1";
		this.lblMustInputStar1.Size = new System.Drawing.Size(14, 16);
		this.lblMustInputStar1.TabIndex = 34;
		this.lblMustInputStar1.Tag = null;
		this.lblMustInputStar1.Text = "*";
		this.lblMustInputStar1.TextDetached = true;
		this.lblMustInputStar2.AutoSize = true;
		this.lblMustInputStar2.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar2.ForeColor = System.Drawing.Color.Black;
		// 修复：星号原 x=47 落在"新密码"标签(x=46)右侧且 Y 下探与其重叠，统一为标签左侧 15px（与星号1/3 规则一致）
		this.lblMustInputStar2.Location = new System.Drawing.Point(31, 226);
		this.lblMustInputStar2.Name = "lblMustInputStar2";
		this.lblMustInputStar2.Size = new System.Drawing.Size(14, 16);
		this.lblMustInputStar2.TabIndex = 35;
		this.lblMustInputStar2.Tag = null;
		this.lblMustInputStar2.Text = "*";
		this.lblMustInputStar2.TextDetached = true;
		this.lblMustInputStar3.AutoSize = true;
		this.lblMustInputStar3.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar3.ForeColor = System.Drawing.Color.Black;
		this.lblMustInputStar3.Location = new System.Drawing.Point(16, 304);
		this.lblMustInputStar3.Name = "lblMustInputStar3";
		this.lblMustInputStar3.Size = new System.Drawing.Size(14, 16);
		this.lblMustInputStar3.TabIndex = 36;
		this.lblMustInputStar3.Tag = null;
		this.lblMustInputStar3.Text = "*";
		this.lblMustInputStar3.TextDetached = true;
		// 统一样式调整：次按钮 = 白底 + 深灰字 + 浅蓝边框
		this.btnCancel.BackColor = System.Drawing.Color.White;
		this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
		this.btnCancel.FlatAppearance.BorderSize = 1;
		this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(208, 215, 222);
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCancel.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		// 统一样式调整：次按钮高度统一 40
		this.btnCancel.Location = new System.Drawing.Point(263, 410);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(130, 40);
		this.btnCancel.TabIndex = 6;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = false;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		// 统一样式调整：次按钮 = 白底 + 深灰字 + 浅蓝边框
		this.btnGetValidateCode.BackColor = System.Drawing.Color.White;
		this.btnGetValidateCode.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
		this.btnGetValidateCode.FlatAppearance.BorderSize = 1;
		this.btnGetValidateCode.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(208, 215, 222);
		this.btnGetValidateCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnGetValidateCode.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnGetValidateCode.Format = "(0s)";
		// 统一样式调整：次按钮高度统一 40
		this.btnGetValidateCode.Location = new System.Drawing.Point(326, 156);
		this.btnGetValidateCode.Name = "btnGetValidateCode";
		this.btnGetValidateCode.Size = new System.Drawing.Size(99, 40);
		this.btnGetValidateCode.TabIndex = 4;
		this.btnGetValidateCode.Text = "获取验证码";
		this.btnGetValidateCode.UseVisualStyleBackColor = false;
		this.btnGetValidateCode.Click += new System.EventHandler(btnGetValidateCode_Click);
		this.lblPhone.AutoSize = true;
		this.lblPhone.BackColor = System.Drawing.Color.Transparent;
		this.lblPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPhone.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPhone.ForeColor = System.Drawing.Color.Black;
		this.lblPhone.Location = new System.Drawing.Point(44, 100);
		this.lblPhone.Name = "lblPhone";
		this.lblPhone.Size = new System.Drawing.Size(73, 22);
		this.lblPhone.TabIndex = 37;
		this.lblPhone.Tag = null;
		this.lblPhone.Text = "预留手机";
		this.lblPhone.TextDetached = true;
		this.txtPhone.AutoSize = false;
		this.txtPhone.BackColor = System.Drawing.Color.FromArgb(239, 239, 239);
		this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPhone.Enabled = false;
		this.txtPhone.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhone.Location = new System.Drawing.Point(126, 92);
		this.txtPhone.Name = "txtPhone";
		this.txtPhone.ReadOnly = true;
		this.txtPhone.Size = new System.Drawing.Size(299, 42);
		this.txtPhone.TabIndex = 38;
		this.txtPhone.Tag = null;
		this.txtPhone.TextDetached = true;
		this.txtPhone.Value = "";
		this.txtPhone.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		base.AcceptButton = this.btnCertain;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.ClientSize = new System.Drawing.Size(481, 480);
		base.Controls.Add(this.txtPhone);
		base.Controls.Add(this.lblPhone);
		base.Controls.Add(this.btnGetValidateCode);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.lblMustInputStar3);
		base.Controls.Add(this.lblMustInputStar2);
		base.Controls.Add(this.lblMustInputStar1);
		base.Controls.Add(this.lblPassword2);
		base.Controls.Add(this.lblVerification);
		base.Controls.Add(this.lblPassword1);
		base.Controls.Add(this.lblPassword);
		base.Controls.Add(this.warnNewPassword2);
		base.Controls.Add(this.warnNewPassword);
		base.Controls.Add(this.btnCertain);
		base.Controls.Add(this.txtVerification);
		base.Controls.Add(this.txtNewPassword2);
		base.Controls.Add(this.txtNewPassword);
		base.Controls.Add(this.txtPassword);
		// 统一样式调整：无边框窗体（配合圆角 Region + 渐变背景 + MouseDown 拖拽）
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmAlterPwd";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "修改密码";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmAlterPwd_FormClosing);
		base.Load += new System.EventHandler(frmAlterPwd_Load);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(frmAlterPwd_MouseDown);
		// 统一样式调整：渐变背景 + 顶部光带绘制
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmAlterPwd_Paint);
		((System.ComponentModel.ISupportInitialize)this.txtPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtVerification).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.warnNewPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.warnNewPassword2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblVerification).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPassword2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar3).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetValidateCode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
