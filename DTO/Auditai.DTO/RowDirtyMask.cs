using System.Collections.Specialized;

namespace Auditai.DTO;

public struct RowDirtyMask
{
	private static readonly int ROWDIRTY_HEIGHT;

	private static readonly int ROWDIRTY_VISIBLE;

	private static readonly int ROWDIRTY_ROLE;

	private static readonly int ROWDIRTY_LOCKER;

	private static readonly int ROWDIRTY_PERMISSIONS;

	private static readonly int ROWDIRTY_CREATOR;

	private static readonly int ROWDIRTY_INDEX;

	private BitVector32 _mask;

	public bool IsHeightDirty
	{
		get
		{
			return _mask[ROWDIRTY_HEIGHT];
		}
		set
		{
			_mask[ROWDIRTY_HEIGHT] = value;
		}
	}

	public bool IsIndexDirty
	{
		get
		{
			return _mask[ROWDIRTY_INDEX];
		}
		set
		{
			_mask[ROWDIRTY_INDEX] = value;
		}
	}

	public bool IsVisibleDirty
	{
		get
		{
			return _mask[ROWDIRTY_VISIBLE];
		}
		set
		{
			_mask[ROWDIRTY_VISIBLE] = value;
		}
	}

	public bool IsLockerDirty
	{
		get
		{
			return _mask[ROWDIRTY_LOCKER];
		}
		set
		{
			_mask[ROWDIRTY_LOCKER] = value;
		}
	}

	public bool IsRoleDirty
	{
		get
		{
			return _mask[ROWDIRTY_ROLE];
		}
		set
		{
			_mask[ROWDIRTY_ROLE] = value;
		}
	}

	public bool IsPermissionsDirty
	{
		get
		{
			return _mask[ROWDIRTY_PERMISSIONS];
		}
		set
		{
			_mask[ROWDIRTY_PERMISSIONS] = value;
		}
	}

	public bool IsCreatorDirty
	{
		get
		{
			return _mask[ROWDIRTY_CREATOR];
		}
		set
		{
			_mask[ROWDIRTY_CREATOR] = value;
		}
	}

	static RowDirtyMask()
	{
		ROWDIRTY_HEIGHT = BitVector32.CreateMask();
		ROWDIRTY_VISIBLE = BitVector32.CreateMask(ROWDIRTY_HEIGHT);
		ROWDIRTY_ROLE = BitVector32.CreateMask(ROWDIRTY_VISIBLE);
		ROWDIRTY_LOCKER = BitVector32.CreateMask(ROWDIRTY_ROLE);
		ROWDIRTY_PERMISSIONS = BitVector32.CreateMask(ROWDIRTY_LOCKER);
		ROWDIRTY_CREATOR = BitVector32.CreateMask(ROWDIRTY_PERMISSIONS);
		ROWDIRTY_INDEX = BitVector32.CreateMask(ROWDIRTY_CREATOR);
	}

	public RowDirtyMask(int i)
	{
		_mask = new BitVector32(i);
	}

	public int ToInt()
	{
		return _mask.Data;
	}

	public bool AnySet()
	{
		return ToInt() != 0;
	}
}
