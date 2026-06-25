using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullRow : IMessage<PullRow>, Google.Protobuf.IMessage, IEquatable<PullRow>, IDeepCloneable<PullRow>
{
	private static readonly MessageParser<PullRow> _parser = new MessageParser<PullRow>(() => new PullRow());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int IndexFieldNumber = 2;

	private OptionalInt32 index_;

	public const int HeightFieldNumber = 3;

	private OptionalInt32 height_;

	public const int VisibleFieldNumber = 4;

	private OptionalBool visible_;

	public const int LockerFieldNumber = 5;

	private OptionalInt64 locker_;

	public const int RoleFieldNumber = 6;

	private OptionalInt32 role_;

	public const int PermissionsFieldNumber = 7;

	private OptionalString permissions_;

	public const int CreatorFieldNumber = 8;

	private OptionalInt64 creator_;

	[DebuggerNonUserCode]
	public static MessageParser<PullRow> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[14];

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
	public OptionalInt32 Index
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
	public OptionalInt32 Height
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
	public OptionalBool Visible
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
	public OptionalInt64 Locker
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
	public OptionalInt32 Role
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
	public OptionalString Permissions
	{
		get
		{
			return permissions_;
		}
		set
		{
			permissions_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalInt64 Creator
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
	public PullRow()
	{
	}

	[DebuggerNonUserCode]
	public PullRow(PullRow other)
		: this()
	{
		id_ = other.id_;
		index_ = ((other.index_ != null) ? other.index_.Clone() : null);
		height_ = ((other.height_ != null) ? other.height_.Clone() : null);
		visible_ = ((other.visible_ != null) ? other.visible_.Clone() : null);
		locker_ = ((other.locker_ != null) ? other.locker_.Clone() : null);
		role_ = ((other.role_ != null) ? other.role_.Clone() : null);
		permissions_ = ((other.permissions_ != null) ? other.permissions_.Clone() : null);
		creator_ = ((other.creator_ != null) ? other.creator_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullRow Clone()
	{
		return new PullRow(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullRow);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullRow other)
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
		if (!object.Equals(Index, other.Index))
		{
			return false;
		}
		if (!object.Equals(Height, other.Height))
		{
			return false;
		}
		if (!object.Equals(Visible, other.Visible))
		{
			return false;
		}
		if (!object.Equals(Locker, other.Locker))
		{
			return false;
		}
		if (!object.Equals(Role, other.Role))
		{
			return false;
		}
		if (!object.Equals(Permissions, other.Permissions))
		{
			return false;
		}
		if (!object.Equals(Creator, other.Creator))
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
		if (index_ != null)
		{
			num ^= Index.GetHashCode();
		}
		if (height_ != null)
		{
			num ^= Height.GetHashCode();
		}
		if (visible_ != null)
		{
			num ^= Visible.GetHashCode();
		}
		if (locker_ != null)
		{
			num ^= Locker.GetHashCode();
		}
		if (role_ != null)
		{
			num ^= Role.GetHashCode();
		}
		if (permissions_ != null)
		{
			num ^= Permissions.GetHashCode();
		}
		if (creator_ != null)
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
		if (index_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Index);
		}
		if (height_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Height);
		}
		if (visible_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Visible);
		}
		if (locker_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(Locker);
		}
		if (role_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(Role);
		}
		if (permissions_ != null)
		{
			output.WriteRawTag(58);
			output.WriteMessage(Permissions);
		}
		if (creator_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(Creator);
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
		if (index_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Index);
		}
		if (height_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Height);
		}
		if (visible_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Visible);
		}
		if (locker_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Locker);
		}
		if (role_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Role);
		}
		if (permissions_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Permissions);
		}
		if (creator_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Creator);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PullRow other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0L)
		{
			Id = other.Id;
		}
		if (other.index_ != null)
		{
			if (index_ == null)
			{
				Index = new OptionalInt32();
			}
			Index.MergeFrom(other.Index);
		}
		if (other.height_ != null)
		{
			if (height_ == null)
			{
				Height = new OptionalInt32();
			}
			Height.MergeFrom(other.Height);
		}
		if (other.visible_ != null)
		{
			if (visible_ == null)
			{
				Visible = new OptionalBool();
			}
			Visible.MergeFrom(other.Visible);
		}
		if (other.locker_ != null)
		{
			if (locker_ == null)
			{
				Locker = new OptionalInt64();
			}
			Locker.MergeFrom(other.Locker);
		}
		if (other.role_ != null)
		{
			if (role_ == null)
			{
				Role = new OptionalInt32();
			}
			Role.MergeFrom(other.Role);
		}
		if (other.permissions_ != null)
		{
			if (permissions_ == null)
			{
				Permissions = new OptionalString();
			}
			Permissions.MergeFrom(other.Permissions);
		}
		if (other.creator_ != null)
		{
			if (creator_ == null)
			{
				Creator = new OptionalInt64();
			}
			Creator.MergeFrom(other.Creator);
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
				Id = input.ReadInt64();
				break;
			case 18u:
				if (index_ == null)
				{
					Index = new OptionalInt32();
				}
				input.ReadMessage(Index);
				break;
			case 26u:
				if (height_ == null)
				{
					Height = new OptionalInt32();
				}
				input.ReadMessage(Height);
				break;
			case 34u:
				if (visible_ == null)
				{
					Visible = new OptionalBool();
				}
				input.ReadMessage(Visible);
				break;
			case 42u:
				if (locker_ == null)
				{
					Locker = new OptionalInt64();
				}
				input.ReadMessage(Locker);
				break;
			case 50u:
				if (role_ == null)
				{
					Role = new OptionalInt32();
				}
				input.ReadMessage(Role);
				break;
			case 58u:
				if (permissions_ == null)
				{
					Permissions = new OptionalString();
				}
				input.ReadMessage(Permissions);
				break;
			case 66u:
				if (creator_ == null)
				{
					Creator = new OptionalInt64();
				}
				input.ReadMessage(Creator);
				break;
			}
		}
	}
}
