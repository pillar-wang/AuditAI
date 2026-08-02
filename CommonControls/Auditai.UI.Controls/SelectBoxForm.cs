﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;

namespace Auditai.UI.Controls;

public class SelectBoxForm : C1RibbonForm
{
	private const string NULLVALUE = "空白";

	private List<string> allItems = new List<string>();

	private bool _opposite;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1CheckBox ckbExceptSelect;

	private C1CheckBox ckbCheckAll;

	private C1Button btnConfirm;

	private C1Button btnCancel;

	private CheckedListBox checkListBox;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel pnkCheckedList;

	private C1SplitterPanel pnlBtnButtons;

	public List<string> SelectedValues { get; set; } = new List<string>();

	#region 按钮尺寸规范（与 MessageShowBox/UpdateForm/InputBoxImpl 保持一致）
	/// <summary>按钮宽度</summary>
	private const int ButtonWidth = 110;
	/// <summary>按钮高度</summary>
	private const int ButtonHeight = 34;
	/// <summary>按钮右侧距面板右边缘的距离</summary>
	private const int ButtonRightMargin = 50;
	/// <summary>按钮之间的水平间距</summary>
	private const int ButtonGap = 12;
	#endregion

	private SelectBoxForm()
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	public SelectBoxForm(IEnumerable<string> values)
		: this()
	{
		allItems = (from v in values.Distinct()
			where !string.IsNullOrEmpty(v)
			select v).ToList();
		allItems.Insert(0, "");
		checkListBox.Items.Add("空白");
		foreach (string item in allItems.Skip(1))
		{
			checkListBox.Items.Add(item);
		}
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentTree(this);
		StandardView();
		return base.ShowDialog();
	}

	private void StandardView()
	{
		c1SplitContainer1.SplitterWidth = 0;
		pnkCheckedList.BorderWidth = 0;
		pnlBtnButtons.BorderWidth = 0;
	}

	private void ckbCheckAll_CheckedChanged(object sender, EventArgs e)
	{
		for (int i = 0; i < checkListBox.Items.Count; i++)
		{
			checkListBox.SetItemChecked(i, ckbCheckAll.Checked);
		}
	}

