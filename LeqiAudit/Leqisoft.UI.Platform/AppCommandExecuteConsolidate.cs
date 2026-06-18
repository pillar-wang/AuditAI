using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandExecuteConsolidate : AppCommandMenu
{
	public override string Text => "跨项目汇总数据结果";

	public override Image LargeImage => Leqisoft.UI.Platform.Properties.Resources.ConsolidateStatements;

	protected override string Tooltip => TipResource.合并报表按钮;

	public AppCommandExecuteConsolidate()
		: base(new AppCommandBase[2]
		{
			AppCommands.ExecuteConsolidateFull,
			AppCommands.ExecuteConsolidateBrief
		})
	{
	}
}
