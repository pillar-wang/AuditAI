using System.Collections.Specialized;

namespace Auditai.DTO;

public struct CellStyleMask
{
	private static readonly int MASK_FONTSIZE;

	private static readonly int MASK_FORECOLOR;

	private static readonly int MASK_BACKCOLOR;

	private static readonly int MASK_FONTFAMILY;

	private static readonly int MASK_ALIGN;

	private static readonly int MASK_BOLD;

	private static readonly int MASK_ITALIC;

	private static readonly int MASK_UNDERLINE;

	private static readonly int MASK_MARGIN;

	private static readonly int MASK_DATATYPE;

	private static readonly int MASK_FORMAT;

	private static readonly int MASK_LOCKER;

	private static readonly int MASK_DEFAULTVALUE;

	private static readonly int MASK_COMMENT;

	private BitVector32 _mask;

	public bool FontSize
	{
		get
		{
			return _mask[MASK_FONTSIZE];
		}
		set
		{
			_mask[MASK_FONTSIZE] = value;
		}
	}

	public bool ForeColor
	{
		get
		{
			return _mask[MASK_FORECOLOR];
		}
		set
		{
			_mask[MASK_FORECOLOR] = value;
		}
	}

	public bool BackColor
	{
		get
		{
			return _mask[MASK_BACKCOLOR];
		}
		set
		{
			_mask[MASK_BACKCOLOR] = value;
		}
	}

	public bool FontFamily
	{
		get
		{
			return _mask[MASK_FONTFAMILY];
		}
		set
		{
			_mask[MASK_FONTFAMILY] = value;
		}
	}

	public bool Align
	{
		get
		{
			return _mask[MASK_ALIGN];
		}
		set
		{
			_mask[MASK_ALIGN] = value;
		}
	}

	public bool Bold
	{
		get
		{
			return _mask[MASK_BOLD];
		}
		set
		{
			_mask[MASK_BOLD] = value;
		}
	}

	public bool Italic
	{
		get
		{
			return _mask[MASK_ITALIC];
		}
		set
		{
			_mask[MASK_ITALIC] = value;
		}
	}

	public bool Underline
	{
		get
		{
			return _mask[MASK_UNDERLINE];
		}
		set
		{
			_mask[MASK_UNDERLINE] = value;
		}
	}

	public bool Margin
	{
		get
		{
			return _mask[MASK_MARGIN];
		}
		set
		{
			_mask[MASK_MARGIN] = value;
		}
	}

	public bool DataType
	{
		get
		{
			return _mask[MASK_DATATYPE];
		}
		set
		{
			_mask[MASK_DATATYPE] = value;
		}
	}

	public bool Format
	{
		get
		{
			return _mask[MASK_FORMAT];
		}
		set
		{
			_mask[MASK_FORMAT] = value;
		}
	}

	public bool Locker
	{
		get
		{
			return _mask[MASK_LOCKER];
		}
		set
		{
			_mask[MASK_LOCKER] = value;
		}
	}

	public bool DefaultValue
	{
		get
		{
			return _mask[MASK_DEFAULTVALUE];
		}
		set
		{
			_mask[MASK_DEFAULTVALUE] = value;
		}
	}

	public bool Comment
	{
		get
		{
			return _mask[MASK_COMMENT];
		}
		set
		{
			_mask[MASK_COMMENT] = value;
		}
	}

	static CellStyleMask()
	{
		MASK_FONTSIZE = BitVector32.CreateMask();
		MASK_FORECOLOR = BitVector32.CreateMask(MASK_FONTSIZE);
		MASK_BACKCOLOR = BitVector32.CreateMask(MASK_FORECOLOR);
		MASK_FONTFAMILY = BitVector32.CreateMask(MASK_BACKCOLOR);
		MASK_ALIGN = BitVector32.CreateMask(MASK_FONTFAMILY);
		MASK_BOLD = BitVector32.CreateMask(MASK_ALIGN);
		MASK_ITALIC = BitVector32.CreateMask(MASK_BOLD);
		MASK_UNDERLINE = BitVector32.CreateMask(MASK_ITALIC);
		MASK_MARGIN = BitVector32.CreateMask(MASK_UNDERLINE);
		MASK_DATATYPE = BitVector32.CreateMask(MASK_MARGIN);
		MASK_FORMAT = BitVector32.CreateMask(MASK_DATATYPE);
		MASK_LOCKER = BitVector32.CreateMask(MASK_FORMAT);
		MASK_DEFAULTVALUE = BitVector32.CreateMask(MASK_LOCKER);
		MASK_COMMENT = BitVector32.CreateMask(MASK_DEFAULTVALUE);
	}

	public CellStyleMask(int i)
	{
		_mask = new BitVector32(i);
	}

	public int ToInt()
	{
		return _mask.Data;
	}
}
