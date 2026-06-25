using System.Collections.Generic;

namespace Auditai.Model;

public class GraphGroup
{
	public TreeGroup TreeGroup { get; set; }

	public List<GraphNode> Children { get; } = new List<GraphNode>();

}
