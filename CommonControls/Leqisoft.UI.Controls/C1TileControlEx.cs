using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Tile;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class C1TileControlEx : C1TileControl
{
	protected class CornerImageData
	{
		public Image Image;

		public Point Offset;
	}

	private static readonly Image _imgClose = Resources.tileClose;

	private static readonly Image _imgCloseHover = Resources.tileCloseSlide;

	private static readonly Image _imgCloseDown = Resources.tileCloseDown;

	private bool _allowCloseButton;

	private bool _isMouseWithinCloseButton;

	private Tile _mouseTile;

	private Tile _mouseCloseButtonTile;

	private Size _mouseCloseButtonMargin = new Size(3, 3);

	private static readonly Image _imgCheckedMark = Resources.Checked;

	private Tile _previousClickedTile;

	private MouseButtons _previousClickedButtons;

	private Pen _selectBorderPen;

	private int _customBorderWidth = 3;

	private Color _customerBorderColor;

	private Stopwatch _watch = new Stopwatch();

	private readonly Pen _penGroupCaptionUnderline = new Pen(Color.FromArgb(0, 73, 92), 1f);

	private HashSet<Tile> _selectedTileSet = new HashSet<Tile>();

	private Dictionary<Tile, Image> _rightTopImageTileDic;

	private Dictionary<Tile, CornerImageData> _leftTopImageTileDic;

	protected List<C1TileCustomButtonRender> _tileCustomButtonRenderList;

	private bool _skipTileClick;

	public bool AllowMultiSelect { get; set; }

	public bool AllowCloseButton
	{
		get
		{
			return _allowCloseButton;
		}
		set
		{
			if (value != _allowCloseButton)
			{
				_allowCloseButton = value;
			}
		}
	}

	public Color CustomBorderColor
	{
		get
		{
			return _customerBorderColor;
		}
		set
		{
			_customerBorderColor = value;
			_selectBorderPen = new Pen(value, _customBorderWidth);
		}
	}

	public int CustomBorderWidth
	{
		get
		{
			return _customBorderWidth;
		}
		set
		{
			_customBorderWidth = value;
			_selectBorderPen = new Pen(CustomBorderColor, value);
		}
	}

	public Tile SelectedTile => _selectedTileSet.FirstOrDefault();

	public IEnumerable<Tile> SelectedTiles => _selectedTileSet;

	public event EventHandler<Tile> TileUnCheckedEvent;

	public event EventHandler<Tile> DoubleClickTile;

	public event EventHandler<TileEventArgs> TileCloseClick;

	public C1TileControlEx()
	{
		base.Margin = Padding.Empty;
		base.Padding = Padding.Empty;
		base.GroupTextSize = 9f;
		base.GroupPadding = new Padding(0, 40, 0, 0);
		base.SurfacePadding = new Padding(15, 10, 15, 5);
		base.GroupTextX = 5;
		base.GroupTextY = 5;
		base.GroupSpacing = 10;
		base.Orientation = LayoutOrientation.Vertical;
		_customerBorderColor = Theme.SelectedLeqiTheme.ThemeContext.DarkColor;
		_selectBorderPen = new Pen(CustomBorderColor, _customBorderWidth);
		base.AllowChecking = false;
		base.Paint += This_Paint;
		base.TileUnchecked += C1TileControlEx_TileUnchecked;
	}

	private void C1TileControlEx_TileUnchecked(object sender, TileEventArgs e)
	{
		if (_selectedTileSet.Contains(e.Tile))
		{
			e.Tile.Checked = true;
		}
	}

	public void AddTileCustomButtonRender(C1TileCustomButtonRender render)
	{
		if (_tileCustomButtonRenderList == null)
		{
			_tileCustomButtonRenderList = new List<C1TileCustomButtonRender>();
		}
		_tileCustomButtonRenderList.Add(render);
	}

	public void RemoveTileCustomButtonRender(C1TileCustomButtonRender render)
	{
		if (_tileCustomButtonRenderList != null)
		{
			_tileCustomButtonRenderList.Remove(render);
			if (_tileCustomButtonRenderList.Count == 0)
			{
				_tileCustomButtonRenderList = null;
			}
		}
	}

	public void SelectTile(Tile tile)
	{
		if (AllowMultiSelect)
		{
			_selectedTileSet.Add(tile);
			return;
		}
		_selectedTileSet.Clear();
		_selectedTileSet.Add(tile);
	}

	public void CancelSelectTile(Tile tile)
	{
		_selectedTileSet.Remove(tile);
	}

	public void ToggleTile(Tile tile)
	{
		if (_selectedTileSet.Contains(tile))
		{
			CancelSelectTile(tile);
		}
		else
		{
			SelectTile(tile);
		}
	}

	public void ClearSelected()
	{
		_selectedTileSet.Clear();
	}

	public void ClearExternalImage()
	{
		if (_leftTopImageTileDic != null)
		{
			_leftTopImageTileDic.Clear();
		}
		if (_rightTopImageTileDic != null)
		{
			_rightTopImageTileDic.Clear();
		}
	}

	public void AddRightTopImage(Tile tile, Image rightTop)
	{
		if (_rightTopImageTileDic == null)
		{
			_rightTopImageTileDic = new Dictionary<Tile, Image>();
		}
		_rightTopImageTileDic[tile] = rightTop;
	}

	public void AddLeftTopImage(Tile tile, Image leftTop, Point offset)
	{
		if (_leftTopImageTileDic == null)
		{
			_leftTopImageTileDic = new Dictionary<Tile, CornerImageData>();
		}
		_leftTopImageTileDic[tile] = new CornerImageData
		{
			Image = leftTop,
			Offset = offset
		};
	}

	public Image GetLeftTopImage(Tile tile)
	{
		if (_leftTopImageTileDic == null)
		{
			return null;
		}
		if (_leftTopImageTileDic.TryGetValue(tile, out var value))
		{
			return value.Image;
		}
		return null;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			MouseEventArgs mouseEventArgs = new MouseEventArgs(MouseButtons.Left, e.Clicks, e.X, e.Y, e.Delta);
			base.OnMouseDown(mouseEventArgs);
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		Tile tileAt = GetTileAt(e.Location);
		if (tileAt != null)
		{
			if (AllowMultiSelect && _selectedTileSet.Contains(tileAt) && InCheckRect(e.Location, tileAt))
			{
				_selectedTileSet.Remove(tileAt);
				this.TileUnCheckedEvent?.Invoke(this, tileAt);
				_skipTileClick = true;
				base.OnMouseUp(e);
				_skipTileClick = false;
				return;
			}
			if (!AllowMultiSelect && !_selectedTileSet.Contains(tileAt))
			{
				_selectedTileSet.Clear();
				_selectedTileSet.Add(tileAt);
			}
			TileMouseEventArgs tileMouseEventArgs = new TileMouseEventArgs(e, tileAt);
			TileClick_DoubleClick(tileMouseEventArgs);
		}
		if (e.Button == MouseButtons.Right)
		{
			MouseEventArgs mouseEventArgs = new MouseEventArgs(MouseButtons.Left, e.Clicks, e.X, e.Y, e.Delta);
			base.OnMouseUp(mouseEventArgs);
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (_allowCloseButton)
		{
			bool flag = false;
			Tile tileAt = GetTileAt(e.Location);
			if (tileAt != _mouseTile)
			{
				_mouseTile = tileAt;
				Invalidate();
			}
			if (tileAt != null)
			{
				Group group = tileAt.Group;
				int num = group.X + tileAt.X + tileAt.Width - _imgClose.Width - _mouseCloseButtonMargin.Width;
				int num2 = group.Y + tileAt.Y + _mouseCloseButtonMargin.Height + base.ScrollOffset;
				if (new Rectangle(num, num2, _imgClose.Width, _imgClose.Height).Contains(e.Location))
				{
					flag = true;
				}
			}
			if (_isMouseWithinCloseButton != flag)
			{
				_isMouseWithinCloseButton = flag;
				_mouseCloseButtonTile = tileAt;
				Invalidate();
			}
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		if (_allowCloseButton && _isMouseWithinCloseButton)
		{
			Tile tileAt = GetTileAt(e.Location);
			if (tileAt != null)
			{
				this.TileCloseClick?.Invoke(this, new TileEventArgs(tileAt));
				_mouseTile = null;
				_mouseCloseButtonTile = null;
			}
		}
		base.OnMouseClick(e);
	}

	protected override void OnTileClicked(TileEventArgs e)
	{
		if (!_skipTileClick)
		{
			base.OnTileClicked(e);
		}
	}

	private void This_Paint(object sender, PaintEventArgs e)
	{
		Paint_LeftTopImage(e);
		Paint_RightTopImage(e);
		Paint_Checked(e);
		Paint_Close(e);
		Paint_CustomButton(e);
	}

	private void Paint_LeftTopImage(PaintEventArgs e)
	{
		if (_leftTopImageTileDic == null || _leftTopImageTileDic.Count == 0)
		{
			return;
		}
		foreach (Tile key in _leftTopImageTileDic.Keys)
		{
			CornerImageData cornerImageData = _leftTopImageTileDic[key];
			if (cornerImageData != null && cornerImageData.Image != null)
			{
				e.Graphics.DrawImage(cornerImageData.Image, new PointF(key.Group.X + key.X + cornerImageData.Offset.X, key.Group.Y + key.Y + base.ScrollOffset + cornerImageData.Offset.Y));
			}
		}
	}

	private void Paint_RightTopImage(PaintEventArgs e)
	{
		if (_rightTopImageTileDic == null || _rightTopImageTileDic.Count == 0)
		{
			return;
		}
		foreach (Tile key in _rightTopImageTileDic.Keys)
		{
			Image image = _rightTopImageTileDic[key];
			if (image != null)
			{
				e.Graphics.DrawImage(image, new PointF(key.Group.X + key.X + key.Width - image.Width, key.Group.Y + key.Y + base.ScrollOffset));
			}
		}
	}

	private void Paint_Checked(PaintEventArgs e)
	{
		foreach (Tile item in _selectedTileSet)
		{
			if (item != null && item.Visible && base.Groups.Contains(item.Group))
			{
				e.Graphics.DrawRectangle(_selectBorderPen, item.Group.X + item.X - 2, item.Group.Y + item.Y - 2 + base.ScrollOffset, item.Width + 3, item.Height + 3);
				if (AllowMultiSelect)
				{
					e.Graphics.DrawImage(_imgCheckedMark, new PointF(item.Group.X + item.X + item.Width - _imgCheckedMark.Width - 3, item.Group.Y + item.Y + 3 + base.ScrollOffset));
				}
			}
		}
		PaintGroupCaptionUnderline(e.Graphics);
	}

	private void Paint_Close(PaintEventArgs e)
	{
		if (!_allowCloseButton)
		{
			return;
		}
		Tile mouseCloseButtonTile = _mouseCloseButtonTile;
		if (_isMouseWithinCloseButton && mouseCloseButtonTile != null && mouseCloseButtonTile.Group != null)
		{
			int num = mouseCloseButtonTile.Group.X + mouseCloseButtonTile.X + mouseCloseButtonTile.Width - _imgClose.Width - _mouseCloseButtonMargin.Width;
			int num2 = mouseCloseButtonTile.Group.Y + mouseCloseButtonTile.Y + _mouseCloseButtonMargin.Height + base.ScrollOffset;
			if (Control.MouseButtons.HasFlag(MouseButtons.Left))
			{
				e.Graphics.DrawImage(_imgCloseDown, num, num2);
			}
			else
			{
				e.Graphics.DrawImage(_imgCloseHover, num, num2);
			}
		}
		else if (_mouseTile != null && _mouseTile.Group != null)
		{
			mouseCloseButtonTile = _mouseTile;
			int num3 = mouseCloseButtonTile.Group.X + mouseCloseButtonTile.X + mouseCloseButtonTile.Width - _imgClose.Width - _mouseCloseButtonMargin.Width;
			int num4 = mouseCloseButtonTile.Group.Y + mouseCloseButtonTile.Y + _mouseCloseButtonMargin.Height + base.ScrollOffset;
			e.Graphics.DrawImage(_imgClose, num3, num4);
		}
	}

	private void PaintGroupCaptionUnderline(Graphics g)
	{
		foreach (Group group in base.Groups)
		{
			if (group.Tiles.Count > 0)
			{
				SizeF sizeF = g.MeasureString(group.Text, new Font((base.GroupFont ?? Font).FontFamily, base.GroupTextSize));
				int num = base.Padding.Left + group.X + base.GroupTextX;
				float num2 = (float)(base.ScrollOffset + base.Padding.Top + group.Y + base.GroupTextY) + sizeF.Height + 5f;
				g.DrawLine(_penGroupCaptionUnderline, num, num2, (float)num + sizeF.Width, num2);
			}
		}
	}

	private void Paint_CustomButton(PaintEventArgs e)
	{
		if (_tileCustomButtonRenderList == null)
		{
			return;
		}
		foreach (C1TileCustomButtonRender tileCustomButtonRender in _tileCustomButtonRenderList)
		{
			tileCustomButtonRender.OnPaint(e);
		}
	}

	private void TileClick_DoubleClick(TileMouseEventArgs e)
	{
		bool flag = false;
		if (e.Tile == _previousClickedTile && e.MouseEA.Button == _previousClickedButtons && _watch.ElapsedMilliseconds > 0 && _watch.ElapsedMilliseconds < SystemInformation.DoubleClickTime)
		{
			flag = true;
		}
		_previousClickedTile = e.Tile;
		_previousClickedButtons = e.MouseEA.Button;
		_watch.Restart();
		if (flag && e.MouseEA.Button == MouseButtons.Left)
		{
			this.DoubleClickTile?.Invoke(this, e.Tile);
		}
		else
		{
			if (_tileCustomButtonRenderList == null)
			{
				return;
			}
			foreach (C1TileCustomButtonRender tileCustomButtonRender in _tileCustomButtonRenderList)
			{
				if (tileCustomButtonRender.OnTileSingleClicked(e))
				{
					break;
				}
			}
		}
	}

	private bool InCheckRect(Point p, Tile tile)
	{
		return new Rectangle(tile.Group.X + tile.X + tile.Width - _imgCheckedMark.Width - 3, tile.Group.Y + tile.Y + 3 + base.ScrollOffset, _imgCheckedMark.Width, _imgCheckedMark.Height).Contains(p);
	}

	public bool InCheckRect(Point p)
	{
		if (!AllowMultiSelect)
		{
			return false;
		}
		Tile tileAt = GetTileAt(p);
		if (tileAt == null)
		{
			return false;
		}
		if (!_selectedTileSet.Contains(tileAt))
		{
			return false;
		}
		return InCheckRect(p, tileAt);
	}

	public Rectangle GetTileRectangle(Tile tile)
	{
		return new Rectangle(tile.Group.X + tile.X, tile.Group.Y + tile.Y + base.ScrollOffset, tile.Width, tile.Height);
	}
}
