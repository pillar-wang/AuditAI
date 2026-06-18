using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupProjects : AppCommandGroup
{
	public override string Text => StringConstBase.Current.Project + "管理";

	public override System.Drawing.Image Image => Resources.ProjectManager;

	public AppGroupProjects()
	{
		base.Commands.Add(AppCommands.ManageProjects);
		base.Commands.Add(AppCommands.SyncProject);
	}
}
