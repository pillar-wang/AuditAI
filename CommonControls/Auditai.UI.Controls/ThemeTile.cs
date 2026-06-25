using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Tile;

namespace Auditai.UI.Controls;

internal class ThemeTile
{
	private string _selectThemeId;

	private TileSetting tileSetting;

	private bool _attached;

	public C1TileControl View { get; private set; }

	public event EventHandler<string> AfterSelectedThemeChanged;

	public ThemeTile()
	{
		Initialize();
	}

	public void CheckTile(string currentThemeId)
	{
		if (string.IsNullOrEmpty(currentThemeId))
		{
			currentThemeId = Theme.ThemePool[0].Id;
		}
		if (Theme.ThemePool.Exists((AuditaiTheme t) => t.Id == currentThemeId))
		{
			View.Groups[0].Tiles[currentThemeId].Checked = true;
		}
	}

	private void Initialize()
	{
		tileSetting = new TileSetting();
		View = new C1TileControl
		{
			Dock = DockStyle.Fill,
			AllowChecking = true,
			CellWidth = tileSetting.Width,
			CellHeight = tileSetting.Height,
			Font = new Font("微软雅黑", tileSetting.FontSize),
			HotBorderColor = tileSetting.HotBorderColor,
			Orientation = LayoutOrientation.Vertical,
			Padding = default(Padding),
			GroupPadding = default(Padding),
			TileForeColor = tileSetting.FontColor,
			TileBorderColor = tileSetting.HotBorderColor,
			TileBackColor = Color.Transparent,
			CheckBorderColor = Color.Red,
			CheckMarkColor = Color.Blue,
			ScrollBarStyle = ScrollBarStyle.Default
		};
		TextElement item = new TextElement
		{
			Alignment = tileSetting.FontAlignment,
			ForeColor = tileSetting.FontColor,
			FontSize = tileSetting.FontSize,
			BackColor = Color.Transparent
		};
		ImageElement item2 = new ImageElement();
		View.DefaultTemplate.Elements.Add(item2);
		View.DefaultTemplate.Elements.Add(item);
		AttachEvent();
		Group group = new Group();
		foreach (AuditaiTheme item3 in Theme.ThemePool)
		{
			Tile tile = new Tile
			{
				Name = item3.Id,
				Text = item3.FriendName,
				Tag = item3
			};
			if (item3.ThemeFlags.HasFlag(ThemeEnum.Picture))
			{
				tile.Image = item3.ThemeContext.SmallImage;
			}
			else
			{
				tile.BackColor = item3.ThemeContext.TileColor;
			}
			tile.Click += Tile_Click;
			group.Tiles.Add(tile);
		}
		View.Groups.Add(group);
	}

	private void AttachEvent()
	{
		if (!_attached)
		{
			View.TileChecked += TileControl_TileChecked;
			View.TileUnchecked += TileControl_TileUnchecked;
			_attached = true;
		}
	}

	private void DettachEvent()
	{
		if (_attached)
		{
			View.TileChecked -= TileControl_TileChecked;
			View.TileUnchecked -= TileControl_TileUnchecked;
			_attached = false;
		}
	}

	private void Tile_Click(object sender, EventArgs e)
	{
		Tile tile = sender as Tile;
		_selectThemeId = tile.Name;
		tile.Checked = true;
	}

	private void TileControl_TileChecked(object sender, TileEventArgs e)
	{
		DettachEvent();
		try
		{
			Tile[] checkedTiles = View.CheckedTiles;
			foreach (Tile tile in checkedTiles)
			{
				if (tile != e.Tile)
				{
					tile.Checked = false;
				}
			}
			_selectThemeId = e.Tile.Name;
			this.AfterSelectedThemeChanged?.Invoke(this, _selectThemeId);
		}
		finally
		{
			AttachEvent();
		}
	}

	private void TileControl_TileUnchecked(object sender, TileEventArgs e)
	{
		DettachEvent();
		try
		{
			if (e.Tile.Name.Equals(_selectThemeId))
			{
				e.Tile.Checked = true;
			}
		}
		finally
		{
			AttachEvent();
		}
	}
}
