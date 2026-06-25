using System;
using System.Collections.Generic;
using System.Drawing;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.SignalR;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class ProjectHierarchyDecorator
{
	protected class TreeNodeFlickerCollection
	{
		protected LinkedList<FlickNodeData> _flickList = new LinkedList<FlickNodeData>();

		protected Dictionary<string, LinkedListNode<FlickNodeData>> _userFlickerDic = new Dictionary<string, LinkedListNode<FlickNodeData>>();

		protected ProjectHierarchyDecorator _owner;

		public const int EveryFrameFilckeBuildMaxCount = 10;

		public const int MaxFlickerCount = 100;

		public TreeNodeFlickerCollection(ProjectHierarchyDecorator owner)
		{
			_owner = owner;
		}

		public void Clear()
		{
			_owner._flickerBuildQueue.Clear();
			_userFlickerDic.Clear();
			for (LinkedListNode<FlickNodeData> linkedListNode = _flickList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				if (!linkedListNode.Value._isDelete && linkedListNode.Value._flicker != null)
				{
					linkedListNode.Value._flicker.RevertToOrignImage();
				}
			}
			_flickList.Clear();
		}

		public void Remove(string userId)
		{
			if (_userFlickerDic.TryGetValue(userId, out var value))
			{
				_userFlickerDic.Remove(userId);
				value.Value._isDelete = true;
				if (value.Value._flicker != null)
				{
					value.Value._flicker.RevertToOrignImage();
				}
			}
		}

		public void Add(string userId, GridRowFlicker flicker)
		{
			if (_userFlickerDic.TryGetValue(userId, out var value))
			{
				if (!value.Value._isDelete && value.Value._flicker != null)
				{
					value.Value._flicker.RevertToOrignImage();
				}
				value.Value._isDelete = true;
				value.Value._flicker = null;
				_userFlickerDic.Remove(userId);
			}
			FlickNodeData flickNodeData = new FlickNodeData();
			flickNodeData._flicker = flicker;
			LinkedListNode<FlickNodeData> value2 = _flickList.AddLast(flickNodeData);
			_userFlickerDic.Add(userId, value2);
			if (flicker != null)
			{
				flicker.FlickData = flickNodeData;
			}
		}

		public void DoFlickUpdate()
		{
			if (Program.MainForm.IsInSyncingProject || _owner.IsPaused)
			{
				return;
			}
			if (_owner._flickerBuildQueue.Count > 0)
			{
				int num = 0;
				while (_owner._flickerBuildQueue.Count > 0 && num < 10)
				{
					FlickerBuildData flickerBuildData = _owner._flickerBuildQueue.Dequeue();
					num += _owner.HandleOpenNavTreeNode(flickerBuildData.UserId);
				}
			}
			if (_flickList.First == null)
			{
				return;
			}
			LinkedListNode<FlickNodeData> linkedListNode = _flickList.First;
			HashSet<object> hashSet = new HashSet<object>();
			while (linkedListNode != null)
			{
				FlickNodeData value = linkedListNode.Value;
				if (value._isDelete || value._flicker == null)
				{
					LinkedListNode<FlickNodeData> node = linkedListNode;
					linkedListNode = linkedListNode.Next;
					_flickList.Remove(node);
					continue;
				}
				value._flicker.ShowFlickImage(hashSet);
				if (hashSet.Count < 100)
				{
					linkedListNode = linkedListNode.Next;
					continue;
				}
				break;
			}
		}
	}

	protected class GridRowFlicker
	{
		protected bool _isDisposed;

		protected System.Drawing.Image _flickImage;

		public Member Member;

		public FlickNodeData FlickData;

		public ProjectHierarchy.TreeGroupView TreeGroupView;

		public C1OutPage OutPage;

		public C1.Win.C1FlexGrid.Row Row;

		protected object _lastModifyObj;

		protected System.Drawing.Image _lastModifyOrignImage;

		public void SetFlickImage(System.Drawing.Image image)
		{
			_flickImage = image;
		}

		public void RevertToOrignImage()
		{
			RevertLastModifyRowOrignImage();
		}

		private void RevertLastModifyRowOrignImage()
		{
			if (_lastModifyObj == null)
			{
				return;
			}
			try
			{
				if (_lastModifyObj is C1.Win.C1FlexGrid.Row row)
				{
					row.Grid.SetCellImage(row.Index, 0, _lastModifyOrignImage);
				}
				else if (_lastModifyObj is C1OutPage c1OutPage)
				{
					c1OutPage.Image = _lastModifyOrignImage;
				}
			}
			catch
			{
			}
			finally
			{
				_lastModifyObj = null;
				_lastModifyOrignImage = null;
			}
		}

		public void ShowFlickImage(HashSet<object> hasUpdatedTargetSet)
		{
			try
			{
				if (_isDisposed)
				{
					return;
				}
				if (Member.UserState == null)
				{
					RevertToOrignImage();
					FlickData._isDelete = true;
					return;
				}
				object obj = null;
				if (Program.MainForm.ProjectHierarchy._currentGroup != TreeGroupView)
				{
					obj = OutPage;
				}
				else
				{
					Node node = Row.Node;
					while (node != null && !node.Row.IsVisible)
					{
						node = node.Parent;
					}
					if (node == null)
					{
						obj = OutPage;
						return;
					}
					obj = node.Row;
				}
				if (obj != _lastModifyObj)
				{
					RevertLastModifyRowOrignImage();
					_lastModifyObj = obj;
					if (_lastModifyObj is C1OutPage)
					{
						_lastModifyOrignImage = Resources.chatgroup;
					}
					else if (_lastModifyObj is C1.Win.C1FlexGrid.Row row)
					{
						_lastModifyOrignImage = TreeGroupView.GetTreeNodeIcon(row.UserData as TreeNodeBase);
					}
					else
					{
						_lastModifyOrignImage = null;
					}
				}
				if (hasUpdatedTargetSet.Contains(_lastModifyObj))
				{
					return;
				}
				System.Drawing.Image image = null;
				image = ((!SecondTrigger.Display) ? Resources.Empty16 : _flickImage);
				if (obj is C1OutPage c1OutPage2)
				{
					c1OutPage2.Image = image;
				}
				else
				{
					if (!(obj is C1.Win.C1FlexGrid.Row row2))
					{
						return;
					}
					row2.Grid.SetCellImage(row2.Index, 0, image);
				}
				hasUpdatedTargetSet.Add(_lastModifyObj);
			}
			catch
			{
				_isDisposed = true;
			}
		}
	}

	protected class FlickNodeData
	{
		public GridRowFlicker _flicker;

		public bool _isDelete;
	}

	protected class FlickerBuildData
	{
		public string UserId;

		public string ProjectId;

		public string TableId;

		public string TreeNodePath;
	}

	protected ProjectHierarchy _projectHierarchy;

	protected TreeNodeFlickerCollection _treeNodeFlickerCollection;

	protected Queue<FlickerBuildData> _flickerBuildQueue = new Queue<FlickerBuildData>();

	protected bool _isPaused;

	protected object _pauseLockObject = new object();

	public bool IsPaused
	{
		get
		{
			lock (_pauseLockObject)
			{
				return _isPaused;
			}
		}
		set
		{
			lock (_pauseLockObject)
			{
				_isPaused = value;
			}
		}
	}

	public ProjectHierarchyDecorator(ProjectHierarchy owner)
	{
		_projectHierarchy = owner;
		_treeNodeFlickerCollection = new TreeNodeFlickerCollection(this);
		SecondTrigger.Trigger.Tick += Trigger_Tick;
	}

	private void Trigger_Tick(object sender, EventArgs e)
	{
		DoFlickUpdate();
	}

	protected void DoFlickUpdate()
	{
		try
		{
			_treeNodeFlickerCollection.DoFlickUpdate();
		}
		catch
		{
		}
	}

	public void RefreshNavTreeNodeFlickImage()
	{
		DoFlickUpdate();
	}

	public void ReBuildNavTreeFlicker()
	{
		try
		{
			_treeNodeFlickerCollection.Clear();
			MemberManager instance = MemberManager.GetInstance();
			IEnumerable<MemTab> enumerable = instance.GetGroup(SignalRClient.UserState.ProjectId)?.GetSelfAndMembers();
			if (enumerable == null)
			{
				return;
			}
			foreach (MemTab item in enumerable)
			{
				if (item is Member { UserState: { TreeNodeId: not null } })
				{
					HandleOpenNavTreeNodeDelay(item.Id);
				}
			}
			_treeNodeFlickerCollection.DoFlickUpdate();
		}
		catch (Exception exception)
		{
			exception.Log("重构文件导航树的用户头像闪烁处理时发生了未预期的异常");
		}
	}

	public void HandleOpenNavTreeNodeDelay(string userId)
	{
		_flickerBuildQueue.Enqueue(new FlickerBuildData
		{
			UserId = userId
		});
	}

	public int HandleOpenNavTreeNode(string userId)
	{
		string text = User.Current.Id.ToString();
		Group group = MemberManager.GetInstance().GetGroup(SignalRClient.UserState.ProjectId);
		if (group == null || !group.Exists(text))
		{
			_treeNodeFlickerCollection.Clear();
			return 0;
		}
		if (userId == text)
		{
			return 0;
		}
		if (group == null || !group.Exists(userId))
		{
			_treeNodeFlickerCollection.Remove(userId);
			return 0;
		}
		Member member = MemberManager.GetInstance().GetMember(userId);
		if (member == null)
		{
			_treeNodeFlickerCollection.Remove(userId);
			return 0;
		}
		string text2 = Project.Current.Id.ToString();
		UserState userState = member.UserState;
		if (userState == null || userState.ProjectId != text2)
		{
			_treeNodeFlickerCollection.Remove(userId);
			return 0;
		}
		if (!long.TryParse(userState.TreeNodeId, out var nId))
		{
			_treeNodeFlickerCollection.Remove(userId);
			return 0;
		}
		C1.Win.C1FlexGrid.Row row = _projectHierarchy.FindNode(null);
		if (row == null)
		{
			_treeNodeFlickerCollection.Remove(userId);
			return 0;
		}
		Node node = row.Node;
		ProjectHierarchy.TreeGroupView treeGroupView = _projectHierarchy.GetTreeGroupView(row.Grid);
		if (treeGroupView == null)
		{
			_treeNodeFlickerCollection.Remove(userId);
			return 0;
		}
		try
		{
			GridRowFlicker gridRowFlicker = new GridRowFlicker();
			gridRowFlicker.Member = member;
			gridRowFlicker.TreeGroupView = treeGroupView;
			gridRowFlicker.OutPage = (C1.Win.C1Command.C1OutPage)treeGroupView.Page;
			gridRowFlicker.Row = row;
			gridRowFlicker.SetFlickImage(member.GetOrGenerateImage16());
			_treeNodeFlickerCollection.Add(userId, gridRowFlicker);
		}
		catch
		{
		}
		return 1;
	}
}
