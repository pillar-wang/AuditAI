using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Auditai.DTO;

namespace Auditai.Model;

public class FormulaEvaluator
{
	private string _text;

	private CommonTokenStream _cts;

	private FormulaParser _parser;

	private FormulaParser.FormulaContext _tree;

	private static Dictionary<Column, List<Cell>> _columnCellsCache = new Dictionary<Column, List<Cell>>();

	public FormulaEvaluationEnvironment Env { get; set; }

	public FormulaEvaluator(string text)
	{
		_text = text;
		ICharStream input = CharStreams.fromstring(_text);
		FormulaLexer formulaLexer = new FormulaLexer(input);
		formulaLexer.AddErrorListener(LexerErrorListener.Instance);
		_cts = new CommonTokenStream(formulaLexer);
		_parser = new FormulaParser(_cts);
		_parser.ErrorHandler = new BailErrorStrategy();
		try
		{
			_tree = _parser.formula();
		}
		catch (ParseCanceledException)
		{
			throw new FormulaSyntaxException("", 0);
		}
	}

	public Operand EvaluateToOperand()
	{
		if (Env == null)
		{
			throw new InvalidOperationException("调用 EvaluateToOperand() 前必须先设置 FormulaEvaluator.Env 属性。请使用 EvaluateDocumentFormula() 或其他带有 Env 上下文的求值方法。");
		}
		FormulaEvaluationVisitor formulaEvaluationVisitor = new FormulaEvaluationVisitor(Env);
		Operand operand = formulaEvaluationVisitor.Visit(_tree);
		if (operand is ErrorOperand errorOperand)
		{
			errorOperand.ThrowException();
		}
		return operand;
	}

	public object Evaluate()
	{
		Operand operand = EvaluateToOperand();
		if (operand is TreeListOperand || operand is TableListOperand || operand is MultiListOperand)
		{
			throw new FormulaNotApplicableException("列表类函数无法直接求值");
		}
		return operand.Evaluate();
	}

	public IsFillFormula IsFill()
	{
		IsFillFormula isFillFormula = new IsFillFormula();
		ParseTreeWalker.Default.Walk(isFillFormula, _tree);
		return isFillFormula;
	}

	public IsLedgerCollectFillFormula IsLedgerCollectFillFormula()
	{
		IsLedgerCollectFillFormula isLedgerCollectFillFormula = new IsLedgerCollectFillFormula();
		ParseTreeWalker.Default.Walk(isLedgerCollectFillFormula, _tree);
		return isLedgerCollectFillFormula;
	}

	public bool HasCancelFunction()
	{
		HasCancelFunctionListener hasCancelFunctionListener = new HasCancelFunctionListener();
		ParseTreeWalker.Default.Walk(hasCancelFunctionListener, _tree);
		return hasCancelFunctionListener.Result;
	}

	public bool HasCurrentTableColumn(Table table)
	{
		HasCurrentTableColumnListener hasCurrentTableColumnListener = new HasCurrentTableColumnListener(table);
		ParseTreeWalker.Default.Walk(hasCurrentTableColumnListener, _tree);
		return hasCurrentTableColumnListener.Result;
	}

	public bool CannotExportExcel(CannotExportExcelContext context)
	{
		CannotExportExcelListener cannotExportExcelListener = new CannotExportExcelListener(context);
		ParseTreeWalker.Default.Walk(cannotExportExcelListener, _tree);
		return cannotExportExcelListener.ForbidExport;
	}

	public ValueSetOperand EvaluateLqDistinct()
	{
		FormulaEvaluationVisitor formulaEvaluationVisitor = new FormulaEvaluationVisitor(Env);
		Operand operand = formulaEvaluationVisitor.Visit(_tree);
		return operand as ValueSetOperand;
	}

	public HashSet<Id64> GetReferredTableIds()
	{
		GetReferredTablesListener getReferredTablesListener = new GetReferredTablesListener();
		ParseTreeWalker.Default.Walk(getReferredTablesListener, _tree);
		return getReferredTablesListener.ReferredTables;
	}

	public FormulaReferences GetReferences(FormulaReferenceResolver resolver)
	{
		GetReferencesListener getReferencesListener = new GetReferencesListener(resolver);
		ParseTreeWalker.Default.Walk(getReferencesListener, _tree);
		return getReferencesListener.References;
	}

