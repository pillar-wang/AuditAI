using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandCalculateAllTables : AppCommandButton
{
	public override string Text => "全部表运算";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.CalculateAllTables;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.CalcAllTables();
	};

	protected override string Tooltip => TipResource.全部表运算;
}
