using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupUserManage : AppCommandGroup
{
	public override string Text => "用户资料";

	public override Image Image => Resources.Users;

	public AppGroupUserManage()
	{
		base.Commands.Add(AppCommands.UserInfo);
		base.Commands.Add(AppCommands.ChangePassword);
	}
}
