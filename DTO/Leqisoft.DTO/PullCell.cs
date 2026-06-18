using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public sealed class PullCell : IMessage<PullCell>, Google.Protobuf.IMessage, IEquatable<PullCell>, IDeepCloneable<PullCell>
{
	private static readonly MessageParser<PullCell> _parser = new MessageParser<PullCell>(() => new PullCell());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int CIdFieldNumber = 2;

	private OptionalInt64 cId_;

	public const int RIdFieldNumber = 3;

	private OptionalInt64 rId_;

	public const int ValueFieldNumber = 4;

	private OptionalBytes value_;

	public const int FormulaFieldNumber = 5;

	private OptionalString formula_;

	public const int StyleFieldNumber = 6;

	private NullableInt64 style_;

	public const int CollectSourceFieldNumber = 7;

	private OptionalString collectSource_;

	public const int HeaderFormulaFieldNumber = 8;

	private OptionalString headerFormula_;

	[DebuggerNonUserCode]
	public static MessageParser<PullCell> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[15];

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
	public OptionalInt64 CId
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
	public OptionalInt64 RId
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
	public OptionalBytes Value
	{
		get
		{
			return value_;
		}
		set
		{
			value_ = value;
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
	public NullableInt64 Style
	{
		get
		{
			return style_;
		}
		set
		{
			style_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString CollectSource
	{
		get
		{
			return collectSource_;
		}
		set
		{
			collectSource_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString HeaderFormula
	{
		get
		{
			return headerFormula_;
		}
		set
		{
			headerFormula_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PullCell()
	{
	}

	[DebuggerNonUserCode]
	public PullCell(PullCell other)
		: this()
	{
		id_ = other.id_;
		cId_ = ((other.cId_ != null) ? other.cId_.Clone() : null);
		rId_ = ((other.rId_ != null) ? other.rId_.Clone() : null);
		value_ = ((other.value_ != null) ? other.value_.Clone() : null);
		formula_ = ((other.formula_ != null) ? other.formula_.Clone() : null);
		style_ = ((other.style_ != null) ? other.style_.Clone() : null);
		collectSource_ = ((other.collectSource_ != null) ? other.collectSource_.Clone() : null);
		headerFormula_ = ((other.headerFormula_ != null) ? other.headerFormula_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullCell Clone()
	{
		return new PullCell(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullCell);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullCell other)
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
		if (!object.Equals(CId, other.CId))
		{
			return false;
		}
		if (!object.Equals(RId, other.RId))
		{
			return false;
		}
		if (!object.Equals(Value, other.Value))
		{
			return false;
		}
		if (!object.Equals(Formula, other.Formula))
		{
			return false;
		}
		if (!object.Equals(Style, other.Style))
		{
			return false;
		}
		if (!object.Equals(CollectSource, other.CollectSource))
		{
			return false;
		}
		if (!object.Equals(HeaderFormula, other.HeaderFormula))
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
		if (cId_ != null)
		{
			num ^= CId.GetHashCode();
		}
		if (rId_ != null)
		{
			num ^= RId.GetHashCode();
		}
		if (value_ != null)
		{
			num ^= Value.GetHashCode();
		}
		if (formula_ != null)
		{
			num ^= Formula.GetHashCode();
		}
		if (style_ != null)
		{
			num ^= Style.GetHashCode();
		}
		if (collectSource_ != null)
		{
			num ^= CollectSource.GetHashCode();
		}
		if (headerFormula_ != null)
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
		if (cId_ != null)
		{
			output.WriteRawTag(18);
			output.WriteMessage(CId);
		}
		if (rId_ != null)
		{
			output.WriteRawTag(26);
			output.WriteMessage(RId);
		}
		if (value_ != null)
		{
			output.WriteRawTag(34);
			output.WriteMessage(Value);
		}
		if (formula_ != null)
		{
			output.WriteRawTag(42);
			output.WriteMessage(Formula);
		}
		if (style_ != null)
		{
			output.WriteRawTag(50);
			output.WriteMessage(Style);
		}
		if (collectSource_ != null)
		{
			output.WriteRawTag(58);
			output.WriteMessage(CollectSource);
		}
		if (headerFormula_ != null)
		{
			output.WriteRawTag(66);
			output.WriteMessage(HeaderFormula);
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
		if (cId_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CId);
		}
		if (rId_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(RId);
		}
		if (value_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Value);
		}
		if (formula_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Formula);
		}
		if (style_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(Style);
		}
		if (collectSource_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(CollectSource);
		}
		if (headerFormula_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize(HeaderFormula);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PullCell other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Id != 0L)
		{
			Id = other.Id;
		}
		if (other.cId_ != null)
		{
			if (cId_ == null)
			{
				CId = new OptionalInt64();
			}
			CId.MergeFrom(other.CId);
		}
		if (other.rId_ != null)
		{
			if (rId_ == null)
			{
				RId = new OptionalInt64();
			}
			RId.MergeFrom(other.RId);
		}
		if (other.value_ != null)
		{
			if (value_ == null)
			{
				Value = new OptionalBytes();
			}
			Value.MergeFrom(other.Value);
		}
		if (other.formula_ != null)
		{
			if (formula_ == null)
			{
				Formula = new OptionalString();
			}
			Formula.MergeFrom(other.Formula);
		}
		if (other.style_ != null)
		{
			if (style_ == null)
			{
				Style = new NullableInt64();
			}
			Style.MergeFrom(other.Style);
		}
		if (other.collectSource_ != null)
		{
			if (collectSource_ == null)
			{
				CollectSource = new OptionalString();
			}
			CollectSource.MergeFrom(other.CollectSource);
		}
		if (other.headerFormula_ != null)
		{
			if (headerFormula_ == null)
			{
				HeaderFormula = new OptionalString();
			}
			HeaderFormula.MergeFrom(other.HeaderFormula);
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
				if (cId_ == null)
				{
					CId = new OptionalInt64();
				}
				input.ReadMessage(CId);
				break;
			case 26u:
				if (rId_ == null)
				{
					RId = new OptionalInt64();
				}
				input.ReadMessage(RId);
				break;
			case 34u:
				if (value_ == null)
				{
					Value = new OptionalBytes();
				}
				input.ReadMessage(Value);
				break;
			case 42u:
				if (formula_ == null)
				{
					Formula = new OptionalString();
				}
				input.ReadMessage(Formula);
				break;
			case 50u:
				if (style_ == null)
				{
					Style = new NullableInt64();
				}
				input.ReadMessage(Style);
				break;
			case 58u:
				if (collectSource_ == null)
				{
					CollectSource = new OptionalString();
				}
				input.ReadMessage(CollectSource);
				break;
			case 66u:
				if (headerFormula_ == null)
				{
					HeaderFormula = new OptionalString();
				}
				input.ReadMessage(HeaderFormula);
				break;
			}
		}
	}
}
