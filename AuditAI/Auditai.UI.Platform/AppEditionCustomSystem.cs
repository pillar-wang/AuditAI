using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppEditionCustomSystem : AppEditionGeneral
{
	public int EditionCode { get; private set; }

	public string ApplicationName { get; private set; }

	public Image Team_Icon { get; set; }

	public Image Current_Project_Icon { get; set; }

	public Image Current_System_Template_Icon { get; set; }

	public Image Current_Custom_Template_Icon { get; set; }

	public Image Project_Tile_Icon { get; set; }

	public Image System_Template_Tile_Icon { get; set; }

	public Image Vip_System_Template_Tile_Icon { get; set; }

	public Image Custom_Template_Tile_Icon { get; set; }

	public Image Use_Empty_Template_Tile_Icon { get; set; }

	public Image Payed_Template_Tile_Corner_Icon { get; set; }

	public Image UnPay_Template_Tile_Corner_Icon { get; set; }

	public override int Code => EditionCode;

	public override string Name => ApplicationName;

	public override string PlatformName => ApplicationName;

	public override Image Icon
	{
		get
		{
			if (Team_Icon != null)
			{
				return Team_Icon;
			}
			return Resources.imgEnterprise;
		}
	}

	public override string Tooltip => string.Empty;

	public override Image ProjectTileIcon
	{
		get
		{
			if (Project_Tile_Icon != null)
			{
				return Project_Tile_Icon;
			}
			return Resources.tileModule;
		}
	}

	public override Image SystemTemplateTileIcon
	{
		get
		{
			if (System_Template_Tile_Icon != null)
			{
				return System_Template_Tile_Icon;
			}
			return Resources.tileTemplate;
		}
	}

	public override Image VipSystemTemplateTileIcon
	{
		get
		{
			if (Vip_System_Template_Tile_Icon != null)
			{
				return Vip_System_Template_Tile_Icon;
			}
			return Resources.vipTemplate;
		}
	}

	public override Image CustomTemplateTileIcon
	{
		get
		{
			if (Custom_Template_Tile_Icon != null)
			{
				return Custom_Template_Tile_Icon;
			}
			return Resources.tileModule;
		}
	}

	public override Image CurrentProjectIcon
	{
		get
		{
			if (Current_Project_Icon != null)
			{
				return Current_Project_Icon;
			}
			return Resources.tileModule;
		}
	}

	public override Image CurrentSystemTemplateIcon
	{
		get
		{
			if (Current_System_Template_Icon != null)
			{
				return Current_System_Template_Icon;
			}
			return Resources.CurrentSystemTemplate;
		}
	}

	public override Image CurrentCustomTemplateIcon
	{
		get
		{
			if (Current_Custom_Template_Icon != null)
			{
				return Current_Custom_Template_Icon;
			}
			return Resources.CurrentTemplate;
		}
	}

	public override Image UseEmptyTemplateTileIcon
	{
		get
		{
			if (Use_Empty_Template_Tile_Icon != null)
			{
				return Use_Empty_Template_Tile_Icon;
			}
			return Resources.tileModule;
		}
	}

	public override Image PayedTemplateTileCornerIcon => Payed_Template_Tile_Corner_Icon;

	public override Image UnPayTemplateTileCornerIcon => UnPay_Template_Tile_Corner_Icon;

	public AppEditionCustomSystem(int editionCode, string appName)
	{
		EditionCode = editionCode;
		ApplicationName = appName;
	}
}
