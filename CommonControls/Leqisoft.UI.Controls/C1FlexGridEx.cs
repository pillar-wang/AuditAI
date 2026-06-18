using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using Leqisoft.Model;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class C1FlexGridEx : C1FlexGrid
{
	public struct COMPOSITIONFORM
	{
		public uint dwStyle;

		public Point ptCurrentPos;

		public RECT rcArea;
	}

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class LOGFONT
	{
		public int lfHeight;

		public int lfWidth;

		public int lfEscapement;

		public int lfOrientation;

		public FontWeight lfWeight;

		[MarshalAs(UnmanagedType.U1)]
		public bool lfItalic;

		[MarshalAs(UnmanagedType.U1)]
		public bool lfUnderline;

		[MarshalAs(UnmanagedType.U1)]
		public bool lfStrikeOut;

		public FontCharSet lfCharSet;

		public FontPrecision lfOutPrecision;

		public FontClipPrecision lfClipPrecision;

		public FontQuality lfQuality;

		public FontPitchAndFamily lfPitchAndFamily;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string lfFaceName;
	}

	public enum FontWeight
	{
		FW_DONTCARE = 0,
		FW_THIN = 100,
		FW_EXTRALIGHT = 200,
		FW_LIGHT = 300,
		FW_NORMAL = 400,
		FW_MEDIUM = 500,
		FW_SEMIBOLD = 600,
		FW_BOLD = 700,
		FW_EXTRABOLD = 800,
		FW_HEAVY = 900
	}

	public enum FontCharSet : byte
	{
		ANSI_CHARSET = 0,
		DEFAULT_CHARSET = 1,
		SYMBOL_CHARSET = 2,
		SHIFTJIS_CHARSET = 128,
		HANGEUL_CHARSET = 129,
		HANGUL_CHARSET = 129,
		GB2312_CHARSET = 134,
		CHINESEBIG5_CHARSET = 136,
		OEM_CHARSET = byte.MaxValue,
		JOHAB_CHARSET = 130,
		HEBREW_CHARSET = 177,
		ARABIC_CHARSET = 178,
		GREEK_CHARSET = 161,
		TURKISH_CHARSET = 162,
		VIETNAMESE_CHARSET = 163,
		THAI_CHARSET = 222,
		EASTEUROPE_CHARSET = 238,
		RUSSIAN_CHARSET = 204,
		MAC_CHARSET = 77,
		BALTIC_CHARSET = 186
	}

	public enum FontPrecision : byte
	{
		OUT_DEFAULT_PRECIS,
		OUT_STRING_PRECIS,
		OUT_CHARACTER_PRECIS,
		OUT_STROKE_PRECIS,
		OUT_TT_PRECIS,
		OUT_DEVICE_PRECIS,
		OUT_RASTER_PRECIS,
		OUT_TT_ONLY_PRECIS,
		OUT_OUTLINE_PRECIS,
		OUT_SCREEN_OUTLINE_PRECIS,
		OUT_PS_ONLY_PRECIS
	}

	public enum FontClipPrecision : byte
	{
		CLIP_DEFAULT_PRECIS = 0,
		CLIP_CHARACTER_PRECIS = 1,
		CLIP_STROKE_PRECIS = 2,
		CLIP_MASK = 15,
		CLIP_LH_ANGLES = 16,
		CLIP_TT_ALWAYS = 32,
		CLIP_DFA_DISABLE = 64,
		CLIP_EMBEDDED = 128
	}

	public enum FontQuality : byte
	{
		DEFAULT_QUALITY,
		DRAFT_QUALITY,
		PROOF_QUALITY,
		NONANTIALIASED_QUALITY,
		ANTIALIASED_QUALITY,
		CLEARTYPE_QUALITY,
		CLEARTYPE_NATURAL_QUALITY
	}

	[Flags]
	public enum FontPitchAndFamily : byte
	{
		DEFAULT_PITCH = 0,
		FIXED_PITCH = 1,
		VARIABLE_PITCH = 2,
		FF_DONTCARE = 0,
		FF_ROMAN = 0x10,
		FF_SWISS = 0x20,
		FF_MODERN = 0x30,
		FF_SCRIPT = 0x40,
		FF_DECORATIVE = 0x50
	}

	private const int WM_IME_COMPOSITION = 271;

	private const int WM_KEYDOWN = 256;

	private const int WM_IME_CHAR = 646;

	private const int CFS_POINT = 2;

	private const int GCS_COMPSTR = 8;

	private bool _raiseOnGridChanged = true;

	private bool _isMouseInFilterImageRect;

	private bool _isResizingColumnMouseDown;

	private bool _isResizingRowMouseDown;

	private Point _mouseDownPosition;

	private bool _noreentry;

	public bool IsResizingColumn { get; private set; }

	public bool IsResizingRow { get; private set; }

	public FilterManager FilterManager { get; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyRow => Selection.TopRow - base.Rows.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyRowSel => Selection.BottomRow - base.Rows.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyCol => Selection.LeftCol - base.Cols.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyColSel => Selection.RightCol - base.Cols.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyRowsCount
	{
		get
		{
			return base.Rows.Count - base.Rows.Fixed;
		}
		set
		{
			base.Rows.Count = base.Rows.Fixed + value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyColsCount
	{
		get
		{
			return base.Cols.Count - base.Cols.Fixed;
		}
		set
		{
			base.Cols.Count = base.Cols.Fixed + value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyTopRow => base.TopRow - base.Rows.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyBottomRow => base.BottomRow - base.Rows.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyLeftCol => base.LeftCol - base.Cols.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int BodyRightCol => base.RightCol - base.Cols.Fixed;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SelectionType SelectionType { get; private set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public CellRange BodySelection => ToBodyRange(Selection);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsEntireRowSelected
	{
		get
		{
			if (base.Cols.Fixed == 0)
			{
				return false;
			}
			if (Selection.LeftCol == base.Cols.Fixed)
			{
				return Selection.RightCol == base.Cols.Count - 1;
			}
			return false;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsEntireColumnSelected
	{
		get
		{
			if (base.Rows.Fixed == 0)
			{
				return false;
			}
			if (Selection.TopRow == base.Rows.Fixed)
			{
				return Selection.BottomRow == base.Rows.Count - 1;
			}
			return false;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public List<int> SkipRows { get; set; } = new List<int>();


	public event RowColEventHandler BodyBeforeEdit;

	public event RowColEventHandler BodyAfterEdit;

	public event RowColEventHandler BodyAfterResizeRow;

	public event RowColEventHandler BodyAfterResizeColumn;

	public event RowColEventHandler BodyCellChanged;

	public event EventHandler BodySelectionChanged;

	public event RangeEventHandler BodyAfterScroll;

	public event OwnerDrawCellEventHandler BodyOwnerDrawCell;

	public event RangeEventHandler BodyAfterRowColChange;

	public event ValidateEditEventHandler BodyValidateEdit;

	public event RowColEventHandler BodyStartEdit;

	public event RowColEventHandler BodySetupEditor;

	public event EventHandler<HandledMouseEventArgs> PreviewMouseDown;

	public event PaintEventHandler PaintBackground;

	[DllImport("imm32")]
	public static extern IntPtr ImmGetContext(IntPtr hwnd);

	[DllImport("imm32")]
	public static extern bool ImmReleaseContext(IntPtr hwnd, IntPtr himc);

	[DllImport("imm32")]
	public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM lpCompForm);

	[DllImport("imm32", CharSet = CharSet.Unicode)]
	public static extern bool ImmSetCompositionFont(IntPtr hIMC, IntPtr logfont);

	[DllImport("imm32", CharSet = CharSet.Unicode)]
	public static extern int ImmGetCompositionString(IntPtr himc, int dword, byte[] buf, int bufLen);

	[DllImport("user32", CharSet = CharSet.Unicode)]
	private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

	public void QuickAppendRows(int count)
	{
		base.Rows.Count += count;
	}

	public void QuickInsertRows(int index, int count)
	{
		base.Rows.Count += count;
		BodyMoveRows(BodyRowsCount - count, count, index);
	}

	public C1FlexGridEx()
	{
		base.DrawMode = DrawModeEnum.OwnerDraw;
		base.Glyphs[GlyphEnum.Checked] = Resources.Checked;
		base.Glyphs[GlyphEnum.Unchecked] = Resources.Unchecked;
		FilterManager = new FilterManager(this);
		FilterManager.Context = new GridFilterContext(this);
		base.FocusRect = FocusRectEnum.None;
	}

	protected override void OnBeforeSelChange(RangeEventArgs e)
	{
		if (!_noreentry)
		{
			bool flag = base.Rows.Fixed > 0 && e.NewRange.ContainsRow(base.Rows.Fixed) && e.NewRange.ContainsRow(base.Rows.Count - 1);
			bool flag2 = base.Cols.Fixed > 0 && e.NewRange.ContainsCol(base.Cols.Fixed) && e.NewRange.ContainsCol(base.Cols.Count - 1);
			if (!flag && !flag2)
			{
				CellRange cellRange = e.NewRange;
				foreach (CellRange item in (IEnumerable)base.MergedRanges)
				{
					if (RangeIntersects(cellRange, item))
					{
						e.Cancel = true;
						cellRange = RangeUnion(cellRange, item);
					}
				}
				if (e.Cancel)
				{
					_noreentry = true;
					try
					{
						Select(cellRange, show: true);
					}
					catch (IndexOutOfRangeException)
					{
					}
					_noreentry = false;
				}
			}
		}
		base.OnBeforeSelChange(e);
	}

	public void BodyInsertRows(int index, int count)
	{
		int count2 = base.Rows.Count;
		base.Rows.Count = count2 + count;
		_raiseOnGridChanged = false;
		base.Rows.MoveRange(count2, count, index + base.Rows.Fixed);
		_raiseOnGridChanged = true;
	}

	public void BodyRemoveRow(int index)
	{
		RemoveItem(index + base.Rows.Fixed);
	}

	public void BodyMoveRows(int index, int count, int indexNew)
	{
		base.Rows.MoveRange(index + base.Rows.Fixed, count, indexNew + base.Rows.Fixed);
	}

	public void BodyMoveColumns(int index, int count, int indexNew)
	{
		base.Cols.MoveRange(index + base.Cols.Fixed, count, indexNew + base.Cols.Fixed);
	}

	public void SafeSelect(int rowIndex, int colIndex, bool isToShow = true)
	{
		SafeSelect(rowIndex, colIndex, rowIndex, colIndex, isToShow);
	}

	public void SafeSelect(int topRow, int leftCol, int buttonRow, int rightCol, bool isToShow = true)
	{
		try
		{
			if (topRow < 0 && buttonRow < 0)
			{
				Select(-1, -1);
			}
			else if (base.Rows.Count != 0 && base.Cols.Count != 0)
			{
				int row = ((topRow >= 0) ? topRow : 0);
				int rowSel = ((buttonRow >= base.Rows.Count) ? (base.Rows.Count - 1) : buttonRow);
				int col = ((leftCol >= 0) ? leftCol : 0);
				int colSel = ((rightCol >= base.Cols.Count) ? (base.Cols.Count - 1) : rightCol);
				Select(row, col, rowSel, colSel, isToShow);
			}
		}
		catch (Exception)
		{
		}
	}

	public void BodyInsertCols(int index, int count)
	{
		_raiseOnGridChanged = false;
		base.Cols.InsertRange(index + base.Cols.Fixed, count);
		_raiseOnGridChanged = true;
	}

	public void BodyRemoveCol(int index)
	{
		base.Cols.Remove(index + base.Cols.Fixed);
	}

	public C1.Win.C1FlexGrid.Row BodyGetRow(int index)
	{
		return base.Rows[index + base.Rows.Fixed];
	}

	public C1.Win.C1FlexGrid.Column BodyGetCol(int index)
	{
		return base.Cols[index + base.Cols.Fixed];
	}

	public object BodyGetData(int row, int col)
	{
		return GetData(row + base.Rows.Fixed, col + base.Cols.Fixed) ?? string.Empty;
	}

	public bool BodySetData(int row, int col, object value)
	{
		return SetData(row + base.Rows.Fixed, col + base.Cols.Fixed, value);
	}

	public void BodySelect(int row, int col)
	{
		Select(row + base.Rows.Fixed, col + base.Cols.Fixed);
	}

	public void BodySelect(int row, int col, int rowSel, int colSel)
	{
		Select(row + base.Rows.Fixed, col + base.Cols.Fixed, rowSel + base.Rows.Fixed, colSel + base.Cols.Fixed);
	}

	public CellRange BodyGetCell(int row, int col)
	{
		return GetCellRange(row + base.Rows.Fixed, col + base.Cols.Fixed);
	}

	public void BodyAddMergedRange(int topRow, int leftCol, int bottomRow, int rightCol)
	{
		base.MergedRanges.Add(topRow + base.Rows.Fixed, leftCol + base.Cols.Fixed, bottomRow + base.Rows.Fixed, rightCol + base.Cols.Fixed);
	}

	public void BodyRemoveMergedRange(int topRow, int leftCol)
	{
		base.MergedRanges.Remove(GetMergedRange(topRow + base.Rows.Fixed, leftCol + base.Cols.Fixed));
	}

	public Rectangle GetCellRectUnclipped(int row, int col)
	{
		return GetCellRectDisplay(row, col, clipHorz: false, clipVert: false);
	}

	public Rectangle GetCellRangeRectUnclipped(int row1, int col1, int row2, int col2)
	{
		Rectangle cellRectUnclipped = GetCellRectUnclipped(row1, col1);
		Rectangle cellRectUnclipped2 = GetCellRectUnclipped(row2, col2);
		return new Rectangle(cellRectUnclipped.Left, cellRectUnclipped.Top, cellRectUnclipped2.Right - cellRectUnclipped.Left, cellRectUnclipped2.Bottom - cellRectUnclipped.Top);
	}

	public Rectangle GetColumnRectUnclipped(int col)
	{
		Rectangle cellRectDisplay = GetCellRectDisplay(base.Rows.Fixed, col);
		return new Rectangle(cellRectDisplay.Left, cellRectDisplay.Top, base.Cols[col].WidthDisplay, base.Rows[base.Rows.Count - 1].Bottom - cellRectDisplay.Top);
	}

	public void SetSubtreeVisible(Node n, bool v)
	{
		Node[] nodes = n.Nodes;
		for (int num = n.Children - 1; num >= 0; num--)
		{
			SetSubtreeVisible(nodes[num], v);
		}
		n.Row.Visible = v;
	}

	public void ExpandAll()
	{
		BeginUpdate();
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)base.Rows)
		{
			if (item.IsNode)
			{
				item.Node.Collapsed = false;
			}
		}
		EndUpdate();
	}

	public void CollapseAll()
	{
		BeginUpdate();
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)base.Rows)
		{
			if (item.IsNode)
			{
				item.Node.Collapsed = true;
			}
		}
		EndUpdate();
	}

	protected override void OnBeforeEdit(RowColEventArgs e)
	{
		if (!IsIndexOutOfRange(e.Row, e.Col))
		{
			CellRange mergedRange = GetMergedRange(e.Row, e.Col);
			if (mergedRange.TopRow >= base.Rows.Fixed && mergedRange.LeftCol >= base.Cols.Fixed)
			{
				RowColEventArgs rowColEventArgs = new RowColEventArgs(mergedRange.TopRow - base.Rows.Fixed, mergedRange.LeftCol - base.Cols.Fixed);
				this.BodyBeforeEdit?.Invoke(this, rowColEventArgs);
				e.Cancel = rowColEventArgs.Cancel;
			}
			base.OnBeforeEdit(e);
		}
	}

	protected override void OnAfterEdit(RowColEventArgs e)
	{
		if (!IsIndexOutOfRange(e.Row, e.Col))
		{
			base.OnAfterEdit(e);
			object obj = base[e.Row, e.Col];
			CellRange mergedRange = GetMergedRange(e.Row, e.Col);
			mergedRange.Data = string.Empty;
			if (obj is string text)
			{
				obj = text.Replace("\r\n", "\n").Replace("\r", "\n");
			}
			base[e.Row, e.Col] = obj;
			int num = e.Row - base.Rows.Fixed;
			int num2 = e.Col - base.Cols.Fixed;
			if (num >= 0 && num2 >= 0)
			{
				OnBodyAfterEdit(new RowColEventArgs(num, num2));
			}
		}
	}

	protected virtual void OnBodyAfterEdit(RowColEventArgs e)
	{
		this.BodyAfterEdit?.Invoke(this, e);
	}

	protected override void OnBeforeResizeRow(RowColEventArgs e)
	{
		base.OnBeforeResizeRow(e);
	}

	protected override void OnAfterResizeRow(RowColEventArgs e)
	{
		base.OnAfterResizeRow(e);
		int num = e.Row - base.Rows.Fixed;
		int col = e.Col - base.Cols.Fixed;
		if (num < 0)
		{
			return;
		}
		if (IsEntireRowSelected)
		{
			if (Selection.TopRow <= e.Row && e.Row <= Selection.BottomRow)
			{
				for (int i = Selection.TopRow; i <= Selection.BottomRow; i++)
				{
					OnBodyAfterResizeRow(new RowColEventArgs(i - base.Rows.Fixed, col));
				}
			}
			else
			{
				OnBodyAfterResizeRow(new RowColEventArgs(num, col));
			}
		}
		else
		{
			OnBodyAfterResizeRow(new RowColEventArgs(num, col));
		}
	}

	protected virtual void OnBodyAfterResizeRow(RowColEventArgs e)
	{
		this.BodyAfterResizeRow?.Invoke(this, e);
	}

	protected override void OnAfterResizeColumn(RowColEventArgs e)
	{
		base.OnAfterResizeColumn(e);
		int row = e.Row - base.Rows.Fixed;
		int num = e.Col - base.Cols.Fixed;
		if (num < 0)
		{
			return;
		}
		if (IsEntireColumnSelected)
		{
			if (Selection.LeftCol <= e.Col && e.Col <= Selection.RightCol)
			{
				for (int i = Selection.LeftCol; i <= Selection.RightCol; i++)
				{
					OnBodyAfterResizeColumn(new RowColEventArgs(row, i - base.Cols.Fixed));
				}
			}
			else
			{
				OnBodyAfterResizeColumn(new RowColEventArgs(row, num));
			}
		}
		else
		{
			OnBodyAfterResizeColumn(new RowColEventArgs(row, num));
		}
	}

	protected virtual void OnBodyAfterResizeColumn(RowColEventArgs e)
	{
		this.BodyAfterResizeColumn?.Invoke(this, e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = HitTest();
		bool flag = hitTestInfo.Type == HitTestTypeEnum.ColumnHeader && IsEntireColumnSelected && Selection.ContainsCol(hitTestInfo.Column);
		bool flag2 = hitTestInfo.Type == HitTestTypeEnum.RowHeader && IsEntireRowSelected && Selection.ContainsRow(hitTestInfo.Row);
		bool flag3 = hitTestInfo.Type == HitTestTypeEnum.Cell && Selection.Contains(hitTestInfo.Row, hitTestInfo.Column);
		if (e.Button == MouseButtons.Right && !flag && !flag2 && !flag3)
		{
			OnMouseDown(new MouseEventArgs(MouseButtons.Left, e.Clicks, e.X, e.Y, e.Delta));
		}
		if (hitTestInfo.Type == HitTestTypeEnum.ColumnResize && e.Clicks == 1 && e.Button == MouseButtons.Left)
		{
			_isResizingColumnMouseDown = true;
			_mouseDownPosition = e.Location;
		}
		if (hitTestInfo.Type == HitTestTypeEnum.RowResize && e.Clicks == 1 && e.Button == MouseButtons.Left)
		{
			_isResizingRowMouseDown = true;
			_mouseDownPosition = e.Location;
		}
		HandledMouseEventArgs handledMouseEventArgs = new HandledMouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta);
		this.PreviewMouseDown?.Invoke(this, handledMouseEventArgs);
		if (!handledMouseEventArgs.Handled)
		{
			base.OnMouseDown(e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		_isResizingColumnMouseDown = false;
		IsResizingColumn = false;
		_isResizingRowMouseDown = false;
		IsResizingRow = false;
		base.OnMouseUp(e);
	}

	protected override void OnCellChanged(RowColEventArgs e)
	{
		base.OnCellChanged(e);
		int num = e.Row - base.Rows.Fixed;
		int num2 = e.Col - base.Cols.Fixed;
		if (num >= 0 && num2 >= 0)
		{
			OnBodyCellChanged(new RowColEventArgs(num, num2));
		}
	}

	protected void OnBodyCellChanged(RowColEventArgs e)
	{
		this.BodyCellChanged?.Invoke(this, e);
	}

	protected override void OnGridChanged(object sender, GridChangedEventArgs e)
	{
		base.OnGridChanged(sender, e);
		if (_raiseOnGridChanged)
		{
			if (e.GridChangedType == GridChangedTypeEnum.AfterSelChange)
			{
				OnBodySelectionChanged();
			}
			else if (e.GridChangedType == GridChangedTypeEnum.ColRemoved || e.GridChangedType == GridChangedTypeEnum.RowRemoved)
			{
				OnBodySelectionChanged();
			}
		}
	}

	private void OnBodySelectionChanged()
	{
		if (IsEntireColumnSelected && IsEntireRowSelected)
		{
			SelectionType = SelectionType.Table;
		}
		else if (IsEntireColumnSelected)
		{
			SelectionType = SelectionType.Column;
		}
		else if (IsEntireRowSelected)
		{
			SelectionType = SelectionType.Row;
		}
		else
		{
			SelectionType = SelectionType.Range;
		}
		this.BodySelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	protected override void OnAfterScroll(RangeEventArgs e)
	{
		this.BodyAfterScroll?.Invoke(this, new RangeEventArgs(ToBodyRange(e.OldRange), ToBodyRange(e.NewRange)));
		base.OnAfterScroll(e);
	}

	protected override void OnOwnerDrawCell(OwnerDrawCellEventArgs e)
	{
		if (!IsCellFixed(e.Row, e.Col))
		{
			OwnerDrawCellEventArgs ownerDrawCellEventArgs = new OwnerDrawCellEventArgs(this, e.Graphics, e.Row - base.Rows.Fixed, e.Col - base.Cols.Fixed, e.Style, e.Bounds, e.Text, e.Image);
			this.BodyOwnerDrawCell?.Invoke(this, ownerDrawCellEventArgs);
			e.Text = ownerDrawCellEventArgs.Text;
			e.Image = ownerDrawCellEventArgs.Image;
			e.Style = ownerDrawCellEventArgs.Style;
			e.Handled = ownerDrawCellEventArgs.Handled;
		}
		base.OnOwnerDrawCell(e);
	}

	protected override void OnAfterRowColChange(RangeEventArgs e)
	{
		this.BodyAfterRowColChange?.Invoke(this, new RangeEventArgs(ToBodyRange(e.OldRange), ToBodyRange(e.NewRange)));
		base.OnAfterRowColChange(e);
	}

	protected override void OnValidateEdit(ValidateEditEventArgs e)
	{
		if (!IsCellFixed(e.Row, e.Col))
		{
			ValidateEditEventArgs validateEditEventArgs = new ValidateEditEventArgs(e.Row - base.Rows.Fixed, e.Col - base.Cols.Fixed, e.Checkbox);
			this.BodyValidateEdit?.Invoke(this, validateEditEventArgs);
			e.Cancel = validateEditEventArgs.Cancel;
		}
		base.OnValidateEdit(e);
	}

	protected override void OnStartEdit(RowColEventArgs e)
	{
		if (!IsCellFixed(e.Row, e.Col))
		{
			this.BodyStartEdit?.Invoke(this, new RowColEventArgs(e.Row - base.Rows.Fixed, e.Col - base.Cols.Fixed));
		}
		base.OnStartEdit(e);
	}

	protected override void OnSetupEditor(RowColEventArgs e)
	{
		if (base[e.Row, e.Col] is string input && base.Editor.GetType().Name == "GridEditorTextBox")
		{
			base.Editor.Text = Regex.Replace(input, "(?<!\\r)\\n", "\r\n");
		}
		if (!IsCellFixed(e.Row, e.Col))
		{
			this.BodySetupEditor?.Invoke(this, new RowColEventArgs(e.Row - base.Rows.Fixed, e.Col - base.Cols.Fixed));
		}
		base.OnSetupEditor(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = HitTest(e.Location);
		if (_isResizingColumnMouseDown && !IsResizingColumn && Math.Abs(e.Location.X - _mouseDownPosition.X) > SystemInformation.DragSize.Width)
		{
			IsResizingColumn = true;
		}
		if (_isResizingRowMouseDown && !IsResizingRow && Math.Abs(e.Location.Y - _mouseDownPosition.Y) > SystemInformation.DragSize.Height)
		{
			IsResizingRow = true;
		}
		if (!IsResizingColumn && !IsResizingRow)
		{
			if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader && hitTestInfo.Row < base.Rows.Fixed && (FilterManager.IsPointInFilterImageRect(e.Location, hitTestInfo.Column, isCancelAllIcon: true) || FilterManager.IsPointInFilterImageRect(e.Location, hitTestInfo.Column)))
			{
				if (!FilterManager.IsEditingFormula && !FilterManager.IsEditingColHeader && !FilterManager.IsLocked)
				{
					Cursor = Cursors.Arrow;
					if (!_isMouseInFilterImageRect)
					{
						_isMouseInFilterImageRect = true;
						Invalidate();
					}
				}
			}
			else
			{
				if (!FilterManager.IsEditingFormula && !FilterManager.IsEditingColHeader && !FilterManager.IsLocked && _isMouseInFilterImageRect)
				{
					_isMouseInFilterImageRect = false;
					Invalidate();
				}
				base.OnMouseMove(e);
			}
		}
		else
		{
			base.OnMouseMove(e);
		}
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		base.OnPaintBackground(e);
		this.PaintBackground?.Invoke(this, e);
	}

	public CellRange ToBodyRange(CellRange range)
	{
		CellRange result = default(CellRange);
		result.c1 = ((range.c1 >= base.Cols.Fixed) ? (range.c1 - base.Cols.Fixed) : 0);
		result.c2 = ((range.c2 >= base.Cols.Fixed) ? (range.c2 - base.Cols.Fixed) : 0);
		result.r1 = ((range.r1 >= base.Rows.Fixed) ? (range.r1 - base.Rows.Fixed) : 0);
		result.r2 = ((range.r2 >= base.Rows.Fixed) ? (range.r2 - base.Rows.Fixed) : 0);
		return result;
	}

	public bool IsRowIndexOutOfRange(int rowIndex)
	{
		if (rowIndex < 0 || rowIndex >= base.Rows.Count)
		{
			return true;
		}
		return false;
	}

	public bool IsIndexOutOfRange(int rowIndex, int colIndex)
	{
		if (rowIndex < 0 || rowIndex >= base.Rows.Count)
		{
			return true;
		}
		if (colIndex < 0 || colIndex >= base.Cols.Count)
		{
			return true;
		}
		return false;
	}

	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
		case 256:
			if ((int)m.WParam == 229)
			{
				return;
			}
			break;
		case 646:
			if (GetAllowEditing())
			{
				StartEditing();
				if (base.Editor != null)
				{
					SendMessage(base.Editor.Handle, m.Msg, m.WParam, m.LParam);
				}
			}
			return;
		case 271:
		{
			COMPOSITIONFORM lpCompForm = default(COMPOSITIONFORM);
			lpCompForm.dwStyle = 2u;
			Rectangle cellRect = GetCellRect(base.Row, base.Col);
			IntPtr intPtr = ImmGetContext(base.Handle);
			byte[] buf = null;
			int num = ImmGetCompositionString(intPtr, 8, buf, 0);
			buf = new byte[num];
			ImmGetCompositionString(intPtr, 8, buf, num);
			string @string = Encoding.Unicode.GetString(buf);
			CellRange cellRange = GetCellRange(base.Row, base.Col);
			if (cellRange.IsValid)
			{
				C1.Win.C1FlexGrid.CellStyle styleDisplay = cellRange.StyleDisplay;
				lpCompForm.ptCurrentPos = new Point(GetX(styleDisplay.TextAlign, cellRect, styleDisplay.Font, @string), GetY(styleDisplay.TextAlign, cellRect, styleDisplay.Font, @string));
				ImmSetCompositionWindow(intPtr, ref lpCompForm);
				LOGFONT lOGFONT = new LOGFONT();
				styleDisplay.Font.ToLogFont(lOGFONT);
				IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LOGFONT)));
				Marshal.StructureToPtr(lOGFONT, intPtr2, fDeleteOld: false);
				ImmSetCompositionFont(intPtr, intPtr2);
				Marshal.FreeHGlobal(intPtr2);
			}
			ImmReleaseContext(base.Handle, intPtr);
			break;
		}
		}
		base.WndProc(ref m);
	}

	private int GetX(TextAlignEnum ta, Rectangle cellRect, Font font, string str)
	{
		switch (ta)
		{
		case TextAlignEnum.LeftTop:
		case TextAlignEnum.LeftCenter:
		case TextAlignEnum.LeftBottom:
		case TextAlignEnum.GeneralTop:
		case TextAlignEnum.GeneralCenter:
		case TextAlignEnum.GeneralBottom:
			return cellRect.Left;
		case TextAlignEnum.CenterTop:
		case TextAlignEnum.CenterCenter:
		case TextAlignEnum.CenterBottom:
			return cellRect.Left + cellRect.Width / 2 - (int)(GetStringWidth(font, str) / 2f);
		case TextAlignEnum.RightTop:
		case TextAlignEnum.RightCenter:
		case TextAlignEnum.RightBottom:
			return cellRect.Right - (int)GetStringWidth(font, str);
		default:
			return 0;
		}
	}

	private int GetY(TextAlignEnum ta, Rectangle cellRect, Font font, string str)
	{
		switch (ta)
		{
		case TextAlignEnum.LeftTop:
		case TextAlignEnum.CenterTop:
		case TextAlignEnum.RightTop:
		case TextAlignEnum.GeneralTop:
			return cellRect.Top;
		case TextAlignEnum.LeftCenter:
		case TextAlignEnum.CenterCenter:
		case TextAlignEnum.RightCenter:
		case TextAlignEnum.GeneralCenter:
			return cellRect.Top + cellRect.Height / 2 - (int)(GetFontHeight(font, str) / 2f);
		case TextAlignEnum.LeftBottom:
		case TextAlignEnum.CenterBottom:
		case TextAlignEnum.RightBottom:
		case TextAlignEnum.GeneralBottom:
			return cellRect.Bottom - (int)GetFontHeight(font, str);
		default:
			return 0;
		}
	}

	private float GetStringWidth(Font font, string str)
	{
		return CreateGraphics().MeasureString(str, font).Width;
	}

	private float GetFontHeight(Font font, string str)
	{
		return CreateGraphics().MeasureString(str, font).Height;
	}

	private bool GetAllowEditing()
	{
		if (!base.AllowEditing)
		{
			return false;
		}
		try
		{
			if (!base.Rows[base.Row].AllowEditing)
			{
				return false;
			}
			if (!base.Cols[base.Col].AllowEditing)
			{
				return false;
			}
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
		RowColEventArgs rowColEventArgs = new RowColEventArgs(base.Row, base.Col);
		OnBeforeEdit(rowColEventArgs);
		if (rowColEventArgs.Cancel)
		{
			return false;
		}
		return true;
	}

	public int GetTotalWidth(Action extra = null)
	{
		base.ExtendLastCol = false;
		AutoSizeCols();
		extra?.Invoke();
		int num = base.Cols[base.Cols.Count - 1].Right + 4;
		if (base.ScrollBarsVisible.HasFlag(ScrollBars.Vertical))
		{
			num += SystemInformation.VerticalScrollBarWidth;
		}
		base.ExtendLastCol = true;
		return num;
	}

	public bool RangeIntersects(CellRange r1, CellRange r2)
	{
		if (r2.TopRow <= r1.BottomRow && r2.BottomRow >= r1.TopRow && r2.LeftCol <= r1.RightCol)
		{
			return r2.RightCol >= r1.LeftCol;
		}
		return false;
	}

	public void AdjustPosition(Size bound, int border = 0, bool top0 = false, bool fullHeight = false)
	{
		int num = border;
		for (int i = 0; i < base.Rows.Count; i++)
		{
			num += base.Rows[i].HeightDisplay;
		}
		int num2 = border;
		for (int j = 0; j < base.Cols.Count; j++)
		{
			num2 += base.Cols[j].WidthDisplay;
		}
		if (num2 > bound.Width)
		{
			num += SystemInformation.HorizontalScrollBarHeight;
		}
		if (num > bound.Height)
		{
			num2 += SystemInformation.VerticalScrollBarWidth;
			if (num2 > bound.Width)
			{
				num += SystemInformation.HorizontalScrollBarHeight;
			}
		}
		bool flag = num2 > bound.Width;
		bool flag2 = num > bound.Height;
		if (flag)
		{
			base.Width = bound.Width;
			base.Left = 0;
		}
		else
		{
			base.Left = (bound.Width - num2) / 2;
			if (flag2)
			{
				base.Width = num2 + base.Left;
			}
			else
			{
				base.Width = num2;
			}
		}
		if (flag2 || fullHeight)
		{
			base.Height = bound.Height;
			base.Top = 0;
			return;
		}
		if (top0)
		{
			base.Top = 0;
		}
		else
		{
			base.Top = (bound.Height - num) / 2;
		}
		if (flag)
		{
			base.Height = num + base.Top;
		}
		else
		{
			base.Height = num;
		}
	}

	public int GetGridWidth()
	{
		int num = 0;
		for (int i = 0; i < base.Cols.Count; i++)
		{
			num += base.Cols[i].WidthDisplay;
		}
		return num;
	}

	public CellRange RangeUnion(CellRange cr1, CellRange cr2)
	{
		int num = Math.Min(cr1.TopRow, cr2.TopRow);
		int num2 = Math.Min(cr1.LeftCol, cr2.LeftCol);
		int num3 = Math.Max(cr1.BottomRow, cr2.BottomRow);
		int num4 = Math.Max(cr1.RightCol, cr2.RightCol);
		return GetCellRange((cr1.r2 > cr1.r1) ? num : num3, (cr1.c2 > cr1.c1) ? num2 : num4, (cr1.r2 > cr1.r1) ? num3 : num, (cr1.c2 > cr1.c1) ? num4 : num2);
	}

	public static TextAlignEnum ToTextAlign(CellTextAlign a)
	{
		return a switch
		{
			CellTextAlign.TopCenter => TextAlignEnum.CenterTop, 
			CellTextAlign.TopLeft => TextAlignEnum.LeftTop, 
			CellTextAlign.TopRight => TextAlignEnum.RightTop, 
			CellTextAlign.MiddleCenter => TextAlignEnum.CenterCenter, 
			CellTextAlign.MiddleLeft => TextAlignEnum.LeftCenter, 
			CellTextAlign.MiddleRight => TextAlignEnum.RightCenter, 
			CellTextAlign.BottomLeft => TextAlignEnum.LeftBottom, 
			CellTextAlign.BottomCenter => TextAlignEnum.CenterBottom, 
			CellTextAlign.BottomRight => TextAlignEnum.RightBottom, 
			_ => TextAlignEnum.LeftCenter, 
		};
	}

	public static ImageAlignEnum ToImageAlign(CellTextAlign a)
	{
		return a switch
		{
			CellTextAlign.TopCenter => ImageAlignEnum.CenterTop, 
			CellTextAlign.TopLeft => ImageAlignEnum.LeftTop, 
			CellTextAlign.TopRight => ImageAlignEnum.RightTop, 
			CellTextAlign.MiddleCenter => ImageAlignEnum.CenterCenter, 
			CellTextAlign.MiddleLeft => ImageAlignEnum.LeftCenter, 
			CellTextAlign.MiddleRight => ImageAlignEnum.RightCenter, 
			CellTextAlign.BottomLeft => ImageAlignEnum.LeftBottom, 
			CellTextAlign.BottomCenter => ImageAlignEnum.CenterBottom, 
			CellTextAlign.BottomRight => ImageAlignEnum.RightBottom, 
			_ => ImageAlignEnum.LeftCenter, 
		};
	}
}
