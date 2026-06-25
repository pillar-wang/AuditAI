﻿﻿﻿﻿﻿﻿﻿namespace Auditai.UI.Platform;

public static class AppCommands
{
	public static AppCommandManageProjects ManageProjects { get; } = new AppCommandManageProjects();


	public static AppCommandSyncProject SyncProject { get; } = new AppCommandSyncProject();


	public static AppCommandProjectMembers ProjectEdit { get; } = new AppCommandProjectMembers();


	public static AppCommandUserInfo UserInfo { get; } = new AppCommandUserInfo();


	public static AppCommandSwitchTeam SwitchTeam { get; } = new AppCommandSwitchTeam();


	public static AppCommandTeamUsers TeamUsers { get; } = new AppCommandTeamUsers();


	public static AppCommandChangePassword ChangePassword { get; } = new AppCommandChangePassword();


	public static AppCommandAccessControl AccessControl { get; } = new AppCommandAccessControl();


	public static AppCommandMoveUp MoveUp { get; } = new AppCommandMoveUp();


	public static AppCommandMoveDown MoveDown { get; } = new AppCommandMoveDown();


	public static AppCommandHideNodes HideNodes { get; } = new AppCommandHideNodes();


	public static AppCommandRemoveNodes RemoveNodes { get; } = new AppCommandRemoveNodes();


	public static AppCommandManageSnapshots ManageSnapshots { get; } = new AppCommandManageSnapshots();


	public static AppCommandRecycleNode RecycleNode { get; } = new AppCommandRecycleNode();


	public static AppCommandManageVariables ManageVariables { get; } = new AppCommandManageVariables();


	public static AppCommandQuit Quit { get; } = new AppCommandQuit();


	public static AppCommandMakeUsbCollector MakeUsbCollector { get; } = new AppCommandMakeUsbCollector();


	public static AppCommandLaunchCollector LaunchCollector { get; } = new AppCommandLaunchCollector();


	public static AppCommandGenerateLedger GenerateLedger { get; } = new AppCommandGenerateLedger();


	public static AppCommandOpenLedger OpenLedger { get; } = new AppCommandOpenLedger();


	public static AppCommandMergeLedgers MergeLedgers { get; } = new AppCommandMergeLedgers();


	public static AppCommandBalanceSheet BalanceSheet { get; } = new AppCommandBalanceSheet();


	public static AppCommandMonthSummary MonthSummary { get; } = new AppCommandMonthSummary();


	public static AppCommandGeneralLedger GeneralLedger { get; } = new AppCommandGeneralLedger();


	public static AppCommandSubsidiaryLedger SubsidiaryLedger { get; } = new AppCommandSubsidiaryLedger();


	public static AppCommandVouchers Vouchers { get; } = new AppCommandVouchers();


	public static AppCommandMyFavorites MyFavorites { get; } = new AppCommandMyFavorites();


	public static AppCommandAgeAnalysis AgeAnalysis { get; } = new AppCommandAgeAnalysis();


	public static AppCommandTrendAnalysis TrendAnalysis { get; } = new AppCommandTrendAnalysis();


	public static AppCommandStructureAnalysis StructureAnalysis { get; } = new AppCommandStructureAnalysis();


	public static AppCommandFillToTable FillToTable { get; } = new AppCommandFillToTable();


	public static AppCommandLedgerOneClickCollect LedgerOneClickCollect { get; } = new AppCommandLedgerOneClickCollect();


	public static AppCommandLedgerPortrait LedgerPortrait { get; } = new AppCommandLedgerPortrait();


	public static AppCommandLedgerLandscape LedgerLandscape { get; } = new AppCommandLedgerLandscape();


	public static AppCommandLedgerPrintPreview LedgerPrintPreview { get; } = new AppCommandLedgerPrintPreview();


	public static AppCommandLedgerPrint LedgerPrint { get; } = new AppCommandLedgerPrint();


	public static AppCommandExport Export { get; } = new AppCommandExport();


	public static AppCommandTableStyle0 TableStyle0 { get; } = new AppCommandTableStyle0();


	public static AppCommandTableStyleNoLine TableStyleNoLine { get; } = new AppCommandTableStyleNoLine();


	public static AppCommandTableStyle1 TableStyle1 { get; } = new AppCommandTableStyle1();


	public static AppCommandTableStyle2 TableStyle2 { get; } = new AppCommandTableStyle2();


	public static AppCommandTableStyle3 TableStyle3 { get; } = new AppCommandTableStyle3();


	public static AppCommandTableStyle4 TableStyle4 { get; } = new AppCommandTableStyle4();


	public static AppCommandTableStyle TableStyle { get; } = new AppCommandTableStyle();

	public static AppCommandTableStyleCustom TableStyleCustom { get; } = new AppCommandTableStyleCustom();


	public static AppCommandBatchApplyTableStyle BatchApplyTableStyle { get; } = new AppCommandBatchApplyTableStyle();

	/// <summary>
	/// 待应用的自定义表格边框样式（由样式配置对话框设置，InsertModelTable 中消费后清空）。
	/// 仅当 IsCustomStyle 为 true 时有效。
	/// </summary>
	public static Auditai.Model.TableBorderStyle PendingCustomStyle;


	public static AppCommandPageView PageView { get; } = new AppCommandPageView();


	public static AppCommandDraftView DraftView { get; } = new AppCommandDraftView();


