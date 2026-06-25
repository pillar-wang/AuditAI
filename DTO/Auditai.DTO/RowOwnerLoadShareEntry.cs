using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class RowOwnerLoadShareEntry : IMessage<RowOwnerLoadShareEntry>, Google.Protobuf.IMessage, IEquatable<RowOwnerLoadShareEntry>, IDeepCloneable<RowOwnerLoadShareEntry>
{
	private static readonly MessageParser<RowOwnerLoadShareEntry> _parser = new MessageParser<RowOwnerLoadShareEntry>(() => new RowOwnerLoadShareEntry());

	private UnknownFieldSet _unknownFields;

	public const int CreatorFieldNumber = 1;

	private long creator_;

	public const int SharedFieldNumber = 2;

	private static readonly FieldCodec<long> _repeated_shared_codec = FieldCodec.ForInt64(18u);

	private readonly RepeatedField<long> shared_ = new RepeatedField<long>();

	[DebuggerNonUserCode]
	public static MessageParser<RowOwnerLoadShareEntry> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RowownerloadshareReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long Creator
	{
		get
		{
			return creator_;
		}
		set
		{
			creator_ = value;
		}
	}

	[DebuggerNonUserCode]
	public RepeatedField<long> Shared => shared_;

	[DebuggerNonUserCode]
	public RowOwnerLoadShareEntry()
	{
	}

	[DebuggerNonUserCode]
	public RowOwnerLoadShareEntry(RowOwnerLoadShareEntry other)
		: this()
	{
		creator_ = other.creator_;
		shared_ = other.shared_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RowOwnerLoadShareEntry Clone()
	{
		return new RowOwnerLoadShareEntry(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RowOwnerLoadShareEntry);
	}

	[DebuggerNonUserCode]
	public bool Equals(RowOwnerLoadShareEntry other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Creator != other.Creator)
		{
			return false;
		}
		if (!shared_.Equals(other.shared_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Creator != 0L)
		{
			num ^= Creator.GetHashCode();
		}
		num ^= shared_.GetHashCode();
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
		if (Creator != 0L)
		{
			output.WriteRawTag(8);
			output.WriteInt64(Creator);
		}
		shared_.WriteTo(output, _repeated_shared_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		if (Creator != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Creator);
		}
		num += shared_.CalculateSize(_repeated_shared_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RowOwnerLoadShareEntry other)
	{
		if (other != null)
		{
			if (other.Creator != 0L)
			{
				Creator = other.Creator;
			}
			shared_.Add(other.shared_);
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
				Creator = input.ReadInt64();
				break;
			case 16u:
			case 18u:
				shared_.AddEntriesFrom(input, _repeated_shared_codec);
				break;
			}
		}
	}
}
