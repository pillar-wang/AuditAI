using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public sealed class CellAttachments : IMessage<CellAttachments>, Google.Protobuf.IMessage, IEquatable<CellAttachments>, IDeepCloneable<CellAttachments>
{
	private static readonly MessageParser<CellAttachments> _parser = new MessageParser<CellAttachments>(() => new CellAttachments());

	private UnknownFieldSet _unknownFields;

	public const int EntriesFieldNumber = 1;

	private static readonly FieldCodec<CellAttachmentEntry> _repeated_entries_codec = FieldCodec.ForMessage(10u, CellAttachmentEntry.Parser);

	private readonly RepeatedField<CellAttachmentEntry> entries_ = new RepeatedField<CellAttachmentEntry>();

	[DebuggerNonUserCode]
	public static MessageParser<CellAttachments> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => CellattachmentsReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<CellAttachmentEntry> Entries => entries_;

	[DebuggerNonUserCode]
	public CellAttachments()
	{
	}

	[DebuggerNonUserCode]
	public CellAttachments(CellAttachments other)
		: this()
	{
		entries_ = other.entries_.Clone();
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CellAttachments Clone()
	{
		return new CellAttachments(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CellAttachments);
	}

	[DebuggerNonUserCode]
	public bool Equals(CellAttachments other)
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
	public void MergeFrom(CellAttachments other)
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
