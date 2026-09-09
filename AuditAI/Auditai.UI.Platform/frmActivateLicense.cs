﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

public class frmActivateLicense : Form
{
	private Label lblMachineCodeTitle;

	private TextBox txtMachineCode;

	private Label lblLicenseKeyTitle;

	private TextBox txtLicenseKey;

	private Button btnActivate;

	private Button btnCancel;

	private Label lblTip;

	private IContainer components;

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

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	// 统一样式调整：无边框窗体的标题栏拖拽（参考 frmFindPwd_MouseDown）
	private void frmActivateLicense_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(Handle, 274, 61458, 0);
	}

	// 统一样式调整：浅蓝渐变背景 + 顶部 3px 主色光带（参考 frmFindPwd_Paint）
	private void frmActivateLicense_Paint(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		Rectangle bgRect = new Rectangle(0, 0, base.Width, base.Height);
		// 垂直渐变：浅天蓝 → 浅灰白
		using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
			bgRect, Color.FromArgb(239, 246, 255), Color.FromArgb(245, 248, 250),
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

	public frmActivateLicense()
	{
		InitializeComponent();
		// 统一样式调整：无边框窗体（配合圆角 Region + 渐变背景 + MouseDown 拖拽）
		FormBorderStyle = FormBorderStyle.None;
		MaximizeBox = false;
		MinimizeBox = false;
		txtMachineCode.Text = MachineCode.Code;
		// 统一样式调整：绑定当前主题树（参考 frmAlterPwd.ShowDialog 的用法）
		try
		{
			Theme.SetCurrentTree(this);
		}
		catch
		{
		}
		// 统一样式调整：窗体 12px 圆角 + 按钮 8px 圆角，须在布局/尺寸确定后应用
		ApplyRoundedRegion(AuditTheme.CardRadius);
		ApplyRoundedButton(btnActivate, AuditTheme.ButtonRadius);
		ApplyRoundedButton(btnCancel, AuditTheme.ButtonRadius);
		// 统一样式调整：Theme.SetCurrentTree 会覆盖按钮配色，须在其后重设主/次按钮样式
		RefreshButtonStyles();
	}

	/// <summary>统一样式调整：重新应用主/次按钮配色（Theme.SetCurrentTree 之后调用，防止被主题覆盖）</summary>
	private void RefreshButtonStyles()
	{
		// 主按钮：Brand 蓝底白字
		btnActivate.BackColor = AuditTheme.Brand;
		btnActivate.ForeColor = Color.White;
		btnActivate.FlatStyle = FlatStyle.Flat;
		btnActivate.FlatAppearance.BorderSize = 0;
		btnActivate.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		btnActivate.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		// 次按钮：白底灰字 + 浅蓝边框
		btnCancel.BackColor = Color.White;
		btnCancel.ForeColor = Color.FromArgb(30, 41, 59);
		btnCancel.FlatStyle = FlatStyle.Flat;
		btnCancel.FlatAppearance.BorderSize = 1;
		btnCancel.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.lblMachineCodeTitle = new System.Windows.Forms.Label();
		this.txtMachineCode = new System.Windows.Forms.TextBox();
		this.lblLicenseKeyTitle = new System.Windows.Forms.Label();
		this.txtLicenseKey = new System.Windows.Forms.TextBox();
		this.btnActivate = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.lblTip = new System.Windows.Forms.Label();
		base.SuspendLayout();
		// lblMachineCodeTitle
		this.lblMachineCodeTitle.AutoSize = true;
		// 统一样式调整：标签背景透明，避免渐变背景上出现灰色色块
		this.lblMachineCodeTitle.BackColor = System.Drawing.Color.Transparent;
		this.lblMachineCodeTitle.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblMachineCodeTitle.Location = new System.Drawing.Point(20, 20);
		this.lblMachineCodeTitle.Name = "lblMachineCodeTitle";
		this.lblMachineCodeTitle.Size = new System.Drawing.Size(73, 22);
		this.lblMachineCodeTitle.TabIndex = 0;
		this.lblMachineCodeTitle.Text = "机器码：";
		// txtMachineCode
		this.txtMachineCode.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
		this.txtMachineCode.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtMachineCode.Location = new System.Drawing.Point(104, 20);
		this.txtMachineCode.Name = "txtMachineCode";
		this.txtMachineCode.ReadOnly = true;
		this.txtMachineCode.Size = new System.Drawing.Size(254, 30);
		this.txtMachineCode.TabIndex = 1;
		// lblLicenseKeyTitle
		this.lblLicenseKeyTitle.AutoSize = true;
		// 统一样式调整：标签背景透明，避免渐变背景上出现灰色色块
		this.lblLicenseKeyTitle.BackColor = System.Drawing.Color.Transparent;
		this.lblLicenseKeyTitle.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblLicenseKeyTitle.Location = new System.Drawing.Point(20, 72);
		this.lblLicenseKeyTitle.Name = "lblLicenseKeyTitle";
		this.lblLicenseKeyTitle.Size = new System.Drawing.Size(73, 22);
		this.lblLicenseKeyTitle.TabIndex = 2;
		this.lblLicenseKeyTitle.Text = "激活码：";
		// txtLicenseKey
		this.txtLicenseKey.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtLicenseKey.Location = new System.Drawing.Point(104, 68);
		this.txtLicenseKey.Name = "txtLicenseKey";
		this.txtLicenseKey.Size = new System.Drawing.Size(254, 30);
		this.txtLicenseKey.TabIndex = 3;
		// lblTip
		this.lblTip.AutoSize = false;
		// 统一样式调整：标签背景透明，避免渐变背景上出现灰色色块
		this.lblTip.BackColor = System.Drawing.Color.Transparent;
		this.lblTip.Font = new System.Drawing.Font("微软雅黑", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblTip.ForeColor = System.Drawing.Color.Gray;
		this.lblTip.Location = new System.Drawing.Point(20, 111);
		this.lblTip.Name = "lblTip";
		this.lblTip.Size = new System.Drawing.Size(338, 42);
		this.lblTip.TabIndex = 4;
		this.lblTip.Text = "请联系管理员获取激活码，激活后需重新登录";
		this.lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// btnActivate
		// 统一样式调整：主按钮 = Brand 蓝底白字，补充悬停/按下态颜色
		this.btnActivate.BackColor = AuditTheme.Brand;
		this.btnActivate.FlatAppearance.BorderSize = 0;
		this.btnActivate.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		this.btnActivate.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		this.btnActivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnActivate.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnActivate.ForeColor = System.Drawing.Color.White;
		// 统一样式调整：主按钮高度统一 40
		this.btnActivate.Location = new System.Drawing.Point(104, 163);
		this.btnActivate.Name = "btnActivate";
		this.btnActivate.Size = new System.Drawing.Size(117, 40);
		this.btnActivate.TabIndex = 5;
		this.btnActivate.Text = "激活";
		this.btnActivate.UseVisualStyleBackColor = false;
		this.btnActivate.Click += new System.EventHandler(btnActivate_Click);
		// btnCancel
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		// 统一样式调整：次按钮 = 白底 + 深灰字 + 浅蓝边框
		this.btnCancel.BackColor = System.Drawing.Color.White;
		this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
		this.btnCancel.FlatAppearance.BorderSize = 1;
		this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(229, 231, 235);
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		// 统一样式调整：次按钮高度统一 40
		this.btnCancel.Location = new System.Drawing.Point(241, 163);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(117, 40);
		this.btnCancel.TabIndex = 6;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = false;
		// frmActivateLicense
		base.AcceptButton = this.btnActivate;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(390, 228);
		base.Controls.Add(this.lblTip);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnActivate);
		base.Controls.Add(this.txtLicenseKey);
		base.Controls.Add(this.lblLicenseKeyTitle);
		base.Controls.Add(this.txtMachineCode);
		base.Controls.Add(this.lblMachineCodeTitle);
		this.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		// 统一样式调整：无边框窗体（配合圆角 Region + 渐变背景 + MouseDown 拖拽）
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmActivateLicense";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "License 激活";
		// 统一样式调整：标题栏拖拽 + 渐变背景/顶部光带绘制
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(frmActivateLicense_MouseDown);
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmActivateLicense_Paint);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private async void btnActivate_Click(object sender, EventArgs e)
	{
		string licenseKey = txtLicenseKey.Text.Trim();
		if (string.IsNullOrEmpty(licenseKey))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请输入激活码");
			return;
		}
		string machineCode = MachineCode.Code;
		try
		{
			btnActivate.Enabled = false;
			// 调用 WebApiClient 激活 License
			var result = await WebApiClient.ActivateLicense(licenseKey, machineCode);
			if (result != null && result["activated"]?.Value<bool>() == true)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "激活成功，请重新登录");
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				string errorMsg = result?["message"]?.Value<string>() ?? "激活失败，请检查 LicenseKey";
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, errorMsg);
			}
			return;
		}
		catch (Exception ex)
		{
			ex.Log("激活 License 失败");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "激活失败：" + ex.Message);
			return;
		}
		finally
		{
			btnActivate.Enabled = true;
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
}
