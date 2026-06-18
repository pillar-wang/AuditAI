using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Leqisoft.DTO;

public sealed class PushColumn : IMessage<PushColumn>, Google.Protobuf.IMessage, IEquatable<PushColumn>, IDeepCloneable<PushColumn>
{
	private static readonly MessageParser<PushColumn> _parser = new MessageParser<PushColumn>(() => new PushColumn());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int ActionFieldNumber = 2;

	private int action_;

	public const int MaskFieldNumber = 3;

	private int mask_;

	public const int WidthFieldNumber = 4;

	private int width_;

	public const int IndexFieldNumber = 5;

	private int index_;

	public const int CaptionFieldNumber = 6;

	private string caption_ = "";

	public const int FormulaFieldNumber = 7;

	private string formula_ = "";

	public const int VisibleFieldNumber = 16;

	private bool visible_;

	public const int StyleIdFieldNumber = 17;

	private Int64Value styleId_;

	public const int CaptionStyleFieldNumber = 18;

	private string captionStyle_ = "";

	public const int ConsolidateAttribsFieldNumber = 19;

	private string consolidateAttribs_ = "";

	public const int SubtotalAttribsFieldNumber = 20;

	private int subtotalAttribs_;

	public const int PermissionsFieldNumber = 21;

	private string permissions_ = "";

	public const int CaptionFormulaFieldNumber = 22;

	private string captionFormula_ = "";

	public const int CrossAttributesFieldNumber = 23;

	private ByteString crossAttributes_ = ByteString.Empty;

