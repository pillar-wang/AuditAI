using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1Themes;
using Auditai.DTO;
using Auditai.Model;
using Auditai.SignalR;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Chat;
using Auditai.UI.Platform.Properties;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

public class ChatForm : C1RibbonForm, ISetTheme
{
	private enum ViewState
	{
		Team,
		Project
	}

	private delegate void InvokeCallback();

	private C1CommandLink lnkEmotion = new C1CommandLink();

	private C1Command cmdEmotion = new C1Command();

	private C1CommandLink lnkFile = new C1CommandLink();

	private C1Command cmdFile = new C1Command();

	private C1CommandLink lnkPush = new C1CommandLink();

	private C1Command cmdPush = new C1Command();

	private C1CommandLink lnkBarrage = new C1CommandLink();

	private C1Command cmdBarrage = new C1Command();

	private C1CommandLink lnkNotifySound = new C1CommandLink();

	private C1Command cmdNotifySound = new C1Command();

	private C1CommandLink lnkHistory = new C1CommandLink();

	private C1Command cmdHistory = new C1Command();

	private C1ContextMenu ctxProject = new C1ContextMenu();

	private C1ContextMenu ctxTeam = new C1ContextMenu();

	private C1Command cmdRefreshProjectMembers = new C1Command();

	private C1CommandLink lnkRefreshProjectMembers = new C1CommandLink();

	private C1Command cmdRefreshTeamMembers = new C1Command();

	private C1CommandLink lnkRefreshTeamMembers = new C1CommandLink();

	private TabFlickerProxy tabProjectMembersProxy;

	private TabFlickerProxy tabTeamMembersProxy;

	private EmojiManager emojiManager;

	private ViewState _viewState;

	private Dictionary<string, IRecordEditor> recordEditorMap;

	public IRecordPersistence recordPersistence = new SqlitePersistence();

	private bool _attached;

	private IContainer components;

	private C1CommandHolder c1CommandHolder1;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlChat;

	private C1SplitContainer ctnChat;

	private C1SplitterPanel pnlChatTitle;

	private C1SplitterPanel pnlChatRecord;

	private C1SplitterPanel pnlSend;

	private C1SplitContainer ctnSend;

	private C1SplitterPanel pnlSendToolbar;

	private C1ToolBar tlbSendToolbar;

	private C1CommandLink c1CommandLink1;

	private C1CommandLink c1CommandLink2;

	private C1CommandLink c1CommandLink3;

	private C1SplitterPanel pnlSendButton;

	private C1Button btnSend;

	private C1SplitterPanel pnlSendContent;

	private C1SplitterPanel pnlGroup;

	private C1Label lblchatName;

	private C1PictureBox btnChangeHeader;

	private C1PictureBox btnChat;

	private C1Label lblSelfName;

	private C1ToolBar c1ToolBar1;

	private C1CommandLink c1CommandLink4;

	private C1CommandLink c1CommandLink5;

	private C1CommandLink c1CommandLink6;

	private TextBox txtMessage;

	private C1DockingTab c1DockingTab1;

	private C1DockingTabPage tabProjectMembers;

	private C1FlexGrid grdProjectMembers;

	private C1DockingTabPage tabTeamMembers;

	private C1FlexGrid grdTeamMembers;

	public MemberListEditor ProjectMemberViewEditor { get; set; }

	public MemberListEditor TeamMemberViewEditor { get; set; }

	public bool IsBulletEnabled => cmdBarrage.Checked;

	public bool IsSoundEnabled => cmdNotifySound.Checked;

	public event EventHandler<string> SelectedMemberChanged;

	public ChatForm()
	{
		InitializeComponent();
		base.Shown += ChatForm_Shown;
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		string arg = Path.Combine(directoryName, "config", "record", $"{Auditai.Model.User.Current.Id}.db");
		recordPersistence.Load(arg);
		Initialize();
		ThemeManager.GetInstance().Register(this);
		MemberManager.GetInstance().OnlineStatusChanged += ChatForm_OnlineChanged;
		MemberManager.GetInstance().TeamMemberChanged += ChatForm_TeamMemberChanged;
		MemberManager.GetInstance().ProjectMemberChanged += ChatForm_ProjectMemberChanged;
		MemberManager.GetInstance().MemberInfoChanged += ChatForm_MemberInfoChanged;
		MemberManager.GetInstance().MessageArrived += ChatForm_MessageArrived;
		MemberManager.GetInstance().RePopulate += ChatForm_RePopulate;
		if (base.Owner == null)
		{
			base.Owner = Program.MainForm?.View;
		}
	}