	public static AppCommandShowParagraphMarkers ShowParagraphMarkers { get; } = new AppCommandShowParagraphMarkers();


	public static AppCommandShowHorizontalRuler ShowHorizontalRuler { get; } = new AppCommandShowHorizontalRuler();


	public static AppCommandShowVerticalRuler ShowVerticalRuler { get; } = new AppCommandShowVerticalRuler();


	public static AppCommandShowDocumentNavigator ShowDocumentNavigator { get; } = new AppCommandShowDocumentNavigator();


	public static AppCommandZoom25 Zoom25 { get; } = new AppCommandZoom25();


	public static AppCommandZoom50 Zoom50 { get; } = new AppCommandZoom50();


	public static AppCommandZoom75 Zoom75 { get; } = new AppCommandZoom75();


	public static AppCommandZoom100 Zoom100 { get; } = new AppCommandZoom100();


	public static AppCommandZoom125 Zoom125 { get; } = new AppCommandZoom125();


	public static AppCommandZoom200 Zoom200 { get; } = new AppCommandZoom200();


	public static AppCommandZoom400 Zoom400 { get; } = new AppCommandZoom400();


	public static AppCommandZoomWholePage ZoomWholePage { get; } = new AppCommandZoomWholePage();


	public static AppCommandZoomPageWidth ZoomPageWidth { get; } = new AppCommandZoomPageWidth();


	public static AppCommandZoomTextWidth ZoomTextWidth { get; } = new AppCommandZoomTextWidth();


	public static AppCommandZoom Zoom { get; } = new AppCommandZoom();


	public static AppCommandShowNodeNumber ShowNodeNumber { get; } = new AppCommandShowNodeNumber();


	public static AppCommandShowFoot ShowFoot { get; } = new AppCommandShowFoot();


	public static AppCommandShowFormula ShowFormula { get; } = new AppCommandShowFormula();


	public static AppCommandShowValidation ShowValidation { get; } = new AppCommandShowValidation();


	public static AppCommandShowTooltip ShowTooltip { get; } = new AppCommandShowTooltip();


	public static AppCommandToggleFullscreen ToggleFullscreen { get; } = new AppCommandToggleFullscreen();


	public static AppCommandShowHelp ShowHelp { get; } = new AppCommandShowHelp();


	public static AppCommandLockTable LockTable { get; } = new AppCommandLockTable();


	public static AppCommandRowOwnerExclusive RowOwnerExclusive { get; } = new AppCommandRowOwnerExclusive();


	public static AppCommandRowOwnerLoad RowOwnerLoad { get; } = new AppCommandRowOwnerLoad();


	public static AppCommandFormatBrush FormatBrush { get; } = new AppCommandFormatBrush();


	public static AppCommandMoveUpRow MoveUpRow { get; } = new AppCommandMoveUpRow();


	public static AppCommandMoveDownRow MoveDownRow { get; } = new AppCommandMoveDownRow();


	public static AppCommandMoveUpTicketRow MoveUpTicketRow { get; } = new AppCommandMoveUpTicketRow();


	public static AppCommandMoveDownTicketRow MoveDownTicketRow { get; } = new AppCommandMoveDownTicketRow();


	public static AppCommandTicketColumnMoveLeft MoveLeftTicketColumn { get; } = new AppCommandTicketColumnMoveLeft();


	public static AppCommandTicketColumnMoveRight MoveRightTicketColumn { get; } = new AppCommandTicketColumnMoveRight();


	public static AppCommandIncreaseRowHeight IncreaseRowHeight { get; } = new AppCommandIncreaseRowHeight();


	public static AppCommandDecreaseRowHeight DecreaseRowHeight { get; } = new AppCommandDecreaseRowHeight();


	public static AppCommandMoveLeftColumn MoveLeftColumn { get; } = new AppCommandMoveLeftColumn();


	public static AppCommandMoveRightColumn MoveRightColumn { get; } = new AppCommandMoveRightColumn();


	public static AppCommandIncreaseColumnWidth IncreaseColumnWidth { get; } = new AppCommandIncreaseColumnWidth();


	public static AppCommandDecreaseColumnWidth DecreaseColumnWidth { get; } = new AppCommandDecreaseColumnWidth();


	public static AppCommandMergeCells MergeCells { get; } = new AppCommandMergeCells();


	public static AppCommandSplitCells SplitCells { get; } = new AppCommandSplitCells();


	public static AppCommandTableFont TableFont { get; } = new AppCommandTableFont();


	public static AppCommandTableFontSize TableFontSize { get; } = new AppCommandTableFontSize();


	public static AppCommandForeColor ForeColor { get; } = new AppCommandForeColor();


	public static AppCommandBold Bold { get; } = new AppCommandBold();


	public static AppCommandGrowFont GrowFont { get; } = new AppCommandGrowFont();


	public static AppCommandBackColor BackColor { get; } = new AppCommandBackColor();


	public static AppCommandItalic Italic { get; } = new AppCommandItalic();


	public static AppCommandShrinkFont ShrinkFont { get; } = new AppCommandShrinkFont();


	public static AppCommandAlignTopLeft AlignTopLeft { get; } = new AppCommandAlignTopLeft();


	public static AppCommandAlignMiddleLeft AlignMiddleLeft { get; } = new AppCommandAlignMiddleLeft();


	public static AppCommandAlignBottomLeft AlignBottomLeft { get; } = new AppCommandAlignBottomLeft();


