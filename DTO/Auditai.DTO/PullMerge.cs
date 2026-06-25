using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullMerge : IMessage<PullMerge>, Google.Protobuf.IMessage, IEquatable<PullMerge>, IDeepCloneable<PullMerge>
{
	private static readonly MessageParser<PullMerge> _parser = new MessageParser<PullMerge>(() => new PullMerge());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int TopLeftFieldNumber = 2;

	private OptionalInt64 topLeft_;

	public const int BottomRightFieldNumber = 3;

	private OptionalInt64 bottomRight_;

	[DebuggerNonUserCode]
	public static MessageParser<PullMerge> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[17];

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
	public OptionalInt64 TopLeft
	{
		get
		{
			return topLeft_;
		}
		set
		{
			topLeft_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalInt64 BottomRight
	{
		get
		{
			return bottomRight_;
		}
		set
		{
			bottomRight_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PullMerge()
	{
	}

	[DebuggerNonUserCode]
	public PullMerge(PullMerge other)
		: this()
	{
		id_ = other.id_;
		topLeft_ = ((other.topLeft_ != null) ? other.topLeft_.Clone() : null);
		bottomRight_ = ((other.bottomRight_ != null) ? other.bottomRight_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullMerge Clone()
	{
		return new PullMerge(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullMerge);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullMerge other)
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
		if (!object.Equals(TopLeft, other.TopLeft))
		{
			return false;
		}
		if (!object.Equals(BottomRight, other.BottomRight))
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
		if (topLeft_ != null)
		{
			num ^= TopLeft.GetHashCode();
		}
		if (bottomRight_ != null)
		{
			num ^= BottomRight.GetHashCode();
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
		if (topLeft_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(TopLeft);
		}
		if (bottomRight_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(BottomRight);
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
		if (topLeft_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(TopLeft);
		}
		if (bottomRight_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(BottomRight);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PullMerge other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0L)
		{
			Id = other.Id;
		}
		if (other.topLeft_ != null)
		{
			if (topLeft_ == null)
			{
				TopLeft = new OptionalInt64();
			}
			TopLeft.MergeFrom(other.TopLeft);
		}
		if (other.bottomRight_ != null)
		{
			if (bottomRight_ == null)
			{
				BottomRight = new OptionalInt64();
			}
			BottomRight.MergeFrom(other.BottomRight);
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
				if (topLeft_ == null)
				{
					TopLeft = new OptionalInt64();
				}
				input.ReadMessage(TopLeft);
				break;
			case 26u:
				if (bottomRight_ == null)
				{
					BottomRight = new OptionalInt64();
				}
				input.ReadMessage(BottomRight);
				break;
			}
		}
	}
}
