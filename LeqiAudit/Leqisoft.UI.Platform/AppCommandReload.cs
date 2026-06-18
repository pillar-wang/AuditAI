using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandReload : AppCommandButton
{
	public override Image SmallIcon => ContextResources.ctxReloadFile;

	protected override string Tooltip => TipResource.Ribbon菜单_主窗体右上角配置栏_重新载入;

	protected override void Clicked()
	{
		Program.MainForm.ProjectHierarchy.ReloadNode();
	}
}
