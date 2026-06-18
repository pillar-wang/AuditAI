using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandChangePassword : AppCommandButton
{
	public override string Text => "修改密码";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.PwdEdit;

	protected override string Tooltip => TipResource.密码修改按钮;

	protected override void Clicked()
	{
		Program.MainForm.AlterPwd();
	}
}
