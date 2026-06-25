using System;
using System.Windows.Forms;
using C1.Win.C1Tile;

namespace Auditai.UI.Controls;

public class TileMouseEventArgs : EventArgs
{
	public Tile Tile { get; }

	public MouseEventArgs MouseEA { get; }

	public TileMouseEventArgs(MouseEventArgs m, Tile t)
	{
		Tile = t;
		MouseEA = m;
	}
}
