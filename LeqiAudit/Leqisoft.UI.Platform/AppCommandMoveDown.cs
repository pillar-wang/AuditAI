using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMoveDown : AppCommandButton
{
	public override string Text => "下移位置";

	public override Image LargeIcon => Resources.MoveDown;

	protected override void Clicked()
	{
		Program.MainForm.ProjectHierarchy.MoveDownNode();
	}
}
