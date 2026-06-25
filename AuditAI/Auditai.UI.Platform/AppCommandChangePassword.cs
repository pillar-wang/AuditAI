using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandChangePassword : AppCommandButton
{
	public override string Text => "修改密码";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.PwdEdit;

	protected override string Tooltip => TipResource.密码修改按钮;

	protected override void Clicked()
	{
		Program.MainForm.AlterPwd();
	}
}
