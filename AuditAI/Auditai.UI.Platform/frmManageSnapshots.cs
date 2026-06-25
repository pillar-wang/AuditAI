using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

internal class frmManageSnapshots : C1RibbonForm
{
	internal enum ViewKind
	{
		Snapshot,
		Recycle
	}

	private const string CN_NAME = "Name";

	private const string CN_KIND = "Kind";

	private const string CN_SIZE = "Size";

	private const string CN_DATETIME = "DateTime";

	private ManageSnapshots _owner;

	private C1ContextMenu _ctx = new C1ContextMenu();

	private C1Command _cmdDelete = new C1Command();

	private C1CommandLink _lnkDelete = new C1CommandLink();

	private IContainer components;

	private C1FlexGridEx _grid;

	private C1SplitContainer _ctn;

	private C1SplitterPanel _pnl1;

	private C1Button _btnCancel;

	private C1Button _btnOk;

	private C1SplitterPanel _pnl2;

	internal ViewKind View { get; set; }

	internal SnapshotInfo SelectedSnapshot { get; set; }

	internal frmManageSnapshots(ManageSnapshots owner)
	{
		InitializeComponent();
		_owner = owner;
		_grid.Paint += delegate(object s1, PaintEventArgs e1)
		{
			_grid.DrawFormBorder(e1.Graphics);
		};
	}

	private void frmManageSnapshots_Load(object sender, EventArgs e)
	{
		base.Shown += FrmManageSnapshots_Shown;
		_grid.Rows.DefaultSize = 30;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		_grid.Cols.Fixed = 0;
		_grid.SelectionMode = SelectionModeEnum.Row;
		_grid.ExtendLastCol = true;
		_grid.AllowAddNew = false;
		_grid.AllowDelete = false;
		_grid.AllowDragging = AllowDraggingEnum.None;
		_grid.AllowEditing = false;
		_grid.AllowFiltering = false;
		_grid.AllowFreezing = AllowFreezingEnum.None;
		_grid.AllowMerging = AllowMergingEnum.None;
		_grid.AllowMergingFixed = AllowMergingEnum.None;
		_grid.AllowResizing = AllowResizingEnum.Both;
		_grid.AllowSorting = AllowSortingEnum.None;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Name = "Name";
		column.Caption = "文件名";
		column = _grid.Cols.Add();
		column.Name = "Kind";
		column.Caption = "文件类型";
		column = _grid.Cols.Add();
		column.Name = "Size";
		column.Caption = "文件大小";
		column = _grid.Cols.Add();
		column.Name = "DateTime";
		_grid.MouseDoubleClick += _grid_MouseDoubleClick;
		_grid.MouseClick += _grid_MouseClick;
		_btnOk.Click += _btnOk_Click;
		_btnCancel.Click += _btnCancel_Click;
		_cmdDelete.CommandStateQuery += _cmdDelete_CommandStateQuery;
		_cmdDelete.Click += _cmdDelete_Click;
		_lnkDelete.Command = _cmdDelete;
		_ctx.CommandLinks.Add(_lnkDelete);
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
	}

