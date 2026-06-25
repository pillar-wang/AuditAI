using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandBatchColumnRemove : AppCommandButton
{
	public override Image LargeIcon => Resources.BatchColumnRemove;

	public override string Text => "跨表批量删除列";

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.BatchColumnRemove();
	}
}
