using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PushDocument : IMessage<PushDocument>, Google.Protobuf.IMessage, IEquatable<PushDocument>, IDeepCloneable<PushDocument>
{
	private static readonly MessageParser<PushDocument> _parser = new MessageParser<PushDocument>(() => new PushDocument());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int ProjectIdFieldNumber = 2;

	private ByteString projectId_ = ByteString.Empty;

	public const int VersionFieldNumber = 3;

	private int version_;

	public const int MaskFieldNumber = 4;

	private int mask_;

	public const int ParagraphsFieldNumber = 5;

	private static readonly FieldCodec<PushParagraph> _repeated_paragraphs_codec = FieldCodec.ForMessage(42u, PushParagraph.Parser);

	private readonly RepeatedField<PushParagraph> paragraphs_ = new RepeatedField<PushParagraph>();

	public const int LockerFieldNumber = 16;

	private long locker_;

	public const int SectPrFieldNumber = 17;

	private string sectPr_ = "";

	public const int MergeTableFieldNumber = 18;

	private long mergeTable_;

	[DebuggerNonUserCode]
	public static MessageParser<PushDocument> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushdocumentReflection.Descriptor.MessageTypes[0];

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
	public ByteString ProjectId
	{
		get
		{
			return projectId_;
		}
		set
		{
			projectId_ = ProtoPreconditions.CheckNotNull(value, "value");
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
	public RepeatedField<PushParagraph> Paragraphs => paragraphs_;

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
	public string SectPr
	{
		get
		{
			return sectPr_;
		}
		set
		{
			sectPr_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public long MergeTable
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
	public PushDocument()
	{
	}

	[DebuggerNonUserCode]
	public PushDocument(PushDocument other)
		: this()
	{
		id_ = other.id_;
		projectId_ = other.projectId_;
		version_ = other.version_;
		mask_ = other.mask_;
		paragraphs_ = other.paragraphs_.Clone();
		locker_ = other.locker_;
		sectPr_ = other.sectPr_;
		mergeTable_ = other.mergeTable_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushDocument Clone()
	{
		return new PushDocument(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushDocument);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushDocument other)
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
		if (ProjectId != other.ProjectId)
		{
			return false;
		}
		if (Version != other.Version)
		{
			return false;
		}
		if (Mask != other.Mask)
		{
			return false;
		}
		if (!paragraphs_.Equals(other.paragraphs_))
		{
			return false;
		}
		if (Locker != other.Locker)
		{
			return false;
		}
		if (SectPr != other.SectPr)
		{
			return false;
		}
		if (MergeTable != other.MergeTable)
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
		if (ProjectId.Length != 0)
		{
			num ^= ProjectId.GetHashCode();
		}
		if (Version != 0)
		{
			num ^= Version.GetHashCode();
		}
		if (Mask != 0)
		{
			num ^= Mask.GetHashCode();
		}
		num ^= paragraphs_.GetHashCode();
		if (Locker != 0L)
		{
			num ^= Locker.GetHashCode();
		}
		if (SectPr.Length != 0)
		{
			num ^= SectPr.GetHashCode();
		}
		if (MergeTable != 0L)
		{
			num ^= MergeTable.GetHashCode();
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
		if (ProjectId.Length != 0)
		{
			output.WriteRawTag(18);
			output.WriteBytes(ProjectId);
		}
		if (Version != 0)
		{
			output.WriteRawTag(24);
			output.WriteInt32(Version);
		}
		if (Mask != 0)
		{
			output.WriteRawTag(32);
			output.WriteInt32(Mask);
		}
		paragraphs_.WriteTo(output, _repeated_paragraphs_codec);
		if (Locker != 0L)
		{
			output.WriteRawTag(128, 1);
			output.WriteInt64(Locker);
		}
		if (SectPr.Length != 0)
		{
			output.WriteRawTag(138, 1);
			output.WriteString(SectPr);
		}
		if (MergeTable != 0L)
		{
			output.WriteRawTag(144, 1);
			output.WriteInt64(MergeTable);
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
		if (ProjectId.Length != 0)
		{
			num += 1 + CodedOutputStream.ComputeBytesSize(ProjectId);
		}
		if (Version != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Version);
		}
		if (Mask != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Mask);
		}
		num += paragraphs_.CalculateSize(_repeated_paragraphs_codec);
		if (Locker != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(Locker);
		}
		if (SectPr.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(SectPr);
		}
		if (MergeTable != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(MergeTable);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushDocument other)
	{
		if (other != null)
		{
			if (other.Id != 0L)
			{
				Id = other.Id;
			}
			if (other.ProjectId.Length != 0)
			{
				ProjectId = other.ProjectId;
			}
			if (other.Version != 0)
			{
				Version = other.Version;
			}
			if (other.Mask != 0)
			{
				Mask = other.Mask;
			}
			paragraphs_.Add(other.paragraphs_);
			if (other.Locker != 0L)
			{
				Locker = other.Locker;
			}
			if (other.SectPr.Length != 0)
			{
				SectPr = other.SectPr;
			}
			if (other.MergeTable != 0L)
			{
				MergeTable = other.MergeTable;
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
			case 18u:
				ProjectId = input.ReadBytes();
				break;
			case 24u:
				Version = input.ReadInt32();
				break;
			case 32u:
				Mask = input.ReadInt32();
				break;
			case 42u:
				paragraphs_.AddEntriesFrom(input, _repeated_paragraphs_codec);
				break;
			case 128u:
				Locker = input.ReadInt64();
				break;
			case 138u:
				SectPr = input.ReadString();
				break;
			case 144u:
				MergeTable = input.ReadInt64();
				break;
			}
		}
	}
}
