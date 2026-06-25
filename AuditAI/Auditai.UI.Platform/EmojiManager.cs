using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.EmojiResource;

namespace Auditai.UI.Platform;

public class EmojiManager
{
	private C1FlexGrid _emojiTable;

	private ToolStripDropDown dropDown;

	private ToolStripControlHost controlHost;

	private Dictionary<string, Image> Emojis;

	public int Width { get; set; } = 5;


	public int Height { get; set; } = 5;


	public event EventHandler<string> EmojiSelected;

	public EmojiManager(int _width, int _height)
	{
		Width = _width;
		Height = _height;
		Emojis = EmojiLib.GetImages();
		_emojiTable = new C1FlexGrid
		{
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			Dock = DockStyle.Fill,
			AllowEditing = false,
			ScrollBars = ScrollBars.Vertical
		};
		_emojiTable.Rows.Count = 0;
		_emojiTable.Cols.Count = 0;
		_emojiTable.Rows.DefaultSize = 36;
		for (int i = 0; i < Width; i++)
		{
			Column column = _emojiTable.Cols.Add();
			column.DataType = typeof(Image);
			column.Width = 36;
			column.ImageAlign = ImageAlignEnum.CenterCenter;
		}
		int num = 0;
		foreach (KeyValuePair<string, Image> emoji in Emojis)
		{
			if (num % Width == 0)
			{
				num = 0;
				_emojiTable.Rows.Add();
			}
			Row row = _emojiTable.Rows[_emojiTable.Rows.Count - 1];
			row[num] = emoji.Value;
			num++;
		}
		int width = 36 * Width + 17;
		int height = 36 * Height + 5;
		_emojiTable.Width = width;
		_emojiTable.Height = height;
		controlHost = new ToolStripControlHost(_emojiTable);
		controlHost.Width = width;
		controlHost.Height = height;
		dropDown = new ToolStripDropDown();
		dropDown.Width = width;
		dropDown.Height = height;
		dropDown.Items.Add(controlHost);
		_emojiTable.Click += delegate
		{
			int mouseRow = _emojiTable.MouseRow;
			int mouseCol = _emojiTable.MouseCol;
			if (mouseRow >= 0 && mouseCol >= 0)
			{
				object selected = _emojiTable.Rows[mouseRow][mouseCol];
				if (selected != null)
				{
					KeyValuePair<string, Image> keyValuePair = Emojis.FirstOrDefault((KeyValuePair<string, Image> emj) => selected.Equals(emj.Value));
					if (!default(KeyValuePair<string, Image>).Equals(keyValuePair))
					{
						this.EmojiSelected?.Invoke(this, "[:" + keyValuePair.Key + "]");
						dropDown.Close();
					}
				}
			}
		};
	}

	public void Show(Point p, ToolStripDropDownDirection d)
	{
		dropDown.Show(p, d);
		_emojiTable.Focus();
	}

	public void Close()
	{
		dropDown.Close();
	}
}
