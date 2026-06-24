using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using C1.C1Excel;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class FormTicketImportXlsx
{
	private readonly C1RibbonForm _form;

	private readonly C1DockingTab _tab;

	private readonly C1FlexGridEx _grid;

	private readonly C1XLBookEx _xl;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	public TicketDesignTableVM Ticket { get; private set; }

	public FormTicketImportXlsx()
	{
		_form = FormFactory.Create();
		_form.Size = new Size(800, 600);
		_form.Text = "选择工作表";
		_form.Shown += _form_Shown;
		_form.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.ImportXlsx16);
		C1SplitContainer c1SplitContainer = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_form.Controls.Add(c1SplitContainer);
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			KeepRelativeSize = false,
			MinHeight = 50,
			Height = 50,
			Resizable = false,
			Dock = PanelDockStyle.Bottom
		};
		_btnOk = new C1Button
		{
			Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right),
			Text = "确定"
		};
		_btnOk.Click += _btnOk_Click;
		c1SplitterPanel.Controls.Add(_btnOk);
		_btnCancel = new C1Button
		{
			Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right),
			Text = "取消"
		};
		_form.CancelButton = _btnCancel;
		c1SplitterPanel.Controls.Add(_btnCancel);
		c1SplitContainer.Panels.Add(c1SplitterPanel);
		c1SplitterPanel = new C1SplitterPanel
		{
			KeepRelativeSize = false,
			MinHeight = 25,
			Height = 25,
			Resizable = false,
			Dock = PanelDockStyle.Bottom
		};
		c1SplitContainer.Panels.Add(c1SplitterPanel);
		_tab = new C1DockingTab
		{
			Alignment = TabAlignment.Bottom,
			Dock = DockStyle.Fill,
			TabSizeMode = TabSizeModeEnum.User,
			ShowTabList = true,
			TabsShowFocusCues = false
		};
		_tab.SelectedTabChanged += _tab_SelectedTabChanged;
		c1SplitterPanel.Controls.Add(_tab);
		c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false
		};
		c1SplitContainer.Panels.Add(c1SplitterPanel);
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowEditing = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowMergingFixed = AllowMergingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
		};
		_grid.Rows.DefaultSize = 30;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 1;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.Paint += _grid_Paint;
		_grid.MouseMove += _grid_MouseMove;
		c1SplitterPanel.Controls.Add(_grid);
		_xl = new C1XLBookEx();
		Theme.SetCurrentTree(_form);
	}

	public bool Import()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Title = "选择导入为" + Program.MainForm.TicketDesignEditor.Table.Ticket.GetLevelString() + "的Excel模板";
		openFileDialog.Filter = "Excel 文件(*.xlsx)|*.xlsx";
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return false;
		}
		try
		{
			_xl.Load(openFileDialog.FileName);
		}
		catch (Exception ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return false;
		}
		Populate();
		if (_form.ShowDialog() != DialogResult.OK)
		{
			return false;
		}
		return true;
	}

	private void _form_Shown(object sender, EventArgs e)
	{
		_btnOk.Size = new Size(80, 30);
		_btnOk.Location = new Point(600, 10);
		_btnCancel.Size = new Size(80, 30);
		_btnCancel.Location = new Point(700, 10);
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col == 0 && e.Row >= _grid.Rows.Fixed)
		{
			e.Text = (e.Row - _grid.Rows.Fixed + 1).ToString();
		}
		else if (e.Row == 0 && e.Col >= _grid.Cols.Fixed)
		{
			e.Text = Leqisoft.Model.Column.GetExcelColumnName(e.Col - _grid.Cols.Fixed);
		}
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
		TicketDesignCellVM cell = Ticket.GetCell(e.Row, e.Col);
		FontStyle fontStyle = FontStyle.Regular;
		if (cell.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (cell.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		styleNew.Font = new Font(cell.FontFamily, cell.FontSize, fontStyle);
		styleNew.ForeColor = cell.ForeColor;
		styleNew.TextAlign = C1FlexGridEx.ToTextAlign(cell.Align);
		styleNew.Margins = new System.Drawing.Printing.Margins(cell.Indent, 0, 0, 0);
		if (cell.HasFormula())
		{
			styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Leqisoft.Model.Project.Current);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
				e.Text = formulaEvaluator.GetDisplayStringTicket(resolver, -1, 0);
				return;
			}
			catch (FormulaException)
			{
				e.Text = "[公式出错]";
				return;
			}
		}
		e.Text = cell.Text;
		styleNew.BackColor = cell.BackColor;
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		for (int i = 0; i < Ticket.GetRowsCount(); i++)
		{
			int j;
			for (j = 0; j < Ticket.GetColumnsCount(); j++)
			{
				TicketMerge ticketMerge = Ticket.Merges.FirstOrDefault((TicketMerge m) => m.Contains(i, j));
				TicketDesignCellVM cell = Ticket.GetCell(i, j);
				Rectangle cellRect = _grid.GetCellRect(i + _grid.Rows.Fixed, j + _grid.Cols.Fixed);
				cellRect.Offset(-1, -1);
				if (cell.Top.Width > 0 && (ticketMerge == null || i == ticketMerge.TopRow))
				{
					using Pen pen = new Pen(Color.Black, cell.Top.Width);
					e.Graphics.DrawLine(pen, cellRect.Left, cellRect.Top, cellRect.Right, cellRect.Top);
				}
				if (cell.Right.Width > 0 && (ticketMerge == null || j == ticketMerge.RightColumn))
				{
					using Pen pen2 = new Pen(Color.Black, cell.Right.Width);
					e.Graphics.DrawLine(pen2, cellRect.Right, cellRect.Top, cellRect.Right, cellRect.Bottom);
				}
				if (cell.Bottom.Width > 0 && (ticketMerge == null || i == ticketMerge.BottomRow))
				{
					using Pen pen3 = new Pen(Color.Black, cell.Bottom.Width);
					e.Graphics.DrawLine(pen3, cellRect.Left, cellRect.Bottom, cellRect.Right, cellRect.Bottom);
				}
				if (cell.Left.Width > 0 && (ticketMerge == null || j == ticketMerge.LeftColumn))
				{
					using Pen pen4 = new Pen(Color.Black, cell.Left.Width);
					e.Graphics.DrawLine(pen4, cellRect.Left, cellRect.Top, cellRect.Left, cellRect.Bottom);
				}
			}
		}
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		if (_grid.HitTest().Type == HitTestTypeEnum.Cell)
		{
			_grid.Cursor = TableEditor.CursorTable;
		}
		else
		{
			_grid.Cursor = null;
		}
	}

	private void _tab_SelectedTabChanged(object sender, EventArgs e)
	{
		PopulateSheet();
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.OK;
	}

	private void PopulateSheet()
	{
		XLSheet sheet = _xl.Sheets[_tab.SelectedIndex];
		CreateDesignVM(sheet);
		PopulateVm();
	}

	private void Populate()
	{
		foreach (XLSheet item in (IEnumerable)_xl.Sheets)
		{
			_tab.TabPages.Add(new C1DockingTabPage
			{
				Text = item.Name
			});
		}
		PopulateSheet();
	}

	private void PopulateVm()
	{
		_grid.BeginUpdate();
		int rowsCount = Ticket.GetRowsCount();
		int columnsCount = Ticket.GetColumnsCount();
		_grid.BodyRowsCount = rowsCount;
		_grid.BodyColsCount = columnsCount;
		for (int i = 0; i < rowsCount; i++)
		{
			_grid.BodyGetRow(i).Height = Ticket.GetRow(i).Height;
		}
		for (int j = 0; j < columnsCount; j++)
		{
			_grid.BodyGetCol(j).Width = Ticket.GetColumn(j).Width;
		}
		PopulateMerge();
		_grid.EndUpdate();
	}

	private void PopulateMerge()
	{
		_grid.MergedRanges.Clear();
		foreach (TicketMerge merge in Ticket.Merges)
		{
			_grid.BodyAddMergedRange(merge.TopRow, merge.LeftColumn, merge.BottomRow, merge.RightColumn);
		}
	}

	private void CreateDesignVM(XLSheet sheet)
	{
		Ticket = new TicketDesignTableVM();
		Ticket.AppendColumns(C1XLBookEx.GetValidColCount(sheet));
		Ticket.AppendRows(C1XLBookEx.GetValidRowCount(sheet));
		for (int i = 0; i < Ticket.GetColumnsCount(); i++)
		{
			XLColumn c = sheet.Columns[i];
			TicketDesignColumnVM column = Ticket.GetColumn(i);
			column.Width = C1XLBookEx.ColumnWidthToPixel(c);
		}
		for (int j = 0; j < Ticket.GetRowsCount(); j++)
		{
			XLRow r = sheet.Rows[j];
			TicketDesignRowVM row = Ticket.GetRow(j);
			row.Height = C1XLBookEx.RowHeightToPixel(r);
			for (int k = 0; k < Ticket.GetColumnsCount(); k++)
			{
				XLCell xLCell = sheet[j, k];
				TicketDesignCellVM cell = Ticket.GetCell(j, k);
				_xl.PopulateTicketDesignCellStyle(xLCell.Style, cell);
				if (xLCell.Formula == "")
				{
					cell.Text = xLCell.Text;
				}
			}
		}
		if (Ticket.Rows.Count <= 0 || Ticket.Columns.Count <= 0)
		{
			return;
		}
		int val = Ticket.Rows.Count - 1;
		int val2 = Ticket.Columns.Count - 1;
		foreach (XLCellRange item2 in (IEnumerable)sheet.MergedCells)
		{
			int num = Math.Min(val, item2.RowFrom);
			int num2 = Math.Min(val, item2.RowTo);
			int num3 = Math.Min(val2, item2.ColumnFrom);
			int num4 = Math.Min(val2, item2.ColumnTo);
			if (num != num2 || num3 != num4)
			{
				TicketMerge item = new TicketMerge
				{
					TopRow = num,
					LeftColumn = num3,
					BottomRow = num2,
					RightColumn = num4
				};
				Ticket.Merges.Add(item);
			}
		}
	}
}
