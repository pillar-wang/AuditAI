using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PushMerge : IMessage<PushMerge>, Google.Protobuf.IMessage, IEquatable<PushMerge>, IDeepCloneable<PushMerge>
{
	private static readonly MessageParser<PushMerge> _parser = new MessageParser<PushMerge>(() => new PushMerge());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int ActionFieldNumber = 2;

	private int action_;

	public const int TopLeftFieldNumber = 3;

	private long topLeft_;

	public const int BottomRightFieldNumber = 4;

	private long bottomRight_;

	[DebuggerNonUserCode]
	public static MessageParser<PushMerge> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushtableReflection.Descriptor.MessageTypes[5];

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
	public long TopLeft
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
	public long BottomRight
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
	public PushMerge()
	{
	}

	[DebuggerNonUserCode]
	public PushMerge(PushMerge other)
		: this()
	{
		id_ = other.id_;
		action_ = other.action_;
		topLeft_ = other.topLeft_;
		bottomRight_ = other.bottomRight_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushMerge Clone()
	{
		return new PushMerge(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushMerge);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushMerge other)
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
		if (TopLeft != other.TopLeft)
		{
			return false;
		}
		if (BottomRight != other.BottomRight)
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
		if (TopLeft != 0L)
		{
			num ^= TopLeft.GetHashCode();
		}
		if (BottomRight != 0L)
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
		if (Action != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Action);
		}
		if (TopLeft != 0L)
		{
			output.WriteRawTag(24);
			output.WriteInt64(TopLeft);
		}
		if (BottomRight != 0L)
		{
			output.WriteRawTag(32);
			output.WriteInt64(BottomRight);
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
		if (TopLeft != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(TopLeft);
		}
		if (BottomRight != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(BottomRight);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushMerge other)
	{
		if (other != null)
		{
			if (other.Id != 0L)
			{
				Id = other.Id;
			}
			if (other.Action != 0)
			{
				Action = other.Action;
			}
			if (other.TopLeft != 0L)
			{
				TopLeft = other.TopLeft;
			}
			if (other.BottomRight != 0L)
			{
				BottomRight = other.BottomRight;
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
				Action = input.ReadInt32();
				break;
			case 24u:
				TopLeft = input.ReadInt64();
				break;
			case 32u:
				BottomRight = input.ReadInt64();
				break;
			}
		}
	}
}
