﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.CommonControls;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;
using Newtonsoft.Json.Linq;
using Leqisoft.LocalDataStore;

namespace Leqisoft.UI.Platform;

public class dlgProjectManagement : C1RibbonForm
{
	private enum ViewState
	{
		Project,
		Template
	}

	private const string CN_PROJNUM = "Number";

	private const string CN_PROJNAME = "Name";

	private const string CN_PROJCAT = "Category";

	private const string CN_PROJLEADER = "Leaders";

	private const string CN_PROJASSIST = "Assistants";

	private const string CN_PROJCHECK = "Checkers";

	private const string CN_PROJNOTE = "Note";

	private const string CN_PROJAUDITEE = "Auditee";

	private const string CN_CREATOR = "Creator";

	private const string CN_TMPLEDITOR = "Editor";

	private const string CN_TMPLUSER = "User";

	public C1ContextMenu ProjectContextMenu = new C1ContextMenu();

	private C1CommandLink lnkSort = new C1CommandLink();

	private C1ContextMenu ctxSort = new C1ContextMenu();

	private C1CommandLink lnkSortByCreateTime = new C1CommandLink();

	private C1Command cmdSortByCreateTime = new C1Command();

	private C1CommandLink lnkSortByOpenTime = new C1CommandLink();

	private C1Command cmdSortByOpenTime = new C1Command();

	private C1CommandLink lnkSortByNumber = new C1CommandLink();

	private C1Command cmdSortByNumber = new C1Command();

	private C1CommandLink lnkSortByName = new C1CommandLink();

	private C1Command cmdSortByName = new C1Command();

	private C1CommandLink lnkSortByType = new C1CommandLink();

	private C1Command cmdSortByType = new C1Command();

	private C1Command cmdOpenProjectFromServer = new C1Command();

	private C1CommandLink lnkOpenProjectFromServer = new C1CommandLink();

	private TooltipBox _ttb = new TooltipBox();

	private TooltipManager tooltipManager = new TooltipManager();

	private ProjectManageTileViewer _tileViewer;

	private ProjectExport _projectExport;

	private bool _noAllowReentry;

	private List<Leqisoft.DTO.Project> _projects;

	private List<Leqisoft.DTO.Project> _templates;

	private RibbonImageProcess ImageProcess = new RibbonImageProcess();

	private readonly FlickerManager _flickerManager = new FlickerManager();

	private readonly C1CommandFlickerProxy _cmdProjectUserManageFlickerProxy;

	private readonly C1CommandFlickerProxy _cmdTemplateUserManageFlickerProxy;

	private readonly List<Tuple<string, C1Command>> Categories = new List<Tuple<string, C1Command>>();

	private IContainer components;

	private C1CommandHolder commandHolder;

	private C1SplitContainer splMain;

	private C1SplitterPanel pnlToolBar;

	private C1CommandDock commandDock;

	private C1ToolBar tbrProject;

	private C1CommandLink lnkCreateProject;

	private C1SplitterPanel pnlMain;

	private C1Command cmdCreateProject;

	private C1Command cmdOpenProject;

	private C1Command cmdModifyProject;

	private C1Command cmdDeleteProject;

	private C1CommandLink lnkOpenProject;

	private C1CommandLink lnkModifyProject;

	private C1CommandLink lnkDeleteProject;

	private C1Command cmdToTemplate;

	private C1CommandLink lnkToTemplate;

	private C1Command cmdCreateTemplate;

	private C1Command cmdOpenTemplate;

	private C1Command cmdModifyTemplate;

	private C1Command cmdDeleteTemplate;

	private C1Command cmdToProject;

	private C1ToolBar tbrTemplate;

	private C1CommandLink lnkCreateTemplate;

	private C1CommandLink lnkOpenTemplate;

	private C1CommandLink lnkModifyTemplate;

	private C1CommandLink lnkDeleteTemplate;

	private C1CommandLink lnkToProject;

	private C1Command cmdRefreshProject;

	private C1Command cmdRefreshTemplate;

	private C1CommandLink lnkRefreshProject;

	private C1CommandLink lnkRefreshTemplate;

	private C1Command cmdMembership;

	private C1CommandLink lnkMembership;

	private C1Command cmdDuplicateProject;

	private C1Command cmdSaveAsTemplate;

	private C1Command cmdDuplicateTemplate;

	private C1CommandLink lnkDuplicateTemplate;

	private C1CommandLink lnkDuplicateProject;

	private C1CommandLink lnkSaveAsTemplate;

	private C1Command cmdExportProject;

	private C1CommandLink lnkExportProject;

	private C1Command cmdExportProjectFile;

	private C1CommandLink lnkExportProjectFile;

	private C1Command cmdImportProject;

	private C1CommandLink lnkImportProject;

	private C1Command cmdDisplayStyle;

	private C1CommandLink lnkDisplayStyle;

	private C1CommandLink lnkDisplayStyleProject;

	private C1SplitContainer ctnMain;

	private C1SplitterPanel pnlTools;

	private C1SplitterPanel pnlProjectDisplay;

	private C1FlexGrid flxProjects;

	private C1SplitContainer ctnTools;

	public C1SplitterPanel pnlPrevious;

	private C1SplitterPanel pnlbtnSearch;

	private C1SplitterPanel pnlSearchBox;

	public C1Button btnPrevious;

	private C1TextBox txtSearch;

	private C1CheckBox btnSearch;

	private C1Command cmdUseTemplate;

	private C1CommandLink lnkUseTemplate;

	private C1Command cmdAlterPersonInfo;

	private C1CommandLink lnkAlterPersonInfo;

	private C1Command cmdTemplateUserManage;

	private C1Command cmdTemplateUserAlter;

	private C1CommandLink lnkTemplateUserManage;

	private C1CommandLink lnkTemplateUserAlter;

	private C1Command cmdChangePassword;

	private C1Command cmdChangePasswordT;

	private C1CommandLink lnkChangePasswordT;

	private C1CommandLink lnkChangePassword;

	private C1Command cmdHelpCenter;

	private C1CommandLink lnkHelpCenter;

	private C1CommandLink lnkHelpCenterTemplate;

	private C1SplitterPanel pnlCategory;

	private C1ToolBar tbrCategory;

	private C1CommandLink c1CommandLink1;

	private bool UserClosing { get; set; } = true;


	private ViewState State { get; set; }

	public DisplayStyle Style { get; set; } = new DisplayStyle();


	public Leqisoft.DTO.Project SelectedProject
	{
		get
		{
			if (Style.ViewMode == ListTileViewMode.List)
			{
				if (flxProjects.Row >= flxProjects.Rows.Fixed && flxProjects.Row < flxProjects.Rows.Count)
				{
					return flxProjects.Rows[flxProjects.Row].UserData as Leqisoft.DTO.Project;
				}
				return null;
			}
			if (Style.ViewMode == ListTileViewMode.Tile)
			{
				return _tileViewer?.SelectedProject;
			}
			return null;
		}
	}

