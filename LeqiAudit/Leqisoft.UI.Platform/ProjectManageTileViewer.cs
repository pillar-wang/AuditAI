using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class ProjectManageTileViewer
{
	private Template titleTemplate;

	private Template projectTemplate;

	private dlgProjectManagement owner;

	private Leqisoft.DTO.Project parentProject;

	private readonly object TAG_CREATEPROJECTTILE = new object();

	private Pen selectBorderPen = new Pen(Theme.SelectedLeqiTheme.ThemeContext.DarkColor, 4f);

	private SortKind SortKind
	{
		get
		{
			return owner.Style.SortKind;
		}
		set
		{
			owner.Style.SortKind = value;
		}
	}

	public List<Leqisoft.DTO.Project> Projects { get; set; }

	public C1TileControlEx View { get; private set; }

	public Leqisoft.DTO.Project SelectedProject => (View.SelectedTile?.Tag as Tuple<Leqisoft.DTO.Project, bool>)?.Item1;

	private System.Drawing.Image ProjectTileImage => Program.MainForm.CurrentEdition.ProjectTileIcon;

	private System.Drawing.Image SystemTemplateTileImage => Program.MainForm.CurrentEdition.SystemTemplateTileIcon;

	private System.Drawing.Image CustomTemplateTileImage => Program.MainForm.CurrentEdition.CustomTemplateTileIcon;

	public event EventHandler<Leqisoft.DTO.Project> OpenProject;

	public ProjectManageTileViewer(dlgProjectManagement dlgProject)
	{
		owner = dlgProject;
		Initialize();
	}

	private void View_DoubleClickTile(object sender, Tile e)
	{
		OnOpenProject((e.Tag as Tuple<Leqisoft.DTO.Project, bool>)?.Item1);
	}

	private bool validRelativeArea(Tile tile, Rectangle rec)
	{
		Point point = View.PointToClient(Control.MousePosition);
		int num = tile.Group.X + tile.X;
		int num2 = tile.Group.Y + tile.Y + View.ScrollOffset;
		int x = point.X;
		int y = point.Y;
		int num3 = x - num;
		int num4 = y - num2;
		return num3 > rec.X && num3 < rec.X + rec.Width && num4 > rec.Y && num4 < rec.Y + rec.Height;
	}

	private void OnOpenProject(Leqisoft.DTO.Project project)
	{
		try
		{
			View.Enabled = false;
			this.OpenProject?.Invoke(this, project);
		}
		finally
		{
			View.Enabled = true;
		}
	}

	private void AppendGroup(string title, IEnumerable<Leqisoft.DTO.Project> projects)
	{
		View.Groups.Add(CreateTitleGroup(title));
		View.Groups.Add(CreateProjectGroup(projects));
	}

	private C1.Win.C1Tile.Group CreateProjectGroup(IEnumerable<Leqisoft.DTO.Project> projects)
	{
		C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
		foreach (Leqisoft.DTO.Project project in projects)
		{
			group.Tiles.Add(CreateProjectTile(project));
		}
		return group;
	}

	private C1.Win.C1Tile.Group CreateTitleGroup(string title)
	{
		C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
		Tile tile = new Tile
		{
			Text = title,
			HorizontalSize = 20,
			VerticalSize = 2,
			Template = titleTemplate,
			BackColor = Color.Transparent
		};
		tile.Paint += delegate(object s1, PaintEventArgs e1)
		{
			SizeF sizeF = e1.Graphics.MeasureString(title, new Font("微软雅黑", 9f));
			e1.Graphics.DrawLine(new Pen(Color.FromArgb(0, 73, 92), 1f), new Point(8, tile.Height - 9), new Point(8 + (int)sizeF.Width, tile.Height - 9));
		};
		group.Tiles.Add(tile);
		return group;
	}

	private Tile CreateProjectTile(Leqisoft.DTO.Project project)
	{
		bool flag = Projects.Any((Leqisoft.DTO.Project p) => p.ParentId == project.Id);
		return new Tile
		{
			Tag = Tuple.Create(project, flag),
			VerticalSize = 4,
			HorizontalSize = 5,
			Text = project.Name,
			Template = projectTemplate,
			Image1 = ((project.Type == ProjectType.Project) ? ProjectTileImage : (project.SystemBuild ? SystemTemplateTileImage : CustomTemplateTileImage)),
			Image2 = (flag ? Resources.subProject : null)
		};
	}

	private Template CreateTemplateProject()
	{
		Template template = new Template();
		template.Description = "Win32";
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
		panelElement.Margin = new Padding(0, 20, 0, 0);
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
		textElement.FixedHeight = 40;
		textElement.FixedWidth = 130;
		panelElement3.Children.Add(textElement);
		panelElement3.FixedHeight = 40;
		panelElement3.FixedWidth = 130;
		template.Elements.Add(panelElement);
		template.Elements.Add(panelElement2);
		template.Elements.Add(panelElement3);
		template.Name = "mapImgTemplate";
		return template;
	}

	private Template CreateTemplateTitle()
	{
		Template template = new Template();
		template.Description = "Subgroup";
		PanelElement panelElement = new PanelElement();
		panelElement.AlignmentOfContents = ContentAlignment.BottomLeft;
		PanelElement panelElement2 = new PanelElement();
		panelElement2.BackColor = Color.FromArgb(0, 73, 92);
		panelElement2.Dock = DockStyle.Bottom;
		panelElement2.FixedHeight = 1;
		TextElement textElement = new TextElement();
		textElement.ForeColor = Color.Black;
		textElement.ForeColorSelector = ForeColorSelector.Unbound;
		textElement.Font = new Font("微软雅黑", 9f, FontStyle.Regular);
		textElement.Margin = new Padding(0, 0, 0, 6);
		textElement.SingleLine = true;
		panelElement.Children.Add(textElement);
		panelElement.Dock = DockStyle.Fill;
		panelElement.Padding = new Padding(8, 0, 8, 8);
		template.Elements.Add(panelElement);
		template.Enabled = false;
		template.Name = "subgroupTemplate";
		return template;
	}

	private void Initialize()
	{
		View = new C1TileControlEx
		{
			CellWidth = 10,
			CellHeight = 10,
			AllowChecking = false,
			Dock = DockStyle.Fill,
			CellSpacing = 20,
			Margin = new Padding(0),
			Padding = new Padding(0),
			GroupPadding = new Padding(0),
			Orientation = LayoutOrientation.Vertical,
			TileBorderColor = Color.White,
			GroupSpacing = 5
		};
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			KeepRelativeSize = false,
			Resizable = false,
			MinHeight = 10,
			Height = 25
		};
		projectTemplate = CreateTemplateProject();
		titleTemplate = CreateTemplateTitle();
		View.Templates.Add(projectTemplate);
		View.Templates.Add(titleTemplate);
		View.DoubleClickTile += View_DoubleClickTile;
		Theme.SetCurrentTree(View);
		SetTheme();
	}

	private void SetTheme()
	{
		View.TileBorderColor = Color.Transparent;
		View.CustomBorderColor = Theme.SelectedLeqiTheme.ThemeContext.DarkColor;
	}

	public void Populate(bool isTemplate)
	{
		if (Projects == null)
		{
			return;
		}
		View.ClearSelected();
		View.BeginUpdate();
		try
		{
			View.Groups.Clear();
			if (parentProject == null)
			{
				owner.pnlPrevious.Visible = false;
				if (!(Program.MainForm.CurrentEdition is AppEditionGeneral))
				{
					IEnumerable<string> recent = ProjectInfoManager.GetInstance().GetRecent();
					AppendGroup("最近使用", from i in recent
						select Projects.Find((Leqisoft.DTO.Project p) => p.Id.ToString() == i) into p
						where p != null
						select p);
				}
				if (owner.Style.SortKind == SortKind.Category)
				{
					IOrderedEnumerable<IGrouping<string, Leqisoft.DTO.Project>> orderedEnumerable = from p in Projects
						where !p.ParentId.HasValue
						group p by p.Category into g
						orderby g.Key
						select g;
					foreach (IGrouping<string, Leqisoft.DTO.Project> item2 in orderedEnumerable)
					{
						AppendGroup(item2.Key, item2);
					}
				}
				else
				{
					AppendGroup(isTemplate ? "所有模板" : ("所有" + StringConstBase.Current.Project), owner.SortProjectImpl(Projects.Where((Leqisoft.DTO.Project p) => !p.ParentId.HasValue), SortKind));
					if (Program.MainForm.CurrentEdition is AppEditionGeneral && !SoftwareLicenseManager.IsAddProjectOutOfLicenseLimit())
					{
						Tile item = new Tile
						{
							Tag = TAG_CREATEPROJECTTILE,
							VerticalSize = 4,
							HorizontalSize = 5,
							Text = "新建" + StringConstBase.Current.Project,
							Template = projectTemplate,
							Image1 = Resources.CreateModule
						};
						View.Groups[1].Tiles.Insert(0, item);
					}
				}
				Theme.SetCurrentTree(View);
				SetTheme();
				return;
			}
			owner.pnlPrevious.Visible = true;
			if (owner.Style.SortKind == SortKind.Category)
			{
				IOrderedEnumerable<IGrouping<string, Leqisoft.DTO.Project>> orderedEnumerable2 = from p in Projects
					where p.ParentId == parentProject.Id
					group p by p.Category into g
					orderby g.Key
					select g;
				{
					foreach (IGrouping<string, Leqisoft.DTO.Project> item3 in orderedEnumerable2)
					{
						AppendGroup(item3.Key, item3);
					}
					return;
				}
			}
			IEnumerable<Leqisoft.DTO.Project> projects = owner.SortProjectImpl(Projects.Where((Leqisoft.DTO.Project p) => p.ParentId == parentProject.Id), SortKind);
			AppendGroup(isTemplate ? "所有模板" : ("所有" + StringConstBase.Current.Project), projects);
		}
		finally
		{
			View.EndUpdate();
		}
	}

	public void Populate(IEnumerable<Leqisoft.DTO.Project> projects, bool isTemplate)
	{
		if (projects == null)
		{
			return;
		}
		View.ClearSelected();
		View.BeginUpdate();
		try
		{
			View.Groups.Clear();
			AppendGroup(isTemplate ? "模板列表" : (StringConstBase.Current.Project + "列表"), projects);
		}
		finally
		{
			View.EndUpdate();
		}
	}

	public void PreviousPage()
	{
		if (Projects != null)
		{
			parentProject = Projects.Find(delegate(Leqisoft.DTO.Project p)
			{
				Guid id = p.Id;
				Guid? obj = parentProject?.ParentId;
				return id == obj;
			});
			Populate(parentProject?.TemplateId.HasValue ?? false);
		}
	}
}
