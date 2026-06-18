﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class AdvanceFilterBox : C1RibbonForm
{
	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1CommandLink lnkAppendRow = new C1CommandLink();

	private C1Command cmdAppendRow = new C1Command();

	private C1CommandLink lnkDeleteRow = new C1CommandLink();

	private C1Command cmdDeleteRow = new C1Command();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private C1CommandLink lnkAppendRow2 = new C1CommandLink();

	private C1Command cmdAppendRow2 = new C1Command();

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	public AdvanceGrid AdvanceGrid;

	private C1Button btnFilter;

	private C1Button btnCancel;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel buttonPanel;

	private C1SplitterPanel GridPanel;

	public AdvanceFilterBox()
	{
		InitializeComponent();
		base.Shown += AdvanceFilterBox_Shown;
		AdvanceGrid.Rows.DefaultSize = 30;
		base.StartPosition = FormStartPosition.CenterScreen;
		BindContexMenu();
	}

	private void AdvanceFilterBox_Shown(object sender, EventArgs e)
	{
		Theme.SetCurrentTree(this);
	}

	private void BindContexMenu()
	{
		cmdAppendRow.Text = "新增行";
		lnkAppendRow.Command = cmdAppendRow;
		cmdAppendRow.Click += CmdAppendRow_Click;
		cmdAppendRow.Image = Resources.ctxAppendRow;
		ctxCell.CommandLinks.Add(lnkAppendRow);
		cmdDeleteRow.Text = "删除行";
		lnkDeleteRow.Command = cmdDeleteRow;
		cmdDeleteRow.Click += CmdDeleteRow_Click;
		cmdDeleteRow.Image = Resources.ctxDeleteRow;
		ctxCell.CommandLinks.Add(lnkDeleteRow);
		AdvanceGrid.MouseClick += AdvanceGrid_MouseClick;
		AdvanceGrid.MouseDown += AdvanceGrid_MouseDown;
		cmdAppendRow2.Text = "新增行";
		lnkAppendRow2.Command = cmdAppendRow2;
		cmdAppendRow2.Click += CmdAppendRow_Click;
		cmdAppendRow2.Image = Resources.ctxAppendRow;
		ctxEmpty.CommandLinks.Add(lnkAppendRow2);
	}

	private void CmdAppendRow_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("追加行", "请输入追加行数：");
		if (!num.HasValue)
		{
			return;
		}
		AdvanceGrid.BeginUpdate();
		try
		{
			AdvanceGrid.Rows.Add((int)num.Value);
			PopulateIndex();
		}
		catch (Exception ex)
		{
			System.Windows.Forms.MessageBox.Show(ex.Message);
		}
		finally
		{
			AdvanceGrid.EndUpdate();
		}
	}

	private void AdvanceGrid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		switch (AdvanceGrid.HitTest(e.Location).Type)
		{
		case HitTestTypeEnum.Cell:
		case HitTestTypeEnum.RowHeader:
			if (AdvanceGrid.MouseRow >= AdvanceGrid.Rows.Fixed)
			{
				ctxCell.ShowContextMenu(AdvanceGrid, e.Location);
			}
			break;
		case HitTestTypeEnum.None:
			ctxEmpty.ShowContextMenu(AdvanceGrid, e.Location);
			break;
		}
	}

	private void CmdDeleteRow_Click(object sender, ClickEventArgs e)
	{
		int topRow = AdvanceGrid.Selection.TopRow;
		int count = AdvanceGrid.Selection.BottomRow - topRow + 1;
		if (topRow >= AdvanceGrid.Rows.Fixed)
		{
			AdvanceGrid.BeginUpdate();
			try
			{
				AdvanceGrid.Rows.RemoveRange(topRow, count);
				PopulateIndex();
			}
			finally
			{
				AdvanceGrid.EndUpdate();
			}
		}
	}

	private void PopulateIndex()
	{
		for (int i = AdvanceGrid.Rows.Fixed; i < AdvanceGrid.Rows.Count; i++)
		{
			AdvanceGrid.Rows[i][0] = i.ToString();
		}
	}

	private void AdvanceGrid_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTestInfo = AdvanceGrid.HitTest(e.Location);
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.Cell || type == HitTestTypeEnum.RowHeader)
		{
			if (hitTestInfo.Column == 0)
			{
				AdvanceGrid.Select(new CellRange
				{
					r1 = hitTestInfo.Row,
					r2 = hitTestInfo.Row,
					c1 = 0,
					c2 = AdvanceGrid.Cols.Count - 1
				});
			}
			if (!AdvanceGrid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				AdvanceGrid.Select(hitTestInfo.Row, hitTestInfo.Column);
			}
		}
	}

	private void btnFilter_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
	}

	private void btnCancle_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Leqisoft.UI.Controls.AdvanceFilterBox));
		this.btnFilter = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.AdvanceGrid = new Leqisoft.UI.Controls.AdvanceGrid();
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.buttonPanel = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.GridPanel = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.btnFilter).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.AdvanceGrid).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.buttonPanel.SuspendLayout();
		this.GridPanel.SuspendLayout();
		base.SuspendLayout();
		this.btnFilter.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnFilter.Location = new System.Drawing.Point(431, 17);
		this.btnFilter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnFilter.Name = "btnFilter";
		this.btnFilter.Size = new System.Drawing.Size(70, 26);
		this.btnFilter.TabIndex = 1;
		this.btnFilter.Text = "筛选";
		this.btnFilter.UseVisualStyleBackColor = true;
		this.btnFilter.Click += new System.EventHandler(btnFilter_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(533, 17);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消筛选";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancle_Click);
		this.AdvanceGrid.AllowDragging = C1.Win.C1FlexGrid.AllowDraggingEnum.Rows;
		this.AdvanceGrid.AutoResize = true;
		this.AdvanceGrid.ColumnInfo = resources.GetString("AdvanceGrid.ColumnInfo");
		this.AdvanceGrid.ColumnInfos = null;
		this.AdvanceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.AdvanceGrid.Location = new System.Drawing.Point(0, 0);
		this.AdvanceGrid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.AdvanceGrid.Name = "AdvanceGrid";
		this.AdvanceGrid.Rows.Count = 6;
		this.AdvanceGrid.Rows.DefaultSize = 20;
		this.AdvanceGrid.Size = new System.Drawing.Size(642, 311);
		this.AdvanceGrid.TabIndex = 0;
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.c1SplitContainer1.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.buttonPanel);
		this.c1SplitContainer1.Panels.Add(this.GridPanel);
		this.c1SplitContainer1.Size = new System.Drawing.Size(642, 367);
		this.c1SplitContainer1.SplitterWidth = 0;
		this.c1SplitContainer1.TabIndex = 3;
		this.buttonPanel.Controls.Add(this.btnFilter);
		this.buttonPanel.Controls.Add(this.btnCancel);
		this.buttonPanel.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.buttonPanel.Height = 56;
		this.buttonPanel.KeepRelativeSize = false;
		this.buttonPanel.Location = new System.Drawing.Point(0, 311);
		this.buttonPanel.Name = "buttonPanel";
		this.buttonPanel.Size = new System.Drawing.Size(642, 56);
		this.buttonPanel.SizeRatio = 16.519;
		this.buttonPanel.TabIndex = 1;
		this.GridPanel.Controls.Add(this.AdvanceGrid);
		this.GridPanel.Height = 311;
		this.GridPanel.Location = new System.Drawing.Point(0, 0);
		this.GridPanel.MinHeight = 0;
		this.GridPanel.Name = "GridPanel";
		this.GridPanel.Size = new System.Drawing.Size(642, 311);
		this.GridPanel.SizeRatio = 100.0;
		this.GridPanel.TabIndex = 0;
		base.AcceptButton = this.btnFilter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(642, 367);
		base.Controls.Add(this.c1SplitContainer1);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AdvanceFilterBox";
		base.ShowInTaskbar = false;
		this.Text = "高级筛选";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.btnFilter).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.AdvanceGrid).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.buttonPanel.ResumeLayout(false);
		this.GridPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
