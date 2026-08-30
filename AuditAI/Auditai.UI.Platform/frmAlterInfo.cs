﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using Auditai.SignalR;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class frmAlterInfo : C1RibbonForm
{
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

	private bool _hadRequestSMS;

	private string _previousPhone = string.Empty;

	private string _previousUsername = string.Empty;

	private Auditai.DTO.User user;

	private bool _headChanged;

	private IContainer components;

	private C1TextBoxEx txtUserName;

	private C1TextBoxEx txtName;

	private C1TextBoxEx txtEmail;

	private C1TextBoxEx txtPhone;

	private C1TextBoxEx txtCity;

	private C1Button btnConfirm;

	private RadioButton radMale;

	private RadioButton radFemale;

	private C1Label lblUserName;

	private C1Label lblName;

	private C1Label lblEmail;

	private C1Label lblSex;

	private C1Label lblCity;

	private C1Label lblPhone;

	private C1Label lblMustInputStar1;

	private C1CommandHolder c1CommandHolder1;

	private TimerButton btnSMS;

	private C1TextBoxEx txtSMS;

	private C1Label lblSMS;

	private C1PictureBox pictureHead;

	private C1Label c1Label1;

	private C1Button btnCancel;

	private C1Label c1Label2;

	public bool UserNameChanged { get; set; }

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

	private void frmAlterInfo_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

	private void frmAlterInfo_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			AnimateWindow(base.Handle, 100, 851968);
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
		btnConfirm.BackColor = AuditTheme.Brand;
		btnConfirm.ForeColor = Color.White;
		btnConfirm.FlatStyle = FlatStyle.Flat;
		btnConfirm.FlatAppearance.BorderSize = 0;
		btnConfirm.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		btnConfirm.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		// 次按钮：白底灰字 + 浅蓝边框
		btnCancel.BackColor = Color.White;
		btnCancel.ForeColor = Color.FromArgb(30, 41, 59);
		btnCancel.FlatStyle = FlatStyle.Flat;
		btnCancel.FlatAppearance.BorderSize = 1;
		btnCancel.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
		btnSMS.BackColor = Color.White;
		btnSMS.ForeColor = Color.FromArgb(30, 41, 59);
		btnSMS.FlatStyle = FlatStyle.Flat;
		btnSMS.FlatAppearance.BorderSize = 1;
		btnSMS.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
	}

	/// <summary>窗体显示后重新应用圆角：C1RibbonForm 创建句柄时会重置 Region，构造时设置的圆角会失效</summary>
	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		ApplyRoundedRegion(AuditTheme.CardRadius);
	}

	public frmAlterInfo()
	{
		InitializeComponent();

		// 统一样式调整：窗体 12px 圆角 + 按钮 8px 圆角，须在布局/尺寸确定后应用
		ApplyRoundedRegion(AuditTheme.CardRadius);
		ApplyRoundedButton(btnConfirm, AuditTheme.ButtonRadius);
		ApplyRoundedButton(btnCancel, AuditTheme.ButtonRadius);
		ApplyRoundedButton(btnSMS, AuditTheme.ButtonRadius);

		base.Shown += FrmAlterInfo_Shown;
		Initialize();
		// 统一样式调整：C1Button 皮肤会覆盖设计时配色，构造末尾重设主/次按钮样式
		RefreshButtonStyles();
	}

	private void FrmAlterInfo_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.SwitchUser);
	}

	// 统一样式调整：浅蓝渐变背景 + 顶部 3px 主色光带（参考 frmFindPwd_Paint）
	private void frmAlterInfo_Paint(object sender, PaintEventArgs e)
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

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentTree(this);
		return base.ShowDialog();
	}

	public void FocusPhone()
	{
		txtPhone.Focus();
		base.ActiveControl = txtPhone;
	}

	private async void Initialize()
	{
		if (Program.IsOnPremise)
		{
			txtPhone.Enabled = false;
		}
		// 本地模式：手机号可直接修改，无需短信验证
		if (StorageRouter.IsLocalMode)
		{
			txtPhone.Enabled = true;
		}
		base.AcceptButton = btnConfirm;
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
					tb.BorderColor = AuditTheme.Brand;
				};
				tb.MouseLeave += delegate
				{
					tb.BorderColor = AuditTheme.BorderStrong;
				};
			}
		}
		try
		{
			if (StorageRouter.IsLocalMode)
			{
				user = await Auditai.LocalDataStore.LocalDataStore.GetUserById(Auditai.Model.User.Current.Id);
			}
			else
			{
				user = await WebApiClient.GetUserById(Auditai.Model.User.Current.Id);
			}
			txtUserName.Text = user.UserName;
			txtPhone.Text = user.Phone;
			txtEmail.Text = user.Email;
			txtName.Text = user.Name;
			txtCity.Text = user.City;
			radFemale.Checked = user.Sex?.ToLower() == "f";
			radMale.Checked = !radFemale.Checked;
			SetPicture(user.Picture, radMale.Checked);
			ShowButtonSMS(visible: false);
			_previousPhone = user.Phone ?? string.Empty;
			_previousUsername = user.UserName ?? string.Empty;
		}
		catch (NormalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (ServerException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (TimeoutException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
		}
		catch (HttpRequestException ex4)
		{
			Exception innerException = ex4.InnerException;
			string text = ((innerException == null) ? ex4.Message : innerException.Message);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
		}
		radMale.CheckedChanged += RadMale_CheckedChanged;
		radFemale.CheckedChanged += RadFemale_CheckedChanged;
	}

	private void RadMale_CheckedChanged(object sender, EventArgs e)
	{
		if (user != null && user.Picture == null && radMale.Checked)
		{
			pictureHead.Image = Resources.Boy.ToSize(32, 32);
		}
	}

	private void RadFemale_CheckedChanged(object sender, EventArgs e)
	{
		if (user != null && user.Picture == null && radFemale.Checked)
		{
			pictureHead.Image = Resources.Girl.ToSize(32, 32);
		}
	}

	private void ShowButtonSMS(bool visible)
	{
		lblSMS.Visible = visible;
		txtSMS.Visible = visible;
		btnSMS.Visible = visible;
	}

	private byte[] GetPicture()
	{
		try
		{
			object obj = pictureHead.Image.Clone();
			Bitmap bitmap = ((System.Drawing.Image)obj).ToSize(100, 100);
			using MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, ImageFormat.Png);
			return memoryStream.GetBuffer();
		}
		catch (Exception)
		{
			return null;
		}
	}

	private void SetPicture(byte[] bytes, bool sex)
	{
		System.Drawing.Image image = null;
		try
		{
			using MemoryStream stream = new MemoryStream(bytes);
			image = System.Drawing.Image.FromStream(stream);
		}
		catch (Exception)
		{
		}
		if (image == null)
		{
			image = (sex ? Resources.Boy : Resources.Girl);
		}
		pictureHead.Image = (image as Bitmap)?.ToSize(32, 32);
	}

	private async void btnConfirm_Click(object sender, EventArgs e)
	{
		// 修复 BUG: 此前缺少防重复点击机制，用户在 await WebApiClient.UpdateUserInfo /
		// UpdatePhoneInfo / SignalRClient.ChangeMemberInfo 期间可重复点击，
		// 导致多次发送更新请求和 SignalR 通知。现在禁用按钮直到异步操作完成。
		btnConfirm.Enabled = false;
		try
		{
			if (!Regex.IsMatch(txtUserName.Text.Trim(), "^.{2,20}$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名不为空,且必须为2-20个字符不区分大小写");
				return;
			}
			if (!Regex.IsMatch(txtName.Text.Trim(), "^.{2,20}$"))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "姓名不为空,且必须为2-20个字符不区分大小写");
				return;
			}
			Auditai.DTO.User user = new Auditai.DTO.User
			{
				Id = Auditai.Model.User.Current.Id,
				UserName = txtUserName.Text.Trim(),
				Phone = txtPhone.Text.Trim(),
				Email = txtEmail.Text.Trim(),
				Name = txtName.Text.Trim(),
				City = txtCity.Text.Trim(),
				Sex = (radMale.Checked ? "m" : (radFemale.Checked ? "f" : "m")),
				Picture = GetPicture()
			};
			if (StorageRouter.IsLocalMode)
			{
				// 本地模式：直接保存，无需短信验证
				await Auditai.LocalDataStore.LocalDataStore.UpdateUserInfo(user);
			}
			else if (txtPhone.Text.Trim().Equals(_previousPhone))
			{
				await WebApiClient.UpdateUserInfo(user);
			}
			else
			{
				if (!_hadRequestSMS)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改手机号必须先请求短信验证码！");
					return;
				}
				if (!Regex.IsMatch(txtSMS.Text.Trim(), "^\\w{1,10}$"))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "验证码格式不正确！");
					return;
				}
				await WebApiClient.UpdatePhoneInfo(user, txtSMS.Text.Trim());
			}
			Auditai.Model.User.Current.Name = user.Name;
			Auditai.Model.User.Current.UserName = user.UserName;
			MemberManager instance = MemberManager.GetInstance();
			Member member = instance.GetMember(user.Id.ToString());
			if (member != null)
			{
				member.Name = user.Name;
			}
			await SignalRClient.ChangeMemberInfo(Auditai.Model.User.Current.Id.ToString());
			UserNameChanged = !txtUserName.Text.Trim().Equals(_previousUsername);
			base.DialogResult = DialogResult.OK;
			if (_headChanged)
			{
				ChatManager.OnAfterHeadChanged(pictureHead.Image as Bitmap);
			}
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
		catch (TimeoutException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
		}
		catch (HttpRequestException ex4)
		{
			Exception innerException = ex4.InnerException;
			string text = ((innerException == null) ? ex4.Message : innerException.Message);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
		}
		finally
		{
			btnConfirm.Enabled = true;
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private async void btnGetSMS_Click(object sender, EventArgs e)
	{
		string phoneText = txtPhone.Text.Trim();
		if (!Regex.IsMatch(phoneText, "^[0-9]{11,11}$"))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "手机号格式不正确！请检查是否为11位数字");
			return;
		}
		try
		{
			if (await WebApiClient.PhoneExists(phoneText))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "手机号已注册！一个手机号只能绑定一个账号");
				return;
			}
			btnSMS.Format = "(0s)";
			btnSMS.Start(120);
			await WebApiClient.GetValidateCode(phoneText, "4");
			_hadRequestSMS = true;
		}
		catch (NormalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			btnSMS.Reset();
		}
		catch (ServerException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			btnSMS.Reset();
		}
		catch (TimeoutException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
			btnSMS.Reset();
		}
		catch (HttpRequestException ex4)
		{
			Exception innerException = ex4.InnerException;
			string text = ((innerException == null) ? ex4.Message : innerException.Message);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
			btnSMS.Reset();
		}
	}

	private void txtPhone_TextChanged(object sender, EventArgs e)
	{
		// 本地模式：无需短信验证码
		if (StorageRouter.IsLocalMode)
		{
			ShowButtonSMS(visible: false);
			return;
		}
		if (txtPhone.Text.Trim().Equals(_previousPhone))
		{
			ShowButtonSMS(visible: false);
		}
		else
		{
			ShowButtonSMS(visible: true);
		}
	}

	private void pictureHead_Click(object sender, EventArgs e)
	{
		using (OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = "图片文件|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tiff"
		})
		{
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					object obj = System.Drawing.Image.FromFile(openFileDialog.FileName).Clone();
					Bitmap image = ((System.Drawing.Image)obj).ToSize(32, 32);
					pictureHead.Image = image;
					_headChanged = true;
				}
				catch (OutOfMemoryException)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未能打开此图片，请重试或者更改图片。");
				}
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
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.radMale = new System.Windows.Forms.RadioButton();
		this.radFemale = new System.Windows.Forms.RadioButton();
		this.lblUserName = new C1.Win.C1Input.C1Label();
		this.lblName = new C1.Win.C1Input.C1Label();
		this.lblEmail = new C1.Win.C1Input.C1Label();
		this.lblSex = new C1.Win.C1Input.C1Label();
		this.lblCity = new C1.Win.C1Input.C1Label();
		this.lblPhone = new C1.Win.C1Input.C1Label();
		this.lblMustInputStar1 = new C1.Win.C1Input.C1Label();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.lblSMS = new C1.Win.C1Input.C1Label();
		this.pictureHead = new C1.Win.C1Input.C1PictureBox();
		this.btnSMS = new Auditai.UI.Controls.TimerButton();
		this.txtSMS = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtCity = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtPhone = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtEmail = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtName = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtUserName = new Auditai.UI.Controls.C1TextBoxEx();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.c1Label2 = new C1.Win.C1Input.C1Label();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblSex).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCity).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblSMS).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHead).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnSMS).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtSMS).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtCity).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmail).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).BeginInit();
		base.SuspendLayout();
		// 统一样式调整：主按钮 = Brand 蓝底白字 + Flat 无边框
		this.btnConfirm.BackColor = AuditTheme.Brand;
		this.btnConfirm.ForeColor = System.Drawing.Color.White;
		this.btnConfirm.FlatAppearance.BorderSize = 0;
		this.btnConfirm.FlatAppearance.MouseDownBackColor = AuditTheme.BrandActive;
		this.btnConfirm.FlatAppearance.MouseOverBackColor = AuditTheme.BrandHover;
		this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnConfirm.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		// 统一样式调整：主按钮高度统一 40
		this.btnConfirm.Location = new System.Drawing.Point(70, 551);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(130, 40);
		this.btnConfirm.TabIndex = 7;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = false;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.radMale.AutoSize = true;
		this.radMale.BackColor = System.Drawing.Color.Transparent;
		this.radMale.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.radMale.Location = new System.Drawing.Point(131, 298);
		this.radMale.Name = "radMale";
		this.radMale.Size = new System.Drawing.Size(49, 27);
		this.radMale.TabIndex = 4;
		this.radMale.TabStop = true;
		this.radMale.Text = "男";
		this.radMale.UseVisualStyleBackColor = false;
		this.radFemale.AutoSize = true;
		this.radFemale.BackColor = System.Drawing.Color.Transparent;
		this.radFemale.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.radFemale.Location = new System.Drawing.Point(205, 298);
		this.radFemale.Name = "radFemale";
		this.radFemale.Size = new System.Drawing.Size(49, 27);
		this.radFemale.TabIndex = 4;
		this.radFemale.TabStop = true;
		this.radFemale.Text = "女";
		this.radFemale.UseVisualStyleBackColor = false;
		this.lblUserName.AutoSize = true;
		this.lblUserName.BackColor = System.Drawing.Color.Transparent;
		this.lblUserName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblUserName.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblUserName.ForeColor = System.Drawing.Color.Black;
		this.lblUserName.Location = new System.Drawing.Point(66, 114);
		this.lblUserName.Name = "lblUserName";
		this.lblUserName.Size = new System.Drawing.Size(44, 17);
		this.lblUserName.TabIndex = 18;
		this.lblUserName.Tag = null;
		this.lblUserName.Text = "用户名";
		this.lblUserName.TextDetached = true;
		this.lblName.AutoSize = true;
		this.lblName.BackColor = System.Drawing.Color.Transparent;
		this.lblName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblName.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblName.ForeColor = System.Drawing.Color.Black;
		this.lblName.Location = new System.Drawing.Point(82, 179);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(42, 22);
		this.lblName.TabIndex = 20;
		this.lblName.Tag = null;
		this.lblName.Text = "姓名";
		this.lblName.TextDetached = true;
		this.lblEmail.AutoSize = true;
		this.lblEmail.BackColor = System.Drawing.Color.Transparent;
		this.lblEmail.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEmail.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblEmail.ForeColor = System.Drawing.Color.Black;
		this.lblEmail.Location = new System.Drawing.Point(82, 250);
		this.lblEmail.Name = "lblEmail";
		this.lblEmail.Size = new System.Drawing.Size(42, 22);
		this.lblEmail.TabIndex = 21;
		this.lblEmail.Tag = null;
		this.lblEmail.Text = "邮箱";
		this.lblEmail.TextDetached = true;
		this.lblSex.AutoSize = true;
		this.lblSex.BackColor = System.Drawing.Color.Transparent;
		this.lblSex.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblSex.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSex.ForeColor = System.Drawing.Color.Black;
		this.lblSex.Location = new System.Drawing.Point(82, 302);
		this.lblSex.Name = "lblSex";
		this.lblSex.Size = new System.Drawing.Size(42, 22);
		this.lblSex.TabIndex = 23;
		this.lblSex.Tag = null;
		this.lblSex.Text = "性别";
		this.lblSex.TextDetached = true;
		this.lblCity.AutoSize = true;
		this.lblCity.BackColor = System.Drawing.Color.Transparent;
		this.lblCity.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCity.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblCity.ForeColor = System.Drawing.Color.Black;
		this.lblCity.Location = new System.Drawing.Point(51, 415);
		this.lblCity.Name = "lblCity";
		this.lblCity.Size = new System.Drawing.Size(73, 22);
		this.lblCity.TabIndex = 24;
		this.lblCity.Tag = null;
		this.lblCity.Text = "所在城市";
		this.lblCity.TextDetached = true;
		this.lblPhone.AutoSize = true;
		this.lblPhone.BackColor = System.Drawing.Color.Transparent;
		this.lblPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPhone.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPhone.ForeColor = System.Drawing.Color.Black;
		this.lblPhone.Location = new System.Drawing.Point(51, 344);
		this.lblPhone.Name = "lblPhone";
		this.lblPhone.Size = new System.Drawing.Size(57, 22);
		this.lblPhone.TabIndex = 28;
		this.lblPhone.Tag = null;
		this.lblPhone.Text = "手机号";
		this.lblPhone.TextDetached = true;
		this.lblMustInputStar1.AutoSize = true;
		this.lblMustInputStar1.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar1.ForeColor = System.Drawing.Color.Black;
		this.lblMustInputStar1.Location = new System.Drawing.Point(52, 103);
		this.lblMustInputStar1.Name = "lblMustInputStar1";
		this.lblMustInputStar1.Size = new System.Drawing.Size(14, 16);
		this.lblMustInputStar1.TabIndex = 42;
		this.lblMustInputStar1.Tag = null;
		this.lblMustInputStar1.Text = "*";
		this.lblMustInputStar1.TextDetached = true;
		this.c1CommandHolder1.Owner = this;
		this.lblSMS.AutoSize = true;
		this.lblSMS.BackColor = System.Drawing.Color.Transparent;
		this.lblSMS.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblSMS.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSMS.ForeColor = System.Drawing.Color.Black;
		this.lblSMS.Location = new System.Drawing.Point(32, 495);
		this.lblSMS.Name = "lblSMS";
		this.lblSMS.Size = new System.Drawing.Size(88, 22);
		this.lblSMS.TabIndex = 47;
		this.lblSMS.Tag = null;
		this.lblSMS.Text = "短信验证码";
		this.lblSMS.TextDetached = true;
		this.pictureHead.Cursor = System.Windows.Forms.Cursors.Hand;
		this.pictureHead.Location = new System.Drawing.Point(229, 16);
		this.pictureHead.Name = "pictureHead";
		this.pictureHead.Size = new System.Drawing.Size(42, 42);
		this.pictureHead.TabIndex = 50;
		this.pictureHead.TabStop = false;
		this.pictureHead.Click += new System.EventHandler(pictureHead_Click);
		// 统一样式调整：次按钮 = 白底 + 深灰字 + 浅蓝边框
		this.btnSMS.BackColor = System.Drawing.Color.White;
		this.btnSMS.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
		this.btnSMS.FlatAppearance.BorderSize = 1;
		this.btnSMS.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(208, 215, 222);
		this.btnSMS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSMS.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnSMS.Format = "(0s)";
		// 统一样式调整：次按钮高度统一 40
		this.btnSMS.Location = new System.Drawing.Point(330, 485);
		this.btnSMS.Name = "btnSMS";
		this.btnSMS.Size = new System.Drawing.Size(98, 40);
		this.btnSMS.TabIndex = 49;
		this.btnSMS.Text = "获取验证码";
		this.btnSMS.UseVisualStyleBackColor = false;
		this.btnSMS.Click += new System.EventHandler(btnGetSMS_Click);
		this.txtSMS.AutoSize = false;
		this.txtSMS.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtSMS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtSMS.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtSMS.Location = new System.Drawing.Point(129, 485);
		this.txtSMS.Name = "txtSMS";
		this.txtSMS.Size = new System.Drawing.Size(181, 42);
		this.txtSMS.TabIndex = 48;
		this.txtSMS.Tag = null;
		this.txtSMS.TextDetached = true;
		this.txtSMS.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtCity.AutoSize = false;
		this.txtCity.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtCity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCity.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtCity.Location = new System.Drawing.Point(129, 415);
		this.txtCity.Name = "txtCity";
		this.txtCity.Size = new System.Drawing.Size(299, 42);
		this.txtCity.TabIndex = 6;
		this.txtCity.Tag = null;
		this.txtCity.TextDetached = true;
		this.txtCity.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPhone.AutoSize = false;
		this.txtPhone.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPhone.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhone.Location = new System.Drawing.Point(129, 344);
		this.txtPhone.Name = "txtPhone";
		this.txtPhone.Size = new System.Drawing.Size(299, 42);
		this.txtPhone.TabIndex = 5;
		this.txtPhone.Tag = null;
		this.txtPhone.TextDetached = true;
		this.txtPhone.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtPhone.TextChanged += new System.EventHandler(txtPhone_TextChanged);
		this.txtEmail.AutoSize = false;
		this.txtEmail.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtEmail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtEmail.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtEmail.Location = new System.Drawing.Point(129, 239);
		this.txtEmail.Name = "txtEmail";
		this.txtEmail.Size = new System.Drawing.Size(299, 42);
		this.txtEmail.TabIndex = 3;
		this.txtEmail.Tag = null;
		this.txtEmail.TextDetached = true;
		this.txtEmail.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtName.AutoSize = false;
		this.txtName.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtName.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtName.Location = new System.Drawing.Point(129, 170);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(299, 42);
		this.txtName.TabIndex = 1;
		this.txtName.Tag = null;
		this.txtName.TextDetached = true;
		this.txtName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtUserName.AutoSize = false;
		this.txtUserName.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtUserName.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtUserName.Location = new System.Drawing.Point(129, 103);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.Size = new System.Drawing.Size(299, 42);
		this.txtUserName.TabIndex = 0;
		this.txtUserName.Tag = null;
		this.txtUserName.TextDetached = true;
		this.txtUserName.Value = "";
		this.txtUserName.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.c1Label1.AutoSize = true;
		this.c1Label1.BackColor = System.Drawing.Color.Transparent;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.ForeColor = System.Drawing.Color.Black;
		this.c1Label1.Location = new System.Drawing.Point(198, 61);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(104, 22);
		this.c1Label1.TabIndex = 51;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "点击修改头像";
		this.c1Label1.TextDetached = true;
		// 统一样式调整：次按钮 = 白底 + 深灰字 + 浅蓝边框
		this.btnCancel.BackColor = System.Drawing.Color.White;
		this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
		this.btnCancel.FlatAppearance.BorderSize = 1;
		this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(208, 215, 222);
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCancel.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		// 统一样式调整：次按钮高度统一 40
		this.btnCancel.Location = new System.Drawing.Point(320, 551);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(130, 40);
		this.btnCancel.TabIndex = 8;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = false;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.c1Label2.AutoSize = true;
		this.c1Label2.BackColor = System.Drawing.Color.Transparent;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.ForeColor = System.Drawing.Color.Black;
		this.c1Label2.Location = new System.Drawing.Point(66, 183);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(14, 16);
		this.c1Label2.TabIndex = 52;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "*";
		this.c1Label2.TextDetached = true;
		base.AcceptButton = this.btnConfirm;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.ClientSize = new System.Drawing.Size(533, 614);
		base.Controls.Add(this.c1Label2);
		base.Controls.Add(this.c1Label1);
		base.Controls.Add(this.pictureHead);
		base.Controls.Add(this.btnSMS);
		base.Controls.Add(this.txtSMS);
		base.Controls.Add(this.lblSMS);
		base.Controls.Add(this.lblMustInputStar1);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.lblPhone);
		base.Controls.Add(this.lblCity);
		base.Controls.Add(this.lblSex);
		base.Controls.Add(this.lblEmail);
		base.Controls.Add(this.lblName);
		base.Controls.Add(this.lblUserName);
		base.Controls.Add(this.radFemale);
		base.Controls.Add(this.radMale);
		base.Controls.Add(this.btnConfirm);
		base.Controls.Add(this.txtCity);
		base.Controls.Add(this.txtPhone);
		base.Controls.Add(this.txtEmail);
		base.Controls.Add(this.txtName);
		base.Controls.Add(this.txtUserName);
		this.DoubleBuffered = true;
		// 统一样式调整：无边框窗体（配合圆角 Region + 渐变背景 + MouseDown 拖拽）
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmAlterInfo";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "用户资料";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmAlterInfo_FormClosing);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(frmAlterInfo_MouseDown);
		// 统一样式调整：渐变背景 + 顶部光带绘制
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmAlterInfo_Paint);
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblEmail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblSex).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCity).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMustInputStar1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblSMS).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureHead).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnSMS).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtSMS).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtCity).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtPhone).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtEmail).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtUserName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
