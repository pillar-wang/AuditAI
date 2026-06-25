using System.Windows.Forms;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class PushNodeMessage : IActionMessage
{
	public PushNodeMessage(string fromName, long nodeId, string nodeName, TreeNodeBase treeNode)
	{
		base.Content = treeNode;
		ActionDeal = async delegate(object node)
		{
			ProjectHierarchy hierarchy = Program.MainForm.ProjectHierarchy;
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, fromName + "向您推送来《" + nodeName + "》，是否现在打开？", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				if (node is TreeNodeBase node2)
				{
					Program.MainForm.SwitchMainView();
					hierarchy.FindAndSelectNode(node2);
				}
				else if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "尚未同步，是否现在同步" + StringConstBase.Current.Project + "？", MessageBoxButtons.OKCancel) == DialogResult.OK)
				{
					await Program.MainForm.SyncCurrentProject();
					hierarchy.FindAndSelectNode(hierarchy.FindNode(null)?.UserData as TreeNodeBase);
				}
			}
		};
	}
}
