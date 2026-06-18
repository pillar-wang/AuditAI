using System;
using System.Windows.Forms;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.Util;

namespace Leqisoft.UI.Platform;

public class TeamMergeMessage : IActionMessage
{
	public TeamMergeMessage(string userName, string name, string desManagerId)
	{
		ActionDeal = async delegate
		{
			if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "（" + userName + "）" + name + "向您发来合并组织请求，您同意后，组织内的同事及项目将被合并到（" + userName + "）" + name + "创建的组织中，您创建的组织将被解散，现在确定要被合并吗？", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					await WebApiClient.AllowTeamMerge(desManagerId);
					Leqisoft.DTO.User user = await WebApiClient.GetUserById(Leqisoft.Model.User.Current.Id);
					Leqisoft.Model.User.Current.TeamId = user.TeamId;
					Leqisoft.Model.User.Current.IsTeamAdmin = false;
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "组织合并完成！");
				}
				catch (Exception ex)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
				}
			}
		};
	}
}
