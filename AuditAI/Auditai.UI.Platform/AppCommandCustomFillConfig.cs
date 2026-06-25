using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandCustomFillConfig : AppCommandButton
{
	public override string Text => "填充配置";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.TableCollect;

	protected override Func<Task> ClickedTask => delegate
	{
		return Task.Run(async () =>
		{
			Program.MainForm.ShowCustomFillConfig();
		});
	};

	protected override string Tooltip => TipResource.采数填充按钮;
}