using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1Tile;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.PlatformResource;
using Leqisoft.SignalR;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;
using Newtonsoft.Json.Linq;

namespace Leqisoft.UI.Platform;

public class dlgTeamCreator : C1RibbonForm
{
	private C1TileControlEx _tileControl;

	private Template _teamTemplate;

	private Template _titleTemplate;

	private IContainer components;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel pnlHeader;

	private C1Label lblTeamName;

	private C1TextBox txtTeamName;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancel;

	private C1Button btnConfirm;

	public Guid? TeamId { get; private set; }

	public dlgTeamCreator()
	{
		InitializeComponent();
		base.Shown += DlgTeamCreator_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		_tileControl = new C1TileControlEx
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
			TileBorderColor = Color.Transparent,
			GroupSpacing = 5
		};
		_teamTemplate = CreateTeamTemplate1();
		_tileControl.Templates.Add(_teamTemplate);
		_titleTemplate = CreateTitleTemplate();
		_tileControl.Templates.Add(_titleTemplate);
		_tileControl.Groups.Clear();
		_tileControl.Groups.Add(CreateTitleGroup("请选择组织类型"));
		C1.Win.C1Tile.Group group = CreateGroup(string.Empty);
		_tileControl.Groups.Add(group);
		InitializeModule(group);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
	}

	private void DlgTeamCreator_Shown(object sender, EventArgs e)
	{
		base.Icon = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetThemedIcon(Resources.toolCreateTeam);
	}

	private void InitializeModule(C1.Win.C1Tile.Group group)
	{
		foreach (AppEditionBase edition in AppEditions.Editions)
		{
			group.Tiles.Add(CreateTile(edition));
		}
	}

	private C1.Win.C1Tile.Group CreateTitleGroup(string title)
	{
		C1.Win.C1Tile.Group group = new C1.Win.C1Tile.Group();
		Tile item = new Tile
		{
			Text = title,
			HorizontalSize = 5,
			VerticalSize = 2,
			Template = _titleTemplate,
			BackColor = Color.Transparent
		};
		group.Tiles.Add(item);
		return group;
	}

	private Template CreateTitleTemplate()
	{
		Template template = new Template();
		template.Description = "Subgroup";
		PanelElement panelElement = new PanelElement();
		panelElement.AlignmentOfContents = ContentAlignment.BottomLeft;
		PanelElement panelElement2 = new PanelElement();
		panelElement2.BackColor = Color.FromArgb(0, 73, 92);
		panelElement2.Dock = DockStyle.Bottom;
		panelElement2.FixedHeight = 1;
		C1.Win.C1Tile.TextElement textElement = new C1.Win.C1Tile.TextElement();
		textElement.ForeColor = Color.Black;
		textElement.ForeColorSelector = ForeColorSelector.Unbound;
		textElement.Font = new Font("微软雅黑", 9f, FontStyle.Regular);
		textElement.Margin = new Padding(0, 0, 0, 6);
		textElement.SingleLine = true;
		panelElement.Children.Add(panelElement2);
		panelElement.Children.Add(textElement);
		panelElement.Dock = DockStyle.Fill;
		panelElement.Padding = new Padding(8, 0, 8, 8);
		template.Elements.Add(panelElement);
		template.Enabled = false;
		template.Name = "subgroupTemplate";
		return template;
	}

	private void SetTheme()
	{
		_tileControl.TileBorderColor = Color.Transparent;
		_tileControl.CustomBorderColor = Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeContext.DarkColor;
		c1SplitContainer1.SplitterWidth = 0;
	}

	private C1.Win.C1Tile.Group CreateGroup(string groupName)
	{
		return new C1.Win.C1Tile.Group
		{
			Text = groupName
		};
	}

	private Tile CreateTile(AppEditionBase edition)
	{
		return new Tile
		{
			HorizontalSize = 9,
			VerticalSize = 6,
			Tag = edition,
			Image = edition.Icon,
			Text1 = edition.Name,
			Text2 = "● " + edition.Tooltip,
			ForeColor1 = Color.Red,
			Template = _teamTemplate
		};
	}

	private Template CreateTeamTemplate2()
	{
		Template template = new Template();
		PanelElement panelElement = new PanelElement();
		panelElement.Dock = DockStyle.Left;
		panelElement.FixedWidth = 60;
		panelElement.AlignmentOfContents = ContentAlignment.MiddleCenter;
		C1.Win.C1Tile.ImageElement item = new C1.Win.C1Tile.ImageElement();
		panelElement.Children.Add(item);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.Dock = DockStyle.Fill;
		panelElement2.AlignmentOfContents = ContentAlignment.MiddleLeft;
		C1.Win.C1Tile.TextElement textElement = new C1.Win.C1Tile.TextElement();
		textElement.SingleLine = false;
		panelElement2.Children.Add(textElement);
		template.Elements.Add(panelElement);
		template.Elements.Add(panelElement2);
		return template;
	}

	private Template CreateTeamTemplate1()
	{
		Template template = new Template();
		PanelElement panelElement = new PanelElement();
		panelElement.Dock = DockStyle.Bottom;
		panelElement.AlignmentOfContents = ContentAlignment.TopLeft;
		panelElement.FixedHeight = 100;
		C1.Win.C1Tile.TextElement textElement = new C1.Win.C1Tile.TextElement();
		textElement.SingleLine = false;
		textElement.Margin = new Padding(10, 0, 10, 0);
		textElement.TextSelector = TextSelector.Text2;
		panelElement.Children.Add(textElement);
		template.Elements.Add(panelElement);
		PanelElement panelElement2 = new PanelElement();
		panelElement2.Dock = DockStyle.Left;
		panelElement2.AlignmentOfContents = ContentAlignment.MiddleCenter;
		panelElement2.FixedWidth = 60;
		C1.Win.C1Tile.ImageElement item = new C1.Win.C1Tile.ImageElement();
		panelElement2.Children.Add(item);
		template.Elements.Add(panelElement2);
		PanelElement panelElement3 = new PanelElement();
		panelElement3.Dock = DockStyle.Fill;
		panelElement3.AlignmentOfContents = ContentAlignment.MiddleLeft;
		C1.Win.C1Tile.TextElement textElement2 = new C1.Win.C1Tile.TextElement();
		textElement2.SingleLine = false;
		textElement2.FontSize = 11f;
		textElement2.FontBold = ThreeStateBoolean.True;
		textElement2.TextSelector = TextSelector.Text1;
		textElement2.ForeColorSelector = ForeColorSelector.ForeColor1;
		panelElement3.Children.Add(textElement2);
		template.Elements.Add(panelElement3);
		return template;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private async Task<Guid?> CreateTeam(string teamName, int type)
	{
		Guid? createId = null;
		ProgressForm<JObject> progressForm = new ProgressForm<JObject>(async delegate(IProgress<ProgressInfo> iProg)
		{
			iProg.Report(new ProgressInfo
			{
				MainProgress = 100,
				MainCaption = "正在创建组织，请稍候..."
			});
			return await WebApiClient.CreateTeam(teamName, type);
		});
		progressForm.ShowDialog();
		try
		{
			JObject result = await progressForm.Task;
			result.Value<DateTime>("ExpireDate");
			result.Value<int>("TrialPeriod");
			await SignalRClient.ChangeTeamMember(Leqisoft.Model.User.Current.Id.ToString(), Guid.NewGuid().ToString(), Leqisoft.Model.User.Current.TeamId.ToString());
			createId = Guid.Parse(result.Value<string>("TeamId"));
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
		return createId;
	}

	private async void btnConfirm_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(txtTeamName.Text))
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "组织名称不允许为空");
			return;
		}
		if (txtTeamName.Text.Length > 50)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "组织名称不允许超过50个字符");
			return;
		}
		AppEditionBase appEditionBase;
		switch (Program.ClientPlatformType)
		{
		case PlatformType.AuditPlatform:
			appEditionBase = AppEditions.Audit;
			break;
		case PlatformType.EnterpriseReportPlatform:
			appEditionBase = AppEditions.EnterpriseReport;
			break;
		case PlatformType.EnterpriseManagerPlatform:
			appEditionBase = AppEditions.EnterpriseManager;
			break;
		case PlatformType.TableDevelopPlatform:
			appEditionBase = AppEditions.TableDevelop;
			break;
		case PlatformType.ProductionCostAccountingSystem:
			appEditionBase = AppEditions.ProductionCostAccountingSystem;
			break;
		case PlatformType.ContractLedgerManagementSystem:
			appEditionBase = AppEditions.ContractLedgerManagementSystem;
			break;
		case PlatformType.RDExpenseLedgerSystem:
			appEditionBase = AppEditions.RDExpenseLedgerSystem;
			break;
		case PlatformType.SalesOrderManagementSystem:
			appEditionBase = AppEditions.SalesOrderManagementSystem;
			break;
		case PlatformType.PSIManagementSystem:
			appEditionBase = AppEditions.PSIManagementSystem;
			break;
		case PlatformType.ProjectLedgerManagementSystem:
			appEditionBase = AppEditions.ProjectLedgerManagementSystem;
			break;
		case PlatformType.Custom:
			appEditionBase = AppEditions.CustomSystem;
			break;
		default:
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前客户端类型未知，无法创建对应的组织!");
			return;
		}
		Guid? guid2 = (TeamId = await CreateTeam(txtTeamName.Text.Trim(), appEditionBase.Code));
		Guid? guid3 = guid2;
		if (guid3.HasValue)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
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
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlHeader = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblTeamName = new C1.Win.C1Input.C1Label();
		this.txtTeamName = new C1.Win.C1Input.C1TextBox();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblTeamName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtTeamName).BeginInit();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		base.SuspendLayout();
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.c1SplitContainer1.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.c1SplitContainer1.HeaderHeight = 27;
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.pnlHeader);
		this.c1SplitContainer1.Panels.Add(this.pnlButtons);
		this.c1SplitContainer1.Size = new System.Drawing.Size(434, 118);
		this.c1SplitContainer1.SplitterWidth = 1;
		this.c1SplitContainer1.TabIndex = 0;
		this.pnlHeader.Controls.Add(this.lblTeamName);
		this.pnlHeader.Controls.Add(this.txtTeamName);
		this.pnlHeader.Height = 58;
		this.pnlHeader.KeepRelativeSize = false;
		this.pnlHeader.Location = new System.Drawing.Point(0, 0);
		this.pnlHeader.MinWidth = 52;
		this.pnlHeader.Name = "pnlHeader";
		this.pnlHeader.Resizable = false;
		this.pnlHeader.Size = new System.Drawing.Size(434, 58);
		this.pnlHeader.TabIndex = 0;
		this.pnlHeader.Width = 434;
		this.lblTeamName.AutoSize = true;
		this.lblTeamName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblTeamName.Location = new System.Drawing.Point(14, 24);
		this.lblTeamName.Name = "lblTeamName";
		this.lblTeamName.Size = new System.Drawing.Size(104, 17);
		this.lblTeamName.TabIndex = 1;
		this.lblTeamName.Tag = null;
		this.lblTeamName.Text = "请输入组织名称：";
		this.lblTeamName.TextDetached = true;
		this.txtTeamName.Location = new System.Drawing.Point(123, 22);
		this.txtTeamName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtTeamName.Name = "txtTeamName";
		this.txtTeamName.Size = new System.Drawing.Size(234, 21);
		this.txtTeamName.TabIndex = 0;
		this.txtTeamName.Tag = null;
		this.txtTeamName.TextDetached = true;
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnConfirm);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 59;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 59);
		this.pnlButtons.MinWidth = 52;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size(434, 59);
		this.pnlButtons.TabIndex = 2;
		this.pnlButtons.Width = 434;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(238, 24);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Location = new System.Drawing.Point(123, 24);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 1;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(434, 118);
		base.Controls.Add(this.c1SplitContainer1);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "dlgTeamCreator";
		this.Text = "创建组织";
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlHeader.ResumeLayout(false);
		this.pnlHeader.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.lblTeamName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtTeamName).EndInit();
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		base.ResumeLayout(false);
	}
}
