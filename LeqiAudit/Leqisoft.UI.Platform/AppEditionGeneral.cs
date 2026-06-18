﻿using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppEditionGeneral : AppEditionBase
{
	public override int Code => 1;

	public override Image Icon => Resources.imgEnterprise;

	public override string Name => "企业单位";

	public override string Tooltip => "适用于企业集团单位搭建云报表管理系统，包括：财务快报、决算报表、预算报表、管理报表、经营报表、考核报表等任意报表类型。针对合并财务报表应用场景，可实现合并财务报表及附注的自动生成。";

	public override string PlatformName => "AuditAI 集团报表平台";

	public override Image ProjectTileIcon => Resources.tile;

	public override Image SystemTemplateTileIcon => Resources.tileTemplate;

	public override Image VipSystemTemplateTileIcon => Resources.vipTemplate;

	public override Image CustomTemplateTileIcon => Resources.customTemplate;

	public override Image CurrentProjectIcon => Resources.CurrentProject;

	public override Image CurrentSystemTemplateIcon => Resources.CurrentSystemTemplate;

	public override Image CurrentCustomTemplateIcon => Resources.CurrentTemplate;

	public override Image UseEmptyTemplateTileIcon => Resources.UseEmptyTemplate;

	public override void GenerateRibbon()
	{
		base.GenerateRibbon();
		AppCommands.FillToTable.Button.Text = "填充至表格";
		AppCommandTabs.Advanced.RibbonTab.Groups.Remove(AppCommandGroups.Confirmation.RibbonGroup);
	}
}
