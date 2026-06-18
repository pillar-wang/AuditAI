using System.Collections.Generic;

namespace Leqisoft.Model;

public class GraphGroup
{
	public TreeGroup TreeGroup { get; set; }

	public List<GraphNode> Children { get; } = new List<GraphNode>();

}
