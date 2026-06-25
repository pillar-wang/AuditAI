using System.Drawing;

namespace Auditai.UI.Platform;

public class TreeNodeCacheState
{
	public TreeNodeCacheKind Kind { get; set; }

	public Point ScrollPosition { get; set; }

	public Rectangle Selection { get; set; }
}