	public dlgProjectManagement()
	{
		InitializeComponent();
		cmdCreateProject.Text = "新建" + StringConstBase.Current.Project;
		cmdOpenProject.Text = "打开" + StringConstBase.Current.Project;
		cmdModifyProject.Text = "修改" + StringConstBase.Current.Project;
		cmdDeleteProject.Text = "删除" + StringConstBase.Current.Project;
		cmdDuplicateProject.Text = "复制" + StringConstBase.Current.Project;
		cmdRefreshProject.Text = "刷新" + StringConstBase.Current.Project;
		cmdUseTemplate.Text = "基于模板创建" + StringConstBase.Current.Project;
		cmdToProject.Text = StringConstBase.Current.Project + "管理";
		cmdExportProject.Text = StringConstBase.Current.Project + "导出";
		cmdExportProjectFile.Text = "导出项目文件";
		cmdImportProject.Text = "导入项目";
		lnkDeleteProject.Text = "删除" + StringConstBase.Current.Project;
		Style.Load(ConfigManager.PROJECTMANAGEMENT_VIEWCONFIG);
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			SwitchTo(ListTileViewMode.Tile);
			Style.SortKind = SortKind.CreateTime;
		}
		else
		{
			SwitchTo(Style.ViewMode);
		}
		base.Width = Style.Width;
		base.Height = Style.Height;
		base.WindowState = (FormWindowState)Style.WindowState;
		base.StartPosition = FormStartPosition.CenterScreen;
		flxProjects.AfterRowColChange += FlxProjects_AfterRowColChange;
		flxProjects.Paint += delegate(object s1, PaintEventArgs e1)
		{
			flxProjects.DrawFormBorder(e1.Graphics);
		};
		flxProjects.MouseClick += FlxProjects_MouseClick;
		flxProjects.OwnerDrawCell += flxProjects_OwnerDrawCell;
		flxProjects.MouseDoubleClick += flxProjects_MouseDoubleClick;
		_tileViewer = new ProjectManageTileViewer(this);
		_tileViewer.OpenProject += _tileViewer_OpenProject;
		pnlProjectDisplay.Controls.Add(_tileViewer.View);
		base.SizeChanged += delegate
		{
			Style.Width = base.Width;
			Style.Height = base.Height;
			Style.WindowState = base.WindowState;
		};
		cmdOpenProjectFromServer.Text = "全新打开" + StringConstBase.Current.Project;
		cmdOpenProjectFromServer.CommandStateQuery += CmdOpenProjectFromServer_CommandStateQuery;
		cmdOpenProjectFromServer.Click += CmdOpenProjectFromServer_Click;
		lnkOpenProjectFromServer.Command = cmdOpenProjectFromServer;
		ProjectContextMenu.CommandLinks.Add(lnkOpenProjectFromServer);
		cmdSortByCreateTime.CommandStateQuery += CmdSortByCreateTime_CommandStateQuery;
		cmdSortByCreateTime.Click += CmdSortByCreateTime_Click;
		lnkSortByCreateTime.Command = cmdSortByCreateTime;
		ctxSort.CommandLinks.Add(lnkSortByCreateTime);
		cmdSortByOpenTime.CommandStateQuery += CmdSortByOpenTime_CommandStateQuery;
		cmdSortByOpenTime.Click += CmdSortByOpenTime_Click;
		lnkSortByOpenTime.Command = cmdSortByOpenTime;
		ctxSort.CommandLinks.Add(lnkSortByOpenTime);
		cmdSortByNumber.CommandStateQuery += CmdSortByNumber_CommandStateQuery;
		cmdSortByNumber.Click += CmdSortByNumber_Click;
		lnkSortByNumber.Command = cmdSortByNumber;
		ctxSort.CommandLinks.Add(lnkSortByNumber);
		cmdSortByName.CommandStateQuery += CmdSortByName_CommandStateQuery;
		cmdSortByName.Click += CmdSortByName_Click;
		lnkSortByName.Command = cmdSortByName;
		ctxSort.CommandLinks.Add(lnkSortByName);
		cmdSortByType.CommandStateQuery += CmdSortByType_CommandStateQuery;
		cmdSortByType.Click += CmdSortByType_Click;
		lnkSortByType.Command = cmdSortByType;
		ctxSort.CommandLinks.Add(lnkSortByType);
		ctxSort.Text = "排序方式";
		lnkSort.Command = ctxSort;
		ProjectContextMenu.CommandLinks.Add(lnkSort);
		foreach (C1CommandLink commandLink in tbrProject.CommandLinks)
		{
			if (commandLink.Command != C1Command.Empty)
			{
				ImageProcess.Register(new C1CommandAdapter(commandLink.Command));
			}
		}
		foreach (C1CommandLink commandLink2 in tbrTemplate.CommandLinks)
		{
			if (commandLink2.Command != C1Command.Empty)
			{
				ImageProcess.Register(new C1CommandAdapter(commandLink2.Command));
			}
		}
		Leqisoft.UI.Controls.Theme.SelectedThemeById(UserSet.Config.CurrentTheme);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		splMain.FixedLineWidth = 0;
		flxProjects.ExtendLastCol = true;
		flxProjects.Styles.Normal.TextAlign = TextAlignEnum.LeftCenter;
		flxProjects.Rows.DefaultSize = 30;
		AttachTooltip();
		cmdMembership.Text = (Leqisoft.Model.User.Current.IsTeamAdmin ? "同事管理" : "我的同事");
		cmdTemplateUserManage.Text = (Leqisoft.Model.User.Current.IsTeamAdmin ? "同事管理" : "我的同事");
		cmdModifyProject.Text = ((Program.MainForm.CurrentEdition is AppEditionGeneral) ? ("重命名" + StringConstBase.Current.Project) : "修改项目");
		_cmdProjectUserManageFlickerProxy = new C1CommandFlickerProxy(cmdMembership);
		_cmdProjectUserManageFlickerProxy.SetTimer(SecondTrigger.Trigger);
		_cmdTemplateUserManageFlickerProxy = new C1CommandFlickerProxy(cmdTemplateUserManage);
		_cmdTemplateUserManageFlickerProxy.SetTimer(SecondTrigger.Trigger);
	}

	private void FlxProjects_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			HitTestInfo hitTestInfo = flxProjects.HitTest(e.Location);
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				flxProjects.Select(hitTestInfo.Row, 0, hitTestInfo.Row, flxProjects.Cols.Count - 1, show: false);
			}
			cmdOpenProjectFromServer.Visible = SelectedProject != null && hitTestInfo.Row >= flxProjects.Rows.Fixed && hitTestInfo.Row < flxProjects.Rows.Count && flxProjects.Rows[hitTestInfo.Row].UserData == SelectedProject;
			ctxSort.Visible = true;
			ProjectContextMenu.ShowContextMenu(flxProjects, e.Location);
		}
	}

	private void SetTheme()
	{
		btnSearch.FlatStyle = FlatStyle.Flat;
		btnSearch.FlatAppearance.BorderSize = 0;
		btnSearch.ImageAlign = ContentAlignment.MiddleCenter;
		btnSearch.BackColor = Color.Transparent;
		btnSearch.FlatAppearance.CheckedBackColor = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeContext.TileColor;
		btnSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
		btnPrevious.FlatStyle = FlatStyle.Flat;
		btnPrevious.FlatAppearance.BorderSize = 0;
		ctnMain.SplitterWidth = 0;
		ctnTools.SplitterWidth = 0;
		LeqiTheme selectedLeqiTheme = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme;
		if (selectedLeqiTheme != null && selectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			ImageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			ImageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		ImageProcess.ProcessImage();
	}

	private async void _tileViewer_OpenProject(object sender, Leqisoft.DTO.Project e)
	{
		if (SelectedProject == null)
		{
			await CreateProject(null);
		}
		else
		{
			await OpenProject();
		}
	}

	private async void cmdCreateProject_Click(object sender, ClickEventArgs e)
	{
		cmdCreateProject.Enabled = false;
		await CreateProject(null);
		cmdCreateProject.Enabled = true;
	}

	private async void cmdCreateTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdCreateTemplate.Enabled = false;
		await CreateTemplate();
		cmdCreateTemplate.Enabled = true;
	}

	private void cmdCreateProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = !(Program.MainForm.CurrentEdition is AppEditionGeneral);
	}

	private void CmdOpenProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SelectedProject == null)
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private async void cmdOpenProject_Click(object sender, ClickEventArgs e)
	{
		cmdOpenProject.Enabled = false;
		await OpenProject();
		cmdOpenProject.Enabled = true;
	}

	private async void CmdOpenProjectFromServer_Click(object sender, ClickEventArgs e)
	{
		Leqisoft.DTO.Project selectedProject = SelectedProject;
		string text = ((selectedProject.Type == ProjectType.Project) ? StringConstBase.Current.Project : "模板");
		if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "全新打开" + text + "将自云端全新下载，您本地未上传的数据将被放弃，确定要全新打开" + text + "吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
		{
			return;
		}
		cmdOpenProjectFromServer.Enabled = false;
		string dbPathByGuid = MainForm.GetDbPathByGuid(selectedProject.Id);
		try
		{
			try
			{
				string localDataCacheDirectory = FileCacheManager.GetLocalDataCacheDirectory(selectedProject.Id);
				if (Directory.Exists(localDataCacheDirectory))
				{
					Directory.Delete(localDataCacheDirectory, recursive: true);
				}
				File.Delete(dbPathByGuid);
			}
			catch (Exception exception)
			{
				exception.Log("全新打开项目时，删除本地文件失败");
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该" + text + "处于打开状态，请重新登录再全新打开该" + text + "！");
				return;
			}
			MainForm.RecentProjects.Remove(selectedProject.Id);
			await OpenProject();
		}
		catch (Exception arg)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"删除本地文件过程中发生异常，详细信息：\n{arg}");
		}
		finally
		{
			cmdOpenProjectFromServer.Enabled = true;
		}
	}

	private void CmdOpenProjectFromServer_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		Leqisoft.DTO.Project selectedProject = SelectedProject;
		if (selectedProject == null)
		{
			cmdOpenProjectFromServer.Visible = false;
			return;
		}
		cmdOpenProjectFromServer.Text = "全新打开" + ((selectedProject.Type == ProjectType.Project) ? StringConstBase.Current.Project : "模板");
		cmdOpenProjectFromServer.Visible = selectedProject.Type == ProjectType.Project || CanOpenTemplate();
	}

	private bool CanOpenTemplate()
	{
		if (SelectedProject == null)
		{
			return false;
		}
		if (SelectedProject.Users == null)
		{
			return false;
		}
		Leqisoft.DTO.User user = SelectedProject.Users.FirstOrDefault((Leqisoft.DTO.User u) => u.Id == Leqisoft.Model.User.Current.Id);
		if (user == null)
		{
			return false;
		}
		return user.Role == UserRole.Editor;
	}

	private void CmdOpenTemplate_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SelectedProject == null)
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = CanOpenTemplate();
		}
	}

	private async void cmdOpenTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdOpenTemplate.Enabled = false;
		await OpenProject();
		cmdOpenTemplate.Enabled = true;
	}

	private void cmdExportProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SelectedProject == null)
		{
			e.Enabled = false;
		}
		else if (IsAssistant())
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private async void cmdExportProject_Click(object sender, ClickEventArgs e)
	{
		cmdExportProject.Enabled = false;
		await ExportProject();
		cmdExportProject.Enabled = true;
	}

	private void cmdExportProjectFile_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = true;
		// 仅本地模式可用；且需要选中项目
		if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			e.Enabled = false;
			return;
		}
		e.Enabled = SelectedProject != null;
	}

	private void cmdExportProjectFile_Click(object sender, ClickEventArgs e)
	{
		ExportProjectFile();
	}

	private void cmdImportProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = true;
		// 仅本地模式可用；不需要选中项目
		e.Enabled = Leqisoft.LocalDataStore.StorageRouter.IsLocalMode;
	}

	private async void cmdImportProject_Click(object sender, ClickEventArgs e)
	{
		cmdImportProject.Enabled = false;
		await ImportProject();
		cmdImportProject.Enabled = true;
	}

	/// <summary>导出选中项目为 .lqaudit 归档文件</summary>
	private void ExportProjectFile()
	{
		var selected = SelectedProject;
		if (selected == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要导出的项目");
			return;
		}

		// 默认文件名：编号 名称.lqaudit
		string defaultName = string.IsNullOrWhiteSpace(selected.Number)
			? $"{selected.Name}.lqaudit"
			: $"{selected.Number} {selected.Name}.lqaudit";
		// 清理文件名中的非法字符
		foreach (char c in Path.GetInvalidFileNameChars())
			defaultName = defaultName.Replace(c, '_');

		using (var sfd = new SaveFileDialog
		{
			Filter = "乐其审计项目归档 (*.lqaudit)|*.lqaudit",
			Title = "导出项目文件",
			FileName = defaultName
		})
		{
			if (sfd.ShowDialog() != DialogResult.OK)
				return;

			try
			{
				ProjectArchive.Export(selected, sfd.FileName);
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"项目导出成功！\n\n文件位置: " + sfd.FileName, MessageBoxButtons.OK, "导出完成");
			}
			catch (Exception ex)
			{
				ex.Log();
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"项目导出失败: " + ex.Message, MessageBoxButtons.OK, "导出错误");
			}
		}
	}

	/// <summary>从 .lqaudit 归档文件导入项目</summary>
	private async Task ImportProject()
	{
		using (var ofd = new OpenFileDialog
		{
			Filter = "乐其审计项目归档 (*.lqaudit)|*.lqaudit",
			Title = "导入项目",
			CheckFileExists = true
		})
		{
			if (ofd.ShowDialog() != DialogResult.OK)
				return;

			// 先读取元信息进行预览确认
			ProjectArchiveMetadata metadata;
			try
			{
				metadata = ProjectArchive.ReadMetadata(ofd.FileName);
			}
			catch (Exception ex)
			{
				ex.Log();
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"无法读取项目文件: " + ex.Message, MessageBoxButtons.OK, "导入错误");
				return;
			}

			// 确认对话框
			string summary = $"项目名称: {metadata.ProjectName}\n" +
			                 $"项目编号: {metadata.ProjectNumber}\n" +
			                 $"类别: {metadata.Category}\n" +
			                 $"导出者: {metadata.ExportedBy}\n" +
			                 $"导出时间: {metadata.ExportTime:yyyy-MM-dd HH:mm}\n" +
			                 $"Schema版本: v{metadata.SchemaVersion}\n\n" +
			                 $"导入后将生成新的项目 ID，不影响原项目。\n是否继续？";
			if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
				summary, MessageBoxButtons.YesNo, "确认导入项目") != DialogResult.Yes)
				return;

			// 执行导入
			try
			{
				var newProject = ProjectArchive.Import(ofd.FileName);
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					$"项目导入成功！\n\n新项目名称: {newProject.Name}\n新项目 ID: {newProject.Id}",
					MessageBoxButtons.OK, "导入完成");

				// 刷新项目列表
				await PopulateProjects();
			}
			catch (Exception ex)
			{
				ex.Log();
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None,
					"项目导入失败: " + ex.Message, MessageBoxButtons.OK, "导入错误");
			}
		}
	}

	private void cmdModifyProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (State == ViewState.Project)
		{
			if (SelectedProject == null)
			{
				e.Enabled = false;
			}
			else if (IsManager())
			{
				e.Enabled = true;
			}
			else
			{
				e.Enabled = false;
			}
		}
	}

	private async void cmdModifyProject_Click(object sender, ClickEventArgs e)
	{
		cmdModifyProject.Enabled = false;
		await ModifyProject();
		cmdModifyProject.Enabled = true;
	}

	private void cmdModifyTemplate_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SelectedProject == null)
		{
			e.Enabled = false;
		}
		else if (IsEditor())
		{
			e.Enabled = true;
		}
		else
		{
			e.Enabled = false;
		}
	}

	private async void cmdModifyTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdModifyTemplate.Enabled = false;
		await ModifyTemplate();
		cmdModifyTemplate.Enabled = true;
	}

	private void cmdDeleteProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (State == ViewState.Project)
		{
			if (SelectedProject == null)
			{
				e.Enabled = false;
			}
			else if (IsManager())
			{
				e.Enabled = true;
			}
			else
			{
				e.Enabled = false;
			}
		}
	}

	private async void cmdDeleteProject_Click(object sender, ClickEventArgs e)
	{
		cmdDeleteProject.Enabled = false;
		await DeleteProject();
		cmdDeleteProject.Enabled = true;
	}

	private void cmdDeleteTemplate_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SelectedProject == null)
		{
			e.Enabled = false;
		}
		else if (IsEditor())
		{
			e.Enabled = true;
		}
		else
		{
			e.Enabled = false;
		}
	}

	private async void cmdDeleteTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdDeleteTemplate.Enabled = false;
		await DeleteTemplate();
		cmdDeleteTemplate.Enabled = true;
	}

	private void cmdDuplicateProject_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SoftwareLicenseManager.IsDuplicateProjectOutOfLicenseLimit())
		{
			e.Visible = false;
			return;
		}
		e.Visible = true;
		if (State == ViewState.Project)
		{
			if (SelectedProject == null)
			{
				e.Enabled = false;
			}
			else if (IsAssistant())
			{
				e.Enabled = false;
			}
			else
			{
				e.Enabled = true;
			}
		}
	}

	private async void cmdDuplicateProject_Click(object sender, ClickEventArgs e)
	{
		cmdDuplicateProject.Enabled = false;
		await DuplicateProject();
		cmdDuplicateProject.Enabled = true;
	}

	private void cmdDuplicateTemplate_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SelectedProject == null)
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private async void cmdDuplicateTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdDuplicateTemplate.Enabled = false;
		await DuplicateTemplate();
		cmdDuplicateTemplate.Enabled = true;
	}

	private void cmdSaveAsTemplate_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			e.Visible = false;
			return;
		}
		e.Visible = true;
		if (State == ViewState.Project)
		{
			if (SelectedProject == null)
			{
				e.Enabled = false;
			}
			else if (IsAssistant())
			{
				e.Enabled = false;
			}
			else
			{
				e.Enabled = true;
			}
		}
	}

	private async void cmdSaveAsTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdSaveAsTemplate.Enabled = false;
		await SaveAsTemplate();
		cmdSaveAsTemplate.Enabled = true;
	}

	private async void cmdRefreshProject_Click(object sender, ClickEventArgs e)
	{
		cmdRefreshProject.Enabled = false;
		await PopulateProjects();
		cmdRefreshProject.Enabled = true;
	}

	private async void cmdRefreshTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdRefreshTemplate.Enabled = false;
		await PopulateTemplates();
		cmdRefreshTemplate.Enabled = true;
	}

	private async void cmdToTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdToTemplate.Enabled = false;
		await PopulateTemplates();
		cmdToTemplate.Enabled = true;
	}

	private async void cmdToProject_Click(object sender, ClickEventArgs e)
	{
		cmdToProject.Enabled = false;
		await PopulateProjects();
		cmdToProject.Enabled = true;
	}

	private async void cmdMembership_Click(object sender, ClickEventArgs e)
	{
		cmdMembership.Enabled = false;
		Program.ManageUsers();
		await PopulateProjects();
		cmdMembership.Enabled = true;
	}

	private void CmdSortByCreateTime_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSortByCreateTime.Text = "创建时间";
		cmdSortByCreateTime.Checked = Style.SortKind == SortKind.CreateTime;
	}

	private async void CmdSortByCreateTime_Click(object sender, ClickEventArgs e)
	{
		cmdSortByCreateTime.Enabled = false;
		Style.SortKind = SortKind.CreateTime;
		if (State != 0)
		{
			await PopulateTemplates();
		}
		else
		{
			await PopulateProjects();
		}
		cmdSortByCreateTime.Enabled = true;
	}

	private void CmdSortByOpenTime_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSortByOpenTime.Text = "打开时间";
		cmdSortByOpenTime.Checked = Style.SortKind == SortKind.OpenTime;
	}

	private async void CmdSortByOpenTime_Click(object sender, ClickEventArgs e)
	{
		cmdSortByOpenTime.Enabled = false;
		Style.SortKind = SortKind.OpenTime;
		if (State != 0)
		{
			await PopulateTemplates();
		}
		else
		{
			await PopulateProjects();
		}
		cmdSortByOpenTime.Enabled = true;
	}

	private void CmdSortByNumber_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSortByNumber.Text = ((State == ViewState.Project) ? (StringConstBase.Current.Project + "编号") : "模板编号");
		cmdSortByNumber.Checked = Style.SortKind == SortKind.Number;
	}

	private async void CmdSortByNumber_Click(object sender, ClickEventArgs e)
	{
		cmdSortByNumber.Enabled = false;
		Style.SortKind = SortKind.Number;
		if (State != 0)
		{
			await PopulateTemplates();
		}
		else
		{
			await PopulateProjects();
		}
		cmdSortByNumber.Enabled = true;
	}

	private void CmdSortByName_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSortByName.Text = ((State == ViewState.Project) ? (StringConstBase.Current.Project + "名称") : "模板名称");
		cmdSortByName.Checked = Style.SortKind == SortKind.Name;
	}

	private async void CmdSortByName_Click(object sender, ClickEventArgs e)
	{
		cmdSortByName.Enabled = false;
		Style.SortKind = SortKind.Name;
		if (State != 0)
		{
			await PopulateTemplates();
		}
		else
		{
			await PopulateProjects();
		}
		cmdSortByName.Enabled = true;
	}

	private void CmdSortByType_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSortByType.Text = ((State == ViewState.Project) ? (StringConstBase.Current.Project + "类别") : "模板类别");
		cmdSortByType.Checked = Style.SortKind == SortKind.Category;
	}

	private async void CmdSortByType_Click(object sender, ClickEventArgs e)
	{
		cmdSortByType.Enabled = false;
		Style.SortKind = SortKind.Category;
		if (State != 0)
		{
			await PopulateTemplates();
		}
		else
		{
			await PopulateProjects();
		}
		cmdSortByType.Enabled = true;
	}

	private void CmdUseTemplate_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (SelectedProject == null)
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private async void CmdUseTemplate_Click(object sender, ClickEventArgs e)
	{
		cmdUseTemplate.Enabled = false;
		await CreateProject(SelectedProject);
		cmdUseTemplate.Enabled = true;
	}

	private async void cmdDisplayStyle_Click(object sender, ClickEventArgs e)
	{
		cmdDisplayStyle.Enabled = false;
		switch (Style.ViewMode)
		{
		case ListTileViewMode.List:
			SwitchTo(ListTileViewMode.Tile);
			if (State == ViewState.Project)
			{
				await PopulateProjects();
			}
			else if (State == ViewState.Template)
			{
				await PopulateTemplates();
			}
			break;
		case ListTileViewMode.Tile:
			SwitchTo(ListTileViewMode.List);
			if (State == ViewState.Project)
			{
				await PopulateProjects();
			}
			else if (State == ViewState.Template)
			{
				await PopulateTemplates();
			}
			break;
		}
		cmdDisplayStyle.Enabled = true;
	}

	private async void flxProjects_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		flxProjects.Enabled = false;
		HitTestInfo hitTestInfo = flxProjects.HitTest();
		if (flxProjects.Rows.Fixed <= hitTestInfo.Row && hitTestInfo.Row < flxProjects.Rows.Count)
		{
			await OpenProject();
		}
		flxProjects.Enabled = true;
	}

	private void FlxProjects_AfterRowColChange(object sender, RangeEventArgs e)
	{
		int topRow = e.NewRange.TopRow;
		if (topRow >= flxProjects.Rows.Fixed && e.NewRange.BottomRow - e.NewRange.TopRow == 0 && topRow != e.OldRange.TopRow && flxProjects.Rows[topRow].UserData is Leqisoft.DTO.Project project)
		{
			int index = flxProjects.Cols["Name"].Index;
			Rectangle cellRect = flxProjects.GetCellRect(topRow, index);
			XElement xElement = new XElement("div", from l in project.Note.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None)
				select new XElement("p", l));
			_ttb.SetText((State == ViewState.Project) ? (StringConstBase.Current.Project + "备注") : "模板备注", xElement.ToString());
			if (!string.IsNullOrWhiteSpace(project.Note))
			{
				_ttb.Show(flxProjects, new Point(cellRect.Right, cellRect.Bottom));
			}
			else
			{
				_ttb.Hide();
			}
		}
	}

	private void flxProjects_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (flxProjects.IsCellFixed(e.Row, e.Col))
		{
			return;
		}
		try
		{
			if (State == ViewState.Project)
			{
				if (e.Col == flxProjects.Cols["Leaders"].Index || e.Col == flxProjects.Cols["Assistants"].Index || e.Col == flxProjects.Cols["Checkers"].Index)
				{
					IEnumerable<Leqisoft.DTO.User> source = (IEnumerable<Leqisoft.DTO.User>)flxProjects[e.Row, e.Col];
					e.Text = (source.Any() ? string.Join(",", source.Select((Leqisoft.DTO.User u) => u.Name)) : "(空)");
				}
			}
			else
			{
				if (State != ViewState.Template)
				{
					return;
				}
				if (e.Col == flxProjects.Cols["Editor"].Index)
				{
					IEnumerable<Leqisoft.DTO.User> enumerable = (IEnumerable<Leqisoft.DTO.User>)flxProjects[e.Row, e.Col];
					if (enumerable == null)
					{
						e.Text = string.Empty;
						return;
					}
					e.Text = (enumerable.Any() ? string.Join(",", enumerable.Select((Leqisoft.DTO.User u) => u.Name)) : "(空)");
				}
				else
				{
					if (e.Col != flxProjects.Cols["User"].Index)
					{
						return;
					}
					Leqisoft.DTO.Project project = flxProjects.Rows[e.Row].UserData as Leqisoft.DTO.Project;
					if (project.TeamVisible)
					{
						e.Text = "(所有同事)";
						return;
					}
					IEnumerable<Leqisoft.DTO.User> source2 = (IEnumerable<Leqisoft.DTO.User>)flxProjects[e.Row, e.Col];
					e.Text = (source2.Any() ? string.Join(",", source2.Select((Leqisoft.DTO.User u) => u.Name)) : "(空)");
				}
			}
		}
		catch
		{
		}
	}

	private async void dlgProjectManagement_Shown(object sender, EventArgs e)
	{
		await PopulateProjects();
		if (!(Program.MainForm.CurrentEdition is AppEditionGeneral) && _projects != null && _projects.Count == 0)
		{
			await PopulateTemplates();
		}
	}

	private async void dlgProjectManagement_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing && UserClosing)
		{
			base.DialogResult = DialogResult.Cancel;
			if (Application.OpenForms.Count == 0)
			{
				await Program.Logout();
			}
		}
		Style.Save(ConfigManager.PROJECTMANAGEMENT_VIEWCONFIG);
	}

	private void ToProjectView()
	{
		State = ViewState.Project;
		tbrProject.Show();
		tbrProject.Location = Point.Empty;
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Leqisoft.UI.Controls.Theme.ConvertSize(Leqisoft.UI.Platform.Properties.Resources.Projects, 16, 16));
		Text = StringConstBase.Current.Project + "管理";
		tbrTemplate.Hide();
		flxProjects.BeginUpdate();
		flxProjects.Rows.Count = 0;
		flxProjects.Cols.Count = 0;
		C1.Win.C1FlexGrid.Column column = flxProjects.Cols.Add();
		column.Name = "Number";
		flxProjects.Cols.Add().Name = "Name";
		flxProjects.Cols.Add().Name = "Category";
		flxProjects.Cols.Add().Name = "Auditee";
		flxProjects.Cols.Add().Name = "Creator";
		flxProjects.Cols.Add().Name = "Leaders";
		flxProjects.Cols.Add().Name = "Assistants";
		flxProjects.Cols.Add().Name = "Checkers";
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			flxProjects.Cols["Checkers"].Visible = false;
		}
		flxProjects.Cols.Add().Name = "Note";
		C1.Win.C1FlexGrid.Row row = flxProjects.Rows.Add();
		flxProjects.Rows.Fixed = 1;
		row["Number"] = StringConstBase.Current.Project + "编号";
		flxProjects.Cols["Number"].TextAlign = TextAlignEnum.LeftCenter;
		row["Name"] = StringConstBase.Current.Project + "名称";
		row["Category"] = StringConstBase.Current.Project + "类别";
		row["Auditee"] = StringConstBase.Current.Auditee;
		row["Creator"] = "创建者";
		row["Leaders"] = StringConstBase.Current.Manager;
		row["Assistants"] = StringConstBase.Current.Assistant;
		row["Checkers"] = "复核人";
		row["Note"] = "备注";
		flxProjects.EndUpdate();
	}

	private void ToTemplateView()
	{
		State = ViewState.Template;
		tbrProject.Hide();
		tbrTemplate.Show();
		tbrTemplate.Location = Point.Empty;
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Leqisoft.UI.Platform.Properties.Resources.Templates);
		Text = "模板管理";
		flxProjects.BeginUpdate();
		flxProjects.Rows.Count = 0;
		flxProjects.Cols.Count = 0;
		flxProjects.Cols.Add().Name = "Number";
		flxProjects.Cols.Add().Name = "Name";
		flxProjects.Cols.Add().Name = "Category";
		flxProjects.Cols.Add().Name = "Creator";
		flxProjects.Cols.Add().Name = "Editor";
		flxProjects.Cols.Add().Name = "User";
		flxProjects.Cols.Add().Name = "Note";
		C1.Win.C1FlexGrid.Row row = flxProjects.Rows.Add();
		flxProjects.Rows.Fixed = 1;
		row["Number"] = "模板编号";
		row["Name"] = "模板名称";
		row["Category"] = "模板类别";
		row["Creator"] = "创建者";
		row["Editor"] = "可编辑的用户";
		row["User"] = "可使用的用户";
		row["Note"] = "备注";
		flxProjects.EndUpdate();
	}

	public IEnumerable<Leqisoft.DTO.Project> SortProjectImpl(IEnumerable<Leqisoft.DTO.Project> projects, SortKind kind)
	{
		switch (kind)
		{
		case SortKind.CreateTime:
			return projects.OrderBy((Leqisoft.DTO.Project i) => i.CreateTime);
		case SortKind.OpenTime:
		{
			Dictionary<Leqisoft.DTO.Project, DateTime> source = projects.ToDictionary((Leqisoft.DTO.Project p) => p, (Leqisoft.DTO.Project p) => ProjectInfoManager.GetInstance().GetProject(p.Id.ToString())?.OpenTime ?? DateTime.MinValue);
			return from i in source
				orderby i.Value descending
				select i.Key;
		}
		case SortKind.Number:
			return projects.OrderBy((Leqisoft.DTO.Project p) => p.Number);
		case SortKind.Name:
			return projects.OrderBy((Leqisoft.DTO.Project p) => p.Name);
		case SortKind.Category:
			return projects.OrderBy((Leqisoft.DTO.Project p) => p.Category);
		default:
			return projects.OrderBy((Leqisoft.DTO.Project p) => p.Number);
		}
	}

	public IEnumerable<Leqisoft.DTO.Project> FilterProjectImpl(IEnumerable<Leqisoft.DTO.Project> projects, string keyword)
	{
		Dictionary<Leqisoft.DTO.Project, int> source = projects.ToDictionary((Leqisoft.DTO.Project p) => p, (Leqisoft.DTO.Project p) => FuzzySearch.Filter(p.Name, keyword) + FuzzySearch.Filter(p.Number, keyword) + FuzzySearch.Filter(p.Category, keyword) + FuzzySearch.Filter(p.Auditee, keyword));
		return from t in source
			where t.Value > 0
			orderby t.Value
			select t.Key;
	}

	private void SwitchTo(ListTileViewMode view)
	{
		Style.ViewMode = view;
		LeqiTheme selectedLeqiTheme = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme;
		ImageStrategy imageStrategy2;
		if (selectedLeqiTheme == null || !selectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			ImageStrategy imageStrategy = new DefaultImageStrategy();
			imageStrategy2 = imageStrategy;
		}
		else
		{
			ImageStrategy imageStrategy = new WhiteImageStrategy();
			imageStrategy2 = imageStrategy;
		}
		ImageStrategy imageStrategy3 = imageStrategy2;
		switch (view)
		{
		case ListTileViewMode.List:
			cmdDisplayStyle.Text = "磁贴模式";
			cmdDisplayStyle.Image = imageStrategy3.ProcessImage(Leqisoft.UI.Platform.Properties.Resources.tileMode);
			pnlPrevious.Visible = false;
			break;
		case ListTileViewMode.Tile:
			cmdDisplayStyle.Text = "列表模式";
			cmdDisplayStyle.Image = imageStrategy3.ProcessImage(Leqisoft.UI.Platform.Properties.Resources.listMode);
			break;
		}
	}

	private async Task PopulateProjects()
	{
		switch (Style.ViewMode)
		{
		case ListTileViewMode.List:
			ToProjectView();
			await PopulateProjectsList();
			flxProjects.BringToFront();
			break;
		case ListTileViewMode.Tile:
			ToProjectView();
			await PopulateProjectsTile();
			_tileViewer.View.BringToFront();
			break;
		}
		if (await OnlyMyself())
		{
			_cmdProjectUserManageFlickerProxy.Start();
		}
		else
		{
			_cmdProjectUserManageFlickerProxy.Stop();
		}
	}

	private async Task<bool> OnlyMyself()
	{
		bool isTeamAdmin = Leqisoft.Model.User.Current.IsTeamAdmin;
		bool flag = isTeamAdmin;
		if (flag)
		{
			flag = (await Leqisoft.LocalDataStore.StorageRouter.GetTeamUsersWithPic()).Count() == 1;
		}
		return flag;
	}

	private async Task PopulateTemplates()
	{
		switch (Style.ViewMode)
		{
		case ListTileViewMode.List:
			ToTemplateView();
			await PopulateTemplatesList();
			flxProjects.BringToFront();
			break;
		case ListTileViewMode.Tile:
			ToTemplateView();
			await PopulateTemplatesTile();
			_tileViewer.View.BringToFront();
			break;
		}
		pnlPrevious.Visible = false;
	}

	private void PopulateProjectsList(List<Leqisoft.DTO.Project> projects)
	{
		flxProjects.BeginUpdate();
		flxProjects.Rows.Count = 1;
		flxProjects.Cols.Fixed = 0;
		flxProjects.Tree.Column = 0;
		Dictionary<Leqisoft.DTO.Project, bool> marked = projects.ToDictionary((Leqisoft.DTO.Project p) => p, (Leqisoft.DTO.Project p) => false);
		for (int i = 0; i < projects.Count; i++)
		{
			Leqisoft.DTO.Project project2 = projects[i];
			AddProject(project2);
		}
		flxProjects.Select(-1, -1);
		flxProjects.Cols["Note"].Width = 1;
		flxProjects.EndUpdate();
		flxProjects.BeginUpdate();
		flxProjects.AutoSizeCols();
		flxProjects.EndUpdate();
		void AddProject(Leqisoft.DTO.Project p)
		{
			if (!marked[p])
			{
				marked[p] = true;
				if (p.ParentId.HasValue)
				{
					Leqisoft.DTO.Project project3 = projects.FirstOrDefault(delegate(Leqisoft.DTO.Project pr)
					{
						Guid id = pr.Id;
						Guid? parentId = p.ParentId;
						return id == parentId;
					});
					if (project3 == null)
					{
						C1.Win.C1FlexGrid.Row row = flxProjects.Rows.Add();
						PopulateProject(p, row);
						row.IsNode = true;
					}
					else
					{
						if (!marked[project3])
						{
							AddProject(project3);
						}
						int index = FindRowByProject(project3);
						C1.Win.C1FlexGrid.Row row2 = flxProjects.Rows[index].Node.AddNode(NodeTypeEnum.LastChild, p.Number, p, null).Row;
						PopulateProject(p, row2);
						row2.IsNode = true;
					}
				}
				else
				{
					C1.Win.C1FlexGrid.Row row3 = flxProjects.Rows.Add();
					PopulateProject(p, row3);
					row3.IsNode = true;
				}
			}
		}
		int FindRowByProject(Leqisoft.DTO.Project p)
		{
			for (int j = flxProjects.Rows.Fixed; j < flxProjects.Rows.Count; j++)
			{
				if (object.Equals(flxProjects.Rows[j].UserData, p))
				{
					return j;
				}
			}
			return -1;
		}
		static void PopulateProject(Leqisoft.DTO.Project project, C1.Win.C1FlexGrid.Row r)
		{
			r["Number"] = project.Number;
			r["Name"] = project.Name;
			r["Category"] = project.Category;
			r["Creator"] = project.Creator.Name;
			r["Leaders"] = project.Users.Where((Leqisoft.DTO.User u) => u.Role == UserRole.Manager);
			r["Assistants"] = project.Users.Where((Leqisoft.DTO.User u) => u.Role == UserRole.Assistant);
			r["Checkers"] = project.Users.Where((Leqisoft.DTO.User u) => u.Role == UserRole.Checker);
			r["Note"] = project.Note;
			r["Auditee"] = project.Auditee;
			r.UserData = project;
		}
	}

	private void PopulateTemplatesList(List<Leqisoft.DTO.Project> templates)
	{
		flxProjects.BeginUpdate();
		flxProjects.Rows.Count = 1;
		foreach (Leqisoft.DTO.Project template in templates)
		{
			string value;
			IEnumerable<Leqisoft.DTO.User> value2;
			if (template.SystemBuild)
			{
				if (Leqisoft.Model.User.Current.IsSystemSupporter)
				{
					value = template.Creator.Name;
					value2 = template.Users.Where((Leqisoft.DTO.User u) => u.Role == UserRole.Editor);
				}
				else
				{
					value = string.Empty;
					value2 = null;
				}
			}
			else
			{
				value = template.Creator.Name;
				value2 = template.Users.Where((Leqisoft.DTO.User u) => u.Role == UserRole.Editor);
			}
			C1.Win.C1FlexGrid.Row row = flxProjects.Rows.Add();
			row["Number"] = template.Number;
			row["Name"] = template.Name;
			row["Category"] = template.Category;
			row["Creator"] = value;
			row["Editor"] = value2;
			row["User"] = template.Users.Where((Leqisoft.DTO.User u) => u.Role == UserRole.User);
			row["Note"] = template.Note;
			row.UserData = template;
		}
		flxProjects.Tree.Column = 0;
		flxProjects.AutoSizeCols(10);
		flxProjects.EndUpdate();
	}

	private void PopulateCategory()
	{
		Dictionary<string, bool> dictionary = Categories.ToDictionary((Tuple<string, C1Command> tup) => tup.Item1, (Tuple<string, C1Command> tup) => tup.Item2.Checked);
		Categories.Clear();
		tbrCategory.CommandLinks.Clear();
		HashSet<string> hashSet = new HashSet<string>(_projects.Select((Leqisoft.DTO.Project p) => (p.Category ?? "").Split('|')).SelectMany((string[] cats) => cats));
		hashSet.Remove("");
		foreach (string item in hashSet)
		{
			C1Command c1Command = new C1Command
			{
				CheckAutoToggle = true,
				Text = item
			};
			Categories.Add(Tuple.Create(item, c1Command));
			if (dictionary.TryGetValue(item, out var value))
			{
				c1Command.Checked = value;
			}
			c1Command.Click += delegate
			{
				PopulateProjectsList(SortProjectImpl(FilterProjects(_projects), Style.SortKind).ToList());
			};
			C1CommandLink value2 = new C1CommandLink(c1Command)
			{
				ButtonLook = ButtonLookFlags.Text
			};
			tbrCategory.CommandLinks.Add(value2);
		}
	}

	private IEnumerable<Leqisoft.DTO.Project> FilterProjects(IEnumerable<Leqisoft.DTO.Project> projects)
	{
		if (Categories.All((Tuple<string, C1Command> tup) => !tup.Item2.Checked))
		{
			return projects;
		}
		return projects.Where(delegate(Leqisoft.DTO.Project p)
		{
			HashSet<string> cats = new HashSet<string>(p.Category.Split('|'));
			cats.Remove("");
			return Categories.Any((Tuple<string, C1Command> tup) => cats.Contains(tup.Item1) && tup.Item2.Checked);
		});
	}

	private async Task PopulateProjectsList()
	{
		ProgressForm<IEnumerable<Leqisoft.DTO.Project>> progressForm = new ProgressForm<IEnumerable<Leqisoft.DTO.Project>>(async delegate(IProgress<ProgressInfo> iProgress)
		{
			iProgress.Report(new ProgressInfo
			{
				MainCaption = "正在获取" + StringConstBase.Current.Project + "信息，请稍候...",
				MainProgress = 100
			});
			return await Leqisoft.LocalDataStore.StorageRouter.GetProjects();
		});
		progressForm.ShowDialog();
		try
		{
			_projects = (await progressForm.Task).ToList();
			PopulateCategory();
			PopulateProjectsList(SortProjectImpl(FilterProjects(_projects), Style.SortKind).ToList());
		}
		catch (NormalException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (ServerException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
		}
		catch (HttpRequestException ex3)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
		}
		catch (TimeoutException ex4)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
	}

	private async Task PopulateProjectsTile()
	{
		ProgressForm<IEnumerable<Leqisoft.DTO.Project>> progressForm = new ProgressForm<IEnumerable<Leqisoft.DTO.Project>>(delegate(IProgress<ProgressInfo> iProgress)
		{
			iProgress.Report(new ProgressInfo
			{
				MainCaption = "正在获取" + StringConstBase.Current.Project + "信息，请稍候...",
				MainProgress = 100
			});
			return Leqisoft.LocalDataStore.StorageRouter.GetProjects();
		});
		progressForm.ShowDialog();
		try
		{
			_projects = (await progressForm.Task).ToList();
			PopulateCategory();
			_tileViewer.Projects = _projects;
			_tileViewer.Populate(isTemplate: false);
		}
		catch (NormalException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (ServerException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
		}
		catch (HttpRequestException ex3)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
		}
		catch (TimeoutException ex4)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
	}

	private async Task PopulateTemplatesList()
	{
		ProgressForm<IEnumerable<Leqisoft.DTO.Project>> progressForm = new ProgressForm<IEnumerable<Leqisoft.DTO.Project>>(delegate(IProgress<ProgressInfo> iProgress)
		{
			iProgress.Report(new ProgressInfo
			{
				MainCaption = "正在获取模板信息，请稍候...",
				MainProgress = 100
			});
			return Leqisoft.LocalDataStore.StorageRouter.GetProjects();
		});
		progressForm.ShowDialog();
		try
		{
			_templates = (await progressForm.Task).ToList();
			List<Leqisoft.DTO.Project> templates = SortProjectImpl(_templates, Style.SortKind).ToList();
			PopulateTemplatesList(templates);
		}
		catch (NormalException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (ServerException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
		}
		catch (HttpRequestException ex3)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
		}
		catch (TimeoutException ex4)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
	}

	private async Task PopulateTemplatesTile()
	{
		ProgressForm<IEnumerable<Leqisoft.DTO.Project>> progressForm = new ProgressForm<IEnumerable<Leqisoft.DTO.Project>>(delegate(IProgress<ProgressInfo> iProgress)
		{
			iProgress.Report(new ProgressInfo
			{
				MainCaption = "正在获取模板信息，请稍候...",
				MainProgress = 100
			});
			return Leqisoft.LocalDataStore.StorageRouter.GetProjects();
		});
		progressForm.ShowDialog();
		try
		{
			_templates = (await progressForm.Task).ToList();
			_tileViewer.Projects = _templates;
			_tileViewer.Populate(isTemplate: true);
			_tileViewer.View.BringToFront();
		}
		catch (NormalException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (ServerException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
		}
		catch (HttpRequestException ex3)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
		}
		catch (TimeoutException ex4)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
		}
	}

	private async Task CreateProject(Leqisoft.DTO.Project usedTemplate)
	{
		if (Leqisoft.Model.User.Current.TeamId == Guid.Empty)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您还未加入组织，请点击“同事管理”，创建一个组织，或者让同事邀请您进入现有的组织，开始体验协同办公！");
			return;
		}
		Leqisoft.DTO.Project project = Leqisoft.Model.User.Current.GetNewProjectCandidate();
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			if (usedTemplate == null)
			{
				FormSelectTemplate formSelectTemplate = new FormSelectTemplate();
				if (formSelectTemplate.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				project.TemplateId = formSelectTemplate.ResultTemplate?.Id;
				project.Name = formSelectTemplate.ResultTemplate?.Name ?? GetNextProjectName();
			}
			else
			{
				project.TemplateId = usedTemplate.Id;
			}
			FormProjectMembers formProjectMembers = new FormProjectMembers();
			formProjectMembers.Project = project;
			if (formProjectMembers.ShowDialog() != DialogResult.OK)
			{
				return;
			}
		}
		else
		{
			project.TemplateId = usedTemplate?.Id;
			dlgProjectEditor dlgProjectEditor2 = new dlgProjectEditor();
			dlgProjectEditor2.Project = project;
			if (!dlgProjectEditor2.ShowCreate())
			{
				return;
			}
		}
		try
		{
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
			{
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在创建" + StringConstBase.Current.Project + "...",
					MainProgress = 100
				});
				await Leqisoft.LocalDataStore.StorageRouter.CreateProject(project);
				return Task.FromResult<object>(null);
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			ProjectInfoManager.GetInstance().UpdateOpenTime(project.Id.ToString(), DateTime.Now);
			await PopulateProjects();
			FindAndSelectRow(project);
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private string GetNextProjectName()
	{
		HashSet<string> hashSet = new HashSet<string>(_projects.Select((Leqisoft.DTO.Project p) => p.Name));
		int i;
		for (i = 1; hashSet.Contains($"新{StringConstBase.Current.Project} {i}"); i++)
		{
		}
		return $"新{StringConstBase.Current.Project} {i}";
	}

	private async Task DuplicateProject()
	{
		Guid id = SelectedProject.Id;
		Leqisoft.DTO.Project newProject = SelectedProject.Clone();
		newProject.Id = Guid.NewGuid();
		newProject.Number += " - 副本";
		newProject.Name += " - 副本";
		newProject.Users = new List<Leqisoft.DTO.User>
		{
			new Leqisoft.DTO.User
			{
				Id = Leqisoft.Model.User.Current.Id,
				Name = Leqisoft.Model.User.Current.Name,
				UserName = Leqisoft.Model.User.Current.UserName,
				Role = UserRole.Manager
			}
		};
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			FormProjectMembers formProjectMembers = new FormProjectMembers();
			formProjectMembers.Project = newProject;
			if (formProjectMembers.ShowDialog() != DialogResult.OK)
			{
				return;
			}
		}
		else
		{
			dlgProjectEditor dlgProjectEditor2 = new dlgProjectEditor();
			dlgProjectEditor2.Project = newProject;
			if (!dlgProjectEditor2.ShowDuplicate())
			{
				return;
			}
		}
		try
		{
			JObject jObj = new JObject();
			jObj["OldProject"] = id;
			jObj["NewProject"] = JToken.FromObject(newProject);
			jObj["ClearPermissions"] = false;
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> prog)
			{
				prog.Report(new ProgressInfo
				{
					MainCaption = "正在执行复制，请稍候",
					MainProgress = 100
				});
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
					await WebApiClient.DuplicateProject(jObj);
				return Task.FromResult<object>(null);
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			ProjectInfoManager.GetInstance().UpdateOpenTime(newProject.Id.ToString(), DateTime.Now);
			await PopulateProjects();
			FindAndSelectRow(newProject);
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async Task CreateTemplate()
	{
		if (Leqisoft.Model.User.Current.TeamId == Guid.Empty)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您还未加入组织，请点击“同事管理”，创建一个组织，或者让同事邀请您进入现有的组织，开始体验协同办公！");
			return;
		}
		Leqisoft.DTO.Project template = Leqisoft.Model.User.Current.GetNewTemplateCandidate();
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = template;
		if (!dlgTemplateEditor2.ShowCreate())
		{
			return;
		}
		try
		{
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
			{
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在创建模板...",
					MainProgress = 100
				});
				await Leqisoft.LocalDataStore.StorageRouter.CreateProject(template);
				return Task.FromResult<object>(null);
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			ProjectInfoManager.GetInstance().UpdateOpenTime(template.Id.ToString(), DateTime.Now);
			await PopulateTemplates();
			FindAndSelectRow(template);
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async Task DuplicateTemplate()
	{
		Guid id = SelectedProject.Id;
		Leqisoft.DTO.Project newTemplate = SelectedProject.Clone();
		newTemplate.Id = Guid.NewGuid();
		newTemplate.Number += " - 副本";
		newTemplate.Name += " - 副本";
		newTemplate.Users = new List<Leqisoft.DTO.User>
		{
			new Leqisoft.DTO.User
			{
				Id = Leqisoft.Model.User.Current.Id,
				Name = Leqisoft.Model.User.Current.Name,
				UserName = Leqisoft.Model.User.Current.UserName,
				Role = UserRole.Editor
			}
		};
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = newTemplate;
		if (!dlgTemplateEditor2.ShowDuplicate())
		{
			return;
		}
		try
		{
			JObject jObj = new JObject();
			jObj["OldProject"] = id;
			jObj["NewProject"] = JToken.FromObject(newTemplate);
			jObj["ClearPermissions"] = false;
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> prog)
			{
				prog.Report(new ProgressInfo
				{
					MainCaption = "正在执行复制，请稍候",
					MainProgress = 100
				});
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
					await WebApiClient.DuplicateProject(jObj);
				return Task.FromResult<object>(null);
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			ProjectInfoManager.GetInstance().UpdateOpenTime(newTemplate.Id.ToString(), DateTime.Now);
			await PopulateTemplates();
			FindAndSelectRow(newTemplate);
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async Task SaveAsTemplate()
	{
		Guid id = SelectedProject.Id;
		Leqisoft.DTO.Project newTemplate = SelectedProject.Clone();
		newTemplate.Id = Guid.NewGuid();
		newTemplate.Number = string.Empty;
		newTemplate.Name = string.Empty;
		newTemplate.Category = string.Empty;
		newTemplate.Users = new Leqisoft.DTO.User[1]
		{
			new Leqisoft.DTO.User
			{
				Id = Leqisoft.Model.User.Current.Id,
				UserName = Leqisoft.Model.User.Current.UserName,
				Role = UserRole.Editor,
				Name = Leqisoft.Model.User.Current.Name
			}
		};
		newTemplate.Type = ProjectType.Template;
		newTemplate.ChargeType = ChargeType.None;
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = newTemplate;
		if (!dlgTemplateEditor2.ShowFromProject())
		{
			return;
		}
		try
		{
			JObject jObj = new JObject();
			jObj["OldProject"] = id;
			jObj["NewProject"] = JToken.FromObject(newTemplate);
			jObj["ClearPermissions"] = true;
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
			{
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在另存模板...",
					MainProgress = 100
				});
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
					await WebApiClient.DuplicateProject(jObj);
				return Task.FromResult<object>(null);
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			ProjectInfoManager.GetInstance().UpdateOpenTime(newTemplate.Id.ToString(), DateTime.Now);
			await PopulateTemplates();
			FindAndSelectRow(newTemplate);
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async Task OpenProject()
	{
		if (_noAllowReentry)
		{
			return;
		}
		try
		{
			_noAllowReentry = true;
			Leqisoft.DTO.Project sp = SelectedProject;
			if (sp == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择要打开的" + StringConstBase.Current.Project);
			}
			else if (SelectedProject.Type != ProjectType.Template || CanOpenTemplate())
			{
				Task<Leqisoft.Model.Project> task = Program.MainForm.OpenOrSwitchToProject(sp.Id);
				if (await task != null)
				{
					base.DialogResult = DialogResult.OK;
					UserClosing = false;
					Close();
					ProjectInfoManager.GetInstance().UpdateOpenTime(sp.Id.ToString(), DateTime.Now);
				}
			}
		}
		finally
		{
			_noAllowReentry = false;
		}
	}

	private async Task ExportProject()
	{
		if (SelectedProject == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择要导出的" + StringConstBase.Current.Project);
			return;
		}
		try
		{
			Leqisoft.Model.Project project = await Program.MainForm.OpenProjectDb_DownloadIfNotExist(SelectedProject);
			if (project == null)
			{
				return;
			}
			bool flag = project.GetAllTableNodes().Any((TreeTableNode n) => !n.Table.LocalExists);
			if (!flag)
			{
				flag = project.GetAllDocumentNodes().Any((TreeDocumentNode n) => !n.Document.LocalExists);
			}
			if (!flag || Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, StringConstBase.Current.Project + "中有表格或文档还未同步，确定仍要继续导出？", MessageBoxButtons.OKCancel) != DialogResult.Cancel)
			{
				if (_projectExport == null)
				{
					_projectExport = new ProjectExport();
				}
				_projectExport.Project = project;
				if (DialogResult.OK == await _projectExport.SaveDialog())
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出成功");
				}
				ProjectInfoManager.GetInstance().UpdateOpenTime(project.Id.ToString(), DateTime.Now);
				await PopulateProjects();
			}
		}
		catch (IOException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出失败!" + ex.Message);
		}
		catch (Exception ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出失败!失败原因:" + ex2.Message);
		}
	}

	private async Task ModifyProject()
	{
		Leqisoft.DTO.Project clone = SelectedProject.Clone();
		if (Program.MainForm.CurrentEdition is AppEditionGeneral)
		{
			string text = InputForm.Text("重命名" + StringConstBase.Current.Project, "选定的" + StringConstBase.Current.Project + "重命名为：", SelectedProject.Name);
			if (text == null)
			{
				return;
			}
			clone.Name = text;
		}
		else
		{
			dlgProjectEditor dlgProjectEditor2 = new dlgProjectEditor();
			dlgProjectEditor2.Project = clone;
			if (!dlgProjectEditor2.ShowModify())
			{
				return;
			}
		}
		try
		{
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
			{
				iProg.Report(new ProgressInfo
				{
					MainCaption = "正在修改" + StringConstBase.Current.Project + "信息...",
					MainProgress = 100
				});
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				{
					await WebApiClient.UpdateProject(clone);
					await SignalRClient.ChangeProjectMember(clone.Id.ToString());
				}
				return Task.FromResult<object>(null);
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			ProjectInfoManager.GetInstance().UpdateOpenTime(SelectedProject.Id.ToString(), DateTime.Now);
			await PopulateProjects();
		}
		catch (HttpRequestException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
		}
		catch (TimeoutException ex2)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private async Task ModifyTemplate()
	{
		Leqisoft.DTO.Project clone = SelectedProject.Clone();
		dlgTemplateEditor dlgTemplateEditor2 = new dlgTemplateEditor();
		dlgTemplateEditor2.Template = clone;
		if (dlgTemplateEditor2.ShowModify())
		{
			try
			{
				if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				{
					await WebApiClient.UpdateProject(clone);
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改模板成功");
					ProjectInfoManager.GetInstance().UpdateOpenTime(clone.Id.ToString(), DateTime.Now);
					await PopulateTemplates();
					await SignalRClient.ChangeProjectMember(clone.Id.ToString());
				}
			}
			catch (HttpRequestException ex)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
			}
			catch (TimeoutException ex2)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
		}
	}

	private async Task DeleteProject()
	{
		if (_noAllowReentry)
		{
			return;
		}
		try
		{
			_noAllowReentry = true;
			if (Program.MainForm?.CurrentProject?.Id == SelectedProject.Id)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "不能删除当前正在编辑的" + StringConstBase.Current.Project);
			}
			else if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定要删除" + StringConstBase.Current.Project + "吗？", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					await Leqisoft.LocalDataStore.StorageRouter.DeleteProject(SelectedProject.Id);
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "删除" + StringConstBase.Current.Project + "成功");
					await PopulateProjects();
					return;
				}
				catch (HttpRequestException ex)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
					return;
				}
				catch (TimeoutException ex2)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
					return;
				}
			}
		}
		finally
		{
			_noAllowReentry = false;
		}
	}

	private async Task DeleteTemplate()
	{
		if (_noAllowReentry)
		{
			return;
		}
		try
		{
			_noAllowReentry = true;
			if (Program.MainForm?.CurrentProject?.Id == SelectedProject.Id)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "不能删除当前正在编辑的模板");
			}
			else if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定要删除模板吗？", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				try
				{
					if (!Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
					{
						await WebApiClient.DeleteProject(SelectedProject.Id);
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "删除模板成功");
					}
					await PopulateTemplates();
					return;
				}
				catch (HttpRequestException ex)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
					return;
				}
				catch (TimeoutException ex2)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
					return;
				}
			}
		}
		finally
		{
			_noAllowReentry = false;
		}
	}

	private bool IsCreator()
	{
		return SelectedProject.Creator.Id == Leqisoft.Model.User.Current.Id;
	}

	private bool IsEditor()
	{
		Leqisoft.DTO.User user = SelectedProject.Users.FirstOrDefault((Leqisoft.DTO.User u) => u.Id == Leqisoft.Model.User.Current.Id);
		if (user == null)
		{
			return false;
		}
		return user.Role == UserRole.Editor;
	}

	private bool IsAssistant()
	{
		Leqisoft.DTO.User user = SelectedProject.Users.FirstOrDefault((Leqisoft.DTO.User u) => u.Id == Leqisoft.Model.User.Current.Id);
		if (user == null)
		{
			return true;
		}
		return user.Role == UserRole.Assistant;
	}

	private bool IsManager()
	{
		Leqisoft.DTO.User user = SelectedProject.Users.FirstOrDefault((Leqisoft.DTO.User u) => u.Id == Leqisoft.Model.User.Current.Id);
		if (user == null)
		{
			return false;
		}
		return user.Role == UserRole.Manager;
	}

	private void FindAndSelectRow(Leqisoft.DTO.Project p)
	{
		for (int i = flxProjects.Rows.Fixed; i < flxProjects.Rows.Count; i++)
		{
			if ((flxProjects.Rows[i].UserData as Leqisoft.DTO.Project)?.Id == p.Id)
			{
				flxProjects.Row = i;
				break;
			}
		}
	}

	public void AttachTooltip()
	{
		tbrProject.ShowToolTips = false;
		tbrTemplate.ShowToolTips = false;
		tbrProject.CurrentLinkChanged += delegate(object s1, CommandLinkEventArgs e1)
		{
			if (tooltipManager.ShouldDisplay)
			{
				if (e1.CommandLink == null)
				{
					tooltipManager.Hide();
				}
				else
				{
					Rectangle bounds2 = e1.CommandLink.Bounds;
					int num2 = bounds2.Left + (bounds2.Right - bounds2.Left) / 2;
					int top2 = bounds2.Top;
					TipInfo tipInfo2 = tooltipManager.Get(e1.CommandLink);
					tooltipManager.Show(tipInfo2, tbrProject, num2, top2);
				}
			}
		};
		tbrTemplate.CurrentLinkChanged += delegate(object s1, CommandLinkEventArgs e1)
		{
			if (tooltipManager.ShouldDisplay)
			{
				if (e1.CommandLink == null)
				{
					tooltipManager.Hide();
				}
				else
				{
					Rectangle bounds = e1.CommandLink.Bounds;
					int num = bounds.Left + (bounds.Right - bounds.Left) / 2;
					int top = bounds.Top;
					TipInfo tipInfo = tooltipManager.Get(e1.CommandLink);
					tooltipManager.Show(tipInfo, tbrTemplate, num, top);
				}
			}
		};
		tooltipManager.Attach(lnkOpenProject, TipInfo.Parse(TipResource.打开Project按钮));
		tooltipManager.Attach(lnkCreateProject, TipInfo.Parse(TipResource.新建Project按钮));
		tooltipManager.Attach(lnkDeleteProject, TipInfo.Parse(TipResource.删除Project按钮));
		tooltipManager.Attach(lnkModifyProject, TipInfo.Parse(TipResource.修改Project按钮));
		tooltipManager.Attach(lnkDuplicateProject, TipInfo.Parse(TipResource.复制Project按钮));
		tooltipManager.Attach(lnkCreateTemplate, TipInfo.Parse(TipResource.新建模板按钮));
		tooltipManager.Attach(lnkDeleteTemplate, TipInfo.Parse(TipResource.删除模板按钮));
		tooltipManager.Attach(lnkModifyTemplate, TipInfo.Parse(TipResource.修改模板按钮));
		tooltipManager.Attach(lnkSaveAsTemplate, TipInfo.Parse(TipResource.另存模板按钮));
		tooltipManager.Attach(lnkDuplicateTemplate, TipInfo.Parse(TipResource.复制模板按钮));
		tooltipManager.Attach(lnkExportProject, TipInfo.Parse(TipResource.Project导出按钮));
		tooltipManager.Attach(lnkMembership, TipInfo.Parse(TipResource.Project管理窗体_同事管理按钮));
		tooltipManager.Attach(lnkToTemplate, TipInfo.Parse(TipResource.模板管理按钮));
	}

	private void btnPrevious_Click(object sender, EventArgs e)
	{
		_tileViewer.PreviousPage();
	}

	private async void txtSearch_TextChanged(object sender, EventArgs e)
	{
		if (_projects == null)
		{
			return;
		}
		TextBox textBox = sender as TextBox;
		string text = textBox.Text;
		switch (Style.ViewMode)
		{
		case ListTileViewMode.List:
			if (State == ViewState.Project)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					await PopulateProjects();
					break;
				}
				IEnumerable<Leqisoft.DTO.Project> source = FilterProjectImpl(_projects, text);
				ToProjectView();
				PopulateProjectsList(source.ToList());
			}
			else if (State == ViewState.Template)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					await PopulateTemplates();
					break;
				}
				IEnumerable<Leqisoft.DTO.Project> source2 = FilterProjectImpl(_templates, text);
				ToTemplateView();
				PopulateTemplatesList(source2.ToList());
			}
			break;
		case ListTileViewMode.Tile:
			if (State == ViewState.Project)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					await PopulateProjects();
					break;
				}
				ToProjectView();
				IEnumerable<Leqisoft.DTO.Project> projects = FilterProjectImpl(_projects, text);
				_tileViewer.Populate(projects, isTemplate: false);
			}
			else if (State == ViewState.Template)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					await PopulateTemplates();
					break;
				}
				IEnumerable<Leqisoft.DTO.Project> projects2 = FilterProjectImpl(_templates, text);
				ToTemplateView();
				_tileViewer.Populate(projects2, isTemplate: true);
			}
			break;
		}
	}

	private void btnSearch_Click(object sender, EventArgs e)
	{
		txtSearch.Visible = !txtSearch.Visible;
		if (txtSearch.Visible)
		{
			txtSearch.Focus();
		}
	}

	private async void btnSearch_CheckedChanged(object sender, EventArgs e)
	{
		if (!btnSearch.Checked)
		{
			switch (State)
			{
			case ViewState.Project:
				await PopulateProjects();
				break;
			case ViewState.Template:
				await PopulateTemplates();
				break;
			}
		}
	}

	private void cmdAlterPersonInfo_Click(object sender, ClickEventArgs e)
	{
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持修改用户信息");
			return;
		}
		frmAlterInfo frmAlterInfo2 = new frmAlterInfo();
		if (frmAlterInfo2.ShowDialog() == DialogResult.OK)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改成功！");
		}
	}

	private async void cmdTemplateUserManage_Click(object sender, ClickEventArgs e)
	{
		cmdTemplateUserManage.Enabled = false;
		Program.ManageUsers();
		await PopulateTemplates();
		cmdTemplateUserManage.Enabled = true;
	}

	private void cmdTemplateUserAlter_Click(object sender, ClickEventArgs e)
	{
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "本地模式不支持修改用户信息");
			return;
		}
		frmAlterInfo frmAlterInfo2 = new frmAlterInfo();
		if (frmAlterInfo2.ShowDialog() == DialogResult.OK)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改成功！");
		}
	}

	private void cmdChangePassword_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.AlterPwd();
	}

	private void cmdChangePasswordT_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.AlterPwd();
	}

	private void cmdDisplayStyle_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = !(Program.MainForm.CurrentEdition is AppEditionGeneral);
	}

	private void cmdToTemplate_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = !(Program.MainForm.CurrentEdition is AppEditionGeneral);
	}

	private void cmdHelpCenter_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = SoftwareLicenseManager.IsShowHelpDocumentButton();
	}

	private void cmdHelpCenter_Click(object sender, ClickEventArgs e)
	{
		HelpCenterUtil.OpenHelpCenterHomePage();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.commandHolder = new C1.Win.C1Command.C1CommandHolder();
		this.cmdCreateProject = new C1.Win.C1Command.C1Command();
		this.cmdOpenProject = new C1.Win.C1Command.C1Command();
		this.cmdModifyProject = new C1.Win.C1Command.C1Command();
		this.cmdDeleteProject = new C1.Win.C1Command.C1Command();
		this.cmdDuplicateProject = new C1.Win.C1Command.C1Command();
		this.cmdSaveAsTemplate = new C1.Win.C1Command.C1Command();
		this.cmdRefreshProject = new C1.Win.C1Command.C1Command();
		this.cmdToTemplate = new C1.Win.C1Command.C1Command();
		this.cmdUseTemplate = new C1.Win.C1Command.C1Command();
		this.cmdCreateTemplate = new C1.Win.C1Command.C1Command();
		this.cmdOpenTemplate = new C1.Win.C1Command.C1Command();
		this.cmdModifyTemplate = new C1.Win.C1Command.C1Command();
		this.cmdDeleteTemplate = new C1.Win.C1Command.C1Command();
		this.cmdDuplicateTemplate = new C1.Win.C1Command.C1Command();
		this.cmdRefreshTemplate = new C1.Win.C1Command.C1Command();
		this.cmdToProject = new C1.Win.C1Command.C1Command();
		this.cmdMembership = new C1.Win.C1Command.C1Command();
		this.cmdExportProject = new C1.Win.C1Command.C1Command();
		this.cmdExportProjectFile = new C1.Win.C1Command.C1Command();
		this.cmdImportProject = new C1.Win.C1Command.C1Command();
		this.cmdDisplayStyle = new C1.Win.C1Command.C1Command();
		this.cmdAlterPersonInfo = new C1.Win.C1Command.C1Command();
		this.cmdTemplateUserManage = new C1.Win.C1Command.C1Command();
		this.cmdTemplateUserAlter = new C1.Win.C1Command.C1Command();
		this.cmdChangePassword = new C1.Win.C1Command.C1Command();
		this.cmdChangePasswordT = new C1.Win.C1Command.C1Command();
		this.cmdHelpCenter = new C1.Win.C1Command.C1Command();
		this.splMain = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlToolBar = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.commandDock = new C1.Win.C1Command.C1CommandDock();
		this.tbrTemplate = new C1.Win.C1Command.C1ToolBar();
		this.lnkUseTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkCreateTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkOpenTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkModifyTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkDeleteTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkDuplicateTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkDisplayStyle = new C1.Win.C1Command.C1CommandLink();
		this.lnkRefreshTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkToProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkTemplateUserManage = new C1.Win.C1Command.C1CommandLink();
		this.lnkTemplateUserAlter = new C1.Win.C1Command.C1CommandLink();
		this.lnkChangePasswordT = new C1.Win.C1Command.C1CommandLink();
		this.lnkHelpCenterTemplate = new C1.Win.C1Command.C1CommandLink();
		this.tbrProject = new C1.Win.C1Command.C1ToolBar();
		this.lnkCreateProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkOpenProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkModifyProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkDeleteProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkDuplicateProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkExportProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkExportProjectFile = new C1.Win.C1Command.C1CommandLink();
		this.lnkImportProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkSaveAsTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkDisplayStyleProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkRefreshProject = new C1.Win.C1Command.C1CommandLink();
		this.lnkToTemplate = new C1.Win.C1Command.C1CommandLink();
		this.lnkMembership = new C1.Win.C1Command.C1CommandLink();
		this.lnkAlterPersonInfo = new C1.Win.C1Command.C1CommandLink();
		this.lnkChangePassword = new C1.Win.C1Command.C1CommandLink();
		this.lnkHelpCenter = new C1.Win.C1Command.C1CommandLink();
		this.pnlMain = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.ctnMain = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlTools = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.ctnTools = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlPrevious = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnPrevious = new C1.Win.C1Input.C1Button();
		this.pnlbtnSearch = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnSearch = new C1.Win.C1Input.C1CheckBox();
		this.pnlSearchBox = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtSearch = new C1.Win.C1Input.C1TextBox();
		this.pnlCategory = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlProjectDisplay = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.flxProjects = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.tbrCategory = new C1.Win.C1Command.C1ToolBar();
		this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink();
		((System.ComponentModel.ISupportInitialize)this.commandHolder).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.splMain).BeginInit();
		this.splMain.SuspendLayout();
		this.pnlToolBar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.commandDock).BeginInit();
		this.commandDock.SuspendLayout();
		this.pnlMain.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ctnMain).BeginInit();
		this.ctnMain.SuspendLayout();
		this.pnlTools.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ctnTools).BeginInit();
		this.ctnTools.SuspendLayout();
		this.pnlPrevious.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnPrevious).BeginInit();
		this.pnlbtnSearch.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnSearch).BeginInit();
		this.pnlSearchBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtSearch).BeginInit();
		this.pnlCategory.SuspendLayout();
		this.pnlProjectDisplay.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.flxProjects).BeginInit();
		base.SuspendLayout();
		this.commandHolder.Commands.Add(this.cmdCreateProject);
		this.commandHolder.Commands.Add(this.cmdOpenProject);
		this.commandHolder.Commands.Add(this.cmdModifyProject);
		this.commandHolder.Commands.Add(this.cmdDeleteProject);
		this.commandHolder.Commands.Add(this.cmdDuplicateProject);
		this.commandHolder.Commands.Add(this.cmdSaveAsTemplate);
		this.commandHolder.Commands.Add(this.cmdRefreshProject);
		this.commandHolder.Commands.Add(this.cmdToTemplate);
		this.commandHolder.Commands.Add(this.cmdUseTemplate);
		this.commandHolder.Commands.Add(this.cmdCreateTemplate);
		this.commandHolder.Commands.Add(this.cmdOpenTemplate);
		this.commandHolder.Commands.Add(this.cmdModifyTemplate);
		this.commandHolder.Commands.Add(this.cmdDeleteTemplate);
		this.commandHolder.Commands.Add(this.cmdDuplicateTemplate);
		this.commandHolder.Commands.Add(this.cmdRefreshTemplate);
		this.commandHolder.Commands.Add(this.cmdToProject);
		this.commandHolder.Commands.Add(this.cmdMembership);
		this.commandHolder.Commands.Add(this.cmdExportProject);
		this.commandHolder.Commands.Add(this.cmdExportProjectFile);
		this.commandHolder.Commands.Add(this.cmdImportProject);
		this.commandHolder.Commands.Add(this.cmdDisplayStyle);
		this.commandHolder.Commands.Add(this.cmdAlterPersonInfo);
		this.commandHolder.Commands.Add(this.cmdTemplateUserManage);
		this.commandHolder.Commands.Add(this.cmdTemplateUserAlter);
		this.commandHolder.Commands.Add(this.cmdChangePassword);
		this.commandHolder.Commands.Add(this.cmdChangePasswordT);
		this.commandHolder.Commands.Add(this.cmdHelpCenter);
		this.commandHolder.Owner = this;
		this.cmdCreateProject.Image = Leqisoft.UI.Platform.Properties.Resources.CreateProject;
		this.cmdCreateProject.Name = "cmdCreateProject";
		this.cmdCreateProject.ShortcutText = "";
		this.cmdCreateProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdCreateProject_Click);
		this.cmdCreateProject.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdCreateProject_CommandStateQuery);
		this.cmdOpenProject.Image = Leqisoft.UI.Platform.Properties.Resources.OpenProject;
		this.cmdOpenProject.Name = "cmdOpenProject";
		this.cmdOpenProject.ShortcutText = "";
		this.cmdOpenProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdOpenProject_Click);
		this.cmdOpenProject.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(CmdOpenProject_CommandStateQuery);
		this.cmdModifyProject.Image = Leqisoft.UI.Platform.Properties.Resources.ModifyProject;
		this.cmdModifyProject.Name = "cmdModifyProject";
		this.cmdModifyProject.ShortcutText = "";
		this.cmdModifyProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdModifyProject_Click);
		this.cmdModifyProject.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdModifyProject_CommandStateQuery);
		this.cmdDeleteProject.Enabled = false;
		this.cmdDeleteProject.Image = Leqisoft.UI.Platform.Properties.Resources.RemoveProject;
		this.cmdDeleteProject.Name = "cmdDeleteProject";
		this.cmdDeleteProject.ShortcutText = "";
		this.cmdDeleteProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdDeleteProject_Click);
		this.cmdDeleteProject.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdDeleteProject_CommandStateQuery);
		this.cmdDuplicateProject.Image = Leqisoft.UI.Platform.Properties.Resources.DuplicateProject;
		this.cmdDuplicateProject.Name = "cmdDuplicateProject";
		this.cmdDuplicateProject.ShortcutText = "";
		this.cmdDuplicateProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdDuplicateProject_Click);
		this.cmdDuplicateProject.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdDuplicateProject_CommandStateQuery);
		this.cmdSaveAsTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.SaveAsTemplate;
		this.cmdSaveAsTemplate.Name = "cmdSaveAsTemplate";
		this.cmdSaveAsTemplate.ShortcutText = "";
		this.cmdSaveAsTemplate.Text = "另存模板";
		this.cmdSaveAsTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdSaveAsTemplate_Click);
		this.cmdSaveAsTemplate.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdSaveAsTemplate_CommandStateQuery);
		this.cmdRefreshProject.Image = Leqisoft.UI.Platform.Properties.Resources.RefreshProject;
		this.cmdRefreshProject.Name = "cmdRefreshProject";
		this.cmdRefreshProject.ShortcutText = "";
		this.cmdRefreshProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdRefreshProject_Click);
		this.cmdToTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.Templates;
		this.cmdToTemplate.Name = "cmdToTemplate";
		this.cmdToTemplate.ShortcutText = "";
		this.cmdToTemplate.Text = "模板管理";
		this.cmdToTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdToTemplate_Click);
		this.cmdToTemplate.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdToTemplate_CommandStateQuery);
		this.cmdUseTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.UseTemplate;
		this.cmdUseTemplate.Name = "cmdUseTemplate";
		this.cmdUseTemplate.ShortcutText = "";
		this.cmdUseTemplate.Click += new C1.Win.C1Command.ClickEventHandler(CmdUseTemplate_Click);
		this.cmdUseTemplate.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(CmdUseTemplate_CommandStateQuery);
		this.cmdCreateTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.CreateTemplate;
		this.cmdCreateTemplate.Name = "cmdCreateTemplate";
		this.cmdCreateTemplate.ShortcutText = "";
		this.cmdCreateTemplate.Text = "新建模板";
		this.cmdCreateTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdCreateTemplate_Click);
		this.cmdOpenTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.OpenTemplate;
		this.cmdOpenTemplate.Name = "cmdOpenTemplate";
		this.cmdOpenTemplate.ShortcutText = "";
		this.cmdOpenTemplate.Text = "打开模板";
		this.cmdOpenTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdOpenTemplate_Click);
		this.cmdOpenTemplate.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(CmdOpenTemplate_CommandStateQuery);
		this.cmdModifyTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.ModifyTemplate;
		this.cmdModifyTemplate.Name = "cmdModifyTemplate";
		this.cmdModifyTemplate.ShortcutText = "";
		this.cmdModifyTemplate.Text = "修改模板";
		this.cmdModifyTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdModifyTemplate_Click);
		this.cmdModifyTemplate.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdModifyTemplate_CommandStateQuery);
		this.cmdDeleteTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.RemoveTemplate;
		this.cmdDeleteTemplate.Name = "cmdDeleteTemplate";
		this.cmdDeleteTemplate.ShortcutText = "";
		this.cmdDeleteTemplate.Text = "删除模板";
		this.cmdDeleteTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdDeleteTemplate_Click);
		this.cmdDeleteTemplate.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdDeleteTemplate_CommandStateQuery);
		this.cmdDuplicateTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.DuplicateTemplate;
		this.cmdDuplicateTemplate.Name = "cmdDuplicateTemplate";
		this.cmdDuplicateTemplate.ShortcutText = "";
		this.cmdDuplicateTemplate.Text = "复制模板";
		this.cmdDuplicateTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdDuplicateTemplate_Click);
		this.cmdDuplicateTemplate.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdDuplicateTemplate_CommandStateQuery);
		this.cmdRefreshTemplate.Image = Leqisoft.UI.Platform.Properties.Resources.RefreshTemplate;
		this.cmdRefreshTemplate.Name = "cmdRefreshTemplate";
		this.cmdRefreshTemplate.ShortcutText = "";
		this.cmdRefreshTemplate.Text = "刷新模板";
		this.cmdRefreshTemplate.Click += new C1.Win.C1Command.ClickEventHandler(cmdRefreshTemplate_Click);
		this.cmdToProject.Image = Leqisoft.UI.Platform.Properties.Resources.Projects;
		this.cmdToProject.Name = "cmdToProject";
		this.cmdToProject.ShortcutText = "";
		this.cmdToProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdToProject_Click);
		this.cmdMembership.Image = Leqisoft.UI.Platform.Properties.Resources.Users;
		this.cmdMembership.Name = "cmdMembership";
		this.cmdMembership.ShortcutText = "";
		this.cmdMembership.Text = "同事管理";
		this.cmdMembership.Click += new C1.Win.C1Command.ClickEventHandler(cmdMembership_Click);
		this.cmdExportProject.Image = Leqisoft.UI.Platform.Properties.Resources.ProjectExport;
		this.cmdExportProject.Name = "cmdExportProject";
		this.cmdExportProject.ShortcutText = "";
		this.cmdExportProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdExportProject_Click);
		this.cmdExportProject.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdExportProject_CommandStateQuery);
		this.cmdExportProjectFile.Image = Leqisoft.UI.Platform.Properties.Resources.ProjectExport;
		this.cmdExportProjectFile.Name = "cmdExportProjectFile";
		this.cmdExportProjectFile.ShortcutText = "";
		this.cmdExportProjectFile.Text = "导出项目文件";
		this.cmdExportProjectFile.Click += new C1.Win.C1Command.ClickEventHandler(cmdExportProjectFile_Click);
		this.cmdExportProjectFile.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdExportProjectFile_CommandStateQuery);
		this.cmdImportProject.Image = Leqisoft.UI.Platform.Properties.Resources.ProjectExport;
		this.cmdImportProject.Name = "cmdImportProject";
		this.cmdImportProject.ShortcutText = "";
		this.cmdImportProject.Text = "导入项目";
		this.cmdImportProject.Click += new C1.Win.C1Command.ClickEventHandler(cmdImportProject_Click);
		this.cmdImportProject.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdImportProject_CommandStateQuery);
		this.cmdDisplayStyle.Image = Leqisoft.UI.Platform.Properties.Resources.tileMode;
		this.cmdDisplayStyle.Name = "cmdDisplayStyle";
		this.cmdDisplayStyle.ShortcutText = "";
		this.cmdDisplayStyle.Text = "磁贴模式";
		this.cmdDisplayStyle.Click += new C1.Win.C1Command.ClickEventHandler(cmdDisplayStyle_Click);
		this.cmdDisplayStyle.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdDisplayStyle_CommandStateQuery);
		this.cmdAlterPersonInfo.Image = Leqisoft.UI.Platform.Properties.Resources.SwitchUser;
		this.cmdAlterPersonInfo.Name = "cmdAlterPersonInfo";
		this.cmdAlterPersonInfo.ShortcutText = "";
		this.cmdAlterPersonInfo.Text = "用户资料";
		this.cmdAlterPersonInfo.Click += new C1.Win.C1Command.ClickEventHandler(cmdAlterPersonInfo_Click);
		this.cmdTemplateUserManage.Image = Leqisoft.UI.Platform.Properties.Resources.Users;
		this.cmdTemplateUserManage.Name = "cmdTemplateUserManage";
		this.cmdTemplateUserManage.ShortcutText = "";
		this.cmdTemplateUserManage.Text = "同事管理";
		this.cmdTemplateUserManage.Click += new C1.Win.C1Command.ClickEventHandler(cmdTemplateUserManage_Click);
		this.cmdTemplateUserAlter.Image = Leqisoft.UI.Platform.Properties.Resources.SwitchUser;
		this.cmdTemplateUserAlter.Name = "cmdTemplateUserAlter";
		this.cmdTemplateUserAlter.ShortcutText = "";
		this.cmdTemplateUserAlter.Text = "修改信息";
		this.cmdTemplateUserAlter.Click += new C1.Win.C1Command.ClickEventHandler(cmdTemplateUserAlter_Click);
		this.cmdChangePassword.Image = Leqisoft.UI.Platform.Properties.Resources.PwdEdit;
		this.cmdChangePassword.Name = "cmdChangePassword";
		this.cmdChangePassword.ShortcutText = "";
		this.cmdChangePassword.Text = "修改密码";
		this.cmdChangePassword.Click += new C1.Win.C1Command.ClickEventHandler(cmdChangePassword_Click);
		this.cmdChangePasswordT.Image = Leqisoft.UI.Platform.Properties.Resources.PwdEdit;
		this.cmdChangePasswordT.Name = "cmdChangePasswordT";
		this.cmdChangePasswordT.ShortcutText = "";
		this.cmdChangePasswordT.Text = "修改密码";
		this.cmdChangePasswordT.Click += new C1.Win.C1Command.ClickEventHandler(cmdChangePasswordT_Click);
		this.cmdHelpCenter.Image = Leqisoft.UI.Platform.Properties.Resources.HelpCenter;
		this.cmdHelpCenter.Name = "cmdHelpCenter";
		this.cmdHelpCenter.ShortcutText = "";
		this.cmdHelpCenter.Text = "帮助中心";
		this.cmdHelpCenter.Click += new C1.Win.C1Command.ClickEventHandler(cmdHelpCenter_Click);
		this.cmdHelpCenter.CommandStateQuery += new C1.Win.C1Command.CommandStateQueryEventHandler(cmdHelpCenter_CommandStateQuery);
		this.splMain.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.splMain.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.splMain.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splMain.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.splMain.Location = new System.Drawing.Point(0, 0);
		this.splMain.Name = "splMain";
		this.splMain.Panels.Add(this.pnlToolBar);
		this.splMain.Panels.Add(this.pnlMain);
		this.splMain.Size = new System.Drawing.Size(892, 567);
		this.splMain.SplitterWidth = 0;
		this.splMain.TabIndex = 2;
		this.pnlToolBar.Controls.Add(this.commandDock);
		this.pnlToolBar.Height = 66;
		this.pnlToolBar.KeepRelativeSize = false;
		this.pnlToolBar.Location = new System.Drawing.Point(0, 0);
		this.pnlToolBar.Name = "pnlToolBar";
		this.pnlToolBar.Resizable = false;
		this.pnlToolBar.Size = new System.Drawing.Size(892, 66);
		this.pnlToolBar.SizeRatio = 11.62;
		this.pnlToolBar.TabIndex = 0;
		this.commandDock.Controls.Add(this.tbrTemplate);
		this.commandDock.Controls.Add(this.tbrProject);
		this.commandDock.Id = 2;
		this.commandDock.Location = new System.Drawing.Point(0, 0);
		this.commandDock.Name = "commandDock";
		this.commandDock.Size = new System.Drawing.Size(892, 144);
		this.tbrTemplate.AccessibleName = "Tool Bar";
		this.tbrTemplate.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.tbrTemplate.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.tbrTemplate.CommandHolder = this.commandHolder;
		this.tbrTemplate.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[13]
		{
			this.lnkUseTemplate, this.lnkCreateTemplate, this.lnkOpenTemplate, this.lnkModifyTemplate, this.lnkDeleteTemplate, this.lnkDuplicateTemplate, this.lnkDisplayStyle, this.lnkRefreshTemplate, this.lnkToProject, this.lnkTemplateUserManage,
			this.lnkTemplateUserAlter, this.lnkChangePasswordT, this.lnkHelpCenterTemplate
		});
		this.tbrTemplate.Location = new System.Drawing.Point(0, 0);
		this.tbrTemplate.MinButtonSize = 42;
		this.tbrTemplate.Movable = false;
		this.tbrTemplate.Name = "tbrTemplate";
		this.tbrTemplate.Size = new System.Drawing.Size(795, 63);
		this.tbrTemplate.Text = "c1ToolBar1";
		this.tbrTemplate.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.tbrTemplate.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.lnkUseTemplate.Command = this.cmdUseTemplate;
		this.lnkCreateTemplate.Command = this.cmdCreateTemplate;
		this.lnkCreateTemplate.SortOrder = 1;
		this.lnkOpenTemplate.Command = this.cmdOpenTemplate;
		this.lnkOpenTemplate.SortOrder = 2;
		this.lnkModifyTemplate.Command = this.cmdModifyTemplate;
		this.lnkModifyTemplate.SortOrder = 3;
		this.lnkDeleteTemplate.Command = this.cmdDeleteTemplate;
		this.lnkDeleteTemplate.SortOrder = 4;
		this.lnkDuplicateTemplate.Command = this.cmdDuplicateTemplate;
		this.lnkDuplicateTemplate.SortOrder = 5;
		this.lnkDisplayStyle.Command = this.cmdDisplayStyle;
		this.lnkDisplayStyle.SortOrder = 6;
		this.lnkRefreshTemplate.Command = this.cmdRefreshTemplate;
		this.lnkRefreshTemplate.SortOrder = 7;
		this.lnkToProject.Command = this.cmdToProject;
		this.lnkToProject.Delimiter = true;
		this.lnkToProject.SortOrder = 8;
		this.lnkTemplateUserManage.Command = this.cmdTemplateUserManage;
		this.lnkTemplateUserManage.Delimiter = true;
		this.lnkTemplateUserManage.SortOrder = 9;
		this.lnkTemplateUserAlter.Command = this.cmdTemplateUserAlter;
		this.lnkTemplateUserAlter.SortOrder = 10;
		this.lnkTemplateUserAlter.Text = "用户资料";
		this.lnkChangePasswordT.Command = this.cmdChangePasswordT;
		this.lnkChangePasswordT.SortOrder = 11;
		this.lnkHelpCenterTemplate.Command = this.cmdHelpCenter;
		this.lnkHelpCenterTemplate.Delimiter = true;
		this.lnkHelpCenterTemplate.SortOrder = 12;
		this.tbrProject.AccessibleName = "Tool Bar";
		this.tbrProject.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.tbrProject.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.tbrProject.CommandHolder = this.commandHolder;
		this.tbrProject.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[16]
		{
			this.lnkCreateProject, this.lnkOpenProject, this.lnkModifyProject, this.lnkDeleteProject, this.lnkDuplicateProject, this.lnkExportProject, this.lnkExportProjectFile, this.lnkImportProject, this.lnkSaveAsTemplate, this.lnkDisplayStyleProject, this.lnkRefreshProject, this.lnkToTemplate,
			this.lnkMembership, this.lnkAlterPersonInfo, this.lnkChangePassword, this.lnkHelpCenter
		});
		this.tbrProject.Location = new System.Drawing.Point(0, 63);
		this.tbrProject.MinButtonSize = 42;
		this.tbrProject.Movable = false;
		this.tbrProject.Name = "tbrProject";
		this.tbrProject.Size = new System.Drawing.Size(892, 63);
		this.tbrProject.Text = "c1ToolBar1";
		this.tbrProject.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.tbrProject.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.lnkCreateProject.Command = this.cmdCreateProject;
		this.lnkOpenProject.Command = this.cmdOpenProject;
		this.lnkOpenProject.SortOrder = 1;
		this.lnkModifyProject.Command = this.cmdModifyProject;
		this.lnkModifyProject.SortOrder = 2;
		this.lnkDeleteProject.Command = this.cmdDeleteProject;
		this.lnkDeleteProject.SortOrder = 3;
		this.lnkDuplicateProject.Command = this.cmdDuplicateProject;
		this.lnkDuplicateProject.SortOrder = 4;
		this.lnkExportProject.Command = this.cmdExportProject;
		this.lnkExportProject.SortOrder = 5;
		this.lnkExportProjectFile.Command = this.cmdExportProjectFile;
		this.lnkExportProjectFile.SortOrder = 6;
		this.lnkImportProject.Command = this.cmdImportProject;
		this.lnkImportProject.SortOrder = 7;
		this.lnkSaveAsTemplate.Command = this.cmdSaveAsTemplate;
		this.lnkSaveAsTemplate.SortOrder = 8;
		this.lnkDisplayStyleProject.Command = this.cmdDisplayStyle;
		this.lnkDisplayStyleProject.SortOrder = 9;
		this.lnkRefreshProject.Command = this.cmdRefreshProject;
		this.lnkRefreshProject.SortOrder = 10;
		this.lnkToTemplate.Command = this.cmdToTemplate;
		this.lnkToTemplate.Delimiter = true;
		this.lnkToTemplate.SortOrder = 11;
		this.lnkMembership.Command = this.cmdMembership;
		this.lnkMembership.Delimiter = true;
		this.lnkMembership.SortOrder = 12;
		this.lnkAlterPersonInfo.Command = this.cmdAlterPersonInfo;
		this.lnkAlterPersonInfo.SortOrder = 13;
		this.lnkChangePassword.Command = this.cmdChangePassword;
		this.lnkChangePassword.SortOrder = 14;
		this.lnkHelpCenter.Command = this.cmdHelpCenter;
		this.lnkHelpCenter.Delimiter = true;
		this.lnkHelpCenter.SortOrder = 15;
		this.pnlMain.Controls.Add(this.ctnMain);
		this.pnlMain.Height = 500;
		this.pnlMain.Location = new System.Drawing.Point(0, 67);
		this.pnlMain.Name = "pnlMain";
		this.pnlMain.Size = new System.Drawing.Size(892, 500);
		this.pnlMain.SizeRatio = 90.455;
		this.pnlMain.TabIndex = 1;
		this.ctnMain.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnMain.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnMain.Location = new System.Drawing.Point(0, 0);
		this.ctnMain.Name = "ctnMain";
		this.ctnMain.Panels.Add(this.pnlTools);
		this.ctnMain.Panels.Add(this.pnlCategory);
		this.ctnMain.Panels.Add(this.pnlProjectDisplay);
		this.ctnMain.Size = new System.Drawing.Size(892, 500);
		this.ctnMain.SplitterWidth = 0;
		this.ctnMain.TabIndex = 2;
		this.pnlTools.Controls.Add(this.ctnTools);
		this.pnlTools.Height = 25;
		this.pnlTools.KeepRelativeSize = false;
		this.pnlTools.Location = new System.Drawing.Point(0, 0);
		this.pnlTools.MinHeight = 25;
		this.pnlTools.Name = "pnlTools";
		this.pnlTools.Size = new System.Drawing.Size(892, 25);
		this.pnlTools.SizeRatio = 5.0;
		this.pnlTools.TabIndex = 0;
		this.ctnTools.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnTools.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnTools.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnTools.Location = new System.Drawing.Point(0, 0);
		this.ctnTools.Name = "ctnTools";
		this.ctnTools.Panels.Add(this.pnlPrevious);
		this.ctnTools.Panels.Add(this.pnlbtnSearch);
		this.ctnTools.Panels.Add(this.pnlSearchBox);
		this.ctnTools.Size = new System.Drawing.Size(892, 25);
		this.ctnTools.SplitterWidth = 0;
		this.ctnTools.TabIndex = 0;
		this.pnlPrevious.Controls.Add(this.btnPrevious);
		this.pnlPrevious.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlPrevious.Height = 25;
		this.pnlPrevious.KeepRelativeSize = false;
		this.pnlPrevious.Location = new System.Drawing.Point(0, 0);
		this.pnlPrevious.MinWidth = 30;
		this.pnlPrevious.Name = "pnlPrevious";
		this.pnlPrevious.Size = new System.Drawing.Size(30, 25);
		this.pnlPrevious.SizeRatio = 100.0;
		this.pnlPrevious.TabIndex = 0;
		this.pnlPrevious.Width = 30;
		this.btnPrevious.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.btnBack;
		this.btnPrevious.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnPrevious.Location = new System.Drawing.Point(0, 0);
		this.btnPrevious.Name = "btnPrevious";
		this.btnPrevious.Size = new System.Drawing.Size(25, 25);
		this.btnPrevious.TabIndex = 0;
		this.btnPrevious.UseVisualStyleBackColor = true;
		this.btnPrevious.Click += new System.EventHandler(btnPrevious_Click);
		this.pnlbtnSearch.Controls.Add(this.btnSearch);
		this.pnlbtnSearch.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlbtnSearch.Height = 25;
		this.pnlbtnSearch.KeepRelativeSize = false;
		this.pnlbtnSearch.Location = new System.Drawing.Point(30, 0);
		this.pnlbtnSearch.MinWidth = 30;
		this.pnlbtnSearch.Name = "pnlbtnSearch";
		this.pnlbtnSearch.Size = new System.Drawing.Size(30, 25);
		this.pnlbtnSearch.SizeRatio = 100.0;
		this.pnlbtnSearch.TabIndex = 1;
		this.pnlbtnSearch.Width = 30;
		this.btnSearch.Appearance = System.Windows.Forms.Appearance.Button;
		this.btnSearch.BackColor = System.Drawing.SystemColors.Control;
		this.btnSearch.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.btnSearch;
		this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.btnSearch.ForeColor = System.Drawing.SystemColors.ControlText;
		this.btnSearch.Location = new System.Drawing.Point(0, 0);
		this.btnSearch.Name = "btnSearch";
		this.btnSearch.Size = new System.Drawing.Size(26, 24);
		this.btnSearch.TabIndex = 0;
		this.btnSearch.UseVisualStyleBackColor = true;
		this.btnSearch.Value = null;
		this.btnSearch.CheckedChanged += new System.EventHandler(btnSearch_CheckedChanged);
		this.btnSearch.Click += new System.EventHandler(btnSearch_Click);
		this.pnlSearchBox.Controls.Add(this.txtSearch);
		this.pnlSearchBox.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Right;
		this.pnlSearchBox.Location = new System.Drawing.Point(60, 0);
		this.pnlSearchBox.Name = "pnlSearchBox";
		this.pnlSearchBox.Size = new System.Drawing.Size(832, 25);
		this.pnlSearchBox.SizeRatio = 100.0;
		this.pnlSearchBox.TabIndex = 2;
		this.pnlSearchBox.Width = 832;
		this.txtSearch.AutoSize = false;
		this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtSearch.Location = new System.Drawing.Point(0, 0);
		this.txtSearch.Margin = new System.Windows.Forms.Padding(0);
		this.txtSearch.Name = "txtSearch";
		this.txtSearch.Size = new System.Drawing.Size(832, 25);
		this.txtSearch.TabIndex = 0;
		this.txtSearch.Tag = null;
		this.txtSearch.TextDetached = true;
		this.txtSearch.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtSearch.Visible = false;
		this.txtSearch.TextChanged += new System.EventHandler(txtSearch_TextChanged);
		this.pnlCategory.Controls.Add(this.tbrCategory);
		this.pnlCategory.Height = 40;
		this.pnlCategory.KeepRelativeSize = false;
		this.pnlCategory.Location = new System.Drawing.Point(0, 25);
		this.pnlCategory.Name = "pnlCategory";
		this.pnlCategory.Resizable = false;
		this.pnlCategory.Size = new System.Drawing.Size(892, 40);
		this.pnlCategory.SizeRatio = 8.421;
		this.pnlCategory.TabIndex = 2;
		this.pnlProjectDisplay.Controls.Add(this.flxProjects);
		this.pnlProjectDisplay.Height = 434;
		this.pnlProjectDisplay.Location = new System.Drawing.Point(0, 66);
		this.pnlProjectDisplay.Name = "pnlProjectDisplay";
		this.pnlProjectDisplay.Size = new System.Drawing.Size(892, 434);
		this.pnlProjectDisplay.TabIndex = 1;
		this.flxProjects.AllowDragging = C1.Win.C1FlexGrid.AllowDraggingEnum.None;
		this.flxProjects.AllowEditing = false;
		this.flxProjects.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.flxProjects.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.flxProjects.ColumnInfo = "10,1,0,0,0,110,Columns:";
		this.flxProjects.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flxProjects.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.flxProjects.FocusRect = C1.Win.C1FlexGrid.FocusRectEnum.None;
		this.flxProjects.Font = new System.Drawing.Font("Microsoft YaHei", 9f);
		this.flxProjects.Location = new System.Drawing.Point(0, 0);
		this.flxProjects.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.flxProjects.Name = "flxProjects";
		this.flxProjects.Rows.DefaultSize = 22;
		this.flxProjects.SelectionMode = C1.Win.C1FlexGrid.SelectionModeEnum.Row;
		this.flxProjects.Size = new System.Drawing.Size(892, 434);
		this.flxProjects.TabIndex = 4;
		this.tbrCategory.AutoSize = false;
		this.tbrCategory.CommandHolder = this.commandHolder;
		this.tbrCategory.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[1] { this.c1CommandLink1 });
		this.tbrCategory.Dock = System.Windows.Forms.DockStyle.Top;
		this.tbrCategory.Location = new System.Drawing.Point(0, 0);
		this.tbrCategory.Movable = false;
		this.tbrCategory.Name = "tbrCategory";
		this.tbrCategory.Size = new System.Drawing.Size(892, 24);
		this.tbrCategory.Text = "c1ToolBar1";
		this.tbrCategory.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.tbrCategory.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.c1CommandLink1.Text = "New Command";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(892, 567);
		base.Controls.Add(this.splMain);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "dlgProjectManagement";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(dlgProjectManagement_FormClosed);
		base.Shown += new System.EventHandler(dlgProjectManagement_Shown);
		((System.ComponentModel.ISupportInitialize)this.commandHolder).EndInit();
		((System.ComponentModel.ISupportInitialize)this.splMain).EndInit();
		this.splMain.ResumeLayout(false);
		this.pnlToolBar.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.commandDock).EndInit();
		this.commandDock.ResumeLayout(false);
		this.pnlMain.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ctnMain).EndInit();
		this.ctnMain.ResumeLayout(false);
		this.pnlTools.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ctnTools).EndInit();
		this.ctnTools.ResumeLayout(false);
		this.pnlPrevious.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnPrevious).EndInit();
		this.pnlbtnSearch.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnSearch).EndInit();
		this.pnlSearchBox.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txtSearch).EndInit();
		this.pnlCategory.ResumeLayout(false);
		this.pnlProjectDisplay.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.flxProjects).EndInit();
		base.ResumeLayout(false);
	}
}
