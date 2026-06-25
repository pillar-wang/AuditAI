using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandGenerateConfirmation : AppCommandButton
{
	public override string Text => "生成函证";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ConfirmationGenerate;

	protected override Func<Task> ClickedTask => () => Program.MainForm.GenerateConfirmationFromDocument();

	protected override string Tooltip => TipResource.高级功能菜单_文档_生成函证;
}
