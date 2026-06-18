using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandConsolidateSetting : AppCommandButton
{
	public override string Text => "跨项目汇总数据设置";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ConsolidateSettings;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.EditConsolidateSettings();
	};

	protected override string Tooltip => TipResource.合并设置按钮;
}
