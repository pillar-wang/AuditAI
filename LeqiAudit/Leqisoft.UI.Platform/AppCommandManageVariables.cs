using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandManageVariables : AppCommandButton
{
	public override string Text => "变量管理";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ReferenceManager;

	protected override string Tooltip => TipResource.变量管理按钮;

	protected override void Clicked()
	{
		Program.MainForm.EditReferences();
	}
}
