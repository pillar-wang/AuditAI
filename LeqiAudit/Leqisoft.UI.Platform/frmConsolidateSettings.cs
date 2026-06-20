﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class frmConsolidateSettings : C1RibbonForm
{
	internal DropCheckBox<Leqisoft.Model.Column> _dcbGroup;

	internal DropCheckBox<Leqisoft.Model.Column> _dcbAggregate;

	private IContainer components;

	private C1SplitContainer container;

	private C1SplitterPanel pnlBottom;

	private C1SplitterPanel pnlGrid;

	private C1Label lbl1;

	private C1Label lbl2;

	internal C1FlexGrid _grid;

	internal C1Button _btnCancel;

	internal C1Button _btnOk;

	internal C1ComboBox _cmbMode;

	internal C1Label _lblMode;

	internal C1TextBox _txtConsolidationName;

	internal C1Label _lblConsolidationName;

	internal C1CheckBox _chkShowDetail;

	public frmConsolidateSettings()
	{
		InitializeComponent();
		base.Shown += FrmConsolidateSettings_Shown;
		_grid.SelectionMode = SelectionModeEnum.Cell;
		_grid.Paint += delegate(object s1, PaintEventArgs e1)
		{
			_grid.DrawFormBorder(e1.Graphics);
		};
		_dcbGroup = new DropCheckBox<Leqisoft.Model.Column>
		{
			Left = lbl1.Left + lbl1.Width + 20,
			Top = lbl1.Top,
			ValueDisplay = (Leqisoft.Model.Column c) => c.CaptionDisplay,
			TextDisplay = (IEnumerable<Leqisoft.Model.Column> iter) => string.Join("|", iter.Select((Leqisoft.Model.Column c) => c.CaptionDisplay))
		};
		pnlBottom.Controls.Add(_dcbGroup);
		_dcbAggregate = new DropCheckBox<Leqisoft.Model.Column>
		{
			Left = lbl2.Left + lbl2.Width + 20,
			Top = lbl2.Top,
			ValueDisplay = (Leqisoft.Model.Column c) => c.CaptionDisplay,
			TextDisplay = (IEnumerable<Leqisoft.Model.Column> iter) => string.Join("|", iter.Select((Leqisoft.Model.Column c) => c.CaptionDisplay))
		};
		pnlBottom.Controls.Add(_dcbAggregate);
		_lblMode = new C1Label
		{
			AutoSize = true,
			BackColor = System.Drawing.Color.Transparent,
			BorderStyle = System.Windows.Forms.BorderStyle.None,
			ForeColor = System.Drawing.Color.Black,
			Location = new System.Drawing.Point(15, 55),
			Name = "_lblMode",
			Size = new System.Drawing.Size(60, 17),
			Value = "合并方式"
		};
		pnlBottom.Controls.Add(_lblMode);
		_cmbMode = new C1ComboBox
		{
			Location = new System.Drawing.Point(80, 52),
			Name = "_cmbMode",
			Size = new System.Drawing.Size(120, 23),
			DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList
		};
		_cmbMode.Items.Add("分组汇总合并");
		_cmbMode.Items.Add("逐行追加合并");
		pnlBottom.Controls.Add(_cmbMode);
		_txtConsolidationName = new C1TextBox
		{
			Location = new System.Drawing.Point(220, 52),
			Name = "_txtConsolidationName",
			Size = new System.Drawing.Size(150, 23)
		};
		pnlBottom.Controls.Add(_txtConsolidationName);
		_lblConsolidationName = new C1Label
		{
			AutoSize = true,
			BackColor = System.Drawing.Color.Transparent,
			BorderStyle = System.Windows.Forms.BorderStyle.None,
			ForeColor = System.Drawing.Color.Black,
			Location = new System.Drawing.Point(220, 35),
			Name = "_lblConsolidationName",
			Value = "合并报表名称"
		};
		pnlBottom.Controls.Add(_lblConsolidationName);
		_chkShowDetail = new C1CheckBox
		{
			Location = new System.Drawing.Point(420, 55),
			Name = "_chkShowDetail",
			Size = new System.Drawing.Size(140, 20),
			Text = "显示工作底稿明细"
		};
		pnlBottom.Controls.Add(_chkShowDetail);
	}

	private void FrmConsolidateSettings_Shown(object sender, EventArgs e)
	{
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Resources.ConsolidateSettings);
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
		this.container = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlBottom = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._btnCancel = new C1.Win.C1Input.C1Button();
		this._btnOk = new C1.Win.C1Input.C1Button();
		this.lbl2 = new C1.Win.C1Input.C1Label();
		this.lbl1 = new C1.Win.C1Input.C1Label();
		this.pnlGrid = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._grid = new C1.Win.C1FlexGrid.C1FlexGrid();
		((System.ComponentModel.ISupportInitialize)this.container).BeginInit();
		this.container.SuspendLayout();
		this.pnlBottom.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this._btnOk).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lbl2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lbl1).BeginInit();
		this.pnlGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._grid).BeginInit();
		base.SuspendLayout();
		this.container.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.container.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.container.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.container.Dock = System.Windows.Forms.DockStyle.Fill;
		this.container.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.container.FixedLineWidth = 0;
		this.container.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.container.HeaderHeight = 27;
		this.container.Location = new System.Drawing.Point(0, 0);
		this.container.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.container.Name = "container";
		this.container.Panels.Add(this.pnlBottom);
		this.container.Panels.Add(this.pnlGrid);
		this.container.Size = new System.Drawing.Size(792, 503);
		this.container.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.container.SplitterWidth = 5;
		this.container.TabIndex = 0;
		this.container.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlBottom.Controls.Add(this._btnCancel);
		this.pnlBottom.Controls.Add(this._btnOk);
		this.pnlBottom.Controls.Add(this.lbl2);
		this.pnlBottom.Controls.Add(this.lbl1);
		this.pnlBottom.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlBottom.Height = 103;
		this.pnlBottom.KeepRelativeSize = false;
		this.pnlBottom.Location = new System.Drawing.Point(0, 400);
		this.pnlBottom.MinHeight = 52;
		this.pnlBottom.MinWidth = 52;
		this.pnlBottom.Name = "pnlBottom";
		this.pnlBottom.Resizable = false;
		this.pnlBottom.Size = new System.Drawing.Size(792, 103);
		this.pnlBottom.SizeRatio = 17.34;
		this.pnlBottom.TabIndex = 0;
		this.pnlBottom.Width = 792;
		this._btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this._btnCancel.Location = new System.Drawing.Point(684, 57);
		this._btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._btnCancel.Name = "_btnCancel";
		this._btnCancel.Size = new System.Drawing.Size(87, 33);
		this._btnCancel.TabIndex = 3;
		this._btnCancel.Text = "取消";
		this._btnCancel.UseVisualStyleBackColor = true;
		this._btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this._btnOk.Location = new System.Drawing.Point(567, 57);
		this._btnOk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._btnOk.Name = "_btnOk";
		this._btnOk.Size = new System.Drawing.Size(87, 33);
		this._btnOk.TabIndex = 2;
		this._btnOk.Text = "确定";
		this._btnOk.UseVisualStyleBackColor = true;
		this.lbl2.AutoSize = true;
		this.lbl2.BackColor = System.Drawing.Color.Transparent;
		this.lbl2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lbl2.ForeColor = System.Drawing.Color.Black;
		this.lbl2.Location = new System.Drawing.Point(309, 23);
		this.lbl2.Name = "lbl2";
		this.lbl2.Size = new System.Drawing.Size(29, 17);
		this.lbl2.TabIndex = 1;
		this.lbl2.Tag = null;
		this.lbl2.Value = "合并金额列";
		this.lbl1.AutoSize = true;
		this.lbl1.BackColor = System.Drawing.Color.Transparent;
		this.lbl1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lbl1.ForeColor = System.Drawing.Color.Black;
		this.lbl1.Location = new System.Drawing.Point(15, 23);
		this.lbl1.Name = "lbl1";
		this.lbl1.Size = new System.Drawing.Size(29, 17);
		this.lbl1.TabIndex = 0;
		this.lbl1.Tag = null;
		this.lbl1.Value = "合并维度列";
		this.pnlGrid.Controls.Add(this._grid);
		this.pnlGrid.Height = 400;
		this.pnlGrid.KeepRelativeSize = false;
		this.pnlGrid.Location = new System.Drawing.Point(0, 0);
		this.pnlGrid.MinHeight = 52;
		this.pnlGrid.MinWidth = 52;
		this.pnlGrid.Name = "pnlGrid";
		this.pnlGrid.Size = new System.Drawing.Size(792, 400);
		this.pnlGrid.SizeRatio = 100.0;
		this.pnlGrid.TabIndex = 1;
		this.pnlGrid.Width = 792;
		this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this._grid.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this._grid.Location = new System.Drawing.Point(0, 0);
		this._grid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._grid.Name = "_grid";
		this._grid.Rows.DefaultSize = 20;
		this._grid.Size = new System.Drawing.Size(792, 400);
		this._grid.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(792, 503);
		base.Controls.Add(this.container);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmConsolidateSettings";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "合并报表设置";
		((System.ComponentModel.ISupportInitialize)this.container).EndInit();
		this.container.ResumeLayout(false);
		this.pnlBottom.ResumeLayout(false);
		this.pnlBottom.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this._btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this._btnOk).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lbl2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lbl1).EndInit();
		this.pnlGrid.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this._grid).EndInit();
		base.ResumeLayout(false);
	}
}
