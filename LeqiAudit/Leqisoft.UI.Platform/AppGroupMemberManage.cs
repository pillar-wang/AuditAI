using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupMemberManage : AppCommandGroup
{
	public override string Text => "同事及成员管理";

	public override Image Image => Resources.Users;

	public AppGroupMemberManage()
	{
		base.Commands.Add(AppCommands.TeamUsers);
		base.Commands.Add(AppCommands.ProjectEdit);
		base.Commands.Add(AppCommands.AccessControl);
	}
}
