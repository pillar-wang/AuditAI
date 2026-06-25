using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandBatchExport : AppCommandButton
{
	public override string Text => "批量导出文件";

	public override Image LargeIcon => Resources.BatchExport;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.BatchExport(Text);
	};
}
