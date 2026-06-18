using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public sealed class PushCellStyle : IMessage<PushCellStyle>, Google.Protobuf.IMessage, IEquatable<PushCellStyle>, IDeepCloneable<PushCellStyle>
{
	private static readonly MessageParser<PushCellStyle> _parser = new MessageParser<PushCellStyle>(() => new PushCellStyle());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int MaskFieldNumber = 2;

	private int mask_;

	public const int FontFamilyFieldNumber = 3;

	private string fontFamily_ = "";

	public const int FontSizeFieldNumber = 4;

	private float fontSize_;

	public const int ForeColorFieldNumber = 5;

	private int foreColor_;

	public const int BackColorFieldNumber = 6;

	private int backColor_;

	public const int AlignFieldNumber = 7;

	private int align_;

	public const int MarginFieldNumber = 8;

	private int margin_;

	public const int BoldFieldNumber = 9;

	private bool bold_;

	public const int ItalicFieldNumber = 10;

	private bool italic_;

	public const int UnderlineFieldNumber = 11;

	private bool underline_;

	public const int DataTypeFieldNumber = 12;

	private int dataType_;

	public const int FormatFieldNumber = 13;

	private string format_ = "";

	public const int LockerFieldNumber = 16;

	private long locker_;

	public const int DefaultValueFieldNumber = 17;

	private string defaultValue_ = "";

	public const int CommentFieldNumber = 18;

	private string comment_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PushCellStyle> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushtableReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Mask
	{
		get
		{
			return mask_;
		}
		set
		{
			mask_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string FontFamily
	{
		get
		{
			return fontFamily_;
		}
		set
		{
			fontFamily_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public float FontSize
	{
		get
		{
			return fontSize_;
		}
		set
		{
			fontSize_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int ForeColor
	{
		get
		{
			return foreColor_;
		}
		set
		{
			foreColor_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int BackColor
	{
		get
		{
			return backColor_;
		}
		set
		{
			backColor_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Align
	{
		get
		{
			return align_;
		}
		set
		{
			align_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Margin
	{
		get
		{
			return margin_;
		}
		set
		{
			margin_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool Bold
	{
		get
		{
			return bold_;
		}
		set
		{
			bold_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool Italic
	{
		get
		{
			return italic_;
		}
		set
		{
			italic_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool Underline
	{
		get
		{
			return underline_;
		}
		set
		{
			underline_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int DataType
	{
		get
		{
			return dataType_;
		}
		set
		{
			dataType_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Format
	{
		get
		{
			return format_;
		}
		set
		{
			format_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public long Locker
	{
		get
		{
			return locker_;
		}
		set
		{
			locker_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string DefaultValue
	{
		get
		{
			return defaultValue_;
		}
		set
		{
			defaultValue_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Comment
	{
		get
		{
			return comment_;
		}
		set
		{
			comment_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PushCellStyle()
	{
	}

	[DebuggerNonUserCode]
	public PushCellStyle(PushCellStyle other)
		: this()
	{
		id_ = other.id_;
		mask_ = other.mask_;
		fontFamily_ = other.fontFamily_;
		fontSize_ = other.fontSize_;
		foreColor_ = other.foreColor_;
		backColor_ = other.backColor_;
		align_ = other.align_;
		margin_ = other.margin_;
		bold_ = other.bold_;
		italic_ = other.italic_;
		underline_ = other.underline_;
		dataType_ = other.dataType_;
		format_ = other.format_;
		locker_ = other.locker_;
		defaultValue_ = other.defaultValue_;
		comment_ = other.comment_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushCellStyle Clone()
	{
		return new PushCellStyle(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushCellStyle);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushCellStyle other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (Mask != other.Mask)
		{
			return false;
		}
		if (FontFamily != other.FontFamily)
		{
			return false;
		}
		if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(FontSize, other.FontSize))
		{
			return false;
		}
		if (ForeColor != other.ForeColor)
		{
			return false;
		}
		if (BackColor != other.BackColor)
		{
			return false;
		}
		if (Align != other.Align)
		{
			return false;
		}
		if (Margin != other.Margin)
		{
			return false;
		}
		if (Bold != other.Bold)
		{
			return false;
		}
		if (Italic != other.Italic)
		{
			return false;
		}
		if (Underline != other.Underline)
		{
			return false;
		}
		if (DataType != other.DataType)
		{
			return false;
		}
		if (Format != other.Format)
		{
			return false;
		}
		if (Locker != other.Locker)
		{
			return false;
		}
		if (DefaultValue != other.DefaultValue)
		{
			return false;
		}
		if (Comment != other.Comment)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Id != 0L)
		{
			num ^= Id.GetHashCode();
		}
		if (Mask != 0)
		{
			num ^= Mask.GetHashCode();
		}
		if (FontFamily.Length != 0)
		{
			num ^= FontFamily.GetHashCode();
		}
		if (FontSize != 0f)
		{
			num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(FontSize);
		}
		if (ForeColor != 0)
		{
			num ^= ForeColor.GetHashCode();
		}
		if (BackColor != 0)
		{
			num ^= BackColor.GetHashCode();
		}
		if (Align != 0)
		{
			num ^= Align.GetHashCode();
		}
		if (Margin != 0)
		{
			num ^= Margin.GetHashCode();
		}
		if (Bold)
		{
			num ^= Bold.GetHashCode();
		}
		if (Italic)
		{
			num ^= Italic.GetHashCode();
		}
		if (Underline)
		{
			num ^= Underline.GetHashCode();
		}
		if (DataType != 0)
		{
			num ^= DataType.GetHashCode();
		}
		if (Format.Length != 0)
		{
			num ^= Format.GetHashCode();
		}
		if (Locker != 0L)
		{
			num ^= Locker.GetHashCode();
		}
		if (DefaultValue.Length != 0)
		{
			num ^= DefaultValue.GetHashCode();
		}
		if (Comment.Length != 0)
		{
			num ^= Comment.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= _unknownFields.GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString(this);
	}

	[DebuggerNonUserCode]
	public void WriteTo(CodedOutputStream output)
	{
		if (Id != 0L)
		{
			output.WriteRawTag(8);
			output.WriteInt64(Id);
		}
		if (Mask != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Mask);
		}
		if (FontFamily.Length != 0)
		{
			output.WriteRawTag(26);
			output.WriteString(FontFamily);
		}
		if (FontSize != 0f)
		{
			output.WriteRawTag(37);
			output.WriteFloat(FontSize);
		}
		if (ForeColor != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(ForeColor);
		}
		if (BackColor != 0)
		{
			output.WriteRawTag(48);
			output.WriteInt32(BackColor);
		}
		if (Align != 0)
		{
			output.WriteRawTag(56);
			output.WriteInt32(Align);
		}
		if (Margin != 0)
		{
			output.WriteRawTag(64);
			output.WriteInt32(Margin);
		}
		if (Bold)
		{
			output.WriteRawTag(72);
			output.WriteBool(Bold);
		}
		if (Italic)
		{
			output.WriteRawTag(80);
			output.WriteBool(Italic);
		}
		if (Underline)
		{
			output.WriteRawTag(88);
			output.WriteBool(Underline);
		}
		if (DataType != 0)
		{
			output.WriteRawTag(96);
			output.WriteInt32(DataType);
		}
		if (Format.Length != 0)
		{
			output.WriteRawTag(106);
			output.WriteString(Format);
		}
		if (Locker != 0L)
		{
			output.WriteRawTag(128, 1);
			output.WriteInt64(Locker);
		}
		if (DefaultValue.Length != 0)
		{
			output.WriteRawTag(138, 1);
			output.WriteString(DefaultValue);
		}
		if (Comment.Length != 0)
		{
			output.WriteRawTag(146, 1);
			output.WriteString(Comment);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (Id != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Id);
		}
		if (Mask != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Mask);
		}
		if (FontFamily.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(FontFamily);
		}
		if (FontSize != 0f)
		{
			num += 5;
		}
		if (ForeColor != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ForeColor);
		}
		if (BackColor != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(BackColor);
		}
		if (Align != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Align);
		}
		if (Margin != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Margin);
		}
		if (Bold)
		{
			num += 2;
		}
		if (Italic)
		{
			num += 2;
		}
		if (Underline)
		{
			num += 2;
		}
		if (DataType != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(DataType);
		}
		if (Format.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Format);
		}
		if (Locker != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(Locker);
		}
		if (DefaultValue.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(DefaultValue);
		}
		if (Comment.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Comment);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushCellStyle other)
	{
		if (other != null)
		{
			if (other.Id != 0L)
			{
				Id = other.Id;
			}
			if (other.Mask != 0)
			{
				Mask = other.Mask;
			}
			if (other.FontFamily.Length != 0)
			{
				FontFamily = other.FontFamily;
			}
			if (other.FontSize != 0f)
			{
				FontSize = other.FontSize;
			}
			if (other.ForeColor != 0)
			{
				ForeColor = other.ForeColor;
			}
			if (other.BackColor != 0)
			{
				BackColor = other.BackColor;
			}
			if (other.Align != 0)
			{
				Align = other.Align;
			}
			if (other.Margin != 0)
			{
				Margin = other.Margin;
			}
			if (other.Bold)
			{
				Bold = other.Bold;
			}
			if (other.Italic)
			{
				Italic = other.Italic;
			}
			if (other.Underline)
			{
				Underline = other.Underline;
			}
			if (other.DataType != 0)
			{
				DataType = other.DataType;
			}
			if (other.Format.Length != 0)
			{
				Format = other.Format;
			}
			if (other.Locker != 0L)
			{
				Locker = other.Locker;
			}
			if (other.DefaultValue.Length != 0)
			{
				DefaultValue = other.DefaultValue;
			}
			if (other.Comment.Length != 0)
			{
				Comment = other.Comment;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CodedInputStream input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
				break;
			case 8u:
				Id = input.ReadInt64();
				break;
			case 16u:
				Mask = input.ReadInt32();
				break;
			case 26u:
				FontFamily = input.ReadString();
				break;
			case 37u:
				FontSize = input.ReadFloat();
				break;
			case 40u:
				ForeColor = input.ReadInt32();
				break;
			case 48u:
				BackColor = input.ReadInt32();
				break;
			case 56u:
				Align = input.ReadInt32();
				break;
			case 64u:
				Margin = input.ReadInt32();
				break;
			case 72u:
				Bold = input.ReadBool();
				break;
			case 80u:
				Italic = input.ReadBool();
				break;
			case 88u:
				Underline = input.ReadBool();
				break;
			case 96u:
				DataType = input.ReadInt32();
				break;
			case 106u:
				Format = input.ReadString();
				break;
			case 128u:
				Locker = input.ReadInt64();
				break;
			case 138u:
				DefaultValue = input.ReadString();
				break;
			case 146u:
				Comment = input.ReadString();
				break;
			}
		}
	}
}