	public FormulaReferences ValidationGetReferences(FormulaEvaluationEnvironment env)
	{
		ValidationGetReferencesListener validationGetReferencesListener = new ValidationGetReferencesListener(env);
		ParseTreeWalker.Default.Walk(validationGetReferencesListener, _tree);
		return validationGetReferencesListener.References;
	}

	public string GetDisplayString(FormulaReferenceResolver resolver, Table contextTable = null)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		DisplayRewriter listener = new DisplayRewriter(tokenStreamRewriter, resolver, contextTable);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string GetDisplayStringTicket(FormulaReferenceResolver resolver, int dataRowStart, int dataRowCount, Table contextTable = null, TicketTable ticket = null)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		TicketDisplayRewriter listener = new TicketDisplayRewriter(tokenStreamRewriter, resolver, contextTable, dataRowStart, dataRowCount, ticket);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string GetDisplayStringLedgerCollectFormula(FormulaReferenceResolver resolver, Table contextTable = null, ILegderVirtualTableSetting legderVirtualTableSetting = null)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		LedgerCollectFormulaDisplayRewriter listener = new LedgerCollectFormulaDisplayRewriter(tokenStreamRewriter, resolver, contextTable, legderVirtualTableSetting);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public Tuple<List<TooltipListener.FormulaTooltipSegment>, string> GetFormulaTooltipSegments(FormulaReferenceResolver resolver, Table contextTable, ValidationResult vr = null)
	{
		TooltipListener tooltipListener = new TooltipListener(_text, resolver, contextTable, vr);
		ParseTreeWalker.Default.Walk(tooltipListener, _tree);
		return Tuple.Create(tooltipListener.Result, _text.Substring(tooltipListener._nextPreTextStart));
	}

	public string DuplicateTableRewrite(FormulaReferenceResolver resolver, Dictionary<Id64, Table> dic, bool transProject)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		DuplicateTableRewriter listener = new DuplicateTableRewriter(tokenStreamRewriter, resolver, dic, transProject);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string ExcelExport(ExcelExporterContext context)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		ExcelRewriter listener = new ExcelRewriter(tokenStreamRewriter, context);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string PasteFormula(FormulaReferenceResolver resolver, Cell source, Cell dest)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		PasteFormulaRewriter listener = new PasteFormulaRewriter(tokenStreamRewriter, resolver, source, dest);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public List<Tuple<Id64, Id64, FormulaDependencyObjectKind>> GetRefIds()
	{
		GetRefIdsListener getRefIdsListener = new GetRefIdsListener();
		ParseTreeWalker.Default.Walk(getRefIdsListener, _tree);
		return getRefIdsListener.Result;
	}

	public List<FormulaRefInfo> GetRefEntries()
	{
		GetRefEntriesListener getRefEntriesListener = new GetRefEntriesListener();
		ParseTreeWalker.Default.Walk(getRefEntriesListener, _tree);
		return getRefEntriesListener.Result;
	}

	public CellsOperand CondExprToFilter()
	{
		if (!(_tree.expr() is FormulaParser.FuncContext funcContext))
		{
			return null;
		}
		string text = funcContext.FuncName().GetText();
		if (text.Equals("LqSumIf", StringComparison.OrdinalIgnoreCase) || text.Equals("SumIf", StringComparison.OrdinalIgnoreCase))
		{
			FormulaParser.ExprContext tree = funcContext.expr(0);
			FormulaEvaluationVisitor formulaEvaluationVisitor = new FormulaEvaluationVisitor(Env);
			return formulaEvaluationVisitor.Visit(tree) as CellsOperand;
		}
		return null;
	}

	public bool CanPatternMatchBatch()
	{
		CanGenerateBatchFunctionNameListener canGenerateBatchFunctionNameListener = new CanGenerateBatchFunctionNameListener();
		ParseTreeWalker.Default.Walk(canGenerateBatchFunctionNameListener, _tree);
		return canGenerateBatchFunctionNameListener.FunctionName != null;
	}

