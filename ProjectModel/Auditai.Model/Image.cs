using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using Auditai.DTO;

namespace Auditai.Model;

public class Image
{
	private static Dictionary<Tuple<RotateFlipType, RotateFlipType>, RotateFlipType> dicDoRotateFlip;

	private static readonly int IMAGEDIRTY_FILEID;

	private static readonly int IMAGEDIRTY_CENTER;

	private static readonly int IMAGEDIRTY_ZOOMFACTOR;

	private static readonly int IMAGEDIRTY_PAGESETUP;

	private static readonly int IMAGEDIRTY_ROTATEFLIP;

	private BitVector32 _dirty;

	public bool _isLoaded;

	public bool _isFirstOpened;

	public TreeImageNode TreeNode { get; set; }

	public Project Project => TreeNode.Project;

	public Id64 Id => TreeNode.Id;

	public SyncStatus Status => TreeNode.Status;

	public int Version => TreeNode.Version;

	public Guid FileId { get; set; }

	public PointF Center { get; set; } = new PointF(0.5f, 0.5f);


	public float ZoomFactor { get; set; } = 1f;


	public PageSetup PageSetup { get; set; } = new PageSetup();


	public RotateFlipType RotateFlip { get; set; }

	public bool NeedSave { get; set; }

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
			return _dirty[IMAGEDIRTY_FILEID];
		}
		set
		{
			_dirty[IMAGEDIRTY_FILEID] = value;
		}
	}

	public bool IsCenterDirty
	{
		get
		{
			return _dirty[IMAGEDIRTY_CENTER];
		}
		set
		{
			_dirty[IMAGEDIRTY_CENTER] = value;
		}
	}

	public bool IsZoomFactorDirty
	{
		get
		{
			return _dirty[IMAGEDIRTY_ZOOMFACTOR];
		}
		set
		{
			_dirty[IMAGEDIRTY_ZOOMFACTOR] = value;
		}
	}

	public bool IsPageSetupDirty
	{
		get
		{
			return _dirty[IMAGEDIRTY_PAGESETUP];
		}
		set
		{
			_dirty[IMAGEDIRTY_PAGESETUP] = value;
		}
	}

	public bool IsRotateFlipDirty
	{
		get
		{
			return _dirty[IMAGEDIRTY_ROTATEFLIP];
		}
		set
		{
			_dirty[IMAGEDIRTY_ROTATEFLIP] = value;
		}
	}

	public bool LocalExists => Version > -1;

	static Image()
	{
		dicDoRotateFlip = new Dictionary<Tuple<RotateFlipType, RotateFlipType>, RotateFlipType>
		{
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipNone, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.Rotate90FlipNone
			},
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipNone, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.Rotate270FlipNone
			},
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipNone, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.RotateNoneFlipX
			},
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipNone, RotateFlipType.Rotate180FlipX),
				RotateFlipType.Rotate180FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.Rotate180FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.RotateNoneFlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipNone, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.Rotate90FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipNone, RotateFlipType.Rotate180FlipX),
				RotateFlipType.Rotate270FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipNone, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.Rotate270FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipNone, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.Rotate90FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipNone, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.Rotate180FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipNone, RotateFlipType.Rotate180FlipX),
				RotateFlipType.RotateNoneFlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipNone, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.RotateNoneFlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipNone, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.Rotate180FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipNone, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.Rotate270FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipNone, RotateFlipType.Rotate180FlipX),
				RotateFlipType.Rotate90FlipX
			},
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipX, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.Rotate90FlipX
			},
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipX, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.Rotate270FlipX
			},
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipX, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.RotateNoneFlipNone
			},
			{
				Tuple.Create(RotateFlipType.RotateNoneFlipX, RotateFlipType.Rotate180FlipX),
				RotateFlipType.Rotate180FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipX, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.Rotate180FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipX, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.RotateNoneFlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipX, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.Rotate90FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate90FlipX, RotateFlipType.Rotate180FlipX),
				RotateFlipType.Rotate270FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipX, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.Rotate270FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipX, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.Rotate90FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipX, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.Rotate180FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate180FlipX, RotateFlipType.Rotate180FlipX),
				RotateFlipType.RotateNoneFlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipX, RotateFlipType.Rotate90FlipNone),
				RotateFlipType.RotateNoneFlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipX, RotateFlipType.Rotate270FlipNone),
				RotateFlipType.Rotate180FlipX
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipX, RotateFlipType.RotateNoneFlipX),
				RotateFlipType.Rotate270FlipNone
			},
			{
				Tuple.Create(RotateFlipType.Rotate270FlipX, RotateFlipType.Rotate180FlipX),
				RotateFlipType.Rotate90FlipNone
			}
		};
		IMAGEDIRTY_FILEID = BitVector32.CreateMask();
		IMAGEDIRTY_CENTER = BitVector32.CreateMask(IMAGEDIRTY_FILEID);
		IMAGEDIRTY_ZOOMFACTOR = BitVector32.CreateMask(IMAGEDIRTY_CENTER);
		IMAGEDIRTY_PAGESETUP = BitVector32.CreateMask(IMAGEDIRTY_ZOOMFACTOR);
		IMAGEDIRTY_ROTATEFLIP = BitVector32.CreateMask(IMAGEDIRTY_PAGESETUP);
	}

	public void UpdateFileId(Guid fileId)
	{
		FileId = fileId;
		NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			IsFileIdDirty = true;
		}
	}

	public void UpdateCenter(PointF center)
	{
		Center = center;
		NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			IsCenterDirty = true;
		}
	}

	public void UpdateZoomFactor(float zf)
	{
		ZoomFactor = zf;
		NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			IsZoomFactorDirty = true;
		}
	}

	public void UpdateRotateFlip(RotateFlipType rotateFlip)
	{
		RotateFlip = rotateFlip;
		NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			IsRotateFlipDirty = true;
		}
	}

	public void DoRotateFlip(RotateFlipType rf)
	{
		UpdateRotateFlip(dicDoRotateFlip[Tuple.Create(RotateFlip, rf)]);
	}

	public void DirtifyPageSetup()
	{
		NeedSave = true;
		if (Status == SyncStatus.Synced)
		{
			IsPageSetupDirty = true;
		}
	}

	public void SetSynced()
	{
		TreeNode.IsEntityDirty = false;
		_dirty = default(BitVector32);
	}

	public Image LoadAndReturn()
	{
		if (!_isLoaded)
		{
			_isLoaded = true;
			if (!LocalExists)
			{
				return this;
			}
			Auditai.DTO.Image image = Project.Dal.GetImage(Id);
			FileId = image.FileId;
			Dirty = image.Dirty;
			Center = new PointF(image.CenterX, image.CenterY);
			ZoomFactor = image.ZoomFactor;
			RotateFlip = (RotateFlipType)image.RotateFlip;
			if (image.PageSetup != null)
			{
				PageSetup.Deserialize(image.PageSetup);
			}
		}
		return this;
	}

	public Image ReloadAndReturn()
	{
		_isLoaded = false;
		return LoadAndReturn();
	}

	public void Save()
	{
		Project.Dal.SaveImage(ToDto());
		NeedSave = false;
	}

	public Auditai.DTO.Image ToDto()
	{
		return new Auditai.DTO.Image
		{
			Id = Id,
			FileId = FileId,
			Version = Version,
			Dirty = Dirty,
			CenterX = Center.X,
			CenterY = Center.Y,
			ZoomFactor = ZoomFactor,
			PageSetup = PageSetup.Serialize(),
			RotateFlip = (int)RotateFlip
		};
	}

	public System.Drawing.Image GetGraphicsImage()
	{
		string path = Project.FileCacheManager.GetPath(FileId);
		using FileStream fileStream = new FileStream(path, FileMode.Open);
		MemoryStream memoryStream = new MemoryStream();
		fileStream.CopyTo(memoryStream);
		GC.Collect();
		return System.Drawing.Image.FromStream(memoryStream, useEmbeddedColorManagement: true, validateImageData: false);
	}
}
