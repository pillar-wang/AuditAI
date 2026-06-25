using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullColumn : IMessage<PullColumn>, Google.Protobuf.IMessage, IEquatable<PullColumn>, IDeepCloneable<PullColumn>
{
	private static readonly MessageParser<PullColumn> _parser = new MessageParser<PullColumn>(() => new PullColumn());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int CaptionFieldNumber = 2;

	private OptionalString caption_;

	public const int IndexFieldNumber = 3;

	private OptionalInt32 index_;

	public const int WidthFieldNumber = 4;

	private OptionalInt32 width_;

	public const int VisibleFieldNumber = 5;

	private OptionalBool visible_;

	public const int StyleIdFieldNumber = 6;

	private NullableInt64 styleId_;

	public const int CaptionStyleFieldNumber = 7;

	private OptionalString captionStyle_;

	public const int ConsolidateAttribsFieldNumber = 8;

	private OptionalString consolidateAttribs_;

	public const int FormulaFieldNumber = 9;

	private OptionalString formula_;

	public const int SubtotalAttribsFieldNumber = 10;

	private OptionalInt32 subtotalAttribs_;

	public const int PermissionsFieldNumber = 11;

	private OptionalString permissions_;

	public const int CaptionFormulaFieldNumber = 12;

	private OptionalString captionFormula_;

	public const int CrossAttributesFieldNumber = 13;

	private OptionalBytes crossAttributes_;

	[DebuggerNonUserCode]
	public static MessageParser<PullColumn> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[13];

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
	public OptionalString Caption
	{
		get
		{
			return caption_;
		}
		set
		{
			caption_ = value;
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
	public OptionalInt32 Width
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
	public NullableInt64 StyleId
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
	public OptionalString CaptionStyle
	{
		get
		{
			return captionStyle_;
		}
		set
		{
			captionStyle_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString ConsolidateAttribs
	{
		get
		{
			return consolidateAttribs_;
		}
		set
		{
			consolidateAttribs_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString Formula
	{
		get
		{
			return formula_;
		}
		set
		{
			formula_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalInt32 SubtotalAttribs
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
	public OptionalString CaptionFormula
	{
		get
		{
			return captionFormula_;
		}
		set
		{
			captionFormula_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalBytes CrossAttributes
	{
		get
		{
			return crossAttributes_;
		}
		set
		{
			crossAttributes_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PullColumn()
	{
	}

	[DebuggerNonUserCode]
	public PullColumn(PullColumn other)
		: this()
	{
		id_ = other.id_;
		caption_ = ((other.caption_ != null) ? other.caption_.Clone() : null);
		index_ = ((other.index_ != null) ? other.index_.Clone() : null);
		width_ = ((other.width_ != null) ? other.width_.Clone() : null);
		visible_ = ((other.visible_ != null) ? other.visible_.Clone() : null);
		styleId_ = ((other.styleId_ != null) ? other.styleId_.Clone() : null);
		captionStyle_ = ((other.captionStyle_ != null) ? other.captionStyle_.Clone() : null);
		consolidateAttribs_ = ((other.consolidateAttribs_ != null) ? other.consolidateAttribs_.Clone() : null);
		formula_ = ((other.formula_ != null) ? other.formula_.Clone() : null);
		subtotalAttribs_ = ((other.subtotalAttribs_ != null) ? other.subtotalAttribs_.Clone() : null);
		permissions_ = ((other.permissions_ != null) ? other.permissions_.Clone() : null);
		captionFormula_ = ((other.captionFormula_ != null) ? other.captionFormula_.Clone() : null);
		crossAttributes_ = ((other.crossAttributes_ != null) ? other.crossAttributes_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullColumn Clone()
	{
		return new PullColumn(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullColumn);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullColumn other)
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
		if (!object.Equals(Caption, other.Caption))
		{
			return false;
		}
		if (!object.Equals(Index, other.Index))
		{
			return false;
		}
		if (!object.Equals(Width, other.Width))
		{
			return false;
		}
		if (!object.Equals(Visible, other.Visible))
		{
			return false;
		}
		if (!object.Equals(StyleId, other.StyleId))
		{
			return false;
		}
		if (!object.Equals(CaptionStyle, other.CaptionStyle))
		{
			return false;
		}
		if (!object.Equals(ConsolidateAttribs, other.ConsolidateAttribs))
		{
			return false;
		}
		if (!object.Equals(Formula, other.Formula))
		{
			return false;
		}
		if (!object.Equals(SubtotalAttribs, other.SubtotalAttribs))
		{
			return false;
		}
		if (!object.Equals(Permissions, other.Permissions))
		{
			return false;
		}
		if (!object.Equals(CaptionFormula, other.CaptionFormula))
		{
			return false;
		}
		if (!object.Equals(CrossAttributes, other.CrossAttributes))
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
		if (caption_ != null)
		{
			num ^= Caption.GetHashCode();
		}
		if (index_ != null)
		{
			num ^= Index.GetHashCode();
		}
		if (width_ != null)
		{
			num ^= Width.GetHashCode();
		}
		if (visible_ != null)
		{
			num ^= Visible.GetHashCode();
		}
		if (styleId_ != null)
		{
			num ^= StyleId.GetHashCode();
		}
		if (captionStyle_ != null)
		{
			num ^= CaptionStyle.GetHashCode();
		}
		if (consolidateAttribs_ != null)
		{
			num ^= ConsolidateAttribs.GetHashCode();
		}
		if (formula_ != null)
		{
			num ^= Formula.GetHashCode();
		}
		if (subtotalAttribs_ != null)
		{
			num ^= SubtotalAttribs.GetHashCode();
		}
		if (permissions_ != null)
		{
			num ^= Permissions.GetHashCode();
		}
		if (captionFormula_ != null)
		{
			num ^= CaptionFormula.GetHashCode();
		}
		if (crossAttributes_ != null)
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
		if (caption_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(Caption);
		}
		if (index_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(Index);
		}
		if (width_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Width);
		}
		if (visible_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(Visible);
		}
		if (styleId_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(StyleId);
		}
		if (captionStyle_ != null)
		{
			output.WriteRawTag(58);
			output.WriteMessage(CaptionStyle);
		}
		if (consolidateAttribs_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(ConsolidateAttribs);
		}
		if (formula_ != null)
		{
			output.WriteRawTag(74);
			output.WriteMessage(Formula);
		}
		if (subtotalAttribs_ != null)
		{
			output.WriteRawTag(82);
			output.WriteMessage(SubtotalAttribs);
		}
		if (permissions_ != null)
		{
			output.WriteRawTag(90);
			output.WriteMessage(Permissions);
		}
		if (captionFormula_ != null)
		{
			output.WriteRawTag(98);
			output.WriteMessage(CaptionFormula);
		}
		if (crossAttributes_ != null)
		{
			output.WriteRawTag(106);
			output.WriteMessage(CrossAttributes);
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
		if (caption_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Caption);
		}
		if (index_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Index);
		}
		if (width_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Width);
		}
		if (visible_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Visible);
		}
		if (styleId_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(StyleId);
		}
		if (captionStyle_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CaptionStyle);
		}
		if (consolidateAttribs_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(ConsolidateAttribs);
		}
		if (formula_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Formula);
		}
		if (subtotalAttribs_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(SubtotalAttribs);
		}
		if (permissions_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Permissions);
		}
		if (captionFormula_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CaptionFormula);
		}
		if (crossAttributes_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CrossAttributes);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PullColumn other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0L)
		{
			Id = other.Id;
		}
		if (other.caption_ != null)
		{
			if (caption_ == null)
			{
				Caption = new OptionalString();
			}
			Caption.MergeFrom(other.Caption);
		}
		if (other.index_ != null)
		{
			if (index_ == null)
			{
				Index = new OptionalInt32();
			}
			Index.MergeFrom(other.Index);
		}
		if (other.width_ != null)
		{
			if (width_ == null)
			{
				Width = new OptionalInt32();
			}
			Width.MergeFrom(other.Width);
		}
		if (other.visible_ != null)
		{
			if (visible_ == null)
			{
				Visible = new OptionalBool();
			}
			Visible.MergeFrom(other.Visible);
		}
		if (other.styleId_ != null)
		{
			if (styleId_ == null)
			{
				StyleId = new NullableInt64();
			}
			StyleId.MergeFrom(other.StyleId);
		}
		if (other.captionStyle_ != null)
		{
			if (captionStyle_ == null)
			{
				CaptionStyle = new OptionalString();
			}
			CaptionStyle.MergeFrom(other.CaptionStyle);
		}
		if (other.consolidateAttribs_ != null)
		{
			if (consolidateAttribs_ == null)
			{
				ConsolidateAttribs = new OptionalString();
			}
			ConsolidateAttribs.MergeFrom(other.ConsolidateAttribs);
		}
		if (other.formula_ != null)
		{
			if (formula_ == null)
			{
				Formula = new OptionalString();
			}
			Formula.MergeFrom(other.Formula);
		}
		if (other.subtotalAttribs_ != null)
		{
			if (subtotalAttribs_ == null)
			{
				SubtotalAttribs = new OptionalInt32();
			}
			SubtotalAttribs.MergeFrom(other.SubtotalAttribs);
		}
		if (other.permissions_ != null)
		{
			if (permissions_ == null)
			{
				Permissions = new OptionalString();
			}
			Permissions.MergeFrom(other.Permissions);
		}
		if (other.captionFormula_ != null)
		{
			if (captionFormula_ == null)
			{
				CaptionFormula = new OptionalString();
			}
			CaptionFormula.MergeFrom(other.CaptionFormula);
		}
		if (other.crossAttributes_ != null)
		{
			if (crossAttributes_ == null)
			{
				CrossAttributes = new OptionalBytes();
			}
			CrossAttributes.MergeFrom(other.CrossAttributes);
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
				if (caption_ == null)
				{
					Caption = new OptionalString();
				}
				input.ReadMessage(Caption);
				break;
			case 26u:
				if (index_ == null)
				{
					Index = new OptionalInt32();
				}
				input.ReadMessage(Index);
				break;
			case 34u:
				if (width_ == null)
				{
					Width = new OptionalInt32();
				}
				input.ReadMessage(Width);
				break;
			case 42u:
				if (visible_ == null)
				{
					Visible = new OptionalBool();
				}
				input.ReadMessage(Visible);
				break;
			case 50u:
				if (styleId_ == null)
				{
					StyleId = new NullableInt64();
				}
				input.ReadMessage(StyleId);
				break;
			case 58u:
				if (captionStyle_ == null)
				{
					CaptionStyle = new OptionalString();
				}
				input.ReadMessage(CaptionStyle);
				break;
			case 66u:
				if (consolidateAttribs_ == null)
				{
					ConsolidateAttribs = new OptionalString();
				}
				input.ReadMessage(ConsolidateAttribs);
				break;
			case 74u:
				if (formula_ == null)
				{
					Formula = new OptionalString();
				}
				input.ReadMessage(Formula);
				break;
			case 82u:
				if (subtotalAttribs_ == null)
				{
					SubtotalAttribs = new OptionalInt32();
				}
				input.ReadMessage(SubtotalAttribs);
				break;
			case 90u:
				if (permissions_ == null)
				{
					Permissions = new OptionalString();
				}
				input.ReadMessage(Permissions);
				break;
			case 98u:
				if (captionFormula_ == null)
				{
					CaptionFormula = new OptionalString();
				}
				input.ReadMessage(CaptionFormula);
				break;
			case 106u:
				if (crossAttributes_ == null)
				{
					CrossAttributes = new OptionalBytes();
				}
				input.ReadMessage(CrossAttributes);
				break;
			}
		}
	}
}
