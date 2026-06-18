using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Ribbon;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.CommonControls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform.Chat;

public class TabMember
{
	private FlickerManager flickerManager = new FlickerManager();

	private RibbonTabFlickerProxy _tabProxy;

	private RibbonGroup ribbonGroupProject;

	private RibbonGroup ribbonGroupTeam;

	private RibbonGroup ribbonGroupSystem;

	private FlickerManager systemMessageFlickerManager = new FlickerManager();

	public RibbonTab View { get; }

	public TabMember(RibbonTab ribbonTab)
	{
		View = ribbonTab;
		View.Select += View_Select;
		View.Deselect += View_Deselect;
		ribbonGroupProject = View.Groups.Add("即时讨论");
		ribbonGroupProject.Image = Resources.chatITab16;
		ribbonGroupTeam = View.Groups.Add("同事列表");
		ribbonGroupTeam.Visible = false;
		ribbonGroupSystem = View.Groups.Add("消息通知");
		_tabProxy = new RibbonTabFlickerProxy(View);
		_tabProxy.SetTimer(SecondTrigger.Trigger);
		_tabProxy.UpdateOrignImage(null);
		_tabProxy.UpdateTwinkleContent("我的消息");
		MemberManager.GetInstance().OnlineStatusChanged += TabMember_OnlineChanged;
		MemberManager.GetInstance().OpenProjectChanged += TabMember_ProjectChanged;
		MemberManager.GetInstance().TeamMemberChanged += TabMember_TeamMemberChanged;
		MemberManager.GetInstance().ProjectMemberChanged += TabMember_ProjectMemberChanged;
		MemberManager.GetInstance().MemberInfoChanged += TabMember_MemberInfoChanged;
		MemberManager.GetInstance().PushTreeNode += TabMember_PushTreeNode;
		MemberManager.GetInstance().RecieveFileMesssage += TabMember_RecieveFileMesssage;
		MemberManager.GetInstance().MessageArrived += TabMember_MessageArrived;
		MemberManager.GetInstance().RePopulate += TabMember_RePopulate;
	}

	private void TabMember_RePopulate(object sender, EventArgs e)
	{
		Populate();
	}

	private void TabMember_MessageArrived(object sender, Tuple<string, string, NotifyMessage> e)
	{
		MemberManager instance = MemberManager.GetInstance();
		Member member = instance.GetMember(e.Item1);
		if (member != null)
		{
			if (!Exists(e.Item2.ToString()))
			{
				AppendCollegue(member);
			}
			StartFlicker(e.Item2);
		}
	}

	private void TabMember_PushTreeNode(object sender, Tuple<long, long, string, string, string> e)
	{
		StartFlicker(e.Item1.ToString());
	}

	private void TabMember_RecieveFileMesssage(object sender, Tuple<Member, NotifyMessage> e)
	{
		string id = e.Item1.Id;
		StartFlicker(id.ToString());
	}

	private void TabMember_MemberInfoChanged(object sender, string e)
	{
		Populate();
	}

	private void TabMember_ProjectMemberChanged(object sender, string e)
	{
		Populate();
	}

	private void TabMember_TeamMemberChanged(object sender, string e)
	{
		Populate();
	}

	private void TabMember_ProjectChanged(object sender, long e)
	{
		Populate();
	}

	public void Populate()
	{
		flickerManager.Dispose();
		PopulatePorjectMembers();
		PopulateTeamMembers();
	}

