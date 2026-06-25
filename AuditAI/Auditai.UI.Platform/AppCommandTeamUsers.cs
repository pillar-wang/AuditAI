using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTeamUsers : AppCommandButton
{
	public override Image LargeIcon => Resources.Users;

	public override string Text => "增减同事";

	protected override void Clicked()
	{
		Program.ManageUsers();
	}
}
