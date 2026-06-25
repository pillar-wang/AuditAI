using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandReload : AppCommandButton
{
	public override Image SmallIcon => ContextResources.ctxReloadFile;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_重新载入;

	protected override void Clicked()
	{
		Program.MainForm.ProjectHierarchy.ReloadNode();
	}
}