	public static AppCommandAlignTopCenter AlignTopCenter { get; } = new AppCommandAlignTopCenter();


	public static AppCommandAlignMiddleCenter AlignMiddleCenter { get; } = new AppCommandAlignMiddleCenter();


	public static AppCommandAlignBottomCenter AlignBottomCenter { get; } = new AppCommandAlignBottomCenter();


	public static AppCommandAlignTopRight AlignTopRight { get; } = new AppCommandAlignTopRight();


	public static AppCommandAlignMiddleRight AlignMiddleRight { get; } = new AppCommandAlignMiddleRight();


	public static AppCommandAlignBottomRight AlignBottomRight { get; } = new AppCommandAlignBottomRight();


	public static AppCommandUnindent Unindent { get; } = new AppCommandUnindent();


	public static AppCommandIndent Indent { get; } = new AppCommandIndent();


	public static AppCommandDataFormatText DataFormatText { get; } = new AppCommandDataFormatText();


	public static AppCommandDataFormatNumber DataFormatNumber { get; } = new AppCommandDataFormatNumber();


	public static AppCommandDataFormatComma DataFormatComma { get; } = new AppCommandDataFormatComma();


	public static AppCommandDataFormatDollar DataFormatDollar { get; } = new AppCommandDataFormatDollar();


	public static AppCommandDataFormatRmb DataFormatRmb { get; } = new AppCommandDataFormatRmb();


	public static AppCommandDataFormatPercent DataFormatPercent { get; } = new AppCommandDataFormatPercent();


	public static AppCommandDataFormatNumeric DataFormatNumeric { get; } = new AppCommandDataFormatNumeric();


	public static AppCommandDataFormatDateChinese DataFormatDateChinese { get; } = new AppCommandDataFormatDateChinese();


	public static AppCommandDataFormatDateDash DataFormatDateDash { get; } = new AppCommandDataFormatDateDash();


	public static AppCommandDataFormatDateSlash DataFormatDateSlash { get; } = new AppCommandDataFormatDateSlash();


	public static AppCommandDataFormatDateDot DataFormatDateDot { get; } = new AppCommandDataFormatDateDot();


	public static AppCommandDataFormatDateYearMonthChinese DataFormatDateYearMonthChinese { get; } = new AppCommandDataFormatDateYearMonthChinese();


	public static AppCommandDataFormatDateYearMonthDash DataFormatDateYearMonthDash { get; } = new AppCommandDataFormatDateYearMonthDash();


	public static AppCommandDataFormatDateYearMonthSlash DataFormatDateYearMonthSlash { get; } = new AppCommandDataFormatDateYearMonthSlash();


	public static AppCommandDataFormatDateYearMonthDot DataFormatDateYearMonthDot { get; } = new AppCommandDataFormatDateYearMonthDot();


	public static AppCommandDataFormatDate DataFormatDate { get; } = new AppCommandDataFormatDate();


	public static AppCommandDataFormatTimeLong DataFormatTimeLong { get; } = new AppCommandDataFormatTimeLong();


	public static AppCommandDataFormatTimeLongChinese DataFormatTimeLongChinese { get; } = new AppCommandDataFormatTimeLongChinese();


	public static AppCommandDataFormatTimeShort DataFormatTimeShort { get; } = new AppCommandDataFormatTimeShort();


	public static AppCommandDataFormatTimeShortChinese DataFormatTimeShortChinese { get; } = new AppCommandDataFormatTimeShortChinese();


	public static AppCommandDataFormatTime DataFormatTime { get; } = new AppCommandDataFormatTime();


	public static AppCommandDataFormatBoolTickCross DataFormatBoolTickCross { get; } = new AppCommandDataFormatBoolTickCross();


	public static AppCommandDataFormatBoolYesNo DataFormatBoolYesNo { get; } = new AppCommandDataFormatBoolYesNo();


	public static AppCommandDataFormatBoolRightWrong DataFormatBoolRightWrong { get; } = new AppCommandDataFormatBoolRightWrong();


	public static AppCommandDataFormatBoolCheckBox DataFormatBoolCheckBox { get; } = new AppCommandDataFormatBoolCheckBox();


	public static AppCommandDataFormatBoolOnOff DataFormatBoolOnOff { get; } = new AppCommandDataFormatBoolOnOff();


	public static AppCommandDataFormatBoolean DataFormatBoolean { get; } = new AppCommandDataFormatBoolean();


	public static AppCommandDataFormat DataFormat { get; } = new AppCommandDataFormat();


	public static AppCommandZeroFormatEmpty ZeroFormatEmpty { get; } = new AppCommandZeroFormatEmpty();


	public static AppCommandZeroFormatZero ZeroFormatZero { get; } = new AppCommandZeroFormatZero();


	public static AppCommandZeroFormatDash ZeroFormatDash { get; } = new AppCommandZeroFormatDash();


	public static AppCommandZeroFormat ZeroFormat { get; } = new AppCommandZeroFormat();


	public static AppCommandMorePrecision MorePrecision { get; } = new AppCommandMorePrecision();


	public static AppCommandLessPrecision LessPrecision { get; } = new AppCommandLessPrecision();


	public static AppCommandAuxEdit AuxEdit { get; } = new AppCommandAuxEdit();


	public static AppCommandAutoNumber AutoNumber { get; } = new AppCommandAutoNumber();