	private void _cmdDelete_Click(object sender, ClickEventArgs e)
	{
		SnapshotInfo si = _grid.Rows[_grid.Row].UserData as SnapshotInfo;
		string text = null;
		switch (View)
		{
		case ViewKind.Snapshot:
			text = "确定要永久删除选定的历史文件吗？";
			break;
		case ViewKind.Recycle:
			text = "确定要永久删除选定的回收文件吗？";
			break;
		}
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, text, MessageBoxButtons.OKCancel) != DialogResult.Cancel)
		{
			Program.MainForm.CurrentProject.SnapshotManager.DeleteSnapshot(si);
			Populate();
		}
	}

	private void _cmdDelete_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		_cmdDelete.Text = "删除";
		if (_grid.Row >= _grid.Rows.Fixed && _grid.Row < _grid.Rows.Count)
		{
			_cmdDelete.Enabled = true;
		}
		else
		{
			_cmdDelete.Enabled = false;
		}
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		if (_grid.Row >= _grid.Rows.Fixed && _grid.Row < _grid.Rows.Count)
		{
			SnapshotInfo selectedSnapshot = _grid.Rows[_grid.Row].UserData as SnapshotInfo;
			SelectedSnapshot = selectedSnapshot;
			base.DialogResult = DialogResult.OK;
		}
		else
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择");
		}
	}

	private void FrmManageSnapshots_Shown(object sender, EventArgs e)
	{
		Populate();
	}

	private void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.RowHeader || hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			SnapshotInfo selectedSnapshot = _grid.Rows[hitTestInfo.Row].UserData as SnapshotInfo;
			SelectedSnapshot = selectedSnapshot;
			base.DialogResult = DialogResult.OK;
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && _grid.HitTest(e.Location).Type == HitTestTypeEnum.Cell)
		{
			_ctx.ShowContextMenu(_grid, e.Location);
		}
	}

	private static string GetKindString(int kind)
	{
		return kind switch
		{
			0 => "云表格", 
			1 => "云文档", 
			2 => "云图片", 
			3 => "PDF", 
			_ => "未知", 
		};
	}

	private static Bitmap GetKindBitmap(int kind)
	{
		return kind switch
		{
			0 => Resources.TreeTable, 
			1 => Resources.TreeDoc, 
			2 => Resources.TreeImage, 
			3 => Resources.TreePdf, 
			_ => null, 
		};
	}

	private void Populate()
	{
		List<SnapshotInfo> list = null;
		switch (View)
		{
		case ViewKind.Snapshot:
			Text = "历史版本";
			_grid.Cols["DateTime"].Caption = "保存时间";
			list = Program.MainForm.CurrentProject.SnapshotManager.GetSnapshots(Program.MainForm.ProjectHierarchy.SelectedNode);
			break;
		case ViewKind.Recycle:
			Text = "回收文件";
			_grid.Cols["DateTime"].Caption = "删除时间";
			list = Program.MainForm.CurrentProject.SnapshotManager.GetRecycleList();
			break;
		}
		_grid.Rows.Count = _grid.Rows.Fixed;
		foreach (SnapshotInfo item in list)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
			row["Name"] = item.Name;
			row["Kind"] = GetKindString(item.Kind);
			row["Size"] = Auditai.Model.Util.GetReadableFileSize(item.Size);
			row["DateTime"] = item.DateTime;
			_grid.SetCellImage(row.Index, "Name", GetKindBitmap(item.Kind));
			row.UserData = item;
		}
		_grid.AutoSizeCols();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Platform.frmManageSnapshots));
		this._grid = new Auditai.UI.Controls.C1FlexGridEx();
		this._ctn = new C1.Win.C1SplitContainer.C1SplitContainer();
		this._pnl1 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._btnCancel = new C1.Win.C1Input.C1Button();
		this._btnOk = new C1.Win.C1Input.C1Button();
		this._pnl2 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this._grid).BeginInit();
		((System.ComponentModel.ISupportInitialize)this._ctn).BeginInit();
		this._ctn.SuspendLayout();
		this._pnl1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this._btnOk).BeginInit();
		this._pnl2.SuspendLayout();
		base.SuspendLayout();
		this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this._grid.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this._grid.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this._grid.Location = new System.Drawing.Point(0, 0);
		this._grid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._grid.Name = "_grid";
		this._grid.Rows.DefaultSize = 20;
		this._grid.Size = new System.Drawing.Size(592, 366);
		this._grid.TabIndex = 0;
		this._ctn.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this._ctn.Dock = System.Windows.Forms.DockStyle.Fill;
		this._ctn.HeaderHeight = 27;
		this._ctn.Location = new System.Drawing.Point(0, 0);
		this._ctn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._ctn.Name = "_ctn";
		this._ctn.Panels.Add(this._pnl1);
		this._ctn.Panels.Add(this._pnl2);
		this._ctn.Size = new System.Drawing.Size(592, 419);
		this._ctn.SplitterWidth = 5;
		this._ctn.TabIndex = 1;
		this._ctn.UseParentVisualStyle = false;
		this._pnl1.Controls.Add(this._btnCancel);
		this._pnl1.Controls.Add(this._btnOk);
		this._pnl1.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this._pnl1.Height = 52;
		this._pnl1.KeepRelativeSize = false;
		this._pnl1.Location = new System.Drawing.Point(0, 367);
		this._pnl1.MinHeight = 52;
		this._pnl1.MinWidth = 52;
		this._pnl1.Name = "_pnl1";
		this._pnl1.Resizable = false;
		this._pnl1.Size = new System.Drawing.Size(592, 52);
		this._pnl1.SizeRatio = 8.769;
		this._pnl1.TabIndex = 0;
		this._pnl1.Width = 592;
		this._btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this._btnCancel.Location = new System.Drawing.Point(494, 7);
		this._btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._btnCancel.Name = "_btnCancel";
		this._btnCancel.Size = new System.Drawing.Size(87, 33);
		this._btnCancel.TabIndex = 1;
		this._btnCancel.Text = "取消";
		this._btnCancel.UseVisualStyleBackColor = true;
		this._btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this._btnOk.Location = new System.Drawing.Point(390, 7);
		this._btnOk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._btnOk.Name = "_btnOk";
		this._btnOk.Size = new System.Drawing.Size(87, 33);
		this._btnOk.TabIndex = 0;
		this._btnOk.Text = "确定";
		this._btnOk.UseVisualStyleBackColor = true;
		this._pnl2.Controls.Add(this._grid);
		this._pnl2.Height = 366;
		this._pnl2.Location = new System.Drawing.Point(0, 0);
		this._pnl2.MinHeight = 52;
		this._pnl2.MinWidth = 52;
		this._pnl2.Name = "_pnl2";
		this._pnl2.Size = new System.Drawing.Size(592, 366);
		this._pnl2.TabIndex = 1;
		this._pnl2.Width = 592;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(592, 419);
		base.Controls.Add(this._ctn);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmManageSnapshots";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "历史版本";
		base.Load += new System.EventHandler(frmManageSnapshots_Load);
		((System.ComponentModel.ISupportInitialize)this._grid).EndInit();
		((System.ComponentModel.ISupportInitialize)this._ctn).EndInit();
		this._ctn.ResumeLayout(false);
		this._pnl1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this._btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this._btnOk).EndInit();
		this._pnl2.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
