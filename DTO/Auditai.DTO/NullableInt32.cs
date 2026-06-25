using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class NullableInt32 : IMessage<NullableInt32>, Google.Protobuf.IMessage, IEquatable<NullableInt32>, IDeepCloneable<NullableInt32>
{
	private static readonly MessageParser<NullableInt32> _parser = new MessageParser<NullableInt32>(() => new NullableInt32());

	private UnknownFieldSet _unknownFields;

	public const int IsNullFieldNumber = 1;

	private bool isNull_;

	public const int ValueFieldNumber = 2;

	private int value_;

	[DebuggerNonUserCode]
	public static MessageParser<NullableInt32> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[7];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public bool IsNull
	{
		get
		{
			return isNull_;
		}
		set
		{
			isNull_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableInt32()
	{
	}

	[DebuggerNonUserCode]
	public NullableInt32(NullableInt32 other)
		: this()
	{
		isNull_ = other.isNull_;
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public NullableInt32 Clone()
	{
		return new NullableInt32(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as NullableInt32);
	}

	[DebuggerNonUserCode]
	public bool Equals(NullableInt32 other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (IsNull != other.IsNull)
		{
			return false;
		}
		if (Value != other.Value)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (IsNull)
		{
			num ^= IsNull.GetHashCode();
		}
		if (Value != 0)
		{
			num ^= Value.GetHashCode();
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
		if (IsNull)
		{
			output.WriteRawTag(8);
			output.WriteBool(IsNull);
		}
		if (Value != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Value);
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
		if (IsNull)
		{
			num += 2;
		}
		if (Value != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Value);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(NullableInt32 other)
	{
		if (other != null)
		{
			if (other.IsNull)
			{
				IsNull = other.IsNull;
			}
			if (other.Value != 0)
			{
				Value = other.Value;
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
				IsNull = input.ReadBool();
				break;
			case 16u:
				Value = input.ReadInt32();
				break;
			}
		}
	}
}
