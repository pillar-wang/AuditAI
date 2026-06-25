using System.Collections.Specialized;

namespace Auditai.DTO;

public struct TableDirtyMask
{
	private BitVector32 _mask;

	private static readonly int TABLEDIRTY_TITLE;

	private static readonly int TABLEDIRTY_NOTE;

	private static readonly int TABLEDIRTY_HEADERHEIGHTS;

	private static readonly int TABLEDIRTY_DEFAULTSTYLE;

	private static readonly int TABLEDIRTY_PAGESETUP;

	private static readonly int TABLEDIRTY_CONSOLIDATE;

	private static readonly int TABLEDIRTY_BORDERSTYLE;

	private static readonly int TABLEDIRTY_FROZENCOLS;

	private static readonly int TABLEDIRTY_HEADERMODE;

	private static readonly int TABLEDIRTY_COLLECTSOURCE;

	private static readonly int TABLEDIRTY_LOCKER;

	private static readonly int TABLEDIRTY_MAKER;

	private static readonly int TABLEDIRTY_CHECKER;

	private static readonly int TABLEDIRTY_FILTER;

	private static readonly int TABLEDIRTY_FOOT;

	private static readonly int TABLEDIRTY_ROWOWNEREXCLUSIVE;

	private static readonly int TABLEDIRTY_ROWOWNERLOAD;

	private static readonly int TABLEDIRTY_ROWOWNERLOADSHARE;

	private static readonly int TABLEDIRTY_TICKET;

	private static readonly int TABLEDIRTY_CONTROLFORMULA;

	public bool IsTitleDirty
	{
		get
		{
			return _mask[TABLEDIRTY_TITLE];
		}
		set
		{
			_mask[TABLEDIRTY_TITLE] = value;
		}
	}

	public bool IsFootDirty
	{
		get
		{
			return _mask[TABLEDIRTY_FOOT];
		}
		set
		{
			_mask[TABLEDIRTY_FOOT] = value;
		}
	}

	public bool IsNoteDirty
	{
		get
		{
			return _mask[TABLEDIRTY_NOTE];
		}
		set
		{
			_mask[TABLEDIRTY_NOTE] = value;
		}
	}

	public bool IsHeaderHeightsDirty
	{
		get
		{
			return _mask[TABLEDIRTY_HEADERHEIGHTS];
		}
		set
		{
			_mask[TABLEDIRTY_HEADERHEIGHTS] = value;
		}
	}

	public bool IsDefaultStyleDirty
	{
		get
		{
			return _mask[TABLEDIRTY_DEFAULTSTYLE];
		}
		set
		{
			_mask[TABLEDIRTY_DEFAULTSTYLE] = value;
		}
	}

	public bool IsPageSetupDirty
	{
		get
		{
			return _mask[TABLEDIRTY_PAGESETUP];
		}
		set
		{
			_mask[TABLEDIRTY_PAGESETUP] = value;
		}
	}

	public bool IsConsolidateSettingsDirty
	{
		get
		{
			return _mask[TABLEDIRTY_CONSOLIDATE];
		}
		set
		{
			_mask[TABLEDIRTY_CONSOLIDATE] = value;
		}
	}

	public bool IsBorderStyleDirty
	{
		get
		{
			return _mask[TABLEDIRTY_BORDERSTYLE];
		}
		set
		{
			_mask[TABLEDIRTY_BORDERSTYLE] = value;
		}
	}

	public bool IsFrozenColsDirty
	{
		get
		{
			return _mask[TABLEDIRTY_FROZENCOLS];
		}
		set
		{
			_mask[TABLEDIRTY_FROZENCOLS] = value;
		}
	}

	public bool IsHeaderModeDirty
	{
		get
		{
			return _mask[TABLEDIRTY_HEADERMODE];
		}
		set
		{
			_mask[TABLEDIRTY_HEADERMODE] = value;
		}
	}

	public bool IsCollectSourceDirty
	{
		get
		{
			return _mask[TABLEDIRTY_COLLECTSOURCE];
		}
		set
		{
			_mask[TABLEDIRTY_COLLECTSOURCE] = value;
		}
	}

	public bool IsLockerDirty
	{
		get
		{
			return _mask[TABLEDIRTY_LOCKER];
		}
		set
		{
			_mask[TABLEDIRTY_LOCKER] = value;
		}
	}

