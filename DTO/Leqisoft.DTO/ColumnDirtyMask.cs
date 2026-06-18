using System.Collections.Specialized;

namespace Leqisoft.DTO;

public struct ColumnDirtyMask
{
	private static readonly int COLUMNDIRTY_WIDTH;

	private static readonly int COLUMNDIRTY_CAPTION;

	private static readonly int COLUMNDIRTY_VISIBLE;

	private static readonly int COLUMNDIRTY_FORMULA;

	private static readonly int COLUMNDIRTY_STYLE;

	private static readonly int COLUMNDIRTY_CAPTIONSTYLE;

	private static readonly int COLUMNDIRTY_CONSOLIDATEATTRIBS;

	private static readonly int COLUMNDIRTY_SUBTOTALATTRIBS;

	private static readonly int COLUMNDIRTY_PERMISSIONS;

	private static readonly int COLUMNDIRTY_CAPTIONFORMULA;

	private static readonly int COLUMNDIRTY_INDEX;

	private static readonly int COLUMNDIRTY_CROSSATTRIBUTES;

	private BitVector32 _mask;

	public bool IsWidthDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_WIDTH];
		}
		set
		{
			_mask[COLUMNDIRTY_WIDTH] = value;
		}
	}

	public bool IsIndexDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_INDEX];
		}
		set
		{
			_mask[COLUMNDIRTY_INDEX] = value;
		}
	}

	public bool IsCaptionDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_CAPTION];
		}
		set
		{
			_mask[COLUMNDIRTY_CAPTION] = value;
		}
	}

	public bool IsVisibleDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_VISIBLE];
		}
		set
		{
			_mask[COLUMNDIRTY_VISIBLE] = value;
		}
	}

	public bool IsStyleDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_STYLE];
		}
		set
		{
			_mask[COLUMNDIRTY_STYLE] = value;
		}
	}

	public bool IsCaptionStyleDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_CAPTIONSTYLE];
		}
		set
		{
			_mask[COLUMNDIRTY_CAPTIONSTYLE] = value;
		}
	}

	public bool IsConsolidateAttribsDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_CONSOLIDATEATTRIBS];
		}
		set
		{
			_mask[COLUMNDIRTY_CONSOLIDATEATTRIBS] = value;
		}
	}

	public bool IsSubtotalAttribDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_SUBTOTALATTRIBS];
		}
		set
		{
			_mask[COLUMNDIRTY_SUBTOTALATTRIBS] = value;
		}
	}

	public bool IsFormulaDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_FORMULA];
		}
		set
		{
			_mask[COLUMNDIRTY_FORMULA] = value;
		}
	}

	public bool IsPermissionsDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_PERMISSIONS];
		}
		set
		{
			_mask[COLUMNDIRTY_PERMISSIONS] = value;
		}
	}

	public bool IsCaptionFormulaDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_CAPTIONFORMULA];
		}
		set
		{
			_mask[COLUMNDIRTY_CAPTIONFORMULA] = value;
		}
	}

	public bool IsCrossAttributesDirty
	{
		get
		{
			return _mask[COLUMNDIRTY_CROSSATTRIBUTES];
		}
		set
		{
			_mask[COLUMNDIRTY_CROSSATTRIBUTES] = value;
		}
	}

	static ColumnDirtyMask()
	{
		COLUMNDIRTY_WIDTH = BitVector32.CreateMask();
		COLUMNDIRTY_CAPTION = BitVector32.CreateMask(COLUMNDIRTY_WIDTH);
		COLUMNDIRTY_VISIBLE = BitVector32.CreateMask(COLUMNDIRTY_CAPTION);
		COLUMNDIRTY_FORMULA = BitVector32.CreateMask(COLUMNDIRTY_VISIBLE);
		COLUMNDIRTY_STYLE = BitVector32.CreateMask(COLUMNDIRTY_FORMULA);
		COLUMNDIRTY_CAPTIONSTYLE = BitVector32.CreateMask(COLUMNDIRTY_STYLE);
		COLUMNDIRTY_CONSOLIDATEATTRIBS = BitVector32.CreateMask(COLUMNDIRTY_CAPTIONSTYLE);
		COLUMNDIRTY_SUBTOTALATTRIBS = BitVector32.CreateMask(COLUMNDIRTY_CONSOLIDATEATTRIBS);
		COLUMNDIRTY_PERMISSIONS = BitVector32.CreateMask(COLUMNDIRTY_SUBTOTALATTRIBS);
		COLUMNDIRTY_CAPTIONFORMULA = BitVector32.CreateMask(COLUMNDIRTY_PERMISSIONS);
		COLUMNDIRTY_INDEX = BitVector32.CreateMask(COLUMNDIRTY_CAPTIONFORMULA);
		COLUMNDIRTY_CROSSATTRIBUTES = BitVector32.CreateMask(COLUMNDIRTY_INDEX);
	}

	public ColumnDirtyMask(int i)
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
