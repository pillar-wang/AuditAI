using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PushTable : IMessage<PushTable>, Google.Protobuf.IMessage, IEquatable<PushTable>, IDeepCloneable<PushTable>
{
	private static readonly MessageParser<PushTable> _parser = new MessageParser<PushTable>(() => new PushTable());

	private UnknownFieldSet _unknownFields;

	public const int IdFieldNumber = 1;

	private long id_;

	public const int ProjectIdFieldNumber = 2;

	private ByteString projectId_ = ByteString.Empty;

	public const int VersionFieldNumber = 3;

	private int version_;

	public const int MaskFieldNumber = 4;

	private int mask_;

	public const int ColumnsFieldNumber = 5;

	private static readonly FieldCodec<PushColumn> _repeated_columns_codec = FieldCodec.ForMessage(42u, PushColumn.Parser);

	private readonly RepeatedField<PushColumn> columns_ = new RepeatedField<PushColumn>();

	public const int RowsFieldNumber = 6;

	private static readonly FieldCodec<PushRow> _repeated_rows_codec = FieldCodec.ForMessage(50u, PushRow.Parser);

	private readonly RepeatedField<PushRow> rows_ = new RepeatedField<PushRow>();

	public const int CellsFieldNumber = 7;

	private static readonly FieldCodec<PushCell> _repeated_cells_codec = FieldCodec.ForMessage(58u, PushCell.Parser);

	private readonly RepeatedField<PushCell> cells_ = new RepeatedField<PushCell>();

	public const int CellStylesFieldNumber = 8;

	private static readonly FieldCodec<PushCellStyle> _repeated_cellStyles_codec = FieldCodec.ForMessage(66u, PushCellStyle.Parser);

	private readonly RepeatedField<PushCellStyle> cellStyles_ = new RepeatedField<PushCellStyle>();

	public const int MergesFieldNumber = 9;

	private static readonly FieldCodec<PushMerge> _repeated_merges_codec = FieldCodec.ForMessage(74u, PushMerge.Parser);

	private readonly RepeatedField<PushMerge> merges_ = new RepeatedField<PushMerge>();

	public const int CellAttachmentsFieldNumber = 10;

	private static readonly FieldCodec<PushCellAttachment> _repeated_cellAttachments_codec = FieldCodec.ForMessage(82u, PushCellAttachment.Parser);

	private readonly RepeatedField<PushCellAttachment> cellAttachments_ = new RepeatedField<PushCellAttachment>();

	public const int TitleFieldNumber = 16;

	private string title_ = "";

	public const int NoteFieldNumber = 17;

	private string note_ = "";

	public const int HeaderHeightsFieldNumber = 18;

	private string headerHeights_ = "";

	public const int DefaultStyleIdFieldNumber = 19;

	private long defaultStyleId_;

	public const int PageSetupFieldNumber = 20;

	private string pageSetup_ = "";

	public const int ConsolidateSettingsFieldNumber = 21;

	private string consolidateSettings_ = "";

	public const int BorderStyleFieldNumber = 22;

	private int borderStyle_;

	public const int FrozenColsFieldNumber = 23;

	private int frozenCols_;

	public const int HeaderModeFieldNumber = 24;

	private int headerMode_;

	public const int CollectSourceFieldNumber = 25;

	private string collectSource_ = "";

	public const int LockerFieldNumber = 26;

	private long locker_;

	public const int FilterInfoFieldNumber = 27;

	private string filterInfo_ = "";

	public const int FootFieldNumber = 28;

	private string foot_ = "";

	public const int RowOwnerExclusiveFieldNumber = 29;

	private bool rowOwnerExclusive_;

	public const int RowOwnerLoadFieldNumber = 30;

	private bool rowOwnerLoad_;

	public const int RowOwnerLoadShareFieldNumber = 31;

	private ByteString rowOwnerLoadShare_ = ByteString.Empty;

	public const int TicketFieldNumber = 32;

	private string ticket_ = "";

	public const int ControlFormulaFieldNumber = 33;

	private string controlFormula_ = "";

	[DebuggerNonUserCode]
	public static MessageParser<PushTable> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PushtableReflection.Descriptor.MessageTypes[0];

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
	public RepeatedField<PushColumn> Columns => columns_;

	[DebuggerNonUserCode]
	public RepeatedField<PushRow> Rows => rows_;

	[DebuggerNonUserCode]
	public RepeatedField<PushCell> Cells => cells_;

	[DebuggerNonUserCode]
	public RepeatedField<PushCellStyle> CellStyles => cellStyles_;

	[DebuggerNonUserCode]
	public RepeatedField<PushMerge> Merges => merges_;

	[DebuggerNonUserCode]
	public RepeatedField<PushCellAttachment> CellAttachments => cellAttachments_;

	[DebuggerNonUserCode]
	public string Title
	{
		get
		{
			return title_;
		}
		set
		{
			title_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Note
	{
		get
		{
			return note_;
		}
		set
		{
			note_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string HeaderHeights
	{
		get
		{
			return headerHeights_;
		}
		set
		{
			headerHeights_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public long DefaultStyleId
	{
		get
		{
			return defaultStyleId_;
		}
		set
		{
			defaultStyleId_ = value;
		}
	}

	[DebuggerNonUserCode]
	public string PageSetup
	{
		get
		{
			return pageSetup_;
		}
		set
		{
			pageSetup_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ConsolidateSettings
	{
		get
		{
			return consolidateSettings_;
		}
		set
		{
			consolidateSettings_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public int BorderStyle
	{
		get
		{
			return borderStyle_;
		}
		set
		{
			borderStyle_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int FrozenCols
	{
		get
		{
			return frozenCols_;
		}
		set
		{
			frozenCols_ = value;
		}
	}

	[DebuggerNonUserCode]
	public int HeaderMode
	{
		get
		{
			return headerMode_;
		}
		set
		{
			headerMode_ = value;
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
	public string FilterInfo
	{
		get
		{
			return filterInfo_;
		}
		set
		{
			filterInfo_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Foot
	{
		get
		{
			return foot_;
		}
		set
		{
			foot_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public bool RowOwnerExclusive
	{
		get
		{
			return rowOwnerExclusive_;
		}
		set
		{
			rowOwnerExclusive_ = value;
		}
	}

	[DebuggerNonUserCode]
	public bool RowOwnerLoad
	{
		get
		{
			return rowOwnerLoad_;
		}
		set
		{
			rowOwnerLoad_ = value;
		}
	}

	[DebuggerNonUserCode]
	public ByteString RowOwnerLoadShare
	{
		get
		{
			return rowOwnerLoadShare_;
		}
		set
		{
			rowOwnerLoadShare_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string Ticket
	{
		get
		{
			return ticket_;
		}
		set
		{
			ticket_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public string ControlFormula
	{
		get
		{
			return controlFormula_;
		}
		set
		{
			controlFormula_ = ProtoPreconditions.CheckNotNull(value, "value");
		}
	}

	[DebuggerNonUserCode]
	public PushTable()
	{
	}

	[DebuggerNonUserCode]
	public PushTable(PushTable other)
		: this()
	{
		id_ = other.id_;
		projectId_ = other.projectId_;
		version_ = other.version_;
		mask_ = other.mask_;
		columns_ = other.columns_.Clone();
		rows_ = other.rows_.Clone();
		cells_ = other.cells_.Clone();
		cellStyles_ = other.cellStyles_.Clone();
		merges_ = other.merges_.Clone();
		cellAttachments_ = other.cellAttachments_.Clone();
		title_ = other.title_;
		note_ = other.note_;
		headerHeights_ = other.headerHeights_;
		defaultStyleId_ = other.defaultStyleId_;
		pageSetup_ = other.pageSetup_;
		consolidateSettings_ = other.consolidateSettings_;
		borderStyle_ = other.borderStyle_;
		frozenCols_ = other.frozenCols_;
		headerMode_ = other.headerMode_;
		collectSource_ = other.collectSource_;
		locker_ = other.locker_;
		filterInfo_ = other.filterInfo_;
		foot_ = other.foot_;
		rowOwnerExclusive_ = other.rowOwnerExclusive_;
		rowOwnerLoad_ = other.rowOwnerLoad_;
		rowOwnerLoadShare_ = other.rowOwnerLoadShare_;
		ticket_ = other.ticket_;
		controlFormula_ = other.controlFormula_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PushTable Clone()
	{
		return new PushTable(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PushTable);
	}

	[DebuggerNonUserCode]
	public bool Equals(PushTable other)
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
		if (!columns_.Equals(other.columns_))
		{
			return false;
		}
		if (!rows_.Equals(other.rows_))
		{
			return false;
		}
		if (!cells_.Equals(other.cells_))
		{
			return false;
		}
		if (!cellStyles_.Equals(other.cellStyles_))
		{
			return false;
		}
		if (!merges_.Equals(other.merges_))
		{
			return false;
		}
		if (!cellAttachments_.Equals(other.cellAttachments_))
		{
			return false;
		}
		if (Title != other.Title)
		{
			return false;
		}
		if (Note != other.Note)
		{
			return false;
		}
		if (HeaderHeights != other.HeaderHeights)
		{
			return false;
		}
		if (DefaultStyleId != other.DefaultStyleId)
		{
			return false;
		}
		if (PageSetup != other.PageSetup)
		{
			return false;
		}
		if (ConsolidateSettings != other.ConsolidateSettings)
		{
			return false;
		}
		if (BorderStyle != other.BorderStyle)
		{
			return false;
		}
		if (FrozenCols != other.FrozenCols)
		{
			return false;
		}
		if (HeaderMode != other.HeaderMode)
		{
			return false;
		}
		if (CollectSource != other.CollectSource)
		{
			return false;
		}
		if (Locker != other.Locker)
		{
			return false;
		}
		if (FilterInfo != other.FilterInfo)
		{
			return false;
		}
		if (Foot != other.Foot)
		{
			return false;
		}
		if (RowOwnerExclusive != other.RowOwnerExclusive)
		{
			return false;
		}
		if (RowOwnerLoad != other.RowOwnerLoad)
		{
			return false;
		}
		if (RowOwnerLoadShare != other.RowOwnerLoadShare)
		{
			return false;
		}
		if (Ticket != other.Ticket)
		{
			return false;
		}
		if (ControlFormula != other.ControlFormula)
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
		num ^= columns_.GetHashCode();
		num ^= rows_.GetHashCode();
		num ^= cells_.GetHashCode();
		num ^= cellStyles_.GetHashCode();
		num ^= merges_.GetHashCode();
		num ^= cellAttachments_.GetHashCode();
		if (Title.Length != 0)
		{
			num ^= Title.GetHashCode();
		}
		if (Note.Length != 0)
		{
			num ^= Note.GetHashCode();
		}
		if (HeaderHeights.Length != 0)
		{
			num ^= HeaderHeights.GetHashCode();
		}
		if (DefaultStyleId != 0L)
		{
			num ^= DefaultStyleId.GetHashCode();
		}
		if (PageSetup.Length != 0)
		{
			num ^= PageSetup.GetHashCode();
		}
		if (ConsolidateSettings.Length != 0)
		{
			num ^= ConsolidateSettings.GetHashCode();
		}
		if (BorderStyle != 0)
		{
			num ^= BorderStyle.GetHashCode();
		}
		if (FrozenCols != 0)
		{
			num ^= FrozenCols.GetHashCode();
		}
		if (HeaderMode != 0)
		{
			num ^= HeaderMode.GetHashCode();
		}
		if (CollectSource.Length != 0)
		{
			num ^= CollectSource.GetHashCode();
		}
		if (Locker != 0L)
		{
			num ^= Locker.GetHashCode();
		}
		if (FilterInfo.Length != 0)
		{
			num ^= FilterInfo.GetHashCode();
		}
		if (Foot.Length != 0)
		{
			num ^= Foot.GetHashCode();
		}
		if (RowOwnerExclusive)
		{
			num ^= RowOwnerExclusive.GetHashCode();
		}
		if (RowOwnerLoad)
		{
			num ^= RowOwnerLoad.GetHashCode();
		}
		if (RowOwnerLoadShare.Length != 0)
		{
			num ^= RowOwnerLoadShare.GetHashCode();
		}
		if (Ticket.Length != 0)
		{
			num ^= Ticket.GetHashCode();
		}
		if (ControlFormula.Length != 0)
		{
			num ^= ControlFormula.GetHashCode();
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
		columns_.WriteTo(output, _repeated_columns_codec);
		rows_.WriteTo(output, _repeated_rows_codec);
		cells_.WriteTo(output, _repeated_cells_codec);
		cellStyles_.WriteTo(output, _repeated_cellStyles_codec);
		merges_.WriteTo(output, _repeated_merges_codec);
		cellAttachments_.WriteTo(output, _repeated_cellAttachments_codec);
		if (Title.Length != 0)
		{
			output.WriteRawTag(130, 1);
			output.WriteString(Title);
		}
		if (Note.Length != 0)
		{
			output.WriteRawTag(138, 1);
			output.WriteString(Note);
		}
		if (HeaderHeights.Length != 0)
		{
			output.WriteRawTag(146, 1);
			output.WriteString(HeaderHeights);
		}
		if (DefaultStyleId != 0L)
		{
			output.WriteRawTag(152, 1);
			output.WriteInt64(DefaultStyleId);
		}
		if (PageSetup.Length != 0)
		{
			output.WriteRawTag(162, 1);
			output.WriteString(PageSetup);
		}
		if (ConsolidateSettings.Length != 0)
		{
			output.WriteRawTag(170, 1);
			output.WriteString(ConsolidateSettings);
		}
		if (BorderStyle != 0)
		{
			output.WriteRawTag(176, 1);
			output.WriteInt32(BorderStyle);
		}
		if (FrozenCols != 0)
		{
			output.WriteRawTag(184, 1);
			output.WriteInt32(FrozenCols);
		}
		if (HeaderMode != 0)
		{
			output.WriteRawTag(192, 1);
			output.WriteInt32(HeaderMode);
		}
		if (CollectSource.Length != 0)
		{
			output.WriteRawTag(202, 1);
			output.WriteString(CollectSource);
		}
		if (Locker != 0L)
		{
			output.WriteRawTag(208, 1);
			output.WriteInt64(Locker);
		}
		if (FilterInfo.Length != 0)
		{
			output.WriteRawTag(218, 1);
			output.WriteString(FilterInfo);
		}
		if (Foot.Length != 0)
		{
			output.WriteRawTag(226, 1);
			output.WriteString(Foot);
		}
		if (RowOwnerExclusive)
		{
			output.WriteRawTag(232, 1);
			output.WriteBool(RowOwnerExclusive);
		}
		if (RowOwnerLoad)
		{
			output.WriteRawTag(240, 1);
			output.WriteBool(RowOwnerLoad);
		}
		if (RowOwnerLoadShare.Length != 0)
		{
			output.WriteRawTag(250, 1);
			output.WriteBytes(RowOwnerLoadShare);
		}
		if (Ticket.Length != 0)
		{
			output.WriteRawTag(130, 2);
			output.WriteString(Ticket);
		}
		if (ControlFormula.Length != 0)
		{
			output.WriteRawTag(138, 2);
			output.WriteString(ControlFormula);
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
		num += columns_.CalculateSize(_repeated_columns_codec);
		num += rows_.CalculateSize(_repeated_rows_codec);
		num += cells_.CalculateSize(_repeated_cells_codec);
		num += cellStyles_.CalculateSize(_repeated_cellStyles_codec);
		num += merges_.CalculateSize(_repeated_merges_codec);
		num += cellAttachments_.CalculateSize(_repeated_cellAttachments_codec);
		if (Title.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Title);
		}
		if (Note.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Note);
		}
		if (HeaderHeights.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(HeaderHeights);
		}
		if (DefaultStyleId != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(DefaultStyleId);
		}
		if (PageSetup.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(PageSetup);
		}
		if (ConsolidateSettings.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(ConsolidateSettings);
		}
		if (BorderStyle != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(BorderStyle);
		}
		if (FrozenCols != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(FrozenCols);
		}
		if (HeaderMode != 0)
		{
			num += 2 + CodedOutputStream.ComputeInt32Size(HeaderMode);
		}
		if (CollectSource.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(CollectSource);
		}
		if (Locker != 0L)
		{
			num += 2 + CodedOutputStream.ComputeInt64Size(Locker);
		}
		if (FilterInfo.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(FilterInfo);
		}
		if (Foot.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Foot);
		}
		if (RowOwnerExclusive)
		{
			num += 3;
		}
		if (RowOwnerLoad)
		{
			num += 3;
		}
		if (RowOwnerLoadShare.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeBytesSize(RowOwnerLoadShare);
		}
		if (Ticket.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(Ticket);
		}
		if (ControlFormula.Length != 0)
		{
			num += 2 + CodedOutputStream.ComputeStringSize(ControlFormula);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	public void MergeFrom(PushTable other)
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
			columns_.Add(other.columns_);
			rows_.Add(other.rows_);
			cells_.Add(other.cells_);
			cellStyles_.Add(other.cellStyles_);
			merges_.Add(other.merges_);
			cellAttachments_.Add(other.cellAttachments_);
			if (other.Title.Length != 0)
			{
				Title = other.Title;
			}
			if (other.Note.Length != 0)
			{
				Note = other.Note;
			}
			if (other.HeaderHeights.Length != 0)
			{
				HeaderHeights = other.HeaderHeights;
			}
			if (other.DefaultStyleId != 0L)
			{
				DefaultStyleId = other.DefaultStyleId;
			}
			if (other.PageSetup.Length != 0)
			{
				PageSetup = other.PageSetup;
			}
			if (other.ConsolidateSettings.Length != 0)
			{
				ConsolidateSettings = other.ConsolidateSettings;
			}
			if (other.BorderStyle != 0)
			{
				BorderStyle = other.BorderStyle;
			}
			if (other.FrozenCols != 0)
			{
				FrozenCols = other.FrozenCols;
			}
			if (other.HeaderMode != 0)
			{
				HeaderMode = other.HeaderMode;
			}
			if (other.CollectSource.Length != 0)
			{
				CollectSource = other.CollectSource;
			}
			if (other.Locker != 0L)
			{
				Locker = other.Locker;
			}
			if (other.FilterInfo.Length != 0)
			{
				FilterInfo = other.FilterInfo;
			}
			if (other.Foot.Length != 0)
			{
				Foot = other.Foot;
			}
			if (other.RowOwnerExclusive)
			{
				RowOwnerExclusive = other.RowOwnerExclusive;
			}
			if (other.RowOwnerLoad)
			{
				RowOwnerLoad = other.RowOwnerLoad;
			}
			if (other.RowOwnerLoadShare.Length != 0)
			{
				RowOwnerLoadShare = other.RowOwnerLoadShare;
			}
			if (other.Ticket.Length != 0)
			{
				Ticket = other.Ticket;
			}
			if (other.ControlFormula.Length != 0)
			{
				ControlFormula = other.ControlFormula;
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
				columns_.AddEntriesFrom(input, _repeated_columns_codec);
				break;
			case 50u:
				rows_.AddEntriesFrom(input, _repeated_rows_codec);
				break;
			case 58u:
				cells_.AddEntriesFrom(input, _repeated_cells_codec);
				break;
			case 66u:
				cellStyles_.AddEntriesFrom(input, _repeated_cellStyles_codec);
				break;
			case 74u:
				merges_.AddEntriesFrom(input, _repeated_merges_codec);
				break;
			case 82u:
				cellAttachments_.AddEntriesFrom(input, _repeated_cellAttachments_codec);
				break;
			case 130u:
				Title = input.ReadString();
				break;
			case 138u:
				Note = input.ReadString();
				break;
			case 146u:
				HeaderHeights = input.ReadString();
				break;
			case 152u:
				DefaultStyleId = input.ReadInt64();
				break;
			case 162u:
				PageSetup = input.ReadString();
				break;
			case 170u:
				ConsolidateSettings = input.ReadString();
				break;
			case 176u:
				BorderStyle = input.ReadInt32();
				break;
			case 184u:
				FrozenCols = input.ReadInt32();
				break;
			case 192u:
				HeaderMode = input.ReadInt32();
				break;
			case 202u:
				CollectSource = input.ReadString();
				break;
			case 208u:
				Locker = input.ReadInt64();
				break;
			case 218u:
				FilterInfo = input.ReadString();
				break;
			case 226u:
				Foot = input.ReadString();
				break;
			case 232u:
				RowOwnerExclusive = input.ReadBool();
				break;
			case 240u:
				RowOwnerLoad = input.ReadBool();
				break;
			case 250u:
				RowOwnerLoadShare = input.ReadBytes();
				break;
			case 258u:
				Ticket = input.ReadString();
				break;
			case 266u:
				ControlFormula = input.ReadString();
				break;
			}
		}
	}
}
