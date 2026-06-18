﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;
using Leqisoft.LocalDataStore;

namespace Leqisoft.UI.Platform;

public static class ChatManager
{
	public static BulletLauncher BulletLauncher;

	public static Lazy<ChatForm> ChatForm;

	private static object lock1;

	public static event EventHandler<Bitmap> AfterHeadChanged;

	private static void Chatform_SelectedMemberChanged(object sender, string e)
	{
		Program.MainForm.TabMember.HandleSelectChange(e);
	}

	public static void OnAfterHeadChanged(Bitmap image)
	{
		ChatManager.AfterHeadChanged?.Invoke(null, image);
	}

	static ChatManager()
	{
		ChatForm = new Lazy<ChatForm>(delegate
		{
			ChatForm chatForm = new ChatForm();
			chatForm.SelectedMemberChanged += Chatform_SelectedMemberChanged;
			return chatForm;
		});
		lock1 = new object();
		AfterHeadChanged += ChatManager_AfterHeadChanged;
	}

	private static void ChatManager_AfterHeadChanged(object sender, Bitmap bitmap)
	{
		if (bitmap != null)
		{
			MemberManager instance = MemberManager.GetInstance();
			Member member = instance.GetMember(Leqisoft.Model.User.Current.Id.ToString());
			member?.SetPicture(bitmap);
			if (ChatForm.IsValueCreated)
			{
				ChatForm.Value.UpdatePersonalInfo(member.Name, member.Image);
			}
		}
	}

	public static async Task Login()
	{
		if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
			return;
		await SignalRClient.Start();
		SignalRClient.Logined += LoginedCallback;
		SignalRClient.UserState.UserId = Leqisoft.Model.User.Current.Id.ToString();
		SignalRClient.UserState.TeamId = Leqisoft.Model.User.Current.TeamId.ToString();
		await SignalRClient.Login(Leqisoft.Model.User.Current.Id.ToString(), SignalRClient.UserState);
		static void LoginedCallback(object sender, LoginEventArgs e)
		{
			MemberManager.GetInstance().UpdateUserState(e.OnlineUsers);
			SignalRClient.Logined -= LoginedCallback;
		}
	}

	public static async Task UpdateTeamMember()
	{
		MemberManager memberManager = MemberManager.GetInstance();
		try
		{
			IEnumerable<Leqisoft.DTO.User> enumerable;
			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				enumerable = await Leqisoft.LocalDataStore.StorageRouter.GetTeamUsersWithPic();
			else
				enumerable = await WebApiClient.GetTeamUsersWithPic();
			Guid teamId = enumerable.First().TeamId;
			lock (lock1)
			{
				Group group = memberManager.GetGroup(teamId.ToString());
				if (group == null)
				{
					group = new Group
					{
						Id = teamId.ToString(),
						Name = "全体同事"
					};
					group.SetPicture(Resources.group);
					memberManager.AddGroup(group);
				}
				else
				{
					group.Id = teamId.ToString();
					group.Clear();
					memberManager.AddGroup(group);
				}
				if (!enumerable.Any((Leqisoft.DTO.User u) => u.Id == Leqisoft.Model.User.Current.Id))
				{
					Leqisoft.Model.User.Current.TeamId = Guid.Empty;
					Leqisoft.Model.User.Current.IsTeamAdmin = false;
					Leqisoft.Model.User.Current.LicenseDate = DateTime.MaxValue;
					return;
				}
				foreach (Leqisoft.DTO.User item in enumerable)
				{
					Member member = memberManager.GetMember(item.Id.ToString());
					if (member == null)
					{
						member = new Member
						{
							Id = item.Id.ToString()
						};
						memberManager.AddMember(member);
					}
					member.Name = item.Name;
					member.TeamId = item.TeamId;
					member.Sex = item.Sex != "f";
					member.SetPicture(item.Picture);
					group.Add(member);
				}
			}
			MemberManager memberManager2 = memberManager;
			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				memberManager2.UpdateUserState(Enumerable.Empty<UserState>());
			else
				memberManager2.UpdateUserState(await SignalRClient.QueryOnlineTeam(teamId.ToString()));
		}
		catch (HttpRequestException exception)
		{
			exception.Log();
		}
		catch (TimeoutException exception2)
		{
			exception2.Log();
		}
		catch (Exception ex)
		{
			Group group2 = memberManager.GetGroup(Leqisoft.Model.User.Current.TeamId.ToString());
			if (group2 == null)
			{
				group2 = new Group
				{
					Id = Leqisoft.Model.User.Current.TeamId.ToString(),
					Name = "全体同事"
				};
				group2.SetPicture(Resources.group);
				memberManager.AddGroup(group2);
			}
			else
			{
				group2.Id = Leqisoft.Model.User.Current.TeamId.ToString();
				group2.Clear();
				memberManager.AddGroup(group2);
			}
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			ex.Log();
		}
	}

