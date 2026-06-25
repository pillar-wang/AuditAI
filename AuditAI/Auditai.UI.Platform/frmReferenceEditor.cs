using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class frmReferenceEditor : C1RibbonForm
{
	private IContainer components;

	private C1SplitContainer _ctnAll;

	private C1SplitterPanel _pnlBottom;

	private C1SplitterPanel _pnlGrid;

	internal C1FlexGridEx _grid;

	internal C1Button btnCancel;

	internal C1Button btnOk;

	public frmReferenceEditor()
	{
		InitializeComponent();
		base.Shown += FrmReferenceEditor_Shown;
		base.Size = new Size(600, 450);
		base.StartPosition = FormStartPosition.CenterScreen;
		_grid.Paint += delegate(object s1, PaintEventArgs e1)
		{
			_grid.DrawFormBorder(e1.Graphics);
		};
	}

	private void FrmReferenceEditor_Shown(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.ReferenceManager);
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
		this._ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this._pnlBottom = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnOk = new C1.Win.C1Input.C1Button();
		this._pnlGrid = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._grid = new Auditai.UI.Controls.C1FlexGridEx();
		((System.ComponentModel.ISupportInitialize)this._ctnAll).BeginInit();
		this._ctnAll.SuspendLayout();
		this._pnlBottom.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).BeginInit();
		this._pnlGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._grid).BeginInit();
		base.SuspendLayout();
		this._ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this._ctnAll.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this._ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this._ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this._ctnAll.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this._ctnAll.FixedLineWidth = 0;
		this._ctnAll.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this._ctnAll.HeaderHeight = 27;
		this._ctnAll.Location = new System.Drawing.Point(0, 0);
		this._ctnAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._ctnAll.Name = "_ctnAll";
		this._ctnAll.Panels.Add(this._pnlBottom);
		this._ctnAll.Panels.Add(this._pnlGrid);
		this._ctnAll.Size = new System.Drawing.Size(592, 419);
		this._ctnAll.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this._ctnAll.SplitterWidth = 5;
		this._ctnAll.TabIndex = 0;
		this._ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this._pnlBottom.Controls.Add(this.btnCancel);
		this._pnlBottom.Controls.Add(this.btnOk);
		this._pnlBottom.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this._pnlBottom.Height = 52;
		this._pnlBottom.KeepRelativeSize = false;
		this._pnlBottom.Location = new System.Drawing.Point(0, 367);
		this._pnlBottom.MinHeight = 52;
		this._pnlBottom.MinWidth = 52;
		this._pnlBottom.Name = "_pnlBottom";
		this._pnlBottom.Resizable = false;
		this._pnlBottom.Size = new System.Drawing.Size(592, 52);
		this._pnlBottom.SizeRatio = 8.215;
		this._pnlBottom.TabIndex = 0;
		this._pnlBottom.Width = 592;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(493, 6);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(87, 33);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOk.Location = new System.Drawing.Point(389, 6);
		this.btnOk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(87, 33);
		this.btnOk.TabIndex = 0;
		this.btnOk.Text = "确定";
		this.btnOk.UseVisualStyleBackColor = true;
		this._pnlGrid.Controls.Add(this._grid);
		this._pnlGrid.Height = 367;
		this._pnlGrid.Location = new System.Drawing.Point(0, 0);
		this._pnlGrid.MinHeight = 52;
		this._pnlGrid.MinWidth = 52;
		this._pnlGrid.Name = "_pnlGrid";
		this._pnlGrid.Size = new System.Drawing.Size(592, 367);
		this._pnlGrid.SizeRatio = 100.0;
		this._pnlGrid.TabIndex = 1;
		this._pnlGrid.Width = 592;
		this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this._grid.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this._grid.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this._grid.Location = new System.Drawing.Point(0, 0);
		this._grid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._grid.Name = "_grid";
		this._grid.Rows.DefaultSize = 20;
		this._grid.Size = new System.Drawing.Size(592, 367);
		this._grid.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(592, 419);
		base.Controls.Add(this._ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmReferenceEditor";
		base.ShowInTaskbar = false;
		this.Text = "变量管理";
		((System.ComponentModel.ISupportInitialize)this._ctnAll).EndInit();
		this._ctnAll.ResumeLayout(false);
		this._pnlBottom.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).EndInit();
		this._pnlGrid.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this._grid).EndInit();
		base.ResumeLayout(false);
	}
}