	private void c1CheckBox1_CheckedChanged(object sender, EventArgs e)
	{
		if (ckbExceptSelect.Checked)
		{
			_opposite = true;
		}
		else
		{
			_opposite = false;
		}
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		SelectedValues.Clear();
		foreach (int checkedIndex in checkListBox.CheckedIndices)
		{
			SelectedValues.Add(allItems[checkedIndex]);
		}
		if (checkListBox.GetItemChecked(0))
		{
			SelectedValues[0] = "";
		}
		if (_opposite)
		{
			allItems.RemoveAll((string t) => SelectedValues.Contains(t));
			SelectedValues = allItems;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
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
		this.ckbExceptSelect = new C1.Win.C1Input.C1CheckBox();
		this.ckbCheckAll = new C1.Win.C1Input.C1CheckBox();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.checkListBox = new System.Windows.Forms.CheckedListBox();
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlBtnButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnkCheckedList = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.ckbExceptSelect).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbCheckAll).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlBtnButtons.SuspendLayout();
		this.pnkCheckedList.SuspendLayout();
		base.SuspendLayout();
		this.ckbExceptSelect.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.ckbExceptSelect.BackColor = System.Drawing.SystemColors.Control;
		this.ckbExceptSelect.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbExceptSelect.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.ckbExceptSelect.ForeColor = System.Drawing.SystemColors.ControlText;
		this.ckbExceptSelect.Location = new System.Drawing.Point(244, 4);
		this.ckbExceptSelect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ckbExceptSelect.Name = "ckbExceptSelect";
		this.ckbExceptSelect.Size = new System.Drawing.Size(120, 44);
		this.ckbExceptSelect.TabIndex = 1;
		this.ckbExceptSelect.Text = "排除所选项";
		this.ckbExceptSelect.UseVisualStyleBackColor = true;
		this.ckbExceptSelect.Value = null;
		this.ckbExceptSelect.CheckedChanged += new System.EventHandler(c1CheckBox1_CheckedChanged);
		this.ckbCheckAll.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ckbCheckAll.BackColor = System.Drawing.SystemColors.Control;
		this.ckbCheckAll.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbCheckAll.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.ckbCheckAll.ForeColor = System.Drawing.SystemColors.ControlText;
		this.ckbCheckAll.Location = new System.Drawing.Point(35, 4);
		this.ckbCheckAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ckbCheckAll.Name = "ckbCheckAll";
		this.ckbCheckAll.Size = new System.Drawing.Size(104, 44);
		this.ckbCheckAll.TabIndex = 2;
		this.ckbCheckAll.Text = "全选";
		this.ckbCheckAll.UseVisualStyleBackColor = true;
		this.ckbCheckAll.Value = null;
		this.ckbCheckAll.CheckedChanged += new System.EventHandler(ckbCheckAll_CheckedChanged);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnConfirm.Location = new System.Drawing.Point(374 - ButtonRightMargin - ButtonWidth - ButtonGap - ButtonWidth, 55);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnConfirm.TabIndex = 3;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(374 - ButtonRightMargin - ButtonWidth, 55);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnCancel.TabIndex = 4;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.checkListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.checkListBox.CheckOnClick = true;
		this.checkListBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.checkListBox.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.checkListBox.FormattingEnabled = true;
		this.checkListBox.IntegralHeight = false;
		this.checkListBox.Location = new System.Drawing.Point(0, 0);
		this.checkListBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
		this.checkListBox.Name = "checkListBox";
		this.checkListBox.Size = new System.Drawing.Size(374, 276);
		this.checkListBox.TabIndex = 0;
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.c1SplitContainer1.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.pnlBtnButtons);
		this.c1SplitContainer1.Panels.Add(this.pnkCheckedList);
		this.c1SplitContainer1.Size = new System.Drawing.Size(374, 381);
		this.c1SplitContainer1.SplitterWidth = 0;
		this.c1SplitContainer1.TabIndex = 5;
		this.pnlBtnButtons.Controls.Add(this.btnCancel);
		this.pnlBtnButtons.Controls.Add(this.ckbExceptSelect);
		this.pnlBtnButtons.Controls.Add(this.btnConfirm);
		this.pnlBtnButtons.Controls.Add(this.ckbCheckAll);
		this.pnlBtnButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlBtnButtons.Height = 104;
		this.pnlBtnButtons.KeepRelativeSize = false;
		this.pnlBtnButtons.Location = new System.Drawing.Point(0, 277);
		this.pnlBtnButtons.Name = "pnlBtnButtons";
		this.pnlBtnButtons.Resizable = false;
		this.pnlBtnButtons.Size = new System.Drawing.Size(374, 104);
		this.pnlBtnButtons.SizeRatio = 27.397;
		this.pnlBtnButtons.TabIndex = 1;
		this.pnkCheckedList.Controls.Add(this.checkListBox);
		this.pnkCheckedList.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnkCheckedList.Height = 276;
		this.pnkCheckedList.Location = new System.Drawing.Point(0, 0);
		this.pnkCheckedList.MinHeight = 0;
		this.pnkCheckedList.Name = "pnkCheckedList";
		this.pnkCheckedList.Size = new System.Drawing.Size(374, 276);
		this.pnkCheckedList.SizeRatio = 100.0;
		this.pnkCheckedList.TabIndex = 0;
		base.AcceptButton = this.btnConfirm;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(374, 381);
		base.Controls.Add(this.c1SplitContainer1);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SelectBoxForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "选择";
		((System.ComponentModel.ISupportInitialize)this.ckbExceptSelect).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbCheckAll).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlBtnButtons.ResumeLayout(false);
		this.pnkCheckedList.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
