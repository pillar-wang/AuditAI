using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class OptionalBytes : IMessage<OptionalBytes>, Google.Protobuf.IMessage, IEquatable<OptionalBytes>, IDeepCloneable<OptionalBytes>
{
	private static readonly MessageParser<OptionalBytes> _parser = new MessageParser<OptionalBytes>(() => new OptionalBytes());

	private UnknownFieldSet _unknownFields;

	public const int ValueFieldNumber = 1;

	private ByteString value_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<OptionalBytes> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[4];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

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
	public OptionalBytes()
	{
	}

	[DebuggerNonUserCode]
	public OptionalBytes(OptionalBytes other)
		: this()
	{
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public OptionalBytes Clone()
	{
		return new OptionalBytes(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as OptionalBytes);
	}

	[DebuggerNonUserCode]
	public bool Equals(OptionalBytes other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
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
		if (Value.Length != 0)
		{
			output.WriteRawTag(10);
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
	public void MergeFrom(OptionalBytes other)
	{
		if (other != null)
		{
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
			if (num != 10)
			{
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
			}
			else
			{
				Value = input.ReadBytes();
			}
		}
	}
}