	public void PopulateSystemGroup()
	{
		View.Ribbon.Invoke((MethodInvoker)delegate
		{
			systemMessageFlickerManager.Dispose();
			ribbonGroupSystem.Items.ClearAndDisposeItems();
			IEnumerable<KeyValuePair<string, Member>> enumerable = MessageHandle.OtherGroupMember.Where((KeyValuePair<string, Member> m) => m.Value.UnhandleActionMessage.Count > 0);
			if (enumerable.Count() == 0)
			{
				ribbonGroupSystem.Visible = false;
				return;
			}
			ribbonGroupSystem.Visible = true;
			foreach (KeyValuePair<string, Member> item in enumerable)
			{
				Member member = item.Value;
				if (!systemMessageFlickerManager.Contains(item.Key))
				{
					RibbonButton rb = new RibbonButton();
					rb.Tag = member;
					rb.Text = member.Name;
					rb.LargeImage = member.Image;
					rb.TextImageRelation = C1.Win.C1Ribbon.TextImageRelation.ImageAboveText;
					rb.Click += delegate
					{
						if (member.UnhandleActionMessage.Count > 0)
						{
							IActionMessage actionMessage = member.UnhandleActionMessage.Dequeue();
							actionMessage?.ActionDeal(actionMessage.Content);
							if (member.UnhandleActionMessage.Count == 0)
							{
								systemMessageFlickerManager.Remove(member.Id);
								ribbonGroupSystem.Items.Remove(rb);
								MessageHandle.OtherGroupMember.Remove(member.Id);
							}
						}
						else
						{
							systemMessageFlickerManager.Remove(member.Id);
							ribbonGroupSystem.Items.Remove(rb);
							MessageHandle.OtherGroupMember.Remove(member.Id);
						}
						if (MessageHandle.OtherGroupMember.Any((KeyValuePair<string, Member> kv2) => kv2.Value.UnhandleActionMessage.Count > 0))
						{
							ribbonGroupSystem.Visible = true;
						}
						else
						{
							ribbonGroupSystem.Visible = false;
						}
					};
					ribbonGroupSystem.Items.Add(rb);
					RibbonLargeFlickerProxy ribbonLargeFlickerProxy = new RibbonLargeFlickerProxy(rb);
					ribbonLargeFlickerProxy.UpdateEmptyImage(Resources.Empty32);
					ribbonLargeFlickerProxy.SetTimer(SecondTrigger.Trigger);
					systemMessageFlickerManager.Add(member.Id, ribbonLargeFlickerProxy);
					systemMessageFlickerManager.Start(member.Id);
				}
				if (!View.Selected)
				{
					System.Drawing.Image image = enumerable.FirstOrDefault().Value?.Image;
					_tabProxy.UpdateTwinkleImage(image);
					_tabProxy.Start();
				}
			}
		});
	}

	public bool Exists(string userId)
	{
		return flickerManager.Contains(userId);
	}

	public void AppendCollegue(Member member)
	{
		ribbonGroupTeam.Items.Add(CreateMemberButton(member));
	}

	public void StartFlicker(string userId)
	{
		RibbonItem ribbonItem = FindMemberById(userId);
		if (ribbonItem == null || !ribbonItem.Visible)
		{
			StopFlicker(userId);
			return;
		}
		flickerManager.Start(userId);
		if (!View.Selected)
		{
			System.Drawing.Image image = MemberManager.GetInstance().GetMember(userId)?.Image;
			_tabProxy.UpdateTwinkleImage(image);
			_tabProxy.Start();
		}
	}

	public void StopFlicker(string userId)
	{
		flickerManager.Stop(userId);
		if (!flickerManager.Any((AbstractFlickerProxy t) => t.Status()) && !systemMessageFlickerManager.Any((AbstractFlickerProxy t) => t.Status()))
		{
			_tabProxy.Stop();
		}
	}

	public void HandleSelectChange(string memId)
	{
		MemberManager instance = MemberManager.GetInstance();
		MemTab memTab = instance.GetMember(memId);
		if (memTab == null)
		{
			memTab = instance.GetGroup(memId);
		}
		if (memTab != null && memTab.UnhandleActionMessage.Count <= 0)
		{
			StopFlicker(memId);
		}
	}

	private void PopulatePorjectMembers()
	{
		ribbonGroupProject.Items.Clear();
		MemberManager instance = MemberManager.GetInstance();
		Group group = instance.GetGroup(SignalRClient.UserState.ProjectId);
		if (group == null)
		{
			return;
		}
		ribbonGroupProject.Items.Add(CreateMemberButton(group));
		IEnumerable<Member> source = group.Members();
		if (source.Count() > 0)
		{
			ribbonGroupProject.Items.Add(new RibbonSeparator());
		}
		foreach (Member item in from m in source
			orderby !m.IsOnline, m.Id
			select m)
		{
			ribbonGroupProject.Items.Add(CreateMemberButton(item));
		}
	}

