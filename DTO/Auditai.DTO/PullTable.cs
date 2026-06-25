using System;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public sealed class PullTable : IMessage<PullTable>, Google.Protobuf.IMessage, IEquatable<PullTable>, IDeepCloneable<PullTable>
{
	private static readonly MessageParser<PullTable> _parser = new MessageParser<PullTable>(() => new PullTable());

	private UnknownFieldSet _unknownFields;

	public const int NewCellsFieldNumber = 1;

	private static readonly FieldCodec<PullCell> _repeated_newCells_codec = FieldCodec.ForMessage(10u, PullCell.Parser);

	private readonly RepeatedField<PullCell> newCells_ = new RepeatedField<PullCell>();

	public const int DelCellsFieldNumber = 2;

	private static readonly FieldCodec<PullCell> _repeated_delCells_codec = FieldCodec.ForMessage(18u, PullCell.Parser);

	private readonly RepeatedField<PullCell> delCells_ = new RepeatedField<PullCell>();

	public const int ModCellsFieldNumber = 3;

	private static readonly FieldCodec<PullCell> _repeated_modCells_codec = FieldCodec.ForMessage(26u, PullCell.Parser);

	private readonly RepeatedField<PullCell> modCells_ = new RepeatedField<PullCell>();

	public const int NewColumnsFieldNumber = 4;

	private static readonly FieldCodec<PullColumn> _repeated_newColumns_codec = FieldCodec.ForMessage(34u, PullColumn.Parser);

	private readonly RepeatedField<PullColumn> newColumns_ = new RepeatedField<PullColumn>();

	public const int DelColumnsFieldNumber = 5;

	private static readonly FieldCodec<PullColumn> _repeated_delColumns_codec = FieldCodec.ForMessage(42u, PullColumn.Parser);

	private readonly RepeatedField<PullColumn> delColumns_ = new RepeatedField<PullColumn>();

	public const int ModColumnsFieldNumber = 6;

	private static readonly FieldCodec<PullColumn> _repeated_modColumns_codec = FieldCodec.ForMessage(50u, PullColumn.Parser);

	private readonly RepeatedField<PullColumn> modColumns_ = new RepeatedField<PullColumn>();

	public const int NewRowsFieldNumber = 7;

	private static readonly FieldCodec<PullRow> _repeated_newRows_codec = FieldCodec.ForMessage(58u, PullRow.Parser);

	private readonly RepeatedField<PullRow> newRows_ = new RepeatedField<PullRow>();

	public const int DelRowsFieldNumber = 8;

	private static readonly FieldCodec<PullRow> _repeated_delRows_codec = FieldCodec.ForMessage(66u, PullRow.Parser);

	private readonly RepeatedField<PullRow> delRows_ = new RepeatedField<PullRow>();

	public const int ModRowsFieldNumber = 9;

	private static readonly FieldCodec<PullRow> _repeated_modRows_codec = FieldCodec.ForMessage(74u, PullRow.Parser);

	private readonly RepeatedField<PullRow> modRows_ = new RepeatedField<PullRow>();

	public const int CellStylesFieldNumber = 10;

	private static readonly FieldCodec<PullCellStyle> _repeated_cellStyles_codec = FieldCodec.ForMessage(82u, PullCellStyle.Parser);

	private readonly RepeatedField<PullCellStyle> cellStyles_ = new RepeatedField<PullCellStyle>();

	public const int NewMergesFieldNumber = 11;

	private static readonly FieldCodec<PullMerge> _repeated_newMerges_codec = FieldCodec.ForMessage(90u, PullMerge.Parser);

	private readonly RepeatedField<PullMerge> newMerges_ = new RepeatedField<PullMerge>();

	public const int DelMergesFieldNumber = 12;

	private static readonly FieldCodec<PullMerge> _repeated_delMerges_codec = FieldCodec.ForMessage(98u, PullMerge.Parser);

	private readonly RepeatedField<PullMerge> delMerges_ = new RepeatedField<PullMerge>();

	public const int NewCellPropsFieldNumber = 13;

	private static readonly FieldCodec<PullCellProp> _repeated_newCellProps_codec = FieldCodec.ForMessage(106u, PullCellProp.Parser);

	private readonly RepeatedField<PullCellProp> newCellProps_ = new RepeatedField<PullCellProp>();

	public const int ModCellPropsFieldNumber = 14;

	private static readonly FieldCodec<PullCellProp> _repeated_modCellProps_codec = FieldCodec.ForMessage(114u, PullCellProp.Parser);

	private readonly RepeatedField<PullCellProp> modCellProps_ = new RepeatedField<PullCellProp>();

	public const int DelCellPropsFieldNumber = 15;

	private static readonly FieldCodec<PullCellProp> _repeated_delCellProps_codec = FieldCodec.ForMessage(122u, PullCellProp.Parser);

	private readonly RepeatedField<PullCellProp> delCellProps_ = new RepeatedField<PullCellProp>();

	public const int TitleFieldNumber = 16;

	private OptionalString title_;

	public const int NoteFieldNumber = 17;

	private OptionalString note_;

	public const int HeaderHeightsFieldNumber = 18;

	private OptionalString headerHeights_;

	public const int DefaultStyleIdFieldNumber = 19;

	private NullableInt64 defaultStyleId_;

	public const int PageSetupFieldNumber = 20;

	private OptionalString pageSetup_;

	public const int ConsolidateSettingsFieldNumber = 21;

	private OptionalString consolidateSettings_;

	public const int BorderStyleFieldNumber = 22;

	private OptionalInt32 borderStyle_;

	public const int FrozenColsFieldNumber = 23;

	private OptionalInt32 frozenCols_;

	public const int HeaderModeFieldNumber = 24;

	private OptionalInt32 headerMode_;

	public const int CollectSourceFieldNumber = 25;

	private OptionalString collectSource_;

	public const int LockerFieldNumber = 26;

	private OptionalInt64 locker_;

	public const int FilterInfoFieldNumber = 27;

	private OptionalString filterInfo_;

	public const int FootFieldNumber = 28;

	private OptionalString foot_;

	public const int RowOwnerLoadShareFieldNumber = 29;

	private OptionalBytes rowOwnerLoadShare_;

	public const int TicketFieldNumber = 30;

	private OptionalString ticket_;

	public const int ControlFormulaFieldNumber = 31;

	private OptionalString controlFormula_;

	public const int ResultFieldNumber = 32;

	private string result_ = "";

	public const int VersionFieldNumber = 33;

	private int version_;

	[DebuggerNonUserCode]
	public static MessageParser<PullTable> Parser => _parser;

	[DebuggerNonUserCode]
	public static MessageDescriptor Descriptor => PulltableReflection.Descriptor.MessageTypes[12];

	[DebuggerNonUserCode]
	MessageDescriptor Google.Protobuf.IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	public RepeatedField<PullCell> NewCells => newCells_;

	[DebuggerNonUserCode]
	public RepeatedField<PullCell> DelCells => delCells_;

	[DebuggerNonUserCode]
	public RepeatedField<PullCell> ModCells => modCells_;

	[DebuggerNonUserCode]
	public RepeatedField<PullColumn> NewColumns => newColumns_;

	[DebuggerNonUserCode]
	public RepeatedField<PullColumn> DelColumns => delColumns_;

	[DebuggerNonUserCode]
	public RepeatedField<PullColumn> ModColumns => modColumns_;

	[DebuggerNonUserCode]
	public RepeatedField<PullRow> NewRows => newRows_;

	[DebuggerNonUserCode]
	public RepeatedField<PullRow> DelRows => delRows_;

	[DebuggerNonUserCode]
	public RepeatedField<PullRow> ModRows => modRows_;

	[DebuggerNonUserCode]
	public RepeatedField<PullCellStyle> CellStyles => cellStyles_;

	[DebuggerNonUserCode]
	public RepeatedField<PullMerge> NewMerges => newMerges_;

	[DebuggerNonUserCode]
	public RepeatedField<PullMerge> DelMerges => delMerges_;

	[DebuggerNonUserCode]
	public RepeatedField<PullCellProp> NewCellProps => newCellProps_;

	[DebuggerNonUserCode]
	public RepeatedField<PullCellProp> ModCellProps => modCellProps_;

	[DebuggerNonUserCode]
	public RepeatedField<PullCellProp> DelCellProps => delCellProps_;

	[DebuggerNonUserCode]
	public OptionalString Title
	{
		get
		{
			return title_;
		}
		set
		{
			title_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString Note
	{
		get
		{
			return note_;
		}
		set
		{
			note_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString HeaderHeights
	{
		get
		{
			return headerHeights_;
		}
		set
		{
			headerHeights_ = value;
		}
	}

	[DebuggerNonUserCode]
	public NullableInt64 DefaultStyleId
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
	public OptionalString PageSetup
	{
		get
		{
			return pageSetup_;
		}
		set
		{
			pageSetup_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString ConsolidateSettings
	{
		get
		{
			return consolidateSettings_;
		}
		set
		{
			consolidateSettings_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalInt32 BorderStyle
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
	public OptionalInt32 FrozenCols
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
	public OptionalInt32 HeaderMode
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
	public OptionalString FilterInfo
	{
		get
		{
			return filterInfo_;
		}
		set
		{
			filterInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString Foot
	{
		get
		{
			return foot_;
		}
		set
		{
			foot_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalBytes RowOwnerLoadShare
	{
		get
		{
			return rowOwnerLoadShare_;
		}
		set
		{
			rowOwnerLoadShare_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString Ticket
	{
		get
		{
			return ticket_;
		}
		set
		{
			ticket_ = value;
		}
	}

	[DebuggerNonUserCode]
	public OptionalString ControlFormula
	{
		get
		{
			return controlFormula_;
		}
		set
		{
			controlFormula_ = value;
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
	public PullTable()
	{
	}

	[DebuggerNonUserCode]
	public PullTable(PullTable other)
		: this()
	{
		newCells_ = other.newCells_.Clone();
		delCells_ = other.delCells_.Clone();
		modCells_ = other.modCells_.Clone();
		newColumns_ = other.newColumns_.Clone();
		delColumns_ = other.delColumns_.Clone();
		modColumns_ = other.modColumns_.Clone();
		newRows_ = other.newRows_.Clone();
		delRows_ = other.delRows_.Clone();
		modRows_ = other.modRows_.Clone();
		cellStyles_ = other.cellStyles_.Clone();
		newMerges_ = other.newMerges_.Clone();
		delMerges_ = other.delMerges_.Clone();
		newCellProps_ = other.newCellProps_.Clone();
		modCellProps_ = other.modCellProps_.Clone();
		delCellProps_ = other.delCellProps_.Clone();
		title_ = ((other.title_ != null) ? other.title_.Clone() : null);
		note_ = ((other.note_ != null) ? other.note_.Clone() : null);
		headerHeights_ = ((other.headerHeights_ != null) ? other.headerHeights_.Clone() : null);
		defaultStyleId_ = ((other.defaultStyleId_ != null) ? other.defaultStyleId_.Clone() : null);
		pageSetup_ = ((other.pageSetup_ != null) ? other.pageSetup_.Clone() : null);
		consolidateSettings_ = ((other.consolidateSettings_ != null) ? other.consolidateSettings_.Clone() : null);
		borderStyle_ = ((other.borderStyle_ != null) ? other.borderStyle_.Clone() : null);
		frozenCols_ = ((other.frozenCols_ != null) ? other.frozenCols_.Clone() : null);
		headerMode_ = ((other.headerMode_ != null) ? other.headerMode_.Clone() : null);
		collectSource_ = ((other.collectSource_ != null) ? other.collectSource_.Clone() : null);
		locker_ = ((other.locker_ != null) ? other.locker_.Clone() : null);
		filterInfo_ = ((other.filterInfo_ != null) ? other.filterInfo_.Clone() : null);
		foot_ = ((other.foot_ != null) ? other.foot_.Clone() : null);
		rowOwnerLoadShare_ = ((other.rowOwnerLoadShare_ != null) ? other.rowOwnerLoadShare_.Clone() : null);
		ticket_ = ((other.ticket_ != null) ? other.ticket_.Clone() : null);
		controlFormula_ = ((other.controlFormula_ != null) ? other.controlFormula_.Clone() : null);
		result_ = other.result_;
		version_ = other.version_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	public PullTable Clone()
	{
		return new PullTable(this);
	}

	[DebuggerNonUserCode]
	public override bool Equals(object other)
	{
		return Equals(other as PullTable);
	}

	[DebuggerNonUserCode]
	public bool Equals(PullTable other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!newCells_.Equals(other.newCells_))
		{
			return false;
		}
		if (!delCells_.Equals(other.delCells_))
		{
			return false;
		}
		if (!modCells_.Equals(other.modCells_))
		{
			return false;
		}
		if (!newColumns_.Equals(other.newColumns_))
		{
			return false;
		}
		if (!delColumns_.Equals(other.delColumns_))
		{
			return false;
		}
		if (!modColumns_.Equals(other.modColumns_))
		{
			return false;
		}
		if (!newRows_.Equals(other.newRows_))
		{
			return false;
		}
		if (!delRows_.Equals(other.delRows_))
		{
			return false;
		}
		if (!modRows_.Equals(other.modRows_))
		{
			return false;
		}
		if (!cellStyles_.Equals(other.cellStyles_))
		{
			return false;
		}
		if (!newMerges_.Equals(other.newMerges_))
		{
			return false;
		}
		if (!delMerges_.Equals(other.delMerges_))
		{
			return false;
		}
		if (!newCellProps_.Equals(other.newCellProps_))
		{
			return false;
		}
		if (!modCellProps_.Equals(other.modCellProps_))
		{
			return false;
		}
		if (!delCellProps_.Equals(other.delCellProps_))
		{
			return false;
		}
		if (!object.Equals(Title, other.Title))
		{
			return false;
		}
		if (!object.Equals(Note, other.Note))
		{
			return false;
		}
		if (!object.Equals(HeaderHeights, other.HeaderHeights))
		{
			return false;
		}
		if (!object.Equals(DefaultStyleId, other.DefaultStyleId))
		{
			return false;
		}
		if (!object.Equals(PageSetup, other.PageSetup))
		{
			return false;
		}
		if (!object.Equals(ConsolidateSettings, other.ConsolidateSettings))
		{
			return false;
		}
		if (!object.Equals(BorderStyle, other.BorderStyle))
		{
			return false;
		}
		if (!object.Equals(FrozenCols, other.FrozenCols))
		{
			return false;
		}
		if (!object.Equals(HeaderMode, other.HeaderMode))
		{
			return false;
		}
		if (!object.Equals(CollectSource, other.CollectSource))
		{
			return false;
		}
		if (!object.Equals(Locker, other.Locker))
		{
			return false;
		}
		if (!object.Equals(FilterInfo, other.FilterInfo))
		{
			return false;
		}
		if (!object.Equals(Foot, other.Foot))
		{
			return false;
		}
		if (!object.Equals(RowOwnerLoadShare, other.RowOwnerLoadShare))
		{
			return false;
		}
		if (!object.Equals(Ticket, other.Ticket))
		{
			return false;
		}
		if (!object.Equals(ControlFormula, other.ControlFormula))
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
		num ^= newCells_.GetHashCode();
		num ^= delCells_.GetHashCode();
		num ^= modCells_.GetHashCode();
		num ^= newColumns_.GetHashCode();
		num ^= delColumns_.GetHashCode();
		num ^= modColumns_.GetHashCode();
		num ^= newRows_.GetHashCode();
		num ^= delRows_.GetHashCode();
		num ^= modRows_.GetHashCode();
		num ^= cellStyles_.GetHashCode();
		num ^= newMerges_.GetHashCode();
		num ^= delMerges_.GetHashCode();
		num ^= newCellProps_.GetHashCode();
		num ^= modCellProps_.GetHashCode();
		num ^= delCellProps_.GetHashCode();
		if (title_ != null)
		{
			num ^= Title.GetHashCode();
		}
		if (note_ != null)
		{
			num ^= Note.GetHashCode();
		}
		if (headerHeights_ != null)
		{
			num ^= HeaderHeights.GetHashCode();
		}
		if (defaultStyleId_ != null)
		{
			num ^= DefaultStyleId.GetHashCode();
		}
		if (pageSetup_ != null)
		{
			num ^= PageSetup.GetHashCode();
		}
		if (consolidateSettings_ != null)
		{
			num ^= ConsolidateSettings.GetHashCode();
		}
		if (borderStyle_ != null)
		{
			num ^= BorderStyle.GetHashCode();
		}
		if (frozenCols_ != null)
		{
			num ^= FrozenCols.GetHashCode();
		}
		if (headerMode_ != null)
		{
			num ^= HeaderMode.GetHashCode();
		}
		if (collectSource_ != null)
		{
			num ^= CollectSource.GetHashCode();
		}
		if (locker_ != null)
		{
			num ^= Locker.GetHashCode();
		}
		if (filterInfo_ != null)
		{
			num ^= FilterInfo.GetHashCode();
		}
		if (foot_ != null)
		{
			num ^= Foot.GetHashCode();
		}
		if (rowOwnerLoadShare_ != null)
		{
			num ^= RowOwnerLoadShare.GetHashCode();
		}
		if (ticket_ != null)
		{
			num ^= Ticket.GetHashCode();
		}
		if (controlFormula_ != null)
		{
			num ^= ControlFormula.GetHashCode();
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
		newCells_.WriteTo(output, _repeated_newCells_codec);
		delCells_.WriteTo(output, _repeated_delCells_codec);
		modCells_.WriteTo(output, _repeated_modCells_codec);
		newColumns_.WriteTo(output, _repeated_newColumns_codec);
		delColumns_.WriteTo(output, _repeated_delColumns_codec);
		modColumns_.WriteTo(output, _repeated_modColumns_codec);
		newRows_.WriteTo(output, _repeated_newRows_codec);
		delRows_.WriteTo(output, _repeated_delRows_codec);
		modRows_.WriteTo(output, _repeated_modRows_codec);
		cellStyles_.WriteTo(output, _repeated_cellStyles_codec);
		newMerges_.WriteTo(output, _repeated_newMerges_codec);
		delMerges_.WriteTo(output, _repeated_delMerges_codec);
		newCellProps_.WriteTo(output, _repeated_newCellProps_codec);
		modCellProps_.WriteTo(output, _repeated_modCellProps_codec);
		delCellProps_.WriteTo(output, _repeated_delCellProps_codec);
		if (title_ != null)
		{
			output.WriteRawTag(130, 1);
			output.WriteMessage(Title);
		}
		if (note_ != null)
		{
			output.WriteRawTag(138, 1);
			output.WriteMessage(Note);
		}
		if (headerHeights_ != null)
		{
			output.WriteRawTag(146, 1);
			output.WriteMessage(HeaderHeights);
		}
		if (defaultStyleId_ != null)
		{
			output.WriteRawTag(154, 1);
			output.WriteMessage(DefaultStyleId);
		}
		if (pageSetup_ != null)
		{
			output.WriteRawTag(162, 1);
			output.WriteMessage(PageSetup);
		}
		if (consolidateSettings_ != null)
		{
			output.WriteRawTag(170, 1);
			output.WriteMessage(ConsolidateSettings);
		}
		if (borderStyle_ != null)
		{
			output.WriteRawTag(178, 1);
			output.WriteMessage(BorderStyle);
		}
		if (frozenCols_ != null)
		{
			output.WriteRawTag(186, 1);
			output.WriteMessage(FrozenCols);
		}
		if (headerMode_ != null)
		{
			output.WriteRawTag(194, 1);
			output.WriteMessage(HeaderMode);
		}
		if (collectSource_ != null)
		{
			output.WriteRawTag(202, 1);
			output.WriteMessage(CollectSource);
		}
		if (locker_ != null)
		{
			output.WriteRawTag(210, 1);
			output.WriteMessage(Locker);
		}
		if (filterInfo_ != null)
		{
			output.WriteRawTag(218, 1);
			output.WriteMessage(FilterInfo);
		}
		if (foot_ != null)
		{
			output.WriteRawTag(226, 1);
			output.WriteMessage(Foot);
		}
		if (rowOwnerLoadShare_ != null)
		{
			output.WriteRawTag(234, 1);
			output.WriteMessage(RowOwnerLoadShare);
		}
		if (ticket_ != null)
		{
			output.WriteRawTag(242, 1);
			output.WriteMessage(Ticket);
		}
		if (controlFormula_ != null)
		{
			output.WriteRawTag(250, 1);
			output.WriteMessage(ControlFormula);
		}
		if (Result.Length != 0)
		{
			output.WriteRawTag(130, 2);
			output.WriteString(Result);
		}
		if (Version != 0)
		{
			output.WriteRawTag(136, 2);
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
		num += newCells_.CalculateSize(_repeated_newCells_codec);
		num += delCells_.CalculateSize(_repeated_delCells_codec);
		num += modCells_.CalculateSize(_repeated_modCells_codec);
		num += newColumns_.CalculateSize(_repeated_newColumns_codec);
		num += delColumns_.CalculateSize(_repeated_delColumns_codec);
		num += modColumns_.CalculateSize(_repeated_modColumns_codec);
		num += newRows_.CalculateSize(_repeated_newRows_codec);
		num += delRows_.CalculateSize(_repeated_delRows_codec);
		num += modRows_.CalculateSize(_repeated_modRows_codec);
		num += cellStyles_.CalculateSize(_repeated_cellStyles_codec);
		num += newMerges_.CalculateSize(_repeated_newMerges_codec);
		num += delMerges_.CalculateSize(_repeated_delMerges_codec);
		num += newCellProps_.CalculateSize(_repeated_newCellProps_codec);
		num += modCellProps_.CalculateSize(_repeated_modCellProps_codec);
		num += delCellProps_.CalculateSize(_repeated_delCellProps_codec);
		if (title_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(Title);
		}
		if (note_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(Note);
		}
		if (headerHeights_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(HeaderHeights);
		}
		if (defaultStyleId_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(DefaultStyleId);
		}
		if (pageSetup_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(PageSetup);
		}
		if (consolidateSettings_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(ConsolidateSettings);
		}
		if (borderStyle_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(BorderStyle);
		}
		if (frozenCols_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(FrozenCols);
		}
		if (headerMode_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(HeaderMode);
		}
		if (collectSource_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(CollectSource);
		}
		if (locker_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(Locker);
		}
		if (filterInfo_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(FilterInfo);
		}
		if (foot_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(Foot);
		}
		if (rowOwnerLoadShare_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(RowOwnerLoadShare);
		}
		if (ticket_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(Ticket);
		}
		if (controlFormula_ != null)
		{
			num += 2 + CodedOutputStream.ComputeMessageSize(ControlFormula);
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
	public void MergeFrom(PullTable other)
	{
		if (other == null)
		{
			return;
		}
		newCells_.Add(other.newCells_);
		delCells_.Add(other.delCells_);
		modCells_.Add(other.modCells_);
		newColumns_.Add(other.newColumns_);
		delColumns_.Add(other.delColumns_);
		modColumns_.Add(other.modColumns_);
		newRows_.Add(other.newRows_);
		delRows_.Add(other.delRows_);
		modRows_.Add(other.modRows_);
		cellStyles_.Add(other.cellStyles_);
		newMerges_.Add(other.newMerges_);
		delMerges_.Add(other.delMerges_);
		newCellProps_.Add(other.newCellProps_);
		modCellProps_.Add(other.modCellProps_);
		delCellProps_.Add(other.delCellProps_);
		if (other.title_ != null)
		{
			if (title_ == null)
			{
				Title = new OptionalString();
			}
			Title.MergeFrom(other.Title);
		}
		if (other.note_ != null)
		{
			if (note_ == null)
			{
				Note = new OptionalString();
			}
			Note.MergeFrom(other.Note);
		}
		if (other.headerHeights_ != null)
		{
			if (headerHeights_ == null)
			{
				HeaderHeights = new OptionalString();
			}
			HeaderHeights.MergeFrom(other.HeaderHeights);
		}
		if (other.defaultStyleId_ != null)
		{
			if (defaultStyleId_ == null)
			{
				DefaultStyleId = new NullableInt64();
			}
			DefaultStyleId.MergeFrom(other.DefaultStyleId);
		}
		if (other.pageSetup_ != null)
		{
			if (pageSetup_ == null)
			{
				PageSetup = new OptionalString();
			}
			PageSetup.MergeFrom(other.PageSetup);
		}
		if (other.consolidateSettings_ != null)
		{
			if (consolidateSettings_ == null)
			{
				ConsolidateSettings = new OptionalString();
			}
			ConsolidateSettings.MergeFrom(other.ConsolidateSettings);
		}
		if (other.borderStyle_ != null)
		{
			if (borderStyle_ == null)
			{
				BorderStyle = new OptionalInt32();
			}
			BorderStyle.MergeFrom(other.BorderStyle);
		}
		if (other.frozenCols_ != null)
		{
			if (frozenCols_ == null)
			{
				FrozenCols = new OptionalInt32();
			}
			FrozenCols.MergeFrom(other.FrozenCols);
		}
		if (other.headerMode_ != null)
		{
			if (headerMode_ == null)
			{
				HeaderMode = new OptionalInt32();
			}
			HeaderMode.MergeFrom(other.HeaderMode);
		}
		if (other.collectSource_ != null)
		{
			if (collectSource_ == null)
			{
				CollectSource = new OptionalString();
			}
			CollectSource.MergeFrom(other.CollectSource);
		}
		if (other.locker_ != null)
		{
			if (locker_ == null)
			{
				Locker = new OptionalInt64();
			}
			Locker.MergeFrom(other.Locker);
		}
		if (other.filterInfo_ != null)
		{
			if (filterInfo_ == null)
			{
				FilterInfo = new OptionalString();
			}
			FilterInfo.MergeFrom(other.FilterInfo);
		}
		if (other.foot_ != null)
		{
			if (foot_ == null)
			{
				Foot = new OptionalString();
			}
			Foot.MergeFrom(other.Foot);
		}
		if (other.rowOwnerLoadShare_ != null)
		{
			if (rowOwnerLoadShare_ == null)
			{
				RowOwnerLoadShare = new OptionalBytes();
			}
			RowOwnerLoadShare.MergeFrom(other.RowOwnerLoadShare);
		}
		if (other.ticket_ != null)
		{
			if (ticket_ == null)
			{
				Ticket = new OptionalString();
			}
			Ticket.MergeFrom(other.Ticket);
		}
		if (other.controlFormula_ != null)
		{
			if (controlFormula_ == null)
			{
				ControlFormula = new OptionalString();
			}
			ControlFormula.MergeFrom(other.ControlFormula);
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
				newCells_.AddEntriesFrom(input, _repeated_newCells_codec);
				break;
			case 18u:
				delCells_.AddEntriesFrom(input, _repeated_delCells_codec);
				break;
			case 26u:
				modCells_.AddEntriesFrom(input, _repeated_modCells_codec);
				break;
			case 34u:
				newColumns_.AddEntriesFrom(input, _repeated_newColumns_codec);
				break;
			case 42u:
				delColumns_.AddEntriesFrom(input, _repeated_delColumns_codec);
				break;
			case 50u:
				modColumns_.AddEntriesFrom(input, _repeated_modColumns_codec);
				break;
			case 58u:
				newRows_.AddEntriesFrom(input, _repeated_newRows_codec);
				break;
			case 66u:
				delRows_.AddEntriesFrom(input, _repeated_delRows_codec);
				break;
			case 74u:
				modRows_.AddEntriesFrom(input, _repeated_modRows_codec);
				break;
			case 82u:
				cellStyles_.AddEntriesFrom(input, _repeated_cellStyles_codec);
				break;
			case 90u:
				newMerges_.AddEntriesFrom(input, _repeated_newMerges_codec);
				break;
			case 98u:
				delMerges_.AddEntriesFrom(input, _repeated_delMerges_codec);
				break;
			case 106u:
				newCellProps_.AddEntriesFrom(input, _repeated_newCellProps_codec);
				break;
			case 114u:
				modCellProps_.AddEntriesFrom(input, _repeated_modCellProps_codec);
				break;
			case 122u:
				delCellProps_.AddEntriesFrom(input, _repeated_delCellProps_codec);
				break;
			case 130u:
				if (title_ == null)
				{
					Title = new OptionalString();
				}
				input.ReadMessage(Title);
				break;
			case 138u:
				if (note_ == null)
				{
					Note = new OptionalString();
				}
				input.ReadMessage(Note);
				break;
			case 146u:
				if (headerHeights_ == null)
				{
					HeaderHeights = new OptionalString();
				}
				input.ReadMessage(HeaderHeights);
				break;
			case 154u:
				if (defaultStyleId_ == null)
				{
					DefaultStyleId = new NullableInt64();
				}
				input.ReadMessage(DefaultStyleId);
				break;
			case 162u:
				if (pageSetup_ == null)
				{
					PageSetup = new OptionalString();
				}
				input.ReadMessage(PageSetup);
				break;
			case 170u:
				if (consolidateSettings_ == null)
				{
					ConsolidateSettings = new OptionalString();
				}
				input.ReadMessage(ConsolidateSettings);
				break;
			case 178u:
				if (borderStyle_ == null)
				{
					BorderStyle = new OptionalInt32();
				}
				input.ReadMessage(BorderStyle);
				break;
			case 186u:
				if (frozenCols_ == null)
				{
					FrozenCols = new OptionalInt32();
				}
				input.ReadMessage(FrozenCols);
				break;
			case 194u:
				if (headerMode_ == null)
				{
					HeaderMode = new OptionalInt32();
				}
				input.ReadMessage(HeaderMode);
				break;
			case 202u:
				if (collectSource_ == null)
				{
					CollectSource = new OptionalString();
				}
				input.ReadMessage(CollectSource);
				break;
			case 210u:
				if (locker_ == null)
				{
					Locker = new OptionalInt64();
				}
				input.ReadMessage(Locker);
				break;
			case 218u:
				if (filterInfo_ == null)
				{
					FilterInfo = new OptionalString();
				}
				input.ReadMessage(FilterInfo);
				break;
			case 226u:
				if (foot_ == null)
				{
					Foot = new OptionalString();
				}
				input.ReadMessage(Foot);
				break;
			case 234u:
				if (rowOwnerLoadShare_ == null)
				{
					RowOwnerLoadShare = new OptionalBytes();
				}
				input.ReadMessage(RowOwnerLoadShare);
				break;
			case 242u:
				if (ticket_ == null)
				{
					Ticket = new OptionalString();
				}
				input.ReadMessage(Ticket);
				break;
			case 250u:
				if (controlFormula_ == null)
				{
					ControlFormula = new OptionalString();
				}
				input.ReadMessage(ControlFormula);
				break;
			case 258u:
				Result = input.ReadString();
				break;
			case 264u:
				Version = input.ReadInt32();
				break;
			}
		}
	}
}
