using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandNodesIndexEdit : AppCommandButton
{
	public override Image LargeIcon => Resources.EditNodesNumber;

	public override string Text => "批量编辑索引号";

	protected override void Clicked()
	{
		Program.MainForm.NodesIndexEdit();
	}
}
