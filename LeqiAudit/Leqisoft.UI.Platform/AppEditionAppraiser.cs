﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppEditionAppraiser : AppEditionBase
{
	public override int Code => 4;

	public override Image Icon => Resources.tileAppraiser;

	public override string Name => "评估师事务所";

	public override string Tooltip => string.Empty;

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
