using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMoveDown : AppCommandButton
{
	public override string Text => "下移位置";

	public override Image LargeIcon => Resources.MoveDown;

	protected override void Clicked()
	{
		Program.MainForm.ProjectHierarchy.MoveDownNode();
	}
}
