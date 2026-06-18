﻿using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1SplitContainer;
using C1.Win.C1Themes;
using Leqisoft.ThemeResource.Properties;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public static class Theme
{
	private static Pen _borderPen;

	public static Pen _themeBorderPen;

	private static Pen _penGridTop;

	private static LeqiTheme _selectedLeqiTheme;

	private static bool _isAdapted;

	private static Bitmap _adaptImage;

	public static List<LeqiTheme> ThemePool { get; set; }

	public static LeqiTheme SelectedLeqiTheme
	{
		get
		{
			return _selectedLeqiTheme ?? ThemePool[0];
		}
		set
		{
			_isAdapted = false;
			_selectedLeqiTheme = value;
			try
			{
				_borderPen = new Pen(SelectedLeqiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color"), 1f);
				_penGridTop.Color = SelectedLeqiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Fixed\\Border\\Color");
				_themeBorderPen.Color = SelectedLeqiTheme.GetC1Theme().GetColor("C1Input\\C1Button\\Default\\Border\\Color");
			}
			catch
			{
			}
		}
	}

	public static Bitmap CurrentBackgroudImage
	{
		get
		{
			if (_isAdapted)
			{
				return _adaptImage;
			}
			Rectangle bounds = Screen.PrimaryScreen.Bounds;
			Bitmap largeImage = SelectedLeqiTheme.ThemeContext.LargeImage;
			if (_adaptImage != null)
			{
				_adaptImage.Dispose();
			}
			_adaptImage = ConvertSize(largeImage, bounds.Width, bounds.Height);
			_isAdapted = true;
			return _adaptImage;
		}
	}

	static Theme()
	{
		_borderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);
		_themeBorderPen = new Pen(Color.Black, 1f);
		_penGridTop = new Pen(Color.DarkGray, 1f);
		_isAdapted = false;
		ThemePool = new List<LeqiTheme>();
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_Office2013LightGray, "leqi_Office2013LightGray", "AuditAI蓝", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(164, 206, 240),
			TileColor = Color.FromArgb(255, 23, 131, 217),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(250, 250, 250),
			LineColor = Color.FromArgb(0, 101, 189),
			DarkColor = Color.FromArgb(0, 101, 189),
			BulletColor = Color.FromArgb(255, 0, 101, 189),
			RibbonTabBorder = Color.FromArgb(198, 198, 198),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_Office2013Red, "leqi_Office2013Red", "墨染青", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(173, 219, 223),
			TileColor = Color.FromArgb(255, 28, 136, 152),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(255, 255, 255),
			LineColor = Color.FromArgb(38, 150, 163),
			ProgressBarColor = Color.FromArgb(38, 150, 163),
			DarkColor = Color.FromArgb(0, 104, 125),
			BulletColor = Color.FromArgb(255, 0, 104, 125),
			RibbonTabBorder = Color.FromArgb(170, 170, 170),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_Office2013Green, "leqi_Office2013Green", "春苔绿", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(178, 203, 189),
			TileColor = Color.FromArgb(255, 33, 115, 69),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(255, 255, 255),
			LineColor = Color.FromArgb(33, 115, 69),
			DarkColor = Color.FromArgb(33, 115, 69),
			BulletColor = Color.FromArgb(255, 33, 115, 69),
			RibbonTabBorder = Color.FromArgb(170, 170, 170),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_Office2016Blue, "leqi_Office2016Blue", "深空蓝", ThemeEnum.Typical | ThemeEnum.WhiteIcon, new ThemeContext
		{
			GradientColor = Color.FromArgb(168, 191, 225),
			TileColor = Color.FromArgb(255, 42, 87, 154),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(250, 250, 250),
			LineColor = Color.FromArgb(62, 179, 245),
			DarkColor = Color.FromArgb(25, 71, 138),
			BulletColor = Color.FromArgb(255, 42, 87, 154),
			RibbonTabBorder = Color.FromArgb(198, 198, 198),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_Office2016Red, "leqi_Office2016Red", "琥珀红", ThemeEnum.Typical | ThemeEnum.WhiteIcon, new ThemeContext
		{
			GradientColor = Color.FromArgb(249, 222, 217),
			TileColor = Color.FromArgb(255, 184, 72, 43),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(255, 255, 255),
			LineColor = Color.FromArgb(221, 71, 47),
			DarkColor = Color.FromArgb(158, 61, 36),
			BulletColor = Color.FromArgb(255, 184, 72, 43),
			RibbonTabBorder = Color.FromArgb(170, 170, 170),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_Office2016Green, "leqi_Office2016Green", "翡翠绿", ThemeEnum.Typical | ThemeEnum.WhiteIcon, new ThemeContext
		{
			GradientColor = Color.FromArgb(198, 230, 213),
			TileColor = Color.FromArgb(255, 34, 116, 71),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(255, 255, 255),
			LineColor = Color.FromArgb(33, 115, 69),
			DarkColor = Color.FromArgb(10, 99, 50),
			BulletColor = Color.FromArgb(255, 34, 116, 71),
			RibbonTabBorder = Color.FromArgb(170, 170, 170),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Blue, "leqi_VS2013Blue", "宝石蓝", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(207, 214, 229),
			TileColor = Color.FromArgb(255, 164, 177, 202),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(214, 219, 233),
			LineColor = Color.FromArgb(77, 96, 130),
			DarkColor = Color.FromArgb(54, 78, 111),
			BulletColor = Color.FromArgb(0, 72, 102),
			RibbonTabBorder = Color.FromArgb(101, 101, 101),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Light, "leqi_VS2013Light", "铂金银", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(184, 202, 229),
			TileColor = Color.FromArgb(238, 238, 242),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(238, 238, 242),
			LineColor = Color.FromArgb(224, 224, 224),
			DarkColor = Color.FromArgb(72, 97, 140),
			BulletColor = Color.FromArgb(40, 131, 243),
			RibbonTabBorder = Color.FromArgb(204, 206, 219),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Tan, "leqi_VS2013Tan", "温暖棕", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(255, 240, 208),
			TileColor = Color.FromArgb(255, 116, 105, 75),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(226, 220, 202),
			LineColor = Color.FromArgb(116, 105, 75),
			DarkColor = Color.FromArgb(116, 105, 75),
			BulletColor = Color.FromArgb(255, 116, 105, 75),
			RibbonTabBorder = Color.FromArgb(102, 81, 0),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Red, "leqi_VS2013Red", "深酒红", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(226, 202, 202),
			TileColor = Color.FromArgb(116, 75, 75),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(226, 202, 202),
			LineColor = Color.FromArgb(116, 75, 75),
			DarkColor = Color.FromArgb(116, 75, 75),
			BulletColor = Color.FromArgb(116, 75, 75),
			RibbonTabBorder = Color.FromArgb(110, 96, 96),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Green, "leqi_VS2013Green", "清新绿", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(217, 228, 218),
			TileColor = Color.FromArgb(87, 120, 90),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(207, 221, 208),
			LineColor = Color.FromArgb(87, 120, 90),
			DarkColor = Color.FromArgb(87, 120, 90),
			BulletColor = Color.FromArgb(87, 120, 90),
			RibbonTabBorder = Color.FromArgb(101, 101, 101),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Purple, "leqi_VS2013Purple", "魅惑紫", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(210, 201, 225),
			TileColor = Color.FromArgb(110, 89, 150),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(211, 202, 226),
			LineColor = Color.FromArgb(110, 89, 150),
			DarkColor = Color.FromArgb(110, 89, 150),
			BulletColor = Color.FromArgb(110, 89, 150),
			RibbonTabBorder = Color.FromArgb(85, 43, 85),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Blue, "leqi_VS2013Blue", "流光蓝", ThemeEnum.Typical | ThemeEnum.Picture, new ThemeContext
		{
			GradientColor = Color.FromArgb(207, 214, 229),
			TileColor = Color.FromArgb(191, 222, 227),
			LargeImage = Leqisoft.UI.Controls.Properties.Resource1.BluePic,
			SmallImage = Leqisoft.UI.Controls.Properties.Resource1.smlBluePic,
			BackColor = Color.FromArgb(214, 219, 233),
			LineColor = Color.FromArgb(77, 96, 130),
			DarkColor = Color.FromArgb(77, 96, 130),
			BulletColor = Color.FromArgb(0, 72, 102),
			RibbonTabBorder = Color.FromArgb(101, 101, 101),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Light, "leqi_VS2013Light", "炫动银", ThemeEnum.Typical | ThemeEnum.Picture, new ThemeContext
		{
			GradientColor = Color.FromArgb(184, 202, 229),
			TileColor = Color.FromArgb(208, 208, 208),
			LargeImage = Leqisoft.UI.Controls.Properties.Resource1.LightPic,
			SmallImage = Leqisoft.UI.Controls.Properties.Resource1.smlLightPic,
			BackColor = Color.FromArgb(238, 238, 242),
			LineColor = Color.FromArgb(224, 224, 224),
			DarkColor = Color.FromArgb(72, 97, 140),
			BulletColor = Color.FromArgb(40, 131, 243),
			RibbonTabBorder = Color.FromArgb(204, 206, 219),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Tan, "leqi_VS2013Tan", "灯光橙", ThemeEnum.Typical | ThemeEnum.Picture, new ThemeContext
		{
			GradientColor = Color.FromArgb(255, 240, 208),
			TileColor = Color.FromArgb(243, 224, 184),
			LargeImage = Leqisoft.UI.Controls.Properties.Resource1.OrangePic,
			SmallImage = Leqisoft.UI.Controls.Properties.Resource1.smlOrangePic,
			BackColor = Color.FromArgb(226, 220, 202),
			LineColor = Color.FromArgb(116, 105, 75),
			DarkColor = Color.FromArgb(116, 105, 75),
			BulletColor = Color.FromArgb(255, 116, 105, 75),
			RibbonTabBorder = Color.FromArgb(102, 81, 0),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Red, "leqi_VS2013Red", "圣诞红", ThemeEnum.Typical | ThemeEnum.Picture, new ThemeContext
		{
			GradientColor = Color.FromArgb(226, 202, 202),
			TileColor = Color.FromArgb(255, 194, 218),
			LargeImage = Leqisoft.UI.Controls.Properties.Resource1.RedPic2,
			SmallImage = Leqisoft.UI.Controls.Properties.Resource1.smlRedPic2,
			BackColor = Color.FromArgb(226, 202, 202),
			LineColor = Color.FromArgb(116, 75, 75),
			DarkColor = Color.FromArgb(116, 75, 75),
			BulletColor = Color.FromArgb(116, 75, 75),
			RibbonTabBorder = Color.FromArgb(110, 96, 96),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Green, "leqi_VS2013Green", "田野绿", ThemeEnum.Typical | ThemeEnum.Picture, new ThemeContext
		{
			GradientColor = Color.FromArgb(217, 228, 218),
			TileColor = Color.FromArgb(190, 219, 161),
			LargeImage = Leqisoft.UI.Controls.Properties.Resource1.GreenPic,
			SmallImage = Leqisoft.UI.Controls.Properties.Resource1.smlGreenPic,
			BackColor = Color.FromArgb(207, 221, 208),
			LineColor = Color.FromArgb(87, 120, 90),
			DarkColor = Color.FromArgb(87, 120, 90),
			BulletColor = Color.FromArgb(87, 120, 90),
			RibbonTabBorder = Color.FromArgb(101, 101, 101),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_VS2013Purple, "leqi_VS2013Purple", "花香紫", ThemeEnum.Typical | ThemeEnum.Picture, new ThemeContext
		{
			GradientColor = Color.FromArgb(210, 201, 225),
			TileColor = Color.FromArgb(255, 245, 253),
			LargeImage = Leqisoft.UI.Controls.Properties.Resource1.RedPic,
			SmallImage = Leqisoft.UI.Controls.Properties.Resource1.smlRedPic,
			BackColor = Color.FromArgb(211, 202, 226),
			LineColor = Color.FromArgb(110, 89, 150),
			DarkColor = Color.FromArgb(110, 89, 150),
			BulletColor = Color.FromArgb(110, 89, 150),
			RibbonTabBorder = Color.FromArgb(85, 43, 85),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_MacBlue, "leqi_MacBlue", "苹果蓝", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(187, 204, 216),
			TileColor = Color.FromArgb(255, 223, 230, 241),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(250, 250, 250),
			LineColor = Color.FromArgb(176, 201, 214),
			DarkColor = Color.FromArgb(72, 97, 140),
			BulletColor = Color.FromArgb(72, 97, 140),
			RibbonTabBorder = Color.FromArgb(198, 198, 198),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
		RegisterTheme(Leqisoft.ThemeResource.Properties.Resource1.leqi_MacSilver, "leqi_MacSilver", "苹果银", ThemeEnum.Typical, new ThemeContext
		{
			GradientColor = Color.FromArgb(187, 204, 216),
			TileColor = Color.FromArgb(233, 233, 233),
			LargeImage = null,
			SmallImage = null,
			BackColor = Color.FromArgb(250, 250, 250),
			LineColor = Color.FromArgb(233, 233, 233),
			DarkColor = Color.FromArgb(72, 97, 140),
			BulletColor = Color.FromArgb(72, 97, 140),
			RibbonTabBorder = Color.FromArgb(198, 198, 198),
			FormulaEditorBorderColor = Color.FromArgb(200, 200, 200)
		});
	}

	public static void SelectedThemeById(string themeId)
	{
		if (string.IsNullOrEmpty(themeId))
		{
			themeId = ThemePool[0].Id;
		}
		SelectedLeqiTheme = ThemePool.Find((LeqiTheme t) => t.Id == themeId);
		ResetTableTicketViewModelPanelThemeContext(SelectedLeqiTheme.FriendName);
		ResetOutBarPageMoreMenuImageContext(SelectedLeqiTheme.FriendName);
		ResetGridMoreMenuImageContext(SelectedLeqiTheme.FriendName);
	}

	public static void SetCurrentTree(Control form)
	{
		C1ThemeController.ApplyThemeToControlTree(form, SelectedLeqiTheme.GetC1Theme());
	}

	public static void SetCurrentObject(object control)
	{
		C1ThemeController.ApplyThemeToObject(control, SelectedLeqiTheme.GetC1Theme());
	}

	public static void DrawFormBorder(this C1FlexGrid grd, Graphics g)
	{
		g.DrawRectangle(_borderPen, 0, 0, grd.Width - 1, grd.Height - 1);
		g.DrawLine(_penGridTop, 0, 0, grd.Width, 0);
	}

	public static void DrawGridBorder(this C1SplitterPanel panel, Graphics g, C1FlexGrid grid)
	{
		Rectangle bounds = grid.Bounds;
		bounds.Inflate(1, 1);
		g.DrawRectangle(_borderPen, bounds);
	}

	private static void RegisterTheme(byte[] theme, string themeName, string friendName, ThemeEnum themeFlags, ThemeContext context)
	{
		LeqiTheme item = new LeqiTheme
		{
			Id = ThemePool.Count.ToString(),
			Name = themeName,
			FriendName = friendName,
			ThemeFlags = themeFlags,
			ThemeBytes = theme,
			ThemeContext = context
		};
		ThemePool.Add(item);
	}

	public static Bitmap ConvertSize(Bitmap srcMap, int desWidth, int desHeight)
	{
		Bitmap bitmap = new Bitmap(desWidth, desHeight);
		Rectangle destRect = new Rectangle(0, 0, desWidth, desHeight);
		Rectangle srcRect = new Rectangle(0, 0, srcMap.Width, srcMap.Height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(srcMap, destRect, srcRect, GraphicsUnit.Pixel);
		return bitmap;
	}

	private static void ResetTableTicketViewModelPanelThemeContext(string themeName)
	{
		TableTicketViewModeThemeContext tableTicketViewModeThemeContext = new TableTicketViewModeThemeContext();
		SelectedLeqiTheme.ThemeContext.TableTicketViewModePanelContext = tableTicketViewModeThemeContext;
		tableTicketViewModeThemeContext.HotBackColor = SelectedLeqiTheme.GetBackgroundSolidColor("C1Command\\C1ToolBar\\Button\\Hot\\Background");
		tableTicketViewModeThemeContext.HotForeColor = SelectedLeqiTheme.GetC1Theme().GetColor("C1Command\\C1OutBar\\Page\\Title\\Default\\ForeColor");
		tableTicketViewModeThemeContext.CheckedBackColor = tableTicketViewModeThemeContext.HotBackColor;
		tableTicketViewModeThemeContext.CheckedForeColor = tableTicketViewModeThemeContext.HotForeColor;
		if (themeName == null)
		{
			return;
		}
		int length = themeName.Length;
		if (length != 3)
		{
			return;
		}
		char c = themeName[0];
		if ((uint)c <= 28201u)
		{
			switch (c)
			{
			default:
				return;
			case '宝':
				if (!(themeName == "宝石蓝"))
				{
					return;
				}
				break;
			case '温':
				if (!(themeName == "温暖棕"))
				{
					return;
				}
				break;
			case '深':
				if (!(themeName == "深酒红"))
				{
					if (themeName == "深空蓝")
					{
						tableTicketViewModeThemeContext.HotBackColor = SelectedLeqiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background");
						tableTicketViewModeThemeContext.CheckedBackColor = tableTicketViewModeThemeContext.HotBackColor;
						tableTicketViewModeThemeContext.CheckedForeColor = SelectedLeqiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\ForeColor");
						tableTicketViewModeThemeContext.HotForeColor = tableTicketViewModeThemeContext.CheckedForeColor;
					}
					return;
				}
				break;
			case '清':
				if (!(themeName == "清新绿"))
				{
					return;
				}
				break;
			case '流':
				if (!(themeName == "流光蓝"))
				{
					return;
				}
				break;
			case '圣':
				if (!(themeName == "圣诞红"))
				{
					return;
				}
				break;
			}
		}
		else if ((uint)c <= 30000u)
		{
			if (c != '灯')
			{
				if (c == '琥')
				{
					if (!(themeName == "琥珀红"))
					{
						return;
					}
					goto IL_023d;
				}
				if (c != '田' || !(themeName == "田野绿"))
				{
					return;
				}
			}
			else if (!(themeName == "灯光橙"))
			{
				return;
			}
		}
		else
		{
			if (c == '翡')
			{
				if (!(themeName == "翡翠绿"))
				{
					return;
				}
				goto IL_023d;
			}
			switch (c)
			{
			default:
				return;
			case '魅':
				if (!(themeName == "魅惑紫"))
				{
					return;
				}
				break;
			case '花':
				if (!(themeName == "花香紫"))
				{
					return;
				}
				break;
			}
		}
		tableTicketViewModeThemeContext.CheckedForeColor = SelectedLeqiTheme.GetC1Theme().GetColor("BaseThemeProperties\\Styles\\Content\\ForeColor");
		tableTicketViewModeThemeContext.HotForeColor = tableTicketViewModeThemeContext.CheckedForeColor;
		return;
		IL_023d:
		tableTicketViewModeThemeContext.HotBackColor = SelectedLeqiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background");
		tableTicketViewModeThemeContext.CheckedBackColor = tableTicketViewModeThemeContext.HotBackColor;
		tableTicketViewModeThemeContext.CheckedForeColor = SelectedLeqiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Highlight\\ForeColor");
		tableTicketViewModeThemeContext.HotForeColor = tableTicketViewModeThemeContext.CheckedForeColor;
	}

	private static void ResetOutBarPageMoreMenuImageContext(string themeName)
	{
		switch (themeName)
		{
		case "翡翠绿":
		case "花香紫":
		case "魅惑紫":
		case "灯光橙":
		case "琥珀红":
		case "田野绿":
		case "深空蓝":
		case "深酒红":
		case "清新绿":
		case "温暖棕":
		case "圣诞红":
		case "宝石蓝":
		case "流光蓝":
			SelectedLeqiTheme.ThemeContext.OutBarPageMoreMenuImageIndex = OutBarPageMoreMenuImageIndex.White;
			break;
		default:
			SelectedLeqiTheme.ThemeContext.OutBarPageMoreMenuImageIndex = OutBarPageMoreMenuImageIndex.Default;
			break;
		}
	}

	private static void ResetGridMoreMenuImageContext(string themeName)
	{
		switch (themeName)
		{
		case "AuditAI蓝":
		case "墨染青":
		case "春苔绿":
		case "苹果蓝":
		case "苹果银":
			SelectedLeqiTheme.ThemeContext.GridMoreMenuImageIndexOnHighLightRow = GridMoreMenuImageIndex.White;
			break;
		default:
			SelectedLeqiTheme.ThemeContext.GridMoreMenuImageIndexOnHighLightRow = GridMoreMenuImageIndex.Default;
			break;
		}
	}
}
