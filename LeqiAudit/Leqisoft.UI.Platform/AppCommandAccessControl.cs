using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAccessControl : AppCommandButton
{
	public override System.Drawing.Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.AccessControl;

	public override string Text => "权限控制";

	protected override string Tooltip => TipResource.Project管理_权限控制;

	protected override void Clicked()
	{
		Program.MainForm.AccessControl();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!Leqisoft.Model.Project.Current.Users.Any((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == Leqisoft.Model.User.Current.Id))
		{
			Enabled = false;
		}
		else if (Leqisoft.Model.Project.Current.Users.First((KeyValuePair<Leqisoft.DTO.User, UserRole> kv) => kv.Key.Id == Leqisoft.Model.User.Current.Id).Value == UserRole.Manager)
		{
			Enabled = true;
		}
		else
		{
			Enabled = false;
		}
	}
}
