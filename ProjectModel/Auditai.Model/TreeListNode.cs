using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class TreeListNode
{
	public string Text { get; set; }

	public List<TreeListNode> Children { get; } = new List<TreeListNode>();


	public TreeListNode AddOrGet(string s)
	{
		TreeListNode treeListNode = Children.FirstOrDefault((TreeListNode n) => n.Text == s);
		if (treeListNode == null)
		{
			treeListNode = new TreeListNode
			{
				Text = s
			};
			Children.Add(treeListNode);
		}
		return treeListNode;
	}
}