	public static AppCommandEditComment EditComment { get; } = new AppCommandEditComment();


	public static AppCommandInsertSymbol InsertSymbol { get; } = new AppCommandInsertSymbol();


	public static AppCommandFind Find { get; } = new AppCommandFind();


	public static AppCommandDocumentLock DocumentLock { get; } = new AppCommandDocumentLock();


	public static AppCommandCopy Copy { get; } = new AppCommandCopy();


	public static AppCommandCut Cut { get; } = new AppCommandCut();


	public static AppCommandPaste Paste { get; } = new AppCommandPaste();


	public static AppCommandDocForeColor DocForeColor { get; } = new AppCommandDocForeColor();


	public static AppCommandDocBackColor DocBackColor { get; } = new AppCommandDocBackColor();


	public static AppCommandDoubleUnderline DoubleUnderline { get; } = new AppCommandDoubleUnderline();


	public static AppCommandUnderline Underline { get; } = new AppCommandUnderline();


	public static AppCommandDocumentFont DocumentFont { get; } = new AppCommandDocumentFont();


	public static AppCommandDocumentFontSize DocumentFontSize { get; } = new AppCommandDocumentFontSize();


	public static AppCommandSubscript Subscript { get; } = new AppCommandSubscript();


	public static AppCommandSuperscript Superscript { get; } = new AppCommandSuperscript();


	public static AppCommandLineSpacing1 LineSpacing1 { get; } = new AppCommandLineSpacing1();


	public static AppCommandLineSpacing15 LineSpacing15 { get; } = new AppCommandLineSpacing15();


	public static AppCommandLineSpacing2 LineSpacing2 { get; } = new AppCommandLineSpacing2();


	public static AppCommandLineSpacingMulti LineSpacingMulti { get; } = new AppCommandLineSpacingMulti();


	public static AppCommandLineSpacingAbsolute LineSpacingAbsolute { get; } = new AppCommandLineSpacingAbsolute();


	public static AppCommandLineSpacing LineSpacing { get; } = new AppCommandLineSpacing();


	public static AppCommandAboveSpacing0 AboveSpacing0 { get; } = new AppCommandAboveSpacing0();


	public static AppCommandAboveSpacing05 AboveSpacing05 { get; } = new AppCommandAboveSpacing05();


	public static AppCommandAboveSpacing1 AboveSpacing1 { get; } = new AppCommandAboveSpacing1();


	public static AppCommandAboveSpacing15 AboveSpacing15 { get; } = new AppCommandAboveSpacing15();


	public static AppCommandAboveSpacingMulti AboveSpacingMulti { get; } = new AppCommandAboveSpacingMulti();


	public static AppCommandAboveSpacingAbsolute AboveSpacingAbsolute { get; } = new AppCommandAboveSpacingAbsolute();


	public static AppCommandAboveSpacing AboveSpacing { get; } = new AppCommandAboveSpacing();


	public static AppCommandBelowSpacing0 BelowSpacing0 { get; } = new AppCommandBelowSpacing0();


	public static AppCommandBelowSpacing05 BelowSpacing05 { get; } = new AppCommandBelowSpacing05();


	public static AppCommandBelowSpacing1 BelowSpacing1 { get; } = new AppCommandBelowSpacing1();


	public static AppCommandBelowSpacing15 BelowSpacing15 { get; } = new AppCommandBelowSpacing15();


	public static AppCommandBelowSpacingMulti BelowSpacingMulti { get; } = new AppCommandBelowSpacingMulti();


	public static AppCommandBelowSpacingAbsolute BelowSpacingAbsolute { get; } = new AppCommandBelowSpacingAbsolute();


	public static AppCommandBelowSpacing BelowSpacing { get; } = new AppCommandBelowSpacing();


	public static AppCommandParagraphAlignLeft ParagraphAlignLeft { get; } = new AppCommandParagraphAlignLeft();


	public static AppCommandParagraphAlignRight ParagraphAlignRight { get; } = new AppCommandParagraphAlignRight();


	public static AppCommandParagraphAlignCenter ParagraphAlignCenter { get; } = new AppCommandParagraphAlignCenter();


	public static AppCommandParagraphAlignJustify ParagraphAlignJustify { get; } = new AppCommandParagraphAlignJustify();


	public static AppCommandParagraphAlign ParagraphAlign { get; } = new AppCommandParagraphAlign();


	public static AppCommandIndentFirstLine IndentFirstLine { get; } = new AppCommandIndentFirstLine();


	public static AppCommandUnindentFirstLine UnindentFirstLine { get; } = new AppCommandUnindentFirstLine();


	public static AppCommandIndentParagraph IndentParagraph { get; } = new AppCommandIndentParagraph();


	public static AppCommandUnindentParagraph UnindentParagraph { get; } = new AppCommandUnindentParagraph();


	public static AppCommandInsertTable InsertTable { get; } = new AppCommandInsertTable();


	public static AppCommandInsertImage InsertImage { get; } = new AppCommandInsertImage();


	public static AppCommandInsertHeader InsertHeader { get; } = new AppCommandInsertHeader();


	public static AppCommandInsertFooter InsertFooter { get; } = new AppCommandInsertFooter();


	public static AppCommandInsertTextFrame InsertTextFrame { get; } = new AppCommandInsertTextFrame();


	public static AppCommandInsertSectionBreak InsertSectionBreak { get; } = new AppCommandInsertSectionBreak();


