using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullDocument : IMessage<PullDocument>, Google.Protobuf.IMessage, IEquatable<PullDocument>, IDeepCloneable<PullDocument>
{
	private static readonly MessageParser<PullDocument> _parser = new MessageParser<PullDocument>(() => new PullDocument());

	private UnknownFieldSet _unknownFields;

	public const int NewParagraphsFieldNumber = 1;

	private static readonly FieldCodec<PullParagraph> _repeated_newParagraphs_codec = FieldCodec.ForMessage(10u, PullParagraph.Parser);

	private readonly RepeatedField<PullParagraph> newParagraphs_ = new RepeatedField<PullParagraph>();

	public const int DelParagraphsFieldNumber = 2;

	private static readonly FieldCodec<PullParagraph> _repeated_delParagraphs_codec = FieldCodec.ForMessage(18u, PullParagraph.Parser);

	private readonly RepeatedField<PullParagraph> delParagraphs_ = new RepeatedField<PullParagraph>();

	public const int ModParagraphsFieldNumber = 3;

	private static readonly FieldCodec<PullParagraph> _repeated_modParagraphs_codec = FieldCodec.ForMessage(26u, PullParagraph.Parser);

	private readonly RepeatedField<PullParagraph> modParagraphs_ = new RepeatedField<PullParagraph>();

	public const int LockerFieldNumber = 16;

	private OptionalInt64 locker_;

	public const int SectPrFieldNumber = 17;

	private OptionalString sectPr_;

	public const int MergeTableFieldNumber = 18;

	private OptionalInt64 mergeTable_;

	public const int ResultFieldNumber = 19;

	private string result_ = "";

	public const int VersionFieldNumber = 20;

	private int version_;

	[DebuggerNonUserCode]
	public static MessageParser<PullDocument> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PullDocumentReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<PullParagraph> NewParagraphs => newParagraphs_;

	[DebuggerNonUserCode]
	public RepeatedField<PullParagraph> DelParagraphs => delParagraphs_;

	[DebuggerNonUserCode]
	public RepeatedField<PullParagraph> ModParagraphs => modParagraphs_;

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
	public OptionalString SectPr
	{
		get
		{
			return sectPr_;
		}
		set
		{
			sectPr_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalInt64 MergeTable
	{
		get
		{
			return mergeTable_;
		}
		set
		{
			mergeTable_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string Result
	{
		get
		{
			return result_;
		}
		set
		{
			result_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int Version
	{
		get
		{
			return version_;
		}
		set
		{
			version_ = value;
		}
	}

	[DebuggerNonUserCode]
	public PullDocument()
	{
	}

	[DebuggerNonUserCode]
	public PullDocument(PullDocument other)
		: this()
	{
		newParagraphs_ = other.newParagraphs_.Clone();
		delParagraphs_ = other.delParagraphs_.Clone();
		modParagraphs_ = other.modParagraphs_.Clone();
		locker_ = ((other.locker_ != null) ? other.locker_.Clone() : null);
		sectPr_ = ((other.sectPr_ != null) ? other.sectPr_.Clone() : null);
		mergeTable_ = ((other.mergeTable_ != null) ? other.mergeTable_.Clone() : null);
		result_ = other.result_;
		version_ = other.version_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullDocument Clone()
	{
		return new PullDocument(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullDocument);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullDocument other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!newParagraphs_.Equals(other.newParagraphs_))
		{
			return false;
		}
		if (!delParagraphs_.Equals(other.delParagraphs_))
		{
			return false;
		}
		if (!modParagraphs_.Equals(other.modParagraphs_))
		{
			return false;
		}
		if (!object.Equals(Locker, other.Locker))
		{
			return false;
		}
		if (!object.Equals(SectPr, other.SectPr))
		{
			return false;
		}
		if (!object.Equals(MergeTable, other.MergeTable))
		{
			return false;
		}
		if (Result != other.Result)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	public override int GetHashCode()
	{
		int num = 1;
		num ^= newParagraphs_.GetHashCode();
		num ^= delParagraphs_.GetHashCode();
		num ^= modParagraphs_.GetHashCode();
		if (locker_ != null)
		{
			num ^= Locker.GetHashCode();
		}
		if (sectPr_ != null)
		{
			num ^= SectPr.GetHashCode();
		}
		if (mergeTable_ != null)
		{
			num ^= MergeTable.GetHashCode();
		}
		if (Result.Length != 0)
		{
			num ^= Result.GetHashCode();
		}
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
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
		newParagraphs_.WriteTo(output, _repeated_newParagraphs_codec);
		delParagraphs_.WriteTo(output, _repeated_delParagraphs_codec);
		modParagraphs_.WriteTo(output, _repeated_modParagraphs_codec);
		if (locker_ != null)
		{
			output.WriteRawTag(130, 1);
			output.WriteMessage(Locker);
		}
		if (sectPr_ != null)
		{
			output.WriteRawTag(138, 1);
			output.WriteMessage(SectPr);
		}
		if (mergeTable_ != null)
		{
			output.WriteRawTag(146, 1);
			output.WriteMessage(MergeTable);
		}
		if (Result.Length != 0)
		{
			output.WriteRawTag(154, 1);
			output.WriteString(Result);
		}
		if (Version != 0)
		{
			output.WriteRawTag(160, 1);
			output.WriteInt32(Version);
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
		num += newParagraphs_.CalculateSize(_repeated_newParagraphs_codec);
		num += delParagraphs_.CalculateSize(_repeated_delParagraphs_codec);
		num += modParagraphs_.CalculateSize(_repeated_modParagraphs_codec);
		if (locker_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(Locker);
		}
		if (sectPr_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(SectPr);
		}
		if (mergeTable_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(MergeTable);
		}
		if (Result.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Result);
		}
		if (Version != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(Version);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PullDocument other)
	{
		if (other == null)
		{
			return;
		}
		newParagraphs_.Add(other.newParagraphs_);
		delParagraphs_.Add(other.delParagraphs_);
		modParagraphs_.Add(other.modParagraphs_);
		if (other.locker_ != null)
		{
			if (locker_ == null)
			{
				Locker = new OptionalInt64();
			}
			Locker.MergeFrom(other.Locker);
		}
		if (other.sectPr_ != null)
		{
			if (sectPr_ == null)
			{
				SectPr = new OptionalString();
			}
			SectPr.MergeFrom(other.SectPr);
		}
		if (other.mergeTable_ != null)
		{
			if (mergeTable_ == null)
			{
				MergeTable = new OptionalInt64();
			}
			MergeTable.MergeFrom(other.MergeTable);
		}
		if (other.Result.Length != 0)
		{
			Result = other.Result;
		}
		if (other.Version != 0)
		{
			Version = other.Version;
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
			case 10u:
				newParagraphs_.AddEntriesFrom(input, _repeated_newParagraphs_codec);
				break;
			case 18u:
				delParagraphs_.AddEntriesFrom(input, _repeated_delParagraphs_codec);
				break;
			case 26u:
				modParagraphs_.AddEntriesFrom(input, _repeated_modParagraphs_codec);
				break;
			case 130u:
				if (locker_ == null)
				{
					Locker = new OptionalInt64();
				}
				input.ReadMessage(Locker);
				break;
			case 138u:
				if (sectPr_ == null)
				{
					SectPr = new OptionalString();
				}
				input.ReadMessage(SectPr);
				break;
			case 146u:
				if (mergeTable_ == null)
				{
					MergeTable = new OptionalInt64();
				}
				input.ReadMessage(MergeTable);
				break;
			case 154u:
				Result = input.ReadString();
				break;
			case 160u:
				Version = input.ReadInt32();
				break;
			}
		}
	}
}
