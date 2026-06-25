using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTableStyleBrush : AppCommandButton
{
	public override string Text => "样式刷";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.TableStyleBrush;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.TableEditor.TableStyleBrush();
	};

	protected override string Tooltip => TipResource.高级功能_样式刷;
}