	private void ChatForm_Shown(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.chat);
	}

	private void ChatForm_RePopulate(object sender, EventArgs e)
	{
		MemberManager instance = MemberManager.GetInstance();
		Group group = instance.GetGroup(SignalRClient.UserState.ProjectId);
		ProjectMemberViewEditor.Populate(group?.GetSelfAndMembers());
		Group group2 = instance.GetGroup(Auditai.Model.User.Current.TeamId.ToString());
		TeamMemberViewEditor.Populate(group2?.GetSelfAndMembers());
		Member member = instance.GetMember(Auditai.Model.User.Current.Id.ToString());
		if (member != null)
		{
			UpdatePersonalInfo(member.Name, member.Image);
		}
	}

	private void ChatForm_MessageArrived(object sender, Tuple<string, string, NotifyMessage> e)
	{
		string item = e.Item1;
		string item2 = e.Item2;
		NotifyMessage item3 = e.Item3;
		if (item3 == null)
		{
			return;
		}
		MemberManager instance = MemberManager.GetInstance();
		Member member = instance.GetMember(item2);
		Group group = instance.GetGroup(item2);
		if (group != null || member != null)
		{
			switch (_viewState)
			{
			case ViewState.Team:
				if (!TeamMemberViewEditor.StartFlicker(item2))
				{
					tabProjectMembersProxy.Start();
					ProjectMemberViewEditor.StartFlicker(item2);
				}
				break;
			case ViewState.Project:
				if (!ProjectMemberViewEditor.StartFlicker(item2))
				{
					tabTeamMembersProxy.Start();
					TeamMemberViewEditor.StartFlicker(item2);
				}
				break;
			}
		}
		if (base.Visible)
		{
			ChatWith(CurrentSelectedId(), donotSetTab: false);
		}
	}

	private void ChatForm_MemberInfoChanged(object sender, string e)
	{
		MemberManager instance = MemberManager.GetInstance();
		Group group = instance.GetGroup(SignalRClient.UserState.ProjectId);
		ProjectMemberViewEditor.Populate(group?.GetSelfAndMembers());
		Group group2 = instance.GetGroup(Auditai.Model.User.Current.TeamId.ToString());
		TeamMemberViewEditor.Populate(group2?.GetSelfAndMembers());
		Member member = instance.GetMember(Auditai.Model.User.Current.Id.ToString());
		if (member != null)
		{
			UpdatePersonalInfo(member.Name, member.Image);
		}
	}

	private void ChatForm_ProjectMemberChanged(object sender, string e)
	{
		MemberManager instance = MemberManager.GetInstance();
		Group group = instance.GetGroup(e);
		ProjectMemberViewEditor.Populate(group?.GetSelfAndMembers());
	}

	private void ChatForm_TeamMemberChanged(object sender, string e)
	{
		MemberManager instance = MemberManager.GetInstance();
		Group group = instance.GetGroup(Auditai.Model.User.Current.TeamId.ToString());
		TeamMemberViewEditor.Populate(group?.GetSelfAndMembers());
	}

	private void ChatForm_OnlineChanged(object sender, long e)
	{
		string userId = e.ToString();
		MemberManager instance = MemberManager.GetInstance();
		Member member = instance.GetMember(userId);
		if (member != null)
		{
			TeamMemberViewEditor.UpdateOnlineStatus(userId, member.IsOnline);
			ProjectMemberViewEditor.UpdateOnlineStatus(userId, member.IsOnline);
		}
	}

	public void UpdatePersonalInfo(string name, System.Drawing.Image image)
	{
		lblSelfName.Text = name;
		btnChangeHeader.Image = image;
	}

	public bool ChatWith(string memId, bool donotSetTab, bool show = true)
	{
		DettachEvent();
		try
		{
			if (show)
			{
				Show();
			}
			MemberManager instance = MemberManager.GetInstance();
			MemTab memTab = instance.GetMember(memId);
			if (memTab == null)
			{
				memTab = instance.GetGroup(memId);
			}
			if (memTab == null)
			{
				return false;
			}
			if (donotSetTab)
			{
				if (_viewState == ViewState.Project)
				{
					ProjectMemberViewEditor.ChatWith(memId);
				}
				else if (_viewState == ViewState.Team)
				{
					TeamMemberViewEditor.ChatWith(memId);
				}
			}
			else if (_viewState == ViewState.Project)
			{
				if (ProjectMemberViewEditor.ChatWith(memId))
				{
					c1DockingTab1.SelectedTab = tabProjectMembers;
					_viewState = ViewState.Project;
				}
				else if (TeamMemberViewEditor.ChatWith(memId))
				{
					c1DockingTab1.SelectedTab = tabTeamMembers;
					_viewState = ViewState.Team;
				}
			}
			else if (_viewState == ViewState.Team)
			{
				if (TeamMemberViewEditor.ChatWith(memId))
				{
					c1DockingTab1.SelectedTab = tabTeamMembers;
					_viewState = ViewState.Team;
				}
				else if (ProjectMemberViewEditor.ChatWith(memId))
				{
					c1DockingTab1.SelectedTab = tabProjectMembers;
					_viewState = ViewState.Project;
				}
			}
			lblchatName.Text = memTab.Name;
			btnChat.Image = ((memTab is Group) ? memTab.Image : ((memTab as Member).IsOnline ? memTab.Image : memTab.GrayImage));
			switch (_viewState)
			{
			case ViewState.Team:
				tabTeamMembersProxy.Stop();
				if (ProjectMemberViewEditor.AnyFlicker())
				{
					tabProjectMembersProxy.Start();
				}
				else
				{
					tabProjectMembersProxy.Stop();
				}
				break;
			case ViewState.Project:
				tabProjectMembersProxy.Stop();
				if (TeamMemberViewEditor.AnyFlicker())
				{
					tabTeamMembersProxy.Start();
				}
				else
				{
					tabTeamMembersProxy.Stop();
				}
				break;
			}
			TeamMemberViewEditor.StopFlicker(memId);
			ProjectMemberViewEditor.StopFlicker(memId);
			GetAndCreateRecordEditor(memId).View().BringToFront();
			while (memTab.UnhandleNotifyMessage.Count > 0)
			{
				TempRecord record = memTab.UnhandleNotifyMessage.Dequeue();
				GetAndCreateRecordEditor(memId).Append(record);
				recordPersistence.AddRecord(record);
			}
			this.SelectedMemberChanged?.Invoke(this, CurrentSelectedId());
			return true;
		}
		finally
		{
			AttachEvent();
		}
	}

	public void SetTheme()
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		ctnAll.SplitterWidth = 2;
		ctnChat.SplitterWidth = 0;
		ctnSend.SplitterWidth = 0;
		c1DockingTab1.TabsSpacing = 10;
		txtMessage.BorderStyle = BorderStyle.None;
		C1Theme c1Theme = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetC1Theme();
		string path = "BaseThemeProperties\\Styles\\Header\\Background";
		Color color = c1Theme.GetColor(path, Color.FromArgb(241, 241, 241));
		pnlChatTitle.BackColor = color;
		pnlSendButton.BackColor = color;
		foreach (KeyValuePair<string, IRecordEditor> item in recordEditorMap)
		{
			item.Value.SetTheme();
		}
	}

	public new void Show()
	{
		if (base.MinimizeBox)
		{
			base.WindowState = FormWindowState.Normal;
		}
		if (base.Owner == null)
		{
			base.Owner = Program.MainForm?.View;
		}
		base.Show();
	}

	private async void btnSend_Click(object sender, EventArgs e)
	{
		await SendMessageImpl();
	}

	private void CmdFile_Click(object sender, ClickEventArgs e)
	{
	}

	private async void CmdPush_Click(object sender, ClickEventArgs e)
	{
		await PushTreeNodeImpl();
	}

	private void CmdHistory_Click(object sender, ClickEventArgs e)
	{
	}

	private void CmdEmotion_Click(object sender, ClickEventArgs e)
	{
		Point point = tlbSendToolbar.PointToScreen(new Point(0, 0));
		emojiManager.Show(point, ToolStripDropDownDirection.AboveRight);
	}

	private async void BtnChangeHeader_Click(object sender, EventArgs e)
	{
		await ChangeHeaderImpl();
	}

	private void EmojiManager_EmojiSelected(object sender, string e)
	{
		txtMessage.SelectedText = e;
	}

	private void C1DockingTab1_SelectedTabChanged(object sender, EventArgs e)
	{
		_viewState = ((c1DockingTab1.SelectedTab == tabProjectMembers) ? ViewState.Project : ViewState.Team);
		string memId = CurrentSelectedId()?.ToString();
		ChatWith(memId, donotSetTab: true, base.Visible);
	}

	private void MemberListBox_SelectedChanged(object sender, MemTab e)
	{
		ChatWith(e.Id, donotSetTab: false, base.Visible);
	}

	private IRecordEditor GetAndCreateRecordEditor(string key)
	{
		if (recordEditorMap.ContainsKey(key))
		{
			return recordEditorMap[key];
		}
		TxRecordEditor txRecordEditor = new TxRecordEditor();
		recordEditorMap.Add(key, txRecordEditor);
		pnlChatRecord.Controls.Add(txRecordEditor.View());
		txRecordEditor.PreviousRecord += RecordEditor_PreviousRecord;
		return txRecordEditor;
	}

	private async void CmdRefreshProjectMembers_Click(object sender, ClickEventArgs e)
	{
		try
		{
			await ChatManager.UpdateProjectMember(Program.MainForm.CurrentProject.Id);
		}
		catch (Exception ex) when (ex is NormalException || ex.InnerException is NormalException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (Exception)
		{
		}
		MemberManager instance = MemberManager.GetInstance();
		instance.GetGroup(SignalRClient.UserState.ProjectId);
		TeamMemberViewEditor.Populate(instance.GetGroup(Auditai.Model.User.Current.TeamId.ToString())?.GetSelfAndMembers());
		ProjectMemberViewEditor.Populate(instance.GetGroup(SignalRClient.UserState.ProjectId)?.GetSelfAndMembers());
		Program.MainForm.TabMember.Populate();
	}

	private async void CmdRefreshTeamMembers_Click(object sender, ClickEventArgs e)
	{
		try
		{
			await ChatManager.UpdateTeamMember();
		}
		catch (Exception ex) when (ex is NormalException || ex.InnerException is NormalException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (Exception)
		{
		}
		MemberManager instance = MemberManager.GetInstance();
		ProjectMemberViewEditor.Populate(instance.GetGroup(SignalRClient.UserState.ProjectId)?.GetSelfAndMembers());
		TeamMemberViewEditor.Populate(instance.GetGroup(Auditai.Model.User.Current.TeamId.ToString())?.GetSelfAndMembers());
		Program.MainForm.TabMember.Populate();
	}

	private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall && e.CloseReason == CloseReason.UserClosing)
		{
			e.Cancel = true;
			Hide();
			if (base.Owner == null)
			{
				base.Owner = Program.MainForm?.View;
			}
			base.Owner.TopLevel = true;
		}
	}

	private void Initialize()
	{
		c1DockingTab1.ShowTabs = false;
		recordEditorMap = new Dictionary<string, IRecordEditor>();
		base.TopLevel = true;
		base.StartPosition = FormStartPosition.CenterScreen;
		tlbSendToolbar.CommandLinks.Clear();
		cmdEmotion.Text = "表情";
		cmdEmotion.Image = Resources.smile;
		cmdEmotion.Click += CmdEmotion_Click;
		lnkEmotion.Command = cmdEmotion;
		tlbSendToolbar.CommandLinks.Add(lnkEmotion);
		cmdFile.Text = "发送文件";
		cmdFile.Image = Resources.transFile;
		cmdFile.Click += CmdFile_Click;
		lnkFile.Command = cmdFile;
		cmdPush.Text = "推送在线云文件";
		cmdPush.Image = Resources.transPush;
		cmdPush.Click += CmdPush_Click;
		lnkPush.Command = cmdPush;
		tlbSendToolbar.CommandLinks.Add(lnkPush);
		cmdHistory.Text = "聊天记录";
		cmdHistory.Image = Resources.chatHistory;
		cmdHistory.Click += CmdHistory_Click;
		lnkHistory.Command = cmdHistory;
		c1ToolBar1.CommandLinks.Clear();
		cmdNotifySound.Checked = true;
		cmdNotifySound.Text = "消息提示音";
		cmdNotifySound.CheckAutoToggle = true;
		cmdNotifySound.Image = Resources.NotifySound;
		lnkNotifySound.Command = cmdNotifySound;
		lnkNotifySound.ButtonLook = ButtonLookFlags.TextAndImage;
		c1ToolBar1.CommandLinks.Add(lnkNotifySound);
		cmdBarrage.Checked = true;
		cmdBarrage.Text = "弹幕";
		cmdBarrage.CheckAutoToggle = true;
		cmdBarrage.Image = Resources.barrage;
		lnkBarrage.ButtonLook = ButtonLookFlags.TextAndImage;
		lnkBarrage.Command = cmdBarrage;
		c1ToolBar1.CommandLinks.Add(lnkBarrage);
		btnChangeHeader.Cursor = Cursors.Hand;
		btnChangeHeader.Click += BtnChangeHeader_Click;
		cmdRefreshProjectMembers.Text = "刷新";
		cmdRefreshProjectMembers.Click += CmdRefreshProjectMembers_Click;
		lnkRefreshProjectMembers.Command = cmdRefreshProjectMembers;
		ctxProject.CommandLinks.Add(lnkRefreshProjectMembers);
		c1CommandHolder1.SetC1ContextMenu(grdProjectMembers, ctxProject);
		cmdRefreshTeamMembers.Text = "刷新";
		cmdRefreshTeamMembers.Click += CmdRefreshTeamMembers_Click;
		lnkRefreshTeamMembers.Command = cmdRefreshTeamMembers;
		ctxTeam.CommandLinks.Add(lnkRefreshTeamMembers);
		c1CommandHolder1.SetC1ContextMenu(grdTeamMembers, ctxTeam);
		emojiManager = new EmojiManager(10, 8);
		emojiManager.Width = 10;
		emojiManager.EmojiSelected += EmojiManager_EmojiSelected;
		TeamMemberViewEditor = new MemberListEditor(grdTeamMembers);
		TeamMemberViewEditor.SetFlickerTimer(SecondTrigger.Trigger);
		ProjectMemberViewEditor = new MemberListEditor(grdProjectMembers);
		ProjectMemberViewEditor.SetFlickerTimer(SecondTrigger.Trigger);
		tabTeamMembersProxy = new TabFlickerProxy(tabTeamMembers);
		tabTeamMembersProxy.UpdateEmptyImage(Resources.Empty32);
		tabTeamMembersProxy.SetTimer(SecondTrigger.Trigger);
		tabProjectMembersProxy = new TabFlickerProxy(tabProjectMembers);
		tabProjectMembersProxy.UpdateEmptyImage(Resources.Empty32);
		tabProjectMembersProxy.SetTimer(SecondTrigger.Trigger);
		_viewState = ViewState.Project;
		c1DockingTab1.SelectedTab = tabProjectMembers;
		SetTheme();
		MemberManager instance = MemberManager.GetInstance();
		Member member = instance.GetMember(Auditai.Model.User.Current.Id.ToString());
		if (member != null)
		{
			UpdatePersonalInfo(member.Name, member.Image);
		}
		Group group = instance.GetGroup(Auditai.Model.User.Current.TeamId.ToString());
		Group group2 = instance.GetGroup(SignalRClient.UserState.ProjectId);
		TeamMemberViewEditor.Populate(group?.GetSelfAndMembers());
		ProjectMemberViewEditor.Populate(group2?.GetSelfAndMembers());
		AttachEvent();
		base.FormClosing += ChatForm_FormClosing;
	}

	private void RecordEditor_PreviousRecord(object sender, EventArgs e)
	{
		MemTab memTab = CurrentSelectedMember();
		if (memTab != null)
		{
			var records = recordPersistence.GetRecords(memTab.Id.ToString(), memTab.TempMostEarly);
			(sender as IRecordEditor).Insert(records.Cast<ChatRecord>());
		}
	}

	private async Task ChangeHeaderImpl()
	{
		try
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "图片文件|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tiff"
			};
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				object obj = System.Drawing.Image.FromFile(openFileDialog.FileName).Clone();
				Bitmap saveImage = ((System.Drawing.Image)obj).ToSize(100, 100);
				using (MemoryStream ms = new MemoryStream())
				{
					saveImage.Save(ms, ImageFormat.Png);
					byte[] buffer = ms.GetBuffer();
					await WebApiClient.UpdatePicture(buffer);
				}
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "头像更新成功");
				ChatManager.OnAfterHeadChanged(saveImage);
				await SignalRClient.ChangeMemberInfo(Auditai.Model.User.Current.Id.ToString());
			}
		}
		catch (Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改头像失败，失败原因：" + ex.Message);
		}
	}

	private async Task PushTreeNodeImpl()
	{
		MemTab memTab = CurrentSelectedMember();
		if (memTab == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择用户沟通！");
			return;
		}
		if (memTab is Group)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "群聊不支持推送文件");
			return;
		}
		MainForm mainForm = Program.MainForm;
		if (!Guid.TryParse((memTab as Member).UserState?.ProjectId, out var result) || result != mainForm.CurrentProject.Id)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "用户当前不在本" + StringConstBase.Current.Project + "，无法推送文件");
			return;
		}
		TreeNodeBase selectedNode = Program.MainForm.ProjectHierarchy.SelectedNode;
		if (selectedNode == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您当前未选择任何节点，无法推送文件");
			return;
		}
		JObject jObject = new JObject();
		jObject.Add("nodeId", selectedNode.Id.ToString());
		jObject.Add("nodeName", selectedNode.Name);
		jObject.Add("name", Auditai.Model.User.Current.Name);
		NotifyMessage notifyMessage = new NotifyMessage
		{
			Bullet = true,
			Value = jObject.ToString(),
			Text = Auditai.Model.User.Current.Name + "向您推送来 " + selectedNode.Name
		};
		await SignalRClient.PushTreeNodeToUser(memTab.Id.ToString(), notifyMessage.ToString());
	}

	private async Task SendMessageImpl()
	{
		Cursor cursor = txtMessage.Cursor;
		try
		{
			MemTab memTab = CurrentSelectedMember();
			if (memTab == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择用户沟通");
				return;
			}
			string text = txtMessage.Text;
			if (string.IsNullOrWhiteSpace(text))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "发送消息不能为空！");
				return;
			}
			txtMessage.Text = string.Empty;
			ChatRecord record = new ChatRecord(memTab.Id, Auditai.Model.User.Current.Id.ToString(), text);
			recordPersistence.AddRecord(record);
			GetAndCreateRecordEditor(memTab.Id).Append(record);
			GetAndCreateRecordEditor(memTab.Id).ScrollEnd();
			text = new NotifyMessage
			{
				Text = text,
				Bullet = cmdBarrage.Checked
			}.ToString();
			if (memTab is Group)
			{
				if (memTab.Id == Auditai.Model.User.Current.TeamId.ToString())
				{
					await SignalRClient.BroadcastToTeamUsers(text);
				}
				else if (memTab.Id == SignalRClient.UserState.ProjectId)
				{
					await SignalRClient.BroadcastToProjectUsers(text);
				}
			}
			else
			{
				await SignalRClient.SendToUser(memTab.Id.ToString(), text);
			}
		}
		finally
		{
			txtMessage.Cursor = cursor;
		}
	}

	private MemTab CurrentSelectedMember()
	{
		if (_viewState != ViewState.Project)
		{
			return TeamMemberViewEditor.CurrentMemTab;
		}
		return ProjectMemberViewEditor.CurrentMemTab;
	}

	private string CurrentSelectedId()
	{
		return CurrentSelectedMember()?.Id ?? null;
	}

	private void AttachEvent()
	{
		if (!_attached)
		{
			c1DockingTab1.SelectedTabChanged += C1DockingTab1_SelectedTabChanged;
			TeamMemberViewEditor.CurrentChanged += MemberListBox_SelectedChanged;
			ProjectMemberViewEditor.CurrentChanged += MemberListBox_SelectedChanged;
			_attached = true;
		}
	}

	private void DettachEvent()
	{
		if (_attached)
		{
			c1DockingTab1.SelectedTabChanged -= C1DockingTab1_SelectedTabChanged;
			TeamMemberViewEditor.CurrentChanged -= MemberListBox_SelectedChanged;
			ProjectMemberViewEditor.CurrentChanged -= MemberListBox_SelectedChanged;
			_attached = false;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Platform.ChatForm));
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlGroup = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1DockingTab1 = new C1.Win.C1Command.C1DockingTab();
		this.tabProjectMembers = new C1.Win.C1Command.C1DockingTabPage();
		this.grdProjectMembers = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.tabTeamMembers = new C1.Win.C1Command.C1DockingTabPage();
		this.grdTeamMembers = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.pnlChat = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.ctnChat = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlChatTitle = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnChat = new C1.Win.C1Input.C1PictureBox();
		this.lblSelfName = new C1.Win.C1Input.C1Label();
		this.btnChangeHeader = new C1.Win.C1Input.C1PictureBox();
		this.lblchatName = new C1.Win.C1Input.C1Label();
		this.pnlChatRecord = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlSend = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.ctnSend = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlSendToolbar = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1ToolBar1 = new C1.Win.C1Command.C1ToolBar();
		this.c1CommandLink4 = new C1.Win.C1Command.C1CommandLink();
		this.c1CommandLink5 = new C1.Win.C1Command.C1CommandLink();
		this.c1CommandLink6 = new C1.Win.C1Command.C1CommandLink();
		this.tlbSendToolbar = new C1.Win.C1Command.C1ToolBar();
		this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink();
		this.c1CommandLink2 = new C1.Win.C1Command.C1CommandLink();
		this.c1CommandLink3 = new C1.Win.C1Command.C1CommandLink();
		this.pnlSendButton = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnSend = new C1.Win.C1Input.C1Button();
		this.pnlSendContent = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtMessage = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlGroup.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1DockingTab1).BeginInit();
		this.c1DockingTab1.SuspendLayout();
		this.tabProjectMembers.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdProjectMembers).BeginInit();
		this.tabTeamMembers.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdTeamMembers).BeginInit();
		this.pnlChat.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ctnChat).BeginInit();
		this.ctnChat.SuspendLayout();
		this.pnlChatTitle.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnChat).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblSelfName).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnChangeHeader).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblchatName).BeginInit();
		this.pnlSend.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ctnSend).BeginInit();
		this.ctnSend.SuspendLayout();
		this.pnlSendToolbar.SuspendLayout();
		this.pnlSendButton.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnSend).BeginInit();
		this.pnlSendContent.SuspendLayout();
		base.SuspendLayout();
		this.c1CommandHolder1.Owner = this;
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(197, 207, 223);
		this.ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(225, 232, 237);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(119, 136, 153);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.FixedLineColor = System.Drawing.Color.FromArgb(138, 156, 184);
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(30, 57, 91);
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlGroup);
		this.ctnAll.Panels.Add(this.pnlChat);
		this.ctnAll.Panels.Add(this.pnlSend);
		this.ctnAll.Size = new System.Drawing.Size(833, 517);
		this.ctnAll.SplitterColor = System.Drawing.Color.FromArgb(138, 156, 184);
		this.ctnAll.SplitterWidth = 2;
		this.ctnAll.TabIndex = 0;
		this.pnlGroup.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
		this.pnlGroup.Controls.Add(this.c1DockingTab1);
		this.pnlGroup.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlGroup.KeepRelativeSize = false;
		this.pnlGroup.Location = new System.Drawing.Point(0, 0);
		this.pnlGroup.Name = "pnlGroup";
		this.pnlGroup.Size = new System.Drawing.Size(228, 517);
		this.pnlGroup.SizeRatio = 27.437;
		this.pnlGroup.TabIndex = 3;
		this.pnlGroup.Width = 228;
		this.c1DockingTab1.Alignment = System.Windows.Forms.TabAlignment.Left;
		this.c1DockingTab1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1DockingTab1.Controls.Add(this.tabProjectMembers);
		this.c1DockingTab1.Controls.Add(this.tabTeamMembers);
		this.c1DockingTab1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1DockingTab1.Indent = 20;
		this.c1DockingTab1.Location = new System.Drawing.Point(0, 0);
		this.c1DockingTab1.Margin = new System.Windows.Forms.Padding(0);
		this.c1DockingTab1.Name = "c1DockingTab1";
		this.c1DockingTab1.Padding = new System.Drawing.Point(5, 5);
		this.c1DockingTab1.Size = new System.Drawing.Size(228, 517);
		this.c1DockingTab1.TabAreaSpacing = 5;
		this.c1DockingTab1.TabIndex = 3;
		this.c1DockingTab1.TabLook = C1.Win.C1Command.ButtonLookFlags.Image;
		this.c1DockingTab1.TabsShowFocusCues = false;
		this.c1DockingTab1.TabsSpacing = 15;
		this.c1DockingTab1.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.tabProjectMembers.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.tabProjectMembers.Controls.Add(this.grdProjectMembers);
		this.tabProjectMembers.Image = Auditai.UI.Platform.Properties.Resources.member;
		this.tabProjectMembers.Location = new System.Drawing.Point(48, 0);
		this.tabProjectMembers.Name = "tabProjectMembers";
		this.tabProjectMembers.Size = new System.Drawing.Size(180, 517);
		this.tabProjectMembers.TabIndex = 0;
		this.grdProjectMembers.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdProjectMembers.ColumnInfo = "0,0,0,0,0,100,Columns:";
		this.grdProjectMembers.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdProjectMembers.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.grdProjectMembers.Location = new System.Drawing.Point(0, 0);
		this.grdProjectMembers.Name = "grdProjectMembers";
		this.grdProjectMembers.Rows.Count = 0;
		this.grdProjectMembers.Rows.DefaultSize = 20;
		this.grdProjectMembers.Rows.Fixed = 0;
		this.grdProjectMembers.Size = new System.Drawing.Size(180, 517);
		this.grdProjectMembers.StyleInfo = resources.GetString("grdProjectMembers.StyleInfo");
		this.grdProjectMembers.TabIndex = 1;
		this.tabTeamMembers.Controls.Add(this.grdTeamMembers);
		this.tabTeamMembers.Image = Auditai.UI.Platform.Properties.Resources.group;
		this.tabTeamMembers.Location = new System.Drawing.Point(48, 0);
		this.tabTeamMembers.Name = "tabTeamMembers";
		this.tabTeamMembers.Size = new System.Drawing.Size(180, 517);
		this.tabTeamMembers.TabIndex = 1;
		this.grdTeamMembers.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdTeamMembers.ColumnInfo = "0,0,0,0,0,100,Columns:";
		this.grdTeamMembers.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdTeamMembers.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.grdTeamMembers.Location = new System.Drawing.Point(0, 0);
		this.grdTeamMembers.Name = "grdTeamMembers";
		this.grdTeamMembers.Rows.Count = 0;
		this.grdTeamMembers.Rows.DefaultSize = 20;
		this.grdTeamMembers.Rows.Fixed = 0;
		this.grdTeamMembers.Size = new System.Drawing.Size(180, 517);
		this.grdTeamMembers.StyleInfo = resources.GetString("grdTeamMembers.StyleInfo");
		this.grdTeamMembers.TabIndex = 2;
		this.pnlChat.Controls.Add(this.ctnChat);
		this.pnlChat.Height = 346;
		this.pnlChat.Location = new System.Drawing.Point(230, 0);
		this.pnlChat.Name = "pnlChat";
		this.pnlChat.Size = new System.Drawing.Size(603, 346);
		this.pnlChat.SizeRatio = 67.181;
		this.pnlChat.TabIndex = 1;
		this.ctnChat.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnChat.BackColor = System.Drawing.Color.FromArgb(197, 207, 223);
		this.ctnChat.CollapsingAreaColor = System.Drawing.Color.FromArgb(225, 232, 237);
		this.ctnChat.CollapsingCueColor = System.Drawing.Color.FromArgb(119, 136, 153);
		this.ctnChat.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnChat.FixedLineColor = System.Drawing.Color.FromArgb(138, 156, 184);
		this.ctnChat.ForeColor = System.Drawing.Color.FromArgb(30, 57, 91);
		this.ctnChat.Location = new System.Drawing.Point(0, 0);
		this.ctnChat.Name = "ctnChat";
		this.ctnChat.Panels.Add(this.pnlChatTitle);
		this.ctnChat.Panels.Add(this.pnlChatRecord);
		this.ctnChat.Size = new System.Drawing.Size(603, 346);
		this.ctnChat.SplitterColor = System.Drawing.Color.FromArgb(138, 156, 184);
		this.ctnChat.SplitterWidth = 0;
		this.ctnChat.TabIndex = 0;
		this.pnlChatTitle.BackColor = System.Drawing.Color.White;
		this.pnlChatTitle.Controls.Add(this.btnChat);
		this.pnlChatTitle.Controls.Add(this.lblSelfName);
		this.pnlChatTitle.Controls.Add(this.btnChangeHeader);
		this.pnlChatTitle.Controls.Add(this.lblchatName);
		this.pnlChatTitle.Height = 40;
		this.pnlChatTitle.KeepRelativeSize = false;
		this.pnlChatTitle.Location = new System.Drawing.Point(0, 0);
		this.pnlChatTitle.Name = "pnlChatTitle";
		this.pnlChatTitle.Size = new System.Drawing.Size(603, 40);
		this.pnlChatTitle.SizeRatio = 13.115;
		this.pnlChatTitle.TabIndex = 0;
		this.btnChat.Location = new System.Drawing.Point(1, 2);
		this.btnChat.Name = "btnChat";
		this.btnChat.Size = new System.Drawing.Size(32, 32);
		this.btnChat.TabIndex = 6;
		this.btnChat.TabStop = false;
		this.lblSelfName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblSelfName.BackColor = System.Drawing.Color.Transparent;
		this.lblSelfName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblSelfName.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSelfName.ForeColor = System.Drawing.Color.Black;
		this.lblSelfName.Location = new System.Drawing.Point(355, 3);
		this.lblSelfName.Name = "lblSelfName";
		this.lblSelfName.Size = new System.Drawing.Size(207, 32);
		this.lblSelfName.TabIndex = 5;
		this.lblSelfName.Tag = null;
		this.lblSelfName.Text = "登录账号";
		this.lblSelfName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblSelfName.TextDetached = true;
		this.lblSelfName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.btnChangeHeader.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnChangeHeader.Location = new System.Drawing.Point(568, 3);
		this.btnChangeHeader.Name = "btnChangeHeader";
		this.btnChangeHeader.Size = new System.Drawing.Size(32, 32);
		this.btnChangeHeader.TabIndex = 4;
		this.btnChangeHeader.TabStop = false;
		this.lblchatName.BackColor = System.Drawing.Color.Transparent;
		this.lblchatName.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblchatName.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblchatName.ForeColor = System.Drawing.Color.Black;
		this.lblchatName.Location = new System.Drawing.Point(39, 2);
		this.lblchatName.Name = "lblchatName";
		this.lblchatName.Size = new System.Drawing.Size(152, 32);
		this.lblchatName.TabIndex = 2;
		this.lblchatName.Tag = null;
		this.lblchatName.Text = "当前同事";
		this.lblchatName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lblchatName.TextDetached = true;
		this.lblchatName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.pnlChatRecord.Height = 306;
		this.pnlChatRecord.Location = new System.Drawing.Point(0, 40);
		this.pnlChatRecord.Name = "pnlChatRecord";
		this.pnlChatRecord.Size = new System.Drawing.Size(603, 306);
		this.pnlChatRecord.TabIndex = 1;
		this.pnlSend.Controls.Add(this.ctnSend);
		this.pnlSend.Height = 169;
		this.pnlSend.Location = new System.Drawing.Point(230, 348);
		this.pnlSend.Name = "pnlSend";
		this.pnlSend.Size = new System.Drawing.Size(603, 169);
		this.pnlSend.TabIndex = 2;
		this.ctnSend.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnSend.BackColor = System.Drawing.Color.FromArgb(197, 207, 223);
		this.ctnSend.CollapsingAreaColor = System.Drawing.Color.FromArgb(225, 232, 237);
		this.ctnSend.CollapsingCueColor = System.Drawing.Color.FromArgb(119, 136, 153);
		this.ctnSend.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnSend.FixedLineColor = System.Drawing.Color.FromArgb(138, 156, 184);
		this.ctnSend.ForeColor = System.Drawing.Color.FromArgb(30, 57, 91);
		this.ctnSend.Location = new System.Drawing.Point(0, 0);
		this.ctnSend.Name = "ctnSend";
		this.ctnSend.Panels.Add(this.pnlSendToolbar);
		this.ctnSend.Panels.Add(this.pnlSendButton);
		this.ctnSend.Panels.Add(this.pnlSendContent);
		this.ctnSend.Size = new System.Drawing.Size(603, 169);
		this.ctnSend.SplitterColor = System.Drawing.Color.FromArgb(138, 156, 184);
		this.ctnSend.SplitterWidth = 0;
		this.ctnSend.TabIndex = 0;
		this.pnlSendToolbar.Controls.Add(this.c1ToolBar1);
		this.pnlSendToolbar.Controls.Add(this.tlbSendToolbar);
		this.pnlSendToolbar.Height = 25;
		this.pnlSendToolbar.KeepRelativeSize = false;
		this.pnlSendToolbar.Location = new System.Drawing.Point(0, 0);
		this.pnlSendToolbar.MinHeight = 25;
		this.pnlSendToolbar.Name = "pnlSendToolbar";
		this.pnlSendToolbar.Size = new System.Drawing.Size(603, 25);
		this.pnlSendToolbar.SizeRatio = 0.0;
		this.pnlSendToolbar.TabIndex = 2;
		this.c1ToolBar1.AccessibleName = "Tool Bar";
		this.c1ToolBar1.AutoSize = false;
		this.c1ToolBar1.BackColor = System.Drawing.Color.White;
		this.c1ToolBar1.Border.Width = 0;
		this.c1ToolBar1.CommandHolder = this.c1CommandHolder1;
		this.c1ToolBar1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[3] { this.c1CommandLink4, this.c1CommandLink5, this.c1CommandLink6 });
		this.c1ToolBar1.Dock = System.Windows.Forms.DockStyle.Right;
		this.c1ToolBar1.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1ToolBar1.Location = new System.Drawing.Point(539, 0);
		this.c1ToolBar1.Movable = false;
		this.c1ToolBar1.Name = "c1ToolBar1";
		this.c1ToolBar1.Size = new System.Drawing.Size(160, 25);
		this.c1ToolBar1.Text = "c1ToolBar1";
		this.c1ToolBar1.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.c1ToolBar1.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.c1CommandLink4.Text = "新命令";
		this.c1CommandLink5.SortOrder = 1;
		this.c1CommandLink6.SortOrder = 2;
		this.tlbSendToolbar.AccessibleName = "Tool Bar";
		this.tlbSendToolbar.AutoSize = false;
		this.tlbSendToolbar.BackColor = System.Drawing.Color.White;
		this.tlbSendToolbar.Border.Width = 0;
		this.tlbSendToolbar.CommandHolder = this.c1CommandHolder1;
		this.tlbSendToolbar.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[3] { this.c1CommandLink1, this.c1CommandLink2, this.c1CommandLink3 });
		this.tlbSendToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tlbSendToolbar.Location = new System.Drawing.Point(0, 0);
		this.tlbSendToolbar.Movable = false;
		this.tlbSendToolbar.Name = "tlbSendToolbar";
		this.tlbSendToolbar.Size = new System.Drawing.Size(603, 25);
		this.tlbSendToolbar.Text = "c1ToolBar1";
		this.tlbSendToolbar.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.tlbSendToolbar.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.c1CommandLink1.Text = "新命令";
		this.c1CommandLink2.SortOrder = 1;
		this.c1CommandLink3.SortOrder = 2;
		this.pnlSendButton.BackColor = System.Drawing.Color.White;
		this.pnlSendButton.Controls.Add(this.btnSend);
		this.pnlSendButton.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlSendButton.Height = 40;
		this.pnlSendButton.KeepRelativeSize = false;
		this.pnlSendButton.Location = new System.Drawing.Point(0, 129);
		this.pnlSendButton.Name = "pnlSendButton";
		this.pnlSendButton.Size = new System.Drawing.Size(603, 40);
		this.pnlSendButton.SizeRatio = 27.778;
		this.pnlSendButton.TabIndex = 1;
		this.btnSend.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnSend.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnSend.Location = new System.Drawing.Point(516, 14);
		this.btnSend.Name = "btnSend";
		this.btnSend.Size = new System.Drawing.Size(75, 23);
		this.btnSend.TabIndex = 0;
		this.btnSend.Text = "发送";
		this.btnSend.UseVisualStyleBackColor = true;
		this.btnSend.Click += new System.EventHandler(btnSend_Click);
		this.pnlSendContent.Controls.Add(this.txtMessage);
		this.pnlSendContent.Height = 104;
		this.pnlSendContent.Location = new System.Drawing.Point(0, 25);
		this.pnlSendContent.Name = "pnlSendContent";
		this.pnlSendContent.Size = new System.Drawing.Size(603, 104);
		this.pnlSendContent.SizeRatio = 100.0;
		this.pnlSendContent.TabIndex = 0;
		this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtMessage.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtMessage.Location = new System.Drawing.Point(0, 0);
		this.txtMessage.Multiline = true;
		this.txtMessage.Name = "txtMessage";
		this.txtMessage.Size = new System.Drawing.Size(603, 104);
		this.txtMessage.TabIndex = 0;
		base.AcceptButton = this.btnSend;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(833, 517);
		base.Controls.Add(this.ctnAll);
		this.DoubleBuffered = true;
		base.Name = "ChatForm";
		this.Text = "即时讨论";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlGroup.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1DockingTab1).EndInit();
		this.c1DockingTab1.ResumeLayout(false);
		this.tabProjectMembers.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdProjectMembers).EndInit();
		this.tabTeamMembers.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdTeamMembers).EndInit();
		this.pnlChat.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ctnChat).EndInit();
		this.ctnChat.ResumeLayout(false);
		this.pnlChatTitle.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnChat).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblSelfName).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnChangeHeader).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblchatName).EndInit();
		this.pnlSend.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ctnSend).EndInit();
		this.ctnSend.ResumeLayout(false);
		this.pnlSendToolbar.ResumeLayout(false);
		this.pnlSendButton.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnSend).EndInit();
		this.pnlSendContent.ResumeLayout(false);
		this.pnlSendContent.PerformLayout();
		base.ResumeLayout(false);
	}
}
