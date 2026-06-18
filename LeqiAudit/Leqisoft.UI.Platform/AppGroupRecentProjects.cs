using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class AppGroupRecentProjects : AppCommandGroup
{
	public override string Text => "已打开" + StringConstBase.Current.Project;
}
