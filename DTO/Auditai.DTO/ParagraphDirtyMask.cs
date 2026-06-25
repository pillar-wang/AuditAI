using System.Collections.Specialized;

namespace Auditai.DTO;

public struct ParagraphDirtyMask
{
	private BitVector32 _mask;

	private static readonly int PARADIRTY_STREAM;

	private static readonly int PARADIRTY_COMMENT;

	private static readonly int PARADIRTY_INDEX;

	public bool IsStreamDirty
	{
		get
		{
			return _mask[PARADIRTY_STREAM];
		}
		set
		{
			_mask[PARADIRTY_STREAM] = value;
		}
	}

	public bool IsCommentDirty
	{
		get
		{
			return _mask[PARADIRTY_COMMENT];
		}
		set
		{
			_mask[PARADIRTY_COMMENT] = value;
		}
	}

	public bool IsIndexDirty
	{
		get
		{
			return _mask[PARADIRTY_INDEX];
		}
		set
		{
			_mask[PARADIRTY_INDEX] = value;
		}
	}

	static ParagraphDirtyMask()
	{
		PARADIRTY_STREAM = BitVector32.CreateMask();
		PARADIRTY_COMMENT = BitVector32.CreateMask(PARADIRTY_STREAM);
		PARADIRTY_INDEX = BitVector32.CreateMask(PARADIRTY_COMMENT);
	}

	public ParagraphDirtyMask(int i)
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
