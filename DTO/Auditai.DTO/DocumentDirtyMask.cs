using System.Collections.Specialized;

namespace Auditai.DTO;

public struct DocumentDirtyMask
{
	private BitVector32 _mask;

	private static readonly int DOCDIRTY_LOCKER;

	private static readonly int DOCDIRTY_SECTPR;

	private static readonly int DOCDIRTY_MERGETABLE;

	public bool IsLockerDirty
	{
		get
		{
			return _mask[DOCDIRTY_LOCKER];
		}
		set
		{
			_mask[DOCDIRTY_LOCKER] = value;
		}
	}

	public bool IsSectPrDirty
	{
		get
		{
			return _mask[DOCDIRTY_SECTPR];
		}
		set
		{
			_mask[DOCDIRTY_SECTPR] = value;
		}
	}

	public bool IsMergeTableDirty
	{
		get
		{
			return _mask[DOCDIRTY_MERGETABLE];
		}
		set
		{
			_mask[DOCDIRTY_MERGETABLE] = value;
		}
	}

	static DocumentDirtyMask()
	{
		DOCDIRTY_LOCKER = BitVector32.CreateMask();
		DOCDIRTY_SECTPR = BitVector32.CreateMask(DOCDIRTY_LOCKER);
		DOCDIRTY_MERGETABLE = BitVector32.CreateMask(DOCDIRTY_SECTPR);
	}

	public DocumentDirtyMask(int i)
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
