using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandCollectByColumn : AppCommandButton
{
	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.TableCollect;

	public override string Text => "列对应采账设置";

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableCollectSet();
	};

	protected override string Tooltip => TipResource.列对应采数设置按钮;
}
