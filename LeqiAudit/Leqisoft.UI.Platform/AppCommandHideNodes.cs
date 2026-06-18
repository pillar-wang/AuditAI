using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandHideNodes : AppCommandButton
{
	public override string Text => "批量隐藏文件";

	public override Image LargeIcon => Resources.HideNodes;

	protected override void Clicked()
	{
		Program.MainForm.ShowHideNodes(0);
	}
}
