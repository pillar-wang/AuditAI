using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullParagraph : IMessage<PullParagraph>, Google.Protobuf.IMessage, IEquatable<PullParagraph>, IDeepCloneable<PullParagraph>
{
	private static readonly MessageParser<PullParagraph> _parser = new MessageParser<PullParagraph>(() => new PullParagraph());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int IndexFieldNumber = 2;

	private OptionalInt32 index_;

	public const int StreamFieldNumber = 3;

	private OptionalBytes stream_;

	public const int SectionFieldNumber = 4;

	private NullableBytes section_;

	public const int CommentFieldNumber = 5;

	private OptionalString comment_;

	[DebuggerNonUserCode]
	public static MessageParser<PullParagraph> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PullDocumentReflection.Descriptor.MessageTypes[1];

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
	public OptionalInt32 Index
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
	public OptionalBytes Stream
	{
		get
		{
			return stream_;
		}
		set
		{
			stream_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableBytes Section
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
	public OptionalString Comment
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
	public PullParagraph()
	{
	}

	[DebuggerNonUserCode]
	public PullParagraph(PullParagraph other)
		: this()
	{
		id_ = other.id_;
		index_ = ((other.index_ != null) ? other.index_.Clone() : null);
		stream_ = ((other.stream_ != null) ? other.stream_.Clone() : null);
		section_ = ((other.section_ != null) ? other.section_.Clone() : null);
		comment_ = ((other.comment_ != null) ? other.comment_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullParagraph Clone()
	{
		return new PullParagraph(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullParagraph);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullParagraph other)
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
		if (!object.Equals(Index, other.Index))
		{
			return false;
		}
		if (!object.Equals(Stream, other.Stream))
		{
			return false;
		}
		if (!object.Equals(Section, other.Section))
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
		if (index_ != null)
		{
			num ^= Index.GetHashCode();
		}
		if (stream_ != null)
		{
			num ^= Stream.GetHashCode();
		}
		if (section_ != null)
		{
			num ^= Section.GetHashCode();
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
		if (index_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Index);
		}
		if (stream_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Stream);
		}
		if (section_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Section);
		}
		if (comment_ != null)
		{
			output.WriteRawTag(42);
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
		if (index_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Index);
		}
		if (stream_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Stream);
		}
		if (section_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Section);
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
	public void MergeFrom(PullParagraph other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0L)
		{
			Id = other.Id;
		}
		if (other.index_ != null)
		{
			if (index_ == null)
			{
				Index = new OptionalInt32();
			}
			Index.MergeFrom(other.Index);
		}
		if (other.stream_ != null)
		{
			if (stream_ == null)
			{
				Stream = new OptionalBytes();
			}
			Stream.MergeFrom(other.Stream);
		}
		if (other.section_ != null)
		{
			if (section_ == null)
			{
				Section = new NullableBytes();
			}
			Section.MergeFrom(other.Section);
		}
		if (other.comment_ != null)
		{
			if (comment_ == null)
			{
				Comment = new OptionalString();
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
				if (index_ == null)
				{
					Index = new OptionalInt32();
				}
				input.ReadMessage(Index);
				break;
			case 26u:
				if (stream_ == null)
				{
					Stream = new OptionalBytes();
				}
				input.ReadMessage(Stream);
				break;
			case 34u:
				if (section_ == null)
				{
					Section = new NullableBytes();
				}
				input.ReadMessage(Section);
				break;
			case 42u:
				if (comment_ == null)
				{
					Comment = new OptionalString();
				}
				input.ReadMessage(Comment);
				break;
			}
		}
	}
}
