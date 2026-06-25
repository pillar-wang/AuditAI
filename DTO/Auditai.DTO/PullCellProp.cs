using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullCellProp : IMessage<PullCellProp>, Google.Protobuf.IMessage, IEquatable<PullCellProp>, IDeepCloneable<PullCellProp>
{
	private static readonly MessageParser<PullCellProp> _parser = new MessageParser<PullCellProp>(() => new PullCellProp());

	private UnknownFieldSet _unknownFields;

	public const int CellIdFieldNumber = 1;

	private long cellId_;

	public const int AttachmentsFieldNumber = 2;

	private OptionalBytes attachments_;

	[DebuggerNonUserCode]
	public static MessageParser<PullCellProp> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[18];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long CellId
	{
		get
		{
			return cellId_;
		}
		set
		{
			cellId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalBytes Attachments
	{
		get
		{
			return attachments_;
		}
		set
		{
			attachments_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PullCellProp()
	{
	}

	[DebuggerNonUserCode]
	public PullCellProp(PullCellProp other)
		: this()
	{
		cellId_ = other.cellId_;
		attachments_ = ((other.attachments_ != null) ? other.attachments_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullCellProp Clone()
	{
		return new PullCellProp(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullCellProp);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullCellProp other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (CellId != other.CellId)
		{
			return false;
		}
		if (!object.Equals(Attachments, other.Attachments))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (CellId != 0L)
		{
			num ^= CellId.GetHashCode();
		}
		if (attachments_ != null)
		{
			num ^= Attachments.GetHashCode();
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
		if (CellId != 0L)
		{
			output.WriteRawTag(8);
			output.WriteInt64(CellId);
		}
		if (attachments_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Attachments);
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
		if (CellId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CellId);
		}
		if (attachments_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Attachments);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PullCellProp other)
	{
		if (other == null)
		{
			return;
		}
		if (other.CellId != 0L)
		{
			CellId = other.CellId;
		}
		if (other.attachments_ != null)
		{
			if (attachments_ == null)
			{
				Attachments = new OptionalBytes();
			}
			Attachments.MergeFrom(other.Attachments);
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
				CellId = input.ReadInt64();
				break;
			case 18u:
				if (attachments_ == null)
				{
					Attachments = new OptionalBytes();
				}
				input.ReadMessage(Attachments);
				break;
			}
		}
	}
}
