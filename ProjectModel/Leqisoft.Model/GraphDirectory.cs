using System.Collections.Generic;
using System.Drawing;

namespace Leqisoft.Model;

public class GraphDirectory : GraphNode
{
	public bool IsExpanded { get; set; }

	public List<GraphNode> Children { get; } = new List<GraphNode>();


	public int ExpandedHeight { get; set; }

	public void CollapseAll()
	{
		IsExpanded = false;
		base.Rect = Rectangle.Empty;
		foreach (GraphNode child in Children)
		{
			child.Rect = Rectangle.Empty;
			if (child is GraphDirectory graphDirectory)
			{
				graphDirectory.CollapseAll();
			}
		}
	}
}
