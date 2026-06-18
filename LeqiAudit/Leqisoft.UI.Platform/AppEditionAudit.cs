﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppEditionAudit : AppEditionBase
{
	public override int Code => 2;

	public override Image Icon => Resources.tileAccountant;

	public override string Name => "会计师事务所";

	public override string Tooltip => "适用于会计师事务所搭建云审计底稿平台系统，软件中已内置了会计师事务所财务报表审计及其他专项审计的常用工作底稿模板，供会计师事务所搭建自主底稿模板时做参考。";

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
