using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandProjectMembers : AppCommandButton
{
	public override string Text => "增减成员";

	public override Image LargeIcon => Resources.ProjectEditor;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.ModifyCurrentProjectMembers();
	};
}
