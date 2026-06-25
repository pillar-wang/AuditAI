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

	public override Image ProjectTileIcon => Resources.tileModule;

	public override Image SystemTemplateTileIcon => Resources.tileTemplate;

	public override Image VipSystemTemplateTileIcon => Resources.vipTemplate;

	public override Image CustomTemplateTileIcon => Resources.tileModule;

	public override Image CurrentProjectIcon => Resources.CurrentModule;

	public override Image CurrentSystemTemplateIcon => Resources.CurrentSystemTemplate;

	public override Image CurrentCustomTemplateIcon => Resources.CurrentTemplate;

	public override Image UseEmptyTemplateTileIcon => Resources.tileModule;
}
