using System.Windows.Forms;

namespace Auditai.UI.Controls;

public interface C1TileCustomButtonRender
{
	void OnPaint(PaintEventArgs e);

	bool OnTileSingleClicked(TileMouseEventArgs e);
}
