using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public sealed class CrossAttributes : IMessage<CrossAttributes>, Google.Protobuf.IMessage, IEquatable<CrossAttributes>, IDeepCloneable<CrossAttributes>
{
	private static readonly MessageParser<CrossAttributes> _parser = new MessageParser<CrossAttributes>(() => new CrossAttributes());

	private UnknownFieldSet _unknownFields;

	public const int RoleFieldNumber = 1;

	private int role_;

	public const int CaptionFieldNumber = 2;

	private string caption_ = "";

	public const int SrcColumnFieldNumber = 3;

	private long srcColumn_;

	[DebuggerNonUserCode]
	public static MessageParser<CrossAttributes> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => CrossattributesReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public int Role
	{
		get
		{
			return role_;
		}
		set
		{
			role_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Caption
	{
		get
		{
			return caption_;
		}
		set
		{
			caption_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public long SrcColumn
	{
		get
		{
			return srcColumn_;
		}
		set
		{
			srcColumn_ = value;
		}
	}

	[DebuggerNonUserCode]
	public CrossAttributes()
	{
	}

	[DebuggerNonUserCode]
	public CrossAttributes(CrossAttributes other)
		: this()
	{
		role_ = other.role_;
		caption_ = other.caption_;
		srcColumn_ = other.srcColumn_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public CrossAttributes Clone()
	{
		return new CrossAttributes(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as CrossAttributes);
	}

	[DebuggerNonUserCode]
	public bool Equals(CrossAttributes other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Role != other.Role)
		{
			return false;
		}
		if (Caption != other.Caption)
		{
			return false;
		}
		if (SrcColumn != other.SrcColumn)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Role != 0)
		{
			num ^= Role.GetHashCode();
		}
		if (Caption.Length != 0)
		{
			num ^= Caption.GetHashCode();
		}
		if (SrcColumn != 0L)
		{
			num ^= SrcColumn.GetHashCode();
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
		if (Role != 0)
		{
			output.WriteRawTag(8);
			output.WriteInt32(Role);
		}
		if (Caption.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteString(Caption);
		}
		if (SrcColumn != 0L)
		{
			output.WriteRawTag(24);
			output.WriteInt64(SrcColumn);
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
		if (Role != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Role);
		}
		if (Caption.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Caption);
		}
		if (SrcColumn != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(SrcColumn);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(CrossAttributes other)
	{
		if (other != null)
		{
			if (other.Role != 0)
			{
				Role = other.Role;
			}
			if (other.Caption.Length != 0)
			{
				Caption = other.Caption;
			}
			if (other.SrcColumn != 0L)
			{
				SrcColumn = other.SrcColumn;
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
				Role = input.ReadInt32();
				break;
			case 18u:
				Caption = input.ReadString();
				break;
			case 24u:
				SrcColumn = input.ReadInt64();
				break;
			}
		}
	}
}