	public static AppCommandInsertPageBreak InsertPageBreak { get; } = new AppCommandInsertPageBreak();


	public static AppCommandInsertMisc InsertMisc { get; } = new AppCommandInsertMisc();


	public static AppCommandTableStyleBrush TableStyleBrush { get; } = new AppCommandTableStyleBrush();


	public static AppCommandCollectByColumn CollectByColumn { get; } = new AppCommandCollectByColumn();


	public static AppCommandCollectByCell CollectByCell { get; } = new AppCommandCollectByCell();


	public static AppCommandExecuteCollect ExecuteCollect { get; } = new AppCommandExecuteCollect();


	public static AppCommandConsolidateSetting ConsolidateSetting { get; } = new AppCommandConsolidateSetting();


	public static AppCommandExecuteConsolidateFull ExecuteConsolidateFull { get; } = new AppCommandExecuteConsolidateFull();


	public static AppCommandExecuteConsolidateBrief ExecuteConsolidateBrief { get; } = new AppCommandExecuteConsolidateBrief();


	public static AppCommandExecuteConsolidate ExecuteConsolidate { get; } = new AppCommandExecuteConsolidate();


	public static AppCommandRefreshConsolidate RefreshConsolidate { get; } = new AppCommandRefreshConsolidate();


	public static AppCommandInsertRefTable InsertRefTable { get; } = new AppCommandInsertRefTable();


	public static AppCommandInsertVariable InsertVariable { get; } = new AppCommandInsertVariable();


	public static AppCommandRefreshRefTable RefreshRefTable { get; } = new AppCommandRefreshRefTable();


	public static AppCommandRefreshDocument RefreshDocument { get; } = new AppCommandRefreshDocument();


	public static AppCommandConfirmationSetting ConfirmationSetting { get; } = new AppCommandConfirmationSetting();


	public static AppCommandGenerateConfirmation GenerateConfirmation { get; } = new AppCommandGenerateConfirmation();


	public static AppCommandCalculateCurrentTable CalculateCurrentTable { get; } = new AppCommandCalculateCurrentTable();


	public static AppCommandCalculateAllTables CalculateAllTables { get; } = new AppCommandCalculateAllTables();


	public static AppCommandValidateCurrentTable ValidateCurrentTable { get; } = new AppCommandValidateCurrentTable();


	public static AppCommandValidateAllTables ValidateAllTables { get; } = new AppCommandValidateAllTables();


	public static AppCommandPreviousError PreviousError { get; } = new AppCommandPreviousError();


	public static AppCommandNextError NextError { get; } = new AppCommandNextError();


	public static AppCommandValidateDocument ValidateDocument { get; } = new AppCommandValidateDocument();


	public static AppCommandDocPreviousError DocPreviousError { get; } = new AppCommandDocPreviousError();


	public static AppCommandDocNextError DocNextError { get; } = new AppCommandDocNextError();


	public static AppCommandAddValidationPoint AddValidationPoint { get; } = new AppCommandAddValidationPoint();


	public static AppCommandRemoveValidationPoint RemoveValidationPoint { get; } = new AppCommandRemoveValidationPoint();


	public static AppCommandDocValidationMgmt DocValidationMgmt { get; } = new AppCommandDocValidationMgmt();


	public static AppCommandPaperA4 PaperA4 { get; } = new AppCommandPaperA4();


	public static AppCommandPaperB4 PaperB4 { get; } = new AppCommandPaperB4();


	public static AppCommandPaperA3 PaperA3 { get; } = new AppCommandPaperA3();


	public static AppCommandPaperB5 PaperB5 { get; } = new AppCommandPaperB5();


	public static AppCommandPaperCustom PaperCustom { get; } = new AppCommandPaperCustom();


	public static AppCommandPaper Paper { get; } = new AppCommandPaper();


	public static AppCommandScalePageWidth ScalePageWidth { get; } = new AppCommandScalePageWidth();


	public static AppCommandWidthScale WidthScale { get; } = new AppCommandWidthScale();


	public static AppCommandScalePageHeight ScalePageHeight { get; } = new AppCommandScalePageHeight();


	public static AppCommandHeightScale HeightScale { get; } = new AppCommandHeightScale();


	public static AppCommandMarginLeft MarginLeft { get; } = new AppCommandMarginLeft();


	public static AppCommandMarginTop MarginTop { get; } = new AppCommandMarginTop();


	public static AppCommandMarginBottom MarginBottom { get; } = new AppCommandMarginBottom();


	public static AppCommandMarginRight MarginRight { get; } = new AppCommandMarginRight();


	public static AppCommandHeaderMargin HeaderMargin { get; } = new AppCommandHeaderMargin();


	public static AppCommandFooterMargin FooterMargin { get; } = new AppCommandFooterMargin();


	public static AppCommandHeaderLeft HeaderLeft { get; } = new AppCommandHeaderLeft();


	public static AppCommandHeaderCenter HeaderCenter { get; } = new AppCommandHeaderCenter();


	public static AppCommandHeaderRight HeaderRight { get; } = new AppCommandHeaderRight();


	public static AppCommandFooterLeft FooterLeft { get; } = new AppCommandFooterLeft();


	public static AppCommandFooterCenter FooterCenter { get; } = new AppCommandFooterCenter();


	public static AppCommandFooterRight FooterRight { get; } = new AppCommandFooterRight();


