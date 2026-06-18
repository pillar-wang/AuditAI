using System;
using System.Collections.Specialized;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class Pdf
{
	private static readonly int PDFDIRTY_FILEID;

	private BitVector32 _dirty;

	public bool _isLoaded;

	public TreePdfNode TreeNode { get; set; }

	public Project Project => TreeNode.Project;

	public Id64 Id => TreeNode.Id;

	public SyncStatus Status => TreeNode.Status;

	public int Version => TreeNode.Version;

	public Guid FileId { get; set; }

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

	public bool IsFileIdDirty
	{
		get
		{
			return _dirty[PDFDIRTY_FILEID];
		}
		set
		{
			_dirty[PDFDIRTY_FILEID] = value;
		}
	}

	public bool LocalExists => Version > -1;

	public int DisplayAreaOffsetX { get; set; }

	public int DisplayAreaOffsetY { get; set; }

	public double ZoomFactor { get; set; } = 1.5;


	public bool _isFirstOpened { get; set; }

	static Pdf()
	{
		PDFDIRTY_FILEID = BitVector32.CreateMask();
	}

	public void SetSynced()
	{
		TreeNode.IsEntityDirty = false;
		_dirty = default(BitVector32);
	}

	public Pdf LoadAndReturn()
	{
		if (!_isLoaded)
		{
			_isLoaded = true;
			if (!LocalExists)
			{
				return this;
			}
			Leqisoft.DTO.Pdf pdf = Project.Dal.GetPdf(Id);
			FileId = pdf.FileId;
		}
		return this;
	}

	public void Save()
	{
		Project.Dal.SavePdf(ToDto());
	}

	public Leqisoft.DTO.Pdf ToDto()
	{
		return new Leqisoft.DTO.Pdf
		{
			Id = Id,
			FileId = FileId,
			Version = Version
		};
	}
}