	public string PatternMatchBatch(FormulaReferenceResolver resolver, Table currentTable, IEnumerable<Table> newTables)
	{
		CanGenerateBatchFunctionNameListener canGenerateBatchFunctionNameListener = new CanGenerateBatchFunctionNameListener();
		ParseTreeWalker.Default.Walk(canGenerateBatchFunctionNameListener, _tree);
		if (canGenerateBatchFunctionNameListener.FunctionName == null)
		{
			return null;
		}
		HashSet<string> hashSet = new HashSet<string>(newTables.Select((Table t) => t.Id.ToString()));
		GenerateBatchFormulaListener generateBatchFormulaListener = new GenerateBatchFormulaListener(canGenerateBatchFunctionNameListener.FunctionName);
		ParseTreeWalker.Default.Walk(generateBatchFormulaListener, _tree);
		hashSet.ExceptWith(generateBatchFormulaListener.ExistingTables);
		StringBuilder stringBuilder = new StringBuilder(_text);
		FormulaParser.ExprContext firstExpr = generateBatchFormulaListener.FirstExpr;
		ListTokenSource tokenSource = new ListTokenSource(_cts.GetTokens(firstExpr.SourceInterval.a, firstExpr.SourceInterval.b));
		CommonTokenStream commonTokenStream = new CommonTokenStream(tokenSource);
		commonTokenStream.Fill();
		Interval interval = Interval.Invalid;
		foreach (string newTableId in hashSet)
		{
			TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(commonTokenStream);
			GenerateBatchFormulaRewriter listener = new GenerateBatchFormulaRewriter(tokenStreamRewriter, resolver, currentTable, newTables.First((Table t) => t.Id.ToString() == newTableId));
			ParseTreeWalker.Default.Walk(listener, firstExpr);
			if (string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "LqDistinct", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "LqAsc", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "LqDesc", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "Distinct", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "DistinctUp", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "DistinctDown", StringComparison.OrdinalIgnoreCase))
			{
				stringBuilder.Insert(generateBatchFormulaListener.InsertPosition, "," + tokenStreamRewriter.GetText());
			}
			else if (string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "LqFilter", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "LqVLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "LqSumIf", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "DistinctF", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "VLookUp", StringComparison.OrdinalIgnoreCase) || string.Equals(canGenerateBatchFunctionNameListener.FunctionName, "SumIf", StringComparison.OrdinalIgnoreCase))
			{
				FormulaParser.ExprContext secondExpr = generateBatchFormulaListener.SecondExpr;
				if (interval.Equals(Interval.Invalid))
				{
					interval = secondExpr.SourceInterval;
				}
				ListTokenSource tokenSource2 = new ListTokenSource(_cts.GetTokens(interval.a, interval.b));
				CommonTokenStream commonTokenStream2 = new CommonTokenStream(tokenSource2);
				commonTokenStream2.Fill();
				TokenStreamRewriter tokenStreamRewriter2 = new TokenStreamRewriter(commonTokenStream2);
				GenerateBatchFormulaRewriter listener2 = new GenerateBatchFormulaRewriter(tokenStreamRewriter2, resolver, currentTable, newTables.First((Table t) => t.Id.ToString() == newTableId));
				ParseTreeWalker.Default.Walk(listener2, secondExpr);
				stringBuilder.Insert(generateBatchFormulaListener.InsertPosition, "," + tokenStreamRewriter.GetText() + "," + tokenStreamRewriter2.GetText());
			}
		}
		return stringBuilder.ToString();
	}

	public string PasteColumnFormula(FormulaReferenceResolver resolver, Column source, Column dest)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		PasteColumnFormulaListener listener = new PasteColumnFormulaListener(tokenStreamRewriter, resolver, source, dest);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string PasteHeaderCellFormula(FormulaReferenceResolver resolver, Cell source, Cell dest)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		PasteHeaderCellFormulaListener listener = new PasteHeaderCellFormulaListener(tokenStreamRewriter, resolver, source, dest);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public bool HasLqCrossTable()
	{
		HasLqCrossTableListener hasLqCrossTableListener = new HasLqCrossTableListener();
		ParseTreeWalker.Default.Walk(hasLqCrossTableListener, _tree);
		return hasLqCrossTableListener.FunctionName != null;
	}

	public bool HasLqSumIfVLookUp()
	{
		HasSumIfVLookUpListener hasSumIfVLookUpListener = new HasSumIfVLookUpListener();
		ParseTreeWalker.Default.Walk(hasSumIfVLookUpListener, _tree);
		return hasSumIfVLookUpListener.FunctionName != null;
	}

