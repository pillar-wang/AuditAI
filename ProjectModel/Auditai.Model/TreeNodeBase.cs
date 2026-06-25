using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Auditai.DTO;

namespace Auditai.Model;

public abstract class TreeNodeBase
{
	private static readonly int TREENODEDIRTY_NAME;

	private static readonly int TREENODEDIRTY_ENTITY;

	private static readonly int TREENODEDIRTY_NUMBER;

	private static readonly int TREENODEDIRTY_PARENT;

	private static readonly int TREENODEDIRTY_GROUP;

	private static readonly int TREENODEDIRTY_PERMISSIONS;

	private static readonly int TREENODEDIRTY_VISIBLE;

	private static readonly int TREENODEDIRTY_ROWWRITE;

	private static readonly int TREENODEDIRTY_ROWREAD;

	private BitVector32 _dirty;

	internal string _formulaUniqueName;

	public Id64 Id { get; set; }

	public SyncStatus Status { get; set; }

	public TreeGroup Group { get; internal set; }

	public Project Project => Group.Project;

	public TreeDirectoryNode Parent { get; internal set; }

	public int Index => Siblings.IndexOf(this);

	public int ServerIndex { get; internal set; }

	public string Name { get; set; }

	public int Version { get; set; }

	public Permissions Permissions { get; } = new Permissions();


	public bool RowWrite { get; set; }

	public bool RowRead { get; set; }

	public bool IsRoot => Parent == null;

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

	public string Number { get; set; }

