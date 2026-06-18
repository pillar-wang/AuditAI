using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandExecuteCollect : AppCommandButton
{
	public override string Text => "采账填充";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.GenerateWorkingPaper;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.AutoImport();
	};

	protected override string Tooltip => TipResource.采数填充按钮;
}
