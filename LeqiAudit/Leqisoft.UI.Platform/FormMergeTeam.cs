using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class FormMergeTeam
{
	private readonly C1RibbonForm _form;

	private readonly C1TextBox _txbUsername;

	private readonly C1TextBox _txbPassword;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	public string Username => _txbUsername.Text;

	public string Password => _txbPassword.Text;

	public FormMergeTeam()
	{
		_form = FormFactory.Create();
		_form.Size = new Size(400, 300);
		_form.FormBorderStyle = FormBorderStyle.FixedDialog;
		_form.MinimizeBox = false;
		_form.MaximizeBox = false;
		_form.Text = "合并组织";
		_form.Controls.Add(new C1Label
		{
			TextDetached = true,
			Text = "请输入被合并组织的系统管理员用户名及登录密码：",
			AutoSize = true,
			Location = new Point(60, 30)
		});
		_form.Controls.Add(new C1Label
		{
			TextDetached = true,
			Text = "被合并组织的系统管理员用户名：",
			AutoSize = true,
			Location = new Point(60, 80)
		});
		_txbUsername = new C1TextBox
		{
			TextDetached = true,
			Location = new Point(60, 110),
			Width = 200
		};
		_form.Controls.Add(_txbUsername);
		_form.Controls.Add(new C1Label
		{
			TextDetached = true,
			Text = "被合并组织的系统管理员密码：",
			AutoSize = true,
			Location = new Point(60, 150)
		});
		_txbPassword = new C1TextBox
		{
			Location = new Point(60, 180),
			PasswordChar = '●',
			Width = 200
		};
		_form.Controls.Add(_txbPassword);
		_btnOk = new C1Button
		{
			Text = "确定",
			Location = new Point(240, 230),
			Size = new Size(70, 30)
		};
		_btnOk.Click += _btnOk_Click;
		_form.Controls.Add(_btnOk);
		_btnCancel = new C1Button
		{
			Text = "取消",
			Location = new Point(320, 230),
			Size = new Size(70, 30)
		};
		_btnCancel.Click += _btnCancel_Click;
		_form.Controls.Add(_btnCancel);
		Theme.SetCurrentTree(_form);
	}

	public DialogResult ShowDialog()
	{
		return _form.ShowDialog();
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.Cancel;
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(Username))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户名不得为空");
		}
		else if (string.IsNullOrEmpty(Password))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "密码不得为空");
		}
		else
		{
			_form.DialogResult = DialogResult.OK;
		}
	}
}