	public bool IsEntityDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_ENTITY];
		}
		set
		{
			_dirty[TREENODEDIRTY_ENTITY] = value;
		}
	}

	public bool IsVisibleDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_VISIBLE];
		}
		set
		{
			_dirty[TREENODEDIRTY_VISIBLE] = value;
		}
	}

	public bool IsRowWriteDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_ROWWRITE];
		}
		set
		{
			_dirty[TREENODEDIRTY_ROWWRITE] = value;
		}
	}

	public bool IsRowReadDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_ROWREAD];
		}
		set
		{
			_dirty[TREENODEDIRTY_ROWREAD] = value;
		}
	}

	public int Level { get; set; }

	public bool Visible { get; set; }

	public string FormulaUniqueName
	{
		get
		{
			Project.RefreshTreeNodeFormulaNamesIfExpired();
			return _formulaUniqueName;
		}
		internal set
		{
			_formulaUniqueName = value;
		}
	}

	public virtual List<TreeNodeBase> Siblings
	{
		get
		{
			if (!IsRoot)
			{
				return Parent.Children;
			}
			return Group.RootNodes;
		}
	}

	public bool CanMoveUp1 => Index > 0;

	public bool CanMoveDown1 => Index < Siblings.Count - 1;

	internal bool IsNameDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_NAME];
		}
		set
		{
			_dirty[TREENODEDIRTY_NAME] = value;
		}
	}

	internal bool IsNumberDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_NUMBER];
		}
		set
		{
			_dirty[TREENODEDIRTY_NUMBER] = value;
		}
	}

	internal bool IsIndexDirty => ServerIndex != Index;

	internal bool IsGroupDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_GROUP];
		}
		set
		{
			_dirty[TREENODEDIRTY_GROUP] = value;
		}
	}

	internal bool IsParentDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_PARENT];
		}
		set
		{
			_dirty[TREENODEDIRTY_PARENT] = value;
		}
	}

	internal bool IsPermissionsDirty
	{
		get
		{
			return _dirty[TREENODEDIRTY_PERMISSIONS];
		}
		set
		{
			_dirty[TREENODEDIRTY_PERMISSIONS] = value;
		}
	}

	static TreeNodeBase()
	{
		TREENODEDIRTY_NAME = BitVector32.CreateMask();
		TREENODEDIRTY_ENTITY = BitVector32.CreateMask(TREENODEDIRTY_NAME);
		TREENODEDIRTY_NUMBER = BitVector32.CreateMask(TREENODEDIRTY_ENTITY);
		TREENODEDIRTY_PARENT = BitVector32.CreateMask(TREENODEDIRTY_NUMBER);
		TREENODEDIRTY_GROUP = BitVector32.CreateMask(TREENODEDIRTY_PARENT);
		TREENODEDIRTY_PERMISSIONS = BitVector32.CreateMask(TREENODEDIRTY_GROUP);
		TREENODEDIRTY_VISIBLE = BitVector32.CreateMask(TREENODEDIRTY_PERMISSIONS);
		TREENODEDIRTY_ROWWRITE = BitVector32.CreateMask(TREENODEDIRTY_VISIBLE);
		TREENODEDIRTY_ROWREAD = BitVector32.CreateMask(TREENODEDIRTY_ROWWRITE);
	}

	public void MoveUp1()
	{
		Project.NeedSave = true;
		int index = Index;
		Siblings.RemoveAt(index);
		Siblings.Insert(index - 1, this);
	}

	public void MoveDown1()
	{
		Project.NeedSave = true;
		int index = Index;
		Siblings.RemoveAt(index);
		Siblings.Insert(index + 1, this);
	}

	public virtual void MoveTo(TreeGroup tg)
	{
		CutForMove();
		UpdateGroup(tg);
		UpdateParent(null);
		tg.RootNodes.Add(this);
	}

	public virtual void MoveTo(TreeGroup tg, int index)
	{
		CutForMove();
		UpdateGroup(tg);
		UpdateParent(null);
		tg.RootNodes.Insert(index, this);
	}

	public virtual void MoveTo(TreeDirectoryNode tdn)
	{
		CutForMove();
		UpdateGroup(tdn.Group);
		UpdateParent(tdn);
		tdn.Children.Add(this);
	}

	public virtual void MoveTo(TreeDirectoryNode tdn, int index)
	{
		CutForMove();
		UpdateGroup(tdn.Group);
		UpdateParent(tdn);
		tdn.Children.Insert(index, this);
	}

	public virtual void UpdateName(string name)
	{
		Name = name;
		if (Status == SyncStatus.Synced)
		{
			IsNameDirty = true;
		}
		Project.NeedSave = true;
		Project._treeNodeFormulaNamesExpired = true;
	}

	public void UpdateNumber(string number)
	{
		if (number != Number)
		{
			Number = number;
			if (Status == SyncStatus.Synced)
			{
				IsNumberDirty = true;
			}
			Project.NeedSave = true;
		}
	}

	internal void UpdateParent(TreeDirectoryNode tdn)
	{
		if (Parent != tdn)
		{
			Parent = tdn;
			if (Status == SyncStatus.Synced)
			{
				IsParentDirty = true;
			}
			Project.NeedSave = true;
		}
	}

	internal void UpdateGroup(TreeGroup tg)
	{
		if (Group != tg)
		{
			Group = tg;
			if (Status == SyncStatus.Synced)
			{
				IsGroupDirty = true;
			}
			Project.NeedSave = true;
		}
	}

	public void UpdateRowWrite(bool value)
	{
		if (value != RowWrite)
		{
			RowWrite = value;
			if (Status == SyncStatus.Synced)
			{
				IsRowWriteDirty = true;
			}
			Project.NeedSave = true;
		}
	}

	public void UpdateRowRead(bool value)
	{
		if (value != RowRead)
		{
			RowRead = value;
			if (Status == SyncStatus.Synced)
			{
				IsRowReadDirty = true;
			}
			Project.NeedSave = true;
		}
	}

	public bool IsVisibleFromRoot()
	{
		TreeNodeBase treeNodeBase = this;
		do
		{
			if (!treeNodeBase.Visible)
			{
				return false;
			}
			treeNodeBase = treeNodeBase.Parent;
		}
		while (treeNodeBase != null);
		return true;
	}

	public void UpdateVisibleToRoot(bool v)
	{
		TreeNodeBase treeNodeBase = this;
		do
		{
			treeNodeBase.UpdateVisible(v);
			treeNodeBase = treeNodeBase.Parent;
		}
		while (treeNodeBase != null);
	}

	public virtual void UpdateVisible(bool v)
	{
		if (Visible != v)
		{
			Visible = v;
			if (Status == SyncStatus.Synced)
			{
				IsVisibleDirty = true;
			}
			Project.NeedSave = true;
		}
	}

	public bool AllAncestorsVisible()
	{
		if (Parent == null)
		{
			return true;
		}
		if (!Parent.Visible)
		{
			return false;
		}
		return Parent.AllAncestorsVisible();
	}

	public string GetDontHavePermissionString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!Permissions.CanRead())
		{
			stringBuilder.Append("查看|");
		}
		if (!Permissions.CanWrite())
		{
			stringBuilder.Append("编辑|");
		}
		if (!Permissions.CanEditSchema())
		{
			stringBuilder.Append("结构调整|");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		return stringBuilder.ToString();
	}

	public void TagPermissionsDirty()
	{
		if (Status == SyncStatus.Synced)
		{
			IsPermissionsDirty = true;
		}
		Project.NeedSave = true;
	}

	public virtual void Remove()
	{
		Siblings.Remove(this);
		if (Status == SyncStatus.New)
		{
			Project.TreeNodesToDelete.Add(Id);
		}
		else if (Status == SyncStatus.Synced)
		{
			Project.RemovedTreeNodes.Add(Id);
		}
		Status = SyncStatus.LocalDeleted;
		Project.NeedSave = true;
		Project._treeNodeFormulaNamesExpired = true;
	}

	public bool HasSchemaPermission()
	{
		if (Project.Current.Creator.Id == User.Current.Id)
		{
			return true;
		}
		if (!Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id))
		{
			return false;
		}
		if (Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return Permissions.CanEditSchema();
	}

	public bool HasReadPermission()
	{
		if (this is TreeDirectoryNode)
		{
			return true;
		}
		if (Project.Current.Creator.Id == User.Current.Id)
		{
			return true;
		}
		if (!Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id))
		{
			return false;
		}
		if (Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return Permissions.CanRead();
	}

	public bool HasWritePermission()
	{
		if (Project.Current.Creator.Id == User.Current.Id)
		{
			return true;
		}
		if (!Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id))
		{
			return false;
		}
		if (Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return Permissions.CanWrite();
	}

	internal virtual void ServerRemove()
	{
		Siblings.Remove(this);
		Project.TreeNodesToDelete.Add(Id);
	}

	public void SetSynced()
	{
		IsNameDirty = false;
		IsNumberDirty = false;
		IsGroupDirty = false;
		IsParentDirty = false;
		IsPermissionsDirty = false;
		IsVisibleDirty = false;
		IsRowReadDirty = false;
		IsRowWriteDirty = false;
		Status = SyncStatus.Synced;
		ServerIndex = Index;
	}

	internal static TreeNodeBase FromDto(TreeNode dto)
	{
		TreeNodeBase treeNodeBase = CreateInstanceFromCode(dto.Type);
		treeNodeBase.Id = dto.Id;
		treeNodeBase.Dirty = dto.Dirty;
		treeNodeBase.Status = (SyncStatus)dto.Status;
		treeNodeBase.Name = dto.Name;
		treeNodeBase.Level = dto.Level;
		treeNodeBase.ServerIndex = dto.ServerIndex;
		treeNodeBase.Version = dto.Version;
		treeNodeBase.Number = dto.Number;
		treeNodeBase.Permissions.Deserialize(dto.Permissions);
		treeNodeBase.Visible = dto.Visible;
		treeNodeBase.RowWrite = dto.RowWrite;
		treeNodeBase.RowRead = dto.RowRead;
		return treeNodeBase;
	}

	public TreeNode ToDto()
	{
		return new TreeNode
		{
			Id = Id,
			Dirty = Dirty,
			Status = (int)Status,
			Name = Name,
			Index = Index,
			ParentId = Parent?.Id,
			GroupId = Group.Id,
			Type = GetCode(),
			Level = Level,
			ServerIndex = ServerIndex,
			Version = Version,
			Number = Number,
			Permissions = Permissions.Serialize(),
			Visible = Visible,
			RowWrite = RowWrite,
			RowRead = RowRead
		};
	}

	protected internal abstract int GetCode();

	private void CutForMove()
	{
		if (IsRoot)
		{
			Group.RootNodes.Remove(this);
		}
		else
		{
			Parent.Children.Remove(this);
		}
	}

	internal string GetValidFormulaName()
	{
		string text = Column.reInvalidChars.Replace(Name, string.Empty);
		if (text == "")
		{
			text = "_";
		}
		return Column.reBeginWithDigit.Replace(text, "_$&");
	}

	internal static TreeNodeBase CreateInstanceFromCode(int code)
	{
		return code switch
		{
			0 => Activator.CreateInstance<TreeDirectoryNode>(), 
			1 => Activator.CreateInstance<TreeTableNode>(), 
			2 => Activator.CreateInstance<TreeDocumentNode>(), 
			3 => Activator.CreateInstance<TreePdfNode>(), 
			4 => Activator.CreateInstance<TreeImageNode>(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
