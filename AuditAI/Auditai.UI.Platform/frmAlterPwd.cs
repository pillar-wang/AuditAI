using System;
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
	private readonly Color _auditaiBlue = Color.FromArgb(0, 195, 245);

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

	public frmAlterPwd()
	{
		InitializeComponent();
		Initialize();
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
					tb.BorderColor = Color.LightGray;
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
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
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
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
		this.txtPassword.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPassword.Location = new System.Drawing.Point(97, 20);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '●';
		this.txtPassword.Size = new System.Drawing.Size(230, 32);
		this.txtPassword.TabIndex = 0;
		this.txtPassword.Tag = null;
		this.txtPassword.TextDetached = true;
		this.txtPassword.Value = "";
		this.txtPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtNewPassword.AutoSize = false;
		this.txtNewPassword.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNewPassword.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtNewPassword.Location = new System.Drawing.Point(97, 174);
		this.txtNewPassword.Name = "txtNewPassword";
		this.txtNewPassword.PasswordChar = '●';
		this.txtNewPassword.Size = new System.Drawing.Size(230, 32);
		this.txtNewPassword.TabIndex = 1;
		this.txtNewPassword.Tag = null;
		this.txtNewPassword.TextDetached = true;
		this.txtNewPassword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtNewPassword.Enter += new System.EventHandler(txtPassword_Enter);
		this.txtNewPassword.Validated += new System.EventHandler(txtNewPassword_Validated);
		this.txtNewPassword2.AutoSize = false;
		this.txtNewPassword2.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtNewPassword2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNewPassword2.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtNewPassword2.Location = new System.Drawing.Point(97, 234);
		this.txtNewPassword2.Name = "txtNewPassword2";
		this.txtNewPassword2.PasswordChar = '●';
		this.txtNewPassword2.Size = new System.Drawing.Size(230, 32);
		this.txtNewPassword2.TabIndex = 2;
		this.txtNewPassword2.Tag = null;
		this.txtNewPassword2.TextDetached = true;
		this.txtNewPassword2.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtNewPassword2.Enter += new System.EventHandler(txtPassword2_Enter);
		this.txtNewPassword2.Validated += new System.EventHandler(txtNewPassword2_Validated);
		this.txtVerification.AutoSize = false;
		this.txtVerification.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtVerification.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtVerification.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtVerification.Location = new System.Drawing.Point(97, 120);
		this.txtVerification.Name = "txtVerification";
		this.txtVerification.Size = new System.Drawing.Size(148, 32);
		this.txtVerification.TabIndex = 3;
		this.txtVerification.Tag = null;
		this.txtVerification.TextDetached = true;
		this.txtVerification.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.btnCertain.FlatAppearance.BorderSize = 0;
		this.btnCertain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCertain.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCertain.Location = new System.Drawing.Point(62, 315);
		this.btnCertain.Name = "btnCertain";
		this.btnCertain.Size = new System.Drawing.Size(100, 33);
		this.btnCertain.TabIndex = 5;
		this.btnCertain.Text = "确定";
		this.btnCertain.UseVisualStyleBackColor = true;
		this.btnCertain.Click += new System.EventHandler(btnCertain_Click);
		this.warnNewPassword.AutoSize = true;
		this.warnNewPassword.BackColor = System.Drawing.Color.Transparent;
		this.warnNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnNewPassword.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnNewPassword.ForeColor = System.Drawing.Color.Black;
		this.warnNewPassword.Location = new System.Drawing.Point(102, 210);
		this.warnNewPassword.Name = "warnNewPassword";
		this.warnNewPassword.Size = new System.Drawing.Size(166, 17);
		this.warnNewPassword.TabIndex = 14;
		this.warnNewPassword.Tag = null;
		this.warnNewPassword.Text = "长度在6-20个字符区分大小写";
		this.warnNewPassword.TextDetached = true;
		this.warnNewPassword.Visible = false;
		this.warnNewPassword2.AutoSize = true;
		this.warnNewPassword2.BackColor = System.Drawing.Color.Transparent;
		this.warnNewPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.warnNewPassword2.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.warnNewPassword2.ForeColor = System.Drawing.Color.Black;
		this.warnNewPassword2.Location = new System.Drawing.Point(102, 270);
		this.warnNewPassword2.Name = "warnNewPassword2";
		this.warnNewPassword2.Size = new System.Drawing.Size(116, 17);
		this.warnNewPassword2.TabIndex = 15;
		this.warnNewPassword2.Tag = null;
		this.warnNewPassword2.Text = "请再输入一次密码！";
		this.warnNewPassword2.TextDetached = true;
		this.warnNewPassword2.Visible = false;
		this.lblPassword.AutoSize = true;
		this.lblPassword.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword.ForeColor = System.Drawing.Color.Black;
		this.lblPassword.Location = new System.Drawing.Point(35, 26);
		this.lblPassword.Name = "lblPassword";
		this.lblPassword.Size = new System.Drawing.Size(56, 17);
		this.lblPassword.TabIndex = 18;
		this.lblPassword.Tag = null;
		this.lblPassword.Text = "登录密码";
		this.lblPassword.TextDetached = true;
		this.lblPassword1.AutoSize = true;
		this.lblPassword1.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword1.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword1.ForeColor = System.Drawing.Color.Black;
		this.lblPassword1.Location = new System.Drawing.Point(24, 241);
		this.lblPassword1.Name = "lblPassword1";
		this.lblPassword1.Size = new System.Drawing.Size(68, 17);
		this.lblPassword1.TabIndex = 19;
		this.lblPassword1.Tag = null;
		this.lblPassword1.Text = "确认新密码";
		this.lblPassword1.TextDetached = true;
		this.lblVerification.AutoSize = true;
		this.lblVerification.BackColor = System.Drawing.Color.Transparent;
		this.lblVerification.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVerification.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblVerification.ForeColor = System.Drawing.Color.Black;
		this.lblVerification.Location = new System.Drawing.Point(23, 126);
		this.lblVerification.Name = "lblVerification";
		this.lblVerification.Size = new System.Drawing.Size(68, 17);
		this.lblVerification.TabIndex = 25;
		this.lblVerification.Tag = null;
		this.lblVerification.Text = "短信验证码";
		this.lblVerification.TextDetached = true;
		this.lblPassword2.AutoSize = true;
		this.lblPassword2.BackColor = System.Drawing.Color.Transparent;
		this.lblPassword2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPassword2.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPassword2.ForeColor = System.Drawing.Color.Black;
		this.lblPassword2.Location = new System.Drawing.Point(47, 181);
		this.lblPassword2.Name = "lblPassword2";
		this.lblPassword2.Size = new System.Drawing.Size(44, 17);
		this.lblPassword2.TabIndex = 27;
		this.lblPassword2.Tag = null;
		this.lblPassword2.Text = "新密码";
		this.lblPassword2.TextDetached = true;
		this.lblMustInputStar1.AutoSize = true;
		this.lblMustInputStar1.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar1.ForeColor = System.Drawing.Color.Black;
		this.lblMustInputStar1.Location = new System.Drawing.Point(24, 28);
		this.lblMustInputStar1.Name = "lblMustInputStar1";
		this.lblMustInputStar1.Size = new System.Drawing.Size(11, 12);
		this.lblMustInputStar1.TabIndex = 34;
		this.lblMustInputStar1.Tag = null;
		this.lblMustInputStar1.Text = "*";
		this.lblMustInputStar1.TextDetached = true;
		this.lblMustInputStar2.AutoSize = true;
		this.lblMustInputStar2.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar2.ForeColor = System.Drawing.Color.Black;
		this.lblMustInputStar2.Location = new System.Drawing.Point(36, 183);
		this.lblMustInputStar2.Name = "lblMustInputStar2";
		this.lblMustInputStar2.Size = new System.Drawing.Size(11, 12);
		this.lblMustInputStar2.TabIndex = 35;
		this.lblMustInputStar2.Tag = null;
		this.lblMustInputStar2.Text = "*";
		this.lblMustInputStar2.TextDetached = true;
		this.lblMustInputStar3.AutoSize = true;
		this.lblMustInputStar3.BackColor = System.Drawing.Color.Transparent;
		this.lblMustInputStar3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMustInputStar3.ForeColor = System.Drawing.Color.Black;
		this.lblMustInputStar3.Location = new System.Drawing.Point(12, 244);
		this.lblMustInputStar3.Name = "lblMustInputStar3";
		this.lblMustInputStar3.Size = new System.Drawing.Size(11, 12);
		this.lblMustInputStar3.TabIndex = 36;
		this.lblMustInputStar3.Tag = null;
		this.lblMustInputStar3.Text = "*";
		this.lblMustInputStar3.TextDetached = true;
		this.btnCancel.FlatAppearance.BorderSize = 0;
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(202, 315);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(100, 33);
		this.btnCancel.TabIndex = 6;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnGetValidateCode.FlatAppearance.BorderSize = 0;
		this.btnGetValidateCode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnGetValidateCode.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnGetValidateCode.Format = "(0s)";
		this.btnGetValidateCode.Location = new System.Drawing.Point(251, 120);
		this.btnGetValidateCode.Name = "btnGetValidateCode";
		this.btnGetValidateCode.Size = new System.Drawing.Size(76, 32);
		this.btnGetValidateCode.TabIndex = 4;
		this.btnGetValidateCode.Text = "获取验证码";
		this.btnGetValidateCode.UseVisualStyleBackColor = true;
		this.btnGetValidateCode.Click += new System.EventHandler(btnGetValidateCode_Click);
		this.lblPhone.AutoSize = true;
		this.lblPhone.BackColor = System.Drawing.Color.Transparent;
		this.lblPhone.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPhone.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPhone.ForeColor = System.Drawing.Color.Black;
		this.lblPhone.Location = new System.Drawing.Point(34, 77);
		this.lblPhone.Name = "lblPhone";
		this.lblPhone.Size = new System.Drawing.Size(56, 17);
		this.lblPhone.TabIndex = 37;
		this.lblPhone.Tag = null;
		this.lblPhone.Text = "预留手机";
		this.lblPhone.TextDetached = true;
		this.txtPhone.AutoSize = false;
		this.txtPhone.BackColor = System.Drawing.Color.FromArgb(239, 239, 239);
		this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPhone.Enabled = false;
		this.txtPhone.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtPhone.Location = new System.Drawing.Point(97, 71);
		this.txtPhone.Name = "txtPhone";
		this.txtPhone.ReadOnly = true;
		this.txtPhone.Size = new System.Drawing.Size(230, 32);
		this.txtPhone.TabIndex = 38;
		this.txtPhone.Tag = null;
		this.txtPhone.TextDetached = true;
		this.txtPhone.Value = "";
		this.txtPhone.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		base.AcceptButton = this.btnCertain;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		base.ClientSize = new System.Drawing.Size(370, 369);
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
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
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
