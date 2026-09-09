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

	public override Image ProjectTileIcon => ProjectCardIconProvider.Project;

	public override Image SystemTemplateTileIcon => ProjectCardIconProvider.SystemTemplate;

	public override Image VipSystemTemplateTileIcon => ProjectCardIconProvider.VipTemplate;

	public override Image CustomTemplateTileIcon => ProjectCardIconProvider.CustomTemplate;

	public override Image CurrentProjectIcon => Resources.CurrentProject;

	public override Image CurrentSystemTemplateIcon => Resources.CurrentSystemTemplate;

	public override Image CurrentCustomTemplateIcon => Resources.CurrentTemplate;

	public override Image UseEmptyTemplateTileIcon => IconRes.UseEmptyTemplate;
}
