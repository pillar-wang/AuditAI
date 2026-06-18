using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Leqisoft.DTO;

public sealed class PushParagraph : IMessage<PushParagraph>, Google.Protobuf.IMessage, IEquatable<PushParagraph>, IDeepCloneable<PushParagraph>
{
	private static readonly MessageParser<PushParagraph> _parser = new MessageParser<PushParagraph>(() => new PushParagraph());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int ActionFieldNumber = 2;

	private int action_;

	public const int MaskFieldNumber = 3;

	private int mask_;

	public const int IndexFieldNumber = 4;

	private int index_;

	public const int StreamFieldNumber = 5;

	private ByteString stream_ = ByteString.Empty;

	public const int SectionFieldNumber = 6;

	private BytesValue section_;

	public const int CommentFieldNumber = 7;

	private string comment_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PushParagraph> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushdocumentReflection.Descriptor.MessageTypes[1];

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
	public int Action
	{
		get
		{
			return action_;
		}
		set
		{
			action_ = value;
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
	public int Index
	{
		get
		{
			return index_;
		}
		set
		{
			index_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString Stream
	{
		get
		{
			return stream_;
		}
		set
		{
			stream_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public BytesValue Section
	{
		get
		{
			return section_;
		}
		set
		{
			section_ = value;
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
	public PushParagraph()
	{
	}

	[DebuggerNonUserCode]
	public PushParagraph(PushParagraph other)
		: this()
	{
		id_ = other.id_;
		action_ = other.action_;
		mask_ = other.mask_;
		index_ = other.index_;
		stream_ = other.stream_;
		section_ = ((other.section_ != null) ? other.section_.Clone() : null);
		comment_ = other.comment_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushParagraph Clone()
	{
		return new PushParagraph(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushParagraph);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushParagraph other)
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
		if (Action != other.Action)
		{
			return false;
		}
		if (Mask != other.Mask)
		{
			return false;
		}
		if (Index != other.Index)
		{
			return false;
		}
		if (Stream != other.Stream)
		{
			return false;
		}
		if (!object.Equals(Section, other.Section))
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
		if (Action != 0)
		{
			num ^= Action.GetHashCode();
		}
		if (Mask != 0)
		{
			num ^= Mask.GetHashCode();
		}
		if (Index != 0)
		{
			num ^= Index.GetHashCode();
		}
		if (Stream.Length != 0)
		{
			num ^= Stream.GetHashCode();
		}
		if (section_ != null)
		{
			num ^= Section.GetHashCode();
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
		if (Action != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Action);
		}
		if (Mask != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Mask);
		}
		if (Index != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(Index);
		}
		if (Stream.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteBytes(Stream);
		}
		if (section_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(Section);
		}
		if (Comment.Length != 0)
		{
			output.WriteRawTag(58);
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
		if (Action != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Action);
		}
		if (Mask != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Mask);
		}
		if (Index != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Index);
		}
		if (Stream.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Stream);
		}
		if (section_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Section);
		}
		if (Comment.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Comment);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushParagraph other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0L)
		{
			Id = other.Id;
		}
		if (other.Action != 0)
		{
			Action = other.Action;
		}
		if (other.Mask != 0)
		{
			Mask = other.Mask;
		}
		if (other.Index != 0)
		{
			Index = other.Index;
		}
		if (other.Stream.Length != 0)
		{
			Stream = other.Stream;
		}
		if (other.section_ != null)
		{
			if (section_ == null)
			{
				Section = new BytesValue();
			}
			Section.MergeFrom(other.Section);
		}
		if (other.Comment.Length != 0)
		{
			Comment = other.Comment;
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
			case 16u:
				Action = input.ReadInt32();
				break;
			case 24u:
				Mask = input.ReadInt32();
				break;
			case 32u:
				Index = input.ReadInt32();
				break;
			case 42u:
				Stream = input.ReadBytes();
				break;
			case 50u:
				if (section_ == null)
				{
					Section = new BytesValue();
				}
				input.ReadMessage(Section);
				break;
			case 58u:
				Comment = input.ReadString();
				break;
			}
		}
	}
}
