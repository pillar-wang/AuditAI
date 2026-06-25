using System;
using System.Threading.Tasks;

namespace Auditai.UI.Platform;

public class AppCommandExecuteConsolidateFull : AppCommandButton
{
	public override string Text => "工作底稿";

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.ExecuteConsolidate(showDataCols: true);
	};
}