	public bool HasInputList()
	{
		HasInputListListener hasInputListListener = new HasInputListListener();
		ParseTreeWalker.Default.Walk(hasInputListListener, _tree);
		return hasInputListListener.FunctionName != null;
	}

	public List<Column> GetReferredTableColumns(Table table)
	{
		GetRefTableColumnListener getRefTableColumnListener = new GetRefTableColumnListener(table);
		ParseTreeWalker.Default.Walk(getRefTableColumnListener, _tree);
		return getRefTableColumnListener.ReferredColumns;
	}

	public void GetReferredTableColumnsAndCells(Table table, out HashSet<Column> referredColumns, out HashSet<Cell> referredCells)
	{
		GetRefTableColumnAndCellListener getRefTableColumnAndCellListener = new GetRefTableColumnAndCellListener(table);
		ParseTreeWalker.Default.Walk(getRefTableColumnAndCellListener, _tree);
		referredCells = getRefTableColumnAndCellListener.ReferredCells;
		referredColumns = getRefTableColumnAndCellListener.ReferredColumns;
	}

	public string PasteFilter(string toInsert)
	{
		FormulaParser.ExprContext exprContext = _tree.expr();
		FormulaParser.FuncContext func = exprContext as FormulaParser.FuncContext;
		if (func != null && (func.FuncName().GetText().Equals("LqFilter", StringComparison.OrdinalIgnoreCase) || func.FuncName().GetText().Equals("DistinctF", StringComparison.OrdinalIgnoreCase)))
		{
			if (AnyRepeat())
			{
				return _text;
			}
			TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
			tokenStreamRewriter.InsertBefore(func.RPAREN().Symbol, "," + toInsert);
			return tokenStreamRewriter.GetText();
		}
		return "DistinctF(" + toInsert + ")";
		bool AnyRepeat()
		{
			for (int i = 0; i < func.expr().Length; i++)
			{
				string text = func.expr(i).GetText();
				if (text == toInsert)
				{
					return true;
				}
			}
			return false;
		}
	}

	public string PasteDistinct(Column srcCol)
	{
		string subexpr = $"[2:{srcCol.Table.Id}:{srcCol.Id}]";
		FormulaParser.ExprContext exprContext = _tree.expr();
		FormulaParser.FuncContext func = exprContext as FormulaParser.FuncContext;
		if (func != null && (func.FuncName().GetText().Equals("LqDistinct", StringComparison.OrdinalIgnoreCase) || func.FuncName().GetText().Equals("Distinct", StringComparison.OrdinalIgnoreCase)))
		{
			if (AnyRepeat())
			{
				return _text;
			}
			TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
			tokenStreamRewriter.InsertBefore(func.RPAREN().Symbol, "," + subexpr);
			return tokenStreamRewriter.GetText();
		}
		return "Distinct(" + subexpr + ")";
		bool AnyRepeat()
		{
			for (int i = 0; i < func.expr().Length; i++)
			{
				string text = func.expr(i).GetText();
				if (text == subexpr)
				{
					return true;
				}
			}
			return false;
		}
	}

	public string PasteVLookUp(string functionName, Column srcCol, Column dstCol, Column col1, Column col2)
	{
		string subexpr = $"[2:{srcCol.Table.Id}:{col1.Id}]=[4:{dstCol.Table.Id}:{col2.Id}],[2:{srcCol.Table.Id}:{srcCol.Id}]";
		FormulaParser.ExprContext exprContext = _tree.expr();
		FormulaParser.FuncContext func = exprContext as FormulaParser.FuncContext;
		if (func != null && func.FuncName().GetText().Equals(functionName, StringComparison.OrdinalIgnoreCase))
		{
			if (AnyRepeat())
			{
				return _text;
			}
			TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
			tokenStreamRewriter.InsertBefore(func.RPAREN().Symbol, "," + subexpr);
			return tokenStreamRewriter.GetText();
		}
		return functionName + "(" + subexpr + ")";
		bool AnyRepeat()
		{
			for (int i = 0; i < func.expr().Length; i += 2)
			{
				string text = func.expr(i).GetText() + "," + func.expr(i + 1).GetText();
				if (text == subexpr)
				{
					return true;
				}
			}
			return false;
		}
	}

