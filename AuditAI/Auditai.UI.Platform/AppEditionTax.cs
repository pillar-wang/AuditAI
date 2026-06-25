using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppEditionTax : AppEditionBase
{
	public override int Code => 3;

	public override Image Icon => Resources.tileTax;

	public override string Name => "税务师事务所";

	public override string Tooltip => "适用于税务师事务所搭建云审计底稿平台系统，软件中已内置了税务师事务所企业所得税审计、土地增值税审计等常用工作底稿模板，供税务师事务所搭建自主底稿模板时做参考。";

	public override string PlatformName => "AuditAI 审计协作平台";

	public override Image ProjectTileIcon => Resources.tile;

	public override Image SystemTemplateTileIcon => Resources.tileTemplate;

	public override Image VipSystemTemplateTileIcon => Resources.vipTemplate;

	public override Image CustomTemplateTileIcon => Resources.customTemplate;

	public override Image CurrentProjectIcon => Resources.CurrentProject;

	public override Image CurrentSystemTemplateIcon => Resources.CurrentSystemTemplate;

	public override Image CurrentCustomTemplateIcon => Resources.CurrentTemplate;

	public override Image UseEmptyTemplateTileIcon => Resources.UseEmptyTemplate;
}
