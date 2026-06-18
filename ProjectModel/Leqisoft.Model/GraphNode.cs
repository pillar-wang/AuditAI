using System.Drawing;

namespace Leqisoft.Model;

public class GraphNode
{
	public int Id { get; set; }

	public GraphGroup GraphGroup { get; set; }

	public GraphDirectory Parent { get; set; }

	public TreeNodeBase ModelNode { get; set; }

	public Rectangle Rect { get; set; }
}
