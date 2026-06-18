using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Leqisoft.DTO;

public sealed class PushCell : IMessage<PushCell>, Google.Protobuf.IMessage, IEquatable<PushCell>, IDeepCloneable<PushCell>
{
	private static readonly MessageParser<PushCell> _parser = new MessageParser<PushCell>(() => new PushCell());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int ActionFieldNumber = 2;

	private int action_;

	public const int MaskFieldNumber = 3;

	private int mask_;

	public const int CIdFieldNumber = 4;

	private long cId_;

	public const int RIdFieldNumber = 5;

	private long rId_;

	public const int ValueFieldNumber = 6;

	private ByteString value_ = ByteString.Empty;

	public const int FormulaFieldNumber = 7;

	private string formula_ = "";

	public const int StyleIdFieldNumber = 8;

	private Int64Value styleId_;

	public const int CollectSourceFieldNumber = 16;

	private string collectSource_ = "";

	public const int HeaderFormulaFieldNumber = 17;

	private string headerFormula_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PushCell> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushtableReflection.Descriptor.MessageTypes[3];

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
	public long CId
	{
		get
		{
			return cId_;
		}
		set
		{
			cId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public long RId
	{
		get
		{
			return rId_;
		}
		set
		{
			rId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public string CollectSource
	{
		get
		{
			return collectSource_;
		}
		set
		{
			collectSource_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string HeaderFormula
	{
		get
		{
			return headerFormula_;
		}
		set
		{
			headerFormula_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PushCell()
	{
	}

	[DebuggerNonUserCode]
	public PushCell(PushCell other)
		: this()
	{
		id_ = other.id_;
		action_ = other.action_;
		mask_ = other.mask_;
		cId_ = other.cId_;
		rId_ = other.rId_;
		value_ = other.value_;
		formula_ = other.formula_;
		styleId_ = ((other.styleId_ != null) ? other.styleId_.Clone() : null);
		collectSource_ = other.collectSource_;
		headerFormula_ = other.headerFormula_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushCell Clone()
	{
		return new PushCell(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushCell);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushCell other)
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
		if (CId != other.CId)
		{
			return false;
		}
		if (RId != other.RId)
		{
			return false;
		}
		if (Value != other.Value)
		{
			return false;
		}
		if (Formula != other.Formula)
		{
			return false;
		}
		if (!object.Equals(StyleId, other.StyleId))
		{
			return false;
		}
		if (CollectSource != other.CollectSource)
		{
			return false;
		}
		if (HeaderFormula != other.HeaderFormula)
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
		if (CId != 0L)
		{
			num ^= CId.GetHashCode();
		}
		if (RId != 0L)
		{
			num ^= RId.GetHashCode();
		}
		if (Value.Length != 0)
		{
			num ^= Value.GetHashCode();
		}
		if (Formula.Length != 0)
		{
			num ^= Formula.GetHashCode();
		}
		if (styleId_ != null)
		{
			num ^= StyleId.GetHashCode();
		}
		if (CollectSource.Length != 0)
		{
			num ^= CollectSource.GetHashCode();
		}
		if (HeaderFormula.Length != 0)
		{
			num ^= HeaderFormula.GetHashCode();
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
		if (CId != 0L)
		{
			output.WriteRawTag(32);
			output.WriteInt64(CId);
		}
		if (RId != 0L)
		{
			output.WriteRawTag(40);
			output.WriteInt64(RId);
		}
		if (Value.Length != 0)
		{
			output.WriteRawTag(50);
			output.WriteBytes(Value);
		}
		if (Formula.Length != 0)
		{
			output.WriteRawTag(58);
			output.WriteString(Formula);
		}
		if (styleId_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(StyleId);
		}
		if (CollectSource.Length != 0)
		{
			output.WriteRawTag(130, 1);
			output.WriteString(CollectSource);
		}
		if (HeaderFormula.Length != 0)
		{
			output.WriteRawTag(138, 1);
			output.WriteString(HeaderFormula);
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
		if (CId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(CId);
		}
		if (RId != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(RId);
		}
		if (Value.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(Value);
		}
		if (Formula.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeStringSize(Formula);
		}
		if (styleId_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(StyleId);
		}
		if (CollectSource.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(CollectSource);
		}
		if (HeaderFormula.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(HeaderFormula);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushCell other)
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
		if (other.CId != 0L)
		{
			CId = other.CId;
		}
		if (other.RId != 0L)
		{
			RId = other.RId;
		}
		if (other.Value.Length != 0)
		{
			Value = other.Value;
		}
		if (other.Formula.Length != 0)
		{
			Formula = other.Formula;
		}
		if (other.styleId_ != null)
		{
			if (styleId_ == null)
			{
				StyleId = new Int64Value();
			}
			StyleId.MergeFrom(other.StyleId);
		}
		if (other.CollectSource.Length != 0)
		{
			CollectSource = other.CollectSource;
		}
		if (other.HeaderFormula.Length != 0)
		{
			HeaderFormula = other.HeaderFormula;
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
				CId = input.ReadInt64();
				break;
			case 40u:
				RId = input.ReadInt64();
				break;
			case 50u:
				Value = input.ReadBytes();
				break;
			case 58u:
				Formula = input.ReadString();
				break;
			case 66u:
				if (styleId_ == null)
				{
					StyleId = new Int64Value();
				}
				input.ReadMessage(StyleId);
				break;
			case 130u:
				CollectSource = input.ReadString();
				break;
			case 138u:
				HeaderFormula = input.ReadString();
				break;
			}
		}
	}
}
