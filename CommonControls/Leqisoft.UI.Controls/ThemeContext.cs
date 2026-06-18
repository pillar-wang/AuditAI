using System.Drawing;

namespace Leqisoft.UI.Controls;

public class ThemeContext
{
	public TableTicketViewModeThemeContext TableTicketViewModePanelContext;

	public OutBarPageMoreMenuImageIndex OutBarPageMoreMenuImageIndex;

	public GridMoreMenuImageIndex GridMoreMenuImageIndexOnHighLightRow;

	private Color _progressBarColor = Color.Empty;

	public Color TileColor { get; set; }

	public Bitmap LargeImage { get; set; }

	public Bitmap SmallImage { get; set; }

	public Color LineColor { get; set; }

	public Color BackColor { get; set; }

	public Color GradientColor { get; set; }

	public Color DarkColor { get; set; }

	public Color BulletColor { get; set; }

	public Color RibbonTabBorder { get; set; }

	public Color FormulaEditorBorderColor { get; set; }

	public Color ProgressBarColor
	{
		get
		{
			if (!(_progressBarColor == Color.Empty))
			{
				return _progressBarColor;
			}
			return DarkColor;
		}
		set
		{
			_progressBarColor = value;
		}
	}
}
