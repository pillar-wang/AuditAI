using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class GridResizingManager
{
	private static Cursor _curResizeRow = new Cursor(new MemoryStream(Resources.ResizeRow));

	private static Cursor _curResizeCol = new Cursor(new MemoryStream(Resources.ResizeCol));

	protected readonly C1FlexGridEx _grid;

	private bool _isBeginResizingColumn;

	private int _beginResizingColumnX;

	private bool _isResizingColumn;

	private int _resizingColumn;

	private bool _isBeginResizingRow;

	private int _beginResizingRowY;

	private bool _isResizingRow;

	private int _resizingRow;

	protected int _hitCheckHalfWidth = 3;

	protected int _hitCheckHalfHeight = 3;

	public static readonly Pen PenResizeDragging = new Pen(Color.Gray, 1f)
	{
		DashStyle = DashStyle.Dash
	};

	public bool IsResizing
	{
		get
		{
			if (!_isResizingColumn)
			{
				return _isResizingRow;
			}
			return true;
		}
	}

	public static Cursor CursorResizeCol => _curResizeCol;

	public event EventHandler<ResizeEventArgs> ResizeColumn;

	public event EventHandler<ResizeEventArgs> ResizeRow;

	public GridResizingManager(C1FlexGridEx grid)
	{
		_grid = grid;
		_grid.PreviewMouseDown += _grid_PreviewMouseDown;
		_grid.MouseMove += _grid_MouseMove;
		_grid.MouseUp += _grid_MouseUp;
		_grid.Paint += _grid_Paint;
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		if (_isResizingColumn)
		{
			Point point = _grid.PointToScreen(Point.Empty);
			int num = Control.MousePosition.X - point.X;
			e.Graphics.DrawLine(PenResizeDragging, num, 0, num, _grid.Height);
		}
		else if (_isResizingRow)
		{
			Point point2 = _grid.PointToScreen(Point.Empty);
			int num2 = Control.MousePosition.Y - point2.Y;
			e.Graphics.DrawLine(PenResizeDragging, 0, num2, _grid.Width, num2);
		}
	}

	private void _grid_MouseUp(object sender, MouseEventArgs e)
	{
		_isBeginResizingColumn = false;
		_isBeginResizingRow = false;
		if (_isResizingColumn)
		{
			_isResizingColumn = false;
			int heightWidth = Math.Abs(_grid.ScrollPosition.X) + e.Location.X - _grid.Cols[_resizingColumn].Left;
			OnResizeColumn(new ResizeEventArgs
			{
				RowCol = _resizingColumn,
				HeightWidth = heightWidth
			});
			_grid.Cursor = Cursors.Default;
			_grid.Invalidate();
		}
		else if (_isResizingRow)
		{
			_isResizingRow = false;
			int heightWidth2 = Math.Abs(_grid.ScrollPosition.Y) + e.Location.Y - _grid.Rows[_resizingRow].Top;
			OnResizeRow(new ResizeEventArgs
			{
				RowCol = _resizingRow,
				HeightWidth = heightWidth2
			});
			_grid.Cursor = Cursors.Default;
			_grid.Invalidate();
		}
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		if (_isResizingColumn || _isResizingRow)
		{
			_grid.Invalidate();
		}
		else if (_isBeginResizingColumn)
		{
			_grid.Cursor = _curResizeCol;
			if (Math.Abs(e.X - _beginResizingColumnX) >= SystemInformation.DragSize.Width)
			{
				_isResizingColumn = true;
			}
		}
		else if (_isBeginResizingRow)
		{
			_grid.Cursor = _curResizeRow;
			if (Math.Abs(e.Y - _beginResizingRowY) >= SystemInformation.DragSize.Height)
			{
				_isResizingRow = true;
			}
		}
		else if (GetResizingColumn(e) > -1)
		{
			_grid.Cursor = _curResizeCol;
		}
		else if (GetResizingRow(e) > -1)
		{
			_grid.Cursor = _curResizeRow;
		}
		else
		{
			OnMouseMove(e);
		}
	}

	protected virtual void OnMouseMove(MouseEventArgs e)
	{
		_grid.Cursor = Cursors.Default;
	}

	private void _grid_PreviewMouseDown(object sender, HandledMouseEventArgs e)
	{
		int resizingColumn = GetResizingColumn(e);
		if (resizingColumn > -1)
		{
			_resizingColumn = resizingColumn;
			_isBeginResizingColumn = true;
			_beginResizingColumnX = e.X;
			e.Handled = true;
			return;
		}
		int resizingRow = GetResizingRow(e);
		if (resizingRow > -1)
		{
			_resizingRow = resizingRow;
			_isBeginResizingRow = true;
			_beginResizingRowY = e.Y;
			e.Handled = true;
		}
	}

	protected virtual void OnResizeColumn(ResizeEventArgs ea)
	{
		this.ResizeColumn?.Invoke(this, ea);
	}

	protected virtual void OnResizeRow(ResizeEventArgs ea)
	{
		this.ResizeRow?.Invoke(this, ea);
	}

	protected virtual int GetResizingColumn(MouseEventArgs e)
	{
		int num = -1;
		int num2 = 0;
		if (num == -1)
		{
			num = _grid.MouseCol;
			num2 = Math.Abs(_grid.ScrollPosition.X) + e.Location.X;
		}
		if (num >= 0)
		{
			Column column = _grid.Cols[num];
			if (Math.Abs(column.Right - num2) <= _hitCheckHalfWidth)
			{
				return num;
			}
			if (Math.Abs(column.Left - num2) <= _hitCheckHalfWidth)
			{
				return num - 1;
			}
		}
		return -1;
	}

	protected virtual int GetResizingRow(MouseEventArgs e)
	{
		int num = -1;
		int num2 = 0;
		if (num == -1)
		{
			num = _grid.MouseRow;
			num2 = Math.Abs(_grid.ScrollPosition.Y) + e.Location.Y;
		}
		if (num >= 0)
		{
			Row row = _grid.Rows[num];
			if (Math.Abs(row.Bottom - num2) <= _hitCheckHalfHeight)
			{
				return num;
			}
			if (Math.Abs(row.Top - num2) <= _hitCheckHalfHeight)
			{
				return num - 1;
			}
		}
		return -1;
	}
}