	public Column GetFillSourceCol(Table table)
	{
		if (_tree.expr() is FormulaParser.FuncContext funcContext)
		{
			string text = funcContext.FuncName().GetText();
			if (text.Equals("LqDistinct", StringComparison.OrdinalIgnoreCase) || text.Equals("LqAsc", StringComparison.OrdinalIgnoreCase) || text.Equals("LqDesc", StringComparison.OrdinalIgnoreCase) || text.Equals("LqFilter", StringComparison.OrdinalIgnoreCase) || text.Equals("LqCollect", StringComparison.OrdinalIgnoreCase) || text.Equals("Distinct", StringComparison.OrdinalIgnoreCase) || text.Equals("DistinctUp", StringComparison.OrdinalIgnoreCase) || text.Equals("DistinctDown", StringComparison.OrdinalIgnoreCase) || text.Equals("DistinctF", StringComparison.OrdinalIgnoreCase) || text.Equals("Collect", StringComparison.OrdinalIgnoreCase) || text.Equals("CollectF", StringComparison.OrdinalIgnoreCase))
			{
				FormulaReferences references = GetReferences(Env.Resolver);
				return references.ColumnReferences.FirstOrDefault((Column c) => c.Table == table);
			}
			return null;
		}
		return null;
	}

	public Operand EvaluateToOperandTicket(TicketEvalContext tec)
	{
		FormulaEvaluationVisitorTicket formulaEvaluationVisitorTicket = new FormulaEvaluationVisitorTicket(Env, tec);
		Operand operand = formulaEvaluationVisitorTicket.Visit(_tree);
		if (operand is ErrorOperand errorOperand)
		{
			errorOperand.ThrowException();
		}
		return operand;
	}

	public Operand EvaluateOnVirtualTable(VirtualTableEvalContext tec, Id64 replaceTableId)
	{
		FormulaEvaluationVisitorVirtualTable formulaEvaluationVisitorVirtualTable = new FormulaEvaluationVisitorVirtualTable(Env, tec, replaceTableId);
		Operand operand = formulaEvaluationVisitorVirtualTable.Visit(_tree);
		if (operand is ErrorOperand errorOperand)
		{
			errorOperand.ThrowException();
		}
		return operand;
	}

	public Operand EvaluateOnLedgerVirtualTable(LedgerVirtualTableEvalContext tec, Id64 balanceVirtualTableId, Id64 voucherVirtualTableId)
	{
		FormulaEvaluationVisitorLedgerVirtualTable formulaEvaluationVisitorLedgerVirtualTable = new FormulaEvaluationVisitorLedgerVirtualTable(Env, tec, balanceVirtualTableId, voucherVirtualTableId);
		Operand operand = formulaEvaluationVisitorLedgerVirtualTable.Visit(_tree);
		if (operand is ErrorOperand errorOperand)
		{
			errorOperand.ThrowException();
		}
		return operand;
	}

	public string RewriteDynamicRowTicket(int dataRowStart, int dataRowCount)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		DynamicRowTicketRewriter listener = new DynamicRowTicketRewriter(tokenStreamRewriter, dataRowStart, dataRowCount);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string RewriteMixTicket(Func<int, bool> checkIsDynamicDataRowCallback, Func<int, int, long> getTicketColumnIdCallback, Func<int, int> getFixedRowIndexCallback)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		MixTicketRewriter listener = new MixTicketRewriter(tokenStreamRewriter, checkIsDynamicDataRowCallback, getTicketColumnIdCallback, getFixedRowIndexCallback);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string RewriteMixTicketFormulaToDesignFormula(Func<int, int> convertFixedRowIndexToVmRowIndexCallback)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		MixTicketRewriterToDesignFormula listener = new MixTicketRewriterToDesignFormula(tokenStreamRewriter, convertFixedRowIndexToVmRowIndexCallback);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string OffsetTicket(int startRow, int rowOffset, int startCol, int colOffset)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		OffsetTicketRewriter listener = new OffsetTicketRewriter(tokenStreamRewriter, startRow, rowOffset, startCol, colOffset);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string PasteTicketFormula(int rowOffset, int colOffset, int rowCount, int colCount)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		PasteTicketFormulaRewriter listener = new PasteTicketFormulaRewriter(tokenStreamRewriter, rowOffset, colOffset, rowCount, colCount);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public TicketReferences GetTicketReferences()
	{
		GetTicketReferencesListener getTicketReferencesListener = new GetTicketReferencesListener();
		ParseTreeWalker.Default.Walk(getTicketReferencesListener, _tree);
		return getTicketReferencesListener.Result;
	}