	public static async Task UpdateProjectMember(Guid projectId)
	{
		MemberManager memberManager = MemberManager.GetInstance();
		try
		{
			IEnumerable<Leqisoft.DTO.User> enumerable;
			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				enumerable = await Leqisoft.LocalDataStore.StorageRouter.GetTeamUsersWithPic();
			else
				enumerable = await WebApiClient.GetProjectUsersWithPic(projectId);
			SignalRClient.UserState.ProjectId = projectId.ToString();
			lock (lock1)
			{
				Group group = memberManager.GetGroup(projectId.ToString());
				if (group == null)
				{
					group = new Group
					{
						Id = projectId.ToString(),
						Name = "全体成员"
					};
					group.SetPicture(Resources.member);
					memberManager.AddGroup(group);
				}
				else
				{
					group.Id = projectId.ToString();
					group.Clear();
					memberManager.AddGroup(group);
				}
				if (!enumerable.Any((Leqisoft.DTO.User u) => u.Id == Leqisoft.Model.User.Current.Id))
				{
					return;
				}
				foreach (Leqisoft.DTO.User item in enumerable)
				{
					Member member = memberManager.GetMember(item.Id.ToString());
					if (member == null)
					{
						member = new Member
						{
							Id = item.Id.ToString()
						};
						memberManager.AddMember(member);
					}
					member.Name = item.Name;
					member.Role = item.Role;
					member.TeamId = item.TeamId;
					member.Sex = item.Sex != "f";
					member.SetPicture(item.Picture);
					group.Add(member);
				}
			}
			MemberManager memberManager2 = memberManager;
			if (Leqisoft.LocalDataStore.StorageRouter.IsLocalMode)
				memberManager2.UpdateUserState(Enumerable.Empty<UserState>());
			else
				memberManager2.UpdateUserState(await SignalRClient.QueryOnlineProject(projectId.ToString()));
		}
		catch (HttpRequestException exception)
		{
			exception.Log();
		}
		catch (TimeoutException exception2)
		{
			exception2.Log();
		}
		catch (NormalException exception3)
		{
			Group group2 = memberManager.GetGroup(projectId.ToString());
			if (group2 == null)
			{
				group2 = new Group
				{
					Id = projectId.ToString(),
					Name = "全体成员"
				};
				group2.SetPicture(Resources.member);
				memberManager.AddGroup(group2);
			}
			else
			{
				group2.Id = projectId.ToString();
				group2.Clear();
				memberManager.AddGroup(group2);
			}
			exception3.Log();
		}
		catch (Exception ex)
		{
			Group group3 = memberManager.GetGroup(projectId.ToString());
			if (group3 == null)
			{
				group3 = new Group
				{
					Id = projectId.ToString(),
					Name = "全体成员"
				};
				group3.SetPicture(Resources.member);
				memberManager.AddGroup(group3);
			}
			else
			{
				group3.Id = projectId.ToString();
				group3.Clear();
				memberManager.AddGroup(group3);
			}
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			ex.Log();
		}
	}
}