	public static AppCommandStartPage StartPage { get; } = new AppCommandStartPage();


	public static AppCommandFootBorder FootBorder { get; } = new AppCommandFootBorder();


	public static AppCommandFixedColumns FixedColumns { get; } = new AppCommandFixedColumns();


	public static AppCommandMonochrome Monochrome { get; } = new AppCommandMonochrome();


	public static AppCommandTablePrintPreview TablePrintPreview { get; } = new AppCommandTablePrintPreview();


	public static AppCommandTableBatchPrint TableBatchPrint { get; } = new AppCommandTableBatchPrint();


	public static AppCommandTablePrint TablePrint { get; } = new AppCommandTablePrint();


	public static AppCommandFileBatchPrint FileBatchPrint { get; set; } = new AppCommandFileBatchPrint();


	public static AppCommandBatchExport BatchExport { get; } = new AppCommandBatchExport();


	public static AppCommandTableExportXlsx TableExportXlsx { get; } = new AppCommandTableExportXlsx();


	public static AppCommandExportPdf ExportPdf { get; } = new AppCommandExportPdf();


	public static AppCommandFileBatchExport FileBatchExport { get; } = new AppCommandFileBatchExport();


	public static AppCommandExportImage ExportImage { get; } = new AppCommandExportImage();


	public static AppCommandApplySelection ApplySelection { get; } = new AppCommandApplySelection();


	public static AppCommandApplyDocument ApplyDocument { get; } = new AppCommandApplyDocument();


	public static AppCommandPortrait Portrait { get; } = new AppCommandPortrait();


	public static AppCommandLandscape Landscape { get; } = new AppCommandLandscape();


	public static AppCommandPaperDirection PaperDirection { get; } = new AppCommandPaperDirection();


	public static AppCommandPage1Column Page1Column { get; } = new AppCommandPage1Column();


	public static AppCommandPage2Columns Page2Columns { get; } = new AppCommandPage2Columns();


	public static AppCommandPage3Columns Page3Columns { get; } = new AppCommandPage3Columns();


	public static AppCommandPageMultiColumns PageMultiColumns { get; } = new AppCommandPageMultiColumns();


	public static AppCommandPageColumns PageColumns { get; } = new AppCommandPageColumns();


	public static AppCommandExportDocx ExportDocx { get; } = new AppCommandExportDocx();


	public static AppCommandSystemSettings SystemSettings { get; } = new AppCommandSystemSettings();


	public static AppCommandHelp Help { get; } = new AppCommandHelp();


	public static AppCommandCheckUpdate CheckUpdate { get; } = new AppCommandCheckUpdate();


	public static AppCommandAbout About { get; } = new AppCommandAbout();


	public static AppCommandUndo Undo { get; } = new AppCommandUndo();


	public static AppCommandRedo Redo { get; } = new AppCommandRedo();


	public static AppCommandProjectMembersSmall ProjectMemberEditSmall { get; } = new AppCommandProjectMembersSmall();


	public static AppCommandSystemMessage SystemMessage { get; } = new AppCommandSystemMessage();


	public static AppCommandInformation Information { get; } = new AppCommandInformation();


	public static AppCommandBack Back { get; } = new AppCommandBack();


	public static AppCommandForward Forward { get; } = new AppCommandForward();


	public static AppCommandReload Reload { get; } = new AppCommandReload();


	public static AppCommandSaveProject SaveProject { get; } = new AppCommandSaveProject();


	public static AppCommandSyncProjectSmall SyncProjectSmall { get; } = new AppCommandSyncProjectSmall();


	public static AppCommandFormulaMap FormulaMap { get; } = new AppCommandFormulaMap();


	public static AppCommandShowTooltipSmall ShowTooltipSmall { get; } = new AppCommandShowTooltipSmall();


	public static AppCommandToggleFullscreenSmall ToggleFullscreenSmall { get; } = new AppCommandToggleFullscreenSmall();


	public static AppCommandShowHelpSmall ShowHelpSmall { get; } = new AppCommandShowHelpSmall();


	public static AppCommandHelpSmall HelpSmall { get; } = new AppCommandHelpSmall();


	public static AppCommandTheme Theme { get; } = new AppCommandTheme();


	public static AppCommandContactWay ContactWay { get; } = new AppCommandContactWay();


	public static AppCommandShowSidebar ShowSidebar { get; } = new AppCommandShowSidebar();


	public static AppCommandTest Test { get; } = new AppCommandTest();


	public static AppCommandFormulaTip1 FormulaTip1 { get; } = new AppCommandFormulaTip1();


	public static AppCommandFormulaTip2 FormulaTip2 { get; } = new AppCommandFormulaTip2();


	public static AppCommandFormulaTip3 FormulaTip3 { get; } = new AppCommandFormulaTip3();


	public static AppCommandFormulaCommit FormulaCommit { get; } = new AppCommandFormulaCommit();


	public static AppCommandFormulaCancel FormulaCancel { get; } = new AppCommandFormulaCancel();


	public static AppCommandBatchColumnDuplicate BatchColumnDuplicate { get; } = new AppCommandBatchColumnDuplicate();


	public static AppCommandBatchColumnRemove BatchColumnRemove { get; } = new AppCommandBatchColumnRemove();


	public static AppCommandBatchColumnRename BatchColumnRename { get; } = new AppCommandBatchColumnRename();


