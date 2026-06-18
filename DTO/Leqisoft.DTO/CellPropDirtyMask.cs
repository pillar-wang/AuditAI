using System.Collections.Specialized;

namespace Leqisoft.DTO;

public struct CellPropDirtyMask
{
	private BitVector32 _mask;

	private static readonly int ATTACHMENTS;

	public bool IsAttachmentsDirty
	{
		get
		{
			return _mask[ATTACHMENTS];
		}
		set
		{
			_mask[ATTACHMENTS] = value;
		}
	}

	static CellPropDirtyMask()
	{
		ATTACHMENTS = BitVector32.CreateMask();
	}

	public CellPropDirtyMask(int i)
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
