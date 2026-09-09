using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppEditionEnterpriseManager : AppEditionGeneral
{
	public override int Code => 6;

	public override Image Icon => Resources.imgEnterprise;

	public override string Name => "业务管控";

	public override string Tooltip => string.Empty;

	public override string PlatformName => "AuditAI 业务管控平台";

	public override Image ProjectTileIcon => ProjectCardIconProvider.Project;

	public override Image SystemTemplateTileIcon => ProjectCardIconProvider.SystemTemplate;

	public override Image VipSystemTemplateTileIcon => ProjectCardIconProvider.VipTemplate;

	public override Image CustomTemplateTileIcon => ProjectCardIconProvider.CustomTemplate;

	public override Image CurrentProjectIcon => Resources.CurrentModule;

	public override Image CurrentSystemTemplateIcon => Resources.CurrentSystemTemplate;

	public override Image CurrentCustomTemplateIcon => Resources.CurrentTemplate;

	public override Image UseEmptyTemplateTileIcon => Resources.tileModule;
}
