using System;
using System.Collections.Generic;
using System.Drawing;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public class CellStyle
{
	internal CellStylePool _pool;

	private int? margin;

	[JsonIgnore]
	public Id64 Id { get; set; }

	public float? FontSize { get; set; }

	public Color? ForeColor { get; set; }

	public Color? BackColor { get; set; }

	public string FontFamily { get; set; }

	public CellTextAlign? Align { get; set; }

	public bool? Bold { get; set; }

	public bool? Italic { get; set; }

	public bool? Underline { get; set; }

	public int? Margin
	{
		get
		{
			return margin;
		}
		set
		{
			margin = value;
			if (margin < 0)
			{
				margin = 0;
			}
		}
	}

	[JsonIgnore]
	public Type DataType { get; set; }

	public DataFormat? Format { get; set; }

	public long? Locker { get; set; }

	[JsonIgnore]
	public SyncStatus Status { get; set; }

	public string DefaultValue { get; set; }

	public string Comment { get; set; }

	internal CellStyleMask GetMask()
	{
		CellStyleMask result = default(CellStyleMask);
		result.FontSize = FontSize.HasValue;
		result.ForeColor = ForeColor.HasValue;
		result.BackColor = BackColor.HasValue;
		result.FontFamily = FontFamily != null;
		result.Align = Align.HasValue;
		result.Bold = Bold.HasValue;
		result.Italic = Italic.HasValue;
		result.Underline = Underline.HasValue;
		result.Margin = margin.HasValue;
		result.DataType = DataType != null;
		result.Format = Format.HasValue;
		result.Locker = Locker.HasValue;
		result.DefaultValue = DefaultValue != null;
		result.Comment = Comment != null;
		return result;
	}

	internal CellStyle Clone()
	{
		return (CellStyle)MemberwiseClone();
	}

	internal void SetSynced()
	{
		Status = SyncStatus.Synced;
	}

	internal CellStyle Duplicate()
	{
		CellStyle cellStyle = Clone();
		cellStyle.Id = Project.Current.GetNextId();
		cellStyle.Status = SyncStatus.New;
		return cellStyle;
	}

	internal string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	internal void Deserialize(string s)
	{
		JsonConvert.PopulateObject(s, this);
	}

	internal Leqisoft.DTO.CellStyle ToDto()
	{
		return new Leqisoft.DTO.CellStyle
		{
			Id = Id,
			BackColor = BackColor.ToNullableInt(),
			FontSize = FontSize,
			ForeColor = ForeColor.ToNullableInt(),
			TableId = _pool._table.Id,
			FontFamily = FontFamily,
			Align = (int?)Align,
			Margin = Margin,
			Bold = Bold,
			Italic = Italic,
			Underline = Underline,
			DataType = Util.DataTypeToNullableInt(DataType),
			Format = Format?.Serialize(),
			Locked = Locker,
			Status = (int)Status,
			DefaultValue = DefaultValue,
			Comment = Comment
		};
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is CellStyle { Align: var align } cellStyle)
		{
			if (align == Align)
			{
				Color? backColor = cellStyle.BackColor;
				Color? backColor2 = BackColor;
				if (backColor.HasValue == backColor2.HasValue && (!backColor.HasValue || backColor.GetValueOrDefault() == backColor2.GetValueOrDefault()) && cellStyle.Bold == Bold && cellStyle.DataType == DataType && cellStyle.FontFamily == FontFamily && cellStyle.FontSize == FontSize)
				{
					backColor2 = cellStyle.ForeColor;
					backColor = ForeColor;
					if (backColor2.HasValue == backColor.HasValue && (!backColor2.HasValue || backColor2.GetValueOrDefault() == backColor.GetValueOrDefault()) && object.Equals(cellStyle.Format, Format) && cellStyle.Italic == Italic && cellStyle.Margin == Margin && cellStyle.Underline == Underline && cellStyle.Locker == Locker && cellStyle.DefaultValue == DefaultValue)
					{
						return cellStyle.Comment == Comment;
					}
				}
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = -220174402;
		num = num * -1521134295 + EqualityComparer<float?>.Default.GetHashCode(FontSize);
		num = num * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(ForeColor);
		num = num * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(BackColor);
		num = num * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FontFamily);
		num = num * -1521134295 + EqualityComparer<CellTextAlign?>.Default.GetHashCode(Align);
		num = num * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Bold);
		num = num * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Italic);
		num = num * -1521134295 + EqualityComparer<bool?>.Default.GetHashCode(Underline);
		num = num * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(Margin);
		num = num * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(DataType);
		num = num * -1521134295 + EqualityComparer<DataFormat?>.Default.GetHashCode(Format);
		num = num * -1521134295 + EqualityComparer<long?>.Default.GetHashCode(Locker);
		num = num * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DefaultValue);
		return num * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Comment);
	}

	public void CopyFrom(CellStyle rhs)
	{
		FontSize = rhs.FontSize;
		ForeColor = rhs.ForeColor;
		BackColor = rhs.BackColor;
		FontFamily = rhs.FontFamily;
		Align = rhs.Align;
		Bold = rhs.Bold;
		Italic = rhs.Italic;
		Underline = rhs.Underline;
		Margin = rhs.Margin;
		DataType = rhs.DataType;
		Format = rhs.Format;
		Locker = rhs.Locker;
		DefaultValue = rhs.DefaultValue;
		Comment = rhs.Comment;
	}
}
