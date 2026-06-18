using System;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls;

public class TreeNodeEventArgs : EventArgs
{
	public TreeNodeBase TreeNode { get; }

	public TreeNodeEventArgs(TreeNodeBase tnb)
	{
		TreeNode = tnb;
	}
}
