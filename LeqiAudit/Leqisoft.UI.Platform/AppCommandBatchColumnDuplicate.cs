using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandBatchColumnDuplicate : AppCommandButton
{
	public override Image LargeIcon => Resources.BatchColumnDuplicate;

	public override string Text => "跨表批量复制列";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.BatchColumnDuplicate();
	}
}
