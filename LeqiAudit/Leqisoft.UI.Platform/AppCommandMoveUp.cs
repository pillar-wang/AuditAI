using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMoveUp : AppCommandButton
{
	public override string Text => "上移位置";

	public override Image LargeIcon => Resources.MoveUp;

	protected override void Clicked()
	{
		Program.MainForm.ProjectHierarchy.MoveUpNode();
	}
}
