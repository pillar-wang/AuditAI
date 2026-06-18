using System;
using System.Threading.Tasks;

namespace Leqisoft.UI.Platform;

public class AppCommandExecuteConsolidateBrief : AppCommandButton
{
	public override string Text => "结果样式";

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.ExecuteConsolidate(showDataCols: false);
	};
}
