using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandHideNodes : AppCommandButton
{
	public override string Text => "批量隐藏文件";

	public override Image LargeIcon => Resources.HideNodes;

	protected override void Clicked()
	{
		Program.MainForm.ShowHideNodes(0);
	}
}
