using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public sealed class NullableBytes : IMessage<NullableBytes>, Google.Protobuf.IMessage, IEquatable<NullableBytes>, IDeepCloneable<NullableBytes>
{
	private static readonly MessageParser<NullableBytes> _parser = new MessageParser<NullableBytes>(() => new NullableBytes());

	private UnknownFieldSet _unknownFields;

	public const int IsNullFieldNumber = 1;

	private bool isNull_;

	public const int ValueFieldNumber = 2;

	private ByteString value_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<NullableBytes> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[11];

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
	public ByteString Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public NullableBytes()
	{
	}

	[DebuggerNonUserCode]
	public NullableBytes(NullableBytes other)
		: this()
	{
		isNull_ = other.isNull_;
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public NullableBytes Clone()
	{
		return new NullableBytes(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as NullableBytes);
	}

	[DebuggerNonUserCode]
	public bool Equals(NullableBytes other)
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
		if (Value.Length != 0)
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
		if (Value.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(Value);
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
		if (Value.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Value);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(NullableBytes other)
	{
		if (other != null)
		{
			if (other.IsNull)
			{
				IsNull = other.IsNull;
			}
			if (other.Value.Length != 0)
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
			case 18u:
				Value = input.ReadBytes();
				break;
			}
		}
	}
}