	public static AppCommandGenerateBatchFormula GenerateBatchFormula { get; } = new AppCommandGenerateBatchFormula();


	public static AppCommandNodesIndexEdit NodesIndexEdit { get; } = new AppCommandNodesIndexEdit();


	public static AppCommandOneClickCollect OneClickCollect { get; } = new AppCommandOneClickCollect();


	public static AppCommandTicketFont TicketFont { get; } = new AppCommandTicketFont();


	public static AppCommandTicketFontSize TicketFontSize { get; } = new AppCommandTicketFontSize();


	public static AppCommandTicketForeColor TicketForeColor { get; } = new AppCommandTicketForeColor();


	public static AppCommandTicketBackColor TicketBackColor { get; } = new AppCommandTicketBackColor();


	public static AppCommandTicketBold TicketBold { get; } = new AppCommandTicketBold();


	public static AppCommandTicketItalic TicketItalic { get; } = new AppCommandTicketItalic();


	public static AppCommandTicketGrowFont TicketGrowFont { get; } = new AppCommandTicketGrowFont();


	public static AppCommandTicketShrinkFont TicketShrinkFont { get; } = new AppCommandTicketShrinkFont();


	public static AppCommandTicketAlignTopLeft TicketAlignTopLeft { get; } = new AppCommandTicketAlignTopLeft();


	public static AppCommandTicketAlignMiddleLeft TicketAlignMiddleLeft { get; } = new AppCommandTicketAlignMiddleLeft();


	public static AppCommandTicketAlignBottomLeft TicketAlignBottomLeft { get; } = new AppCommandTicketAlignBottomLeft();


	public static AppCommandTicketAlignTopCenter TicketAlignTopCenter { get; } = new AppCommandTicketAlignTopCenter();


	public static AppCommandTicketAlignMiddleCenter TicketAlignMiddleCenter { get; } = new AppCommandTicketAlignMiddleCenter();


	public static AppCommandTicketAlignBottomCenter TicketAlignBottomCenter { get; } = new AppCommandTicketAlignBottomCenter();


	public static AppCommandTicketAlignTopRight TicketAlignTopRight { get; } = new AppCommandTicketAlignTopRight();


	public static AppCommandTicketAlignMiddleRight TicketAlignMiddleRight { get; } = new AppCommandTicketAlignMiddleRight();


	public static AppCommandTicketAlignBottomRight TicketAlignBottomRight { get; } = new AppCommandTicketAlignBottomRight();


	public static AppCommandTicketUnindent TicketUnindent { get; } = new AppCommandTicketUnindent();


	public static AppCommandTicketIndent TicketIndent { get; } = new AppCommandTicketIndent();


	public static AppCommandTicketBorderTop TicketBorderTop { get; } = new AppCommandTicketBorderTop();


	public static AppCommandTicketBorderBottom TicketBorderBottom { get; } = new AppCommandTicketBorderBottom();


	public static AppCommandTicketBorderLeft TicketBorderLeft { get; } = new AppCommandTicketBorderLeft();


	public static AppCommandTicketBorderRight TicketBorderRight { get; } = new AppCommandTicketBorderRight();


	public static AppCommandTicketBorderNone TicketBorderNone { get; } = new AppCommandTicketBorderNone();


	public static AppCommandTicketBorderAll TicketBorderAll { get; } = new AppCommandTicketBorderAll();


	public static AppCommandTicketBorder1 TicketBorder1 { get; } = new AppCommandTicketBorder1();


	public static AppCommandTicketBorder2 TicketBorder2 { get; } = new AppCommandTicketBorder2();


	public static AppCommandTicketAdd TicketAdd { get; } = new AppCommandTicketAdd();


	public static AppCommandTicketDelete TicketDelete { get; } = new AppCommandTicketDelete();


	public static AppCommandTicketSave TicketSave { get; } = new AppCommandTicketSave();


	public static AppCommandTicketPrevious TicketPrevious { get; } = new AppCommandTicketPrevious();


	public static AppCommandTicketNext TicketNext { get; } = new AppCommandTicketNext();


	public static AppCommandTicketDesign TicketDesign { get; } = new AppCommandTicketDesign();


	public static AppCommandTicketImportExcel TicketImportExcel { get; } = new AppCommandTicketImportExcel();


	public static AppCommandTicketImportTable TicketImportTable { get; } = new AppCommandTicketImportTable();


	public static AppCommandTicketRowHeightIncrease TicketRowHeightIncrease { get; } = new AppCommandTicketRowHeightIncrease();


	public static AppCommandTicketRowHeightDecrease TicketRowHeightDecrease { get; } = new AppCommandTicketRowHeightDecrease();


	public static AppCommandTicketColumnWidthIncrease TicketColumnWidthIncrease { get; } = new AppCommandTicketColumnWidthIncrease();


	public static AppCommandTicketColumnWidthDecrease TicketColumnWidthDecrease { get; } = new AppCommandTicketColumnWidthDecrease();


	public static AppCommandTicketMode TicketMode { get; } = new AppCommandTicketMode();


	public static AppCommandTicketReport TicketReport { get; } = new AppCommandTicketReport();


	public static AppCommandTicketFormatText TicketFormatText { get; } = new AppCommandTicketFormatText();


	public static AppCommandTicketFormatNumber TicketFormatNumber { get; } = new AppCommandTicketFormatNumber();


	public static AppCommandTicketFormatComma TicketFormatComma { get; } = new AppCommandTicketFormatComma();


