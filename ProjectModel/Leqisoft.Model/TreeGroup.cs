using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class TreeGroup
{
	[CompilerGenerated]
	private sealed class _003CGetAllNodes_003Ed__36 : IEnumerable<TreeNodeBase>, IEnumerable, IEnumerator<TreeNodeBase>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private TreeNodeBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public TreeGroup _003C_003E4__this;

		private List<TreeNodeBase>.Enumerator _003C_003E7__wrap1;

		private TreeNodeBase _003Cnode_003E5__3;

		private IEnumerator<TreeNodeBase> _003C_003E7__wrap3;

		TreeNodeBase IEnumerator<TreeNodeBase>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetAllNodes_003Ed__36(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(List<TreeNodeBase>.Enumerator);
			_003Cnode_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				TreeGroup treeGroup = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = treeGroup.RootNodes.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					if (_003Cnode_003E5__3 is TreeDirectoryNode treeDirectoryNode)
					{
						_003C_003E7__wrap3 = treeDirectoryNode.GetDescendants().GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00d1;
					}
					goto IL_00eb;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00d1;
					}
					IL_00eb:
					_003Cnode_003E5__3 = null;
					break;
					IL_00d1:
					if (_003C_003E7__wrap3.MoveNext())
					{
						TreeNodeBase current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					goto IL_00eb;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_003Cnode_003E5__3 = _003C_003E7__wrap1.Current;
					_003C_003E2__current = _003Cnode_003E5__3;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(List<TreeNodeBase>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<TreeNodeBase> IEnumerable<TreeNodeBase>.GetEnumerator()
		{
			_003CGetAllNodes_003Ed__36 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetAllNodes_003Ed__36(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TreeNodeBase>)this).GetEnumerator();
		}
	}

	private static readonly int TREEGROUPDIRTY_NAME;

	private BitVector32 _dirty;

	public Id64 Id { get; set; }

	public int Dirty
	{
		get
		{
			return _dirty.Data;
		}
		set
		{
			_dirty = new BitVector32(value);
		}
	}

	public Project Project { get; internal set; }

	public string Name { get; set; }

	public int Index => Project.TreeGroups.IndexOf(this);

	public int ServerIndex { get; internal set; }

	public List<TreeNodeBase> RootNodes { get; } = new List<TreeNodeBase>();


	public SyncStatus Status { get; set; }

	public bool CanMoveUp1 => Index > 0;

	public bool CanMoveDown1 => Index < Project.TreeGroups.Count - 1;

	internal bool IsNameDirty
	{
		get
		{
			return _dirty[TREEGROUPDIRTY_NAME];
		}
		set
		{
			_dirty[TREEGROUPDIRTY_NAME] = value;
		}
	}

	internal bool IsIndexDirty => ServerIndex != Index;

	static TreeGroup()
	{
		TREEGROUPDIRTY_NAME = BitVector32.CreateMask();
	}

	public void SetSynced()
	{
		Status = SyncStatus.Synced;
		_dirty = default(BitVector32);
		ServerIndex = Index;
	}

	[IteratorStateMachine(typeof(_003CGetAllNodes_003Ed__36))]
	public IEnumerable<TreeNodeBase> GetAllNodes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllNodes_003Ed__36(-2)
		{
			_003C_003E4__this = this
		};
	}

	public void UpdateName(string name)
	{
		Name = name;
		if (Status == SyncStatus.Synced)
		{
			IsNameDirty = true;
		}
		Project.NeedSave = true;
		Project.FormulaMapDirty = true;
	}

	public void Remove()
	{
		Project.TreeGroups.Remove(this);
		if (Status == SyncStatus.New)
		{
			Project.TreeGroupsToDelete.Add(Id);
		}
		else if (Status == SyncStatus.Synced)
		{
			Project.RemovedTreeGroups.Add(Id);
		}
		Status = SyncStatus.LocalDeleted;
		foreach (TreeNodeBase item in GetAllNodes().ToList())
		{
			item.Remove();
		}
		Project.NeedSave = true;
		Project.FormulaMapDirty = true;
		Project._treeNodeFormulaNamesExpired = true;
	}

	internal void ServerRemove()
	{
		for (int num = RootNodes.Count - 1; num >= 0; num--)
		{
			RootNodes[num].ServerRemove();
		}
		Project.TreeGroupsToDelete.Add(Id);
		Project.TreeGroups.Remove(this);
	}

	public void MoveUp1()
	{
		int index = Index;
		Project.TreeGroups.RemoveAt(index);
		Project.TreeGroups.Insert(index - 1, this);
		Project.NeedSave = true;
		Project.FormulaMapDirty = true;
	}

	public void MoveDown1()
	{
		int index = Index;
		Project.TreeGroups.RemoveAt(index);
		Project.TreeGroups.Insert(index + 1, this);
		Project.NeedSave = true;
		Project.FormulaMapDirty = true;
	}

	public TreeDirectoryNode InsertRootDirectory(int index)
	{
		TreeDirectoryNode treeDirectoryNode = new TreeDirectoryNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewRootName()
		};
		InsertRootNode(treeDirectoryNode, index);
		return treeDirectoryNode;
	}

	public TreeTableNode InsertRootTable(int index, InitTableMode tableMode = InitTableMode.Default)
	{
		Project.ThrowIfMaxExceeded();
		TreeTableNode treeTableNode = new TreeTableNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewTableName()
		};
		InsertRootNode(treeTableNode, index);
		treeTableNode.Table.InitTableForCreate(tableMode);
		if (UserSet.Config.IsTitleFitTableName)
		{
			treeTableNode.Table.Title.TitleCell.Value = treeTableNode.Name;
		}
		return treeTableNode;
	}

	public TreeDocumentNode InsertRootDocument(int index)
	{
		Project.ThrowIfMaxExceeded();
		TreeDocumentNode treeDocumentNode = new TreeDocumentNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewDocumentName()
		};
		treeDocumentNode.Document._isLoaded = true;
		InsertRootNode(treeDocumentNode, index);
		return treeDocumentNode;
	}

	public TreeImageNode InsertRootImage(int index, Guid fileId)
	{
		Project.ThrowIfMaxExceeded();
		TreeImageNode treeImageNode = new TreeImageNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewImageName(),
			IsEntityDirty = true
		};
		treeImageNode.Image._isLoaded = true;
		treeImageNode.Image.FileId = fileId;
		treeImageNode.Image.NeedSave = true;
		InsertRootNode(treeImageNode, index);
		return treeImageNode;
	}

	public TreePdfNode InsertRootPdf(int index, Guid fileId)
	{
		Project.ThrowIfMaxExceeded();
		TreePdfNode treePdfNode = new TreePdfNode
		{
			Id = Project.Current.GetNextId(),
			Name = GetNewPdfName(),
			IsEntityDirty = true
		};
		treePdfNode.Pdf._isLoaded = true;
		treePdfNode.Pdf.FileId = fileId;
		InsertRootNode(treePdfNode, index);
		return treePdfNode;
	}

	public void InsertRootNode(TreeNodeBase root, int index)
	{
		root.Parent = null;
		root.Group = this;
		root.Level = 0;
		root.Visible = true;
		RootNodes.Insert(index, root);
		Project.NeedSave = true;
		Project._treeNodeFormulaNamesExpired = true;
		Project.FormulaMapDirty = true;
		if (root is TreeTableNode value)
		{
			Project._dicTableNodes.Add(root.Id, value);
		}
	}

	internal TreeGroup()
	{
	}

	private string GetNewRootName()
	{
		HashSet<string> hashSet = new HashSet<string>(RootNodes.Select((TreeNodeBase r) => r.Name));
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建文件夹{num}"));
		return $"新建文件夹{num}";
	}

	internal static TreeGroup FromDto(Leqisoft.DTO.TreeGroup dto)
	{
		return new TreeGroup
		{
			Id = dto.Id,
			Name = dto.Name,
			Status = (SyncStatus)dto.Status,
			ServerIndex = dto.ServerIndex,
			Dirty = dto.Dirty
		};
	}

	internal Leqisoft.DTO.TreeGroup ToDto()
	{
		return new Leqisoft.DTO.TreeGroup
		{
			Id = Id,
			ProjectId = Project.Id,
			Dirty = Dirty,
			Index = Index,
			Name = Name,
			Status = (int)Status,
			ServerIndex = ServerIndex
		};
	}

	private string GetNewDirectoryName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in GetAllNodes().OfType<TreeDirectoryNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建文件夹{num}"));
		return $"新建文件夹{num}";
	}

	private string GetNewTableName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in GetAllNodes().OfType<TreeTableNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建表格{num}"));
		return $"新建表格{num}";
	}

	private string GetNewDocumentName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in GetAllNodes().OfType<TreeDocumentNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建文档{num}"));
		return $"新建文档{num}";
	}

	private string GetNewImageName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in GetAllNodes().OfType<TreeImageNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建图片{num}"));
		return $"新建图片{num}";
	}

	private string GetNewPdfName()
	{
		HashSet<string> hashSet = new HashSet<string>(from n in GetAllNodes().OfType<TreePdfNode>()
			select n.Name);
		int num = 0;
		do
		{
			num++;
		}
		while (hashSet.Contains($"新建 PDF{num}"));
		return $"新建 PDF{num}";
	}
}
