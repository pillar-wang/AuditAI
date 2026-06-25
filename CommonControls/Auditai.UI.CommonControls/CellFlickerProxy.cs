using System.Drawing;
using C1.Win.C1FlexGrid;

namespace Auditai.UI.CommonControls;

public class CellFlickerProxy : AbstractFlickerProxy
{
	private C1FlexGrid grid;

	private int row;

	private int col;

	public CellFlickerProxy(C1FlexGrid g, int r, int c)
	{
		grid = g;
		row = r;
		col = c;
		twinkleContent = GetContent();
		orignContent = GetContent();
	}

	public override bool IsDisposed()
	{
		if (grid != null)
		{
			return grid.IsDisposed;
		}
		return true;
	}

	protected override string GetContent()
	{
		return grid[row, col]?.ToString() ?? string.Empty;
	}

	protected override Image GetImage()
	{
		return null;
	}

	protected override void SetView(Image image, string content)
	{
		grid[row, col] = content;
	}
}