	public static AppCommandTicketFormatDollar TicketFormatDollar { get; } = new AppCommandTicketFormatDollar();


	public static AppCommandTicketFormatRmb TicketFormatRmb { get; } = new AppCommandTicketFormatRmb();


	public static AppCommandTicketFormatPercent TicketFormatPercent { get; } = new AppCommandTicketFormatPercent();


	public static AppCommandTicketFormatNumeric TicketFormatNumeric { get; } = new AppCommandTicketFormatNumeric();


	public static AppCommandTicketFormatDateChinese TicketFormatDateChinese { get; } = new AppCommandTicketFormatDateChinese();


	public static AppCommandTicketFormatDateDash TicketFormatDateDash { get; } = new AppCommandTicketFormatDateDash();


	public static AppCommandTicketFormatDateSlash TicketFormatDateSlash { get; } = new AppCommandTicketFormatDateSlash();


	public static AppCommandTicketFormatDateDot TicketFormatDateDot { get; } = new AppCommandTicketFormatDateDot();


	public static AppCommandTicketFormatDateYearMonthChinese TicketFormatDateYearMonthChinese { get; } = new AppCommandTicketFormatDateYearMonthChinese();


	public static AppCommandTicketFormatDateYearMonthDash TicketFormatDateYearMonthDash { get; } = new AppCommandTicketFormatDateYearMonthDash();


	public static AppCommandTicketFormatDateYearMonthSlash TicketFormatDateYearMonthSlash { get; } = new AppCommandTicketFormatDateYearMonthSlash();


	public static AppCommandTicketFormatDateYearMonthDot TicketFormatDateYearMonthDot { get; } = new AppCommandTicketFormatDateYearMonthDot();


	public static AppCommandTicketFormatDate TicketFormatDate { get; } = new AppCommandTicketFormatDate();


	public static AppCommandTicketFormatTimeLong TicketFormatTimeLong { get; } = new AppCommandTicketFormatTimeLong();


	public static AppCommandTicketFormatTimeLongChinese TicketFormatTimeLongChinese { get; } = new AppCommandTicketFormatTimeLongChinese();


	public static AppCommandTicketFormatTimeShort TicketFormatTimeShort { get; } = new AppCommandTicketFormatTimeShort();


	public static AppCommandTicketFormatTimeShortChinese TicketFormatTimeShortChinese { get; } = new AppCommandTicketFormatTimeShortChinese();


	public static AppCommandTicketFormatTime TicketFormatTime { get; } = new AppCommandTicketFormatTime();


	public static AppCommandTicketFormatBoolCheckBox TicketFormatBoolCheckBox { get; } = new AppCommandTicketFormatBoolCheckBox();


	public static AppCommandTicketFormatBoolOnOff TicketFormatBoolOnOff { get; } = new AppCommandTicketFormatBoolOnOff();


	public static AppCommandTicketFormatBoolean TicketFormatBoolean { get; } = new AppCommandTicketFormatBoolean();


	public static AppCommandTicketFormat TicketFormat { get; } = new AppCommandTicketFormat();


	public static AppCommandTicketZeroFormatEmpty TicketZeroFormatEmpty { get; } = new AppCommandTicketZeroFormatEmpty();


	public static AppCommandTicketZeroFormatZero TicketZeroFormatZero { get; } = new AppCommandTicketZeroFormatZero();


	public static AppCommandTicketZeroFormatDash TicketZeroFormatDash { get; } = new AppCommandTicketZeroFormatDash();


	public static AppCommandTicketZeroFormat TicketZeroFormat { get; } = new AppCommandTicketZeroFormat();


	public static AppCommandTicketMorePrecision TicketMorePrecision { get; } = new AppCommandTicketMorePrecision();


	public static AppCommandTicketLessPrecision TicketLessPrecision { get; } = new AppCommandTicketLessPrecision();


	public static AppCommandControlFormula ControlFormula { get; } = new AppCommandControlFormula();


	public static AppCommandTitleIncreaseRowHeight TitleIncreaseRowHeight { get; } = new AppCommandTitleIncreaseRowHeight();


	public static AppCommandTitleDecreaseRowHeight TitleDecreaseRowHeight { get; } = new AppCommandTitleDecreaseRowHeight();


	public static AppCommandTitleUnifyRowHeight TitleUnifyRowHeight { get; } = new AppCommandTitleUnifyRowHeight();


	public static AppCommandTitleIncreaseColumnWidth TitleIncreaseColumnWidth { get; } = new AppCommandTitleIncreaseColumnWidth();


	public static AppCommandTitleDecreaseColumnWidth TitleDecreaseColumnWidth { get; } = new AppCommandTitleDecreaseColumnWidth();


	public static AppCommandTitleUnifyColumnWidth TitleUnifyColumnWidth { get; } = new AppCommandTitleUnifyColumnWidth();


	public static AppCommandCrossProjectDataRef CrossProjectDataRef { get; } = new AppCommandCrossProjectDataRef();


	public static AppCommandRefreshCrossProjectRefs RefreshCrossProjectRefs { get; } = new AppCommandRefreshCrossProjectRefs();


	public static AppCommandCustomFillConfig CustomFillConfig { get; } = new AppCommandCustomFillConfig();


	public static AppCommandExecuteCustomFill ExecuteCustomFill { get; } = new AppCommandExecuteCustomFill();
}
