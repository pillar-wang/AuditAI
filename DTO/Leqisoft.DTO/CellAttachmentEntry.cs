using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public sealed class CellAttachmentEntry : IMessage<CellAttachmentEntry>, Google.Protobuf.IMessage, IEquatable<CellAttachmentEntry>, IDeepCloneable<CellAttachmentEntry>
{
	private static readonly MessageParser<CellAttachmentEntry> _parser = new MessageParser<CellAttachmentEntry>(() => new CellAttachmentEntry());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private ByteString id_ = ByteString.Empty;

	public const int NameFieldNumber = 2;

	private string name_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<CellAttachmentEntry> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => CellattachmentsReflection.Descriptor.MessageTypes[1];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public ByteString Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Name
	{
		get
		{
			return name_;
		}
		set
		{
			name_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public CellAttachmentEntry()
	{
	}

	[DebuggerNonUserCode]
	public CellAttachmentEntry(CellAttachmentEntry other)
		: this()
	{
		id_ = other.id_;
		name_ = other.name_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CellAttachmentEntry Clone()
	{
		return new CellAttachmentEntry(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CellAttachmentEntry);
	}

	[DebuggerNonUserCode]
	public bool Equals(CellAttachmentEntry other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (Name != other.Name)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Id.Length != 0)
		{
			num ^= Id.GetHashCode();
		}
		if (Name.Length != 0)
		{
			num ^= Name.GetHashCode();
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
		if (Id.Length != 0)
		{
			output.WriteRawTag(10);
			output.WriteBytes(Id);
		}
		if (Name.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Name);
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
		if (Id.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Id);
		}
		if (Name.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Name);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CellAttachmentEntry other)
	{
		if (other != null)
		{
			if (other.Id.Length != 0)
			{
				Id = other.Id;
			}
			if (other.Name.Length != 0)
			{
				Name = other.Name;
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
			case 10u:
				Id = input.ReadBytes();
				break;
			case 18u:
				Name = input.ReadString();
				break;
			}
		}
	}
}
