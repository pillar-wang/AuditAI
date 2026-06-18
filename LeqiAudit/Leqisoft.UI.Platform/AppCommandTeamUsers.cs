using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTeamUsers : AppCommandButton
{
	public override Image LargeIcon => Resources.Users;

	public override string Text => "增减同事";

	protected override void Clicked()
	{
		Program.ManageUsers();
	}
}
