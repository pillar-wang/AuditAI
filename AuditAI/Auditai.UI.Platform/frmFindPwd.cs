using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Input;
using Auditai.PlatformResource;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class frmFindPwd : Form
{
	// 小清新：天蓝主色 RGB(50,150,220)
	private static Color _auditaiMainColor = Color.FromArgb(50, 150, 220);

	// UI 缩放因子（用于 Paint 事件绘制坐标）
	private float _scaleFactor = 1.5f;

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

	private C1Label lblEmailVerification;

	private TimerButton btnGetVerification;

	private C1TextBoxEx txtPhoneValidate;

	private C1Label lblFindPwd;

	private C1Label lblUserName;

	private C1Label lblEmail;

	private C1Button btnFindPwd;

	private C1TextBoxEx txtNewPassword;

	private C1TextBoxEx txtUserName;

	private C1Label c1Label1;

	private C1TextBoxEx txtPhone;

	private C1Button btnClose;

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	private void frmFindPwd_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

	private void frmFindPwd_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			AnimateWindow(base.Handle, 100, 851968);
		}
	}

	private void frmFindPwd_Paint(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		// 小清新：浅天蓝渐变背景
		Rectangle bgRect = new Rectangle(0, 0, base.Width, base.Height);
		using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			bgRect, Color.FromArgb(240, 248, 255), Color.FromArgb(225, 240, 255),
			System.Drawing.Drawing2D.LinearGradientMode.Vertical))
		{
			g.FillRectangle(bgBrush, bgRect);
		}
		// 顶部细光带
		Rectangle topBar = new Rectangle(0, 0, base.Width, (int)(3 * _scaleFactor));
		using (var topBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			topBar, Color.FromArgb(80, 170, 240), Color.FromArgb(50, 150, 220),
			System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
		{
			g.FillRectangle(topBrush, topBar);
		}
	}

	/// <summary>给窗体应用圆角区域</summary>
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

	public frmFindPwd()
	{
		InitializeComponent();
		InitPlatformStyle();
		base.StartPosition = FormStartPosition.CenterScreen;
		ApplyRoundedRegion(12);
		ApplyRoundedButton(btnFindPwd, 9);
		ApplyRoundedButton(btnGetVerification, 9);
		ApplyRoundedButton(btnClose, 9);
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
		case PlatformType.ProductionCostAccountingSystem:
		case PlatformType.ContractLedgerManagementSystem:
		case PlatformType.RDExpenseLedgerSystem:
		case PlatformType.SalesOrderManagementSystem:
		case PlatformType.PSIManagementSystem:
		case PlatformType.ProjectLedgerManagementSystem:
			InitPlatform_TableDevelop();
			break;
		case PlatformType.Custom:
			InitPlatform_Custom();
			break;
		}
	}

	private void InitColor()
	{
		// 小清新：强制天蓝色系，覆盖平台默认配色
		_auditaiMainColor = Color.FromArgb(50, 150, 220);
		btnGetVerification.BackColor = _auditaiMainColor;
		btnGetVerification.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 120, 190);
		btnGetVerification.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 170, 240);
		btnFindPwd.BackColor = _auditaiMainColor;
		btnFindPwd.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 120, 190);
		btnFindPwd.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 170, 240);
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

	private void InitPlatform_Custom()
	{
	}

	private void frmFindPwd_Load(object sender, EventArgs e)
	{
		AnimateWindow(base.Handle, 100, 524288);
		Refresh();
		foreach (object control in base.Controls)
		{
			C1TextBox tb = control as C1TextBox;
			if (tb != null)
			{
				tb.BorderColor = Color.FromArgb(210, 210, 210);
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
		base.AcceptButton = btnFindPwd;
		txtPhone.Focus();
	}

	private async void btnGetVerification_Click(object sender, EventArgs e)
	{
		try
		{
			string userName = await WebApiClient.GetUsernameByPhone(txtPhone.Text);
			txtUserName.Text = userName;
			btnGetVerification.Start(120);
			await WebApiClient.GetCodeByName(userName, "3");
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async void btnFindPwd_Click(object sender, EventArgs e)
	{
		try
		{
			if (!Regex.IsMatch(txtPhoneValidate.Text.Trim(), "^\\w+$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "验证码格式不正确");
			}
			if (!Regex.IsMatch(txtNewPassword.Text.Trim(), "^\\w{6,20}$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "新密码长度为6-20位字符！");
				return;
			}
			await WebApiClient.FindPassword(txtUserName.Text.Trim(), txtNewPassword.Text.Trim(), txtPhoneValidate.Text.Trim());
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改成功！");
			Close();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async Task<bool> UserNameValidate()
	{
		if (txtUserName.Text.Length == 0)
		{
			txtPhone.Text = string.Empty;
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名不能为空");
			return false;
		}
		try
		{
			string text;
			try
			{
				text = await WebApiClient.GetFuzzyPhone(txtUserName.Text.Trim());
			}
			catch (HttpRequestException ex)
			{
				txtPhone.Text = string.Empty;
				string message = ex.InnerException.Message;
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, message.Contains("用户不存在") ? "该账号未捆绑手机号，无法重置密码。" : message);
				return false;
			}
			if (!Regex.IsMatch(text, "^[0-9]{11,11}$"))
			{
				txtPhone.Text = string.Empty;
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您未绑定手机号，无法重置密码。请联系客服人员");
				return false;
			}
			string text2 = text.Remove(3, 4).Insert(3, "****");
			txtPhone.Text = text2;
			return true;
		}
		catch (HttpRequestException ex2)
		{
			txtPhone.Text = string.Empty;
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.InnerException.Message);
			return false;
		}
		catch (TimeoutException ex3)
		{
			txtPhone.Text = string.Empty;
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
			return false;
		}
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.lblEmailVerification = new C1.Win.C1Input.C1Label();
		this.txtPhoneValidate = new Auditai.UI.Controls.C1TextBoxEx();
		this.lblFindPwd = new C1.Win.C1Input.C1Label();
		this.lblUserName = new C1.Win.C1Input.C1Label();
		this.lblEmail = new C1.Win.C1Input.C1Label();
		this.btnFindPwd = new C1.Win.C1Input.C1Button();
		this.txtNewPassword = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtUserName = new Auditai.UI.Controls.C1TextBoxEx();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.txtPhone = new Auditai.UI.Controls.C1TextBoxEx();
		this.btnGetVerification = new Auditai.UI.Controls.TimerButton();
		this.btnClose = new C1.Win.C1Input.C1Button();
		((System.ComponentModel.ISupportInitialize)this.lblEmailVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhoneValidate).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblFindPwd).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnFindPwd).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		base.SuspendLayout();
		this.lblEmailVerification.AutoSize = true;
		this.lblEmailVerification.BackColor = System.Drawing.Color.Transparent;
		this.lblEmailVerification.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEmailVerification.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEmailVerification.ForeColor = System.Drawing.Color.Black;
		this.lblEmailVerification.Location = new System.Drawing.Point(27, 207);
		this.lblEmailVerification.Name = "lblEmailVerification";
		this.lblEmailVerification.Size = new System.Drawing.Size(102, 26);
		this.lblEmailVerification.TabIndex = 82;
		this.lblEmailVerification.Tag = null;
		this.lblEmailVerification.Text = "短信验证码";
		this.lblEmailVerification.TextDetached = true;
		this.txtPhoneValidate.AutoSize = false;
		this.txtPhoneValidate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPhoneValidate.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhoneValidate.Location = new System.Drawing.Point(148, 196);
		this.txtPhoneValidate.Name = "txtPhoneValidate";
		this.txtPhoneValidate.Size = new System.Drawing.Size(282, 48);
		this.txtPhoneValidate.TabIndex = 3;
		this.txtPhoneValidate.Tag = null;
		this.txtPhoneValidate.TextDetached = true;
		this.txtPhoneValidate.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.lblFindPwd.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.lblFindPwd.AutoSize = true;
		this.lblFindPwd.BackColor = System.Drawing.Color.Transparent;
		this.lblFindPwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblFindPwd.Font = new System.Drawing.Font("Noto Sans SC", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblFindPwd.ForeColor = System.Drawing.Color.Black;
		this.lblFindPwd.Location = new System.Drawing.Point(192, 45);
		this.lblFindPwd.Name = "lblFindPwd";
		this.lblFindPwd.Size = new System.Drawing.Size(132, 38);
		this.lblFindPwd.TabIndex = 76;
		this.lblFindPwd.Tag = null;
		this.lblFindPwd.Text = "重置密码";
		this.lblFindPwd.TextDetached = true;
		this.lblUserName.AutoSize = true;
		this.lblUserName.BackColor = System.Drawing.Color.Transparent;
		this.lblUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblUserName.ForeColor = System.Drawing.Color.Black;
		this.lblUserName.Location = new System.Drawing.Point(63, 288);
		this.lblUserName.Name = "lblUserName";
		this.lblUserName.Size = new System.Drawing.Size(66, 26);
		this.lblUserName.TabIndex = 74;
		this.lblUserName.Tag = null;
		this.lblUserName.Text = "用户名";
		this.lblUserName.TextDetached = true;
		this.lblEmail.AutoSize = true;
		this.lblEmail.BackColor = System.Drawing.Color.Transparent;
		this.lblEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEmail.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEmail.ForeColor = System.Drawing.Color.Black;
		this.lblEmail.Location = new System.Drawing.Point(63, 129);
		this.lblEmail.Name = "lblEmail";
		this.lblEmail.Size = new System.Drawing.Size(66, 26);
		this.lblEmail.TabIndex = 71;
		this.lblEmail.Tag = null;
		this.lblEmail.Text = "手机号";
		this.lblEmail.TextDetached = true;
		this.btnFindPwd.BackColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.btnFindPwd.FlatAppearance.BorderSize = 0;
		this.btnFindPwd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnFindPwd.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnFindPwd.ForeColor = System.Drawing.Color.White;
		this.btnFindPwd.Location = new System.Drawing.Point(164, 432);
		this.btnFindPwd.Name = "btnFindPwd";
		this.btnFindPwd.Size = new System.Drawing.Size(190, 51);
		this.btnFindPwd.TabIndex = 5;
		this.btnFindPwd.Text = "确定";
		this.btnFindPwd.UseVisualStyleBackColor = false;
		this.btnFindPwd.Click += new System.EventHandler(btnFindPwd_Click);
		this.txtNewPassword.AutoSize = false;
		this.txtNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNewPassword.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtNewPassword.Location = new System.Drawing.Point(148, 354);
		this.txtNewPassword.Name = "txtNewPassword";
		this.txtNewPassword.Size = new System.Drawing.Size(282, 48);
		this.txtNewPassword.TabIndex = 4;
		this.txtNewPassword.Tag = null;
		this.txtNewPassword.TextDetached = true;
		this.txtNewPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtUserName.AutoSize = false;
		this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtUserName.Location = new System.Drawing.Point(148, 276);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.ReadOnly = true;
		this.txtUserName.Size = new System.Drawing.Size(282, 48);
		this.txtUserName.TabIndex = 0;
		this.txtUserName.Tag = null;
		this.txtUserName.TextDetached = true;
		this.txtUserName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.c1Label1.AutoSize = true;
		this.c1Label1.BackColor = System.Drawing.Color.Transparent;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.ForeColor = System.Drawing.Color.Black;
		this.c1Label1.Location = new System.Drawing.Point(63, 363);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(66, 26);
		this.c1Label1.TabIndex = 83;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "新密码";
		this.c1Label1.TextDetached = true;
		this.txtPhone.AutoSize = false;
		this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPhone.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhone.Location = new System.Drawing.Point(148, 117);
		this.txtPhone.Name = "txtPhone";
		this.txtPhone.Size = new System.Drawing.Size(159, 48);
		this.txtPhone.TabIndex = 1;
		this.txtPhone.Tag = null;
		this.txtPhone.TextDetached = true;
		this.txtPhone.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.btnGetVerification.BackColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.btnGetVerification.FlatAppearance.BorderSize = 0;
		this.btnGetVerification.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnGetVerification.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnGetVerification.ForeColor = System.Drawing.Color.White;
		this.btnGetVerification.Format = "(0s)";
		this.btnGetVerification.Location = new System.Drawing.Point(316, 117);
		this.btnGetVerification.Name = "btnGetVerification";
		this.btnGetVerification.Size = new System.Drawing.Size(114, 48);
		this.btnGetVerification.TabIndex = 2;
		this.btnGetVerification.Text = "获取验证码";
		this.btnGetVerification.UseVisualStyleBackColor = false;
		this.btnGetVerification.Click += new System.EventHandler(btnGetVerification_Click);
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Image = Auditai.UI.Platform.Properties.Resources.close2;
		this.btnClose.Location = new System.Drawing.Point(472, 2);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(38, 38);
		this.btnClose.TabIndex = 84;
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		base.AcceptButton = this.btnFindPwd;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.ClientSize = new System.Drawing.Size(512, 514);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.c1Label1);
		base.Controls.Add(this.lblEmailVerification);
		base.Controls.Add(this.btnGetVerification);
		base.Controls.Add(this.txtPhoneValidate);
		base.Controls.Add(this.lblFindPwd);
		base.Controls.Add(this.lblUserName);
		base.Controls.Add(this.lblEmail);
		base.Controls.Add(this.btnFindPwd);
		base.Controls.Add(this.txtNewPassword);
		base.Controls.Add(this.txtPhone);
		base.Controls.Add(this.txtUserName);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "frmFindPwd";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "重置密码";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmFindPwd_FormClosing);
		base.Load += new System.EventHandler(frmFindPwd_Load);
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmFindPwd_Paint);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(frmFindPwd_MouseDown);
		((System.ComponentModel.ISupportInitialize)this.lblEmailVerification).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhoneValidate).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblFindPwd).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnFindPwd).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetVerification).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
