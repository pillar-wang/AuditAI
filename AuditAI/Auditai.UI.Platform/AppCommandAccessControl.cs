using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandAccessControl : AppCommandButton
{
	public override System.Drawing.Image LargeIcon => Auditai.UI.Platform.Properties.Resources.AccessControl;

	public override string Text => "权限控制";

	protected override string Tooltip => TipResource.Project管理_权限控制;

	protected override void Clicked()
	{
		Program.MainForm.AccessControl();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!Auditai.Model.Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id))
		{
			Enabled = false;
		}
		else if (Auditai.Model.Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id).Value == UserRole.Manager)
		{
			Enabled = true;
		}
		else
		{
			Enabled = false;
		}
	}
}
