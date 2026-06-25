using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandExecuteConsolidate : AppCommandMenu
{
	public override string Text => "合并报表结果";

	public override Image LargeImage => Auditai.UI.Platform.Properties.Resources.ConsolidateStatements;

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
