using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandBatchColumnDuplicate : AppCommandButton
{
	public override Image LargeIcon => Resources.BatchColumnDuplicate;

	public override string Text => "跨表批量复制列";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.BatchColumnDuplicate();
	}
}
