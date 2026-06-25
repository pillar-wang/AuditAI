using System.Collections.Specialized;

namespace Auditai.DTO;

public struct CellDirtyMask
{
	private BitVector32 _mask;

	private static readonly int CELLDIRTY_VALUE;

	private static readonly int CELLDIRTY_FORMULA;

	private static readonly int CELLDIRTY_STYLE;

	private static readonly int CELLDIRTY_COLLECTSOURCE;

	private static readonly int CELLDIRTY_HEADERFORMULA;

	public bool IsValueDirty
	{
		get
		{
			return _mask[CELLDIRTY_VALUE];
		}
		set
		{
			_mask[CELLDIRTY_VALUE] = value;
		}
	}

	public bool IsFormulaDirty
	{
		get
		{
			return _mask[CELLDIRTY_FORMULA];
		}
		set
		{
			_mask[CELLDIRTY_FORMULA] = value;
		}
	}

	public bool IsStyleDirty
	{
		get
		{
			return _mask[CELLDIRTY_STYLE];
		}
		set
		{
			_mask[CELLDIRTY_STYLE] = value;
		}
	}

	public bool IsCollectSourceDirty
	{
		get
		{
			return _mask[CELLDIRTY_COLLECTSOURCE];
		}
		set
		{
			_mask[CELLDIRTY_COLLECTSOURCE] = value;
		}
	}

	public bool IsHeaderFormulaDirty
	{
		get
		{
			return _mask[CELLDIRTY_HEADERFORMULA];
		}
		set
		{
			_mask[CELLDIRTY_HEADERFORMULA] = value;
		}
	}

	static CellDirtyMask()
	{
		CELLDIRTY_VALUE = BitVector32.CreateMask();
		CELLDIRTY_FORMULA = BitVector32.CreateMask(CELLDIRTY_VALUE);
		CELLDIRTY_STYLE = BitVector32.CreateMask(CELLDIRTY_FORMULA);
		CELLDIRTY_COLLECTSOURCE = BitVector32.CreateMask(CELLDIRTY_STYLE);
		CELLDIRTY_HEADERFORMULA = BitVector32.CreateMask(CELLDIRTY_COLLECTSOURCE);
	}

	public CellDirtyMask(int i)
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
