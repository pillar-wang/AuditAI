using System;
using System.Threading.Tasks;

namespace Auditai.UI.Platform;

public class AppCommandExecuteConsolidateBrief : AppCommandButton
{
	public override string Text => "汇总结果";

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.ExecuteConsolidate(showDataCols: false);
	};
}
