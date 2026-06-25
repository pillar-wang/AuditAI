using System;
using Auditai.Model;

namespace Auditai.UI.Controls;

public class TreeNodeEventArgs : EventArgs
{
	public TreeNodeBase TreeNode { get; }

	public TreeNodeEventArgs(TreeNodeBase tnb)
	{
		TreeNode = tnb;
	}
}
