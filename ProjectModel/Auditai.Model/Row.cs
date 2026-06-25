using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class Row
{
	private const int MIN_ROW_HEIGHT = 0;

	private const int MAX_ROW_HEIGHT = 9999;

	private int _height;

	private bool _visible;

	internal int _index;

	private long _locker;

	private int _serverIndex;

	private RowRole _role;

	private long _creator;

	private SaveSessionStatus _saveSessionStatus;

	public RowDirtyMask Dirty;

	public Id64 Id { get; set; }

	public SyncStatus Status { get; set; }

	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			NeedSave = true;
		}
	}

	public int Index
	{
		get
		{
			return _index;
		}
		set
		{
			_index = value;
			NeedSave = true;
		}
	}

	public int ServerIndex
	{
		get
		{
			return _serverIndex;
		}
		internal set
		{
			_serverIndex = value;
			NeedSave = true;
		}
	}

	public bool Visible
	{
		get
		{
			return _visible;
		}
		internal set
		{
			_visible = value;
			NeedSave = true;
		}
	}

	public long Creator
	{
		get
		{
			return _creator;
		}
		set
		{
			_creator = value;
			NeedSave = true;
		}
	}

	public SaveSessionStatus SaveSessionStatus
	{
		get
		{
			return _saveSessionStatus;
		}
		set
		{
			_saveSessionStatus = value;
		}
	}

	public Table Table { get; internal set; }

	public RowRole Role
	{
		get
		{
			return _role;
		}
		internal set
		{
			_role = value;
			NeedSave = true;
		}
	}

	public Permissions Permissions { get; } = new Permissions();


	public long Locker
	{
		get
		{
			return _locker;
		}
		set
		{
			_locker = value;
			NeedSave = true;
		}
	}

	public bool IsExisting
	{
		get
		{
			if (Status != 0)
			{
				return Status == SyncStatus.Synced;
			}
			return true;
		}
	}

	public bool IsLocked => _locker != 0;

	public bool NeedSave { get; set; }

	internal bool IsIndexDirty => ServerIndex != Index;

	public int GetMappedIndex()
	{
		if (Index < Table._dbRowSlots.Count)
		{
			return Table._dbRowSlots[Index];
		}
		return Table.DbRowsCount + Index - Table._dbRowSlots.Count;
	}

	public IEnumerable<Cell> GetCells()
	{
		return Table.Cells.Where((Cell c) => c.Row == this);
	}

	public void UpdateHeight(int height)
	{
		if (Height != height)
		{
			if (height < 0)
			{
				throw new ArgumentOutOfRangeException("height", $"行高不能小于{0}");
			}
			if (height > 9999)
			{
				throw new ArgumentOutOfRangeException("height", $"行高不能大于{9999}");
			}
			Height = height;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsHeightDirty = true;
			}
			Table.NeedSave = true;
		}
	}

	public void UpdateVisible(bool visible)
	{
		if (Visible != visible)
		{
			Visible = visible;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsVisibleDirty = true;
			}
			Table.NeedSave = true;
		}
	}

	public void UpdateRole(RowRole role)
	{
		if (Role == role)
		{
			return;
		}
		NeedSave = true;
		Table.NeedSave = true;
		Role = role;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsRoleDirty = true;
		}
		if (role == RowRole.Header || role == RowRole.Fixed)
		{
			Table.HeaderRowCache.Add(this);
			foreach (Cell cell in GetCells())
			{
				cell.UpdateStyle(cell.Column.Style);
			}
		}
		else
		{
			Table.HeaderRowCache.Remove(this);
		}
		if (role == RowRole.Header)
		{
			return;
		}
		foreach (Cell cell2 in GetCells())
		{
			cell2.UpdateHeaderFormula(string.Empty);
		}
	}

	public void UpdateLocker(long l)
	{
		if (Locker != l)
		{
			NeedSave = true;
			Table.NeedSave = true;
			Locker = l;
			if (Status == SyncStatus.Synced)
			{
				Dirty.IsLockerDirty = true;
			}
		}
	}

	public void UpdateCreator(long l)
	{
		NeedSave = true;
		Table.NeedSave = true;
		Creator = l;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsCreatorDirty = true;
		}
	}

	public void TagPermissionsDirty()
	{
		NeedSave = true;
		Table.NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			Dirty.IsPermissionsDirty = true;
		}
	}

	public void Remove()
	{
		Table.Rows.Remove(Index, 1);
	}

	public Auditai.DTO.Row ToDto()
	{
		return new Auditai.DTO.Row
		{
			Id = Id,
			Dirty = Dirty.ToInt(),
			Visible = Visible,
			Index = Index,
			Status = (int)Status,
			Height = Height,
			TableId = Table.Id,
			ServerIndex = ServerIndex,
			Locked = Locker,
			Role = (int)Role,
			Permissions = Permissions.Serialize(),
			Creator = Creator
		};
	}

	public void SetSynced()
	{
		if (Dirty.AnySet())
		{
			NeedSave = true;
			Dirty = default(RowDirtyMask);
		}
		if (Status == SyncStatus.New)
		{
			Status = SyncStatus.Synced;
			NeedSave = true;
		}
		if (ServerIndex != Index)
		{
			ServerIndex = Index;
			NeedSave = true;
		}
	}

	internal Row Duplicate()
	{
		return new Row
		{
			Height = Height,
			Id = Project.Current.GetNextId(),
			NeedSave = true,
			Status = SyncStatus.New,
			Visible = Visible,
			Locker = Locker,
			Dirty = default(RowDirtyMask),
			Role = Role,
			Creator = Creator,
			Index = Index
		};
	}

	internal Row Clone()
	{
		return (Row)MemberwiseClone();
	}
}
