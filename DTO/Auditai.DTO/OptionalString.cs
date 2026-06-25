using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class OptionalString : IMessage<OptionalString>, Google.Protobuf.IMessage, IEquatable<OptionalString>, IDeepCloneable<OptionalString>
{
	private static readonly MessageParser<OptionalString> _parser = new MessageParser<OptionalString>(() => new OptionalString());

	private UnknownFieldSet _unknownFields;

	public const int ValueFieldNumber = 1;

	private string value_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<OptionalString> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[5];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public string Value
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
	public OptionalString()
	{
	}

	[DebuggerNonUserCode]
	public OptionalString(OptionalString other)
		: this()
	{
		value_ = other.value_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public OptionalString Clone()
	{
		return new OptionalString(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as OptionalString);
	}

	[DebuggerNonUserCode]
	public bool Equals(OptionalString other)
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
			output.WriteString(Value);
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
			num += 1 + CodedOutputStream.ComputeStringSize(Value);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(OptionalString other)
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
				Value = input.ReadString();
			}
		}
	}
}