	private void PopulateTeamMembers()
	{
		ribbonGroupTeam.Items.Clear();
		MemberManager instance = MemberManager.GetInstance();
		Group projectGroup = instance.GetGroup(SignalRClient.UserState.ProjectId);
		Group group = instance.GetGroup(Leqisoft.Model.User.Current.TeamId.ToString());
		if (group == null)
		{
			return;
		}
		ribbonGroupTeam.Items.Add(CreateMemberButton(group));
		IOrderedEnumerable<Member> orderedEnumerable = from m in @group.Members()
			where m.Id != Leqisoft.Model.User.Current.Id.ToString()
			where !(projectGroup?.Exists(m.Id) ?? false)
			where m.UnhandleNotifyMessage.Count > 0
			orderby !m.IsOnline, m.Id
			select m;
		if (orderedEnumerable.Count() > 0)
		{
			ribbonGroupTeam.Items.Add(new RibbonSeparator());
		}
		foreach (Member item in orderedEnumerable)
		{
			ribbonGroupTeam.Items.Add(CreateMemberButton(item));
		}
	}

	private RibbonButton CreateMemberButton(MemTab member)
	{
		RibbonButton ribbonButton = new RibbonButton();
		ribbonButton.Tag = member;
		ribbonButton.Text = preName(member.Name);
		ribbonButton.Enabled = member is Group || (member as Member).IsOnline;
		ribbonButton.TextImageRelation = C1.Win.C1Ribbon.TextImageRelation.ImageAboveText;
		ribbonButton.LargeImage = ((member is Group || (member as Member).IsOnline) ? member.Image : member.GrayImage);
		RibbonButton ribbonButton2 = ribbonButton;
		RibbonLargeFlickerProxy ribbonLargeFlickerProxy = new RibbonLargeFlickerProxy(ribbonButton2);
		ribbonLargeFlickerProxy.UpdateEmptyImage(Resources.Empty32);
		ribbonLargeFlickerProxy.SetTimer(SecondTrigger.Trigger);
		flickerManager.Add(member.Id, ribbonLargeFlickerProxy);
		ribbonButton2.Click += delegate
		{
			ribbonButton_Click(member);
		};
		return ribbonButton2;
		static string preName(string name)
		{
			if (name.Length < 8)
			{
				int totalWidth = (int)Math.Ceiling((double)(8 - name.Length) / 2.0);
				name = " ".PadLeft(totalWidth) + name + " ".PadRight(totalWidth);
			}
			return name;
		}
		void ribbonButton_Click(MemTab mem)
		{
			if (!(mem.Id == Leqisoft.Model.User.Current.Id.ToString()))
			{
				if (mem.UnhandleActionMessage.Count > 0)
				{
					IActionMessage actionMessage = mem.UnhandleActionMessage.Dequeue();
					if (actionMessage != null)
					{
						actionMessage.ActionDeal(actionMessage.Content);
						if (mem.UnhandleActionMessage.Count <= 0 && mem.UnhandleNotifyMessage.Count <= 0)
						{
							flickerManager.Stop(mem.Id);
						}
						return;
					}
				}
				ChatManager.ChatForm.Value.ChatWith(mem.Id, donotSetTab: false);
				flickerManager.Stop(mem.Id);
			}
		}
	}

	private RibbonItem FindMemberById(string userId)
	{
		foreach (RibbonGroup group in View.Groups)
		{
			foreach (RibbonItem item in group.Items)
			{
				if (item.Tag is MemTab memTab && memTab.Id == userId)
				{
					return item;
				}
			}
		}
		return null;
	}

	private void View_Select(object sender, EventArgs e)
	{
		_tabProxy.Stop();
	}

	private void View_Deselect(object sender, EventArgs e)
	{
		if (flickerManager.Any((AbstractFlickerProxy t) => t.Status()) || systemMessageFlickerManager.Any((AbstractFlickerProxy t) => t.Status()))
		{
			_tabProxy.Start();
		}
		else
		{
			_tabProxy.Stop();
		}
	}

	private void View_MouseEnter(object sender, EventArgs e)
	{
		_tabProxy.Stop();
	}

	private void View_MouseLeave(object sender, EventArgs e)
	{
		if (!View.Selected && (flickerManager.Any((AbstractFlickerProxy p) => p.Status()) || systemMessageFlickerManager.Any((AbstractFlickerProxy p) => p.Status())))
		{
			_tabProxy.Start();
		}
	}

	private void TabMember_OnlineChanged(object sender, long e)
	{
		Populate();
	}
}
