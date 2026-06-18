using System.Drawing;

namespace Leqisoft.UI.Platform;

public class TreeNodeCacheState
{
	public TreeNodeCacheKind Kind { get; set; }

	public Point ScrollPosition { get; set; }

	public Rectangle Selection { get; set; }
}
