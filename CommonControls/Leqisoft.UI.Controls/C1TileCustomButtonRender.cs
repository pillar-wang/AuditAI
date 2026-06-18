using System.Windows.Forms;

namespace Leqisoft.UI.Controls;

public interface C1TileCustomButtonRender
{
	void OnPaint(PaintEventArgs e);

	bool OnTileSingleClicked(TileMouseEventArgs e);
}
