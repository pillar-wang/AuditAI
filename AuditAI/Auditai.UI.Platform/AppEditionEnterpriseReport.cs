using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppEditionEnterpriseReport : AppEditionGeneral
{
	public override int Code => 5;

	public override Image Icon => Resources.imgEnterprise;

	public override string Name => "集团报表";

	public override string Tooltip => string.Empty;

	public override string PlatformName => "AuditAI 集团报表平台";

	public override Image ProjectTileIcon => Resources.tileTableLib;

	public override Image SystemTemplateTileIcon => Resources.tileTemplate;

	public override Image VipSystemTemplateTileIcon => Resources.vipTemplate;

	public override Image CustomTemplateTileIcon => Resources.tileTableLib;

	public override Image CurrentProjectIcon => Resources.CurrentTableLib;

	public override Image CurrentSystemTemplateIcon => Resources.CurrentSystemTemplate;

	public override Image CurrentCustomTemplateIcon => Resources.CurrentTemplate;

	public override Image UseEmptyTemplateTileIcon => Resources.tileTableLib;
}
