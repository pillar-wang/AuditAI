using System;
using System.Collections.Generic;
using FileTransferModel;
using Leqisoft.DTO;
using Leqisoft.SignalR;

namespace Leqisoft.UI.Platform;

public class MemberManager
{
	private delegate void InvokeDelegate();

	private static MemberManager _instance = new MemberManager();

	private Dictionary<string, Group> _groupMap = new Dictionary<string, Group>();

	private Dictionary<string, Member> _memberMap = new Dictionary<string, Member>();

	public event EventHandler<long> StatusUploaded;

	public event EventHandler<long> OnlineStatusChanged;

	public event EventHandler<long> OpenProjectChanged;

	public event EventHandler<long> OpenTreeNodeChanged;

	public event EventHandler<long> TableCellChanged;

	public event EventHandler<long> DocParagraphChanged;

	public event EventHandler<string> ProjectSynced;

	public event EventHandler<string> MemberInfoChanged;

	public event EventHandler<string> TeamMemberChanged;

	public event EventHandler<string> ProjectMemberChanged;

	public event EventHandler<Tuple<string, string, NotifyMessage>> MessageArrived;

	public event EventHandler<Tuple<long, long, string, string, string>> PushTreeNode;

	public event EventHandler<Tuple<Member, NotifyMessage>> RecieveFileMesssage;

	public event EventHandler RePopulate;

	public event EventHandler<FileSection> AfterSendSection;

	public event EventHandler<FileCache> AfterSendComplete;

	public event EventHandler<string> AfterSendCancel;

	public event EventHandler<string> AfterRecieveCancel;

	public event EventHandler<Tuple<string, string, string, string>> OpenTicketNavTreeNodeChanged;

	public static MemberManager GetInstance()
	{
		return _instance;
	}

	public void OnSendSection(FileSection fileSection)
	{
		this.AfterSendSection?.Invoke(this, fileSection);
	}

	public void OnSendComplete(FileCache fileCache)
	{
		this.AfterSendComplete(this, fileCache);
	}

	public void OnSendCancel(string fileId)
	{
		this.AfterSendCancel?.Invoke(this, fileId);
	}

	public void OnRecieveCancel(string fileId)
	{
		this.AfterRecieveCancel(this, fileId);
	}

	public void OnOnlineStatusChanged(long userId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.OnlineStatusChanged?.Invoke(this, userId);
			});
		}
	}

	public void OnOpenProjectChanged(long userId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.OpenProjectChanged?.Invoke(this, userId);
			});
		}
	}

	public void OnOpenTreeNodeChanged(long userId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.OpenTreeNodeChanged?.Invoke(this, userId);
			});
		}
	}

	public void OnMessageArrived(string fromId, string memId, NotifyMessage message)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.MessageArrived?.Invoke(this, Tuple.Create(fromId, memId, message));
			});
		}
	}

	public void OnTableCellChanged(long userId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.TableCellChanged?.Invoke(this, userId);
			});
		}
	}

	public void OnDocParagraphChanged(long userId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.DocParagraphChanged?.Invoke(this, userId);
			});
		}
	}

	public void OnStatusUploaded(long userId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.StatusUploaded?.Invoke(this, userId);
			});
		}
	}

	public void OnProjectSynced(string projectId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.ProjectSynced?.Invoke(this, projectId);
			});
		}
	}

	public void OnTeamMemberChanged()
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.TeamMemberChanged?.Invoke(this, null);
			});
		}
	}

	public void OnProjectMembersChanged(string projectId)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.ProjectMemberChanged?.Invoke(this, projectId);
			});
		}
	}

	public void OnMemberInfoChanged()
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.MemberInfoChanged?.Invoke(this, null);
			});
		}
	}

	public void OnPushTreeNode(long fromId, long nodeId, string nodeName, string fromName, string message)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.PushTreeNode?.Invoke(this, Tuple.Create(fromId, nodeId, nodeName, fromName, message));
			});
		}
	}

	public void OnRecieveFileMessage(Member member, NotifyMessage notifyMessage)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.RecieveFileMesssage?.Invoke(this, Tuple.Create(member, notifyMessage));
			});
		}
	}

	public void OnOpenTicketNavTreeNodeChanged(string userId, string projectId, string tableId, string treeNodePath)
	{
		MainForm mainForm = Program.MainForm;
		if (mainForm != null && mainForm.View.IsHandleCreated)
		{
			Program.MainForm.View.Invoke((InvokeDelegate)delegate
			{
				this.OpenTicketNavTreeNodeChanged?.Invoke(this, Tuple.Create(userId, projectId, tableId, treeNodePath));
			});
		}
	}

	public void OnRepopulate()
	{
		this.RePopulate?.Invoke(this, EventArgs.Empty);
	}

	private MemberManager()
	{
	}

	public void AddGroup(Group group)
	{
		if (_groupMap.ContainsKey(group.Id))
		{
			_groupMap.Remove(group.Id);
		}
		_groupMap.Add(group.Id, group);
	}

	public Group GetGroup(string groupId)
	{
		if (groupId == null)
		{
			return null;
		}
		if (_groupMap.ContainsKey(groupId))
		{
			return _groupMap[groupId];
		}
		return null;
	}

	public IEnumerable<Group> GetGroups()
	{
		return _groupMap.Values;
	}

	public void AddMember(Member member)
	{
		_memberMap.Add(member.Id, member);
	}

	public Member GetMember(string userId)
	{
		if (userId == null)
		{
			return null;
		}
		if (_memberMap.ContainsKey(userId))
		{
			return _memberMap[userId];
		}
		return null;
	}

	public IEnumerable<Member> GetMembers()
	{
		return _memberMap.Values;
	}

	public void UpdateUserState(IEnumerable<UserState> userStates)
	{
		if (userStates == null)
		{
			return;
		}
		foreach (UserState userState in userStates)
		{
			if (userState.UserId != null && _memberMap.ContainsKey(userState.UserId))
			{
				_memberMap[userState.UserId].UserState = userState;
			}
		}
	}
}
