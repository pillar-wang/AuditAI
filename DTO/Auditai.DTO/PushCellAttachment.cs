using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PushCellAttachment : IMessage<PushCellAttachment>, Google.Protobuf.IMessage, IEquatable<PushCellAttachment>, IDeepCloneable<PushCellAttachment>
{
	private static readonly MessageParser<PushCellAttachment> _parser = new MessageParser<PushCellAttachment>(() => new PushCellAttachment());

	private UnknownFieldSet _unknownFields;

	public const int TableIdFieldNumber = 1;

	private long tableId_;

	public const int CellIdFieldNumber = 2;

	private long cellId_;

	public const int ActionFieldNumber = 3;

	private int action_;

	public const int MaskFieldNumber = 4;

	private int mask_;

	public const int AttachmentsFieldNumber = 5;

	private ByteString attachments_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<PushCellAttachment> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushtableReflection.Descriptor.MessageTypes[6];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long TableId
	{
		get
		{
			return tableId_;
		}
		set
		{
			tableId_ = value;
		}
	}

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
	public int Mask
	{
		get
		{
			return mask_;
		}
		set
		{
			mask_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString Attachments
	{
		get
		{
			return attachments_;
		}
		set
		{
			attachments_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PushCellAttachment()
	{
	}

	[DebuggerNonUserCode]
	public PushCellAttachment(PushCellAttachment other)
		: this()
	{
		tableId_ = other.tableId_;
		cellId_ = other.cellId_;
		action_ = other.action_;
		mask_ = other.mask_;
		attachments_ = other.attachments_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushCellAttachment Clone()
	{
		return new PushCellAttachment(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushCellAttachment);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushCellAttachment other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (TableId != other.TableId)
		{
			return false;
		}
		if (CellId != other.CellId)
		{
			return false;
		}
		if (Action != other.Action)
		{
			return false;
		}
		if (Mask != other.Mask)
		{
			return false;
		}
		if (Attachments != other.Attachments)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (TableId != 0L)
		{
			num ^= TableId.GetHashCode();
		}
		if (CellId != 0L)
		{
			num ^= CellId.GetHashCode();
		}
		if (Action != 0)
		{
			num ^= Action.GetHashCode();
		}
		if (Mask != 0)
		{
			num ^= Mask.GetHashCode();
		}
		if (Attachments.Length != 0)
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
		if (TableId != 0L)
		{
			output.WriteRawTag(8);
			output.WriteInt64(TableId);
		}
		if (CellId != 0L)
		{
			output.WriteRawTag(16);
			output.WriteInt64(CellId);
		}
		if (Action != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Action);
		}
		if (Mask != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(Mask);
		}
		if (Attachments.Length != 0)
		{
			output.WriteRawTag(42);
			output.WriteBytes(Attachments);
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
		if (TableId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(TableId);
		}
		if (CellId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CellId);
		}
		if (Action != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Action);
		}
		if (Mask != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Mask);
		}
		if (Attachments.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Attachments);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushCellAttachment other)
	{
		if (other != null)
		{
			if (other.TableId != 0L)
			{
				TableId = other.TableId;
			}
			if (other.CellId != 0L)
			{
				CellId = other.CellId;
			}
			if (other.Action != 0)
			{
				Action = other.Action;
			}
			if (other.Mask != 0)
			{
				Mask = other.Mask;
			}
			if (other.Attachments.Length != 0)
			{
				Attachments = other.Attachments;
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
				TableId = input.ReadInt64();
				break;
			case 16u:
				CellId = input.ReadInt64();
				break;
			case 24u:
				Action = input.ReadInt32();
				break;
			case 32u:
				Mask = input.ReadInt32();
				break;
			case 42u:
				Attachments = input.ReadBytes();
				break;
			}
		}
	}
}
