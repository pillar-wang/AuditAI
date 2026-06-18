using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupFileManage : AppCommandGroup
{
	public override string Text => "文件管理";

	public override Image Image => Resources.HideNodes;

	public AppGroupFileManage()
	{
		base.Commands.Add(AppCommands.MoveUp);
		base.Commands.Add(AppCommands.MoveDown);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.HideNodes);
		base.Commands.Add(AppCommands.RemoveNodes);
		base.Commands.Add(AppCommands.NodesIndexEdit);
		base.Commands.Add(AppCommands.BatchExport);
		base.Commands.Add(AppCommands.TableBatchPrint);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.ManageSnapshots);
		base.Commands.Add(AppCommands.RecycleNode);
	}
}