	[DebuggerNonUserCode]
	public static MessageParser<PushColumn> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushtableReflection.Descriptor.MessageTypes[1];

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
	public int Width
	{
		get
		{
			return width_;
		}
		set
		{
			width_ = value;
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
	public string Formula
	{
		get
		{
			return formula_;
		}
		set
		{
			formula_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public Int64Value StyleId
	{
		get
		{
			return styleId_;
		}
		set
		{
			styleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string CaptionStyle
	{
		get
		{
			return captionStyle_;
		}
		set
		{
			captionStyle_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ConsolidateAttribs
	{
		get
		{
			return consolidateAttribs_;
		}
		set
		{
			consolidateAttribs_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int SubtotalAttribs
	{
		get
		{
			return subtotalAttribs_;
		}
		set
		{
			subtotalAttribs_ = value;
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
	public string CaptionFormula
	{
		get
		{
			return captionFormula_;
		}
		set
		{
			captionFormula_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public ByteString CrossAttributes
	{
		get
		{
			return crossAttributes_;
		}
		set
		{
			crossAttributes_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PushColumn()
	{
	}

	[DebuggerNonUserCode]
	public PushColumn(PushColumn other)
		: this()
	{
		id_ = other.id_;
		action_ = other.action_;
		mask_ = other.mask_;
		width_ = other.width_;
		index_ = other.index_;
		caption_ = other.caption_;
		formula_ = other.formula_;
		visible_ = other.visible_;
		styleId_ = ((other.styleId_ != null) ? other.styleId_.Clone() : null);
		captionStyle_ = other.captionStyle_;
		consolidateAttribs_ = other.consolidateAttribs_;
		subtotalAttribs_ = other.subtotalAttribs_;
		permissions_ = other.permissions_;
		captionFormula_ = other.captionFormula_;
		crossAttributes_ = other.crossAttributes_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushColumn Clone()
	{
		return new PushColumn(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushColumn);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushColumn other)
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
		if (Width != other.Width)
		{
			return false;
		}
		if (Index != other.Index)
		{
			return false;
		}
		if (Caption != other.Caption)
		{
			return false;
		}
		if (Formula != other.Formula)
		{
			return false;
		}
		if (Visible != other.Visible)
		{
			return false;
		}
		if (!object.Equals(StyleId, other.StyleId))
		{
			return false;
		}
		if (CaptionStyle != other.CaptionStyle)
		{
			return false;
		}
		if (ConsolidateAttribs != other.ConsolidateAttribs)
		{
			return false;
		}
		if (SubtotalAttribs != other.SubtotalAttribs)
		{
			return false;
		}
		if (Permissions != other.Permissions)
		{
			return false;
		}
		if (CaptionFormula != other.CaptionFormula)
		{
			return false;
		}
		if (CrossAttributes != other.CrossAttributes)
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
		if (Width != 0)
		{
			num ^= Width.GetHashCode();
		}
		if (Index != 0)
		{
			num ^= Index.GetHashCode();
		}
		if (Caption.Length != 0)
		{
			num ^= Caption.GetHashCode();
		}
		if (Formula.Length != 0)
		{
			num ^= Formula.GetHashCode();
		}
		if (Visible)
		{
			num ^= Visible.GetHashCode();
		}
		if (styleId_ != null)
		{
			num ^= StyleId.GetHashCode();
		}
		if (CaptionStyle.Length != 0)
		{
			num ^= CaptionStyle.GetHashCode();
		}
		if (ConsolidateAttribs.Length != 0)
		{
			num ^= ConsolidateAttribs.GetHashCode();
		}
		if (SubtotalAttribs != 0)
		{
			num ^= SubtotalAttribs.GetHashCode();
		}
		if (Permissions.Length != 0)
		{
			num ^= Permissions.GetHashCode();
		}
		if (CaptionFormula.Length != 0)
		{
			num ^= CaptionFormula.GetHashCode();
		}
		if (CrossAttributes.Length != 0)
		{
			num ^= CrossAttributes.GetHashCode();
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
		if (Width != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(Width);
		}
		if (Index != 0)
		{
			output.WriteRawTag(40);
			output.WriteInt32(Index);
		}
		if (Caption.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteString(Caption);
		}
		if (Formula.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(Formula);
		}
		if (Visible)
		{
			output.WriteRawTag(128, 1);
			output.WriteBool(Visible);
		}
		if (styleId_ != null)
		{
			output.WriteRawTag(138, 1);
			output.WriteMessage(StyleId);
		}
		if (CaptionStyle.Length != 0)
		{
			output.WriteRawTag(146, 1);
			output.WriteString(CaptionStyle);
		}
		if (ConsolidateAttribs.Length != 0)
		{
			output.WriteRawTag(154, 1);
			output.WriteString(ConsolidateAttribs);
		}
		if (SubtotalAttribs != 0)
		{
			output.WriteRawTag(160, 1);
			output.WriteInt32(SubtotalAttribs);
		}
		if (Permissions.Length != 0)
		{
			output.WriteRawTag(170, 1);
			output.WriteString(Permissions);
		}
		if (CaptionFormula.Length != 0)
		{
			output.WriteRawTag(178, 1);
			output.WriteString(CaptionFormula);
		}
		if (CrossAttributes.Length != 0)
		{
			output.WriteRawTag(186, 1);
			output.WriteBytes(CrossAttributes);
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
		if (Width != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Width);
		}
		if (Index != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Index);
		}
		if (Caption.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Caption);
		}
		if (Formula.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Formula);
		}
		if (Visible)
		{
			num += 3;
		}
		if (styleId_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(StyleId);
		}
		if (CaptionStyle.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(CaptionStyle);
		}
		if (ConsolidateAttribs.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(ConsolidateAttribs);
		}
		if (SubtotalAttribs != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(SubtotalAttribs);
		}
		if (Permissions.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Permissions);
		}
		if (CaptionFormula.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(CaptionFormula);
		}
		if (CrossAttributes.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeBytesSize(CrossAttributes);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushColumn other)
	{
		if (other == null)
		{
			return;
		}
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
		if (other.Width != 0)
		{
			Width = other.Width;
		}
		if (other.Index != 0)
		{
			Index = other.Index;
		}
		if (other.Caption.Length != 0)
		{
			Caption = other.Caption;
		}
		if (other.Formula.Length != 0)
		{
			Formula = other.Formula;
		}
		if (other.Visible)
		{
			Visible = other.Visible;
		}
		if (other.styleId_ != null)
		{
			if (styleId_ == null)
			{
				StyleId = new Int64Value();
			}
			StyleId.MergeFrom(other.StyleId);
		}
		if (other.CaptionStyle.Length != 0)
		{
			CaptionStyle = other.CaptionStyle;
		}
		if (other.ConsolidateAttribs.Length != 0)
		{
			ConsolidateAttribs = other.ConsolidateAttribs;
		}
		if (other.SubtotalAttribs != 0)
		{
			SubtotalAttribs = other.SubtotalAttribs;
		}
		if (other.Permissions.Length != 0)
		{
			Permissions = other.Permissions;
		}
		if (other.CaptionFormula.Length != 0)
		{
			CaptionFormula = other.CaptionFormula;
		}
		if (other.CrossAttributes.Length != 0)
		{
			CrossAttributes = other.CrossAttributes;
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
			case 16u:
				Action = input.ReadInt32();
				break;
			case 24u:
				Mask = input.ReadInt32();
				break;
			case 32u:
				Width = input.ReadInt32();
				break;
			case 40u:
				Index = input.ReadInt32();
				break;
			case 50u:
				Caption = input.ReadString();
				break;
			case 58u:
				Formula = input.ReadString();
				break;
			case 128u:
				Visible = input.ReadBool();
				break;
			case 138u:
				if (styleId_ == null)
				{
					StyleId = new Int64Value();
				}
				input.ReadMessage(StyleId);
				break;
			case 146u:
				CaptionStyle = input.ReadString();
				break;
			case 154u:
				ConsolidateAttribs = input.ReadString();
				break;
			case 160u:
				SubtotalAttribs = input.ReadInt32();
				break;
			case 170u:
				Permissions = input.ReadString();
				break;
			case 178u:
				CaptionFormula = input.ReadString();
				break;
			case 186u:
				CrossAttributes = input.ReadBytes();
				break;
			}
		}
	}
}
