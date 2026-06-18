using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandProjectMembersSmall : AppCommandButton
{
	public override Image SmallIcon => Leqisoft.UI.Platform.Properties.Resources.ProjectEditor16;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.ModifyCurrentProjectMembers();
	};

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_增减成员;
}
