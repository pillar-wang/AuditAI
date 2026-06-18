using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandFileBatchPrint : AppCommandButton
{
	public override string Text => "批量打印";

	public override Image LargeIcon => Resources.BatchPrint;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.BatchPrint_Click(Text);
	};
}