	public bool IsFilterDirty
	{
		get
		{
			return _mask[TABLEDIRTY_FILTER];
		}
		set
		{
			_mask[TABLEDIRTY_FILTER] = value;
		}
	}

	public bool IsRowOwnerExclusiveDirty
	{
		get
		{
			return _mask[TABLEDIRTY_ROWOWNEREXCLUSIVE];
		}
		set
		{
			_mask[TABLEDIRTY_ROWOWNEREXCLUSIVE] = value;
		}
	}

	public bool IsRowOwnerLoadDirty
	{
		get
		{
			return _mask[TABLEDIRTY_ROWOWNERLOAD];
		}
		set
		{
			_mask[TABLEDIRTY_ROWOWNERLOAD] = value;
		}
	}

	public bool IsRowOwnerLoadShareDirty
	{
		get
		{
			return _mask[TABLEDIRTY_ROWOWNERLOADSHARE];
		}
		set
		{
			_mask[TABLEDIRTY_ROWOWNERLOADSHARE] = value;
		}
	}

	public bool IsTicketDirty
	{
		get
		{
			return _mask[TABLEDIRTY_TICKET];
		}
		set
		{
			_mask[TABLEDIRTY_TICKET] = value;
		}
	}

	public bool IsControlFormulaDirty
	{
		get
		{
			return _mask[TABLEDIRTY_CONTROLFORMULA];
		}
		set
		{
			_mask[TABLEDIRTY_CONTROLFORMULA] = value;
		}
	}

	static TableDirtyMask()
	{
		TABLEDIRTY_TITLE = BitVector32.CreateMask();
		TABLEDIRTY_NOTE = BitVector32.CreateMask(TABLEDIRTY_TITLE);
		TABLEDIRTY_HEADERHEIGHTS = BitVector32.CreateMask(TABLEDIRTY_NOTE);
		TABLEDIRTY_DEFAULTSTYLE = BitVector32.CreateMask(TABLEDIRTY_HEADERHEIGHTS);
		TABLEDIRTY_PAGESETUP = BitVector32.CreateMask(TABLEDIRTY_DEFAULTSTYLE);
		TABLEDIRTY_CONSOLIDATE = BitVector32.CreateMask(TABLEDIRTY_PAGESETUP);
		TABLEDIRTY_BORDERSTYLE = BitVector32.CreateMask(TABLEDIRTY_CONSOLIDATE);
		TABLEDIRTY_FROZENCOLS = BitVector32.CreateMask(TABLEDIRTY_BORDERSTYLE);
		TABLEDIRTY_HEADERMODE = BitVector32.CreateMask(TABLEDIRTY_FROZENCOLS);
		TABLEDIRTY_COLLECTSOURCE = BitVector32.CreateMask(TABLEDIRTY_HEADERMODE);
		TABLEDIRTY_LOCKER = BitVector32.CreateMask(TABLEDIRTY_COLLECTSOURCE);
		TABLEDIRTY_MAKER = BitVector32.CreateMask(TABLEDIRTY_LOCKER);
		TABLEDIRTY_CHECKER = BitVector32.CreateMask(TABLEDIRTY_MAKER);
		TABLEDIRTY_FILTER = BitVector32.CreateMask(TABLEDIRTY_CHECKER);
		TABLEDIRTY_FOOT = BitVector32.CreateMask(TABLEDIRTY_FILTER);
		TABLEDIRTY_ROWOWNEREXCLUSIVE = BitVector32.CreateMask(TABLEDIRTY_FOOT);
		TABLEDIRTY_ROWOWNERLOAD = BitVector32.CreateMask(TABLEDIRTY_ROWOWNEREXCLUSIVE);
		TABLEDIRTY_ROWOWNERLOADSHARE = BitVector32.CreateMask(TABLEDIRTY_ROWOWNERLOAD);
		TABLEDIRTY_TICKET = BitVector32.CreateMask(TABLEDIRTY_ROWOWNERLOADSHARE);
		TABLEDIRTY_CONTROLFORMULA = BitVector32.CreateMask(TABLEDIRTY_TICKET);
	}

	public TableDirtyMask(int i)
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
