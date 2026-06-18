using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

internal class frmAccountEditor : C1RibbonForm
{
	private AccountTreeEditor _owner;

	private bool isAuxiliary;

	private bool isAdd;

	private LedgerViewer _viewer;

	private Account _modifyAccount;

	private AuxiliaryClass _operatingAuxClass;

#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1Label lblAccountCode;

	private C1Button btnConfirm;

	private C1Button btnCancel;

	private C1Label lblAccountName;

	private C1TextBox txtAccountCode;

	private C1TextBox txtAccountName;

	private Ledger ledger => _owner.Ledger;

	public string AccountCode { get; private set; }

	public string AccountName { get; private set; }

	public Account ParentAccount { get; private set; }

	public frmAccountEditor(LedgerViewer viewer, AccountTreeEditor onwer)
	{
		_viewer = viewer;
		_owner = onwer;
		InitializeComponent();
		base.Shown += FrmAccountEditor_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		txtAccountCode.Location = new Point(lblAccountCode.Right, lblAccountCode.Top - 2);
		txtAccountName.Location = new Point(lblAccountName.Right, lblAccountName.Top - 2);
	}

	private void FrmAccountEditor_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.largeModifyVoucher);
	}

	public DialogResult ShowAddAccountDialog()
	{
		isAdd = true;
		Text = "新增科目";
		lblAccountCode.Text = "科目代码：";
		lblAccountName.Text = "科目名称：";
		base.ActiveControl = txtAccountCode;
		Theme.SetCurrentTree(this);
		return ShowDialog();
	}

	public DialogResult ShowModifyAccountDialog(Account account)
	{
		_modifyAccount = account;
		Text = "修改科目";
		lblAccountCode.Text = "科目代码：";
		lblAccountName.Text = "科目名称：";
		txtAccountCode.ReadOnly = true;
		base.ActiveControl = txtAccountName;
		Theme.SetCurrentTree(this);
		return ShowDialog();
	}

	public DialogResult ShowAddAuxiliaryDialog(AuxiliaryClass auxiliaryClass)
	{
		_operatingAuxClass = auxiliaryClass;
		isAdd = true;
		isAuxiliary = true;
		Text = "新增辅助核算";
		lblAccountCode.Text = "辅助核算代码：";
		lblAccountName.Text = "辅助核算名称：";
		base.ActiveControl = txtAccountCode;
		Theme.SetCurrentTree(this);
		return ShowDialog();
	}

	public DialogResult ShowModifyAuxiliaryDialog(AuxiliaryClass auxiliaryClass)
	{
		_operatingAuxClass = auxiliaryClass;
		isAuxiliary = true;
		Text = "修改辅助核算";
		lblAccountCode.Text = "辅助核算代码：";
		lblAccountName.Text = "辅助核算名称：";
		base.ActiveControl = txtAccountCode;
		Theme.SetCurrentTree(this);
		return ShowDialog();
	}

	public void SetAccountCode(string code)
	{
		AccountCode = code;
		txtAccountCode.Text = code;
	}

	public void SetAccountName(string name)
	{
		AccountName = name;
		txtAccountName.Text = name;
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		if (isAuxiliary)
		{
			if (!ValidateAuxiliary())
			{
				return;
			}
		}
		else if (!ValidateAccount())
		{
			return;
		}
		AccountCode = txtAccountCode.Text.Trim();
		AccountName = txtAccountName.Text.Trim();
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private bool ValidateAuxiliary()
	{
		if (string.IsNullOrEmpty(txtAccountCode.Text.Trim()))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "辅助核算代码不能为空");
			return false;
		}
		if (string.IsNullOrEmpty(txtAccountName.Text.Trim()))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "辅助核算名称不能为空");
			return false;
		}
		if (isAdd)
		{
			if (_operatingAuxClass.Items.Any((AuxiliaryItem i) => i.Code == txtAccountCode.Text.Trim()))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该辅助核算项代码已存在，辅助核算项代码不能重复。");
				return false;
			}
		}
		else if (ledger.AuxiliaryClasses.Any((AuxiliaryClass c) => c != _operatingAuxClass && c.Code == txtAccountCode.Text.Trim()))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该辅助核算类代码已存在，辅助核算类代码不能重复。");
			return false;
		}
		return true;
	}

	private bool ValidateAccount()
	{
		if (string.IsNullOrEmpty(txtAccountCode.Text.Trim()))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码不能为空");
			return false;
		}
		if (string.IsNullOrEmpty(txtAccountName.Text.Trim()))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目名称不能为空");
			return false;
		}
		ParentAccount = null;
		if (!isAdd)
		{
			return true;
		}
		List<IGrouping<int, Account>> list = (from a in ledger.Accounts
			group a by a.Code.Length into g
			orderby g.Key
			select g).ToList();
		string newCode = txtAccountCode.Text.Trim();
		if (list.Count > 0)
		{
			if (newCode.Length < list.First().Key)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码不符合编码规则，请修改！");
				return false;
			}
			if (newCode.Length > list.Last().Key)
			{
				ParentAccount = list.Last().FirstOrDefault((Account a) => newCode.StartsWith(a.Code));
				if (ParentAccount == null)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码不符合编码规则，请修改！");
					return false;
				}
			}
			else
			{
				if (list.All((IGrouping<int, Account> g) => newCode.Length != g.Key))
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码不符合编码规则，请修改！");
					return false;
				}
				for (int i = 1; i < list.Count; i++)
				{
					if (newCode.Length == list[i].Key)
					{
						ParentAccount = list[i - 1].FirstOrDefault((Account a) => newCode.StartsWith(a.Code));
						if (ParentAccount != null)
						{
							break;
						}
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码不符合编码规则，请修改！");
						return false;
					}
				}
			}
		}
		if (ledger.Accounts.Any((Account a) => a != _modifyAccount && a.Code == newCode))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该科目的科目代码已存在，科目代码不能重复。");
			return false;
		}
		if (ParentAccount != null && ParentAccount.Children.Count == 0 && !_viewer.CacheManager.IsEmptyAccountWithCache(ParentAccount))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "上级科目\u00a0(" + ParentAccount.Code + ")" + ParentAccount.Name + "\u00a0存在数据，不能在其下增加子科目。");
			return false;
		}
		return true;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
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
		this.lblAccountCode = new C1.Win.C1Input.C1Label();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.lblAccountName = new C1.Win.C1Input.C1Label();
		this.txtAccountCode = new C1.Win.C1Input.C1TextBox();
		this.txtAccountName = new C1.Win.C1Input.C1TextBox();
		((System.ComponentModel.ISupportInitialize)this.lblAccountCode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblAccountName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtAccountCode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtAccountName).BeginInit();
		base.SuspendLayout();
		this.lblAccountCode.BackColor = System.Drawing.Color.Transparent;
		this.lblAccountCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblAccountCode.ForeColor = System.Drawing.Color.Black;
		this.lblAccountCode.Location = new System.Drawing.Point(1, 20);
		this.lblAccountCode.Name = "lblAccountCode";
		this.lblAccountCode.Size = new System.Drawing.Size(100, 17);
		this.lblAccountCode.TabIndex = 0;
		this.lblAccountCode.Tag = null;
		this.lblAccountCode.Text = "科目代码：";
		this.lblAccountCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblAccountCode.TextDetached = true;
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Location = new System.Drawing.Point(246, 86);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 1;
		this.btnConfirm.Text = "保存";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(347, 86);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.lblAccountName.BackColor = System.Drawing.Color.Transparent;
		this.lblAccountName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblAccountName.ForeColor = System.Drawing.Color.Black;
		this.lblAccountName.Location = new System.Drawing.Point(1, 54);
		this.lblAccountName.Name = "lblAccountName";
		this.lblAccountName.Size = new System.Drawing.Size(100, 17);
		this.lblAccountName.TabIndex = 4;
		this.lblAccountName.Tag = null;
		this.lblAccountName.Text = "科目名称：";
		this.lblAccountName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblAccountName.TextDetached = true;
		this.txtAccountCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtAccountCode.Location = new System.Drawing.Point(117, 18);
		this.txtAccountCode.Name = "txtAccountCode";
		this.txtAccountCode.Size = new System.Drawing.Size(300, 21);
		this.txtAccountCode.TabIndex = 5;
		this.txtAccountCode.Tag = null;
		this.txtAccountCode.TextDetached = true;
		this.txtAccountName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtAccountName.Location = new System.Drawing.Point(117, 52);
		this.txtAccountName.Name = "txtAccountName";
		this.txtAccountName.Size = new System.Drawing.Size(300, 21);
		this.txtAccountName.TabIndex = 6;
		this.txtAccountName.Tag = null;
		this.txtAccountName.TextDetached = true;
		base.AcceptButton = this.btnConfirm;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(463, 125);
		base.Controls.Add(this.txtAccountName);
		base.Controls.Add(this.txtAccountCode);
		base.Controls.Add(this.lblAccountName);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnConfirm);
		base.Controls.Add(this.lblAccountCode);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmAccountEditor";
		base.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
		this.Text = "frmAccountEditor";
		((System.ComponentModel.ISupportInitialize)this.lblAccountCode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblAccountName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtAccountCode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtAccountName).EndInit();
		base.ResumeLayout(false);
	}
}
