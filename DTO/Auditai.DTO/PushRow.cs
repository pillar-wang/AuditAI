using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PushRow : IMessage<PushRow>, Google.Protobuf.IMessage, IEquatable<PushRow>, IDeepCloneable<PushRow>
{
	private static readonly MessageParser<PushRow> _parser = new MessageParser<PushRow>(() => new PushRow());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int ActionFieldNumber = 2;

	private int action_;

	public const int MaskFieldNumber = 3;

	private int mask_;

	public const int HeightFieldNumber = 4;

	private int height_;

	public const int IndexFieldNumber = 5;

	private int index_;

	public const int VisibleFieldNumber = 16;

	private bool visible_;

	public const int LockerFieldNumber = 17;

	private long locker_;

	public const int RoleFieldNumber = 18;

	private int role_;

	public const int PermissionsFieldNumber = 19;

	private string permissions_ = "";

	public const int CreatorFieldNumber = 20;

	private long creator_;

	[DebuggerNonUserCode]
	public static MessageParser<PushRow> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushtableReflection.Descriptor.MessageTypes[2];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public long Id
	{
		get
		{
			return id_;
		}
		set
		{
			id_ = value;
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
	public int Height
	{
		get
		{
			return height_;
		}
		set
		{
			height_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int Index
	{
		get
		{
			return index_;
		}
		set
		{
			index_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool Visible
	{
		get
		{
			return visible_;
		}
		set
		{
			visible_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long Locker
	{
		get
		{
			return locker_;
		}
		set
		{
			locker_ = value;
		}
	}

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
	public string Permissions
	{
		get
		{
			return permissions_;
		}
		set
		{
			permissions_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

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
	public PushRow()
	{
	}

	[DebuggerNonUserCode]
	public PushRow(PushRow other)
		: this()
	{
		id_ = other.id_;
		action_ = other.action_;
		mask_ = other.mask_;
		height_ = other.height_;
		index_ = other.index_;
		visible_ = other.visible_;
		locker_ = other.locker_;
		role_ = other.role_;
		permissions_ = other.permissions_;
		creator_ = other.creator_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushRow Clone()
	{
		return new PushRow(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushRow);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushRow other)
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
		if (Action != other.Action)
		{
			return false;
		}
		if (Mask != other.Mask)
		{
			return false;
		}
		if (Height != other.Height)
		{
			return false;
		}
		if (Index != other.Index)
		{
			return false;
		}
		if (Visible != other.Visible)
		{
			return false;
		}
		if (Locker != other.Locker)
		{
			return false;
		}
		if (Role != other.Role)
		{
			return false;
		}
		if (Permissions != other.Permissions)
		{
			return false;
		}
		if (Creator != other.Creator)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		if (Id != 0L)
		{
			num ^= Id.GetHashCode();
		}
		if (Action != 0)
		{
			num ^= Action.GetHashCode();
		}
		if (Mask != 0)
		{
			num ^= Mask.GetHashCode();
		}
		if (Height != 0)
		{
			num ^= Height.GetHashCode();
		}
		if (Index != 0)
		{
			num ^= Index.GetHashCode();
		}
		if (Visible)
		{
			num ^= Visible.GetHashCode();
		}
		if (Locker != 0L)
		{
			num ^= Locker.GetHashCode();
		}
		if (Role != 0)
		{
			num ^= Role.GetHashCode();
		}
		if (Permissions.Length != 0)
		{
			num ^= Permissions.GetHashCode();
		}
		if (Creator != 0L)
		{
			num ^= Creator.GetHashCode();
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
		if (Id != 0L)
		{
			output.WriteRawTag(8);
			output.WriteInt64(Id);
		}
		if (Action != 0)
		{
			output.WriteRawTag(16);
			output.WriteInt32(Action);
		}
		if (Mask != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Mask);
		}
		if (Height != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(Height);
		}
		if (Index != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(Index);
		}
		if (Visible)
		{
			output.WriteRawTag(128, 1);
			output.WriteBool(Visible);
		}
		if (Locker != 0L)
		{
			output.WriteRawTag(136, 1);
			output.WriteInt64(Locker);
		}
		if (Role != 0)
		{
			output.WriteRawTag(144, 1);
			output.WriteInt32(Role);
		}
		if (Permissions.Length != 0)
		{
			output.WriteRawTag(154, 1);
			output.WriteString(Permissions);
		}
		if (Creator != 0L)
		{
			output.WriteRawTag(160, 1);
			output.WriteInt64(Creator);
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
		if (Id != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(Id);
		}
		if (Action != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Action);
		}
		if (Mask != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Mask);
		}
		if (Height != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Height);
		}
		if (Index != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Index);
		}
		if (Visible)
		{
			num += 3;
		}
		if (Locker != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(Locker);
		}
		if (Role != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(Role);
		}
		if (Permissions.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Permissions);
		}
		if (Creator != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(Creator);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushRow other)
	{
		if (other != null)
		{
			if (other.Id != 0L)
			{
				Id = other.Id;
			}
			if (other.Action != 0)
			{
				Action = other.Action;
			}
			if (other.Mask != 0)
			{
				Mask = other.Mask;
			}
			if (other.Height != 0)
			{
				Height = other.Height;
			}
			if (other.Index != 0)
			{
				Index = other.Index;
			}
			if (other.Visible)
			{
				Visible = other.Visible;
			}
			if (other.Locker != 0L)
			{
				Locker = other.Locker;
			}
			if (other.Role != 0)
			{
				Role = other.Role;
			}
			if (other.Permissions.Length != 0)
			{
				Permissions = other.Permissions;
			}
			if (other.Creator != 0L)
			{
				Creator = other.Creator;
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
				Id = input.ReadInt64();
				break;
			case 16u:
				Action = input.ReadInt32();
				break;
			case 24u:
				Mask = input.ReadInt32();
				break;
			case 32u:
				Height = input.ReadInt32();
				break;
			case 40u:
				Index = input.ReadInt32();
				break;
			case 128u:
				Visible = input.ReadBool();
				break;
			case 136u:
				Locker = input.ReadInt64();
				break;
			case 144u:
				Role = input.ReadInt32();
				break;
			case 154u:
				Permissions = input.ReadString();
				break;
			case 160u:
				Creator = input.ReadInt64();
				break;
			}
		}
	}
}
