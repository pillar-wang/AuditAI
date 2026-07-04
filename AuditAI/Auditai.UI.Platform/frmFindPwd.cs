﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Input;
using Auditai.DTO;
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

		private C1TextBoxEx txtEmailValidate;

		private C1Label lblFindPwd;

		private C1Label lblUserName;

		private C1Label lblEmail;

		private C1Button btnFindPwd;

		private C1TextBoxEx txtNewPassword;

		private C1TextBoxEx txtUserName;

		private C1Label c1Label1;

		private C1TextBoxEx txtEmail;

	private C1Button btnClose;

	private Panel pnlCard;

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

	public frmFindPwd()
	{
		InitializeComponent();
		InitPlatformStyle();
		base.StartPosition = FormStartPosition.CenterScreen;
		ApplyRoundedRegion(12);
		ApplyRoundedButton(btnFindPwd, 9);
		ApplyRoundedButton(btnGetVerification, 9);
		ApplyRoundedButton(btnClose, 9);
		txtEmail.Focus();
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
		foreach (object control in pnlCard.Controls)
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
		txtEmail.Focus();
	}

	private async void btnGetVerification_Click(object sender, EventArgs e)
	{
		try
		{
			string email = txtEmail.Text.Trim();
			if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "邮箱格式不正确");
				return;
			}
			string userName = await WebApiClient.GetUsernameByEmail(email);
			if (string.IsNullOrEmpty(userName))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该邮箱未注册");
				return;
			}
			txtUserName.Text = userName;
			btnGetVerification.Start(120);
			await WebApiClient.GetValidateCodeByEmail(email);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "验证码已发送到邮箱，请查收");
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
		catch (TimeoutException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (ServerException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.ToString());
		}
	}

	private async void btnFindPwd_Click(object sender, EventArgs e)
	{
		try
		{
			if (!Regex.IsMatch(txtEmailValidate.Text.Trim(), "^\\d{6}$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "验证码格式不正确");
				return;
			}
			if (!Regex.IsMatch(txtNewPassword.Text.Trim(), "^\\w{6,20}$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "新密码长度为6-20位字符！");
				return;
			}
			await WebApiClient.FindPassword(txtUserName.Text.Trim(), txtNewPassword.Text.Trim(), txtEmailValidate.Text.Trim());
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改成功！");
			Close();
		}
		catch (HttpRequestException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException?.Message ?? ex.Message);
		}
		catch (TimeoutException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (ServerException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.ToString());
		}
	}

	

	private void btnClose_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

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
			bgRect, Color.FromArgb(227, 240, 255), Color.FromArgb(245, 248, 250),
			System.Drawing.Drawing2D.LinearGradientMode.Vertical))
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
		// 顶部细光带
		Rectangle topBar = new Rectangle(0, 0, base.Width, (int)(3 * _scaleFactor));
		using (var topBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			topBar, Color.FromArgb(90, 160, 230), Color.FromArgb(74, 144, 226),
			System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
		{
			g.FillRectangle(topBrush, topBar);
		}

		// 白色圆角卡片
		int cardX = (int)(75 * _scaleFactor), cardY = (int)(55 * _scaleFactor);
		int cardW = (int)(380 * _scaleFactor), cardH = (int)(530 * _scaleFactor);
		int radius = (int)(12 * _scaleFactor);
		using (var path = new System.Drawing.Drawing2D.GraphicsPath())
		{
			path.AddArc(cardX, cardY, radius * 2, radius * 2, 180, 90);
			path.AddArc(cardX + cardW - radius * 2, cardY, radius * 2, radius * 2, 270, 90);
			path.AddArc(cardX + cardW - radius * 2, cardY + cardH - radius * 2, radius * 2, radius * 2, 0, 90);
			path.AddArc(cardX, cardY + cardH - radius * 2, radius * 2, radius * 2, 90, 90);
			path.CloseFigure();

			// 卡片阴影
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
		this.txtEmailValidate = new Auditai.UI.Controls.C1TextBoxEx();
		this.lblFindPwd = new C1.Win.C1Input.C1Label();
		this.lblUserName = new C1.Win.C1Input.C1Label();
		this.lblEmail = new C1.Win.C1Input.C1Label();
		this.btnFindPwd = new C1.Win.C1Input.C1Button();
		this.txtNewPassword = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtUserName = new Auditai.UI.Controls.C1TextBoxEx();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.txtEmail = new Auditai.UI.Controls.C1TextBoxEx();
		this.btnGetVerification = new Auditai.UI.Controls.TimerButton();
		this.btnClose = new C1.Win.C1Input.C1Button();
		this.pnlCard = new System.Windows.Forms.Panel();
		((System.ComponentModel.ISupportInitialize)this.lblEmailVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmailValidate).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblFindPwd).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnFindPwd).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetVerification).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		this.pnlCard.SuspendLayout();
		base.SuspendLayout();
		// pnlCard
		this.pnlCard.BackColor = System.Drawing.Color.White;
		this.pnlCard.Controls.Add(this.lblFindPwd);
		this.pnlCard.Controls.Add(this.lblEmail);
		this.pnlCard.Controls.Add(this.txtEmail);
		this.pnlCard.Controls.Add(this.btnGetVerification);
		this.pnlCard.Controls.Add(this.lblEmailVerification);
		this.pnlCard.Controls.Add(this.txtEmailValidate);
		this.pnlCard.Controls.Add(this.lblUserName);
		this.pnlCard.Controls.Add(this.txtUserName);
		this.pnlCard.Controls.Add(this.c1Label1);
		this.pnlCard.Controls.Add(this.txtNewPassword);
		this.pnlCard.Controls.Add(this.btnFindPwd);
		this.pnlCard.Location = new System.Drawing.Point(98, 72);
		this.pnlCard.Name = "pnlCard";
		this.pnlCard.Size = new System.Drawing.Size(585, 689);
		this.pnlCard.TabIndex = 0;
		// lblFindPwd
		this.lblFindPwd.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.lblFindPwd.AutoSize = true;
		this.lblFindPwd.BackColor = System.Drawing.Color.Transparent;
		this.lblFindPwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblFindPwd.Font = new System.Drawing.Font("Noto Sans SC", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblFindPwd.ForeColor = System.Drawing.Color.Black;
		this.lblFindPwd.Location = new System.Drawing.Point(215, 46);
		this.lblFindPwd.Name = "lblFindPwd";
		this.lblFindPwd.Size = new System.Drawing.Size(172, 49);
		this.lblFindPwd.TabIndex = 76;
		this.lblFindPwd.Tag = null;
		this.lblFindPwd.Text = "重置密码";
		this.lblFindPwd.TextDetached = true;
		// lblEmail
		this.lblEmail.AutoSize = true;
		this.lblEmail.BackColor = System.Drawing.Color.Transparent;
		this.lblEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEmail.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEmail.ForeColor = System.Drawing.Color.Black;
		this.lblEmail.Location = new System.Drawing.Point(39, 130);
		this.lblEmail.Name = "lblEmail";
		this.lblEmail.Size = new System.Drawing.Size(86, 34);
		this.lblEmail.TabIndex = 71;
		this.lblEmail.Tag = null;
		this.lblEmail.Text = "邮箱";
		this.lblEmail.TextDetached = true;
		// txtEmail
		this.txtEmail.AutoSize = false;
		this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtEmail.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtEmail.Location = new System.Drawing.Point(156, 117);
		this.txtEmail.Name = "txtEmail";
		this.txtEmail.Size = new System.Drawing.Size(385, 62);
		this.txtEmail.TabIndex = 1;
		this.txtEmail.Tag = null;
		this.txtEmail.TextDetached = true;
		this.txtEmail.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		// btnGetVerification
		this.btnGetVerification.BackColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.btnGetVerification.FlatAppearance.BorderSize = 0;
		this.btnGetVerification.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnGetVerification.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnGetVerification.ForeColor = System.Drawing.Color.White;
		this.btnGetVerification.Format = "(0s)";
		this.btnGetVerification.Location = new System.Drawing.Point(156, 208);
		this.btnGetVerification.Name = "btnGetVerification";
		this.btnGetVerification.Size = new System.Drawing.Size(156, 62);
		this.btnGetVerification.TabIndex = 2;
		this.btnGetVerification.Text = "获取验证码";
		this.btnGetVerification.UseVisualStyleBackColor = false;
		this.btnGetVerification.Click += new System.EventHandler(btnGetVerification_Click);
		// lblEmailVerification
		this.lblEmailVerification.AutoSize = true;
		this.lblEmailVerification.BackColor = System.Drawing.Color.Transparent;
		this.lblEmailVerification.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEmailVerification.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEmailVerification.ForeColor = System.Drawing.Color.Black;
		this.lblEmailVerification.Location = new System.Drawing.Point(39, 299);
		this.lblEmailVerification.Name = "lblEmailVerification";
		this.lblEmailVerification.Size = new System.Drawing.Size(133, 34);
		this.lblEmailVerification.TabIndex = 82;
		this.lblEmailVerification.Tag = null;
		this.lblEmailVerification.Text = "邮箱验证码";
		this.lblEmailVerification.TextDetached = true;
		// txtEmailValidate
		this.txtEmailValidate.AutoSize = false;
		this.txtEmailValidate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtEmailValidate.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtEmailValidate.Location = new System.Drawing.Point(156, 286);
		this.txtEmailValidate.Name = "txtEmailValidate";
		this.txtEmailValidate.Size = new System.Drawing.Size(385, 62);
		this.txtEmailValidate.TabIndex = 3;
		this.txtEmailValidate.Tag = null;
		this.txtEmailValidate.TextDetached = true;
		this.txtEmailValidate.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		// lblUserName
		this.lblUserName.AutoSize = true;
		this.lblUserName.BackColor = System.Drawing.Color.Transparent;
		this.lblUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblUserName.ForeColor = System.Drawing.Color.Black;
		this.lblUserName.Location = new System.Drawing.Point(39, 390);
		this.lblUserName.Name = "lblUserName";
		this.lblUserName.Size = new System.Drawing.Size(86, 34);
		this.lblUserName.TabIndex = 74;
		this.lblUserName.Tag = null;
		this.lblUserName.Text = "用户名";
		this.lblUserName.TextDetached = true;
		// txtUserName
		this.txtUserName.AutoSize = false;
		this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtUserName.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtUserName.Location = new System.Drawing.Point(156, 377);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.ReadOnly = true;
		this.txtUserName.Size = new System.Drawing.Size(385, 62);
		this.txtUserName.TabIndex = 0;
		this.txtUserName.Tag = null;
		this.txtUserName.TextDetached = true;
		this.txtUserName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		// c1Label1
		this.c1Label1.AutoSize = true;
		this.c1Label1.BackColor = System.Drawing.Color.Transparent;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.ForeColor = System.Drawing.Color.Black;
		this.c1Label1.Location = new System.Drawing.Point(39, 481);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(86, 34);
		this.c1Label1.TabIndex = 83;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "新密码";
		this.c1Label1.TextDetached = true;
		// txtNewPassword
		this.txtNewPassword.AutoSize = false;
		this.txtNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNewPassword.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtNewPassword.Location = new System.Drawing.Point(156, 468);
		this.txtNewPassword.Name = "txtNewPassword";
		this.txtNewPassword.Size = new System.Drawing.Size(385, 62);
		this.txtNewPassword.TabIndex = 4;
		this.txtNewPassword.Tag = null;
		this.txtNewPassword.TextDetached = true;
		this.txtNewPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		// btnFindPwd
		this.btnFindPwd.BackColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.btnFindPwd.FlatAppearance.BorderSize = 0;
		this.btnFindPwd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnFindPwd.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnFindPwd.ForeColor = System.Drawing.Color.White;
		this.btnFindPwd.Location = new System.Drawing.Point(182, 585);
		this.btnFindPwd.Name = "btnFindPwd";
		this.btnFindPwd.Size = new System.Drawing.Size(234, 66);
		this.btnFindPwd.TabIndex = 5;
		this.btnFindPwd.Text = "确定";
		this.btnFindPwd.UseVisualStyleBackColor = false;
		this.btnFindPwd.Click += new System.EventHandler(btnFindPwd_Click);
		// btnClose
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Image = Auditai.UI.Platform.Properties.Resources.close2;
		this.btnClose.Location = new System.Drawing.Point(666, 16);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(49, 49);
		this.btnClose.TabIndex = 84;
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		base.AcceptButton = this.btnFindPwd;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.ClientSize = new System.Drawing.Size(780, 832);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.pnlCard);
		this.Font = new System.Drawing.Font("Noto Sans SC", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
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
		((System.ComponentModel.ISupportInitialize)this.txtEmailValidate).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblFindPwd).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnFindPwd).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtNewPassword).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnGetVerification).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		this.pnlCard.ResumeLayout(false);
		this.pnlCard.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
