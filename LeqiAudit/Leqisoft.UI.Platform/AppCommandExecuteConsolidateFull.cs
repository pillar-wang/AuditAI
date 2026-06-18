using System;
using System.Threading.Tasks;

namespace Leqisoft.UI.Platform;

public class AppCommandExecuteConsolidateFull : AppCommandButton
{
	public override string Text => "过程样式";

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.ExecuteConsolidate(showDataCols: true);
	};
}
