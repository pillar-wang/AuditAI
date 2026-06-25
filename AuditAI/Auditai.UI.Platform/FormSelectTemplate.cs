﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1Sizer;
using C1.Win.C1Tile;
using Auditai.LocalDataStore;
using Auditai.DTO;
using Auditai.Model;
using Auditai.PlatformResource;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class FormSelectTemplate
{
	private readonly C1RibbonForm _form;

	private readonly C1Sizer _szMain;

	private readonly C1Sizer _szSearch;

	private readonly C1CheckBox _ckbSearch;

	private readonly C1TextBox _txbSearch;

	private readonly C1TileControlEx _tileControl;

	private readonly Template _tplTile;

	private readonly C1.Win.C1Tile.Group _tg;

	private readonly List<Auditai.DTO.Project> _templates = new List<Auditai.DTO.Project>();

	public Auditai.DTO.Project ResultTemplate { get; private set; }

	public FormSelectTemplate()
	{
		_form = FormFactory.Create();
		_form.Text = StringConstBase.Current.SelectTemplate;
		_form.ShowInTaskbar = false;
		_form.Size = new Size(900, 650);
		_form.Load += _form_Load;
		_form.Shown += _form_Shown;
		_tileControl = new C1TileControlEx
		{
			CellWidth = 180,
			CellHeight = 120,
			CellSpacing = 20,
			Padding = Padding.Empty,
			Orientation = LayoutOrientation.Vertical,
			Dock = DockStyle.Fill
		};
		_tg = new C1.Win.C1Tile.Group();
		_tileControl.Groups.Add(_tg);
		_tplTile = CreateTemplateProject();
		_tileControl.Templates.Add(_tplTile);
		_tileControl.DoubleClickTile += _tileControl_DoubleClickTile;
		_szMain = new C1Sizer
		{
			Dock = DockStyle.Fill,
			SplitterWidth = 0,
			Padding = Padding.Empty
		};
		_szMain.Grid.Columns.Count = 1;
		_szMain.Grid.Rows.Count = 1;
		_szMain.Grid.Columns.SetSizes(new int[1] { 1 });
		_szMain.Grid.Rows.SetSizes(new int[1] { 1 });
		_form.Controls.Add(_tileControl);
		_szSearch = new C1Sizer
		{
			SplitterWidth = 0,
			Padding = Padding.Empty
		};
		_szSearch.Grid.Rows.Count = 1;
		_szSearch.Grid.Columns.Count = 2;
		_szSearch.Grid.Columns.SetSizes(new int[2] { 1, 1 });
		_szSearch.Grid.Columns.SetFixed(default(int));
		_ckbSearch = new C1CheckBox
		{
			BackColor = Color.Transparent,
			BackgroundImage = Resources.btnSearch,
			BackgroundImageLayout = ImageLayout.Center,
			Appearance = Appearance.Button,
			FlatStyle = FlatStyle.Flat
		};
		_ckbSearch.FlatAppearance.BorderSize = 0;
		_szSearch.AddControl(_ckbSearch, 0, 0);
		_txbSearch = new C1TextBox();
		_szSearch.AddControl(_txbSearch, 0, 1);
	}

	public DialogResult ShowDialog()
	{
		return _form.ShowDialog();
	}

	private async Task<IEnumerable<Auditai.DTO.Project>> GetCurrentPlatformTemplates()
	{
		List<Auditai.DTO.Project> list = (await GetCurrentPlatformTemplatesImpl()).ToList();
		list.Sort(delegate(Auditai.DTO.Project left, Auditai.DTO.Project right)
		{
			string text = left.Number;
			string text2 = right.Number;
			if (text == null)
			{
				text = string.Empty;
			}
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			return text.CompareTo(text2);
		});
		return list;
	}

	private async Task<IEnumerable<Auditai.DTO.Project>> GetCurrentPlatformTemplatesImpl()
	{
		if (!StorageRouter.IsLocalMode)
		{
			IEnumerable<Auditai.DTO.Project> enumerable = await WebApiClient.GetTemplates();
			if (Program.ClientPlatformType == PlatformType.TableDevelopPlatform || Program.ClientPlatformType == PlatformType.EnterpriseReportPlatform)
			{
				return enumerable.Where((Auditai.DTO.Project u) => u.SystemBuild);
			}
			if (Program.ClientPlatformType == PlatformType.Custom && ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("only_select_system_template", defaultValue: true))
			{
				return enumerable.Where((Auditai.DTO.Project u) => u.SystemBuild);
			}
			return enumerable;
		}
		// 本地模式：通过 StorageRouter 获取本地模板
		return await StorageRouter.GetTemplates();
	}

	private async Task Populate()
	{
		_tileControl.ClearExternalImage();
		_templates.Clear();
		List<Auditai.DTO.Project> templates = _templates;
		templates.AddRange(await GetCurrentPlatformTemplates());
		_tg.Tiles.Clear();
		_tg.Tiles.Add(new Tile
		{
			HorizontalSize = 1,
			VerticalSize = 1,
			Text = "（" + StringConstBase.Current.NotUseTemplate + "）",
			Template = _tplTile,
			Image1 = Program.MainForm.CurrentEdition.UseEmptyTemplateTileIcon,
			Visible = SoftwareLicenseManager.IsAllowShowEmptyTemplateTile()
		});
		foreach (Auditai.DTO.Project template in _templates)
		{
			Tile tile = new Tile
			{
				HorizontalSize = 1,
				VerticalSize = 1,
				Text = template.Name,
				Template = _tplTile,
				Image1 = Program.MainForm.CurrentEdition.CustomTemplateTileIcon
			};
			_tg.Tiles.Add(tile);
			if (SoftwareLicenseManager.IsTemplateTileShowRightTopImage())
			{
				_tileControl.AddRightTopImage(tile, Resources.iconSample);
			}
		}
	}

	private Template CreateTemplateProject()
	{
		Template template = new Template();
		PanelElement panelElement = new PanelElement();
		panelElement.Alignment = ContentAlignment.TopCenter;
		ImageElement imageElement = new ImageElement();
		imageElement.AlignmentOfContents = ContentAlignment.TopCenter;
		imageElement.FixedHeight = 50;
		imageElement.FixedWidth = 50;
		imageElement.ImageSelector = ImageSelector.Image1;
		panelElement.Children.Add(imageElement);
		panelElement.FixedHeight = 50;
		panelElement.FixedWidth = 50;
		panelElement.Margin = new Padding(0, 30, 0, 0);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.Alignment = ContentAlignment.MiddleRight;
		ImageElement imageElement2 = new ImageElement();
		imageElement2.AlignmentOfContents = ContentAlignment.MiddleRight;
		imageElement2.FixedHeight = 30;
		imageElement2.FixedWidth = 40;
		imageElement2.ImageSelector = ImageSelector.Image2;
		panelElement2.Children.Add(imageElement2);
		panelElement2.FixedHeight = 30;
		panelElement2.FixedWidth = 40;
		panelElement2.Margin = new Padding(0, 0, 3, 0);
		PanelElement panelElement3 = new PanelElement();
		panelElement3.Alignment = ContentAlignment.BottomCenter;
		TextElement textElement = new TextElement();
		textElement.AlignmentOfContents = ContentAlignment.TopCenter;
		textElement.TextTrimming = TextTrimming.EndEllipsis;
		textElement.SingleLine = false;
		textElement.FixedHeight = 50;
		textElement.FixedWidth = 150;
		panelElement3.Children.Add(textElement);
		panelElement3.FixedHeight = 50;
		panelElement3.FixedWidth = 150;
		template.Elements.Add(panelElement);
		template.Elements.Add(panelElement2);
		template.Elements.Add(panelElement3);
		return template;
	}

	private async void _form_Load(object sender, EventArgs e)
	{
		Theme.SetCurrentTree(_form);
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Templates);
		_tileControl.TileBorderColor = Color.Transparent;
		_szMain.Grid.Rows[0].Size = _ckbSearch.BackgroundImage.Height;
		_szSearch.Grid.Columns[0].Size = _ckbSearch.BackgroundImage.Width;
		await Populate();
	}

	private void _tileControl_DoubleClickTile(object sender, Tile e)
	{
		ResultTemplate = ((e.Index > 0) ? _templates[e.Index - 1] : null);
		_form.DialogResult = DialogResult.OK;
	}

	private void _form_Shown(object sender, EventArgs e)
	{
	}
}
