using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1Sizer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class FormAdvancedPaste
{
	private const int MAXROWCOUNT = 5;

	private readonly C1Sizer _szMain;

	private readonly Label _lbl1;

	private readonly C1FlexGridEx _grid1;

	private readonly Label _lbl2;

	private readonly C1FlexGridEx _grid2;

	private readonly Label _lblResult;

	private readonly RichTextBoxEx _rtbResult;

	private readonly C1Sizer _szButtons;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	private readonly FlickerManager _flickerManager;

	private readonly Brush _fillBrushRed;

	private readonly Brush _fillBrushBlue;

	private readonly Brush _coverBrushRed;

	private readonly Brush _coverBrushBlue;

	private readonly Brush _coverBrushMagenta;

	private readonly Pen _matchPen;

	private readonly Pen _flowPen;

	private bool _isInitializing = true;

	private bool _mouseIn1;

	private bool _mouseIn2;

	private Auditai.Model.Column _srcMatchCol;

	private Auditai.Model.Column _dstMatchCol;

	public C1RibbonForm Form { get; }

	public Auditai.Model.Table SrcTable { get; set; }

	public Auditai.Model.Table DstTable { get; set; }

	public Auditai.Model.Column SrcCol { get; set; }

	public Auditai.Model.Column DstCol { get; set; }

	public string FunctionName { get; set; }

	public FormAdvancedPaste()
	{
		Form = FormFactory.Create();
		Form.Size = new Size(800, 750);
		Form.ShowInTaskbar = false;
		Form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Paste);
		Form.Shown += Form_Shown;
		Form.FormClosing += Form_FormClosing;
		_szMain = new C1Sizer
		{
			Dock = DockStyle.Fill,
			SplitterWidth = 0
		};
		_szMain.Padding = Padding.Empty;
		_szMain.Grid.Columns.Count = 1;
		_szMain.Grid.Rows.Count = 7;
		_szMain.Grid.Rows.SetSizes(new int[7] { 40, 1, 40, 1, 40, 80, 60 });
		_szMain.Grid.Rows.SetFixed(0, 2, 4, 5, 6);
		Form.Controls.Add(_szMain);
		_lbl1 = new Label
		{
			Text = "_lbl1",
			TextAlign = ContentAlignment.MiddleCenter
		};
		_szMain.AddControl(_lbl1, 0, 0);
		_grid1 = new C1FlexGridEx
		{
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle,
			AllowEditing = false,
			AllowSorting = AllowSortingEnum.None,
			AllowMergingFixed = AllowMergingEnum.Custom,
			AllowDragging = AllowDraggingEnum.None,
			HighLight = HighLightEnum.Never,
			FocusRect = FocusRectEnum.None
		};
		_grid1.Rows.DefaultSize = 30;
		_grid1.OwnerDrawCell += _grid1_OwnerDrawCell;
		_grid1.BodyOwnerDrawCell += _grid1_BodyOwnerDrawCell;
		_grid1.MouseMove += _grid1_MouseMove;
		_grid1.Paint += _grid1_Paint;
		_grid1.BodySelectionChanged += _grid1_BodySelectionChanged;
		_grid1.MouseEnter += _grid1_MouseEnter;
		_grid1.MouseLeave += _grid1_MouseLeave;
		_grid1.BeforeSelChange += _grid1_BeforeSelChange;
		_szMain.AddControl(_grid1, 1, 0);
		_lbl2 = new Label
		{
			Text = "_lbl2",
			TextAlign = ContentAlignment.MiddleCenter
		};
		_szMain.AddControl(_lbl2, 2, 0);
		_grid2 = new C1FlexGridEx
		{
			SelectionMode = SelectionModeEnum.Column,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle,
			AllowEditing = false,
			AllowSorting = AllowSortingEnum.None,
			AllowMergingFixed = AllowMergingEnum.Custom,
			AllowDragging = AllowDraggingEnum.None,
			HighLight = HighLightEnum.Never,
			FocusRect = FocusRectEnum.None
		};
		_grid2.Rows.DefaultSize = 30;
		_grid2.OwnerDrawCell += _grid2_OwnerDrawCell;
		_grid2.BodyOwnerDrawCell += _grid2_BodyOwnerDrawCell;
		_grid2.MouseMove += _grid2_MouseMove;
		_grid2.Paint += _grid2_Paint;
		_grid2.BodySelectionChanged += _grid2_BodySelectionChanged;
		_grid2.MouseEnter += _grid2_MouseEnter;
		_grid2.MouseLeave += _grid2_MouseLeave;
		_grid2.BeforeSelChange += _grid2_BeforeSelChange;
		_szMain.AddControl(_grid2, 3, 0);
		_lblResult = new Label
		{
			Text = "目标列生成的列公式：",
			TextAlign = ContentAlignment.MiddleLeft
		};
		_szMain.AddControl(_lblResult, 4, 0);
		_rtbResult = new RichTextBoxEx
		{
			ReadOnly = true,
			BorderStyle = BorderStyle.None
		};
		_szMain.AddControl(_rtbResult, 5, 0);
		_szButtons = new C1Sizer();
		_szButtons.Grid.Rows.Count = 3;
		_szButtons.Grid.Rows.SetSizes(new int[3] { 5, 1, 5 });
		_szButtons.Grid.Rows.SetFixed(0, 2);
		_szButtons.Grid.Columns.Count = 5;
		_szButtons.Grid.Columns.SetSizes(new int[5] { 1, 80, 10, 80, 10 });
		_szButtons.Grid.Columns.SetFixed(1, 2, 3, 4);
		_szMain.AddControl(_szButtons, 6, 0);
		_btnOk = new C1Button
		{
			Text = "确定"
		};
		_btnOk.Click += _btnOk_Click;
		_szButtons.AddControl(_btnOk, 1, 1);
		_btnCancel = new C1Button
		{
			Text = "取消"
		};
		_btnCancel.Click += _btnCancel_Click;
		_szButtons.AddControl(_btnCancel, 1, 3);
		_flickerManager = new FlickerManager();
		LabelFlickerProxy labelFlickerProxy = new LabelFlickerProxy(_lbl1);
		labelFlickerProxy.SetTimer(SecondTrigger.Trigger);
		_flickerManager.Add(_lbl1, labelFlickerProxy);
		LabelFlickerProxy labelFlickerProxy2 = new LabelFlickerProxy(_lbl2);
		labelFlickerProxy2.SetTimer(SecondTrigger.Trigger);
		_flickerManager.Add(_lbl2, labelFlickerProxy2);
		_fillBrushRed = new SolidBrush(Color.FromArgb(20, Color.Red));
		_fillBrushBlue = new SolidBrush(Color.FromArgb(20, Color.Blue));
		_coverBrushRed = new SolidBrush(Color.FromArgb(200, 255, 220, 220));
		_coverBrushBlue = new SolidBrush(Color.FromArgb(200, 220, 220, 255));
		_coverBrushMagenta = new SolidBrush(Color.FromArgb(200, 255, 220, 255));
		_lbl2.Paint += _lbl2_Paint;
		_matchPen = new Pen(Color.Red, 1.5f)
		{
			StartCap = LineCap.RoundAnchor,
			EndCap = LineCap.RoundAnchor
		};
		_flowPen = new Pen(Color.Magenta, 2f)
		{
			CustomEndCap = new AdjustableArrowCap(4f, 4f, isFilled: true)
		};
	}

	private void _lbl2_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle cellRect = _grid1.GetCellRect(0, SrcCol.Index + _grid1.Cols.Fixed);
		int x = cellRect.Left + cellRect.Width / 2;
		Rectangle cellRect2 = _grid2.GetCellRect(0, DstCol.Index + _grid2.Cols.Fixed);
		int x2 = cellRect2.Left + cellRect2.Width / 2;
		e.Graphics.DrawLine(_flowPen, x, 0, x2, _lbl2.Height - 1);
		if (_srcMatchCol != null && _dstMatchCol != null)
		{
			cellRect = _grid1.GetCellRect(0, _grid1.Col);
			x = cellRect.Left + cellRect.Width / 2;
			cellRect2 = _grid2.GetCellRect(0, _grid2.Col);
			x2 = cellRect2.Left + cellRect2.Width / 2;
			e.Graphics.DrawLine(_matchPen, x, 1, x2, _lbl2.Height - 2);
		}
	}

	private void _grid2_BeforeSelChange(object sender, RangeEventArgs e)
	{
		if (e.NewRange.ContainsCol(DstCol.Index + _grid2.Cols.Fixed))
		{
			e.Cancel = true;
		}
	}

	private void _grid1_BeforeSelChange(object sender, RangeEventArgs e)
	{
		if (e.NewRange.ContainsCol(SrcCol.Index + _grid1.Cols.Fixed))
		{
			e.Cancel = true;
		}
	}

	private void Form_FormClosing(object sender, FormClosingEventArgs e)
	{
		_flickerManager.Remove(_lbl1);
	}

	private void _grid2_MouseLeave(object sender, EventArgs e)
	{
		_mouseIn2 = false;
	}

	private void _grid2_MouseEnter(object sender, EventArgs e)
	{
		_mouseIn2 = true;
		_grid2.Invalidate();
	}

	private void _grid1_MouseEnter(object sender, EventArgs e)
	{
		_mouseIn1 = true;
	}

	private void _grid1_MouseLeave(object sender, EventArgs e)
	{
		_mouseIn1 = false;
		_grid1.Invalidate();
	}

	private void _grid2_BodySelectionChanged(object sender, EventArgs e)
	{
		if (_grid2.BodyCol >= 0)
		{
			_dstMatchCol = DstTable.Columns[_grid2.BodyCol];
			_grid2.Invalidate();
		}
		else
		{
			_dstMatchCol = null;
		}
		UpdateState();
	}

	private void _grid1_BodySelectionChanged(object sender, EventArgs e)
	{
		if (_grid1.BodyCol >= 0)
		{
			_srcMatchCol = SrcTable.Columns[_grid1.BodyCol];
			_grid1.Invalidate();
		}
		else
		{
			_srcMatchCol = null;
		}
		UpdateState();
	}

	private void _grid2_Paint(object sender, PaintEventArgs e)
	{
		_lbl2.Invalidate();
		if (_mouseIn2)
		{
			PaintMouseCol(_grid2, e, Pens.Red, _fillBrushRed, DstCol.Index + _grid2.Cols.Fixed);
		}
		if (_dstMatchCol != null)
		{
			PaintColCover(_grid2, e, "匹配列", _dstMatchCol.Index + _grid2.Cols.Fixed, Brushes.Red, _coverBrushRed, Pens.Red);
		}
		PaintColCover(_grid2, e, "目标列", DstCol.Index + _grid2.Cols.Fixed, Brushes.Magenta, _coverBrushMagenta, Pens.Magenta);
	}

	private void _grid2_MouseMove(object sender, MouseEventArgs e)
	{
		MouseMove(_grid2, e);
	}

	private void _grid1_MouseMove(object sender, MouseEventArgs e)
	{
		MouseMove(_grid1, e);
	}

	private void MouseMove(C1FlexGridEx grid, MouseEventArgs e)
	{
		grid.Invalidate();
	}

	private void _grid1_Paint(object sender, PaintEventArgs e)
	{
		if (_mouseIn1)
		{
			PaintMouseCol(_grid1, e, Pens.Blue, _fillBrushBlue, SrcCol.Index + _grid1.Cols.Fixed);
		}
		if (_srcMatchCol != null)
		{
			PaintColCover(_grid1, e, "匹配列", _srcMatchCol.Index + _grid1.Cols.Fixed, Brushes.Blue, _coverBrushBlue, Pens.Blue);
		}
		PaintColCover(_grid1, e, "来源列", SrcCol.Index + _grid1.Cols.Fixed, Brushes.Magenta, _coverBrushMagenta, Pens.Magenta);
	}

	private void PaintMouseCol(C1FlexGridEx grid, PaintEventArgs e, Pen pen, Brush brush, int except)
	{
		int mouseCol = grid.MouseCol;
		if (mouseCol >= grid.Cols.Fixed && mouseCol < grid.Cols.Count && mouseCol != except)
		{
			Rectangle columnRectUnclipped = grid.GetColumnRectUnclipped(mouseCol);
			columnRectUnclipped.Offset(-1, -1);
			e.Graphics.FillRectangle(brush, columnRectUnclipped);
		}
	}

	private void PaintColCover(C1FlexGridEx grid, PaintEventArgs e, string text, int col, Brush fore, Brush foreFill, Pen forePen)
	{
		Rectangle columnRectUnclipped = grid.GetColumnRectUnclipped(col);
		columnRectUnclipped.Offset(-1, -1);
		e.Graphics.DrawRectangle(forePen, columnRectUnclipped);
		columnRectUnclipped = new Rectangle(columnRectUnclipped.X + 1, columnRectUnclipped.Y + 1, columnRectUnclipped.Width - 1, columnRectUnclipped.Height - 1);
		e.Graphics.FillRectangle(foreFill, columnRectUnclipped);
		e.Graphics.DrawString(text, Form.Font, fore, columnRectUnclipped, new StringFormat
		{
			Alignment = StringAlignment.Center,
			FormatFlags = StringFormatFlags.DirectionVertical,
			LineAlignment = StringAlignment.Center
		});
	}

	private void _grid1_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		DrawRowNumber(SrcTable, _grid1, e);
	}

	private void _grid2_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		DrawRowNumber(DstTable, _grid2, e);
	}

	private void DrawRowNumber(Auditai.Model.Table table, C1FlexGridEx grid, OwnerDrawCellEventArgs e)
	{
		if (HasEllipseRow(table) && e.Row - grid.Rows.Fixed == 5)
		{
			e.Text = "...";
		}
		else if (e.Col == 0 && e.Row >= grid.Rows.Fixed)
		{
			e.Text = (e.Row - grid.Rows.Fixed + 1).ToString();
		}
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		Form.DialogResult = DialogResult.Cancel;
	}

	private void Form_Shown(object sender, EventArgs e)
	{
		Theme.SetCurrentTree(Form);
		_rtbResult.BackColor = Color.White;
		_grid1.Styles.Focus.DefinedElements &= ~(StyleElementFlags.BackColor | StyleElementFlags.ForeColor);
		_grid2.Styles.Focus.DefinedElements &= ~(StyleElementFlags.BackColor | StyleElementFlags.ForeColor);
		UpdateState();
	}

	private void _grid2_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		DrawCell(DstTable, _grid2, e);
	}

	private void _grid1_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		DrawCell(SrcTable, _grid1, e);
	}

	private void DrawCell(Auditai.Model.Table table, C1FlexGridEx grid, OwnerDrawCellEventArgs e)
	{
		if (HasEllipseRow(table) && e.Row == 5)
		{
			e.Text = "...";
			return;
		}
		Auditai.Model.Cell cell = table[e.Row, e.Col];
		C1.Win.C1FlexGrid.CellStyle styleNew = grid.BodyGetCell(e.Row, e.Col).StyleNew;
		styleNew.DataType = cell.DisplayDataType;
		DataFormat displayFormat = cell.DisplayFormat;
		if (displayFormat.FormatType == DataFormatType.BoolCheckBox)
		{
			e.Text = string.Empty;
			e.Image = (cell.Value.Equals(true) ? grid.Glyphs[GlyphEnum.Checked] : grid.Glyphs[GlyphEnum.Unchecked]);
			styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.DisplayAlign);
		}
		else
		{
			e.Image = null;
			e.Text = cell.GetDisplayValue();
		}
		switch (displayFormat.FormatType)
		{
		case DataFormatType.General:
		case DataFormatType.Number:
		case DataFormatType.Percentage:
		case DataFormatType.NumDollar:
		case DataFormatType.NumRmb:
		case DataFormatType.DateSlash:
		case DataFormatType.DateDash:
		case DataFormatType.DateChinese:
		case DataFormatType.Comma:
		case DataFormatType.BoolCheckBox:
		case DataFormatType.DateYearMonthChinese:
		case DataFormatType.DateYearMonthDash:
		case DataFormatType.DateYearMonthSlash:
		case DataFormatType.DateYearMonthDot:
		case DataFormatType.DateDot:
			styleNew.DataMap = null;
			break;
		case DataFormatType.BoolYesNo:
		case DataFormatType.BoolRightWrong:
		case DataFormatType.BoolTickCross:
			styleNew.DataMap = cell.DisplayFormat.GetFormatDictForBool();
			break;
		case DataFormatType.ComboList:
			styleNew.DataMap = null;
			break;
		}
		styleNew.WordWrap = cell.Value is string;
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		Form.DialogResult = DialogResult.OK;
	}

	public DialogResult ShowDialog()
	{
		Populate();
		return Form.ShowDialog();
	}

	private void UpdateState()
	{
		if (!_isInitializing)
		{
			if (_srcMatchCol == null || _dstMatchCol == null)
			{
				_btnOk.Enabled = false;
			}
			else
			{
				_btnOk.Enabled = true;
				_rtbResult.Text = GetRtbText();
				SyntaxHighlight();
			}
			if (_srcMatchCol == null)
			{
				_lbl1.Text = "请选择《" + SrcTable.TreeNode.Name + "》的匹配列";
				_lbl1.ForeColor = Color.Red;
				_flickerManager.Start(_lbl1);
			}
			else
			{
				_flickerManager.Stop(_lbl1);
				_lbl1.Text = "已选择《" + SrcTable.TreeNode.Name + "》的匹配列为 [" + _srcMatchCol.Caption + "]";
				_lbl1.ForeColor = Color.Green;
			}
			if (_dstMatchCol == null)
			{
				_lbl2.Text = "请选择《" + DstTable.TreeNode.Name + "》的匹配列";
				_lbl2.ForeColor = Color.Red;
				_flickerManager.Start(_lbl2);
				return;
			}
			_flickerManager.Stop(_lbl2);
			_lbl2.Text = "已选择《" + DstTable.TreeNode.Name + "》的匹配列为 [" + _dstMatchCol.Caption + "]";
			_lbl2.ForeColor = Color.Green;
		}
	}

	private string GetRtbText()
	{
		FormulaEvaluator formulaEvaluator = new FormulaEvaluator(GetResultFormula());
		return formulaEvaluator.GetDisplayString(new FormulaReferenceModelResolver(DstTable.Project), DstTable);
	}

	public string GetResultFormula()
	{
		if (!DstCol.HasFormula || _srcMatchCol == null || _dstMatchCol == null)
		{
			return $"{FunctionName}([2:{SrcCol.Table.Id}:{_srcMatchCol.Id}]=[4:{DstCol.Table.Id}:{_dstMatchCol.Id}],[2:{SrcCol.Table.Id}:{SrcCol.Id}])";
		}
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(DstCol.Formula);
			return formulaEvaluator.PasteVLookUp(FunctionName, SrcCol, DstCol, _srcMatchCol, _dstMatchCol);
		}
		catch (FormulaException)
		{
			return $"{FunctionName}([2:{SrcCol.Table.Id}:{_srcMatchCol.Id}]=[4:{DstCol.Table.Id}:{_dstMatchCol.Id}],[2:{SrcCol.Table.Id}:{SrcCol.Id}])";
		}
	}

	private void SyntaxHighlight()
	{
		_rtbResult.SuspendDrawing();
		int selectionStart = _rtbResult.SelectionStart;
		int selectionLength = _rtbResult.SelectionLength;
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(_rtbResult.Text);
			_rtbResult.SelectAll();
			_rtbResult.SelectionBackColor = Color.Transparent;
			_rtbResult.SelectionColor = Color.Black;
			foreach (Tuple<int, int, Color> tokenColorInterval in formulaDisplay.GetTokenColorIntervals())
			{
				_rtbResult.Select(tokenColorInterval.Item1, tokenColorInterval.Item2);
				_rtbResult.SelectionColor = tokenColorInterval.Item3;
			}
			Tuple<List<FormulaDisplayRef>, Color> references = formulaDisplay.GetReferences(Program.MainForm.FormulaEditor.Context);
			foreach (FormulaDisplayRef item in references.Item1)
			{
				foreach (Tuple<int, int> interval in item.Intervals)
				{
					_rtbResult.Select(interval.Item1, interval.Item2);
					_rtbResult.SelectionColor = item.Color;
				}
			}
		}
		catch (FormulaException)
		{
		}
		finally
		{
			_rtbResult.Select(selectionStart, selectionLength);
			_rtbResult.ResumeDrawing();
		}
	}

	public void GuessMatchCols()
	{
		foreach (Auditai.Model.Column column in DstTable.Columns)
		{
			if (column != DstCol && column.HasFormula)
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(column.Formula);
				formulaEvaluator.Env = new FormulaEvaluationEnvironment
				{
					Resolver = new FormulaReferenceModelResolver(DstTable.Project),
					RefManager = DstTable.Project.DataReferenceManager,
					RefEvalContext = new DataReferenceEvaluationContext
					{
						Project = DstTable.Project,
						CurrentTreeNode = DstTable.TreeNode
					}
				};
				Auditai.Model.Column fillSourceCol = formulaEvaluator.GetFillSourceCol(SrcTable);
				if (fillSourceCol != null)
				{
					_srcMatchCol = fillSourceCol;
					_dstMatchCol = column;
					break;
				}
			}
		}
	}

	public void Populate()
	{
		PopulateTable(SrcTable, _grid1, _srcMatchCol);
		PopulateTable(DstTable, _grid2, _dstMatchCol);
		_isInitializing = false;
	}

	private void PopulateTable(Auditai.Model.Table table, C1FlexGridEx grid, Auditai.Model.Column column)
	{
		grid.BeginUpdate();
		grid.Cols[0].StyleNew.TextAlign = TextAlignEnum.CenterCenter;
		grid.BodyRowsCount = GetRowCount(table);
		if (HasEllipseRow(table))
		{
			grid.BodyRowsCount++;
		}
		grid.BodyColsCount = table.Columns.Count;
		grid.Cols.Frozen = table.FrozenCols;
		PopulateColumns(table, grid);
		grid.AutoSizeCols();
		if (grid.Cols[0].Width < 56)
		{
			grid.Cols[0].Width = 56;
		}
		if (column == null)
		{
			grid.Select(1, -1);
		}
		else
		{
			grid.BodySelect(0, column.Index, GetRowCount(table) - 1, column.Index);
		}
		grid.EndUpdate();
	}

	public void PopulateColumns(Auditai.Model.Table table, C1FlexGridEx grid)
	{
		for (int num = grid.MergedRanges.Count - 1; num >= 0; num--)
		{
			if (grid.MergedRanges[num].TopRow < grid.Rows.Fixed)
			{
				grid.MergedRanges.RemoveAt(num);
			}
		}
		int numCaptionRows = table.GetNumCaptionRows();
		grid.Rows.RemoveRange(0, grid.Rows.Fixed);
		grid.Rows.InsertRange(0, numCaptionRows);
		grid.Rows.Fixed = numCaptionRows;
		for (int i = 0; i < numCaptionRows; i++)
		{
			grid.Rows[i].StyleNew.WordWrap = true;
		}
		foreach (Auditai.Model.Column column in table.Columns)
		{
			string[] array = column.CaptionDisplay.Split('_');
			for (int j = 0; j < array.Length; j++)
			{
				grid.SetData(j, column.Index + grid.Cols.Fixed, array[j]);
			}
		}
		List<Auditai.DTO.CellRange> mergeInfo = table.GetMergeInfo(visibleOnly: false);
		foreach (Auditai.DTO.CellRange item in mergeInfo)
		{
			grid.MergedRanges.Add(item.r1, item.c1 + grid.Cols.Fixed, item.r2, item.c2 + grid.Cols.Fixed);
		}
	}

	private int GetRowCount(Auditai.Model.Table table)
	{
		return Math.Min(table.Rows.Count, 5);
	}

	private bool HasEllipseRow(Auditai.Model.Table table)
	{
		return table.Rows.Count > 5;
	}
}
