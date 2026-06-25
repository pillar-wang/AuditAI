using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullCellStyle : IMessage<PullCellStyle>, Google.Protobuf.IMessage, IEquatable<PullCellStyle>, IDeepCloneable<PullCellStyle>
{
	private static readonly MessageParser<PullCellStyle> _parser = new MessageParser<PullCellStyle>(() => new PullCellStyle());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int FontFamilyFieldNumber = 2;

	private NullableString fontFamily_;

	public const int FontSizeFieldNumber = 3;

	private NullableFloat fontSize_;

	public const int ForeColorFieldNumber = 4;

	private NullableInt32 foreColor_;

	public const int BackColorFieldNumber = 5;

	private NullableInt32 backColor_;

	public const int AlignFieldNumber = 6;

	private NullableInt32 align_;

	public const int MarginFieldNumber = 7;

	private NullableInt32 margin_;

	public const int BoldFieldNumber = 8;

	private NullableBool bold_;

	public const int ItalicFieldNumber = 9;

	private NullableBool italic_;

	public const int UnderlineFieldNumber = 10;

	private NullableBool underline_;

	public const int DataTypeFieldNumber = 11;

	private NullableInt32 dataType_;

	public const int FormatFieldNumber = 12;

	private NullableString format_;

	public const int LockerFieldNumber = 13;

	private NullableInt64 locker_;

	public const int DefaultValueFieldNumber = 14;

	private NullableString defaultValue_;

	public const int CommentFieldNumber = 15;

	private NullableString comment_;

	[DebuggerNonUserCode]
	public static MessageParser<PullCellStyle> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[16];

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
	public NullableString FontFamily
	{
		get
		{
			return fontFamily_;
		}
		set
		{
			fontFamily_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableFloat FontSize
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
	public NullableInt32 ForeColor
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
	public NullableInt32 BackColor
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
	public NullableInt32 Align
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
	public NullableInt32 Margin
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
	public NullableBool Bold
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
	public NullableBool Italic
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
	public NullableBool Underline
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
	public NullableInt32 DataType
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
	public NullableString Format
	{
		get
		{
			return format_;
		}
		set
		{
			format_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableInt64 Locker
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
	public NullableString DefaultValue
	{
		get
		{
			return defaultValue_;
		}
		set
		{
			defaultValue_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableString Comment
	{
		get
		{
			return comment_;
		}
		set
		{
			comment_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PullCellStyle()
	{
	}

	[DebuggerNonUserCode]
	public PullCellStyle(PullCellStyle other)
		: this()
	{
		id_ = other.id_;
		fontFamily_ = ((other.fontFamily_ != null) ? other.fontFamily_.Clone() : null);
		fontSize_ = ((other.fontSize_ != null) ? other.fontSize_.Clone() : null);
		foreColor_ = ((other.foreColor_ != null) ? other.foreColor_.Clone() : null);
		backColor_ = ((other.backColor_ != null) ? other.backColor_.Clone() : null);
		align_ = ((other.align_ != null) ? other.align_.Clone() : null);
		margin_ = ((other.margin_ != null) ? other.margin_.Clone() : null);
		bold_ = ((other.bold_ != null) ? other.bold_.Clone() : null);
		italic_ = ((other.italic_ != null) ? other.italic_.Clone() : null);
		underline_ = ((other.underline_ != null) ? other.underline_.Clone() : null);
		dataType_ = ((other.dataType_ != null) ? other.dataType_.Clone() : null);
		format_ = ((other.format_ != null) ? other.format_.Clone() : null);
		locker_ = ((other.locker_ != null) ? other.locker_.Clone() : null);
		defaultValue_ = ((other.defaultValue_ != null) ? other.defaultValue_.Clone() : null);
		comment_ = ((other.comment_ != null) ? other.comment_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullCellStyle Clone()
	{
		return new PullCellStyle(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullCellStyle);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullCellStyle other)
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
		if (!object.Equals(FontFamily, other.FontFamily))
		{
			return false;
		}
		if (!object.Equals(FontSize, other.FontSize))
		{
			return false;
		}
		if (!object.Equals(ForeColor, other.ForeColor))
		{
			return false;
		}
		if (!object.Equals(BackColor, other.BackColor))
		{
			return false;
		}
		if (!object.Equals(Align, other.Align))
		{
			return false;
		}
		if (!object.Equals(Margin, other.Margin))
		{
			return false;
		}
		if (!object.Equals(Bold, other.Bold))
		{
			return false;
		}
		if (!object.Equals(Italic, other.Italic))
		{
			return false;
		}
		if (!object.Equals(Underline, other.Underline))
		{
			return false;
		}
		if (!object.Equals(DataType, other.DataType))
		{
			return false;
		}
		if (!object.Equals(Format, other.Format))
		{
			return false;
		}
		if (!object.Equals(Locker, other.Locker))
		{
			return false;
		}
		if (!object.Equals(DefaultValue, other.DefaultValue))
		{
			return false;
		}
		if (!object.Equals(Comment, other.Comment))
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
		if (fontFamily_ != null)
		{
			num ^= FontFamily.GetHashCode();
		}
		if (fontSize_ != null)
		{
			num ^= FontSize.GetHashCode();
		}
		if (foreColor_ != null)
		{
			num ^= ForeColor.GetHashCode();
		}
		if (backColor_ != null)
		{
			num ^= BackColor.GetHashCode();
		}
		if (align_ != null)
		{
			num ^= Align.GetHashCode();
		}
		if (margin_ != null)
		{
			num ^= Margin.GetHashCode();
		}
		if (bold_ != null)
		{
			num ^= Bold.GetHashCode();
		}
		if (italic_ != null)
		{
			num ^= Italic.GetHashCode();
		}
		if (underline_ != null)
		{
			num ^= Underline.GetHashCode();
		}
		if (dataType_ != null)
		{
			num ^= DataType.GetHashCode();
		}
		if (format_ != null)
		{
			num ^= Format.GetHashCode();
		}
		if (locker_ != null)
		{
			num ^= Locker.GetHashCode();
		}
		if (defaultValue_ != null)
		{
			num ^= DefaultValue.GetHashCode();
		}
		if (comment_ != null)
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
		if (fontFamily_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(FontFamily);
		}
		if (fontSize_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(FontSize);
		}
		if (foreColor_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(ForeColor);
		}
		if (backColor_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(BackColor);
		}
		if (align_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(Align);
		}
		if (margin_ != null)
		{
			output.WriteRawTag(58);
			output.WriteMessage(Margin);
		}
		if (bold_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(Bold);
		}
		if (italic_ != null)
		{
			output.WriteRawTag(74);
			output.WriteMessage(Italic);
		}
		if (underline_ != null)
		{
			output.WriteRawTag(82);
			output.WriteMessage(Underline);
		}
		if (dataType_ != null)
		{
			output.WriteRawTag(90);
			output.WriteMessage(DataType);
		}
		if (format_ != null)
		{
			output.WriteRawTag(98);
			output.WriteMessage(Format);
		}
		if (locker_ != null)
		{
			output.WriteRawTag(106);
			output.WriteMessage(Locker);
		}
		if (defaultValue_ != null)
		{
			output.WriteRawTag(114);
			output.WriteMessage(DefaultValue);
		}
		if (comment_ != null)
		{
			output.WriteRawTag(122);
			output.WriteMessage(Comment);
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
		if (fontFamily_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(FontFamily);
		}
		if (fontSize_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(FontSize);
		}
		if (foreColor_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ForeColor);
		}
		if (backColor_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(BackColor);
		}
		if (align_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Align);
		}
		if (margin_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Margin);
		}
		if (bold_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Bold);
		}
		if (italic_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Italic);
		}
		if (underline_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Underline);
		}
		if (dataType_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(DataType);
		}
		if (format_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Format);
		}
		if (locker_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Locker);
		}
		if (defaultValue_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(DefaultValue);
		}
		if (comment_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Comment);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PullCellStyle other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0L)
		{
			Id = other.Id;
		}
		if (other.fontFamily_ != null)
		{
			if (fontFamily_ == null)
			{
				FontFamily = new NullableString();
			}
			FontFamily.MergeFrom(other.FontFamily);
		}
		if (other.fontSize_ != null)
		{
			if (fontSize_ == null)
			{
				FontSize = new NullableFloat();
			}
			FontSize.MergeFrom(other.FontSize);
		}
		if (other.foreColor_ != null)
		{
			if (foreColor_ == null)
			{
				ForeColor = new NullableInt32();
			}
			ForeColor.MergeFrom(other.ForeColor);
		}
		if (other.backColor_ != null)
		{
			if (backColor_ == null)
			{
				BackColor = new NullableInt32();
			}
			BackColor.MergeFrom(other.BackColor);
		}
		if (other.align_ != null)
		{
			if (align_ == null)
			{
				Align = new NullableInt32();
			}
			Align.MergeFrom(other.Align);
		}
		if (other.margin_ != null)
		{
			if (margin_ == null)
			{
				Margin = new NullableInt32();
			}
			Margin.MergeFrom(other.Margin);
		}
		if (other.bold_ != null)
		{
			if (bold_ == null)
			{
				Bold = new NullableBool();
			}
			Bold.MergeFrom(other.Bold);
		}
		if (other.italic_ != null)
		{
			if (italic_ == null)
			{
				Italic = new NullableBool();
			}
			Italic.MergeFrom(other.Italic);
		}
		if (other.underline_ != null)
		{
			if (underline_ == null)
			{
				Underline = new NullableBool();
			}
			Underline.MergeFrom(other.Underline);
		}
		if (other.dataType_ != null)
		{
			if (dataType_ == null)
			{
				DataType = new NullableInt32();
			}
			DataType.MergeFrom(other.DataType);
		}
		if (other.format_ != null)
		{
			if (format_ == null)
			{
				Format = new NullableString();
			}
			Format.MergeFrom(other.Format);
		}
		if (other.locker_ != null)
		{
			if (locker_ == null)
			{
				Locker = new NullableInt64();
			}
			Locker.MergeFrom(other.Locker);
		}
		if (other.defaultValue_ != null)
		{
			if (defaultValue_ == null)
			{
				DefaultValue = new NullableString();
			}
			DefaultValue.MergeFrom(other.DefaultValue);
		}
		if (other.comment_ != null)
		{
			if (comment_ == null)
			{
				Comment = new NullableString();
			}
			Comment.MergeFrom(other.Comment);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
			case 18u:
				if (fontFamily_ == null)
				{
					FontFamily = new NullableString();
				}
				input.ReadMessage(FontFamily);
				break;
			case 26u:
				if (fontSize_ == null)
				{
					FontSize = new NullableFloat();
				}
				input.ReadMessage(FontSize);
				break;
			case 34u:
				if (foreColor_ == null)
				{
					ForeColor = new NullableInt32();
				}
				input.ReadMessage(ForeColor);
				break;
			case 42u:
				if (backColor_ == null)
				{
					BackColor = new NullableInt32();
				}
				input.ReadMessage(BackColor);
				break;
			case 50u:
				if (align_ == null)
				{
					Align = new NullableInt32();
				}
				input.ReadMessage(Align);
				break;
			case 58u:
				if (margin_ == null)
				{
					Margin = new NullableInt32();
				}
				input.ReadMessage(Margin);
				break;
			case 66u:
				if (bold_ == null)
				{
					Bold = new NullableBool();
				}
				input.ReadMessage(Bold);
				break;
			case 74u:
				if (italic_ == null)
				{
					Italic = new NullableBool();
				}
				input.ReadMessage(Italic);
				break;
			case 82u:
				if (underline_ == null)
				{
					Underline = new NullableBool();
				}
				input.ReadMessage(Underline);
				break;
			case 90u:
				if (dataType_ == null)
				{
					DataType = new NullableInt32();
				}
				input.ReadMessage(DataType);
				break;
			case 98u:
				if (format_ == null)
				{
					Format = new NullableString();
				}
				input.ReadMessage(Format);
				break;
			case 106u:
				if (locker_ == null)
				{
					Locker = new NullableInt64();
				}
				input.ReadMessage(Locker);
				break;
			case 114u:
				if (defaultValue_ == null)
				{
					DefaultValue = new NullableString();
				}
				input.ReadMessage(DefaultValue);
				break;
			case 122u:
				if (comment_ == null)
				{
					Comment = new NullableString();
				}
				input.ReadMessage(Comment);
				break;
			}
		}
	}
}
