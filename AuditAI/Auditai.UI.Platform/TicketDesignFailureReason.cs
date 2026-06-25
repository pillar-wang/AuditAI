namespace Auditai.UI.Platform;

public enum TicketDesignFailureReason
{
    None,
    NoField,
    InvalidStartRow,
    RowNotContinuous,
    InvalidCol,
    InvalidEndRow,
    InvalidDataRowVerticalMerge,
    InvalidDataRowMergeStartRow,
    DataRowMergeNotContinuous,
    DataRowMergeInvalidRightCol,
    InvalidDataRowMergeEndRow,
    InvalidFormula,
    DataRowContainsKeyCell,
    DataRowMergeInvalidLeftCol,
    FieldCountNotEqual,
    FieldCellsNotSameKind,
    LevelNotSupported,
    DataPartNothing,
    TitlePartIncludeDataPartField,
    FooterPartIncludeDataPartField,
    TitlePartExistRepeatField,
    FooterPartExistRepeatField,
    FooterPartExistTitleRepeatField,
    ColumnHeaderRowIncludeField,
    ColumnHeaderRowExistFormula,
    MergeRangeCrossColumnHeaderAndDataArea,
    MergeRangeCrossDynamicRowAndFixedRow,
    WriteTicketFormulaDataToTableCell
}