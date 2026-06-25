using System;
using System.Windows.Forms;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class TeamMergeMessage : IActionMessage
{
	public TeamMergeMessage(string userName, string name, string desManagerId)
	{
		ActionDeal = async delegate
		{
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "（" + userName + "）" + name + "向您发来合并组织请求，您同意后，组织内的同事及项目将被合并到（" + userName + "）" + name + "创建的组织中，您创建的组织将被解散，现在确定要被合并吗？", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					await WebApiClient.AllowTeamMerge(desManagerId);
					Auditai.DTO.User user = await WebApiClient.GetUserById(Auditai.Model.User.Current.Id);
					Auditai.Model.User.Current.TeamId = user.TeamId;
					Auditai.Model.User.Current.IsTeamAdmin = false;
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "组织合并完成！");
				}
				catch (Exception ex)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
				}
			}
		};
	}
}
