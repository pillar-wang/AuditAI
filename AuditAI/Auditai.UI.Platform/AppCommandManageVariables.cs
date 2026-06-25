using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandManageVariables : AppCommandButton
{
	public override string Text => "变量管理";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ReferenceManager;

	protected override string Tooltip => TipResource.变量管理按钮;

	protected override void Clicked()
	{
		Program.MainForm.EditReferences();
	}
}
