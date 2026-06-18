using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandBatchColumnRename : AppCommandButton
{
	public override Image LargeIcon => Resources.BatchColumnRename;

	public override string Text => "跨表批量重命名列";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.BatchColumnRename();
	}
}
