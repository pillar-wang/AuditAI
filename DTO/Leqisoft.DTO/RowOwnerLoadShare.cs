using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public sealed class RowOwnerLoadShare : IMessage<RowOwnerLoadShare>, Google.Protobuf.IMessage, IEquatable<RowOwnerLoadShare>, IDeepCloneable<RowOwnerLoadShare>
{
	private static readonly MessageParser<RowOwnerLoadShare> _parser = new MessageParser<RowOwnerLoadShare>(() => new RowOwnerLoadShare());

	private UnknownFieldSet _unknownFields;

	public const int EntriesFieldNumber = 1;

	private static readonly FieldCodec<RowOwnerLoadShareEntry> _repeated_entries_codec = FieldCodec.ForMessage(10u, RowOwnerLoadShareEntry.Parser);

	private readonly RepeatedField<RowOwnerLoadShareEntry> entries_ = new RepeatedField<RowOwnerLoadShareEntry>();

	[DebuggerNonUserCode]
	public static MessageParser<RowOwnerLoadShare> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => RowownerloadshareReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<RowOwnerLoadShareEntry> Entries => entries_;

	[DebuggerNonUserCode]
	public RowOwnerLoadShare()
	{
	}

	[DebuggerNonUserCode]
	public RowOwnerLoadShare(RowOwnerLoadShare other)
		: this()
	{
		entries_ = other.entries_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public RowOwnerLoadShare Clone()
	{
		return new RowOwnerLoadShare(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as RowOwnerLoadShare);
	}

	[DebuggerNonUserCode]
	public bool Equals(RowOwnerLoadShare other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!entries_.Equals(other.entries_))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= entries_.GetHashCode();
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
		entries_.WriteTo(output, _repeated_entries_codec);
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(output);
		}
	}

	[DebuggerNonUserCode]
	public int CalculateSize()
	{
		int num = 0;
		num += entries_.CalculateSize(_repeated_entries_codec);
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(RowOwnerLoadShare other)
	{
		if (other != null)
		{
			entries_.Add(other.entries_);
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
				entries_.AddEntriesFrom(input, _repeated_entries_codec);
			}
		}
	}
}
