using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppEditionTableDevelop : AppEditionGeneral
{
	public override int Code => 7;

	public override Image Icon => Resources.imgEnterprise;

	public override string Name => "表格开发";

	public override string Tooltip => string.Empty;

	public override string PlatformName => "AuditAI 表格开发平台";

	public override Image ProjectTileIcon => Resources.tileModule;

	public override Image SystemTemplateTileIcon => Resources.tileTemplate;

	public override Image VipSystemTemplateTileIcon => Resources.vipTemplate;

	public override Image CustomTemplateTileIcon => Resources.tileModule;

	public override Image CurrentProjectIcon => Resources.CurrentModule;

	public override Image CurrentSystemTemplateIcon => Resources.CurrentSystemTemplate;

	public override Image CurrentCustomTemplateIcon => Resources.CurrentTemplate;

	public override Image UseEmptyTemplateTileIcon => Resources.tileModule;
}
