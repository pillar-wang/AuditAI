using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.SignalR;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class TicketGridDecorator
{
	public class DrawCollection
	{
		public Dictionary<long, System.Drawing.Image> Values { get; private set; }

		public System.Drawing.Image this[long cellId]
		{
			get
			{
				if (Values.TryGetValue(cellId, out var value))
				{
					return value;
				}
				return null;
			}
		}

		public bool IsEmpty => Values.Count == 0;

		public DrawCollection()
		{
			Values = new Dictionary<long, System.Drawing.Image>();
		}

		public void Add(long cellId, System.Drawing.Image image)
		{
			if (!Values.ContainsKey(cellId))
			{
				Values.Add(cellId, image);
			}
		}

		public bool ContainsCellId(long cellId)
		{
			return Values.ContainsKey(cellId);
		}

		public void Clear()
		{
			Values.Clear();
		}
	}

	protected class NavTreeNodeFlickerCollection
	{
		protected LinkedList<FlickNodeData> _flickList = new LinkedList<FlickNodeData>();

		protected Dictionary<string, LinkedListNode<FlickNodeData>> _userFlickerDic = new Dictionary<string, LinkedListNode<FlickNodeData>>();

		protected TicketGridDecorator _owner;

		public const int EveryFrameFilckeBuildMaxCount = 10;

		public const int MaxFlickerCount = 50;

		public NavTreeNodeFlickerCollection(TicketGridDecorator owner)
		{
			_owner = owner;
		}

		public void Clear()
		{
			_owner._navTreeNodeFlickerBuildQueue.Clear();
			_userFlickerDic.Clear();
			_flickList.Clear();
		}

		public void DoFlickUpdate()
		{
			if (_owner._navTreeNodeFlickerBuildQueue.Count > 0)
			{
				int num = 0;
				Dictionary<TicketNavGrid, Dictionary<string, List<C1.Win.C1FlexGrid.Row>>> rowNodeOpenPathCacheDic = new Dictionary<TicketNavGrid, Dictionary<string, List<C1.Win.C1FlexGrid.Row>>>();
				while (_owner._navTreeNodeFlickerBuildQueue.Count > 0 && num < 10)
				{
					FlickerBuildData flickerBuildData = _owner._navTreeNodeFlickerBuildQueue.Dequeue();
					num += _owner.HandleOpenNavTreeNode(flickerBuildData.UserId, rowNodeOpenPathCacheDic);
				}
			}
			if (_flickList.First == null || !_owner._navTreeOutBar.Visible)
			{
				return;
			}
			C1FlexGridEx currentInShowingNavTreeGrid = _owner._ticketInputEditor.GetCurrentInShowingNavTreeGrid();
			if (currentInShowingNavTreeGrid == null || _owner._ticketInputEditor.Table == null)
			{
				return;
			}
			long value = _owner._ticketInputEditor.Table.Id.Value;
			LinkedListNode<FlickNodeData> linkedListNode = _flickList.First;
			HashSet<object> hashSet = new HashSet<object>();
			while (linkedListNode != null)
			{
				FlickNodeData value2 = linkedListNode.Value;
				if (value2._isDelete || value2._flicker == null)
				{
					LinkedListNode<FlickNodeData> node = linkedListNode;
					linkedListNode = linkedListNode.Next;
					_flickList.Remove(node);
					continue;
				}
				if (value2._tableId != value)
				{
					linkedListNode = linkedListNode.Next;
					continue;
				}
				value2._flicker.ShowFlickImage(hashSet);
				if (hashSet.Count < 50)
				{
					linkedListNode = linkedListNode.Next;
					continue;
				}
				break;
			}
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

		public void Add(string userId, string projectId, long tableId, GridRowFlicker flicker)
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
			flickNodeData._tableId = tableId;
			LinkedListNode<FlickNodeData> value2 = _flickList.AddLast(flickNodeData);
			_userFlickerDic.Add(userId, value2);
			if (flicker != null)
			{
				flicker.FlickData = flickNodeData;
			}
		}
	}

	protected class FlickNodeData
	{
		public GridRowFlicker _flicker;

		public bool _isDelete;

		public long _tableId;
	}

	protected class GridRowFlicker
	{
		protected C1.Win.C1FlexGrid.Row _row;

		protected bool _isDisposed;

		protected object _lastModifyObject;

		protected System.Drawing.Image _lastModifyOrignImage;

		protected System.Drawing.Image _flickImage;

		public Member Member;

		public FlickNodeData FlickData;

		public TicketNavGrid NavTreeGrid;

		public C1OutPage OutPage;

		public GridRowFlicker(C1.Win.C1FlexGrid.Row gridRow)
		{
			_row = gridRow;
		}

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
			if (_lastModifyObject == null)
			{
				return;
			}
			try
			{
				if (_lastModifyObject is C1.Win.C1FlexGrid.Row row)
				{
					row.Grid.SetCellImage(row.Index, 0, null);
				}
				else if (_lastModifyObject is C1OutPage c1OutPage)
				{
					c1OutPage.Image = _lastModifyOrignImage;
				}
			}
			catch
			{
			}
			finally
			{
				_lastModifyObject = null;
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
				if (Program.MainForm.TicketInputEditor.GetCurrentInShowingNavTree() != NavTreeGrid)
				{
					obj = OutPage;
				}
				else
				{
					Node node = _row.Node;
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
				if (obj != _lastModifyObject)
				{
					RevertLastModifyRowOrignImage();
					_lastModifyObject = obj;
					if (_lastModifyObject is C1OutPage)
					{
						_lastModifyOrignImage = Resources.TicketNav;
					}
					else if (_lastModifyObject is C1.Win.C1FlexGrid.Row)
					{
						_lastModifyOrignImage = Resources.Ticket16;
					}
					else
					{
						_lastModifyOrignImage = null;
					}
				}
				if (hasUpdatedTargetSet.Contains(_lastModifyObject))
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
				hasUpdatedTargetSet.Add(_lastModifyObject);
			}
			catch
			{
				_isDisposed = true;
			}
		}
	}

	protected class FlickerBuildData
	{
		public string UserId;

		public string ProjectId;

		public string TableId;

		public string TreeNodePath;
	}

	private readonly C1FlexGridEx _bodyGrid;

	private readonly C1FlexGridEx _titleGrid;

	private readonly C1FlexGridEx _footerGrid;

	private bool _isCellDataDirty = true;

	private TicketInputEditor2 _ticketInputEditor;

	private TicketInputTitleFooterEditor _titleEditor;

	private TicketInputTitleFooterEditor _footerEditor;

	private NavTreeNodeFlickerCollection _ticketNavTreeNodeFlickerCollection;

	private C1OutBarEx _navTreeOutBar;

	private Queue<FlickerBuildData> _navTreeNodeFlickerBuildQueue = new Queue<FlickerBuildData>();

	public const int USER_HEADER_ICON_MAX_COUNT = 10;

	public const int REPAINT_USER_HEADER_ICON_MAX_CELL_COUNT = 5000;

	public const int NAV_GRID_MAX_SEARCH_ROWS_ON_FIND_BY_OPEN_PATH = 10000;

	private readonly DrawCollection _cellDrawCollection = new DrawCollection();

	private bool _oldCollectionIsEmpty;

	public bool Enable { get; set; } = true;


	public TicketGridDecorator(TicketInputEditor2 ticketInputEditor)
	{
		_ticketNavTreeNodeFlickerCollection = new NavTreeNodeFlickerCollection(this);
		_ticketInputEditor = ticketInputEditor;
		_titleEditor = ticketInputEditor.TitleEditor;
		_footerEditor = ticketInputEditor.FooterEditor;
		_bodyGrid = ticketInputEditor.Grid;
		_titleGrid = ticketInputEditor.TitleEditor.View;
		_footerGrid = ticketInputEditor.FooterEditor.View;
		_navTreeOutBar = ticketInputEditor.NavTreeOutBar;
		SecondTrigger.Trigger.Tick += TicketCellFlickUpdate;
		SecondTrigger.Trigger.Tick += TicketNavTreeNodeFlickUpdate;
	}

	public void SetCellFlickerDirty()
	{
		_isCellDataDirty = true;
	}

	protected void ReBuildCellFlicker()
	{
		if (!_isCellDataDirty)
		{
			return;
		}
		_cellDrawCollection.Clear();
		if (_ticketInputEditor.Table == null)
		{
			return;
		}
		TicketNavGrid currentInShowingNavTree = _ticketInputEditor.GetCurrentInShowingNavTree();
		if (currentInShowingNavTree == null)
		{
			return;
		}
		TicketInputTableVM vMData = _ticketInputEditor.VMData;
		if (vMData == null || vMData.GetCellsCount() > 5000)
		{
			return;
		}
		UserState myState = SignalRClient.UserState;
		MemberManager instance = MemberManager.GetInstance();
		List<Member> list = (from m in instance.GetGroup(SignalRClient.UserState.ProjectId)?.Members()
			where m.UserState != null && m.UserState.ProjectId == myState.ProjectId && m.UserState.TreeNodeId == myState.TreeNodeId
			select m).ToList();
		if (list == null)
		{
			return;
		}
		string text = User.Current.Id.ToString();
		Table table = _ticketInputEditor.Table;
		foreach (Member item in list)
		{
			UserState userState = item.UserState;
			if (userState == null)
			{
				return;
			}
			if (long.TryParse(userState.TableCellId, out var result) && !(item.Id == text) && currentInShowingNavTree.IsNavTreeNodeOpenPathMatchedCurrentNavTree(userState.TicketNavTreeNodePath))
			{
				if (_cellDrawCollection.Values.Count >= 10)
				{
					break;
				}
				try
				{
					System.Drawing.Image orGenerateImage = item.GetOrGenerateImage16();
					_cellDrawCollection.Add(result, orGenerateImage);
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		_isCellDataDirty = false;
	}

	public void RefreshCellFlickerWithOutRebuild()
	{
		try
		{
			if (_cellDrawCollection == null || _cellDrawCollection.IsEmpty)
				return;
			_bodyGrid?.Invalidate();
			_titleGrid?.Invalidate();
			_footerEditor?.Invalidate();
		}
		catch { }
	}

	private void TicketCellFlickUpdate(object sender, EventArgs e)
	{
		if (!Enable)
		{
			return;
		}
		if (!_bodyGrid.Visible)
		{
			SetCellFlickerDirty();
			return;
		}
		try
		{
			ReBuildCellFlicker();
			if (_cellDrawCollection.IsEmpty)
			{
				if (!_oldCollectionIsEmpty)
				{
					_bodyGrid.Invalidate();
					_titleGrid.Invalidate();
					_footerEditor.Invalidate();
				}
				_oldCollectionIsEmpty = true;
				return;
			}
			_oldCollectionIsEmpty = false;
			TicketInputTableVM vMData = _ticketInputEditor.VMData;
			int rowsCount = vMData.GetRowsCount();
			int columnsCount = vMData.GetColumnsCount();
			for (int i = 0; i < rowsCount; i++)
			{
				for (int j = 0; j < columnsCount; j++)
				{
					TicketInputCellVM cellVM = vMData.GetCellVM(i, j);
					if (cellVM.Column != null && cellVM.IsTableExistCell && _cellDrawCollection.Values.ContainsKey(cellVM.TableCell.Id.Value))
					{
						_bodyGrid.Invalidate(i, j + _bodyGrid.Cols.Fixed);
					}
				}
			}
			TicketInputTitleFooterVM vMData2 = _titleEditor.VMData;
			int rowsCount2 = vMData2.GetRowsCount();
			int columnsCount2 = vMData2.GetColumnsCount();
			for (int k = 0; k < rowsCount2; k++)
			{
				for (int l = 0; l < columnsCount2; l++)
				{
					TicketInputCellVM cellVM2 = vMData2.GetCellVM(k, l);
					if (cellVM2.Column != null && cellVM2.IsTableExistCell && _cellDrawCollection.Values.ContainsKey(cellVM2.TableCell.Id.Value))
					{
						_titleGrid.Invalidate(k, l);
					}
				}
			}
			TicketInputTitleFooterVM vMData3 = _footerEditor.VMData;
			int rowsCount3 = vMData3.GetRowsCount();
			int columnsCount3 = vMData3.GetColumnsCount();
			for (int m = 0; m < rowsCount3; m++)
			{
				for (int n = 0; n < columnsCount3; n++)
				{
					TicketInputCellVM cellVM3 = vMData3.GetCellVM(m, n);
					if (cellVM3.Column != null && cellVM3.IsTableExistCell && _cellDrawCollection.Values.ContainsKey(cellVM3.TableCell.Id.Value))
					{
						_footerGrid.Invalidate(m, n);
					}
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	protected void NavTreeNodeDoFlickUpdate()
	{
		try
		{
			_ticketNavTreeNodeFlickerCollection.DoFlickUpdate();
		}
		catch
		{
		}
	}

	private void TicketNavTreeNodeFlickUpdate(object sender, EventArgs e)
	{
		if (Enable)
		{
			NavTreeNodeDoFlickUpdate();
		}
	}

	public void RefreshNavTreeNodeFlickImageWithoutRebuild()
	{
		NavTreeNodeDoFlickUpdate();
	}

	public void ReBuildNavTreeFlicker()
	{
		try
		{
			_ticketNavTreeNodeFlickerCollection.Clear();
			MemberManager instance = MemberManager.GetInstance();
			IEnumerable<MemTab> enumerable = instance.GetGroup(SignalRClient.UserState.ProjectId)?.GetSelfAndMembers();
			if (enumerable == null)
			{
				return;
			}
			string text = _ticketInputEditor.Table.Id.Value.ToString();
			foreach (MemTab item in enumerable)
			{
				if (item is Member { UserState: { } userState } && !(userState.TreeNodeId != text) && !string.IsNullOrEmpty(userState.TicketNavTreeNodePath))
				{
					HandleOpenNavTreeNodeDelay(item.Id);
				}
			}
			_ticketNavTreeNodeFlickerCollection.DoFlickUpdate();
		}
		catch (Exception exception)
		{
			exception.Log("重构表单导航树的用户头像闪烁处理时发生了未预期的异常");
		}
	}

	public void HandleOpenNavTreeNodeDelay(string userId)
	{
		_navTreeNodeFlickerBuildQueue.Enqueue(new FlickerBuildData
		{
			UserId = userId
		});
	}

	public int HandleOpenNavTreeNode(string userId, Dictionary<TicketNavGrid, Dictionary<string, List<C1.Win.C1FlexGrid.Row>>> rowNodeOpenPathCacheDic = null)
	{
		try
		{
			string text = User.Current.Id.ToString();
			Group group = MemberManager.GetInstance().GetGroup(SignalRClient.UserState.ProjectId);
			if (group == null || !group.Exists(text))
			{
				_ticketNavTreeNodeFlickerCollection.Clear();
				return 0;
			}
			if (userId == text)
			{
				return 0;
			}
			if (group == null || !group.Exists(userId))
			{
				_ticketNavTreeNodeFlickerCollection.Remove(userId);
				return 0;
			}
			Member member = MemberManager.GetInstance().GetMember(userId);
			UserState userState = member.UserState;
			if (member == null || userState == null)
			{
				_ticketNavTreeNodeFlickerCollection.Remove(userId);
				return 0;
			}
			string text2 = Project.Current.Id.ToString();
			if (userState.ProjectId != text2)
			{
				_ticketNavTreeNodeFlickerCollection.Remove(userId);
				return 0;
			}
			if (!long.TryParse(userState.TreeNodeId, out var result))
			{
				_ticketNavTreeNodeFlickerCollection.Remove(userId);
				return 0;
			}
			if (!_navTreeOutBar.Visible || _ticketInputEditor.Table == null || _ticketInputEditor.Table.Id.Value != result)
			{
				_ticketNavTreeNodeFlickerCollection.Add(userId, text2, result, null);
				return 0;
			}
			if (string.IsNullOrEmpty(userState.TicketNavTreeNodePath))
			{
				_ticketNavTreeNodeFlickerCollection.Add(userId, text2, result, null);
				return 0;
			}
			TicketNavGrid ticketNavByOpenPath = _ticketInputEditor.GetTicketNavByOpenPath(userState.TicketNavTreeNodePath);
			if (ticketNavByOpenPath == null)
			{
				_ticketNavTreeNodeFlickerCollection.Add(userId, text2, result, null);
				return 0;
			}
			C1.Win.C1FlexGrid.Row tableRowByOpenPath = (C1.Win.C1FlexGrid.Row)ticketNavByOpenPath.GetTableRowByOpenPath(userState.TicketNavTreeNodePath, 10000);
			if (tableRowByOpenPath == null)
			{
				_ticketNavTreeNodeFlickerCollection.Add(userId, text2, result, null);
				return 0;
			}
			try
			{
				GridRowFlicker gridRowFlicker = new GridRowFlicker(tableRowByOpenPath);
				gridRowFlicker.Member = member;
				gridRowFlicker.OutPage = _ticketInputEditor.GetNavTreeGridPage(ticketNavByOpenPath);
				gridRowFlicker.NavTreeGrid = ticketNavByOpenPath;
				gridRowFlicker.SetFlickImage(member.GetOrGenerateImage16());
				_ticketNavTreeNodeFlickerCollection.Add(userId, text2, result, gridRowFlicker);
			}
			catch
			{
			}
			return 1;
		}
		catch (Exception exception)
		{
			exception.Log("构建表单导航树闪烁头像时发生了未预期的异常");
			return 0;
		}
	}

	public System.Drawing.Image GetTicketCellUserHeaderIcon(TicketInputCellVM vmCell)
	{
		if (vmCell == null || !vmCell.IsTableExistCell)
		{
			return null;
		}
		Cell tableCell = vmCell.TableCell;
		if (tableCell == null)
		{
			return null;
		}
		System.Drawing.Image image = _cellDrawCollection[tableCell.Id.Value];
		if (image != null)
		{
			if (SecondTrigger.Display)
			{
				return image;
			}
			return Resources.Empty16;
		}
		return null;
	}
}
