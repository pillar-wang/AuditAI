using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class NullableBool : IMessage<NullableBool>, Google.Protobuf.IMessage, IEquatable<NullableBool>, IDeepCloneable<NullableBool>
{
	private static readonly MessageParser<NullableBool> _parser = new MessageParser<NullableBool>(() => new NullableBool());

	private UnknownFieldSet _unknownFields;

	public const int IsNullFieldNumber = 1;

	private bool isNull_;

	public const int ValueFieldNumber = 2;

	private bool value_;

	[DebuggerNonUserCode]
	public static MessageParser<NullableBool> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[6];

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
	public bool Value
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
	public NullableBool()
	{
	}

	[DebuggerNonUserCode]
	public NullableBool(NullableBool other)
		: this()
	{
		isNull_ = other.isNull_;
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public NullableBool Clone()
	{
		return new NullableBool(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as NullableBool);
	}

	[DebuggerNonUserCode]
	public bool Equals(NullableBool other)
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
		if (Value)
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
		if (Value)
		{
			output.WriteRawTag(16);
			output.WriteBool(Value);
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
		if (Value)
		{
			num += 2;
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(NullableBool other)
	{
		if (other != null)
		{
			if (other.IsNull)
			{
				IsNull = other.IsNull;
			}
			if (other.Value)
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
				Value = input.ReadBool();
				break;
			}
		}
	}
}