	public string TicketExportExcel(int titleRowsCount, int dataRowStart, int dataRowCount, Func<int, int> convertDesignRowIndexToVMRowIndexHandle)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		TicketExportExcelRewriter listener = new TicketExportExcelRewriter(tokenStreamRewriter, titleRowsCount, dataRowStart, dataRowCount, convertDesignRowIndexToVMRowIndexHandle);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string TicketExportExcel_MixTicket(int titleRowsCount, Func<int, Tuple<int, int, int>> getTicketColumnSettingHandle, Func<int, int> convertFixedRowIndexToVMRowIndexHandle)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		TicketExportExcelRewriterForMixTicket listener = new TicketExportExcelRewriterForMixTicket(tokenStreamRewriter, titleRowsCount, getTicketColumnSettingHandle, convertFixedRowIndexToVMRowIndexHandle);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	internal static List<Cell> GetCells(Column column)
	{
		if (!_columnCellsCache.ContainsKey(column))
		{
			_columnCellsCache.Add(column, column.GetCells(excludeFixedRows: true).ToList());
		}
		return _columnCellsCache[column];
	}

	public static void ClearCache()
	{
		_columnCellsCache.Clear();
		ColumnOperand._columnCellsEqualCache.Clear();
		ColumnOperand._columnCellsSearchIndexCache.Clear();
		ColumnOperand._columnCellsStringEqualCache.Clear();
		ColumnOperand._columnCellsNotEqualCache.Clear();
		ColumnOperand._columnCellsGreatThanCache.Clear();
		ColumnOperand._columnCellsGreatThanOrEqualCache.Clear();
		ColumnOperand._columnCellsLessThanCache.Clear();
		ColumnOperand._columnCellsLessThanOrEqualCache.Clear();
		CellsOperand._leftOperatorCellListHashSetDictionary.Clear();
		CellsOperand._rightOperatorCellListHashSetDictionary.Clear();
		FunctionEvaluator._cellListRowsCountCache.Clear();
	}

	public static void ClearCache(Table table)
	{
		ClearDictionary(table, _columnCellsCache);
		ClearDictionary(table, ColumnOperand._columnCellsEqualCache);
		ClearDictionary(table, ColumnOperand._columnCellsSearchIndexCache);
		ClearDictionary(table, ColumnOperand._columnCellsStringEqualCache);
		ClearDictionary(table, ColumnOperand._columnCellsNotEqualCache);
		ClearDictionary(table, ColumnOperand._columnCellsGreatThanCache);
		ClearDictionary(table, ColumnOperand._columnCellsGreatThanOrEqualCache);
		ClearDictionary(table, ColumnOperand._columnCellsLessThanCache);
		ClearDictionary(table, ColumnOperand._columnCellsLessThanOrEqualCache);
	}

	private static void ClearDictionary(Table needClearTable, Dictionary<Column, List<Cell>> dic)
	{
		if (dic.Count == 0)
		{
			return;
		}
		List<Column> list = dic.Keys.Where((Column c) => c.Table == needClearTable).ToList();
		foreach (Column item in list)
		{
			dic.Remove(item);
		}
	}

	private static void ClearDictionary(Table needClearTable, Dictionary<Column, Dictionary<object, List<Cell>>> dic)
	{
		if (dic.Count == 0)
		{
			return;
		}
		List<Column> list = dic.Keys.Where((Column c) => c.Table == needClearTable).ToList();
		foreach (Column item in list)
		{
			dic.Remove(item);
		}
	}

	private static void ClearDictionary(Table needClearTable, Dictionary<Column, CellValueSearchIndex> dic)
	{
		if (dic.Count == 0)
		{
			return;
		}
		List<Column> list = dic.Keys.Where((Column c) => c.Table == needClearTable).ToList();
		foreach (Column item in list)
		{
			dic.Remove(item);
		}
	}

	private static void ClearDictionary(Table needClearTable, Dictionary<Column, OperandValueDictionary> dic)
	{
		if (dic.Count == 0)
		{
			return;
		}
		List<Column> list = dic.Keys.Where((Column c) => c.Table == needClearTable).ToList();
		foreach (Column item in list)
		{
			dic.Remove(item);
		}
	}
}
